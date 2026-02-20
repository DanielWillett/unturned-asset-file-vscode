using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

public interface IAssetSpecDatabase
{
    /// <summary>
    /// If the internet should be used to download the latest data.
    /// </summary>
    /// <remarks>Defaults to <see langword="false"/>.</remarks>
    bool UseInternet { get; set; }

    /// <summary>
    /// If the database finished initializing.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// JSON options used to read files.
    /// </summary>
    JsonSerializerOptions? Options { get; set; }

    /// <summary>
    /// List of achievements available for NPCs to grant.
    /// </summary>
    ICollection<string>? NPCAchievementIds { get; }
    
    /// <summary>
    /// The read-context used when reading information for this database.
    /// </summary>
    IDatSpecificationReadContext ReadContext { get; }

    /// <summary>
    /// Live or installed version of the game.
    /// </summary>
    Version? CurrentGameVersion { get; }

    /// <summary>
    /// The Status.json file hosted by Unturned.
    /// </summary>
    JsonDocument? StatusInformation { get; }

    /// <summary>
    /// Where Unturned is currently installed, if it is.
    /// </summary>
    InstallDirUtility UnturnedInstallDirectory { get; }

    /// <summary>
    /// Information about the asset type hierarchy and other relevant information.
    /// </summary>
    AssetInformation Information { get; }

    /// <summary>
    /// List of valid translation keys for blueprint action buttons.
    /// </summary>
    IDictionary<string, ActionButton>? ValidActionButtons { get; }

    /// <summary>
    /// List of valid EBlueprintSkill values pulled from the spec on initialize mapped to their corresponding skill.
    /// </summary>
    IDictionary<string, SkillReference>? BlueprintSkills { get; }

    /// <summary>
    /// List of file type definitions by their type name.
    /// </summary>
    IDictionary<QualifiedType, DatFileType> FileTypes { get; }


    /// <summary>
    /// List of localization file type definitions by their relative path to the localization root folder (using the system's current path separator).
    /// </summary>
    IDictionary<string, DatFileType> LocalizationFileTypes { get; }
    
    /// <summary>
    /// List of custom and file type definitions by their type name.
    /// </summary>
    IDictionary<QualifiedType, DatType> AllTypes { get; }
    
    /// <summary>
    /// Initialize the database.
    /// </summary>
    Task InitializeAsync(CancellationToken token = default);

    /// <summary>
    /// Invokes a task when initialization finishes.
    /// </summary>
    Task OnInitialize(Func<IAssetSpecDatabase, ILoggerFactory, Task> action);
}

public class AssetSpecDatabase : IDisposable, IAssetSpecDatabase
{
    private readonly SpecificationFileReader _fileReader;

    public const string UnturnedName = "Unturned";
    public const string UnturnedAppId = "304930";

    private readonly ISpecDatabaseCache? _cache;
    private readonly ILoggerFactory _loggerFactory;
    private JsonDocument? _statusJson;
    private List<OnInitializeState>? _initializeListeners = new List<OnInitializeState>();
    private readonly object _initLock = new object();

    public ICollection<string>? NPCAchievementIds { get; private set; }

    /// <inheritdoc />
    public IDatSpecificationReadContext ReadContext => _fileReader;

    public Version? CurrentGameVersion { get; private set; }

    /// <summary>
    /// The status document from the game installation or online if necessary.
    /// </summary>
    /// <returns>A cached json document. Do not dispose this document after you're done.</returns>
    public JsonDocument? StatusInformation => _statusJson;

    public InstallDirUtility UnturnedInstallDirectory { get; }

    /// <summary>
    /// Allow downloading the latest version of files from the internet instead of using a possibly outdated embedded version.
    /// </summary>
    public bool UseInternet { get; set; }
    public bool IsInitialized { get; private set; }

    public IDictionary<string, ActionButton>? ValidActionButtons { get; private set; }
    public IDictionary<string, SkillReference>? BlueprintSkills { get; private set; }
    public IDictionary<string, DatFileType> LocalizationFileTypes { get; private set; }
    public IDictionary<QualifiedType, DatFileType> FileTypes { get; private set; }
    public IDictionary<QualifiedType, DatType> AllTypes { get; private set; }

    public AssetInformation Information { get; private set; } = new AssetInformation
    {
        AssetAliases = new Dictionary<string, QualifiedType>(0),
        AssetCategories = new Dictionary<QualifiedType, string>(0),
        Types = new Dictionary<QualifiedType, TypeHierarchy>(0)
    };

    public JsonSerializerOptions? Options { get; set; }

    private static JsonSerializerOptions GetJsonOptions(JsonSerializerOptions? defaultOptions = null)
    {
        bool skipCheck = defaultOptions == null;

        defaultOptions ??= new JsonSerializerOptions
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            MaxDepth = 14,
            AllowTrailingCommas = true
        };

        TryAddConverter<Color, ColorConverter>(defaultOptions, skipCheck);
        TryAddConverter<Color32, Color32Converter>(defaultOptions, skipCheck);
        TryAddConverter<BundleReference, BundleReferenceConverter>(defaultOptions, skipCheck);
        TryAddConverter<GuidOrId, GuidOrIdConverter>(defaultOptions, skipCheck);
        TryAddConverter<QualifiedType, QualifiedTypeConverter>(defaultOptions, skipCheck);
        TryAddConverter<UnityEngineVersion, UnityEngineVersionConverter>(defaultOptions, skipCheck);
        TryAddConverter<TypeHierarchy, TypeHierarchyConverter>(defaultOptions, skipCheck);

        return defaultOptions;

        static void TryAddConverter<TValue, TConverter>(JsonSerializerOptions options, bool skipCheck) where TConverter : JsonConverter, new()
        {
            if (skipCheck || !options.Converters.Any(x => x.CanConvert(typeof(TValue))))
            {
                options.Converters.Add(new TConverter());
            }
        }
    }

    public static AssetSpecDatabase FromOffline(bool useInstallDir = false, ILoggerFactory? loggerFactory = null, JsonSerializerOptions? defaultJsonOptions = null, ISpecDatabaseCache? cache = null)
    {
        loggerFactory ??= NullLoggerFactory.Instance;
        return new AssetSpecDatabase(
            new SpecificationFileReader(
                allowInternet: false,
                loggerFactory,
                new Lazy<HttpClient>(() => new HttpClient()),
                GetJsonOptions(defaultJsonOptions),
                new InstallDirUtility(
                    useInstallDir ? "Unturned" : "\0",
                    useInstallDir ? "304930" : "\0"
                ),
                cache: cache
            ),
            loggerFactory
        )
        {
            UseInternet = false
        };
    }
    

    public static AssetSpecDatabase FromOnline(bool useInstallDir = true, ILoggerFactory? loggerFactory = null, JsonSerializerOptions? defaultJsonOptions = null, ISpecDatabaseCache? cache = null)
    {
        loggerFactory ??= NullLoggerFactory.Instance;
        return new AssetSpecDatabase(
            new SpecificationFileReader(
                allowInternet: true,
                loggerFactory,
                new Lazy<HttpClient>(() => new HttpClient()),
                GetJsonOptions(defaultJsonOptions),
                new InstallDirUtility(
                    useInstallDir ? "Unturned" : "\0",
                    useInstallDir ? "304930" : "\0"
                ),
                cache: cache
            ),
            loggerFactory
        )
        {
            UseInternet = true
        };
    }
    
    public AssetSpecDatabase(SpecificationFileReader fileReader, ILoggerFactory? loggerFactory = null)
    {
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        _fileReader = fileReader;
        Options = fileReader.JsonOptions;
        _cache = fileReader.Cache;
        UnturnedInstallDirectory = fileReader.InstallDirUtility ?? new InstallDirUtility(UnturnedName, UnturnedAppId);

        AllTypes = ImmutableDictionary<QualifiedType, DatType>.Empty;
        FileTypes = ImmutableDictionary<QualifiedType, DatFileType>.Empty;
        LocalizationFileTypes = ImmutableDictionary<string, DatFileType>.Empty;
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interlocked.Exchange(ref _statusJson, null)?.Dispose();
        }
    }

    public Task OnInitialize(Func<IAssetSpecDatabase, ILoggerFactory, Task> action)
    {
        OnInitializeState state;
        state.Callback = action;

        lock (_initLock)
        {
            if (_initializeListeners == null)
            {
                return state.Callback(this, _loggerFactory);
            }

            state.Task = new TaskCompletionSource<int>();
            _initializeListeners.Add(state);
            return state.Task.Task;
        }
    }

    private struct OnInitializeState
    {
        public Func<IAssetSpecDatabase, ILoggerFactory, Task> Callback;
        public TaskCompletionSource<int> Task;
    }

    public async Task InitializeAsync(CancellationToken token = default)
    {
        SpecificationReadResult result = await _fileReader.ReadSpecifications(this, token);
        _statusJson = result.StatusFile;

        FileTypes = result.FileTypes;
        ValidActionButtons = result.ActionButtons;
        AllTypes = result.AllTypes;
        Information = result.Information;
        LocalizationFileTypes = result.LocalizationFileTypes;

        PopulateBlueprintSkills();
        PopulateStatusInformation();

        OnInitializeState[]? initializeListeners;
        lock (_initLock)
        {
            initializeListeners = _initializeListeners?.ToArray();
            _initializeListeners = null;
        }

        IsInitialized = true;

        if (initializeListeners != null)
        {
            Task[] initTasks = new Task[initializeListeners.Length];
            for (int i = 0; i < initializeListeners.Length; ++i)
            {
                initTasks[i] = initializeListeners[i].Callback(this, _loggerFactory);
            }

            await Task.WhenAll(initTasks).ConfigureAwait(false);
        }

        if (_cache != null)
        {
            await _cache.CacheNewFilesAsync(this, token);
        }
    }

    private void PopulateBlueprintSkills()
    {
        if (!AllTypes.TryGetValue(new QualifiedType(SkillType.BlueprintSkillEnumType, true), out DatType? blueprintSkillType) || blueprintSkillType is not DatEnumType bpSkillEnumType)
        {
            BlueprintSkills = ImmutableDictionary<string, SkillReference>.Empty;
            return;
        }

        ImmutableDictionary<string, SkillReference>.Builder bldr = ImmutableDictionary.CreateBuilder<string, SkillReference>(StringComparer.OrdinalIgnoreCase);
        foreach (DatEnumValue value in bpSkillEnumType.Values)
        {
            JsonElement valueDataRoot = value.DataRoot;
            if (valueDataRoot.ValueKind != JsonValueKind.Object || !valueDataRoot.TryGetProperty("Skill"u8, out JsonElement skillValue) || skillValue.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            string? skill = skillValue.GetString();
            if (!SkillReference.TryParse(skill, Information, out SkillReference correspondingSkill))
                continue;

            bldr[value.Value] = correspondingSkill;
        }

        BlueprintSkills = bldr.ToImmutable();
    }

    private void PopulateStatusInformation()
    {
        if (_statusJson == null || _statusJson.RootElement.ValueKind != JsonValueKind.Object)
        {
            NPCAchievementIds = Array.Empty<string>();
            return;
        }

        if (_statusJson.RootElement.TryGetProperty("Game"u8, out JsonElement element)
            && element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty("Major_Version"u8, out JsonElement majorElement)
            && majorElement.ValueKind == JsonValueKind.Number && majorElement.TryGetInt32(out int versionMajor)
            && element.TryGetProperty("Minor_Version"u8, out JsonElement minorElement)
            && minorElement.ValueKind == JsonValueKind.Number && minorElement.TryGetInt32(out int versionMinor)
            && element.TryGetProperty("Patch_Version"u8, out JsonElement patchElement)
            && patchElement.ValueKind == JsonValueKind.Number && patchElement.TryGetInt32(out int versionPatch))
        {
            CurrentGameVersion = new Version(3, versionMajor, versionMinor, versionPatch);
        }
        else
        {
            CurrentGameVersion = null;
        }

        if (_statusJson.RootElement.TryGetProperty("Achievements"u8, out element)
            && element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty("NPC_Achievement_IDs"u8, out element)
            && element.ValueKind == JsonValueKind.Array)
        {
            ImmutableHashSet<string>.Builder idsBuilder = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

            foreach (JsonElement id in element.EnumerateArray())
            {
                if (id.ValueKind == JsonValueKind.String)
                    idsBuilder.Add(id.GetString()!);
            }

            NPCAchievementIds = idsBuilder.ToImmutable();
        }
        else
        {
            NPCAchievementIds = null;
        }
    }

    protected virtual void Log(string msg)
    {
        Console.Write("AssetSpecDatabase >> ");
        Console.WriteLine(msg);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AssetSpecDatabase()
    {
        Dispose(false);
    }
}

/// <summary>
/// A button that can be used as an action label.
/// </summary>
/// <param name="Key">The base key for the button and tooltip.</param>
/// <param name="ButtonValue">The text of the button in English.</param>
/// <param name="TooltipValue">The text of the tooltip in English.</param>
public record struct ActionButton(string Key, string ButtonValue, string TooltipValue)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return Key;
    }
}
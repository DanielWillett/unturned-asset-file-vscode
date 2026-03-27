using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
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

/// <summary>
/// Used with <see cref="IAssetSpecDatabase.OnFinalizingTypes"/> to inject hard-coded types before the lists are finalized.
/// </summary>
/// <param name="readContext">Context about the current read, including necessary services.</param>
/// <param name="types">Collection to add new types to.</param>
public delegate void FinalizingTypesHandler(IDatSpecificationReadContext readContext, ICollection<DatType> types);

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
    /// The default global orderfile.
    /// </summary>
    PropertyOrderFile GlobalOrderFile { get; }

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
    /// Invoked when types are being finalized, allowing callers to add other types.
    /// </summary>
    event FinalizingTypesHandler? OnFinalizingTypes;

    /// <summary>
    /// Initialize the database.
    /// </summary>
    Task InitializeAsync(CancellationToken token = default, Action<float, string?>? progressReport = null, float maxProgress = 1f);

    /// <summary>
    /// Invokes a task when initialization finishes.
    /// </summary>
    Task OnInitialize(Func<IParsingServices, Task> action);
}

public class AssetSpecDatabase : IDisposable, IAssetSpecDatabase
{
    private readonly SpecificationFileReader _fileReader;
    private bool _startedInit;

    internal bool ReadOrderfile = true;

    public const string UnturnedName = "Unturned";
    public const string UnturnedAppId = "304930";

    private readonly ISpecDatabaseCache? _cache;
    private readonly ILoggerFactory _loggerFactory;
    private JsonDocument? _statusJson;
    private List<OnInitializeState>? _initializeListeners = new List<OnInitializeState>();
    private readonly object _initLock = new object();
    private readonly Lazy<IParsingServices> _parsingServices;

    /// <inheritdoc />
    public PropertyOrderFile GlobalOrderFile { get => field ?? throw new InvalidOperationException("Not initialized."); private set; }

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
    public bool UseInternet
    {
        get;
        set
        {
            if (_startedInit || field == value)
                return;

            field = value;
            _fileReader.AllowInternet = value;
        }
    }
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

    event FinalizingTypesHandler? IAssetSpecDatabase.OnFinalizingTypes
    {
        add => _fileReader.OnFinalizingTypes += value;
        remove => _fileReader.OnFinalizingTypes -= value;
    }

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
        return CreateSimple(false, useInstallDir, loggerFactory, defaultJsonOptions, cache);
    }
    
    public static AssetSpecDatabase FromOnline(bool useInstallDir = true, ILoggerFactory? loggerFactory = null, JsonSerializerOptions? defaultJsonOptions = null, ISpecDatabaseCache? cache = null)
    {
        return CreateSimple(true, useInstallDir, loggerFactory, defaultJsonOptions, cache);
    }

    private static AssetSpecDatabase CreateSimple(bool allowInternet, bool useInstallDir, ILoggerFactory? loggerFactory, JsonSerializerOptions? defaultJsonOptions, ISpecDatabaseCache? cache)
    {
        loggerFactory ??= NullLoggerFactory.Instance;

        InstallDirUtility installDirUtility = new InstallDirUtility(
            useInstallDir ? "Unturned" : "\0",
            useInstallDir ? "304930" : "\0"
        );
        IParsingServices? parsingServices = null;

        AssetSpecDatabase database = new AssetSpecDatabase(
            new SpecificationFileReader(
                allowInternet: allowInternet,
                loggerFactory,
                new Lazy<HttpClient>(() => new HttpClient()),
                GetJsonOptions(defaultJsonOptions),
                installDirUtility,
                cache: cache
            ),
            new Lazy<IParsingServices>(
                () => parsingServices
                      ?? throw new InvalidOperationException("Parsing servies not initialized yet.")
            )
        );

        InstallationEnvironment installation = new InstallationEnvironment(database, loggerFactory);
        if (useInstallDir && installDirUtility.TryGetInstallDirectory(out GameInstallDir installDir))
        {
            installation.AddUnturnedSearchableDirectories(
                installDir,
                new UnturnedInstallationEnvironmentExtensions.UnturnedDirectorySearchOptions
                {
                    EnableSandbox = true
                }
            );
        }

        IWorkspaceEnvironment environment = new StaticSourceFileWorkspaceEnvironment(
            cache != null,
            database,
            installationEnvironment: installation
        );

        parsingServices = new ParsingServiceProvider(database, loggerFactory, environment, installDirUtility, installation, new NilProjectFileProvider(database));
        return database;
    }

    public AssetSpecDatabase(SpecificationFileReader fileReader, Lazy<IParsingServices> parsingServices, ILoggerFactory? loggerFactory = null)
    {
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        _fileReader = fileReader;
        UseInternet = fileReader.AllowInternet;
        Options = fileReader.JsonOptions;
        _cache = fileReader.Cache;
        _parsingServices = parsingServices;
        UnturnedInstallDirectory = fileReader.InstallDirUtility ?? new InstallDirUtility(UnturnedName, UnturnedAppId);

        AllTypes = ImmutableDictionary<QualifiedType, DatType>.Empty;
        FileTypes = ImmutableDictionary<QualifiedType, DatFileType>.Empty;
        LocalizationFileTypes = ImmutableDictionary<string, DatFileType>.Empty;

        _fileReader.OnFinalizingTypes += BuiltinTypeRegistrar.OnFinalizingTypes;
    }
    
    protected virtual void Dispose(bool disposing)
    {
        Interlocked.Exchange(ref _statusJson, null)?.Dispose();

        if (!disposing)
            return;

        _fileReader.OnFinalizingTypes -= BuiltinTypeRegistrar.OnFinalizingTypes;
    }

    public Task OnInitialize(Func<IParsingServices, Task> action)
    {
        OnInitializeState state;
        state.Callback = action;

        lock (_initLock)
        {
            if (_initializeListeners == null)
            {
                return state.Callback(_parsingServices.Value);
            }

            state.Task = new TaskCompletionSource<int>();
            _initializeListeners.Add(state);
            return state.Task.Task;
        }
    }

    private struct OnInitializeState
    {
        public Func<IParsingServices, Task> Callback;
        public TaskCompletionSource<int> Task;
    }

    public async Task InitializeAsync(CancellationToken token = default, Action<float, string?>? progressReport = null, float maxProgress = 1f)
    {
        _startedInit = true;
        try
        {
            _fileReader.ProgressReport = progressReport;
            _fileReader.MaxProgress = maxProgress * 0.95f;
            SpecificationReadResult result = await _fileReader.ReadSpecifications(this, token);
            try
            {
                _statusJson = result.StatusFile;

                FileTypes = result.FileTypes;
                ValidActionButtons = result.ActionButtons;
                AllTypes = result.AllTypes;
                Information = result.Information;
                LocalizationFileTypes = result.LocalizationFileTypes;

                progressReport?.Invoke(0.96f * maxProgress, "Populating blueprint skills");
                PopulateBlueprintSkills();
                progressReport?.Invoke(0.97f * maxProgress, "Populating status information");
                PopulateStatusInformation();

                progressReport?.Invoke(0.98f * maxProgress, UseInternet ? "Downloading default orderfile" : "Loading default orderfile");

                if (ReadOrderfile)
                {
                    GlobalOrderFile = await _fileReader.ReadGlobalOrderfile(this, token);
                }
                else
                {
                    GlobalOrderFile = new PropertyOrderFile(SpecificationFileReader.GlobalOrderfileName);
                }
            }
            finally
            {
                _fileReader.DisposeProviders();
            }

            OnInitializeState[]? initializeListeners;
            lock (_initLock)
            {
                initializeListeners = _initializeListeners?.ToArray();
                _initializeListeners = null;
            }

            IsInitialized = true;

            if (initializeListeners != null)
            {
                progressReport?.Invoke(0.98f * maxProgress, "Performing second pass");
                Task[] initTasks = new Task[initializeListeners.Length];
                for (int i = 0; i < initializeListeners.Length; ++i)
                {
                    initTasks[i] = initializeListeners[i].Callback(_parsingServices.Value);
                }

                await Task.WhenAll(initTasks).ConfigureAwait(false);
            }

            if (_cache != null)
            {
                progressReport?.Invoke(0.99f * maxProgress, "Caching new files");
                await _cache.CacheNewFilesAsync(this, token);
            }

            progressReport?.Invoke(maxProgress, "File data initialized");
        }
        finally
        {
            _startedInit = false;
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
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Handles reading asset files into a <see cref="IAssetSpecDatabase"/>.
/// </summary>
public partial class SpecificationFileReader : IDatSpecificationReadContext
{
    private readonly Func<SpecificationFileReader, ISpecificationFileProvider[]> _fileProviderFactory;

    private readonly ILoggerFactory _loggerFactory;
    private readonly Lazy<HttpClient> _httpClientFactory;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly InstallDirUtility? _installDir;
    private readonly ISpecDatabaseCache? _cache;
    private readonly ILogger<SpecificationFileReader> _logger;

    private AssetInformation? _readInformation;
    private JsonDocument? _statusFile;
    private ImmutableArray<string> _actionButtons;
    private Dictionary<QualifiedType, JsonDocument>? _assetFiles;
    private QualifiedType _currentType;
    private ImmutableDictionary<QualifiedType, DatFileType>? _types;
    private ImmutableDictionary<string, DatFileType>? _localizationFiles;

    public bool AllowInternet { get; }

    public string? LatestCommitHash { get; private set; }
    
    private bool IsCacheUpToDate { get; set; }

    public SpecificationFileReader(
        bool allowInternet,
        ILoggerFactory loggerFactory,
        Lazy<HttpClient> httpClientFactory,
        JsonSerializerOptions serializerOptions,
        InstallDirUtility? installDir,
        Func<SpecificationFileReader, ISpecificationFileProvider[]>? fileProviderFactory = null,
        ISpecDatabaseCache? cache = null)
    {
        _logger = loggerFactory.CreateLogger<SpecificationFileReader>();
        _loggerFactory = loggerFactory;
        _httpClientFactory = httpClientFactory;
        _serializerOptions = serializerOptions;
        _installDir = installDir;
        _cache = cache;
        AllowInternet = allowInternet;


        _fileProviderFactory = fileProviderFactory ?? (static reader =>
        [
            new UnturnedInstallationFileProvider(reader._installDir, reader._loggerFactory.CreateLogger<UnturnedInstallationFileProvider>()),
            new CacheSpecificationFileProvider(reader._cache, reader.LatestCommitHash),
            new GitHubSpecificationFileProvider(reader._loggerFactory.CreateLogger<GitHubSpecificationFileProvider>(), reader._httpClientFactory, reader.AllowInternet, () => reader._readInformation),
            new EmbeddedResourceSpecificationFileProvider(reader._loggerFactory.CreateLogger<EmbeddedResourceSpecificationFileProvider>())
        ]);
    }

    public async Task ReadSpecifications(CancellationToken token)
    {
        using HttpClient client = new HttpClient();
        IsCacheUpToDate = false;
        if (AllowInternet && _cache != null)
        {
            LatestCommitHash = await GetLatestCommitAsync(client, token).ConfigureAwait(false);
            if (LatestCommitHash != null)
            {
                IsCacheUpToDate = _cache.IsUpToDateCache(LatestCommitHash);
            }
            else
            {
                _logger.LogWarning(Resources.Log_FailedToReadCommitHashFromRepo);
            }
        }

        Task generateLocalizationFiles = Task.Run(() => GenerateLocalizationFiles(token), token);

        ISpecificationFileProvider[] providers = _fileProviderFactory(this);
        try
        {
            Array.Sort(providers, (a, b) => b.Priority.CompareTo(a.Priority));

            bool[] enabled = new bool[providers.Length];

            await ReadKnownFileAsync(providers, enabled, KnownConfigurationFile.Assets, static async (stream, reader, token) =>
            {
                reader._readInformation = await JsonSerializer.DeserializeAsync<AssetInformation>(
                    stream,
                    reader._serializerOptions,
                    token
                ).ConfigureAwait(false);
            }, isFirst: true, token).ConfigureAwait(false);

            if (_readInformation == null)
            {
                _logger.LogWarning(Resources.Log_UnableToReadFile, "Assets.json");
                _readInformation = new AssetInformation();
            }

            _readInformation.Types ??= new Dictionary<QualifiedType, TypeHierarchy>(0);

            // custom type converter can't read property name
            foreach (KeyValuePair<QualifiedType, TypeHierarchy> type in _readInformation.Types)
            {
                type.Value.Type = new QualifiedType(type.Key);
            }

            _readInformation.AssetAliases ??= new Dictionary<string, QualifiedType>(0);
            _readInformation.AssetCategories ??= new Dictionary<QualifiedType, string>(0);
            _readInformation.KnownFileNames ??= new Dictionary<string, QualifiedType>(0);
            _ = _readInformation.ParentTypes;

            if (LatestCommitHash != null)
            {
                _readInformation.Commit = LatestCommitHash;
            }

            await Task.WhenAll(
                new Task[]
                {
                    // Read Status.json file which includes some needed properties like game version and NPC-assignable achievements.
                    ReadKnownFileAsync(providers, enabled, KnownConfigurationFile.GameStatus, static async (stream, reader, token) =>
                    {
                        reader._statusFile = await JsonDocument.ParseAsync(stream, new JsonDocumentOptions
                        {
                            AllowTrailingCommas = true,
                            CommentHandling = JsonCommentHandling.Skip,
                            MaxDepth = 12
                        }, token).ConfigureAwait(false);
                    }, token: token),

                    // Read item actions from inventory localization file
                    ReadKnownFileAsync(providers, enabled, KnownConfigurationFile.InventoryLocalization, static async (stream, reader, token) =>
                    {
                        string allText = await ReadAllTextAsync(stream, token).ConfigureAwait(false);
                        using StaticSourceFile file = StaticSourceFile.FromOtherFile(string.Empty, allText.AsMemory(), null, SourceNodeTokenizerOptions.Lazy);
                        reader.ReadActionLabels(file.SourceFile);
                    }, token: token),

                    // Read skill names and descriptions from skills localization file.
                    ReadKnownFileAsync(providers, enabled, KnownConfigurationFile.SkillsLocalization, static async (stream, reader, token) =>
                    {
                        string allText = await ReadAllTextAsync(stream, token).ConfigureAwait(false);
                        using StaticSourceFile file = StaticSourceFile.FromOtherFile(string.Empty, allText.AsMemory(), null, SourceNodeTokenizerOptions.Lazy);
                        reader.ReadSkills(file.SourceFile);
                    }, token: token),

                    // Read skillset names from character menu localization file
                    ReadKnownFileAsync(providers, enabled, KnownConfigurationFile.CharacterLocalization, static async (stream, reader, token) =>
                    {
                        string allText = await ReadAllTextAsync(stream, token).ConfigureAwait(false);
                        using StaticSourceFile file = StaticSourceFile.FromOtherFile(string.Empty, allText.AsMemory(), null, SourceNodeTokenizerOptions.Lazy);
                        reader.ReadSkillsets(file.SourceFile);
                    }, token: token)
                }
            );

            // BFS to read asset files in the correct order (lowest type to highest type, ex. Asset -> ItemAsset -> ItemWeaponAsset -> ItemGunAsset).
            Queue<TypeHierarchy> types = new Queue<TypeHierarchy>(_readInformation.ParentTypes.Count);
            foreach (TypeHierarchy baseType in _readInformation.Types.Values.Where(x => x.HasDataFiles))
            {
                types.Enqueue(baseType);
            }

            ImmutableDictionary<QualifiedType, DatFileType>.Builder resolvedTypes;

            _assetFiles = new Dictionary<QualifiedType, JsonDocument>(_readInformation.ParentTypes.Count);
            List<QualifiedType> typeReadOrder = new List<QualifiedType>(_readInformation.ParentTypes.Count);
            try
            {
                while (types.Count > 0)
                {
                    TypeHierarchy type = types.Dequeue();

                    _currentType = type.Type;

                    if (!await ReadAssetFileAsync(providers, enabled, type.Type, static async (stream, reader, token) =>
                        {
                            reader._assetFiles![reader._currentType] = await JsonDocument.ParseAsync(stream,
                                new JsonDocumentOptions
                                {
                                    AllowTrailingCommas = true,
                                    CommentHandling = JsonCommentHandling.Skip,
                                    MaxDepth = 16
                                }, token);
                        }, token: token))
                    {
                        _logger.LogWarning(Resources.Log_FailedToReadAssetFile, type.Type.GetFullTypeName());
                        continue;
                    }

                    typeReadOrder.Add(_currentType);

                    foreach (TypeHierarchy hierarchy in type.ChildTypes.Values.Where(x => x.HasDataFiles))
                    {
                        types.Enqueue(hierarchy);
                    }
                }

                _currentType = QualifiedType.None;

                resolvedTypes = ImmutableDictionary.CreateBuilder<QualifiedType, DatFileType>();

                foreach (QualifiedType type in typeReadOrder)
                {
                    ReadFileType(_assetFiles[type], type.CaseInsensitive, resolvedTypes);
                }
            }
            catch
            {
                foreach (JsonDocument doc in _assetFiles.Values)
                {
                    doc.Dispose();
                }

                throw;
            }
            finally
            {
                _assetFiles = null;
            }

            _types = resolvedTypes.ToImmutable();
        }
        finally
        {
            Array.ForEach(providers, p => (p as IDisposable)?.Dispose());
        }

        await generateLocalizationFiles;
    }

    private async Task GenerateLocalizationFiles(CancellationToken token)
    {
        if (_installDir == null || !_installDir.TryGetInstallDirectory(out GameInstallDir installDirectory))
            return;

        string folder = Path.Combine(installDirectory.BaseFolder, "Localization", "English");

        ImmutableDictionary<string, DatFileType>.Builder localizationFiles = ImmutableDictionary.CreateBuilder<string, DatFileType>(StringComparer.Ordinal);

        foreach (string file in Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories))
        {
            string ext = Path.GetExtension(file);
            if (!string.Equals(ext, ".txt", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(ext, ".dat", StringComparison.OrdinalIgnoreCase)
                || file.Length <= folder.Length + 1)
            {
                continue;
            }

            string id = file.Substring(folder.Length + 1);
            if (string.IsNullOrEmpty(id))
                continue;

            DatFileType fileType = DatFileType.CreateFileType(id, false, default, null);
            fileType.IsKeyOnlyLocalizationFile = _readInformation != null && Array.IndexOf(_readInformation.KeyOnlyLocalizationFiles!, id) >= 0;

            ImmutableArray<DatProperty>.Builder propertiesBuilder = ImmutableArray.CreateBuilder<DatProperty>();

            if (_readInformation != null && Array.IndexOf(_readInformation.LegacyParsedLocalizationFiles!, id) >= 0)
            {
                if (!fileType.IsKeyOnlyLocalizationFile)
                {
                    using StreamReader sr = new StreamReader(file, Encoding.UTF8);
                    while (await sr.ReadLineAsync() is { } line)
                    {
                        if (line.Length == 0 || line[0] == '#')
                            continue;

                        line = line.TrimStart();

                        int index = line.IndexOf(' ');
                        
                        int firstValueIndex = index + 1;
                        while (firstValueIndex < line.Length && line[firstValueIndex] == ' ')
                            ++firstValueIndex;

                        propertiesBuilder.Add(index < 0
                            ? DatProperty.CreateLocalizationKey(line, null, fileType)
                            : DatProperty.CreateLocalizationKey(line.Substring(0, index), firstValueIndex < line.Length ? line.Substring(firstValueIndex) : null, fileType));
                    }
                }
            }
            else
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
                string text = await File.ReadAllTextAsync(file, token);
#else
                string text = File.ReadAllText(file);
#endif
                using StaticSourceFile sourceFile = StaticSourceFile.FromOtherFile(string.Empty, text.AsMemory(), null, SourceNodeTokenizerOptions.Lazy);
                
                foreach (IPropertySourceNode property in sourceFile.SourceFile.Properties)
                {
                    propertiesBuilder.Add(property.Value is not IValueSourceNode val
                        ? DatProperty.CreateLocalizationKey(property.Key, null, fileType)
                        : DatProperty.CreateLocalizationKey(property.Key, val.Value, fileType));
                }
            }

            fileType.Properties = propertiesBuilder.MoveToImmutableOrCopy();

            localizationFiles.Add(id, fileType);
        }

        _localizationFiles = localizationFiles.ToImmutable();
    }

    private static async Task<string> ReadAllTextAsync(Stream stream, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        using StreamReader reader = new StreamReader(stream, Encoding.UTF8, bufferSize: 2048, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await reader.ReadToEndAsync(
#if NET7_0_OR_GREATER
            token
#endif
        ).ConfigureAwait(false);
    }

    private void ReadSkills(ISourceFile file)
    {
        SpecialityInfo?[]? specialities = _readInformation?.Specialities;
        if (specialities == null)
            return;

        for (int i = 0; i < specialities.Length; ++i)
        {
            string iStr = i.ToString(CultureInfo.InvariantCulture);
            SpecialityInfo? info = specialities[i];
            if (info == null)
                continue;

            info.DisplayName = file.TryGetPropertyValue($"Speciality_{iStr}_Tooltip", out IValueSourceNode? value) ? value.Value : null;
            if (info.Skills == null)
                continue;

            for (int s = 0; s < info.Skills.Length; ++s)
            {
                SkillInfo skillInfo = info.Skills[s];
                string sStr = s.ToString(CultureInfo.InvariantCulture);
                skillInfo.DisplayName = file.TryGetPropertyValue($"Speciality_{iStr}_Skill_{sStr}", out value) ? value.Value : null;
                skillInfo.Description = file.TryGetPropertyValue($"Speciality_{iStr}_Skill_{sStr}_Tooltip", out value) ? value.Value : null;
                if (file.TryGetPropertyValue($"Speciality_{iStr}_Skill_{sStr}_Levels_V2", out value))
                {
                    skillInfo.Levels = [ value.Value ];
                }
                else
                {
                    string?[] levels = new string?[skillInfo.MaximumLevel];
                    for (int l = 1; l >= skillInfo.MaximumLevel; ++l)
                    {
                        levels[l - 1] = file.TryGetPropertyValue($"Speciality_{iStr}_Skill_{sStr}_Level_{l.ToString(CultureInfo.InvariantCulture)}", out value) ? value.Value : null;
                    }

                    skillInfo.Levels = levels;
                }
            }
        }
    }

    private void ReadSkillsets(ISourceFile file)
    {
        SkillsetInfo?[]? skillsets = _readInformation?.Skillsets;
        if (skillsets == null)
            return;

        for (int i = 0; i < skillsets.Length; ++i)
        {
            skillsets[i]?.DisplayName = file.TryGetPropertyValue($"Skillset_{i.ToString(CultureInfo.InvariantCulture)}", out IValueSourceNode? value)
                ? value.Value
                : null;
        }
    }

    private void ReadActionLabels(ISourceFile file)
    {
        ImmutableArray<string>.Builder builder = ImmutableArray.CreateBuilder<string>(16);
        foreach (IPropertySourceNode property in file.Properties)
        {
            if (property.KeyIsQuoted)
                continue;
            
            if (property.Key.EndsWith("_Button")
                && property is { HasValue: true, ValueKind: ValueTypeDataRefType.Value }
                && file.TryGetProperty(property.Key + "_Tooltip", out IPropertySourceNode? tooltipProperty)
                && tooltipProperty is { HasValue: true, ValueKind: ValueTypeDataRefType.Value })
            {
                builder.Add(property.Key[..^7]);
            }
        }

        _actionButtons = builder.MoveToImmutableOrCopy();
    }

    private async Task<bool> ReadKnownFileAsync(
        ISpecificationFileProvider[] providers,
        bool[] enabled,
        KnownConfigurationFile file,
        Func<Stream, SpecificationFileReader, CancellationToken, Task> action,
        bool isFirst = false,
        CancellationToken token = default)
    {
        // find asset information file (Assets.json)
        for (int i = 0; i < providers.Length; i++)
        {
            ISpecificationFileProvider fileProvider = providers[i];
            if (isFirst)
            {
                enabled[i] = fileProvider.IsEnabled;
            }

            if (!enabled[i])
                continue;

            if (await fileProvider.ReadKnownFileAsync(file, this, action, token))
                return true;
        }

        return false;
    }

    private async Task<bool> ReadAssetFileAsync(
        ISpecificationFileProvider[] providers,
        bool[] enabled,
        QualifiedType fileType,
        Func<Stream, SpecificationFileReader, CancellationToken, Task> action,
        bool isFirst = false,
        CancellationToken token = default)
    {
        // find asset information file (Assets.json)
        for (int i = 0; i < providers.Length; i++)
        {
            ISpecificationFileProvider fileProvider = providers[i];
            if (isFirst)
            {
                enabled[i] = fileProvider.IsEnabled;
            }

            if (!enabled[i])
                continue;

            if (await fileProvider.ReadAssetAsync(fileType, this, action, token))
                return true;
        }

        return false;
    }

    internal static async Task<string?> GetLatestCommitAsync(HttpClient httpClient, CancellationToken token)
    {
        const string getLatestCommitUrl = $"https://api.github.com/repos/{GitHubSpecificationFileProvider.ThisProjectRepository}/commits?per_page=1";

        JsonDocument? doc = null;
        try
        {
            using (HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, getLatestCommitUrl))
            {
                msg.Version = HttpVersionUtility.LatestVersion;

                msg.Headers.Add("Accept", "application/vnd.github+json");
                msg.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
                msg.Headers.Add("User-Agent", $"unturned-asset-file-vscode/{Assembly.GetExecutingAssembly().GetName().Version!.ToString(3)}");

                using (HttpResponseMessage response = await httpClient.SendAsync(msg, HttpCompletionOption.ResponseContentRead, token))
                {
                    doc = await JsonDocument.ParseAsync(
                        await response.Content.ReadAsStreamAsync(),
                        new JsonDocumentOptions
                        {
                            AllowTrailingCommas = false,
                            CommentHandling = JsonCommentHandling.Disallow,
                            MaxDepth = 8
                        }, token);
                }
            }

            // read commit @ root[0].sha
            JsonElement root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Array)
                return null;

            JsonElement commitInfo;
            using (JsonElement.ArrayEnumerator arrayEnumerator = root.EnumerateArray())
            {
                if (!arrayEnumerator.MoveNext())
                    return null;

                commitInfo = arrayEnumerator.Current;
            }

            if (!commitInfo.TryGetProperty("sha"u8, out JsonElement sha) || sha.ValueKind != JsonValueKind.String)
                return null;

            return sha.GetString();
        }
        finally
        {
            doc?.Dispose();
        }
    }
}
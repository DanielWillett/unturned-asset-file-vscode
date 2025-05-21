using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

public class AssetSpecDatabase : IDisposable
{
    public const string AssetFileUrl = "https://raw.githubusercontent.com/DanielWillett/unturned-asset-file-vscode/refs/heads/master/Asset%20Spec/{0}.json";
    private const string EmbeddedResourceLocation = "DanielWillett.UnturnedDataFileLspServer.Data..Asset_Spec.{0}.json";

    public const string UnturnedName = "Unturned";
    public const string UnturnedAppId = "304930";

    private JsonDocument? _statusJson;

    public string[]? NPCAchievementIds { get; private set; }
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

    public JsonSerializerOptions? Options { get; set; }
    public IReadOnlyList<string> ValidActionButtons { get; set; }

    public IReadOnlyDictionary<QualifiedType, AssetTypeInformation> Types { get; private set; } = new Dictionary<QualifiedType, AssetTypeInformation>(0);

    public AssetInformation Information { get; private set; } = new AssetInformation
    {
        AssetAliases = new Dictionary<string, QualifiedType>(0),
        AssetCategories = new Dictionary<QualifiedType, string>(0),
        Types = new Dictionary<QualifiedType, TypeHierarchy>(0),
        ParentTypes = new Dictionary<QualifiedType, InverseTypeHierarchy>(0)
    };

    public AssetSpecDatabase() : this(new InstallDirUtility(UnturnedName, UnturnedAppId)) { }

    public AssetSpecDatabase(InstallDirUtility unturnedInstallDirectory)
    {
        UnturnedInstallDirectory = unturnedInstallDirectory;
        ValidActionButtons = Array.Empty<string>();
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interlocked.Exchange(ref _statusJson, null)?.Dispose();
        }
    }

    public ISpecType? FindType(string type, AssetFileType fileType)
    {
        type = QualifiedType.NormalizeType(type);
        if (AssetCategory.TypeOf.Type.Equals(type))
        {
            return AssetCategory.TypeOf;
        }

        string? assetType = null;
        int divIndex = type.IndexOf("::", 0, StringComparison.Ordinal);
        if (divIndex >= 0 && divIndex < type.Length - 2)
        {
            if (divIndex != 0)
                assetType = type.Substring(0, divIndex);
            type = type.Substring(divIndex + 2);
        }

        InverseTypeHierarchy hierarchy = Information.GetParentTypes(assetType != null ? new QualifiedType(assetType) : fileType.Type);

        for (int i = -1; i < hierarchy.ParentTypes.Length; ++i)
        {
            QualifiedType qt = i < 0 ? hierarchy.Type : hierarchy.ParentTypes[hierarchy.ParentTypes.Length - i - 1];

            if (!Types.TryGetValue(qt, out AssetTypeInformation info))
            {
                continue;
            }

            ISpecType? t = info.Types.Find(p => p.Type.Equals(type));

            if (t != null)
            {
                return t;
            }
        }

        return null;
    }

    public SpecProperty? FindPropertyInfo(string property, AssetFileType fileType, SpecPropertyContext context = SpecPropertyContext.Property)
    {
        if (context is not SpecPropertyContext.Localization and not SpecPropertyContext.Property)
            throw new ArgumentOutOfRangeException(nameof(context));

        if (!fileType.IsValid)
        {
            return null;
        }

        string? assetType = null;
        bool isLocal = false, isProp = false;
        while (true)
        {
            int divIndex = property.IndexOf("::", 0, StringComparison.Ordinal);
            if (divIndex < 0 || divIndex >= property.Length - 2)
                break;

            if (divIndex != 0)
            {
                assetType = property.Substring(0, divIndex);
                if (assetType.Equals("$local$", StringComparison.OrdinalIgnoreCase))
                {
                    assetType = null;
                    isLocal = true;
                    isProp = false;
                }
                else if (assetType.Equals("$prop$", StringComparison.OrdinalIgnoreCase))
                {
                    assetType = null;
                    isProp = true;
                    isLocal = false;
                }
            }
            property = property.Substring(divIndex + 2);
        }

        InverseTypeHierarchy hierarchy = Information.GetParentTypes(assetType != null ? new QualifiedType(assetType) : fileType.Type);

        for (int i = -1; i < hierarchy.ParentTypes.Length; ++i)
        {
            QualifiedType type = i < 0 ? hierarchy.Type : hierarchy.ParentTypes[hierarchy.ParentTypes.Length - i - 1];

            if (!Types.TryGetValue(type, out AssetTypeInformation info))
            {
                continue;
            }

            List<SpecProperty> props = isLocal || context == SpecPropertyContext.Localization && !isProp ? info.LocalizationProperties : info.Properties;
            SpecProperty? prop = props.Find(p => p.Key.Equals(property, StringComparison.OrdinalIgnoreCase));
            prop ??= props.Find(p => p.Aliases.Contains(property, StringComparison.OrdinalIgnoreCase));

            if (prop != null)
            {
                return prop;
            }
        }

        if (!isLocal && context == SpecPropertyContext.Property)
        {
            return FindPropertyInfo("$prop$::" + property, fileType, SpecPropertyContext.Localization);
        }

        if (!isProp && context == SpecPropertyContext.Localization)
        {
            return FindPropertyInfo("$local$::" + property, fileType, SpecPropertyContext.Property);
        }

        return null;
    }

    public async Task InitializeAsync(CancellationToken token = default)
    {
        Options ??= new JsonSerializerOptions
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            MaxDepth = 14,
            AllowTrailingCommas = true
        };

        Options.Converters.Add(new SpecPropertyTypeConverter(this));

        Lazy<HttpClient> lazy = new Lazy<HttpClient>(LazyThreadSafetyMode.ExecutionAndPublication);

        AssetInformation? assetInfo;
        using (Stream? stream = await GetFileAsync(
            string.Format(AssetFileUrl, "Assets"),
            "Assets",
            lazy
        ).ConfigureAwait(false))
        {
            if (stream != null)
            {
                try
                {
                    assetInfo = await JsonSerializer.DeserializeAsync<AssetInformation>(stream, Options, token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log("Error parsing Assets.json file.");
                    Log(ex.ToString());
                    assetInfo = null;
                }
            }
            else
            {
                assetInfo = null;
            }
        }

        if (assetInfo == null && UseInternet)
        {
            using Stream? stream = await GetFileAsync(null, "Assets", lazy).ConfigureAwait(false);
            if (stream != null)
            {
                try
                {
                    assetInfo = await JsonSerializer.DeserializeAsync<AssetInformation>(stream, Options, token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log("Error parsing embedded Assets.json file.");
                    Log(ex.ToString());
                }
            }
        }

        assetInfo ??= new AssetInformation();
        Dictionary<string, QualifiedType>? aliases = assetInfo.AssetAliases;
        assetInfo.AssetAliases = new Dictionary<string, QualifiedType>(aliases?.Count ?? 0, StringComparer.OrdinalIgnoreCase);
        if (aliases != null)
        {
            foreach (KeyValuePair<string, QualifiedType> kvp in aliases)
                assetInfo.AssetAliases.Add(kvp.Key, kvp.Value);
        }
        assetInfo.AssetCategories ??= new Dictionary<QualifiedType, string>(0);
        assetInfo.Types ??= new Dictionary<QualifiedType, TypeHierarchy>(0);

        // custom type converter can't read property name
        foreach (KeyValuePair<QualifiedType, TypeHierarchy> type in assetInfo.Types)
        {
            type.Value.Type = new QualifiedType(type.Key);
        }

        assetInfo.GetParentTypes(default);
        assetInfo.ParentTypes ??= new Dictionary<QualifiedType, InverseTypeHierarchy>(0);


        Information = assetInfo;

        Task statusTask = DownloadStatusAsync(token);
        Task downloadActionButtons = DownloadPlayerDashboardInventoryLocalizationAsync(token);

        Dictionary<QualifiedType, AssetTypeInformation> types = new Dictionary<QualifiedType, AssetTypeInformation>(assetInfo.ParentTypes.Count);

        if (assetInfo.ParentTypes.Count == 0)
        {
            return;
        }

        const int perThreadCount = 5;

        Task[] tasks = new Task[(assetInfo.ParentTypes.Count - 1) / perThreadCount + 1];
        List<InverseTypeHierarchy> toProcess = new List<InverseTypeHierarchy>(perThreadCount);
        int taskIndex = 0;
        foreach (InverseTypeHierarchy type in assetInfo.ParentTypes.Values)
        {
            if (!type.Hierarchy.HasDataFiles)
                continue;

            toProcess.Add(type);
            if (toProcess.Count < perThreadCount)
                continue;

            Deprocess(tasks, ref taskIndex, toProcess, lazy, types, token);
            toProcess.Clear();
        }

        if (toProcess.Count > 0)
        {
            Deprocess(tasks, ref taskIndex, toProcess, lazy, types, token);
        }

        await Task.WhenAll(new ArraySegment<Task>(tasks, 0, taskIndex)).ConfigureAwait(false);

        await statusTask.ConfigureAwait(false);
        await downloadActionButtons.ConfigureAwait(false);

        if (lazy.IsValueCreated)
        {
            lazy.Value.Dispose();
        }

        Types = types;
        return;


        void Deprocess(Task[] tasks,
            ref int taskIndex,
            List<InverseTypeHierarchy> toProcess,
            Lazy<HttpClient> lazy,
            Dictionary<QualifiedType, AssetTypeInformation> types,
            CancellationToken token)
        {
            InverseTypeHierarchy[] processList = toProcess.ToArray();
            tasks[taskIndex] = Task.Run(async () =>
            {
                foreach (InverseTypeHierarchy type in processList)
                {
                    string normalizedTypeName = type.Type.Normalized.Type.ToLowerInvariant();
                    AssetTypeInformation? typeInfo;

                    using (Stream? stream = await GetFileAsync(
                               string.Format(AssetFileUrl, Uri.EscapeDataString(normalizedTypeName)),
                               normalizedTypeName,
                               lazy
                           ).ConfigureAwait(false))
                    {
                        if (stream != null)
                        {
                            try
                            {
                                typeInfo = await JsonSerializer.DeserializeAsync<AssetTypeInformation>(stream, Options, token).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                lock (this)
                                {
                                    Log($"Error parsing {normalizedTypeName}.json file.");
                                    Log(ex.ToString());
                                }
                                typeInfo = null;
                            }
                        }
                        else
                        {
                            typeInfo = null;
                        }
                    }

                    if (typeInfo == null && UseInternet)
                    {
                        using Stream? stream = await GetFileAsync(null, normalizedTypeName, lazy).ConfigureAwait(false);
                        if (stream != null)
                        {
                            try
                            {
                                typeInfo = await JsonSerializer.DeserializeAsync<AssetTypeInformation>(stream, Options, token).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                lock (this)
                                {
                                    Log($"Error parsing embedded {normalizedTypeName}.json file.");
                                    Log(ex.ToString());
                                }
                            }
                        }
                    }

                    typeInfo ??= new AssetTypeInformation();
                    lock (types)
                    {
                        types[typeInfo.Type] = typeInfo;
                    }
                }
            }, token);
            ++taskIndex;
        }
    }

    protected virtual async Task<Stream?> GetFileAsync(string? url, string fallbackEmbeddedResource, Lazy<HttpClient> httpClient)
    {
        if (!UseInternet || url == null)
            return GetEmbeddedStream(fallbackEmbeddedResource);

        try
        {
            using HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, url);

            using HttpResponseMessage response = await httpClient.Value.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            MemoryStream ms = new MemoryStream();

            await response.Content.CopyToAsync(ms).ConfigureAwait(false);
            ms.Seek(0L, SeekOrigin.Begin);
            return ms;
        }
        catch (Exception ex)
        {
            lock (this)
            {
                Log($"Error downloading \"{url}\".");
                Log(ex.ToString());
            }
        }

        return GetEmbeddedStream(fallbackEmbeddedResource);
    }

    private Stream? GetEmbeddedStream(string fallbackEmbeddedResource)
    {
        if (!fallbackEmbeddedResource.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            fallbackEmbeddedResource = string.Format(EmbeddedResourceLocation, fallbackEmbeddedResource);

        Stream? stream;
        try
        {
            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fallbackEmbeddedResource);
            if (stream == null)
            {
                lock (this)
                    Log($"Couldn't find embedded resource \"{fallbackEmbeddedResource}\".");
            }
        }
        catch (Exception ex)
        {
            lock (this)
            {
                Log($"Error finding embedded resource \"{fallbackEmbeddedResource}\".");
                Log(ex.ToString());
            }
            stream = null;
        }

        return stream;
    }

    private async Task DownloadStatusAsync(CancellationToken token = default)
    {
        if (UnturnedInstallDirectory.TryGetInstallDirectory(out GameInstallDir installDir))
        {
            string statusPath = installDir.GetFile("Status.json");
            if (File.Exists(statusPath))
            {
                try
                {
                    using FileStream fs = new FileStream(statusPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                    JsonDocument doc = await JsonDocument.ParseAsync(fs, new JsonDocumentOptions
                    {
                        AllowTrailingCommas = true,
                        CommentHandling = JsonCommentHandling.Skip,
                        MaxDepth = 6
                    }, token).ConfigureAwait(false);

                    if (Interlocked.CompareExchange(ref _statusJson, doc, null) != null)
                    {
                        doc.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Log("Error reading Status.json file from local install.");
                    Log(ex.ToString());
                }
            }
        }

        JsonDocument? doc2 = _statusJson;
        if (doc2 == null)
        {
            if (!UseInternet || string.IsNullOrEmpty(Information.StatusJsonFallbackUrl))
            {
                Log("Unable to read Status.json from local install, internet disabled.");
                return;
            }

            try
            {
                using HttpClient client = new HttpClient();
                using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, Information.StatusJsonFallbackUrl);
                using HttpResponseMessage response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);

                response.EnsureSuccessStatusCode();

                JsonDocument doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip,
                    MaxDepth = 6
                }, token).ConfigureAwait(false);

                if (Interlocked.CompareExchange(ref _statusJson, doc, null) != null)
                {
                    doc.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log("Unable to read Status.json from internet.");
                Log(ex.ToString());
            }
        }

        JsonDocument? statusDoc = _statusJson;
        if (statusDoc == null)
        {
            return;
        }

        if (statusDoc.RootElement.TryGetProperty("Achievements", out JsonElement achievementSection)
            && achievementSection.ValueKind == JsonValueKind.Object
            && achievementSection.TryGetProperty("NPC_Achievement_IDs", out JsonElement npcAchieveemnts)
            && npcAchieveemnts.ValueKind == JsonValueKind.Array)
        {
            List<string> achievements = new List<string>();
            foreach (JsonElement achievementId in npcAchieveemnts.EnumerateArray())
            {
                if (achievementId.ValueKind != JsonValueKind.String)
                    continue;

                achievements.Add(achievementId.GetString());
            }

            NPCAchievementIds = achievements.ToArray();
        }

        if (statusDoc.RootElement.TryGetProperty("Game", out JsonElement gameSection)
            && gameSection.ValueKind == JsonValueKind.Object
            && gameSection.TryGetProperty("Major_Version", out JsonElement majorSection)
            && majorSection.ValueKind == JsonValueKind.Number
            && gameSection.TryGetProperty("Minor_Version", out JsonElement minorSection)
            && minorSection.ValueKind == JsonValueKind.Number
            && gameSection.TryGetProperty("Patch_Version", out JsonElement patchSection)
            && patchSection.ValueKind == JsonValueKind.Number
            && majorSection.TryGetInt32(out int major)
            && minorSection.TryGetInt32(out int minor)
            && patchSection.TryGetInt32(out int patch))
        {
            CurrentGameVersion = new Version(3, major, minor, patch);
        }
    }

    private async Task DownloadPlayerDashboardInventoryLocalizationAsync(CancellationToken token = default)
    {
        List<string> validButtons = new List<string>(24);
        if (UnturnedInstallDirectory.TryGetInstallDirectory(out GameInstallDir installDir))
        {
            string localPath = installDir.GetFile(@"Localization\English\Player\PlayerDashboardInventory.dat");
            if (File.Exists(localPath))
            {
                try
                {
                    using FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                    await ReadLocalizationFileAsync(fs, validButtons).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log("Error reading PlayerDashboardInventory.dat file from local install.");
                    Log(ex.ToString());
                }
            }
        }

        if (validButtons.Count == 0)
        {
            if (!UseInternet || string.IsNullOrEmpty(Information.PlayerDashboardInventoryLocalizationFallbackUrl))
            {
                Log("Unable to read PlayerDashboardInventory.dat from local install, internet disabled.");
                return;
            }

            try
            {
                using HttpClient client = new HttpClient();
                using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, Information.PlayerDashboardInventoryLocalizationFallbackUrl);
                using HttpResponseMessage response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);

                response.EnsureSuccessStatusCode();

                using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                await ReadLocalizationFileAsync(stream, validButtons).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log("Unable to read PlayerDashboardInventory.dat from internet.");
                Log(ex.ToString());
            }
        }

        ValidActionButtons = validButtons.AsReadOnly();
        return;

        static async Task ReadLocalizationFileAsync(Stream stream, List<string> buttons)
        {
            buttons.Clear();
            List<string> tooltips = new List<string>(24);
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true))
            {
                while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
                {
                    if (line.Length == 0 || line[0] == '#' || line[0] == '/')
                        continue;
                    int space = line.IndexOf(' ');
                    if (space <= 0)
                        continue;

                    ReadOnlySpan<char> key = line.AsSpan(0, space);
                    if (key.EndsWith("_Button".AsSpan(), StringComparison.Ordinal) && key.Length > 7)
                    {
                        string button = key.Slice(0, key.Length - 7).ToString();
                        if (!buttons.Contains(button))
                            buttons.Add(button);
                    }
                    else if (key.EndsWith("_Button_Tooltip".AsSpan(), StringComparison.Ordinal) && key.Length > 15)
                    {
                        tooltips.Add(key.Slice(0, key.Length - 15).ToString());
                    }
                }
            }

            buttons.RemoveAll(x => !tooltips.Contains(x));
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
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
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
    /// JSON options used to read files.
    /// </summary>
    JsonSerializerOptions? Options { get; set; }

    /// <summary>
    /// List of achievements available for NPCs to grant.
    /// </summary>
    string[]? NPCAchievementIds { get; }

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
    IReadOnlyList<string> ValidActionButtons { get; }

    /// <summary>
    /// List of asset files by their type.
    /// </summary>
    IReadOnlyDictionary<QualifiedType, AssetSpecType> Types { get; }
    
    /// <summary>
    /// Initialize the database.
    /// </summary>
    Task InitializeAsync(CancellationToken token = default);
}

public class AssetSpecDatabase : IDisposable, IAssetSpecDatabase
{
    public const string Repository = "DanielWillett/unturned-asset-file-vscode";
    public const string AssetFileUrl = "https://raw.githubusercontent.com/" + Repository + "/refs/heads/master/Asset%20Spec/{0}.json";
    private const string EmbeddedResourceLocation = "DanielWillett.UnturnedDataFileLspServer.Data..Asset_Spec.{0}.json";

    public const string UnturnedName = "Unturned";
    public const string UnturnedAppId = "304930";

    private readonly ISpecDatabaseCache? _cache;
    private string? _commit;
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

    // for debugging
    public bool MultiThreaded { get; set; } = true;

    public JsonSerializerOptions? Options { get; set; }
    public IReadOnlyList<string> ValidActionButtons { get; set; }

    public IReadOnlyDictionary<QualifiedType, AssetSpecType> Types { get; private set; } = new Dictionary<QualifiedType, AssetSpecType>(0);

    public AssetInformation Information { get; private set; } = new AssetInformation
    {
        AssetAliases = new Dictionary<string, QualifiedType>(0),
        AssetCategories = new Dictionary<QualifiedType, string>(0),
        Types = new Dictionary<QualifiedType, TypeHierarchy>(0),
        ParentTypes = new Dictionary<QualifiedType, InverseTypeHierarchy>(0)
    };

    public AssetSpecDatabase(ISpecDatabaseCache? cache = null) : this(new InstallDirUtility(UnturnedName, UnturnedAppId), cache) { }

    public AssetSpecDatabase(InstallDirUtility unturnedInstallDirectory, ISpecDatabaseCache? cache = null)
    {
        _cache = cache;
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

    [MemberNotNullWhen(true, "_cache")]
    private bool IsCacheUpToDate { get; set; }

    public async Task InitializeAsync(CancellationToken token = default)
    {
        IsCacheUpToDate = false;
        _commit = null;

        Options ??= new JsonSerializerOptions
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            MaxDepth = 14,
            AllowTrailingCommas = true
        };

        Options.Converters.Add(new SpecPropertyTypeConverter());
        Options.Converters.Add(new ColorConverter());
        Options.Converters.Add(new Color32Converter());
        Options.Converters.Add(new AssetSpecTypeConverter());
        Options.Converters.Add(new BundleReferenceConverter());
        Options.Converters.Add(new GuidOrIdConverter());
        Options.Converters.Add(new InclusionConditionConverter());
        Options.Converters.Add(new QualifiedTypeConverter());
        Options.Converters.Add(new SpecBundleAssetConverter());
        Options.Converters.Add(new SpecConditionConverter());
        Options.Converters.Add(new SpecDynamicSwitchCaseValueConverter());
        Options.Converters.Add(new SpecDynamicSwitchValueConverter());
        Options.Converters.Add(new SpecDynamicValueConverter());
        Options.Converters.Add(new SpecPropertyConverter());
        Options.Converters.Add(new SpecTypeConverter());
        Options.Converters.Add(new UnityEngineVersionConverter());
        Options.Converters.Add(new TypeHierarchyConverter());

        Lazy<HttpClient> lazy = new Lazy<HttpClient>(LazyThreadSafetyMode.ExecutionAndPublication);

        if (_cache != null)
        {
            string? latestCommit = await GetLatestCommitAsync(lazy.Value, token);
            if (latestCommit != null)
            {
                _commit = latestCommit;
                IsCacheUpToDate = _cache.IsUpToDateCache(latestCommit);
            }
        }

        AssetInformation assetInfo = await DownloadAssetInfoAsync(lazy, token);
        
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


        Task statusTask = DownloadStatusAsync(assetInfo, token);
        Task downloadActionButtons = DownloadPlayerDashboardInventoryLocalizationAsync(assetInfo, token);
        await DownloadSkillLocalization(assetInfo, token);

        Information = assetInfo;
        Dictionary<QualifiedType, AssetSpecType> types = new Dictionary<QualifiedType, AssetSpecType>(assetInfo.ParentTypes.Count);

        if (assetInfo.ParentTypes.Count == 0)
        {
            return;
        }

        int perThreadCount = MultiThreaded ? 5 : assetInfo.ParentTypes.Count;

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

        Dictionary<QualifiedType, int> passes = new Dictionary<QualifiedType, int>(types.Count * 4);

        AssetSpecDatabaseWrapper wrapper = new AssetSpecDatabaseWrapper(this, types);

        PerformSecondPass(types, passes, wrapper);
        PerformThirdPass(types, passes, wrapper);
        PerformFourthPass(types, passes);

        Types = types;

        if (_cache != null)
        {
            await _cache.CacheNewFilesAsync(this, token);
        }
        return;

        void Deprocess(Task[] tasks,
            ref int taskIndex,
            List<InverseTypeHierarchy> toProcess,
            Lazy<HttpClient> lazy,
            Dictionary<QualifiedType, AssetSpecType> types,
            CancellationToken token)
        {
            InverseTypeHierarchy[] processList = toProcess.ToArray();
            tasks[taskIndex] = Task.Run(async () =>
            {
                foreach (InverseTypeHierarchy type in processList)
                {
                    QualifiedType typeName = type.Type.Normalized;
                    string normalizedTypeName = typeName.Type.ToLowerInvariant();
                    AssetSpecType? typeInfo = null;

                    if (IsCacheUpToDate)
                    {
                        typeInfo = await _cache.GetCachedTypeAsync(typeName, token).ConfigureAwait(false);
                    }

                    if (typeInfo == null)
                    {
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
                                    typeInfo = await JsonSerializer.DeserializeAsync<AssetSpecType>(stream, Options, token).ConfigureAwait(false);
                                    if (stream is MemoryStream)
                                        typeInfo.Commit = _commit;
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
                    }

                    if (typeInfo == null && UseInternet)
                    {
                        using Stream? stream = await GetFileAsync(null, normalizedTypeName, lazy).ConfigureAwait(false);
                        if (stream != null)
                        {
                            try
                            {
                                typeInfo = await JsonSerializer.DeserializeAsync<AssetSpecType>(stream, Options, token).ConfigureAwait(false);
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

                    typeInfo ??= new AssetSpecType();

                    if (typeInfo.Type.IsNull)
                    {
                        Log($"Missing type in {normalizedTypeName}.json file.");
                    }
                    else
                    {
                        lock (types)
                        {
                            types[typeInfo.Type] = typeInfo;
                        }
                    }
                }
            }, token);
            ++taskIndex;
        }
    }

    private static async Task<string?> GetLatestCommitAsync(HttpClient httpClient, CancellationToken token)
    {
        const string getLatestCommitUrl = $"https://api.github.com/repos/{Repository}/commits?per_page=1";

        HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, getLatestCommitUrl);
        msg.Headers.Add("Accept", "application/vnd.github+json");
        msg.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
        msg.Headers.Add("User-Agent", $"unturned-asset-file-vscode/{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}");

        HttpResponseMessage response = await httpClient.SendAsync(msg, HttpCompletionOption.ResponseContentRead, token);

        // read commit @ root[0].sha
        using JsonDocument doc = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(),
            new JsonDocumentOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
                MaxDepth = 8
            }, token);

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

    private async Task<AssetInformation> DownloadAssetInfoAsync(Lazy<HttpClient> lazy, CancellationToken token)
    {
        AssetInformation? assetInfo = null;

        if (IsCacheUpToDate)
        {
            assetInfo = await _cache.GetCachedInformationAsync(token);
            if (assetInfo != null)
                return assetInfo;
        }

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
                    if (stream is MemoryStream)
                        assetInfo.Commit = _commit;
                }
                catch (Exception ex)
                {
                    Log("Error parsing Assets.json file.");
                    Log(ex.ToString());
                }
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

        return assetInfo ?? new AssetInformation();
    }

    public void ForEachTypeInHierarchyWhile(ISpecType type, Func<ISpecType, bool> each)
    {
        ForEachTypeInHierarchyWhile(type, Types, each, false);
    }

    private static void ForEachTypeInHierarchyWhile(ISpecType type, IReadOnlyDictionary<QualifiedType, AssetSpecType> types, Func<ISpecType, bool> each, bool reversed = false, bool skipSelf = false)
    {
        if (skipSelf && type.Parent.IsNull)
            return;

        if (type is AssetSpecType specType)
        {
            if (reversed && !specType.Parent.IsNull)
            {
                Stack<AssetSpecType> stack = new Stack<AssetSpecType>(4);
                for (AssetSpecType? assetType = specType; assetType != null; types.TryGetValue(assetType.Parent, out assetType))
                {
                    if (skipSelf && ReferenceEquals(assetType, specType))
                        continue;
                    stack.Push(assetType);
                }
                while (stack.Count > 0)
                {
                    if (!each(stack.Pop()))
                        return;
                }
            }
            else
            {
                for (AssetSpecType? assetType = specType; assetType != null; types.TryGetValue(assetType.Parent, out assetType))
                {
                    if (skipSelf && ReferenceEquals(assetType, specType))
                        continue;
                    if (!each(assetType))
                        return;
                }
            }
        }
        else if (!reversed || type.Parent.IsNull)
        {
            if (!skipSelf && !each(type) || type.Parent.IsNull)
                return;

            ISpecType? parentType = type;
            while (!parentType.Parent.IsNull)
            {
                QualifiedType parent = parentType.Parent;
                for (AssetSpecType? assetType = type.Owner; assetType != null; types.TryGetValue(assetType.Parent, out assetType))
                {
                    parentType = Array.Find(assetType.Types, t => t.Type.Equals(parent));
                    if (parentType != null)
                        break;
                }

                if (parentType == null || !each(parentType))
                    return;
            }
        }
        else
        {
            Stack<ISpecType> stack = new Stack<ISpecType>(2);
            if (!skipSelf)
                stack.Push(type);
            ISpecType? parentType = type;
            while (!parentType.Parent.IsNull)
            {
                QualifiedType parent = parentType.Parent;
                for (AssetSpecType? assetType = type.Owner; assetType != null; types.TryGetValue(assetType.Parent, out assetType))
                {
                    parentType = Array.Find(assetType.Types, t => t.Type.Equals(parent));
                    if (parentType != null)
                        break;
                }

                if (parentType == null)
                    return;

                stack.Push(parentType);
            }
            while (stack.Count > 0)
            {
                if (!each(stack.Pop()))
                    return;
            }
        }
    }

    private static void ForEachPropertyWhile(ISpecType type, Func<SpecProperty, bool> each)
    {
        switch (type)
        {
            case AssetSpecType s:
                foreach (SpecProperty property in s.Properties)
                {
                    if (!each(property))
                        return;
                }
                foreach (SpecProperty property in s.LocalizationProperties)
                {
                    if (!each(property))
                        return;
                }
                break;

            case CustomSpecType c:
                foreach (SpecProperty property in c.Properties)
                {
                    if (!each(property))
                        return;
                }
                foreach (SpecProperty property in c.LocalizationProperties)
                {
                    if (!each(property))
                        return;
                }
                break;
        }
    }

    /// <summary>
    /// Replaces properties with unresolved types with the correct types.
    /// </summary>
    private void PerformSecondPass(Dictionary<QualifiedType, AssetSpecType> types, Dictionary<QualifiedType, int> passes, AssetSpecDatabaseWrapper wrapper)
    {
        const int pass = 2;
        foreach (AssetSpecType info in types.Values)
        {
            ForEachTypeInHierarchyWhile(info, types, t =>
            {
                Run(t, passes, wrapper);
                return true;
            }, reversed: true);
            foreach (CustomSpecType type in info.Types.OfType<CustomSpecType>())
            {
                ForEachTypeInHierarchyWhile(type, types, t =>
                {
                    Run(t, passes, wrapper);
                    return true;
                }, reversed: true);
            }
        }

        return;
        void Run(ISpecType info, Dictionary<QualifiedType, int> passes, AssetSpecDatabaseWrapper wrapper)
        {
            if (passes.TryGetValue(info.Type, out int v) && v >= pass)
                return;

            passes[info.Type] = pass;

            ForEachPropertyWhile(info, prop =>
            {
                if (prop.Type.Type is not ISecondPassSpecPropertyType s)
                    return true;

                try
                {
                    prop.Type = new PropertyTypeOrSwitch(s.Transform(prop, wrapper, info.Owner));
                    if (s is IDisposable disp)
                        disp.Dispose();
                }
                catch (Exception ex)
                {
                    Log($"Failed to perform second pass on property \"{prop.Key}\" in type \"{prop.Owner.Type}\"");
                    Log(ex.ToString());
                }

                return true;
            });
        }
    }

    /// <summary>
    /// Replaces values with unresolved values with the correct values.
    /// </summary>
    private void PerformThirdPass(Dictionary<QualifiedType, AssetSpecType> types, Dictionary<QualifiedType, int> passes, AssetSpecDatabaseWrapper wrapper)
    {
        const int pass = 3;
        foreach (AssetSpecType info in types.Values)
        {
            ForEachTypeInHierarchyWhile(info, types, t =>
            {
                Run(t, passes, wrapper);
                return true;
            }, reversed: true);
            foreach (CustomSpecType type in info.Types.OfType<CustomSpecType>())
            {
                ForEachTypeInHierarchyWhile(type, types, t =>
                {
                    Run(t, passes, wrapper);
                    return true;
                }, reversed: true);
            }
        }

        return;
        void Run(ISpecType info, Dictionary<QualifiedType, int> passes, AssetSpecDatabaseWrapper wrapper)
        {
            if (passes.TryGetValue(info.Type, out int v) && v >= pass)
                return;

            passes[info.Type] = pass;

            ForEachPropertyWhile(info, prop =>
            {
                prop.ProcessValues(value =>
                {
                    if (value is not ISecondPassSpecDynamicValue s)
                        return value;

                    try
                    {
                        value = s.Transform(prop, wrapper, info.Owner);
                        if (s is IDisposable disp)
                            disp.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log($"Failed to perform second pass on property \"{prop.Key}\" in type \"{prop.Owner.Type}\"");
                        Log(ex.ToString());
                    }

                    return value;
                });

                return true;
            });
        }
    }

    /// <summary>
    /// Copies properties from parent types up to the current object.
    /// </summary>
    private void PerformFourthPass(Dictionary<QualifiedType, AssetSpecType> types, Dictionary<QualifiedType, int> passes)
    {
        const int pass = 4;
        foreach (AssetSpecType info in types.Values)
        {
            ForEachTypeInHierarchyWhile(info, types, t =>
            {
                Run(t, types, passes);
                return true;
            }, reversed: true);
            foreach (CustomSpecType type in info.Types.OfType<CustomSpecType>())
            {
                ForEachTypeInHierarchyWhile(type, types, t =>
                {
                    Run(t, types, passes);
                    return true;
                }, reversed: true);
            }
        }

        return;
        void Run(ISpecType info, Dictionary<QualifiedType, AssetSpecType> types, Dictionary< QualifiedType, int> passes)
        {
            if (passes.TryGetValue(info.Type, out int v) && v >= pass)
                return;

            passes[info.Type] = pass;

            ForEachTypeInHierarchyWhile(info, types, t =>
            {
                SpecProperty[]
                    props0 = info.GetProperties(SpecPropertyContext.Property),
                    local0 = info.GetProperties(SpecPropertyContext.Localization),
                    asset0 = info.GetProperties(SpecPropertyContext.BundleAsset),
                    props1 = t.GetProperties(SpecPropertyContext.Property),
                    local1 = t.GetProperties(SpecPropertyContext.Localization),
                    asset1 = t.GetProperties(SpecPropertyContext.BundleAsset);

                info.SetProperties(Merge(info, props0, props1), SpecPropertyContext.Property);
                info.SetProperties(Merge(info, local0, local1), SpecPropertyContext.Localization);
                info.SetProperties(Merge(info, asset0, asset1), SpecPropertyContext.BundleAsset);
                return false;

                SpecProperty[] Merge(ISpecType owner, SpecProperty[] p0, SpecProperty[] p1)
                {
                    if (p1.Length == 0)
                        return p0;

                    if (p1 is SpecBundleAsset[] bundles)
                    {
                        List<SpecBundleAsset> newProperties = new List<SpecBundleAsset>(p0.Length + p1.Length);
                        newProperties.AddRange((SpecBundleAsset[])p0);

                        foreach (SpecBundleAsset prop in bundles)
                        {
                            int existingIndex = newProperties.FindIndex(x => ReferenceEquals(x.Owner, owner) && string.Equals(x.Key, prop.Key, StringComparison.Ordinal));
                            if (existingIndex != -1 && !ReferenceEquals(newProperties[existingIndex].Type, HideInheritedPropertyType.Instance))
                            {
                                Log($"Parent bundle asset {prop.Owner.Type.GetTypeName()}.{prop.Key} hidden by a duplicate bundle asset present in {owner.Type.GetTypeName()}.");
                                continue;
                            }

                            SpecBundleAsset clone = (SpecBundleAsset)prop.Clone();
                            clone.Owner = owner;
                            clone.Parent = prop;
                            if (existingIndex == -1)
                            {
                                newProperties.Add(clone);
                            }
                            else
                            {
                                clone.IsHidden = true;
                                newProperties[existingIndex] = clone;
                            }
                        }

                        // ReSharper disable once CoVariantArrayConversion
                        return newProperties.ToArray();
                    }
                    else
                    {
                        List<SpecProperty> newProperties = new List<SpecProperty>(p0.Length + p1.Length);
                        newProperties.AddRange(p0);

                        foreach (SpecProperty prop in p1)
                        {
                            int existingIndex = newProperties.FindIndex(x => ReferenceEquals(x.Owner, owner) && string.Equals(x.Key, prop.Key, StringComparison.Ordinal));
                            if (existingIndex != -1 && !ReferenceEquals(newProperties[existingIndex].Type, HideInheritedPropertyType.Instance))
                            {
                                Log($"Parent property {prop.Owner.Type.GetTypeName()}.{prop.Key} hidden by a duplicate property present in {owner.Type.GetTypeName()}.");
                                continue;
                            }

                            SpecProperty clone = (SpecProperty)prop.Clone();
                            clone.Owner = owner;
                            clone.Parent = prop;
                            if (existingIndex == -1)
                            {
                                newProperties.Add(clone);
                            }
                            else
                            {
                                clone.IsHidden = true;
                                newProperties[existingIndex] = clone;
                            }
                        }

                        return newProperties.ToArray();
                    }
                }
            }, skipSelf: true);

            ForEachPropertyWhile(info, prop =>
            {
                if (!ReferenceEquals(prop.Type, HideInheritedPropertyType.Instance))
                    return true;

                Log(prop.Owner.Parent.IsNull
                    ? $"Property {prop.Owner.Type}.{prop.Key} hides a property but doesn't define a parent."
                    : $"There is not a property in a parent type that can be hidden by {prop.Owner.Type}.{prop.Key}."
                );

                return true;
            });
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

    private async Task DownloadStatusAsync(AssetInformation assetInfo, CancellationToken token = default)
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
            if (!UseInternet || string.IsNullOrEmpty(assetInfo.StatusJsonFallbackUrl))
            {
                Log("Unable to read Status.json from local install, internet disabled.");
                return;
            }

            try
            {
                using HttpClient client = new HttpClient();
                using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, assetInfo.StatusJsonFallbackUrl);
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

    private async Task DownloadPlayerDashboardInventoryLocalizationAsync(AssetInformation assetInfo, CancellationToken token = default)
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
            if (!UseInternet || string.IsNullOrEmpty(assetInfo.PlayerDashboardInventoryLocalizationFallbackUrl))
            {
                Log("Unable to read PlayerDashboardInventory.dat from local install, internet disabled.");
                return;
            }

            try
            {
                using HttpClient client = new HttpClient();
                using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, assetInfo.PlayerDashboardInventoryLocalizationFallbackUrl);
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

    private async Task DownloadSkillLocalization(AssetInformation assetInfo, CancellationToken token = default)
    {
        HttpClient? client = null;
        try
        {
            bool success = false;
            if (assetInfo.Skillsets != null)
            {
                if (UnturnedInstallDirectory.TryGetInstallDirectory(out GameInstallDir installDir))
                {
                    string localPath = installDir.GetFile(@"Localization\English\Menu\Survivors\MenuSurvivorsCharacter.dat");
                    if (File.Exists(localPath))
                    {
                        try
                        {
                            using FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                            await ReadSkillsetsFileAsync(fs, assetInfo).ConfigureAwait(false);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            Log("Error reading MenuSurvivorsCharacter.dat file from local install.");
                            Log(ex.ToString());
                        }
                    }
                }

                if (!success)
                {
                    if (!UseInternet || string.IsNullOrEmpty(assetInfo.SkillsetsLocalizationFallbackUrl))
                    {
                        Log("Unable to read MenuSurvivorsCharacter.dat from local install, internet disabled.");
                        return;
                    }

                    try
                    {
                        client = new HttpClient();
                        using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, assetInfo.SkillsetsLocalizationFallbackUrl);
                        using HttpResponseMessage response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);

                        response.EnsureSuccessStatusCode();

                        using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        await ReadSkillsetsFileAsync(stream, assetInfo).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Log("Unable to read MenuSurvivorsCharacter.dat from internet.");
                        Log(ex.ToString());
                    }
                }
            }

            success = false;
            if (assetInfo.Specialities != null)
            {
                if (UnturnedInstallDirectory.TryGetInstallDirectory(out GameInstallDir installDir))
                {
                    string localPath = installDir.GetFile(@"Localization\English\Player\PlayerDashboardSkills.dat");
                    if (File.Exists(localPath))
                    {
                        try
                        {
                            using FileStream fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                            await ReadSkillsFileAsync(fs, assetInfo).ConfigureAwait(false);
                            success = true;
                        }
                        catch (Exception ex)
                        {
                            Log("Error reading PlayerDashboardSkills.dat file from local install.");
                            Log(ex.ToString());
                        }
                    }
                }

                if (!success)
                {
                    if (!UseInternet || string.IsNullOrEmpty(assetInfo.SkillsLocalizationFallbackUrl))
                    {
                        Log("Unable to read PlayerDashboardSkills.dat from local install, internet disabled.");
                        return;
                    }

                    try
                    {
                        client ??= new HttpClient();
                        using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, assetInfo.SkillsLocalizationFallbackUrl);
                        using HttpResponseMessage response = await client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);

                        response.EnsureSuccessStatusCode();

                        using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        await ReadSkillsFileAsync(stream, assetInfo).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Log("Unable to read PlayerDashboardSkills.dat from internet.");
                        Log(ex.ToString());
                    }
                }
            }
        }
        finally
        {
            client?.Dispose();
        }

        return;

        static async Task ReadSkillsetsFileAsync(Stream stream, AssetInformation assetInfo)
        {
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
            while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
            {
                if (line.Length == 0 || line[0] == '#' || line[0] == '/')
                    continue;
                int space = line.IndexOf(' ');
                if (space <= 0 || space >= line.Length - 1)
                    continue;

                ReadOnlySpan<char> key = line.AsSpan(0, space);
                if (key.StartsWith("Skillset_".AsSpan(), StringComparison.Ordinal)
                    && key.Length > 9
                    && int.TryParse(key.Slice(9).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int skillsetIndex))
                {
                    SkillsetInfo? info = Array.Find(assetInfo.Skillsets!, x => x != null && x.Index == skillsetIndex);
                    string dn = line.AsSpan(space + 1).Trim().ToString();
                    if (info != null && !string.IsNullOrEmpty(dn))
                        info.DisplayName = dn;
                }
            }
        }
        static async Task ReadSkillsFileAsync(Stream stream, AssetInformation assetInfo)
        {
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
            List<string?> levels = new List<string?>(7);
            int lastSpec = -1, lastSkill = -1;
            while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
            {
                if (line.Length == 0 || line[0] == '#' || line[0] == '/')
                    continue;
                int space = line.IndexOf(' ');
                if (space <= 0 || space >= line.Length - 1)
                    continue;

                ReadOnlySpan<char> key = line.AsSpan(0, space);
                if (!key.StartsWith("Speciality_".AsSpan(), StringComparison.Ordinal) || key.Length <= 11)
                    continue;

                int digitEnd = 11;
                while (digitEnd < key.Length && char.IsDigit(key[digitEnd]))
                    ++digitEnd;

                if (digitEnd == 11 || !int.TryParse(key.Slice(11, digitEnd - 11).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int specialityIndex))
                    continue;

                SpecialityInfo? speciality = Array.Find(assetInfo.Specialities!, x => x != null && x.Index == specialityIndex);
                if (speciality == null)
                    continue;

                string value = line.AsSpan(space + 1).Trim().ToString();
                if (string.IsNullOrEmpty(value))
                    continue;

                ReadOnlySpan<char> extra = key.Slice(digitEnd);

                if (extra.Equals("_Tooltip".AsSpan(), StringComparison.Ordinal))
                {
                    speciality.DisplayName = value;
                }
                else if (!extra.StartsWith("_Skill_".AsSpan(), StringComparison.Ordinal) && extra.Length > 7 || speciality.Skills == null)
                    continue;
                
                digitEnd = 7;
                while (digitEnd < extra.Length && char.IsDigit(extra[digitEnd]))
                    ++digitEnd;

                if (digitEnd == 7 || !int.TryParse(extra.Slice(7, digitEnd - 7).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int skillIndex))
                    continue;

                SkillInfo? skill = Array.Find(speciality.Skills, x => x != null && x.Index == skillIndex);
                if (skill == null)
                    continue;

                extra = extra.Slice(digitEnd);
                if (extra.IsEmpty)
                {
                    skill.DisplayName = value;
                }
                else if (extra.Equals("_Tooltip".AsSpan(), StringComparison.Ordinal))
                {
                    skill.Description = value;
                }
                else if (extra.Equals("_Levels_V2".AsSpan(), StringComparison.Ordinal))
                {
                    CheckLevels();
                    skill.Levels = [ value ];
                }
                else if (!extra.StartsWith("_Level_".AsSpan(), StringComparison.Ordinal))
                {
                    continue;
                }

                digitEnd = 7;
                while (digitEnd < extra.Length && char.IsDigit(extra[digitEnd]))
                    ++digitEnd;

                if (digitEnd == 7 || !int.TryParse(extra.Slice(7, digitEnd - 7).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int levelIndex) || levelIndex < 1)
                    continue;
                --levelIndex;

                if (lastSkill != -1 && lastSkill != skillIndex || lastSpec != -1 && lastSpec != specialityIndex)
                {
                    CheckLevels();
                }

                lastSkill = skillIndex;
                lastSpec = specialityIndex;
                if (levels.Count <= levelIndex)
                {
                    for (int i = levels.Count; i < levelIndex; ++i)
                        levels.Add(null);
                    levels.Add(value);
                }
                else
                {
                    levels[levelIndex] = value;
                }
            }

            CheckLevels();
            return;

            void CheckLevels()
            {
                if (lastSkill != -1 && lastSpec != -1 && levels.Count > 0)
                {
                    SpecialityInfo? speciality = Array.Find(assetInfo.Specialities!, x => x != null && x.Index == lastSpec);
                    SkillInfo? skill = speciality?.Skills == null ? null : Array.Find(speciality.Skills, x => x != null && x.Index == lastSkill);
                    if (skill != null)
                    {
                        while (levels.Count > 0 && levels[levels.Count - 1] == null)
                            levels.RemoveAt(levels.Count - 1);
                        skill.Levels = levels.ToArray();
                        skill.MaximumLevel = skill.Levels.Length;
                    }
                }

                levels.Clear();
                lastSkill = -1;
                lastSpec = -1;
            }
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

    private class AssetSpecDatabaseWrapper : IAssetSpecDatabase
    {
        private readonly AssetSpecDatabase _db;

        public AssetSpecDatabaseWrapper(AssetSpecDatabase db, IReadOnlyDictionary<QualifiedType, AssetSpecType> types)
        {
            _db = db;
            Types = types;
        }

        public bool UseInternet
        {
            get => _db.UseInternet;
            set => _db.UseInternet = value;
        }

        public JsonSerializerOptions? Options
        {
            get => _db.Options;
            set => _db.Options = value;
        }

        public string[]? NPCAchievementIds => _db.NPCAchievementIds;
        public Version? CurrentGameVersion => _db.CurrentGameVersion;
        public JsonDocument? StatusInformation => _db.StatusInformation;
        public InstallDirUtility UnturnedInstallDirectory => _db.UnturnedInstallDirectory;
        public AssetInformation Information => _db.Information;
        public IReadOnlyList<string> ValidActionButtons => _db.ValidActionButtons;
        public IReadOnlyDictionary<QualifiedType, AssetSpecType> Types { get; }
        public Task InitializeAsync(CancellationToken token = default) => _db.InitializeAsync(token);
    }
}

public static class AssetSpecDatabaseExtensions
{
    public static ISpecType? FindType(this IAssetSpecDatabase db, string type, AssetFileType fileType)
    {
        type = QualifiedType.NormalizeType(type);
        if (AssetCategory.TypeOf.Type.Equals(type))
        {
            return AssetCategory.TypeOf;
        }

        if (Type.GetType(type, false, false) is { } specifiedType && typeof(ISpecType).IsAssignableFrom(specifiedType))
        {
            return (ISpecType)Activator.CreateInstance(specifiedType);
        }

        string? assetType = null;
        int divIndex = type.IndexOf("::", 0, StringComparison.Ordinal);
        if (divIndex >= 0 && divIndex < type.Length - 2)
        {
            if (divIndex != 0)
                assetType = type.Substring(0, divIndex);
            type = type.Substring(divIndex + 2);
        }

        if (assetType == null)
        {
            foreach (ISpecType t in fileType.Information.Types)
            {
                if (!t.Type.Equals(type))
                    continue;

                return t;
            }
        }

        InverseTypeHierarchy hierarchy = db.Information.GetParentTypes(assetType != null ? new QualifiedType(assetType) : fileType.Type);

        for (int i = 0; i < hierarchy.ParentTypes.Length; ++i)
        {
            QualifiedType qt = hierarchy.ParentTypes[hierarchy.ParentTypes.Length - i - 1];

            if (!db.Types.TryGetValue(qt, out AssetSpecType info))
            {
                continue;
            }

            ISpecType? t = Array.Find(info.Types, p => p.Type.Equals(type));

            if (t != null)
            {
                return t;
            }
        }

        return null;
    }

    public static SpecProperty? FindPropertyInfo(this IAssetSpecDatabase db, string property, AssetFileType fileType, SpecPropertyContext context = SpecPropertyContext.Property)
    {
        if (context is not SpecPropertyContext.Localization and not SpecPropertyContext.Property and not SpecPropertyContext.BundleAsset)
            throw new ArgumentOutOfRangeException(nameof(context));

        if (!fileType.IsValid)
        {
            return null;
        }

        string? assetType = null;
        bool isLocal = false, isProp = false, isBundleAsset = false;
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
                    isBundleAsset = false;
                }
                else if (assetType.Equals("$prop$", StringComparison.OrdinalIgnoreCase))
                {
                    assetType = null;
                    isProp = true;
                    isLocal = false;
                    isBundleAsset = false;
                }
                else if (assetType.Equals("$bndl$", StringComparison.OrdinalIgnoreCase))
                {
                    assetType = null;
                    isProp = false;
                    isLocal = false;
                    isBundleAsset = true;
                }
            }
            property = property.Substring(divIndex + 2);
        }

        InverseTypeHierarchy hierarchy = db.Information.GetParentTypes(assetType != null ? new QualifiedType(assetType) : fileType.Type);

        for (int i = -1; i < hierarchy.ParentTypes.Length; ++i)
        {
            QualifiedType type = i < 0 ? hierarchy.Type : hierarchy.ParentTypes[hierarchy.ParentTypes.Length - i - 1];

            if (!db.Types.TryGetValue(type, out AssetSpecType info))
            {
                continue;
            }

            if (isBundleAsset || context == SpecPropertyContext.BundleAsset)
            {
                SpecBundleAsset? bundleAsset = Array.Find(info.BundleAssets, p => p.Key.Equals(property, StringComparison.OrdinalIgnoreCase));
                if (bundleAsset != null)
                {
                    return bundleAsset;
                }
            }

            SpecProperty[] props = isLocal || context == SpecPropertyContext.Localization && !isProp ? info.LocalizationProperties : info.Properties;
            SpecProperty? prop = Array.Find(props, p => p.Key.Equals(property, StringComparison.OrdinalIgnoreCase));
            prop ??= Array.Find(props, p => p.Aliases.Any(x => string.Equals(x.Value, property, StringComparison.OrdinalIgnoreCase)));

            if (prop != null)
            {
                return prop;
            }
        }

        if (!isLocal && context == SpecPropertyContext.Property)
        {
            return db.FindPropertyInfo("$prop$::" + property, fileType, SpecPropertyContext.Localization);
        }

        if (!isProp && context == SpecPropertyContext.Localization)
        {
            return db.FindPropertyInfo("$local$::" + property, fileType, SpecPropertyContext.Property);
        }

        if (!isBundleAsset && context == SpecPropertyContext.BundleAsset)
        {
            return db.FindPropertyInfo("$bndl$::" + property, fileType, SpecPropertyContext.Property);
        }

        return null;
    }
}
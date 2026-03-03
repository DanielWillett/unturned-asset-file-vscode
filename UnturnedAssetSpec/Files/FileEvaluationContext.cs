using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public struct FileEvaluationContext
{
    internal static FileEvaluationContext None = default;

    private AssetDatPropertyPosition _rootPosition;
    private PropertyBreadcrumbs _rootBreadcrumbs;
    private IDictionarySourceNode? _targetDictionary;
    private DatTypeWithProperties? _targetDictionaryType;
    private IDictionarySourceNode? _targetRoot;

    internal IPropertySourceNode? CachedPropertyNode;
    internal DatProperty? CachedProperty;

    public readonly IParsingServices Services;
    public readonly ISourceFile File;
    public readonly AssetFileType FileType;
    public readonly SpecPropertyContext PropertyContext;

    private bool _hasDictCache;
    private bool _hasAssetDict;
    private bool _hasMetadataDict;

    public PropertyBreadcrumbs RootBreadcrumbs
    {
        readonly get => _rootBreadcrumbs;
        set
        {
            _targetDictionary = null;
            _targetDictionaryType = null;
            _rootBreadcrumbs = value;
            _targetDictionary = null;
        }
    }

    public AssetDatPropertyPosition RootPosition
    {
        readonly get => _rootPosition;
        set
        {
            _targetRoot = null;
            _targetDictionary = null;
            _targetDictionaryType = null;
            _rootPosition = value;
            _targetDictionaryType = null;
            _targetDictionary = null;
            _targetRoot = null;
        }
    }

    /// <summary>
    /// Whether or not <see cref="File"/> has an "Asset" dictionary.
    /// </summary>
    public bool FileHasAssetDictionary
    {
        get
        {
            if (!_hasDictCache)
                CacheAssetDictionaries();

            return _hasAssetDict;
        }
    }
    /// <summary>
    /// Whether or not <see cref="File"/> has a "Metadata" dictionary.
    /// </summary>

    public bool FileHasMetadataDictionary
    {
        get
        {
            if (!_hasDictCache)
                CacheAssetDictionaries();

            return _hasMetadataDict;
        }
    }

    public FileEvaluationContext(IParsingServices services, ISourceFile sourceFile, AssetDatPropertyPosition root = AssetDatPropertyPosition.Root)
    {
        Services = services;
        File = sourceFile;
        _rootPosition = root;
        _rootBreadcrumbs = PropertyBreadcrumbs.Root;

        if (sourceFile == null)
            return;

        FileType = AssetFileType.FromFile(sourceFile, services.Database);
        PropertyContext = sourceFile.GetPropertyContext();
    }

    public void CreateSubContext([UnscopedRef] out FileEvaluationContext ctx, DatProperty property, int index = -1, PropertyResolutionContext context = PropertyResolutionContext.Modern)
    {
        CreateSubContextIntl(out ctx, index, property, context, static (bc, property, index, context) => bc.Combine(property, index, context));
    }

    public void CreateRootContext([UnscopedRef] out FileEvaluationContext ctx)
    {
        if (_rootBreadcrumbs.IsRoot)
        {
            ctx = this;
            return;
        }

        CreateSubContextIntl<DatProperty?>(out ctx, 0, null, PropertyResolutionContext.Unknown, static (_, _, _, _) => PropertyBreadcrumbs.Root);
    }

    public void CreateSubContext([UnscopedRef] out FileEvaluationContext ctx, string key, int index = -1, PropertyResolutionContext context = PropertyResolutionContext.Modern)
    {
        CreateSubContextIntl(out ctx, index, key, context, static (bc, key, index, context) => bc.Combine(key, index, context));
    }

    public void CreateSubContext([UnscopedRef] out FileEvaluationContext ctx, int index, PropertyResolutionContext context = PropertyResolutionContext.Modern)
    {
        CreateSubContextIntl<DatProperty?>(out ctx, index, null, context, static (bc, _, index, context) => bc.Combine(index, context));
    }

    public void CreateSubContext([UnscopedRef] out FileEvaluationContext ctx, ISourceNode node, PropertyResolutionContext context = PropertyResolutionContext.Modern)
    {
        CreateSubContextIntl(out ctx, 0, node, context, static (bc, node, _, context) => bc.Combine(node, context));
    }

    private void CreateSubContextIntl<TState>(
        [UnscopedRef] out FileEvaluationContext ctx,
        int index,
        TState state,
        PropertyResolutionContext context,
        Func<PropertyBreadcrumbs, TState, int, PropertyResolutionContext, PropertyBreadcrumbs> transformer
    )
    {
        ctx = new FileEvaluationContext(Services, File, RootPosition)
        {
            _targetRoot = _targetRoot,
            _rootBreadcrumbs = transformer(_rootBreadcrumbs, state, index, context),
            _hasAssetDict = _hasAssetDict,
            _hasMetadataDict = _hasMetadataDict,
            _hasDictCache = _hasDictCache
        };
    }

    /// <summary>
    /// Gets the key filter in effect for the current breadcrumbs.
    /// </summary>
    public LegacyExpansionFilter GetKeyFilter()
    {
        return _rootBreadcrumbs.Length == 0 ? LegacyExpansionFilter.Either : _rootBreadcrumbs[^1].Context.ToKeyFilter();
    }

    /// <summary>
    /// Gets the root dictionary being targeted for this evaluation context based on <see cref="RootPosition"/>.
    /// This will always be one of the root dictionaries (the root, <c>Asset</c>, or <c>Metadata</c>).
    /// </summary>
    public bool TryGetTargetRoot([NotNullWhen(true)] out IDictionarySourceNode? targetRoot)
    {
        IDictionarySourceNode? cachedTargetRoot = _targetRoot;
        if (cachedTargetRoot != null)
        {
            targetRoot = cachedTargetRoot;
            return true;
        }

        IDictionarySourceNode rootDictionary = File;
        if (RootPosition != AssetDatPropertyPosition.Root)
        {
            if (rootDictionary is not IAssetSourceFile assetFile)
            {
                targetRoot = null;
                return false;
            }

            switch (RootPosition)
            {
                case AssetDatPropertyPosition.Asset:
                    IDictionarySourceNode? assetData = assetFile.GetAssetDataDictionary();
                    if (assetData != null)
                        rootDictionary = assetData;
                    else goto default;
                    break;

                case AssetDatPropertyPosition.Metadata:
                    IDictionarySourceNode? metadata = assetFile.GetMetadataDictionary();
                    if (metadata != null)
                        rootDictionary = metadata;
                    else goto default;
                    break;

                default:
                    targetRoot = null;
                    return false;
            }
        }

        targetRoot = rootDictionary;
        _targetRoot = rootDictionary;
        return true;
    }

    /// <summary>
    /// Gets the dictionary being targetd for this evaluation context.
    /// Usually this will be one of the root dictionaries (the root, <c>Asset</c>, or <c>Metadata</c>).
    /// </summary>
    public bool TryGetTargetDictionary(
        [NotNullWhen(true)] out IDictionarySourceNode? target,
        [NotNullWhen(true)] out DatTypeWithProperties? dictionaryType
    )
    {
        IDictionarySourceNode? cachedTargetDictionary = _targetDictionary;
        DatTypeWithProperties? objectType = _targetDictionaryType;
        if (cachedTargetDictionary != null && objectType != null && _targetDictionary == cachedTargetDictionary)
        {
            target = cachedTargetDictionary;
            dictionaryType = objectType;
            return true;
        }

        if (!TryGetTargetRoot(out IDictionarySourceNode? rootDictionary))
        {
            target = null;
            dictionaryType = null;
            return false;
        }

        if (RootBreadcrumbs.IsRoot)
        {
            target = rootDictionary;
            dictionaryType = FileType.Information;
            return true;
        }

        if (!RootBreadcrumbs.TryTraceRelativeTo(
                rootDictionary,
                FileType.Information,
                out IAnyValueSourceNode? targetedValue,
                out IType? valueType,
                ref this
            )
            || targetedValue is not IDictionarySourceNode dictionary 
            || valueType is not DatTypeWithProperties propsType)
        {
            target = null;
            dictionaryType = null;
            return false;
        }

        _targetDictionary = dictionary;
        _targetDictionaryType = propsType;
        target = dictionary;
        dictionaryType = propsType;
        return true;
    }

    private void CacheAssetDictionaries()
    {
        if (File is IAssetSourceFile a)
        {
            _hasAssetDict = a.GetAssetDataDictionary() != null;
            _hasMetadataDict = a.GetMetadataDictionary() != null;
        }

        _hasDictCache = true;
    }

    /// <summary>
    /// Gets the target node to read the given property.
    /// </summary>
    public bool TryGetTargetPropertyNodeForProperty(
        DatProperty property,
        [NotNullWhen(true)] out IPropertySourceNode? propertyNode
    )
    {
        if (CachedPropertyNode != null && CachedProperty == property)
        {
            propertyNode = CachedPropertyNode;
            return true;
        }

        if (!_hasDictCache)
        {
            CacheAssetDictionaries();
        }

        if (!property.AssetPosition.IsValidPosition(RootPosition, _hasAssetDict, _hasMetadataDict))
        {
            propertyNode = null;
            return false;
        }

        if (!TryGetTargetDictionary(out IDictionarySourceNode? dictionary, out _))
        {
            propertyNode = null;
            return false;
        }

        return dictionary.TryGetProperty(property, ref this, out propertyNode/*, todo: filter */);
    }

    public readonly IFileRelationalModel GetRelationalModel(SpecPropertyContext context = SpecPropertyContext.Property)
    {
        return Services.RelationalModelProvider.GetProvider(File, context);
    }

    public readonly bool TryGetRelevantMap([NotNullWhen(true)] out RelevantMapInfo? mapInfo)
    {
        // todo
        mapInfo = null;
        return false;
    }
}

public class RelevantMapInfo;
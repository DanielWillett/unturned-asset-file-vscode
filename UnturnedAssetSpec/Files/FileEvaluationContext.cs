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
            _targetRoot = null;
            _targetDictionary = null;
        }
    }

    public FileEvaluationContext(IParsingServices services, ISourceFile sourceFile, AssetDatPropertyPosition root = AssetDatPropertyPosition.Root)
    {
        Services = services;
        File = sourceFile;
        FileType = sourceFile == null ? default : AssetFileType.FromFile(sourceFile, services.Database);
        _rootPosition = root;
        _rootBreadcrumbs = PropertyBreadcrumbs.Root;
    }

    /// <summary>
    /// Gets the root dictionary being targeted for this evaluation context based on <see cref="RootPosition"/>.
    /// This will always be one of the root dictionaries (the root, <c>Asset</c>, or <c>Metadata</c>).
    /// </summary>
    public readonly bool TryGetTargetRoot([NotNullWhen(true)] out IDictionarySourceNode? targetRoot)
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
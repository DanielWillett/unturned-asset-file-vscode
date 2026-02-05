using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

#if false
internal class GetAssetPropertyAddLocationHandler : IGetAssetPropertyAddLocationHandler
{
    private static readonly GetAssetPropertyAddLocationResponse Invalid = new GetAssetPropertyAddLocationResponse
    {
        IsFlag = false,
        Position = null,
        InsertLines = 0
    };

    private readonly OpenedFileTracker _fileTracker;
    private readonly IAssetSpecDatabase _spec;

    public GetAssetPropertyAddLocationHandler(OpenedFileTracker fileTracker, IAssetSpecDatabase spec)
    {
        _fileTracker = fileTracker;
        _spec = spec;
    }

    public Task<GetAssetPropertyAddLocationResponse> Handle(GetAssetPropertyAddLocationParams request, CancellationToken cancellationToken)
    {
        if (!_fileTracker.Files.TryGetValue(request.Document, out OpenedFile? file))
        {
            return Task.FromResult(Invalid);
        }

        ISourceFile sourceFile = file.SourceFile;

        AssetFileType fileType = AssetFileType.FromFile(sourceFile, _spec);
        if (!fileType.IsValid)
        {
            return Task.FromResult(Invalid);
        }

        DatProperty? property = fileType.Information.Properties.FirstOrDefault(x => string.Equals(x.Key, request.Key, StringComparison.Ordinal));
        if (property == null || property.HideOverridden)
        {
            return Task.FromResult(Invalid);
        }

        IDictionarySourceNode asset;
        if (sourceFile is IAssetSourceFile assetSourceFile)
        {
            asset = property.CanBeInMetadata
                ? assetSourceFile.GetMetadataDictionary() ?? assetSourceFile.AssetData
                : assetSourceFile.AssetData;
        }
        else
        {
            asset = sourceFile;
        }
        
        List<DatProperty> properties = fileType.Information.Properties
            .Where(x => ReferenceEquals(x.Deprecated, Value.False))
            .OrderBy(x => x.Key)
            .ToList();

        int lines;
        Position pos;
        if (asset.Children.Length == 0)
        {
            lines = asset.IsRootNode ? 2 : 0;
            pos = asset.IsRootNode
                ? new Position(asset.Range.End.Line - 1, 0)
                : new Position(asset.Range.End.Line - 1, asset.Range.End.Character - 1);
        }
        else
        {
            int index = properties.FindIndex(x => x.Key.Equals(request.Key));

            (DatProperty? property, IPropertySourceNode? node) after = default;
            if (index != -1)
            {
                // todo after = properties
                // todo     .Take(index)
                // todo     .Reverse()
                // todo     .Select(x => file.SourceFile.TryGetProperty(x, out IPropertySourceNode? node, PropertyResolutionContext.Modern) ? (property, node) : (null, null))
                // todo     .FirstOrDefault(x => x.property != null);
            }

            (DatProperty? afterProperty, IPropertySourceNode? afterNode) = after;
            
            if (afterNode != null)
            {
                bool shouldInsertLine = true;
                pos = new Position(
                    afterNode.Range.End.Line + (shouldInsertLine ? 1 : 2) - 1,
                    0
                );

                lines = shouldInsertLine ? 2 : 1;
            }
            else
            {
                lines = asset.IsRootNode ? 2 : 0;
                pos = asset.IsRootNode
                    ? new Position(asset.Range.End.Line - 1, 0)
                    : new Position(asset.Range.End.Line - 2, asset.Range.End.Character - 1);
            }
        }

        return Task.FromResult(new GetAssetPropertyAddLocationResponse
        {
            Position = pos,
            InsertLines = lines,
            IsFlag = property.Type.Equals(FlagType.Instance)
        });
    }
}
#endif
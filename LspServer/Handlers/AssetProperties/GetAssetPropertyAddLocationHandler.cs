using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

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

        AssetFileType fileType = AssetFileType.FromFile(file.File, _spec);
        if (!fileType.IsValid)
        {
            return Task.FromResult(Invalid);
        }

        SpecProperty? property = fileType.Information.Properties.FirstOrDefault(x => string.Equals(x.Key, request.Key, StringComparison.Ordinal));
        if (property == null || property.IsHidden)
        {
            return Task.FromResult(Invalid);
        }

        AssetFileDictionaryValueNode asset = file.File.Metadata != null && property.CanBeInMetadata
            ? file.File.Metadata
            : file.File.Asset;

        List<SpecProperty> properties = fileType.Information.Properties
            .Where(x => ReferenceEquals(x.Deprecated, SpecDynamicValue.False))
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.Key)
            .ToList();

        int lines;
        Position pos;
        if (asset.Pairs.Count == 0)
        {
            lines = asset.IsRoot ? 2 : 0;
            pos = asset.IsRoot
                ? new Position(asset.Range.End.Line - 1, 0)
                : new Position(asset.Range.End.Line - 1, asset.Range.End.Character - 1);
        }
        else
        {
            int index = properties.FindIndex(x => x.Key.Equals(request.Key));

            (SpecProperty? property, AssetFileKeyValuePairNode? node) after = default;
            if (index != -1)
            {
                after = properties
                    .Take(index)
                    .Reverse()
                    .Select(x => file.File.TryGetProperty(x, out AssetFileKeyValuePairNode node) ? (property, node) : (null, null))
                    .FirstOrDefault(x => x.property != null);
            }

            (SpecProperty? afterProperty, AssetFileKeyValuePairNode? afterNode) = after;
            
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
                lines = asset.IsRoot ? 2 : 0;
                pos = asset.IsRoot
                    ? new Position(asset.Range.End.Line - 1, 0)
                    : new Position(asset.Range.End.Line - 2, asset.Range.End.Character - 1);
            }
        }

        return Task.FromResult(new GetAssetPropertyAddLocationResponse
        {
            Position = pos,
            InsertLines = lines,
            IsFlag = property.Type.Equals(KnownTypes.Flag)
        });
    }
}
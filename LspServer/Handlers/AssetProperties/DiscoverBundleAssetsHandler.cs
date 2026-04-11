using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Immutable;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

internal class DiscoverBundleAssetsHandler : IDiscoverBundleAssetsHandler
{
    private static readonly Container<BundleAssetInfo> Empty = new Container<BundleAssetInfo>(Array.Empty<BundleAssetInfo>());

    private readonly OpenedFileTracker _fileTracker;
    private readonly IParsingServices _parsingServices;

    public DiscoverBundleAssetsHandler(OpenedFileTracker fileTracker, IParsingServices parsingServices)
    {
        _fileTracker = fileTracker;
        _parsingServices = parsingServices;
    }

    public Task<Container<BundleAssetInfo>> Handle(DiscoverBundleAssetsParams request, CancellationToken cancellationToken)
    {
        if (!_fileTracker.Files.TryGetValue(request.Document, out OpenedFile? file))
        {
            return Task.FromResult(Empty);
        }

        ISourceFile sourceFile = file.SourceFile;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, sourceFile);
        DatFileType fileType = ctx.FileType.Information;

        if (fileType is not DatAssetFileType { HasBundleAssets: true } assetType)
        {
            return Task.FromResult(Empty);
        }

        List<BundleAssetInfo> outputProperties = new List<BundleAssetInfo>(8);

        for (DatFileType? ft = assetType; ft != null; ft = ft.Parent)
        {
            if (ft is not IDatTypeWithBundleAssets type)
                continue;

            ImmutableArray<DatBundleAsset> bundleAssets = type.BundleAssets;
            for (int i = 0; i < bundleAssets.Length; i++)
            {
                DatBundleAsset unityAsset = bundleAssets[i];
                BundleAssetInfo prop = new BundleAssetInfo
                {
                    Key = unityAsset.Key,
                    Type = unityAsset.Type.TypeName.Type,
                    TypeName = unityAsset.Type.TypeName.GetTypeName()
                };

                if (unityAsset.Description != null && unityAsset.Description.TryEvaluateValue(out Optional<string> desc, ref ctx))
                {
                    prop.Description = desc.Value;
                }

                if (unityAsset.MarkdownDescription != null && unityAsset.MarkdownDescription.TryEvaluateValue(out desc, ref ctx))
                {
                    prop.Markdown = desc.Value;
                }

                IBundleProxy bundle = sourceFile.WorkspaceFile.Bundle;
                UnityObject? obj = bundle.GetCorrespondingAsset(unityAsset.Key, unityAsset.Type, ref ctx);
                if (obj == null)
                {
                    prop.Path = string.Empty;
                }
                else
                {
                    prop.Path = obj.Path;
                    string? prefix = bundle.Bundle?.Prefix;
                    if (!string.IsNullOrEmpty(prefix) && prop.Path.StartsWith(prefix))
                    {
                        prop.Path = prop.Path[prefix.Length..];
                    }
                }

                outputProperties.Add(prop);
            }
        }

        return Task.FromResult(new Container<BundleAssetInfo>(outputProperties));
    }
}
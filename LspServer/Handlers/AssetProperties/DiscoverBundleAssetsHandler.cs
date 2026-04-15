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
using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

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

        IBundleProxy bundle = sourceFile.WorkspaceFile.Bundle;
        string? prefix = bundle.Bundle?.Prefix;

        for (DatFileType? ft = assetType; ft != null; ft = ft.Parent)
        {
            if (ft is not IDatTypeWithBundleAssets type)
                continue;

            ImmutableArray<DatBundleAsset> bundleAssets = type.BundleAssets;
            for (int i = 0; i < bundleAssets.Length; i++)
            {
                DatBundleAsset unityAsset = bundleAssets[i];


                if (!string.IsNullOrEmpty(request.Key))
                {
                    if (!unityAsset.MatchesKey(request.Key, ref ctx, out _))
                    {
                        continue;
                    }

                    ResolveChildObjects(unityAsset, request.Path, outputProperties, ref ctx);
                    goto rtn;
                }


                BundleAssetInfo prop = new BundleAssetInfo
                {
                    Key = unityAsset.Key,
                    Type = unityAsset.Type.TypeName.Type,
                    TypeName = unityAsset.Type.TypeName.GetTypeName(),
                    IsAsset = true
                };

                if (unityAsset.Description?.TryEvaluateValue(out Optional<string> desc, ref ctx) is true)
                {
                    prop.Description = desc.Value;
                }

                if (unityAsset.MarkdownDescription?.TryEvaluateValue(out desc, ref ctx) is true)
                {
                    prop.Markdown = desc.Value;
                }

                if (unityAsset.Required?.TryEvaluateValue(out Optional<bool> isRequired, ref ctx) is true)
                {
                    prop.IsRequired = isRequired.Value;
                }

                UnityObject? obj = bundle.GetCorrespondingAsset(unityAsset.Key, unityAsset.Type, ref ctx);
                if (obj != null)
                {
                    prop.Path = obj.Path;
                    prop.HasChildren = obj.Transform?.ChildCount > 0;
                    if (!string.IsNullOrEmpty(prefix) && prop.Path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        prop.Path = prop.Path[prefix.Length..];
                    }
                }

                outputProperties.Add(prop);
            }
        }

        using (BundleProxyEnumerator bundleEnumerator = bundle.EnumerateAssets(_parsingServices))
        {
            while (bundleEnumerator.MoveNext())
            {
                UnityObject obj = bundleEnumerator.Current;
                if (!string.IsNullOrEmpty(request.Key))
                {
                    if (!string.Equals(obj.Name, request.Key, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    ResolveChildObjects(obj, request.Path, outputProperties, ref ctx);
                    goto rtn;
                }

                ReadOnlySpan<char> path = obj.Path;
                if (!string.IsNullOrEmpty(prefix) && path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    path = path[prefix.Length..];
                }

                bool alreadyExists = false;
                foreach (BundleAssetInfo bundleInfo in outputProperties)
                {
                    if (path.Equals(bundleInfo.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (alreadyExists)
                    continue;

                string type, typeName;
                if (obj.TryGetBundleAssetType(out IBundleAssetType? bundleAssetType))
                {
                    QualifiedType qualifiedType = bundleAssetType.TypeName;
                    type = qualifiedType.Type;
                    typeName = qualifiedType.GetTypeName();
                }
                else
                {
                    type = typeName = obj.ObjectType.ToString();
                }

                UnityTransform? transform = obj.Transform;
                outputProperties.Add(new BundleAssetInfo
                {
                    Key = obj.Name ?? Path.GetFileName(obj.Path),
                    Type = type,
                    TypeName = typeName,
                    HasChildren = transform is { ChildCount: > 0 },
                    Path = path.ToString(),
                    IsUnknown = true,
                    IsAsset = true
                });
            }
        }

        // put included properties at the top
        outputProperties.Sort((a, b) =>
        {
            if (b.Path == null)
            {
                return a.Path == null ? 0 : -1;
            }
            if (a.Path == null)
            {
                return 1;
            }

            ReadOnlySpan<char> aName = OSPathHelper.GetFileName(a.Path);
            ReadOnlySpan<char> bName = OSPathHelper.GetFileName(b.Path);
            return aName.CompareTo(bName, StringComparison.OrdinalIgnoreCase);
        });

        rtn:
        return Task.FromResult(new Container<BundleAssetInfo>(outputProperties));
    }

    private static void ResolveChildObjects(DatBundleAsset unityAsset, string? requestPath, List<BundleAssetInfo> outputProperties, ref FileEvaluationContext ctx)
    {
        IBundleProxy bundle = ctx.File.WorkspaceFile.Bundle;
        UnityObject? obj = bundle.GetCorrespondingAsset(unityAsset.Key, unityAsset.Type, ref ctx);
        if (obj == null)
        {
            return;
        }

        ResolveChildObjects(obj, requestPath, outputProperties, ref ctx);
    }

    private static void ResolveChildObjects(UnityObject obj, string? requestPath, List<BundleAssetInfo> outputProperties, ref FileEvaluationContext ctx)
    {
        UnityTransform? parent = obj.Transform;
        if (parent == null)
        {
            // not a prefab
            return;
        }

        if (!string.IsNullOrEmpty(requestPath))
        {
            parent = parent.Find(requestPath);
            if (parent == null)
            {
                return;
            }
        }

        int index = 0;
        foreach (UnityTransform childObject in parent)
        {
            string? name = childObject.Name;
            outputProperties.Add(new BundleAssetInfo
            {
                Key = name ?? $"<child #{index}>",
                Type = QualifiedType.ObjectType.Type,
                TypeName = "Object",
                HasChildren = childObject.ChildCount > 0,
                IsComponent = false,
                Path = name != null ? OSPathHelper.CombineWithUnixSeparators(requestPath, name) : null
            });
            ++index;
        }
    }
}
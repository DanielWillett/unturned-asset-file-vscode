using DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Visits all valid properties in order in an asset file, associating them with their resolved property definitions.
/// </summary>
/// <remarks>Inheriting classes should override <see cref="AcceptResolvedProperty"/>.</remarks>
public abstract class ResolvedPropertyNodeVisitor : OrderedNodeVisitor
{
    private readonly IFileRelationalModelProvider _modelProvider;
    private readonly IParsingServices _parsingServices;
    private readonly PropertyInclusionFlags _flags;
    private readonly FileRange? _range;
    private readonly HashSet<IPropertySourceNode> _ignoreProperties;

    protected override bool IgnoreMetadata => true;

    protected ResolvedPropertyNodeVisitor(
        IFileRelationalModelProvider modelProvider,
        IParsingServices parsingServices,
        FileRange? range = null,
        PropertyInclusionFlags flags = PropertyInclusionFlags.All | PropertyInclusionFlags.ResolvedOnly)
    {
        _modelProvider = modelProvider;
        _parsingServices = parsingServices;
        _flags = flags;
        _range = range;
        _ignoreProperties = new HashSet<IPropertySourceNode>();
    }

    public static void VisitFile<T>(ISourceFile sourceFile, ref T visitor) where T : ResolvedPropertyNodeVisitor
    {
        if (sourceFile is IAssetSourceFile asset)
        {
            IDictionarySourceNode? assetData = asset.GetAssetDataDictionary(),
                                   metadata = asset.GetMetadataDictionary();
            if (metadata != null || assetData != null)
            {
                metadata?.Visit(ref visitor);
                if (assetData == null)
                {
                    asset.Visit(ref visitor);
                }
                else
                {
                    assetData.Visit(ref visitor);
                    if ((visitor._flags & PropertyInclusionFlags.ResolvedOnly) != 0)
                    {
                        return;
                    }

                    foreach (IPropertySourceNode property in asset.Properties)
                    {
                        if (ReferenceEquals(property, metadata?.Parent) || ReferenceEquals(property, assetData.Parent))
                            continue;

                        visitor.AcceptUnresolvedProperty(property, PropertyBreadcrumbs.Root);
                    }
                }

                return;
            }
        }

        sourceFile.Visit(ref visitor);
    }

    protected virtual void AcceptResolvedProperty(
        DatProperty property,
        IType propertyType,
        ref FileEvaluationContext ctx,
        IPropertySourceNode node) { }

    protected virtual void AcceptUnresolvedProperty(
        IPropertySourceNode node,
        in PropertyBreadcrumbs breadcrumbs) { }

    protected override void AcceptProperty(IPropertySourceNode node)
    {
        if (!node.IsIncluded(_flags))
            return;
        
        if (_range.HasValue)
        {
            FileRange valueRange = node.GetValueRange();
            FileRange keyRange = node.Range;
            FileRange propertyRange = valueRange.End.Character == 0
                ? keyRange
                : new FileRange(keyRange.Start, valueRange.End);

            if (!_range.Value.Overlaps(propertyRange))
            {
                return;
            }
        }

        if (node is { File: IAssetSourceFile, ValueKind: SourceValueType.Dictionary }
            && ReferenceEquals(node.Parent, node.File)
            && (node.Key.Equals("Asset", StringComparison.OrdinalIgnoreCase) || node.Key.Equals("Metadata", StringComparison.OrdinalIgnoreCase))
            )
        {
            // Metadata { } or Asset { } properties
            return;
        }

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.FromNode(node);

        IFileRelationalModel model = _modelProvider.GetProvider(node.File, node.File.GetPropertyContext());
        if (breadcrumbs.IsRoot)
        {
            if (!model.TryGetPropertyInfoFromNode(node, out PropertyNodeRelationalInfo info) || info.ValueType == null)
            {
                if ((_flags & PropertyInclusionFlags.ResolvedOnly) == 0 && !_ignoreProperties.Contains(node))
                {
                    AcceptUnresolvedProperty(node, in breadcrumbs);
                }

                return;
            }

            foreach (IPropertySourceNode n in info.RelatedProperties)
            {
                _ignoreProperties.Add(n);
            }

            if ((_flags & PropertyInclusionFlags.UnresolvedOnly) != 0)
                return;

            FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, node.File, node.GetRootPosition())
            {
                RootBreadcrumbs = breadcrumbs
            };

            AcceptResolvedProperty(info.Property, info.ValueType, ref ctx, node);
        }
        else
        {
            if (!model.TryGetPropertyFromNode(node, out DatProperty? property, valueOnly: true))
            {
                if ((_flags & PropertyInclusionFlags.ResolvedOnly) == 0 && !_ignoreProperties.Contains(node))
                {
                    AcceptUnresolvedProperty(node, in breadcrumbs);
                }
                return;
            }

            FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, node.File, node.GetRootPosition())
            {
                RootBreadcrumbs = breadcrumbs
            };

            if ((_flags & PropertyInclusionFlags.UnresolvedOnly) != 0)
                return;

            if (!property.Type.TryEvaluateType(out IType? type, ref ctx))
                return;

            AcceptResolvedProperty(property, type, ref ctx, node);
        }
    }
}

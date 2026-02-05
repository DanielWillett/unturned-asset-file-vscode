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
    private AssetFileType _fileType;
    private bool _hasFileType;
    private HashSet<(PropertyBreadcrumbs, DatProperty)>? _visitedMultiProperties;
    private readonly FileRange? _range;

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
    }

    public static void VisitFile<T>(ISourceFile sourceFile, ref T visitor) where T : ResolvedPropertyNodeVisitor
    {
        if (sourceFile is IAssetSourceFile asset)
        {
            IDictionarySourceNode? metadata = asset.GetMetadataDictionary();
            if (metadata != null)
            {
                IDictionarySourceNode assetData = asset.AssetData;
                metadata.Visit(ref visitor);
                assetData.Visit(ref visitor);
                if ((visitor._flags & PropertyInclusionFlags.ResolvedOnly) != 0)
                {
                    return;
                }

                foreach (IPropertySourceNode property in asset.Properties)
                {
                    if (ReferenceEquals(property, metadata.Parent) || ReferenceEquals(property, assetData.Parent))
                        continue;

                    visitor.AcceptUnresolvedProperty(property, PropertyBreadcrumbs.Root);
                }

                return;
            }
        }

        sourceFile.Visit(ref visitor);
    }

    protected virtual void AcceptResolvedProperty(
        DatProperty property,
        IType propertyType,
        in FileEvaluationContext ctx,
        IPropertySourceNode node,
        in PropertyBreadcrumbs breadcrumbs) { }

    protected virtual void AcceptUnresolvedProperty(
        IPropertySourceNode node,
        in PropertyBreadcrumbs breadcrumbs) { }

    protected override void AcceptProperty(IPropertySourceNode node)
    {
        if (!node.IsIncluded(_flags))
            return;
        
        if (node is { File: IAssetSourceFile, ValueKind: SourceValueType.Dictionary }
            && ReferenceEquals(node.Parent, node.File)
            && (node.Key.Equals("Asset", StringComparison.OrdinalIgnoreCase) || node.Key.Equals("Metadata", StringComparison.OrdinalIgnoreCase))
            )
        {
            // Metadata { } or Asset { } properties
            return;
        }

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

        if (!_hasFileType)
        {
            _fileType = AssetFileType.FromFile(node.File, _parsingServices.Database);
            _hasFileType = true;
        }

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.FromNode(node);

        DatProperty? property = null;// todo: _modelProvider.GetProperty(node, in _fileType, in breadcrumbs, out PropertyResolutionContext context);
        if (property == null)
        {
            if ((_flags & PropertyInclusionFlags.ResolvedOnly) == 0)
            {
                AcceptUnresolvedProperty(node, in breadcrumbs);
            }
            return;
        }

        if ((_flags & PropertyInclusionFlags.UnresolvedOnly) != 0)
            return;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, node.File);

        if (!property.Type.TryEvaluateType(out IType? type, in ctx))
            return;

        // todo
        //if (type is ILegacyCompositeTypeProvider)
        //{
        //    _visitedMultiProperties ??= new HashSet<(PropertyBreadcrumbs, DatProperty)>();
        //    if (_visitedMultiProperties.Add((breadcrumbs, property)))
        //    {
        //        AcceptResolvedProperty(property, type, in parseContext, node, in breadcrumbs);
        //    }
        //}
        //else
        //{
            AcceptResolvedProperty(property, type, in ctx, node, in breadcrumbs);
        //}
    }
}

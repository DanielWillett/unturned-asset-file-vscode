using System;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Visits all valid properties in order in an asset file, associating them with their resolved property definitions.
/// </summary>
/// <remarks>Inheriting classes should override <see cref="AcceptResolvedProperty"/>.</remarks>
public abstract class ResolvedPropertyNodeVisitor : OrderedNodeVisitor
{
    private readonly IFilePropertyVirtualizer _virtualizer;
    private readonly IAssetSpecDatabase _database;
    private readonly InstallationEnvironment _installEnv;
    private readonly IWorkspaceEnvironment _workspaceEnv;
    private readonly PropertyInclusionFlags _flags;
    private AssetFileType _fileType;
    private bool _hasFileType;
    private HashSet<(PropertyBreadcrumbs, SpecProperty)>? _visitedMultiProperties;
    private readonly FileRange? _range;

    protected override bool IgnoreMetadata => true;

    protected ResolvedPropertyNodeVisitor(
        IFilePropertyVirtualizer virtualizer,
        IAssetSpecDatabase database,
        InstallationEnvironment installEnv,
        IWorkspaceEnvironment workspaceEnv,
        FileRange? range = null,
        PropertyInclusionFlags flags = PropertyInclusionFlags.All | PropertyInclusionFlags.ResolvedOnly)
    {
        _virtualizer = virtualizer;
        _database = database;
        _installEnv = installEnv;
        _workspaceEnv = workspaceEnv;
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
        SpecProperty property,
        ISpecPropertyType propertyType,
        in SpecPropertyTypeParseContext parseCtx,
        IPropertySourceNode node,
        in PropertyBreadcrumbs breadcrumbs) { }

    protected virtual void AcceptUnresolvedProperty(
        IPropertySourceNode node,
        in PropertyBreadcrumbs breadcrumbs) { }

    protected override void AcceptProperty(IPropertySourceNode node)
    {
        if (!node.IsIncluded(_flags))
            return;
        
        if (node is { File: IAssetSourceFile, ValueKind: ValueTypeDataRefType.Dictionary }
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
            _fileType = AssetFileType.FromFile(node.File, _database);
            _hasFileType = true;
        }

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.FromNode(node);

        SpecProperty? property = _virtualizer.GetProperty(node, in _fileType, in breadcrumbs, out PropertyResolutionContext context);
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

        FileEvaluationContext ctx = new FileEvaluationContext(property, node.File, _workspaceEnv, _installEnv, _database, context);
        SpecPropertyTypeParseContext parseContext = new SpecPropertyTypeParseContext(ctx, breadcrumbs, null)
        {
            Node = node.Value,
            Parent = node
        };

        ISpecPropertyType? type = property.Type.GetType(in ctx);
        if (type == null)
            return;

        if (type is ILegacyCompositeTypeProvider)
        {
            _visitedMultiProperties ??= new HashSet<(PropertyBreadcrumbs, SpecProperty)>();
            if (_visitedMultiProperties.Add((breadcrumbs, property)))
            {
                AcceptResolvedProperty(property, type, in parseContext, node, in breadcrumbs);
            }
        }
        else
        {
            AcceptResolvedProperty(property, type, in parseContext, node, in breadcrumbs);
        }
    }
}

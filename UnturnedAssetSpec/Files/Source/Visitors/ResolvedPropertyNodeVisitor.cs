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
        PropertyInclusionFlags flags = PropertyInclusionFlags.All)
    {
        _virtualizer = virtualizer;
        _database = database;
        _installEnv = installEnv;
        _workspaceEnv = workspaceEnv;
        _flags = flags;
        _range = range;
    }

    protected abstract void AcceptResolvedProperty(
        SpecProperty property,
        ISpecPropertyType propertyType,
        in SpecPropertyTypeParseContext parseCtx,
        IPropertySourceNode node,
        PropertyBreadcrumbs breadcrumbs);

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

        if (!_hasFileType)
        {
            _fileType = AssetFileType.FromFile(node.File, _database);
            _hasFileType = true;
        }

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.FromNode(node);

        SpecProperty? property = _virtualizer.GetProperty(node, in _fileType, in breadcrumbs, out PropertyResolutionContext context);
        if (property == null)
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
                AcceptResolvedProperty(property, type, in parseContext, node, breadcrumbs);
            }
        }
        else
        {
            AcceptResolvedProperty(property, type, in parseContext, node, breadcrumbs);
        }
    }
}

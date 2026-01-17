using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public class SourceFilePropertyVirtualizer : IFilePropertyVirtualizer
{
    private readonly IAssetSpecDatabase _database;
    private readonly IWorkspaceEnvironment _workspaceEnvironment;
    private readonly InstallationEnvironment _installationEnvironment;

    public SourceFilePropertyVirtualizer(
        IAssetSpecDatabase database,
        IWorkspaceEnvironment workspaceEnvironment,
        InstallationEnvironment installationEnvironment)
    {
        _database = database;
        _workspaceEnvironment = workspaceEnvironment;
        _installationEnvironment = installationEnvironment;
    }

    protected void GetEvalCtx(out FileEvaluationContext ctx, DatProperty property, ISourceFile file)
    {
        ctx = new FileEvaluationContext(
            property,
            property.Owner,
            file,
            _workspaceEnvironment,
            _installationEnvironment,
            _database,
            PropertyResolutionContext.Modern
        );
    }

    public IEnumerable<IFileProperty> EnumerateProperties(ISourceFile file)
    {
        yield break;
    }

    public IFileProperty? FindProperty(ISourceFile file, DatProperty property)
    {
        return FindProperty(file, property, PropertyBreadcrumbs.Root);
    }

    public IFileProperty? FindProperty(ISourceFile file, DatProperty property, in PropertyBreadcrumbs breadcrumbs)
    {
        AssetFileType fileType = AssetFileType.FromFile(file, _database);

        DatType assetType = fileType.Information;

        SpecPropertyContext ctx = file switch
        {
            ILocalizationSourceFile => SpecPropertyContext.Localization,
            _ => SpecPropertyContext.Property
        };

        ImmutableArray<DatProperty> properties = assetType.GetPropertyArray(ctx);

        int index = properties.IndexOf(property);

        if (index == -1)
        {
            return null;
        }

        GetEvalCtx(out FileEvaluationContext evalCtx, property, file);

        if (file.TryGetProperty(property, in evalCtx, out IPropertySourceNode? propertyNode))
        {
            return new BasicProperty(
                property,
                propertyNode,
                file,
                assetType,
                _workspaceEnvironment,
                _installationEnvironment,
                _database,
                PropertyResolutionContext.Modern
            );
        }

        // i doubt there will ever be any advanced 'Metadata' properties
        IDictionarySourceNode assetData = file is IAssetSourceFile a ? a.AssetData : file;

        IType? type = property.Type as IType;
        if (type == null && property.Type is TypeSwitch switchCase)
        {
            switchCase.TryEvaluateType(out type, in evalCtx);
        }

        // todo
        // if (type is ILegacyCompositeTypeProvider { IsEnabled: true } compType)
        // {
        //     LinkedPropertyVisit visitor = default;
        //     IPropertySourceNode[] nodes;
        //     GetEvalCtx(out FileEvaluationContext evalCtx, property, file);
        //     visitor.List = new PooledList<IPropertySourceNode>();
        //     try
        //     {
        //         compType.VisitLinkedProperties(in evalCtx, property, assetData, ref visitor, breadcrumbs);
        //         nodes = visitor.List.ToArray();
        //     }
        //     finally
        //     {
        //         visitor.List.Dispose();
        //     }
        // 
        //     return new LegacyCompositeTypeProperty(
        //         property,
        //         nodes,
        //         file,
        //         assetType,
        //         _workspaceEnvironment,
        //         _installationEnvironment,
        //         _database,
        //         PropertyResolutionContext.Modern
        //     );
        // }

        if (property.IsTemplate)
        {
            if (!breadcrumbs.TryGetProperty(file, property, in evalCtx, out IPropertySourceNode? propNode))
            {
                return null;
            }

            return new BasicProperty(
                property,
                propNode,
                file,
                assetType, 
                _workspaceEnvironment,
                _installationEnvironment,
                _database,
                PropertyResolutionContext.Modern
            );
        }

        return null;
    }

    /// <inheritdoc />
    public DatProperty? GetProperty(IPropertySourceNode propertyNode, out PropertyResolutionContext context)
    {
        AssetFileType fileType = AssetFileType.FromFile(propertyNode.File, _database);
        PropertyBreadcrumbs bc = PropertyBreadcrumbs.FromNode(propertyNode);
        return GetProperty(propertyNode, in fileType, in bc, out context);
    }

    /// <inheritdoc />
    public DatProperty? GetProperty(IPropertySourceNode node, in AssetFileType fileType, in PropertyBreadcrumbs propertyBreadcrumbs, out PropertyResolutionContext context)
    {
        context = PropertyResolutionContext.Modern;

        if (node.File is IAssetSourceFile
            && ReferenceEquals(node.File, node.Parent)
            && (string.Equals(node.Key, "Metadata", StringComparison.OrdinalIgnoreCase) || string.Equals(node.Key, "Asset", StringComparison.OrdinalIgnoreCase)))
        {
            // Skip "Asset" and "Metadata" properties
            return null;
        }

        string propertyName = node.Key;
        //if (!propertyBreadcrumbs.TryGetDictionaryAndType(
        //        node.File,
        //        in fileType,
        //        _database,
        //        out _,
        //        out DatType? type))
        //{
        //    return null;
        //}

        return null;
        // AssetSpecDatabaseExtensions.PropertyFindResult result = _database.FindPropertyInfoByKey(
        //     propertyName,
        //     type,
        //     PropertyResolutionContext.Modern,
        //     context: node.File is ILocalizationSourceFile ? SpecPropertyContext.Localization : SpecPropertyContext.Property
        // );
        // 
        // return result.Property;
    }

    private struct LinkedPropertyVisit : ISourceNodePropertyVisitor
    {
        public PooledList<IPropertySourceNode> List;
        public void AcceptProperty(IPropertySourceNode node)
        {
            List.Add(node);
        }
    }
}

internal abstract class BaseProperty(
    DatProperty property,
    ISourceFile file,
    DatType type,
    IWorkspaceEnvironment workspaceEnvironment,
    InstallationEnvironment installationEnvironment,
    IAssetSpecDatabase database,
    PropertyResolutionContext context)
{
    protected readonly IWorkspaceEnvironment WorkspaceEnvironment = workspaceEnvironment;
    protected readonly InstallationEnvironment InstallationEnvironment = installationEnvironment;
    protected readonly IAssetSpecDatabase Database = database;
    protected readonly ISourceFile File = file;
    protected readonly PropertyResolutionContext Context = context;

    public DatProperty Property { get; } = property;

    public DatType Owner { get; } = type;

    protected void GetEvalCtx(out FileEvaluationContext ctx)
    {
        ctx = new FileEvaluationContext(
            Property,
            File,
            WorkspaceEnvironment,
            InstallationEnvironment,
            Database,
            Context
        );
    }

    protected void GetParseCtx(IPropertySourceNode propertyNode, in FileEvaluationContext ctx, [UnscopedRef] out SpecPropertyTypeParseContext parse)
    {
        parse = SpecPropertyTypeParseContext.FromFileEvaluationContext(
            in ctx,
            PropertyBreadcrumbs.Root,
            Property,
            propertyNode,
            propertyNode.Value
        );
    }
}

internal class BasicProperty(
    DatProperty property,
    IPropertySourceNode node,
    ISourceFile file,
    DatType type,
    IWorkspaceEnvironment workspaceEnvironment,
    InstallationEnvironment installationEnvironment,
    IAssetSpecDatabase database,
    PropertyResolutionContext context)
    : BaseProperty(property, file, type, workspaceEnvironment, installationEnvironment, database, context), IFileProperty
{
    public bool TryGetValue(out IValue? value)
    {
        value = null;
        return false;
        //GetEvalCtx(out FileEvaluationContext ctx);
        //GetParseCtx(node, in ctx, out SpecPropertyTypeParseContext parse);
        //
        //if (Property.Type.TryParseValue(in parse, out value))
        //{
        //    return true;
        //}
        //
        //value = Property.IncludedDefaultValue ?? Property.DefaultValue!;
        //return false;
    }
}

internal class LegacyCompositeTypeProperty(
    DatProperty property,
    IPropertySourceNode[] nodes,
    ISourceFile file,
    DatType type,
    IWorkspaceEnvironment workspaceEnvironment,
    InstallationEnvironment installationEnvironment,
    IAssetSpecDatabase database,
    PropertyResolutionContext context)
    : BaseProperty(property, file, type, workspaceEnvironment, installationEnvironment, database, context), IFileProperty
{
    public IPropertySourceNode[] Nodes { get; } = nodes;

    /// <inheritdoc />
    public bool TryGetValue(out IValue? value)
    {
        value = null;
        return false;
        // if (Nodes.Length == 0)
        // {
        //     value = null;
        //     return false;
        // }
        // 
        // GetEvalCtx(out FileEvaluationContext ctx);
        // GetParseCtx(Nodes[0], in ctx, out SpecPropertyTypeParseContext parse);
        // 
        // if (Property.Type.TryParseValue(in parse, out value))
        // {
        //     return true;
        // }
        // 
        // value = Property.IncludedDefaultValue ?? Property.DefaultValue!;
        // return false;
    }
}
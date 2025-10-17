using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal class SourceFilePropertyVirtualizer : IFilePropertyVirtualizer
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

    protected void GetEvalCtx(out FileEvaluationContext ctx, SpecProperty property, ISourceFile file)
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

    public IFileProperty? FindProperty(ISourceFile file, SpecProperty property)
    {
        return FindProperty(file, property, PropertyBreadcrumbs.Root);
    }

    public IFileProperty? FindProperty(ISourceFile file, SpecProperty property, PropertyBreadcrumbs breadcrumbs)
    {
        AssetFileType fileType = AssetFileType.FromFile(file, _database);

        AssetSpecType assetType = fileType.Information;

        SpecPropertyContext ctx = file switch
        {
            ILocalizationSourceFile => SpecPropertyContext.Localization,
            _ => SpecPropertyContext.Property
        };

        SpecProperty[] properties = ctx == SpecPropertyContext.Localization ? assetType.LocalizationProperties : assetType.Properties;

        int index = Array.IndexOf(properties, property);

        if (index == -1)
        {
            foreach (SpecProperty import in assetType.Properties)
            {
                if (!import.TryGetImportType(out IPropertiesSpecType t))
                    continue;

                properties = ctx == SpecPropertyContext.Localization ? t.LocalizationProperties : t.Properties;
                index = Array.IndexOf(properties, property);
                if (index >= 0)
                    break;
            }

            if (index == -1)
                return null;
        }

        if (file.TryGetProperty(property, out IPropertySourceNode? propertyNode))
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

        ISpecPropertyType? type;
        if (property.Type.IsSwitch)
        {
            GetEvalCtx(out FileEvaluationContext evalCtx, property, file);
            type = property.Type.GetType(in evalCtx);
        }
        else
        {
            type = property.Type.Type;
        }

        if (type is ILegacyCompositeTypeProvider { IsEnabled: true } compType)
        {
            LinkedPropertyVisit visitor = default;
            visitor.List = new PooledList<IPropertySourceNode>();
            GetEvalCtx(out FileEvaluationContext evalCtx, property, file);
            compType.VisitLinkedProperties(in evalCtx, property, assetData, ref visitor, breadcrumbs);

            IPropertySourceNode[] nodes = visitor.List.ToArray();
            visitor.List.Dispose();

            return new LegacyCompositeTypeProperty(
                property,
                nodes,
                file,
                assetType,
                _workspaceEnvironment,
                _installationEnvironment,
                _database,
                PropertyResolutionContext.Modern
            );
        }

        if (property.IsTemplate)
        {
            // property.Key
            //Regex regex = new Regex(property.Key, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);
            //foreach (ISourceNode node in assetData.Children)
            //{
            //    if (node is not IPropertySourceNode prop || regex.Match(prop.Key) is not { Success: true } match)
            //        continue;
            //
            //    GroupCollection groups = match.Groups;
            //    int groupIndex = 0;
            //    bool wasMatch = true;
            //    for (int i = 0; i < property.TemplateGroups.Length; ++i)
            //    {
            //        wasMatch = false;
            //        // todo: UseValueOf support
            //        TemplateGroup tmpGroup = property.TemplateGroups[i];
            //        if (tmpGroup.Group >= groups.Count || tmpGroup.UseValueOf != null || groupIndex >= keyIndices.Length)
            //            break;
            //
            //        int keyIndex = keyIndices[groupIndex];
            //        ++groupIndex;
            //        if (!int.TryParse(groups[tmpGroup.Group].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out int groupValue))
            //            break;
            //
            //        if (groupValue != keyIndex)
            //            break;
            //
            //        wasMatch = true;
            //    }
            //
            //    if (wasMatch)
            //    {
            //        return new BasicProperty(
            //            property,
            //            prop,
            //            file,
            //            assetType,
            //            _workspaceEnvironment,
            //            _installationEnvironment,
            //            _database,
            //            PropertyResolutionContext.Modern
            //        );
            //    }
            //}
        }

        return null;
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
    SpecProperty property,
    ISourceFile file,
    AssetSpecType type,
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

    public SpecProperty Property { get; } = property;

    public ISpecType Owner { get; } = type;

    protected void GetEvalCtx(out FileEvaluationContext ctx)
    {
        ctx = new FileEvaluationContext(
            Property,
            Owner,
            File,
            WorkspaceEnvironment,
            InstallationEnvironment,
            Database,
            Context
        );
    }

    protected void GetParseCtx(IPropertySourceNode propertyNode, out SpecPropertyTypeParseContext parse)
    {
        GetEvalCtx(out FileEvaluationContext ctx);
        parse = SpecPropertyTypeParseContext.FromFileEvaluationContext(
            ctx,
            Property,
            propertyNode,
            propertyNode.Value
        );
    }
}

internal class BasicProperty(
    SpecProperty property,
    IPropertySourceNode node,
    ISourceFile file,
    AssetSpecType type,
    IWorkspaceEnvironment workspaceEnvironment,
    InstallationEnvironment installationEnvironment,
    IAssetSpecDatabase database,
    PropertyResolutionContext context)
    : BaseProperty(property, file, type, workspaceEnvironment, installationEnvironment, database, context), IFileProperty
{
    public bool TryGetValue(out ISpecDynamicValue? value)
    {
        GetParseCtx(node, out SpecPropertyTypeParseContext parse);

        if (Property.Type.TryParseValue(in parse, out value))
        {
            return true;
        }

        value = Property.IncludedDefaultValue ?? Property.DefaultValue!;
        return false;
    }
}

internal class LegacyCompositeTypeProperty(
    SpecProperty property,
    IPropertySourceNode[] nodes,
    ISourceFile file,
    AssetSpecType type,
    IWorkspaceEnvironment workspaceEnvironment,
    InstallationEnvironment installationEnvironment,
    IAssetSpecDatabase database,
    PropertyResolutionContext context)
    : BaseProperty(property, file, type, workspaceEnvironment, installationEnvironment, database, context), IFileProperty
{
    public IPropertySourceNode[] Nodes { get; } = nodes;

    /// <inheritdoc />
    public bool TryGetValue(out ISpecDynamicValue? value)
    {
        if (Nodes.Length == 0)
        {
            value = null;
            return false;
        }

        GetParseCtx(Nodes[0], out SpecPropertyTypeParseContext parse);

        if (Property.Type.TryParseValue(in parse, out value))
        {
            return true;
        }

        value = Property.IncludedDefaultValue ?? Property.DefaultValue!;
        return false;
    }
}
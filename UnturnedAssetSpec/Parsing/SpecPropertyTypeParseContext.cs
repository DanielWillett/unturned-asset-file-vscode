using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// Information needed to parse values from properties.
/// </summary>
public readonly ref struct SpecPropertyTypeParseContext
{
#if NET7_0_OR_GREATER
    public readonly ref readonly FileEvaluationContext EvaluationContext;
#else
    public readonly FileEvaluationContext EvaluationContext;
#endif
    public readonly PropertyBreadcrumbs Breadcrumbs;

    public required IAnyValueSourceNode? Node { get; init; }
    public required IParentSourceNode? Parent { get; init; }
    public string? BaseKey { get; init; }

    public IAssetSpecDatabase Database => EvaluationContext.Information;
    public ISourceFile? File => EvaluationContext.SourceFile;
    public AssetFileType FileType => EvaluationContext.FileType;

    public IDiagnosticSink? Diagnostics { get; }
    public IReferencedPropertySink? ReferencedPropertySink { get; init; }

    [MemberNotNullWhen(true, nameof(Diagnostics))]
    public bool HasDiagnostics => Diagnostics != null;

    internal bool AutoDefault => Node == null && EvaluationContext.Self?.Owner is { OverridableProperties: true };

    [UnscopedRef]
    public SpecPropertyTypeParseContext WithoutDiagnostics()
    {
        if (!HasDiagnostics)
            return this;

        SpecPropertyTypeParseContext ctx = new SpecPropertyTypeParseContext(in EvaluationContext, Breadcrumbs, null)
        {
            BaseKey = BaseKey,
            Node = Node,
            Parent = Parent,
            ReferencedPropertySink = ReferencedPropertySink
        };
        return ctx;
    }

    [UnscopedRef]
    public SpecPropertyTypeParseContext WithDiagnostics(IDiagnosticSink diagnostics)
    {
        return new SpecPropertyTypeParseContext(in EvaluationContext, Breadcrumbs, diagnostics)
        {
            BaseKey = BaseKey,
            Node = Node,
            Parent = Parent,
            ReferencedPropertySink = ReferencedPropertySink
        };
    }

    public SpecPropertyTypeParseContext(PropertyBreadcrumbs breadcrumbs, IDiagnosticSink diagnostics) : this(FileEvaluationContext.None, breadcrumbs, diagnostics)
    {

    }

    public SpecPropertyTypeParseContext(in FileEvaluationContext evalContext, PropertyBreadcrumbs breadcrumbs, IDiagnosticSink? diagnostics)
    {
        Diagnostics = diagnostics;
#if NET7_0_OR_GREATER
        EvaluationContext = ref evalContext;
#else
        EvaluationContext = evalContext;
#endif
        Breadcrumbs = breadcrumbs;
    }

    public static SpecPropertyTypeParseContext FromFileEvaluationContext(in FileEvaluationContext evalContext, PropertyBreadcrumbs breadcrumbs, DatProperty? property, IParentSourceNode? parentNode, IAnyValueSourceNode? valueNode, IDiagnosticSink? diagnostics = null, IReferencedPropertySink? refProps = null)
    {
        return new SpecPropertyTypeParseContext(in evalContext, breadcrumbs, diagnostics)
        {
            Parent = parentNode,
            Node = valueNode,
            BaseKey = property?.Key ?? (parentNode as IPropertySourceNode)?.Key,
            ReferencedPropertySink = refProps
        };
    }

    public void Log(DatDiagnosticMessage message)
    {
        if (!HasDiagnostics)
            return;

        Diagnostics!.AcceptDiagnostic(message);
    }

    public GuidOrId GetThisId()
    {
        if (File is not IAssetSourceFile assetSourceFile)
            return GuidOrId.Empty;

        Guid? guid = assetSourceFile.Guid;
        if (guid.HasValue && guid.Value != Guid.Empty)
            return new GuidOrId(guid.Value);

        ushort? id = assetSourceFile.Id;
        if (id is null or 0)
            return GuidOrId.Empty;

        return new GuidOrId(id.Value, assetSourceFile.Category);
    }

    //public string GetParseTargetDisplayName()
    //{
    //    if (EvaluationContext.Self != null && Parent is IPropertySourceNode prop)
    //    {
    //        AssetSpecDatabaseExtensions.PropertyFindResult result
    //            = AssetSpecDatabaseExtensions.GetPropertyKeyInfo(EvaluationContext.Self, prop.Key, EvaluationContext.PropertyContext);
    //        return Breadcrumbs.ToString(false, result.Key ?? BaseKey);
    //    }
    //
    //    return Breadcrumbs.ToString(false, BaseKey);
    //}
}
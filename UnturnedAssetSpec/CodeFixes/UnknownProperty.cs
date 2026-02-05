using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;

internal class UnknownProperty : PerPropertyCodeFix<UnknownProperty.UnknownPropertyState>
{
    /// <inheritdoc />
    public override PropertyInclusionFlags InclusionFlags => PropertyInclusionFlags.All | PropertyInclusionFlags.UnresolvedOnly;

    internal struct UnknownPropertyState
    {
        public FileRange Range;
        public string PropertyName;
    }

    public override bool NeedsExplicitDiscover => false;

    protected override string GetLocalizedTitle(CodeFixInstance<UnknownPropertyState> instance)
    {
        return string.Format(DiagnosticResources.UNT1025_CodeFix_Annotation_Label, instance.Parameters.State.PropertyName);
    }

    public UnknownProperty(
        IFileRelationalModelProvider modelProvider,
        IParsingServices parsingServices)
        : base(DatDiagnostics.UNT1025, modelProvider, parsingServices)
    {

    }

    public override bool TryApplyToProperty(
        out UnknownPropertyState state,
        out FileRange range,
        ref bool hasDiagnostic,
        IPropertySourceNode propertyNode,
        IType propertyType,
        DatProperty property,
        in PropertyBreadcrumbs breadcrumbs,
        in FileEvaluationContext ctx)
    {
        state = default;
        range = default;
        return false;
    }

    /// <inheritdoc />
    public override bool TryApplyToUnknownProperty(
        out UnknownPropertyState state,
        out FileRange range,
        ref bool hasDiagnostic,
        IPropertySourceNode propertyNode,
        in PropertyBreadcrumbs breadcrumbs)
    {
        state.Range = propertyNode.GetFullRange();
        state.PropertyName = propertyNode.Key;
        range = state.Range;
        hasDiagnostic = true;
        return true;
    }

    public override void ApplyCodeFix(in CodeFixParameters<UnknownPropertyState> parameters, IMutableWorkspaceFile file)
    {
        UnknownPropertyState state = parameters.State;
        file.UpdateText(state, static (updater, state) =>
        {
            const string annotation = "a";
            updater.AddAnnotation(annotation,
                string.Format(DiagnosticResources.UNT1025_CodeFix_Annotation_Label, state.PropertyName),
                DiagnosticResources.UNT1025_CodeFix_Annotation_Desc,
                needsConfirmation: false
            );

            updater.RemoveOverlappingLines(state.Range, annotation);
        });
    }
}
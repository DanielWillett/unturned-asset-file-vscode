using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;

internal class GenerateNewGuid : PerPropertyCodeFix<GenerateNewGuid.GenerateNewGuidState>
{
    private readonly InstallationEnvironment _installEnv;

    internal struct GenerateNewGuidState
    {
        public bool Dashes;
        public FileRange Range;
    }

    public override bool NeedsExplicitDiscover => false;

    protected override string GetLocalizedTitle(CodeFixInstance<GenerateNewGuidState> instance)
    {
        return DiagnosticResources.UNT107_CodeFix_Annotation_Label;
    }

    public GenerateNewGuid(
        IFilePropertyVirtualizer virtualizer,
        IAssetSpecDatabase database,
        InstallationEnvironment installEnv,
        IWorkspaceEnvironment workspaceEnv)
        : base(DatDiagnostics.UNT107, virtualizer, database, installEnv, workspaceEnv)
    {
        _installEnv = installEnv;
        database.OnInitialize(_ =>
        {
            ValidTypes = [ KnownTypes.Guid ];
            return Task.CompletedTask;
        });
    }

    public override bool TryApplyToProperty(
        out GenerateNewGuidState state,
        out FileRange range,
        ref bool hasDiagnostic,
        IPropertySourceNode propertyNode,
        ISpecPropertyType propertyType,
        SpecProperty property,
        in PropertyBreadcrumbs breadcrumbs,
        in SpecPropertyTypeParseContext parseContext)
    {
        state = default;
        range = default;

        if (propertyNode.ValueKind == ValueTypeDataRefType.Value)
        {
            string? valueStr = propertyNode.GetValueString(out _);
            state.Dashes = valueStr != null && Guid.TryParseExact(valueStr, "D", out _);
            state.Range = propertyNode.GetValueRange();
            return true;
        }

        return false;
    }

    public override void ApplyCodeFix(in CodeFixParameters<GenerateNewGuidState> parameters, IMutableWorkspaceFile file)
    {
        GenerateNewGuidState state = parameters.State;
        file.UpdateText(state, (updater, state) =>
        {
            const string annotation = "a";
            updater.AddAnnotation(annotation,
                DiagnosticResources.UNT107_CodeFix_Annotation_Label,
                DiagnosticResources.UNT107_CodeFix_Annotation_Desc,
                needsConfirmation: false
            );

            Guid guid;
            do
            {
                guid = Guid.NewGuid();
            }
            while (!_installEnv.FindFile(guid).IsNull);

            updater.ReplaceText(state.Range, state.Dashes ? guid.ToString("D") : guid.ToString("N"), annotation);
        });
    }
}
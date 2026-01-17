using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;

internal class BlueprintUseThisKeyword : PerPropertyCodeFix<BlueprintUseThisKeyword.BlueprintUseThisKeywordState>
{
    internal struct BlueprintUseThisKeywordState
    {
        public List<FileRange> Ranges;
        public FileRange Range;
    }

    private const string ThisKeyword = "this";

    public override bool NeedsExplicitDiscover => false;

    protected override string GetLocalizedTitle(CodeFixInstance<BlueprintUseThisKeywordState> instance)
    {
        return DiagnosticResources.UNT101_CodeFix_Annotation_Label;
    }

    public BlueprintUseThisKeyword(
        IFilePropertyVirtualizer virtualizer,
        IAssetSpecDatabase database,
        InstallationEnvironment installEnv,
        IWorkspaceEnvironment workspaceEnv)
        : base(DatDiagnostics.UNT101, virtualizer, database, installEnv, workspaceEnv)
    {
        database.OnInitialize((database, _) =>
        {
            //AssetFileType assetFileType =
            //    AssetFileType.FromType(
            //        QualifiedOrAliasedType.FromType("SDG.Unturned.ItemAsset, Assembly-CSharp"),
            //        database
            //    );

            //HashSet<IType> types = new HashSet<IType>();
            //ISpecType? outputType = database.FindType("SDG.Unturned.BlueprintOutput, Assembly-CSharp", assetFileType);
            //if (outputType is IType<DatCustomTypeInstance> outputPropType)
            //{
            //    types.Add(outputPropType);
            //    types.Add(ListType.Create(new ListTypeArgs<>(), outputPropType));
            //    types.Add(KnownTypes.LegacyCompatibleList(outputPropType, allowSingleLegacy: false, allowSingleModern: true));
            //}

            //ISpecType? supplyType = database.FindType("SDG.Unturned.BlueprintSupply, Assembly-CSharp", assetFileType);
            //if (supplyType is IType<CustomSpecTypeInstance> supplyPropType)
            //{
            //    types.Add(supplyPropType);
            //    types.Add(KnownTypes.List(supplyPropType, allowSingle: true));
            //    types.Add(KnownTypes.LegacyCompatibleList(supplyPropType, allowSingleLegacy: false, allowSingleModern: true));
            //}

            //ValidTypes = types;
            return Task.CompletedTask;
        });
    }

    public override bool TryApplyToProperty(
        out BlueprintUseThisKeywordState state,
        out FileRange range,
        ref bool hasDiagnostic,
        IPropertySourceNode propertyNode,
        IType propertyType,
        DatProperty property,
        in PropertyBreadcrumbs breadcrumbs,
        in SpecPropertyTypeParseContext parseContext)
    {
        state = default;
        range = default;
        if (propertyNode.File is not IAssetSourceFile assetFile)
        {
            return false;
        }

        Guid? guid = assetFile.Guid;
        ushort? id = assetFile.Id;
        switch (propertyType)
        {
            case IListType when propertyNode.Value is IListSourceNode listNode:
                string? valueStr;
                state.Ranges = new List<FileRange>();
                foreach (ISourceNode v in listNode.Children)
                {
                    if (v is not IValueSourceNode value)
                        continue;

                    if (TryCheckIsThisable(value.Value, guid, id))
                    {
                        state.Ranges.Add(value.Range);
                    }
                }

                return state.Ranges.Count > 0;

            case var _ when propertyNode.ValueKind == SourceValueType.Value:
                valueStr = propertyNode.GetValueString(out _);
                if (TryCheckIsThisable(valueStr, guid, id))
                {
                    state.Range = propertyNode.GetValueRange();
                    return true;
                }

                break;
        }

        return false;

        static bool TryCheckIsThisable(string? valueStr, Guid? guid, ushort? id)
        {
            if (string.IsNullOrEmpty(valueStr))
                return false;

            if (guid.HasValue && KnownTypeValueHelper.TryParseGuid(valueStr!, out Guid bpGuid) && bpGuid == guid.Value)
            {
                return true;
            }

            if (id.HasValue && KnownTypeValueHelper.TryParseUInt16(valueStr!, out ushort bpId) && bpId == id.Value)
            {
                return true;
            }

            return false;
        }
    }

    public override void ApplyCodeFix(in CodeFixParameters<BlueprintUseThisKeywordState> parameters, IMutableWorkspaceFile file)
    {
        BlueprintUseThisKeywordState state = parameters.State;
        file.UpdateText(state, static (updater, state) =>
        {
            const string annotation = "a";
            updater.AddAnnotation(annotation,
                DiagnosticResources.UNT101_CodeFix_Annotation_Label,
                DiagnosticResources.UNT101_CodeFix_Annotation_Desc,
                needsConfirmation: true
            );

            if (state.Ranges == null)
            {
                updater.ReplaceText(state.Range, ThisKeyword, annotation);
            }
            else foreach (FileRange range in state.Ranges)
            {
                updater.ReplaceText(range, ThisKeyword, annotation);
            }
        });
    }
}
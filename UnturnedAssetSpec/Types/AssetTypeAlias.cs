using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class AssetTypeAlias : BasicSpecPropertyType<AssetTypeAlias, QualifiedType>
{
    /// <inheritdoc />
    public override string DisplayName => "Asset Type";

    /// <inheritdoc />
    public override string Type => "DanielWillett.UnturnedDataFileLspServer.Data.Types.AssetTypeAlias, UnturnedAssetSpec";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Enum;

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out QualifiedType value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode)
        {
            return FailedToParse(in parse, out value);
        }

        if (parse.Database.Information.AssetAliases.TryGetValue(strValNode.Value, out value))
        {
            return true;
        }

        if (!parse.HasDiagnostics)
            return false;

        parse.Log(new DatDiagnosticMessage
        {
            Range = strValNode.Range,
            Diagnostic = DatDiagnostics.UNT1014,
            Message = string.Format(DiagnosticResources.UNT1014, strValNode.Value)
        });
        return false;
    }
}

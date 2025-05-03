using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class StringSpecPropertyType : BasicSpecPropertyType<StringSpecPropertyType, string>
{
    public static readonly StringSpecPropertyType Instance = new StringSpecPropertyType();

    static StringSpecPropertyType() { }
    private StringSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "String";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Text";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode)
        {
            return FailedToParse(in parse, out value);
        }

        if (parse.HasDiagnostics && KnownTypeValueHelper.ContainsRichText(strValNode.Value))
        {
            parse.Log(new DatDiagnosticMessage
            {
                Range = strValNode.Range,
                Diagnostic = DatDiagnostics.UNT1006,
                Message = DiagnosticResources.UNT1006
            });
        }

        value = strValNode.Value;
        return true;
    }
}
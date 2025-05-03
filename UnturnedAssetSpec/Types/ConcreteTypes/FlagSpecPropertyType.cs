using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class FlagSpecPropertyType : BasicSpecPropertyType<FlagSpecPropertyType, bool>
{
    public static readonly FlagSpecPropertyType Instance = new FlagSpecPropertyType();

    static FlagSpecPropertyType() { }
    private FlagSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Flag";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Boolean;

    /// <inheritdoc />
    public override string DisplayName => "Flag";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out bool value)
    {
        if (parse.Node != null && parse.HasDiagnostics)
        {
            DatDiagnosticMessage diagnostic = new DatDiagnosticMessage
            {
                Range = parse.Parent?.Range ?? parse.Node.Range
            };

            if (parse.Node is AssetFileStringValueNode stringValue
                && KnownTypeValueHelper.TryParseBoolean(stringValue.Value, out bool boolValue)
                && !boolValue)
            {
                diagnostic.Diagnostic = DatDiagnostics.UNT2003;
                diagnostic.Message = string.Format(DiagnosticResources.UNT2003, stringValue.Value);
            }
            else
            {
                diagnostic.Diagnostic = DatDiagnostics.UNT1003;
                diagnostic.Message = DiagnosticResources.UNT1003;
            }

            parse.Log(diagnostic);
        }

        value = true;
        return true;
    }
}

using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class FlagSpecPropertyType : BasicSpecPropertyType<FlagSpecPropertyType, bool>, IStringParseableSpecPropertyType
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
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (span.Equals("true".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
        {
            dynamicValue = SpecDynamicValue.True;
            return true;
        }
        if (span.Equals("false".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
        {
            dynamicValue = SpecDynamicValue.False;
            return true;
        }

        if (KnownTypeValueHelper.TryParseBoolean(stringValue ?? span.ToString(), out bool result))
        {
            dynamicValue = result ? SpecDynamicValue.True : SpecDynamicValue.False;
            return true;
        }

        dynamicValue = null!;
        return false;
    }

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

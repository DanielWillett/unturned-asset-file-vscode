using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class FaceIndexSpecPropertyType : BasicSpecPropertyType<FaceIndexSpecPropertyType, byte>, IStringParseableSpecPropertyType
{
    public static readonly FaceIndexSpecPropertyType Instance = new FaceIndexSpecPropertyType();

    static FaceIndexSpecPropertyType() { }
    private FaceIndexSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "FaceIndex";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Face (Index)";

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (byte.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte result))
        {
            dynamicValue = SpecDynamicValue.UInt8(result);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out byte value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseUInt8(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        if (value < parse.Database.Information.FaceCount)
            return true;

        if (!parse.HasDiagnostics)
            return false;

        parse.Log(new DatDiagnosticMessage
        {
            Range = parse.Node.Range,
            Diagnostic = DatDiagnostics.UNT1008,
            Message = string.Format(DiagnosticResources.UNT1008_Face, (parse.Database.Information.FaceCount - 1).ToString(CultureInfo.CurrentUICulture))
        });

        return false;
    }
}
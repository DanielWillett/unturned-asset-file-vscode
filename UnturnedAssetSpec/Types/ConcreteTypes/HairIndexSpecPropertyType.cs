using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class HairIndexSpecPropertyType : BasicSpecPropertyType<HairIndexSpecPropertyType, byte>, IStringParseableSpecPropertyType
{
    public static readonly HairIndexSpecPropertyType Instance = new HairIndexSpecPropertyType();

    public override int GetHashCode() => 28;

    static HairIndexSpecPropertyType() { }
    private HairIndexSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "HairIndex";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Hair (Index)";

    protected override ISpecDynamicValue CreateValue(byte value) => new SpecDynamicConcreteConvertibleValue<byte>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<byte>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (byte.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte result))
        {
            dynamicValue = SpecDynamicValue.UInt8(result, this);
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

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseUInt8(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        if (value < parse.Database.Information.HairCount)
            return true;

        if (!parse.HasDiagnostics)
            return false;

        parse.Log(new DatDiagnosticMessage
        {
            Range = parse.Node.Range,
            Diagnostic = DatDiagnostics.UNT1008,
            Message = string.Format(DiagnosticResources.UNT1008_Hair, (parse.Database.Information.HairCount - 1).ToString(CultureInfo.CurrentUICulture))
        });

        return false;
    }
}
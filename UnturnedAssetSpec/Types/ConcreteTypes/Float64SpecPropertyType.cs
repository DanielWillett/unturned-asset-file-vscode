using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Float64SpecPropertyType : BasicSpecPropertyType<Float64SpecPropertyType, double>, IStringParseableSpecPropertyType
{
    public static readonly Float64SpecPropertyType Instance = new Float64SpecPropertyType();

    public override int GetHashCode() => 26;

    static Float64SpecPropertyType() { }
    private Float64SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Float64";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Double-Precision Decimal";

    protected override ISpecDynamicValue CreateValue(double value) => new SpecDynamicConcreteConvertibleValue<double>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<double>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (double.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            dynamicValue = SpecDynamicValue.Float64(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out double value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseDouble(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
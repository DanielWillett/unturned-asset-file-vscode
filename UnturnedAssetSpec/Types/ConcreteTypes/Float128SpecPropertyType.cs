using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Float128SpecPropertyType : BasicSpecPropertyType<Float128SpecPropertyType, decimal>, IStringParseableSpecPropertyType
{
    public static readonly Float128SpecPropertyType Instance = new Float128SpecPropertyType();

    static Float128SpecPropertyType() { }
    private Float128SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Float128";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Decimal";

    protected override ISpecDynamicValue CreateValue(decimal value) => new SpecDynamicConcreteConvertibleValue<decimal>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<decimal>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (decimal.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
        {
            dynamicValue = SpecDynamicValue.Float128(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out decimal value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseDecimal(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
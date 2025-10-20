using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Int8SpecPropertyType : BasicSpecPropertyType<Int8SpecPropertyType, sbyte>, IStringParseableSpecPropertyType
{
    public static readonly Int8SpecPropertyType Instance = new Int8SpecPropertyType();

    public override int GetHashCode() => 32;

    static Int8SpecPropertyType() { }
    private Int8SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Int8";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Signed 8-Bit Integer";

    protected override ISpecDynamicValue CreateValue(sbyte value) => new SpecDynamicConcreteConvertibleValue<sbyte>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<sbyte>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (sbyte.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out sbyte result))
        {
            dynamicValue = SpecDynamicValue.Int8(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out sbyte value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseInt8(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
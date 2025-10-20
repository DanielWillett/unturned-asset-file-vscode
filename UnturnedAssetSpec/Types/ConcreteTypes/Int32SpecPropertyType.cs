using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Int32SpecPropertyType : BasicSpecPropertyType<Int32SpecPropertyType, int>, IStringParseableSpecPropertyType
{
    public static readonly Int32SpecPropertyType Instance = new Int32SpecPropertyType();

    public override int GetHashCode() => 30;

    static Int32SpecPropertyType() { }
    private Int32SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Int32";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Signed 32-Bit Integer";

    protected override ISpecDynamicValue CreateValue(int value) => new SpecDynamicConcreteConvertibleValue<int>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<int>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (int.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
        {
            dynamicValue = SpecDynamicValue.Int32(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out int value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseInt32(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
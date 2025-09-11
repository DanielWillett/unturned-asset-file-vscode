using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class UInt8SpecPropertyType : BasicSpecPropertyType<UInt8SpecPropertyType, byte>, IStringParseableSpecPropertyType
{
    public static readonly UInt8SpecPropertyType Instance = new UInt8SpecPropertyType();

    static UInt8SpecPropertyType() { }
    private UInt8SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "UInt8";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Unsigned 8-Bit Integer";

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

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseUInt8(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
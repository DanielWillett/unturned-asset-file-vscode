using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Int64SpecPropertyType : BasicSpecPropertyType<Int64SpecPropertyType, long>, IStringParseableSpecPropertyType
{
    public static readonly Int64SpecPropertyType Instance = new Int64SpecPropertyType();

    static Int64SpecPropertyType() { }
    private Int64SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Int64";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Signed 64-Bit Integer";

    protected override ISpecDynamicValue CreateValue(long value) => new SpecDynamicConcreteConvertibleValue<long>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<long>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (long.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out long result))
        {
            dynamicValue = SpecDynamicValue.Int64(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out long value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseInt64(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
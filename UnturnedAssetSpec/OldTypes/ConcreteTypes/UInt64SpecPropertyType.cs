using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An unsigned 64-bit integer.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop 123
/// </code>
/// </summary>
public sealed class UInt64SpecPropertyType : BaseSpecPropertyType<UInt64SpecPropertyType, ulong>, IStringParseableSpecPropertyType
{
    public static readonly UInt64SpecPropertyType Instance = new UInt64SpecPropertyType();

    public override int GetHashCode() => 53;

    static UInt64SpecPropertyType() { }
    private UInt64SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "UInt64";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Unsigned 64-Bit Integer";

    protected override ISpecDynamicValue CreateValue(ulong value) => new SpecDynamicConcreteConvertibleValue<ulong>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<ulong>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (ulong.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out ulong result))
        {
            dynamicValue = SpecDynamicValue.UInt64(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out ulong value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseUInt64(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An unsigned 32-bit integer.
/// <para>Example: <c>AnimalAsset.Reward_XP</c></para>
/// <code>
/// Prop 123
/// </code>
/// </summary>
public sealed class UInt32SpecPropertyType : BasicSpecPropertyType<UInt32SpecPropertyType, uint>, IStringParseableSpecPropertyType
{
    public static readonly UInt32SpecPropertyType Instance = new UInt32SpecPropertyType();

    public override int GetHashCode() => 52;

    static UInt32SpecPropertyType() { }
    private UInt32SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "UInt32";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Unsigned 32-Bit Integer";

    protected override ISpecDynamicValue CreateValue(uint value) => new SpecDynamicConcreteConvertibleValue<uint>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<uint>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (uint.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out uint result))
        {
            dynamicValue = SpecDynamicValue.UInt32(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out uint value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseUInt32(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
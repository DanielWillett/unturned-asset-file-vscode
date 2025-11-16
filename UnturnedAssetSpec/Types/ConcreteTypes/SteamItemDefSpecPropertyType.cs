using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A 32 bit integer representing a SteamItemDef_t value (Steam inventory item ID).
/// <para>Valid values fall between 1 and 999,999,999 inclusively.</para>
/// <para>Example: <c>ItemBoxAsset.Generate</c></para>
/// <code>
/// Prop 52400
/// </code>
/// </summary>
public sealed class SteamItemDefSpecPropertyType : BasicSpecPropertyType<SteamItemDefSpecPropertyType, int>, IStringParseableSpecPropertyType
{
    public static readonly SteamItemDefSpecPropertyType Instance = new SteamItemDefSpecPropertyType();

    public override int GetHashCode() => 48;

    public const int MinValue = 1;
    public const int MaxValue = 999_999_999;

    static SteamItemDefSpecPropertyType() { }
    private SteamItemDefSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "SteamItemDef";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Steam Item Identifier";

    protected override ISpecDynamicValue CreateValue(int value) => new SpecDynamicConcreteConvertibleValue<int>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<int>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (int.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result)
            && result is >= MinValue and <= MaxValue)
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

        if (parse.Node is not IValueSourceNode strValNode
            || !KnownTypeValueHelper.TryParseInt32(strValNode.Value, out value)
            || value is >= MinValue and <= MaxValue)
        {
            return FailedToParse(in parse, out value);
        }

        return true;
    }
}
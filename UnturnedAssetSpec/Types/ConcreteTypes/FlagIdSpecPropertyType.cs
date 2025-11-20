using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Unique flag ID used for quest conditions and rewards.
/// <para>Example: <c>ObjectNPCAsset.PlayerKnowsNameFlagID</c></para>
/// <code>
/// Prop 123
/// </code>
/// </summary>
public sealed class FlagIdSpecPropertyType : BasicSpecPropertyType<FlagIdSpecPropertyType, ushort>, IStringParseableSpecPropertyType
{

    public static readonly FlagIdSpecPropertyType Instance = new FlagIdSpecPropertyType();

    public override int GetHashCode() => 22;

    static FlagIdSpecPropertyType() { }
    private FlagIdSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "FlagId";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Flag ID";

    protected override ISpecDynamicValue CreateValue(ushort value) => new SpecDynamicConcreteConvertibleValue<ushort>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<ushort>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (ushort.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out ushort result))
        {
            dynamicValue = SpecDynamicValue.UInt16(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out ushort value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseUInt16(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        return true;
    }
}
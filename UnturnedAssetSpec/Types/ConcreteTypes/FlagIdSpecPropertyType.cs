using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class FlagIdSpecPropertyType :
    BaseSpecPropertyType<ushort>,
    ISpecPropertyType<ushort>,
    IEquatable<FlagIdSpecPropertyType?>,
    IStringParseableSpecPropertyType
{
    public static readonly FlagIdSpecPropertyType Instance = new FlagIdSpecPropertyType();

    static FlagIdSpecPropertyType() { }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName => "Flag ID";

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "FlagId";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public Type ValueType => typeof(ushort);

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

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out ushort val))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteValue<ushort>(val, this);
        return true;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ushort value)
    {
        if (parse.Node == null)
            return MissingNode(in parse, out value);

        if (parse.Node is not AssetFileStringValueNode strValNode
            || !KnownTypeValueHelper.TryParseUInt16(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(FlagIdSpecPropertyType? other) => other != null;

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is FlagIdSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<ushort>? other) => other is FlagIdSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}
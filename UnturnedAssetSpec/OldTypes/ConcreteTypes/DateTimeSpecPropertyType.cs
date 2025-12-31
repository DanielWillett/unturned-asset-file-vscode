using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A UTC date-time.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop 2025-11-15T21:30:35
/// </code>
/// </summary>
public sealed class DateTimeSpecPropertyType : BaseSpecPropertyType<DateTimeSpecPropertyType, DateTime>, IStringParseableSpecPropertyType
{
    public static readonly DateTimeSpecPropertyType Instance = new DateTimeSpecPropertyType();

    public override int GetHashCode() => 17;

    static DateTimeSpecPropertyType() { }
    private DateTimeSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "DateTime";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    /// <inheritdoc />
    public override string DisplayName => "Timestamp (UTC)";

    protected override ISpecDynamicValue CreateValue(DateTime value) => new SpecDynamicConcreteConvertibleValue<DateTime>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<DateTime>()?.ToString("O", CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (DateTime.TryParse(stringValue ?? span.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dt))
        {
            dynamicValue = new SpecDynamicConcreteConvertibleValue<DateTime>(dt.ToUniversalTime(), this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out DateTime value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseDateTime(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class DateTimeOffsetSpecPropertyType : BasicSpecPropertyType<DateTimeOffsetSpecPropertyType, DateTimeOffset>, IStringParseableSpecPropertyType
{
    public static readonly DateTimeOffsetSpecPropertyType Instance = new DateTimeOffsetSpecPropertyType();

    static DateTimeOffsetSpecPropertyType() { }
    private DateTimeOffsetSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "DateTimeOffset";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    /// <inheritdoc />
    public override string DisplayName => "Timestamp + Timezone";

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<DateTimeOffset>()?.ToString("O", CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (DateTimeOffset.TryParse(stringValue ?? span.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset dt))
        {
            dynamicValue = new SpecDynamicConcreteValue<DateTimeOffset>(dt.ToUniversalTime(), this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out DateTimeOffset value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseDateTimeOffset(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class DateTimeSpecPropertyType : BasicSpecPropertyType<DateTimeSpecPropertyType, DateTime>, IStringParseableSpecPropertyType
{
    public static readonly DateTimeSpecPropertyType Instance = new DateTimeSpecPropertyType();

    static DateTimeSpecPropertyType() { }
    private DateTimeSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "DateTime";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    /// <inheritdoc />
    public override string DisplayName => "Timestamp (UTC)";

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (DateTime.TryParse(stringValue ?? span.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dt))
        {
            dynamicValue = new SpecDynamicConcreteValue<DateTime>(dt.ToUniversalTime(), this);
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

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseDateTime(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
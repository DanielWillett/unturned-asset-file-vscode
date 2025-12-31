using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A time or time span.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop 00:30:00
/// </code>
/// </summary>
public sealed class TimeSpanSpecPropertyType : BaseSpecPropertyType<TimeSpanSpecPropertyType, TimeSpan>, IStringParseableSpecPropertyType
{
    public static readonly TimeSpanSpecPropertyType Instance = new TimeSpanSpecPropertyType();

    public override int GetHashCode() => 91;

    static TimeSpanSpecPropertyType() { }
    private TimeSpanSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "TimeSpan";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    /// <inheritdoc />
    public override string DisplayName => "Time";

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<TimeSpan>()?.ToString("c", CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (TimeSpan.TryParse(stringValue ?? span.ToString(), CultureInfo.InvariantCulture, out TimeSpan dt))
        {
            dynamicValue = new SpecDynamicConcreteValue<TimeSpan>(dt, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out TimeSpan value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseTimeSpan(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
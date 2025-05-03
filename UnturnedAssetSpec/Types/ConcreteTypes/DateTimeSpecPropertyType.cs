using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class DateTimeSpecPropertyType : BasicSpecPropertyType<DateTimeSpecPropertyType, DateTime>
{
    public static readonly DateTimeSpecPropertyType Instance = new DateTimeSpecPropertyType();

    static DateTimeSpecPropertyType() { }
    private DateTimeSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "DateTime";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public override string DisplayName => "Timestamp (UTC)";

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
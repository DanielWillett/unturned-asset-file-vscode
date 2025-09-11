using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class GuidSpecPropertyType : BasicSpecPropertyType<GuidSpecPropertyType, Guid>, IStringParseableSpecPropertyType
{
    public static readonly GuidSpecPropertyType Instance = new GuidSpecPropertyType();

    static GuidSpecPropertyType() { }
    private GuidSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Guid";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public override string DisplayName => "GUID";

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<Guid>()?.ToString("N");
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (Guid.TryParse(stringValue ?? span.ToString(), out Guid result))
        {
            dynamicValue = SpecDynamicValue.Guid(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out Guid value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseGuid(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
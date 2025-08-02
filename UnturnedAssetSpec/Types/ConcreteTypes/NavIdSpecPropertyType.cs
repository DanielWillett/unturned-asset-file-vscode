using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class NavIdSpecPropertyType : BasicSpecPropertyType<NavIdSpecPropertyType, byte>, IStringParseableSpecPropertyType
{
    public static readonly NavIdSpecPropertyType Instance = new NavIdSpecPropertyType();

    static NavIdSpecPropertyType() { }
    private NavIdSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "NavId";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Navmesh ID";

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (byte.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte result))
        {
            dynamicValue = SpecDynamicValue.UInt8(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out byte value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseUInt8(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        // todo: nav test
        
        return true;
    }
}
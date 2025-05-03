using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class UInt64SpecPropertyType : BasicSpecPropertyType<UInt64SpecPropertyType, ulong>
{
    public static readonly UInt64SpecPropertyType Instance = new UInt64SpecPropertyType();

    static UInt64SpecPropertyType() { }
    private UInt64SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "UInt64";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Unsigned 64-Bit Integer";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out ulong value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseUInt64(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
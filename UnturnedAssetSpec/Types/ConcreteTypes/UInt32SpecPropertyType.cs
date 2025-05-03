using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class UInt32SpecPropertyType : BasicSpecPropertyType<UInt32SpecPropertyType, uint>
{
    public static readonly UInt32SpecPropertyType Instance = new UInt32SpecPropertyType();

    static UInt32SpecPropertyType() { }
    private UInt32SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "UInt32";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Unsigned 32-Bit Integer";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out uint value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseUInt32(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class UInt16SpecPropertyType : BasicSpecPropertyType<UInt16SpecPropertyType, ushort>
{
    public static readonly UInt16SpecPropertyType Instance = new UInt16SpecPropertyType();

    static UInt16SpecPropertyType() { }
    private UInt16SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "UInt16";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Unsigned 16-Bit Integer";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out ushort value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseUInt16(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
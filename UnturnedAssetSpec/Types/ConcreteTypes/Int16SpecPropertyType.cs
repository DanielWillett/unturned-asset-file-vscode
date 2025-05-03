using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Int16SpecPropertyType : BasicSpecPropertyType<Int16SpecPropertyType, short>
{
    public static readonly Int16SpecPropertyType Instance = new Int16SpecPropertyType();

    static Int16SpecPropertyType() { }
    private Int16SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Int16";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Signed 16-Bit Integer";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out short value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseInt16(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
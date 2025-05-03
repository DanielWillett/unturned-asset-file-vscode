using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Int32SpecPropertyType : BasicSpecPropertyType<Int32SpecPropertyType, int>
{
    public static readonly Int32SpecPropertyType Instance = new Int32SpecPropertyType();

    static Int32SpecPropertyType() { }
    private Int32SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Int32";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Signed 32-Bit Integer";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out int value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseInt32(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
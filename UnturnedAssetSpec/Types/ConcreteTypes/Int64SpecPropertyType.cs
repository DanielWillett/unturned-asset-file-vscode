using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Int64SpecPropertyType : BasicSpecPropertyType<Int64SpecPropertyType, long>
{
    public static readonly Int64SpecPropertyType Instance = new Int64SpecPropertyType();

    static Int64SpecPropertyType() { }
    private Int64SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Int64";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Signed 64-Bit Integer";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out long value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseInt64(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
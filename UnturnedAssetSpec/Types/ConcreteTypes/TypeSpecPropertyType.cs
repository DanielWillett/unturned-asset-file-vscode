using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class TypeSpecPropertyType : BasicSpecPropertyType<TypeSpecPropertyType, QualifiedType>
{
    public static readonly TypeSpecPropertyType Instance = new TypeSpecPropertyType();

    static TypeSpecPropertyType() { }
    private TypeSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Type";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public override string DisplayName => "Type";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out QualifiedType value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseType(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class TypeSpecPropertyType : BasicSpecPropertyType<TypeSpecPropertyType, QualifiedType>, IStringParseableSpecPropertyType
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

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<QualifiedType>()?.Type;
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (span.Equals("null".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            dynamicValue = new SpecDynamicConcreteValue<QualifiedType>(this);
            return true;
        }

        if (QualifiedType.ExtractParts(span, out _, out ReadOnlySpan<char> assemblyName) && assemblyName.Length > 0)
        {
            QualifiedType type = new QualifiedType(stringValue ?? span.ToString());
            if (type.IsNormalized)
            {
                dynamicValue = new SpecDynamicConcreteValue<QualifiedType>(type, this);
                return true;
            }
        }

        dynamicValue = null!;
        return false;
    }

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
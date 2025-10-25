using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class TypeOrEnumSpecPropertyType :
    BaseSpecPropertyType<QualifiedType>,
    ISpecPropertyType<QualifiedType>,
    IElementTypeSpecPropertyType,
    IEquatable<TypeOrEnumSpecPropertyType>,
    IStringParseableSpecPropertyType,
    ISpecialTypesSpecPropertyType,
    IValueHoverProviderSpecPropertyType
{
    public QualifiedType ElementType { get; }
    public QualifiedType EnumType { get; }

    string IElementTypeSpecPropertyType.ElementType => EnumType;
    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => new OneOrMore<string?>(ElementType);

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "TypeOrEnum";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public Type ValueType => typeof(QualifiedType);

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

    public override int GetHashCode()
    {
        return 82 ^ HashCode.Combine(ElementType, EnumType);
    }

    public TypeOrEnumSpecPropertyType(QualifiedType elementType, QualifiedType enumType)
    {
        ElementType = elementType;
        EnumType = enumType;
        DisplayName = elementType.IsNull ? $"Type Reference or {enumType.GetTypeName()}" : $"Type Reference of {elementType.GetTypeName()} or {enumType.GetTypeName()}";
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out QualifiedType val))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteValue<QualifiedType>(val, this);
        return true;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out QualifiedType value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode stringNode)
        {
            return FailedToParse(in parse, out value);
        }

        string val = stringNode.Value;

        if (val.IndexOf('.') < 0)
        {
            if (!TryParseEnum(in parse, ref val, out value, out _))
            {
                return false;
            }
        }

        if (!KnownTypeValueHelper.TryParseType(val, out value))
        {
            return FailedToParse(in parse, out value);
        }

        QualifiedType baseType = ElementType;

        if (baseType.Type == null
            || baseType.Type.Equals("*", StringComparison.Ordinal)
            || baseType.Equals("System.Object, mscorlib"))
        {
            return true;
        }

        if (!parse.Database.Information.IsAssignableFrom(baseType, value)
            || parse.Database.Information.IsAbstract(value))
        {
            if (parse.HasDiagnostics)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1015,
                    Message = string.Format(DiagnosticResources.UNT1015, QualifiedType.ExtractTypeName(baseType.Type.AsSpan()).ToString()),
                    Range = stringNode.Range
                });
            }

            value = default;
            return false;
        }

        return true;
    }

    private bool TryParseEnum(in SpecPropertyTypeParseContext parse, ref string val, out QualifiedType value, out ISpecDynamicValue? enumValue)
    {
        ISpecType? enumType = parse.Database.FindType(EnumType.Type, parse.FileType);
        enumValue = null;
        if (enumType is not IStringParseableSpecPropertyType fullEnumType)
        {
            if (parse.HasDiagnostics)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT2005,
                    Message = string.Format(DiagnosticResources.UNT2005, ElementType.Type),
                    Range = parse.Node?.Range ?? parse.Parent?.Range ?? default
                });
            }

            value = QualifiedType.None;
            return false;
        }

        if (fullEnumType.TryParse(val.AsSpan(), val, out ISpecDynamicValue enumVal) && enumVal is ICorrespondingTypeSpecDynamicValue correspondingTypeProvider)
        {
            enumValue = enumVal;
            QualifiedType correspondingType = correspondingTypeProvider.GetCorrespondingType(parse.Database);
            if (!correspondingType.IsNull)
                val = correspondingType.Type;
        }

        value = QualifiedType.None;
        return true;
    }

    /// <inheritdoc />
    public bool Equals(TypeOrEnumSpecPropertyType other) => other != null && ElementType.Equals(other.ElementType);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is TypeOrEnumSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<QualifiedType>? other) => other is TypeOrEnumSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);

    public ValueHoverProviderResult? GetDescription(in SpecPropertyTypeParseContext ctx, ISpecDynamicValue value)
    {
        if (ctx.Node is not IValueSourceNode valueNode)
            return null;

        string val = valueNode.Value;
        if (val.IndexOf('.') >= 0)
            return null;

        if (!TryParseEnum(in ctx, ref val, out QualifiedType v, out ISpecDynamicValue? enumValue)
            || enumValue?.ValueType is not IValueHoverProviderSpecPropertyType valueHoverProvider)
        {
            return null;
        }

        ValueHoverProviderResult? result = valueHoverProvider.GetDescription(in ctx, enumValue);
        if (result == null)
            return null;

        result.CorrespondingType = v;
        return result;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class TypeOrEnumSpecPropertyType :
    BaseSpecPropertyType<QualifiedType>,
    ISpecPropertyType<QualifiedType>,
    IElementTypeSpecPropertyType,
    IEquatable<TypeOrEnumSpecPropertyType>,
    IStringParseableSpecPropertyType,
    ISpecialTypesSpecPropertyType
{
    private IAssetSpecDatabase? _cachedSpecDb;
    private ISpecType? _cachedEnum;

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

    public TypeOrEnumSpecPropertyType(QualifiedType elementType, QualifiedType enumType)
    {
        ElementType = elementType;
        EnumType = enumType;
        DisplayName = elementType.IsNull ? $"Type or {enumType.GetTypeName()}" : $"{elementType.GetTypeName()} or {enumType.GetTypeName()}";
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
        ISpecType? enumType = null;

        if (_cachedSpecDb == parse.Database)
        {
            lock (this)
            {
                if (_cachedSpecDb == parse.Database)
                {
                    enumType = _cachedEnum;
                }
            }
        }

        enumType ??= EnumType.IsNull ? null : parse.Database.FindType(EnumType.Type, parse.FileType);
        lock (this)
        {
            _cachedSpecDb = parse.Database;
            _cachedEnum = enumType;
        }

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

            value = default;
            return false;
        }

        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode stringNode)
        {
            return FailedToParse(in parse, out value);
        }

        string val = stringNode.Value;

        if (val.IndexOf('.') < 0)
        {
            if (fullEnumType.TryParse(val.AsSpan(), val, out ISpecDynamicValue enumVal) && enumVal is ICorrespondingTypeSpecDynamicValue correspondingTypeProvider)
            {
                QualifiedType correspondingType = correspondingTypeProvider.GetCorrespondingType(parse.Database);
                if (!correspondingType.IsNull)
                    val = correspondingType.Type;
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

    /// <inheritdoc />
    public bool Equals(TypeOrEnumSpecPropertyType other) => other != null && ElementType.Equals(other.ElementType);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is TypeOrEnumSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<QualifiedType>? other) => other is TypeOrEnumSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}
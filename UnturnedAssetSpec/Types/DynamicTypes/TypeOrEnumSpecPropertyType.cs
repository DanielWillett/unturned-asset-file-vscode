using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An assembly-qualified type name or enum value.
/// Enums should define a <see cref="EnumSpecTypeValue.CorrespondingType"/> that is used to determine the corresponding type for each enum value.
/// <para>
/// The enum type is specified by the <c>ElementType</c> property and the base type of the Type is specified by the <c>SpecialType[0]</c> property.
/// </para>
/// <para>Example: <c>ItemAsset.Useable</c></para>
/// <code>
/// // type
/// Prop SDG.Unturned.UseableGun, Assembly-CSharp
///
/// // enum
/// Prop Gun
/// </code>
/// </summary>
public sealed class TypeOrEnumSpecPropertyType :
    BaseSpecPropertyType<TypeOrEnumSpecPropertyType, QualifiedType>,
    ISpecPropertyType<QualifiedType>,
    IElementTypeSpecPropertyType,
    IEquatable<TypeOrEnumSpecPropertyType>,
    IStringParseableSpecPropertyType,
    ISpecialTypesSpecPropertyType,
    IValueHoverProviderSpecPropertyType
{
    private readonly IAssetSpecDatabase _database;
    private ISpecType? _enumType;

    public QualifiedType ElementType { get; }
    public QualifiedType EnumType { get; }

    string IElementTypeSpecPropertyType.ElementType => EnumType;
    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => new OneOrMore<string?>(ElementType);

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "TypeOrEnum";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

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

    public TypeOrEnumSpecPropertyType(IAssetSpecDatabase database, QualifiedType elementType, QualifiedType enumType)
    {
        _database = database.ResolveFacade();
        ElementType = elementType;
        EnumType = enumType;
        DisplayName = elementType.IsNull ? $"Type Reference or {enumType.GetTypeName()}" : $"Type Reference of {elementType.GetTypeName()} or {enumType.GetTypeName()}";
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out QualifiedType value)
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
        _enumType ??= _database.FindType(EnumType.Type, parse.FileType);
        enumValue = null;
        if (_enumType is not IStringParseableSpecPropertyType fullEnumType)
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
    public bool Equals(TypeOrEnumSpecPropertyType other) => other != null && ElementType.Equals(other.ElementType) && EnumType.Equals(other.EnumType);

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
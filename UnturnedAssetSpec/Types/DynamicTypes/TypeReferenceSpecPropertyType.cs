using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class TypeReferenceSpecPropertyType :
    BaseSpecPropertyType<QualifiedType>,
    ISpecPropertyType<QualifiedType>,
    IElementTypeSpecPropertyType,
    IEquatable<TypeReferenceSpecPropertyType?>,
    IStringParseableSpecPropertyType
{
    /// <summary>
    /// The base type of the type that should be entered.
    /// </summary>
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "TypeReference";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public Type ValueType => typeof(QualifiedType);

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;

    public TypeReferenceSpecPropertyType(QualifiedType elementType)
    {
        bool isObjectBase = elementType.Type == null
                      || elementType.Equals("System.Object, mscorlib")
                      || elementType.Equals("System.Object, netstandard")
                      || elementType.Equals("System.Object, System.Private.CoreLib")
                      || elementType.Equals("System.Object, System.Runtime");

        DisplayName = isObjectBase
            ? "Type Reference"
            : $"Type Reference of {elementType.GetTypeName()}.";

        ElementType = isObjectBase ? QualifiedType.None : elementType.Normalized;
    }

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<QualifiedType>()?.Type;
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (!QualifiedType.ExtractParts(span, out _, out _))
        {
            dynamicValue = null!;
            return false;
        }

        dynamicValue = new SpecDynamicConcreteValue<QualifiedType>(new QualifiedType(stringValue ?? span.ToString()), this);
        return false;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out QualifiedType type))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteValue<QualifiedType>(type, this);
        return false;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out QualifiedType value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        string? asmQualifiedType;
        FileRange range;

        if (parse.Node is IValueSourceNode stringValue)
        {
            asmQualifiedType = stringValue.Value;
            range = stringValue.Range;
        }
        else if (parse.Node is IDictionarySourceNode dictionary)
        {
            if (!dictionary.TryGetPropertyValue("Type", out IAnyValueSourceNode? node))
            {
                return MissingProperty(in parse, "Type", out value);
            }

            if (node is not IValueSourceNode stringValue2)
            {
                return FailedToParse(in parse, out value);
            }

            asmQualifiedType = stringValue2.Value;
            range = stringValue2.Range;
        }
        else
        {
            return FailedToParse(in parse, out value);
        }

        if (!QualifiedType.ExtractParts(asmQualifiedType.AsSpan(), out _, out _))
        {
            parse.Log(new DatDiagnosticMessage
            {
                Range = range,
                Diagnostic = DatDiagnostics.UNT2013,
                Message = DiagnosticResources.UNT2013
            });
            value = QualifiedType.None;
            return false;
        }

        value = new QualifiedType(asmQualifiedType, false);

        if (parse.HasDiagnostics && ElementType.Type != null)
        {
            InverseTypeHierarchy parents = parse.Database.Information.GetParentTypes(value);
            if (!parents.IsValid || Array.IndexOf(parents.ParentTypes, ElementType) < 0)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = range,
                    Diagnostic = DatDiagnostics.UNT103,
                    Message = string.Format(DiagnosticResources.UNT103, asmQualifiedType, ElementType.Type)
                });
            }
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(TypeReferenceSpecPropertyType? other) => other != null && ElementType.Equals(other.ElementType);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is TypeReferenceSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<QualifiedType>? other) => other is TypeReferenceSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An assembly-qualified CLR type name represented by the <see cref="T:SDG.Unturned.TypeReference{T}"/> type in-game.
/// It can either be formatted as a string or an object.
/// <para>Example: <c>LevelAsset.Default_Game_Mode</c></para>
/// <code>
/// // string
/// Prop SDG.Unturned.SurvivalGameMode, Assembly-CSharp
///
/// // object
/// Prop
/// {
///     Type SDG.Unturned.SurvivalGameMode, Assembly-CSharp
/// }
/// </code>
/// </summary>
public sealed class TypeReferenceSpecPropertyType :
    BaseSpecPropertyType<TypeReferenceSpecPropertyType, QualifiedType>,
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
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;

    public override int GetHashCode()
    {
        return 83 ^ ElementType.GetHashCode();
    }

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

#pragma warning disable CS8500

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out QualifiedType value)
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

        if (asmQualifiedType.Length == 0 || asmQualifiedType.IndexOfAny(KnownTypeValueHelper.InvalidTypeChars) >= 0)
        {
            return FailedToParse(in parse, out value);
        }

        QualifiedType.ExtractParts(asmQualifiedType.AsSpan(), out ReadOnlySpan<char> typeName, out ReadOnlySpan<char> asmName);
        if (asmName.IsEmpty)
        {
            if (parse.HasDiagnostics && !typeName.StartsWith("SDG", StringComparison.Ordinal))
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = range,
                    Diagnostic = DatDiagnostics.UNT1023,
                    Message = DiagnosticResources.UNT1023
                });
            }

            const int defaultAssemblyNameLen = 15;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            unsafe
            {
                ConcatAssemblyNameState state;
                state.Ptr = &typeName;
                value = new QualifiedType(
                    string.Create(typeName.Length + defaultAssemblyNameLen + 2,
                        state,
                        (span, state) =>
                        {
                            state.Ptr->CopyTo(span);
                            int l = state.Ptr->Length;
                            span[l] = ',';
                            span[l + 1] = ' ';
                            ReadOnlySpan<char> defaultAssemblyName = "Assembly-CSharp";
                            defaultAssemblyName.CopyTo(span.Slice(l + 2));
                        })
                    );
            }
#else
            ReadOnlySpan<char> defaultAssemblyName = "Assembly-CSharp";
            Span<char> newTypeName = stackalloc char[typeName.Length + defaultAssemblyNameLen + 2];
            typeName.CopyTo(newTypeName);
            newTypeName[typeName.Length] = ',';
            newTypeName[typeName.Length + 1] = ' ';
            defaultAssemblyName.CopyTo(newTypeName.Slice(typeName.Length + 2));
            value = new QualifiedType(newTypeName.ToString());
#endif
        }
        else
        {
            value = new QualifiedType(asmQualifiedType, false);
        }

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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    private unsafe struct ConcatAssemblyNameState
    {
        public ReadOnlySpan<char>* Ptr;
    }
#endif
#pragma warning restore CS8500

    /// <inheritdoc />
    public bool Equals(TypeReferenceSpecPropertyType? other) => other != null && ElementType.Equals(other.ElementType);
}
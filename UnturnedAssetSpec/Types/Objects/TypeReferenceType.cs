using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An assembly-qualified CLR type name.
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
public sealed class TypeReferenceType : BaseType<QualifiedType, TypeReferenceType>, ITypeParser<QualifiedType>, ITypeFactory, IReferencingType
{
    /// <summary>
    /// Gets the default instance for a <see cref="TypeReferenceType"/>.
    /// </summary>
    [field: MaybeNull]
    public static TypeReferenceType Instance => field ??= new TypeReferenceType();

    /// <summary>
    /// Type ID of this type (<see cref="TypeReferenceType"/>).
    /// </summary>
    public const string TypeId = "TypeReference";

    private readonly OneOrMore<QualifiedType> _baseTypes;
    private readonly DatEnumType? _enumType;
    
    /// <summary>
    /// Factory used to create this type.
    /// </summary>
    public static ITypeFactory Factory => Instance;

    /// <inheritdoc />
    public override string Id => TypeId;

    /// <inheritdoc />
    public override string DisplayName { get; }

    /// <summary>
    /// Type of syntax to accept.
    /// </summary>
    public TypeReferenceKind Kind { get; }

    /// <summary>
    /// Whether or not this kind of type reference is case-sensitive.
    /// </summary>
    public bool IsCaseSensitive => Kind == TypeReferenceKind.Object;

    /// <inheritdoc />
    public OneOrMore<IType> ReferencedTypes => _enumType == null ? OneOrMore<IType>.Null : new OneOrMore<IType>(_enumType);

    /// <inheritdoc />
    public override ITypeParser<QualifiedType> Parser => this;

    public TypeReferenceType() : this(TypeReferenceKind.String, OneOrMore<QualifiedType>.Null) { }

    public TypeReferenceType(TypeReferenceKind kind, OneOrMore<QualifiedType> baseTypes) : this(kind, baseTypes, null!, null!, QualifiedType.None) { }
    public TypeReferenceType(TypeReferenceKind kind, OneOrMore<QualifiedType> baseTypes, DatProperty owner, IDatSpecificationReadContext context, QualifiedType enumType)
    {
        if (kind is < TypeReferenceKind.String or > TypeReferenceKind.Object)
            throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(TypeReferenceKind));
        if (!enumType.IsNull && context == null)
            throw new ArgumentNullException(nameof(context));
        if (!enumType.IsNull && owner == null)
            throw new ArgumentNullException(nameof(owner));

        Kind = kind;
        _baseTypes = baseTypes;
        if (!enumType.IsNull)
        {
            _enumType = context.GetOrReadType(owner, enumType) as DatEnumType;
            if (_enumType == null)
            {
                throw new InvalidOperationException(string.Format(Resources.JsonException_PropertyTypeNotFound, enumType.Type, owner.FullName + ".Type.EnumType"));
            }
        }

        DisplayName = AssetReferenceHelper.GetDisplayName(baseTypes, Resources.Type_Name_TypeReference, Resources.Type_Name_TypeReference_Type, isAsset: false);
        if (_enumType != null)
        {
            DisplayName = string.Format(Resources.Type_Name_TypeReference_TypeOrEnum, _enumType.DisplayName, DisplayName);
        }
    }

    public bool TryParse(ref TypeParserArgs<QualifiedType> args, in FileEvaluationContext ctx, out Optional<QualifiedType> value)
    {
        value = Optional<QualifiedType>.Null;

        switch (args.ValueNode)
        {
            default:
                if (args.MissingValueBehavior != TypeParserMissingValueBehavior.FallbackToDefaultValue)
                {
                    args.DiagnosticSink?.UNT2004_NoValue(ref args, args.ParentNode);
                }
                else
                {
                    if (args.Property?.GetIncludedDefaultValue(args.ParentNode is IPropertySourceNode) is { } defValue)
                    {
                        return defValue.TryGetValueAs(in ctx, out value);
                    }

                    return false;
                }
                break;

            case IListSourceNode l:
                args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, l, args.Type);
                break;

            case IValueSourceNode v:
                if (!TryParseTypeRefValue(ref args, in ctx, v.Value, out QualifiedType type))
                {
                    args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
                    return false;
                }

                value = type;
                return true;

            case IDictionarySourceNode d:
                if (Kind != TypeReferenceKind.Object)
                {
                    args.DiagnosticSink?.UNT2004_TypeReferenceStringOnly(ref args, args.Type, args.ParentNode);
                    return false;
                }

                if (!d.TryGetProperty("Type", out IPropertySourceNode? guidNode))
                {
                    args.DiagnosticSink?.UNT1007(ref args, d, "Type");
                    return false;
                }

                args.ReferencedPropertySink?.AcceptReferencedProperty(guidNode);

                args.CreateSubTypeParserArgs(out TypeParserArgs<QualifiedType> typeArgs, guidNode.Value, guidNode, this, PropertyResolutionContext.Modern);
                switch (guidNode.Value)
                {
                    default:
                        args.DiagnosticSink?.UNT2004_NoValue(ref typeArgs, guidNode);
                        return false;

                    case IListSourceNode list:
                        args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref typeArgs, list, this);
                        return false;

                    case IDictionarySourceNode dict:
                        args.DiagnosticSink?.UNT2004_DictionaryInsteadOfValue(ref typeArgs, dict, this);
                        return false;

                    case IValueSourceNode typeValue:
                        if (!TryParseTypeRefValue(ref typeArgs, in ctx, typeValue.Value, out type))
                        {
                            args.DiagnosticSink?.UNT2004_Generic(ref typeArgs, typeValue.Value, args.Type);
                            return false;
                        }

                        value = type;
                        return true;
                }
        }

        return false;
    }

    private static readonly char[] InvalidTypeChars = [ '\\', ':', '/' ];

#pragma warning disable CS8500
    private bool TryParseTypeRefValue(ref TypeParserArgs<QualifiedType> args, in FileEvaluationContext ctx, string value, out QualifiedType type)
    {
        QualifiedType.ExtractParts(value.AsSpan(), out ReadOnlySpan<char> typeName, out ReadOnlySpan<char> asmName);

        if (Kind == TypeReferenceKind.String && value.IndexOfAny(InvalidTypeChars) >= 0)
        {
            args.DiagnosticSink?.UNT2004_Generic(ref args, value, this);
            type = QualifiedType.None;
            return false;
        }

        if (asmName.IsEmpty)
        {
            if (!typeName.StartsWith("SDG", StringComparison.Ordinal))
            {
                args.DiagnosticSink?.UNT1023(ref args);
            }

            const int defaultAssemblyNameLen = 15;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            unsafe
            {
                ConcatAssemblyNameState state;
                state.Ptr = &typeName;
                type = new QualifiedType(
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
                        }), isCaseInsensitive: !IsCaseSensitive
                    );
            }
#else
            ReadOnlySpan<char> defaultAssemblyName = "Assembly-CSharp";
            Span<char> newTypeName = stackalloc char[typeName.Length + defaultAssemblyNameLen + 2];
            typeName.CopyTo(newTypeName);
            newTypeName[typeName.Length] = ',';
            newTypeName[typeName.Length + 1] = ' ';
            defaultAssemblyName.CopyTo(newTypeName.Slice(typeName.Length + 2));
            type = new QualifiedType(newTypeName.ToString(), isCaseInsensitive: !IsCaseSensitive);
#endif
        }
        else
        {
            type = new QualifiedType(value, !IsCaseSensitive);
        }

        if (args.DiagnosticSink != null && !_baseTypes.IsNull)
        {
            InverseTypeHierarchy parents = ctx.Services.Database.Information.GetParentTypes(value);
            if (!parents.IsValid || !_baseTypes.Any(x => Array.IndexOf(parents.ParentTypes, x) >= 0))
            {
                args.DiagnosticSink?.UNT103(ref args, value, string.Join(", ", _baseTypes.Select(x => x.Type)));
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

    #region JSON

    public bool TryReadValueFromJson<TDataRefReadContext>(
        in JsonElement json,
        out Optional<QualifiedType> value,
        IType<QualifiedType> valueType,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        return TypeParsers.QualifiedType.TryReadValueFromJson(in json, out value, valueType, ref dataRefContext);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, QualifiedType value, IType<QualifiedType> valueType, JsonSerializerOptions options)
    {
        TypeParsers.QualifiedType.WriteValueToJson(writer, value, valueType, options);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        TypeReferenceKind mode = default;
        if (typeDefinition.TryGetProperty("Mode"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
        {
            if (!Enum.TryParse(element.GetString(), ignoreCase: true, out mode))
            {
                throw new JsonException(
                    string.Format(
                        Resources.JsonException_FailedToParseEnum,
                        nameof(TypeReferenceKind),
                        element.GetString(),
                        context.Length != 0 ? $"{owner.FullName}.{context}.Mode" : $"{owner.FullName}.Mode"
                    )
                );
            }
        }

        QualifiedType enumType;
        OneOrMore<QualifiedType> baseTypes;
        if (typeDefinition.TryGetProperty("BaseType"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            baseTypes = new OneOrMore<QualifiedType>(new QualifiedType(element.GetString()!, isCaseInsensitive: true).Normalized);
        }
        else if (typeDefinition.TryGetProperty("BaseTypes"u8, out element)
                 && element.ValueKind != JsonValueKind.Null)
        {
            int len = element.GetArrayLength();
            QualifiedType[] arr = new QualifiedType[len];
            for (int i = 0; i < len; ++i)
            {
                arr[i] = new QualifiedType(element[i].GetString()!, isCaseInsensitive: true).Normalized;
            }

            baseTypes = new OneOrMore<QualifiedType>(arr);
        }
        else
        {
            baseTypes = OneOrMore<QualifiedType>.Null;
        }

        if (typeDefinition.TryGetProperty("EnumType"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            enumType = new QualifiedType(element.GetString()!, isCaseInsensitive: true).Normalized;
        }
        else
        {
            enumType = QualifiedType.None;
        }

        if (!enumType.IsNull)
        {
            return new TypeReferenceType(mode, baseTypes, owner, spec, enumType);
        }

        return baseTypes.IsNull && mode == TypeReferenceKind.String ? Instance : new TypeReferenceType(mode, baseTypes);
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (_enumType == null && _baseTypes.IsNull && Kind == TypeReferenceKind.String)
        {
            writer.WriteStringValue(Id);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);

        if (Kind != TypeReferenceKind.String)
        {
            writer.WriteString("Mode"u8, Kind.ToString());
        }

        if (!_baseTypes.IsNull)
        {
            if (_baseTypes.IsSingle)
            {
                writer.WriteString("BaseType"u8, _baseTypes[0].Type);
            }
            else
            {
                writer.WritePropertyName("BaseTypes"u8);
                writer.WriteStartArray();
                foreach (QualifiedType t in _baseTypes)
                    writer.WriteStringValue(t.Type);
                writer.WriteEndArray();
            }
        }

        if (_enumType != null)
        {
            writer.WriteString("EnumType"u8, _enumType.TypeName.Type);
        }

        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(TypeReferenceType other)
    {
        return other.Kind == Kind && (_enumType?.Equals(other._enumType) ?? other._enumType == null) && other._baseTypes.Equals(_baseTypes);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(1167138807, Kind, _enumType, _baseTypes.Value);
    }
}

/// <summary>
/// Specifies how a <see cref="TypeReferenceType"/> is parsed.
/// </summary>
public enum TypeReferenceKind
{
    /// <summary>
    /// String-only. This type of TypeReference is case-insensitive.
    /// <code>
    /// Prop SDG.Unturned.SurvivalGameMode, Assembly-CSharp
    /// </code>
    /// </summary>
    /// <remarks>Corresponds to the <c>DatValueEx.ParseType</c> extension method.</remarks>
    String,

    /// <summary>
    /// String or object. This type of TypeReference is case-sensitive.
    /// <code>
    /// Prop SDG.Unturned.SurvivalGameMode, Assembly-CSharp
    ///
    /// // or
    /// 
    /// Prop
    /// {
    ///     Type SDG.Unturned.SurvivalGameMode, Assembly-CSharp
    /// }
    /// </code>
    /// </summary>
    /// <remarks>Corresponds to the <c>TypeReference&lt;T&gt;</c> type in-game.</remarks>
    Object
}
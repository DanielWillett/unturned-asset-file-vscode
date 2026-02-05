using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A reference to content in a masterbundle. Supports various different formats used around the game.
/// <para>Example: <c>LevelAsset.Death_Music</c></para>
/// <code>
/// // audio reference
/// core.masterbundle::Sounds/Inventory/Equip.ogg
/// 
/// // masterbundle reference
/// core.masterbundle::Bundles/Ace/Item.prefab
/// // - or -
/// {
///     MasterBundle "core.masterbundle"
///     AssetPath "Bundles/Ace/Item.prefab"
/// }
/// 
/// // content reference
/// core.masterbundle::Bundles/Ace/Item.prefab
/// // - or -
/// {
///     Name "core.masterbundle"
///     Path "Bundles/Ace/Item.prefab"
/// }
/// 
/// // translation reference
/// SDG::Stereo_Songs.Unturned_Theme.Title
/// // - or -
/// SDG#Stereo_Songs.Unturned_Theme.Title
/// // - or -
/// {
///     Namespace SDG
///     Token Stereo_Songs.Unturned_Theme.Title
/// }
/// </code>
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="QualifiedType"/> BaseType or <see cref="QualifiedType"/>[] BaseTypes</c> - One or more allowed base types.</item>
/// </list>
/// </para>
/// </summary>
public sealed class BundleReferenceType : BaseType<BundleReference, BundleReferenceType>, ITypeParser<BundleReference>, ITypeFactory
{
    private static readonly BundleReferenceType?[] DefaultInstances = new BundleReferenceType?[(int)BundleReferenceKind.TranslationReference];

    /// <summary>
    /// Gets the default instance of a certain kind of bundle reference.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static BundleReferenceType GetInstance(BundleReferenceKind kind)
    {
        if (kind is <= BundleReferenceKind.Unspecified or > BundleReferenceKind.TranslationReference)
            throw new ArgumentOutOfRangeException(nameof(kind));
        ref BundleReferenceType? t = ref DefaultInstances[(int)kind - 1];
        BundleReferenceType? r = t;
        if (r != null)
            return r;

        Interlocked.CompareExchange(ref t, new BundleReferenceType(kind), null);
        return t;
    }

    private static readonly string[] DisplayNames =
    [
        Resources.Type_Name_MasterBundleReference,
        Resources.Type_Name_MasterBundleReference,
        Resources.Type_Name_MasterBundleReferenceString,
        Resources.Type_Name_ContentReference,
        Resources.Type_Name_AudioReference,
        Resources.Type_Name_MasterBundleOrContentReference,
        Resources.Type_Name_TranslationReference
    ];

    /// <summary>
    /// Type IDs of this type indexed by <see cref="BundleReferenceKind"/>.
    /// </summary>
    public static readonly ImmutableArray<string> TypeIds = ImmutableArray.Create<string>
    (
        "MasterBundleReference",
        "MasterBundleReference",
        "MasterBundleReferenceString",
        "ContentReference",
        "AudioReference",
        "MasterBundleOrContentReference",
        "TranslationReference"
    );

    public static ITypeFactory Factory => GetInstance(BundleReferenceKind.MasterBundleReference);

    private readonly OneOrMore<QualifiedType> _baseTypes;

    public BundleReferenceKind Kind { get; }

    public override string Id => TypeIds[(int)Kind];

    public override string DisplayName => DisplayNames[(int)Kind];

    public override ITypeParser<BundleReference> Parser => this;

    public BundleReferenceType() : this(BundleReferenceKind.MasterBundleReference) { }

    public BundleReferenceType(BundleReferenceKind kind)
    {
        if (kind is <= BundleReferenceKind.Unspecified or > BundleReferenceKind.TranslationReference)
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
        _baseTypes = OneOrMore<QualifiedType>.Null;
    }

    public BundleReferenceType(BundleReferenceKind kind, OneOrMore<QualifiedType> baseTypes)
    {
        if (kind is <= BundleReferenceKind.Unspecified or > BundleReferenceKind.TranslationReference)
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
        _baseTypes = baseTypes;
    }

    public bool TryParse(ref TypeParserArgs<BundleReference> args, in FileEvaluationContext ctx, out Optional<BundleReference> value)
    {
        value = Optional<BundleReference>.Null;

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
                string? name, path;
                if (Kind == BundleReferenceKind.TranslationReference)
                {
                    if (!KnownTypeValueHelper.TryParseTranslationReference(v.Value, out name, out path))
                    {
                        args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
                        return false;
                    }

                    args.DiagnosticSink?.UNT1018(ref args);
                    value = new BundleReference(name, path, BundleReferenceKind.TranslationReference);
                    return true;
                }

                if (!KnownTypeValueHelper.TryParseMasterBundleReference(v.Value, out name, out path))
                {
                    args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
                    return false;
                }

                BundleReferenceKind rType = Kind;
                if (rType is BundleReferenceKind.MasterBundleReference or BundleReferenceKind.MasterBundleOrContentReference)
                    rType = BundleReferenceKind.MasterBundleReferenceString;

                value = new BundleReference(name, path, rType);
                return true;

            case IDictionarySourceNode d:
                if (Kind == BundleReferenceKind.TranslationReference)
                {
                    args.DiagnosticSink?.UNT1018(ref args);
                }

                if (Kind is BundleReferenceKind.AudioReference or BundleReferenceKind.MasterBundleReferenceString)
                {
                    args.DiagnosticSink?.UNT2004_BundleReferenceStringOnly(ref args, args.Type, args.ParentNode);
                    return false;
                }

                if (Kind != BundleReferenceKind.MasterBundleOrContentReference)
                {
                    if (TryParseRefObject(d, ref args, out BundleReference br, Kind, diagnostics: true))
                    {
                        value = br;
                        return true;
                    }
                }
                else
                {
                    if (TryParseRefObject(d, ref args, out BundleReference br, BundleReferenceKind.MasterBundleReference, diagnostics: false))
                    {
                        value = br;
                        return true;
                    }

                    if (TryParseRefObject(d, ref args, out br, BundleReferenceKind.ContentReference, diagnostics: false))
                    {
                        args.DiagnosticSink?.UNT104(ref args);
                        value = br;
                        return true;
                    }

                    // reparse with diagnostics
                    _ = TryParseRefObject(d, ref args, out br, BundleReferenceKind.MasterBundleReference, diagnostics: true);
                    return false;
                }

                break;
        }

        return false;
    }

    private bool TryParseRefObject(
        IDictionarySourceNode dictionary,
        ref TypeParserArgs<BundleReference> args,
        out BundleReference value,
        BundleReferenceKind rType,
        bool diagnostics)
    {
        string nameProperty = Kind switch
        {
            BundleReferenceKind.ContentReference => "Name",
            BundleReferenceKind.TranslationReference => "Namespace",
            _ => "MasterBundle"
        };
        string pathProperty = Kind switch
        {
            BundleReferenceKind.ContentReference => "Path",
            BundleReferenceKind.TranslationReference => "Token",
            _ => "AssetPath"
        };

        dictionary.TryGetPropertyValue(nameProperty, out IValueSourceNode? nameNode);

        if (!dictionary.TryGetProperty(pathProperty, out IPropertySourceNode? pathNode))
        {
            if (diagnostics)
            {
                args.DiagnosticSink?.UNT1007(ref args, dictionary, pathProperty);
            }
            if (nameNode != null)
                args.ReferencedPropertySink?.AcceptReferencedProperty((IPropertySourceNode)nameNode.Parent);
            value = default;
            return false;
        }

        if (!pathNode.HasValue || pathNode.ValueKind != SourceValueType.Value)
        {
            value = new BundleReference(string.Empty, string.Empty, rType);
            return false;
        }

        value = new BundleReference(nameNode?.Value ?? string.Empty, pathNode.GetValueString(out _)!, rType);

        if (nameNode != null)
            args.ReferencedPropertySink?.AcceptReferencedProperty((IPropertySourceNode)nameNode.Parent);

        args.ReferencedPropertySink?.AcceptReferencedProperty(pathNode);
        
        if (diagnostics)
            args.DiagnosticSink?.UNT108(ref args);

        return true;
    }

    #region JSON

    public bool TryReadValueFromJson(in JsonElement json, out Optional<BundleReference> value, IType<BundleReference> valueType)
    {
        if (TypeParsers.String.TryReadValueFromJson(in json, out Optional<string> strValue, StringType.Instance))
        {
            if (!strValue.HasValue)
            {
                value = Optional<BundleReference>.Null;
                return true;
            }

            if (KnownTypeValueHelper.TryParseMasterBundleReference(strValue.Value, out string name, out string path))
            {
                value = new Optional<BundleReference>(new BundleReference(name, path, Kind));
                return true;
            }
        }

        value = Optional<BundleReference>.Null;
        return false;
    }

    public void WriteValueToJson(Utf8JsonWriter writer, BundleReference value, IType<BundleReference> valueType, JsonSerializerOptions options)
    {
        TypeParsers.String.WriteValueToJson(writer, value.ToString(), StringType.Instance, options);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        BundleReferenceKind mode = BundleReferenceKind.Unspecified;
        for (int i = 1; i < TypeIds.Length; ++i)
        {
            if (!typeId.Equals(TypeIds[i], StringComparison.Ordinal))
                continue;

            mode = (BundleReferenceKind)i;
            break;
        }

        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return new BundleReferenceType(mode);
        }

        OneOrMore<QualifiedType> baseTypes;
        if (typeDefinition.TryGetProperty("BaseType"u8, out JsonElement element)
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

        return baseTypes.IsNull
            ? GetInstance(mode)
            : new BundleReferenceType(mode, baseTypes);
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (_baseTypes.IsNull)
        {
            writer.WriteStringValue(Id);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);
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

        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(BundleReferenceType other)
    {
        return other.Kind == Kind && other._baseTypes.Equals(_baseTypes);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(317512674, Kind, _baseTypes);
    }
}
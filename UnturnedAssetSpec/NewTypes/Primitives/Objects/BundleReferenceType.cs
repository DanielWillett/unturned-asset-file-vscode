using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.NewTypes;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
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
    private static readonly BundleReferenceType?[] DefaultInstances = new BundleReferenceType?[(int)BundleReferenceMode.TranslationReference];

    private static BundleReferenceType GetInstance(BundleReferenceMode mode)
    {
        ref BundleReferenceType? t = ref DefaultInstances[(int)mode - 1];
        BundleReferenceType? r = t;
        if (r != null)
            return r;

        Interlocked.CompareExchange(ref t, new BundleReferenceType(mode), null);
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
    /// Type IDs of this type indexed by <see cref="BundleReferenceMode"/>.
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

    public static ITypeFactory Factory => GetInstance(BundleReferenceMode.MasterBundleReference);

    private readonly OneOrMore<QualifiedType> _baseTypes;

    public BundleReferenceMode Mode { get; }

    public override string Id => TypeIds[(int)Mode];

    public override string DisplayName => DisplayNames[(int)Mode];

    public override ITypeParser<BundleReference> Parser => this;

    public BundleReferenceType() : this(BundleReferenceMode.MasterBundleReference) { }

    public BundleReferenceType(BundleReferenceMode mode)
    {
        if (mode is <= BundleReferenceMode.Unspecified or > BundleReferenceMode.TranslationReference)
            throw new ArgumentOutOfRangeException(nameof(mode));

        Mode = mode;
        _baseTypes = OneOrMore<QualifiedType>.Null;
    }

    public BundleReferenceType(BundleReferenceMode mode, OneOrMore<QualifiedType> baseTypes)
    {
        if (mode is <= BundleReferenceMode.Unspecified or > BundleReferenceMode.TranslationReference)
            throw new ArgumentOutOfRangeException(nameof(mode));

        Mode = mode;
        _baseTypes = baseTypes;
    }

    public bool TryParse(ref TypeParserArgs<BundleReference> args, in FileEvaluationContext ctx, out Optional<BundleReference> value)
    {
        value = Optional<BundleReference>.Null;

        switch (args.ValueNode)
        {
            case null:
                args.DiagnosticSink?.UNT2004_NoValue(ref args, args.ParentNode);
                break;

            case IListSourceNode l:
                args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, l, args.Type);
                break;

            case IValueSourceNode v:
                string? name, path;
                if (Mode == BundleReferenceMode.TranslationReference)
                {
                    if (!KnownTypeValueHelper.TryParseTranslationReference(v.Value, out name, out path))
                    {
                        args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
                        return false;
                    }

                    args.DiagnosticSink?.UNT1018(ref args);
                    value = new BundleReference(name, path, BundleReferenceMode.TranslationReference);
                    return true;
                }

                if (!KnownTypeValueHelper.TryParseMasterBundleReference(v.Value, out name, out path))
                {
                    args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
                    return false;
                }

                BundleReferenceMode rType = Mode;
                if (rType is BundleReferenceMode.MasterBundleReference or BundleReferenceMode.MasterBundleOrContentReference)
                    rType = BundleReferenceMode.MasterBundleReferenceString;

                value = new BundleReference(name, path, rType);
                return true;

            case IDictionarySourceNode d:
                if (Mode == BundleReferenceMode.TranslationReference)
                {
                    args.DiagnosticSink?.UNT1018(ref args);
                }

                if (Mode is BundleReferenceMode.AudioReference or BundleReferenceMode.MasterBundleReferenceString)
                {
                    args.DiagnosticSink?.UNT2004_BundleReferenceStringOnly(ref args, args.Type, args.ParentNode);
                    return false;
                }

                if (Mode != BundleReferenceMode.MasterBundleOrContentReference)
                {
                    if (TryParseRefObject(d, ref args, out BundleReference br, Mode, diagnostics: true))
                    {
                        value = br;
                        return true;
                    }
                }
                else
                {
                    if (TryParseRefObject(d, ref args, out BundleReference br, BundleReferenceMode.MasterBundleReference, diagnostics: false))
                    {
                        value = br;
                        return true;
                    }

                    if (TryParseRefObject(d, ref args, out br, BundleReferenceMode.ContentReference, diagnostics: false))
                    {
                        args.DiagnosticSink?.UNT104(ref args);
                        value = br;
                        return true;
                    }

                    // reparse with diagnostics
                    _ = TryParseRefObject(d, ref args, out br, BundleReferenceMode.MasterBundleReference, diagnostics: true);
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
        BundleReferenceMode rType,
        bool diagnostics)
    {
        string nameProperty = Mode switch
        {
            BundleReferenceMode.ContentReference => "Name",
            BundleReferenceMode.TranslationReference => "Namespace",
            _ => "MasterBundle"
        };
        string pathProperty = Mode switch
        {
            BundleReferenceMode.ContentReference => "Path",
            BundleReferenceMode.TranslationReference => "Token",
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
                args.ReferencedPropertySink?.AcceptReferencedDiagnostic((IPropertySourceNode)nameNode.Parent);
            value = default;
            return false;
        }

        if (!pathNode.HasValue || pathNode.ValueKind != ValueTypeDataRefType.Value)
        {
            value = new BundleReference(string.Empty, string.Empty, rType);
            return false;
        }

        value = new BundleReference(nameNode?.Value ?? string.Empty, pathNode.GetValueString(out _)!, rType);

        if (nameNode != null)
            args.ReferencedPropertySink?.AcceptReferencedDiagnostic((IPropertySourceNode)nameNode.Parent);

        args.ReferencedPropertySink?.AcceptReferencedDiagnostic(pathNode);
        
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
                value = new Optional<BundleReference>(new BundleReference(name, path, Mode));
                return true;
            }
        }

        value = Optional<BundleReference>.Null;
        return false;
    }

    public void WriteValueToJson(Utf8JsonWriter writer, BundleReference value, IType<BundleReference> valueType)
    {
        TypeParsers.String.WriteValueToJson(writer, value.ToString(), StringType.Instance);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context)
    {
        BundleReferenceMode mode = BundleReferenceMode.Unspecified;
        for (int i = 0; i < TypeIds.Length; ++i)
        {
            if (!typeId.Equals(TypeIds[i], StringComparison.Ordinal))
                continue;

            mode = (BundleReferenceMode)i;
            break;
        }

        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return new BundleReferenceType(mode);
        }

        // todo: support switches
        OneOrMore<QualifiedType> baseTypes;
        if (typeDefinition.TryGetProperty("BaseType"u8, out JsonElement element)
            && element.ValueKind != JsonValueKind.Null)
        {
            baseTypes = new OneOrMore<QualifiedType>(new QualifiedType(element.GetString()!, isCaseInsensitive: true));
        }
        else if (typeDefinition.TryGetProperty("BaseTypes"u8, out element)
                 && element.ValueKind != JsonValueKind.Null)
        {
            int len = element.GetArrayLength();
            QualifiedType[] arr = new QualifiedType[len];
            for (int i = 0; i < len; ++i)
            {
                arr[i] = new QualifiedType(element[i].GetString(), isCaseInsensitive: true);
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
        return other.Mode == Mode && other._baseTypes.Equals(_baseTypes);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(317512674, Mode, _baseTypes);
    }
}
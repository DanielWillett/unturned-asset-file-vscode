using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A reference to one or more types of assets formatted as either a string or sometimes as an object (see <see cref="CanParseDictionary"/>).
/// Only accepts <see cref="Guid"/> IDs, not <see cref="ushort"/> IDs.
/// <para>Example: <c>AirdropAsset.Landed_Barricade</c></para>
/// <code>
/// // string
/// Prop fe71781c60314468b22c6b0642a51cd9
///
/// // object
/// Prop
/// {
///     GUID fe71781c60314468b22c6b0642a51cd9
/// }
///
/// // this
/// Prop this
/// </code>
/// <para>
/// Also supports the <c>PreventSelfReference</c> additional property to log a warning if the current asset is referenced.
/// </para>
/// <para>
/// If "this" is one of the element types, the word 'this' will be resolved to the current asset.
/// </para>
/// <para>
/// If an amount is supppled (i.e. "102 x 3") a warning will be logged.
/// </para>
/// </summary>
public sealed class AssetReferenceSpecPropertyType :
    BasicSpecPropertyType<AssetReferenceSpecPropertyType, Guid>,
    ISpecPropertyType<Guid>,
    IElementTypeSpecPropertyType,
    ISpecialTypesSpecPropertyType,
    IEquatable<AssetReferenceSpecPropertyType?>,
    IStringParseableSpecPropertyType,
    IValueHoverProviderSpecPropertyType
{
    private readonly IAssetSpecDatabase _database;

    /// <summary>
    /// Other valid asset types.
    /// </summary>
    public OneOrMore<QualifiedType> OtherElementTypes { get; }

    /// <summary>
    /// Whether or not objects can be parsed.
    /// </summary>
    public bool CanParseDictionary { get; }

    /// <summary>
    /// If the word 'this' can be used to refer to the current asset.
    /// </summary>
    public bool SupportsThis { get; }

    /// <summary>
    /// Primary asset type.
    /// </summary>
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "AssetReference";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;
    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => OtherElementTypes.Select<string?>(x => x.Type);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<Guid>()?.ToString("N");
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (Guid.TryParse(stringValue ?? span.ToString(), out Guid result))
        {
            dynamicValue = SpecDynamicValue.Guid(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    public override int GetHashCode()
    {
        // 55 - 58
        return (55 + (CanParseDictionary ? 1 : 0) + (SupportsThis ? 1 : 0) * 2)
               ^ HashCode.Combine(ElementType, OtherElementTypes);
    }

    public AssetReferenceSpecPropertyType(IAssetSpecDatabase database, QualifiedType elementType, bool canParseDictionary, OneOrMore<string> specialTypes)
    {
        _database = database.ResolveFacade();
        CanParseDictionary = canParseDictionary;

        OtherElementTypes = NormalizeAssetTypes(ref elementType, specialTypes, out bool supportsThis);
        SupportsThis = supportsThis;
        ElementType = elementType;

        DisplayName = CreateDisplayName("Asset Reference", ElementType, OtherElementTypes);
    }

    internal static OneOrMore<QualifiedType> NormalizeAssetTypes(ref QualifiedType elementType, OneOrMore<string> specialTypes, out bool supportsThis)
    {
        supportsThis = ExtractThisElementType(ref elementType, ref specialTypes);

        if (specialTypes.Contains(QualifiedType.AssetBaseType.Type) || elementType == QualifiedType.AssetBaseType)
        {
            specialTypes = OneOrMore<string>.Null;
            elementType = QualifiedType.AssetBaseType;
        }

        if (elementType.Type == null || elementType.Equals(QualifiedType.AssetBaseType))
        {
            elementType = QualifiedType.AssetBaseType;
        }

        return specialTypes
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => new QualifiedType(x))
            .Remove(elementType);
    }

    [Pure]
    internal static string CreateDisplayName(string typeName, QualifiedType elementType, OneOrMore<QualifiedType> otherElementTypes, string joinWord = "to")
    {
        switch (otherElementTypes.Length)
        {
            case 0:
                return elementType == QualifiedType.AssetBaseType
                    ? typeName
                    : $"{typeName} {joinWord} {elementType.GetTypeName()}";

            case 1:
                return elementType == QualifiedType.AssetBaseType
                    ? $"{typeName} {joinWord} {otherElementTypes[0].GetTypeName()}"
                    : $"{typeName} {joinWord} {elementType.GetTypeName()} or {otherElementTypes[0].GetTypeName()}";

            default:
                StringBuilder sb = new StringBuilder(typeName).Append(' ').Append(joinWord).Append(' ');

                int ct;
                if (elementType != QualifiedType.AssetBaseType)
                {
                    sb.Append(elementType.GetTypeName());
                    ct = 1;
                }
                else ct = 0;
                for (int i = 0; i < otherElementTypes.Length; i++)
                {
                    QualifiedType t = otherElementTypes[i];
                    if (ct == otherElementTypes.Length - 1)
                        sb.Append(ct == 1 ? " or " : ", or ");
                    else if (ct != 0)
                        sb.Append(", ");

                    sb.Append(t.GetTypeName());
                    ++ct;
                }

                return sb.ToString();
        }
    }

    internal static bool ExtractThisElementType(ref QualifiedType elementType, ref OneOrMore<string> specialTypes)
    {
        if (!string.Equals(elementType.Type, "this", StringComparison.OrdinalIgnoreCase))
        {
            int l1 = specialTypes.Length;
            specialTypes = specialTypes.Remove("this", StringComparison.OrdinalIgnoreCase);
            return l1 > specialTypes.Length;
        }

        switch (specialTypes.Length)
        {
            case 0:
                elementType = QualifiedType.None;
                break;

            case 1:
                elementType = new QualifiedType(specialTypes[0]);
                specialTypes = OneOrMore<string>.Null;
                break;

            default:
                elementType = new QualifiedType(specialTypes[0]);
                specialTypes = specialTypes.Remove(elementType.Type);
                break;
        }

        return true;
    }

    internal static void CheckInappropriateAmount(in SpecPropertyTypeParseContext parse, IValueSourceNode stringValue)
    {
        if (!parse.HasDiagnostics)
            return;

        int indexOfX = stringValue.Value.IndexOf('x');
        if (indexOfX != -1)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1017,
                Message = DiagnosticResources.UNT1017,
                Range = stringValue.Range with
                {
                    Start = new FilePosition(stringValue.Range.Start.Line, stringValue.Range.Start.Character + indexOfX)
                }
            });
        }
    }

    private bool? _isTypeValid;

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out Guid value)
    {
        if (!_isTypeValid.HasValue)
        {
            InverseTypeHierarchy parents = _database.Information.GetParentTypes(ElementType);
            _isTypeValid = parents.IsValid;
        }

        if (!_isTypeValid.Value)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2005,
                Message = string.Format(DiagnosticResources.UNT2005, $"AssetReference<{QualifiedType.ExtractTypeName(ElementType.Type.AsSpan()).ToString()}>"),
                Range = parse.Node?.Range ?? parse.Parent?.Range ?? default
            });
            value = Guid.Empty;
            return false;
        }

        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is IValueSourceNode stringValue)
        {
            if (SupportsThis && string.Equals(stringValue.Value, "this", StringComparison.InvariantCultureIgnoreCase))
            {
                if (parse.File is IAssetSourceFile { Guid: { } guid } && guid != Guid.Empty)
                {
                    value = guid;

                    CheckSelfRef(in parse, guid, stringValue);

                    return true;
                }

                if (parse.HasDiagnostics)
                {
                    parse.Log(new DatDiagnosticMessage
                    {
                        Diagnostic = DatDiagnostics.UNT2010,
                        Message = DiagnosticResources.UNT2010,
                        Range = stringValue.Range
                    });
                }

                value = Guid.Empty;
                return false;
            }

            CheckInappropriateAmount(in parse, stringValue);

            return ParseGuidFromStringValue(stringValue, in parse, out value);
        }

        if (!CanParseDictionary || parse.Node is not IDictionarySourceNode dictionary)
        {
            return FailedToParse(in parse, out value);
        }

        if (!dictionary.TryGetPropertyValue("GUID", out IAnyValueSourceNode? node))
        {
            return MissingProperty(in parse, "GUID", out value);
        }

        if (node is not IValueSourceNode stringValue2)
        {
            return FailedToParse(in parse, out value);
        }

        return ParseGuidFromStringValue(stringValue2, in parse, out value);
    }

    private bool ParseGuidFromStringValue(IValueSourceNode stringValue, in SpecPropertyTypeParseContext parse, out Guid value)
    {
        if (!KnownTypeValueHelper.TryParseGuid(stringValue.Value, out value))
        {
            if (stringValue.Value.Equals("0", StringComparison.Ordinal))
            {
                value = Guid.Empty;
            }
            else
            {
                return FailedToParse(in parse, out value);
            }
        }
        else if (parse.HasDiagnostics && SupportsThis && parse.File is IAssetSourceFile { Guid: { } guid } && guid == value)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT101,
                Message = DiagnosticResources.UNT101,
                Range = stringValue.Range
            });
        }

        CheckSelfRef(in parse, value, stringValue);

        return true;
    }

    internal static bool GetPreventSelfRef(in SpecPropertyTypeParseContext parse)
    {
        return parse.HasDiagnostics && parse.EvaluationContext.Self.TryGetAdditionalProperty("PreventSelfReference", out bool s) && s;
    }

    internal static void CheckSelfRef(in SpecPropertyTypeParseContext parse, Guid value, IValueSourceNode node, bool? preventSelfRef = null)
    {
        bool setting = preventSelfRef ?? GetPreventSelfRef(in parse);
        if (!setting)
            return;

        if (parse.File is IAssetSourceFile { Guid: { } thisGuid } && thisGuid != Guid.Empty && thisGuid == value)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1019,
                Message = DiagnosticResources.UNT1019,
                Range = node.Range
            });
        }
    }

    internal static void CheckSelfRef(in SpecPropertyTypeParseContext parse, ushort id, int assetCategory, IValueSourceNode node, bool? preventSelfRef = null)
    {
        if (assetCategory == 0)
            return;

        bool setting = preventSelfRef ?? GetPreventSelfRef(in parse);
        if (!setting)
            return;

        if (parse.File is not IAssetSourceFile { Id: { } thisId } assetFile || thisId == 0 || thisId != id)
            return;

        AssetCategoryValue cat = assetFile.Category;
        if (cat.Index == 0 || cat.Index != assetCategory)
            return;

        parse.Log(new DatDiagnosticMessage
        {
            Diagnostic = DatDiagnostics.UNT1019,
            Message = DiagnosticResources.UNT1019,
            Range = node.Range
        });
    }

    /// <inheritdoc />
    public bool Equals(AssetReferenceSpecPropertyType? other) => other != null && ElementType.Equals(other.ElementType) && OtherElementTypes.Equals(other.OtherElementTypes);

    /// <inheritdoc />
    public ValueHoverProviderResult? GetDescription(in SpecPropertyTypeParseContext ctx, ISpecDynamicValue value)
    {
        Guid? guid;
        try
        {
            guid = value.AsConcreteNullable<Guid>();
        }
        catch (InvalidCastException)
        {
            return null;
        }

        if (!guid.HasValue)
            return null;

        OneOrMore<DiscoveredDatFile> files = ctx.EvaluationContext.Environment.FindFile(guid.Value);
        if (files.Length == 1)
        {
            DiscoveredDatFile file = files[0];
            string displayName = file.GetDisplayName();

            string fileUri = new Uri(file.FilePath).AbsoluteUri;

            return new ValueHoverProviderResult(
                displayName,
                file.Type,
                null,
                ReferenceEquals(displayName, file.AssetName) ? null : file.AssetName,
                null,
                fileUri,
                false,
                QualifiedType.None
            )
            {
                LinkName = Path.GetFileName(file.FilePath)
            };
        }
        else
        {
            return null;
        }
    }
}
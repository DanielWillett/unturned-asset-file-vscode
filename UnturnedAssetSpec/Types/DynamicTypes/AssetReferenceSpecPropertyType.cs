using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class AssetReferenceSpecPropertyType :
    BaseSpecPropertyType<Guid>,
    ISpecPropertyType<Guid>,
    IElementTypeSpecPropertyType,
    ISpecialTypesSpecPropertyType,
    IEquatable<AssetReferenceSpecPropertyType?>,
    IStringParseableSpecPropertyType
{
    public OneOrMore<QualifiedType> OtherElementTypes { get; }
    public bool CanParseDictionary { get; }
    public bool SupportsThis { get; }
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "AssetReference";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public Type ValueType => typeof(Guid);

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;
    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => OtherElementTypes.Select<string?>(x => x.Type);

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

    public AssetReferenceSpecPropertyType(QualifiedType elementType, bool canParseDictionary, OneOrMore<string> specialTypes)
    {
        CanParseDictionary = canParseDictionary;

        SupportsThis = ExtractThisElementType(ref elementType, ref specialTypes);

        if (specialTypes.Contains(QualifiedType.AssetBaseType.Type) || elementType == QualifiedType.AssetBaseType)
        {
            specialTypes = OneOrMore<string>.Null;
            elementType = QualifiedType.AssetBaseType;
        }

        if (elementType.Type == null || elementType.Equals(QualifiedType.AssetBaseType))
        {
            ElementType = QualifiedType.AssetBaseType;
        }
        else
        {
            ElementType = elementType;
        }

        OtherElementTypes = specialTypes
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => new QualifiedType(x));

        if (DisplayName != null)
            return;

        switch (OtherElementTypes.Length)
        {
            case 0:
                if (ElementType == QualifiedType.AssetBaseType)
                {
                    DisplayName = "Asset Reference";
                }
                else
                {
                    DisplayName = $"Asset Reference to {ElementType.GetTypeName()}";
                }
                break;

            case 1:
                if (ElementType == QualifiedType.AssetBaseType)
                {
                    DisplayName = $"Asset Reference to {OtherElementTypes[0].GetTypeName()}";
                }
                else
                {
                    DisplayName = $"Asset Reference to {ElementType.GetTypeName()} or {OtherElementTypes[0].GetTypeName()}";
                }
                break;

            default:
                StringBuilder sb = new StringBuilder("Asset Reference to ");

                int ct;
                if (ElementType != QualifiedType.AssetBaseType)
                {
                    sb.Append(ElementType.GetTypeName());
                    ct = 1;
                }
                else ct = 0;
                for (int i = 0; i < OtherElementTypes.Length; i++)
                {
                    QualifiedType t = OtherElementTypes[i];
                    if (ct == OtherElementTypes.Length - 1)
                        sb.Append(ct == 1 ? " or " : ", or ");
                    else if (ct != 0)
                        sb.Append(", ");

                    sb.Append(t.GetTypeName());
                    ++ct;
                }

                DisplayName = sb.ToString();
                break;
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

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out Guid val))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteValue<Guid>(val, this);
        return true;
    }

    internal static void CheckInappropriateAmount(in SpecPropertyTypeParseContext parse, AssetFileStringValueNode stringValue)
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

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out Guid value)
    {
        InverseTypeHierarchy parents = parse.Database.Information.GetParentTypes(ElementType);
        if (!parents.IsValid)
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

        if (parse.Node is AssetFileStringValueNode stringValue)
        {
            if (SupportsThis && string.Equals(stringValue.Value, "this", StringComparison.InvariantCultureIgnoreCase))
            {
                if (parse.File != null && parse.File.GetGuid() is { } guid && guid != Guid.Empty)
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

            if (!KnownTypeValueHelper.TryParseGuid(stringValue.Value, out value))
            {
                return FailedToParse(in parse, out value);
            }

            CheckSelfRef(in parse, value, stringValue);

            return true;
        }

        if (!CanParseDictionary || parse.Node is not AssetFileDictionaryValueNode dictionary)
        {
            return FailedToParse(in parse, out value);
        }

        if (!dictionary.TryGetValue("GUID", out AssetFileValueNode? node))
        {
            return MissingProperty(in parse, "GUID", out value);
        }

        if (node is not AssetFileStringValueNode stringValue2 || !KnownTypeValueHelper.TryParseGuid(stringValue2.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        CheckSelfRef(in parse, value, stringValue2);

        return true;
    }

    internal static bool GetPreventSelfRef(in SpecPropertyTypeParseContext parse)
    {
        return parse.HasDiagnostics && parse.EvaluationContext.Self.TryGetAdditionalProperty("PreventSelfReference", out bool s) && s;
    }

    internal static void CheckSelfRef(in SpecPropertyTypeParseContext parse, Guid value, AssetFileNode node, bool? preventSelfRef = null)
    {
        bool setting = preventSelfRef ?? GetPreventSelfRef(in parse);
        if (!setting)
            return;

        if (parse.File != null && parse.File.GetGuid() is { } thisGuid && thisGuid != Guid.Empty && thisGuid == value)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1019,
                Message = DiagnosticResources.UNT1019,
                Range = node.Range
            });
        }
    }

    internal static void CheckSelfRef(in SpecPropertyTypeParseContext parse, ushort id, int assetCategory, AssetFileNode node, bool? preventSelfRef = null)
    {
        if (assetCategory == 0)
            return;

        bool setting = preventSelfRef ?? GetPreventSelfRef(in parse);
        if (!setting)
            return;

        if (parse.File?.GetId() is not { } thisId || thisId == 0 || thisId != id)
            return;

        EnumSpecTypeValue cat = parse.File.GetCategory(parse.Database);
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
    public bool Equals(ISpecPropertyType? other) => other is AssetReferenceSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<Guid>? other) => other is AssetReferenceSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}
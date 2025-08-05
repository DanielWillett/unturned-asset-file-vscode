using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class GuidOrIdSpecPropertyType :
    BaseSpecPropertyType<GuidOrId>,
    ISpecPropertyType<GuidOrId>,
    IElementTypeSpecPropertyType,
    ISpecialTypesSpecPropertyType,
    IEquatable<GuidOrIdSpecPropertyType>,
    IStringParseableSpecPropertyType
{
    private IAssetSpecDatabase? _cachedSpecDb;
    private ISpecType? _cachedType;

    public OneOrMore<QualifiedType> OtherElementTypes { get; }
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "GuidOrId";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public Type ValueType => typeof(GuidOrId);

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;
    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => OtherElementTypes.Select<string?>(x => x.Type);

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (Guid.TryParse(stringValue ??= span.ToString(), out Guid result))
        {
            dynamicValue = new SpecDynamicConcreteValue<GuidOrId>(new GuidOrId(result), this);
            return true;
        }

        if (ushort.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out ushort id))
        {
            dynamicValue = new SpecDynamicConcreteValue<GuidOrId>(id == 0 ? new GuidOrId(Guid.Empty) : new GuidOrId(id), this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    public GuidOrIdSpecPropertyType(QualifiedType elementType, OneOrMore<string> specialTypes = default)
    {
        ElementType = elementType;
        DisplayName = elementType.IsNull ? "GUID or ID" : $"GUID or ID ({elementType.GetTypeName()})";
        OtherElementTypes = specialTypes
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => new QualifiedType(x));
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out GuidOrId val))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteValue<GuidOrId>(val, this);
        return true;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out GuidOrId value)
    {
        // todo: remember that [vehicle ]redirect assets need to work properly
        ISpecType? specType = null;

        if (_cachedSpecDb == parse.Database)
        {
            lock (this)
            {
                if (_cachedSpecDb == parse.Database)
                {
                    specType = _cachedType;
                }
            }
        }

        if (specType == null)
        {
            specType = parse.Database.FindType(ElementType.Type, parse.FileType);
            lock (this)
            {
                _cachedSpecDb = parse.Database;
                _cachedType = specType;
            }
        }

        if (specType is not EnumSpecType enumType)
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

        if (val.IndexOf('.') >= 0)
        {
            EnumSpecTypeValue[] values = enumType.Values;
            bool found = false;
            for (int i = 0; i < values.Length; ++i)
            {
                ref EnumSpecTypeValue enumVal = ref values[i];
                if (enumVal.CorrespondingType.IsNull || !string.Equals(enumVal.Value, val, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                val = enumVal.CorrespondingType.Type;
                found = true;
                break;
            }

            if (!found && int.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out int index) && index >= 0 && index < values.Length)
            {
                string? type = values[index].CorrespondingType.Type;
                if (type != null)
                    val = type;
            }
        }

        return GuidOrId.TryParse(val, out value) || FailedToParse(in parse, out value);
    }

    /// <inheritdoc />
    public bool Equals(GuidOrIdSpecPropertyType other) => other != null && ElementType.Equals(other.ElementType) && OtherElementTypes.Equals(other.OtherElementTypes);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is GuidOrIdSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<GuidOrId>? other) => other is GuidOrIdSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(TVisitor visitor) => visitor.Visit(this);
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A backwards-compatable reference to one or more types of assets formatted as a string.
/// Accepts both <see cref="Guid"/> and <see cref="ushort"/> IDs.
/// <para>Example: <c>ItemBarricadeAsset.Explosion</c></para>
/// <code>
/// Prop fcfe74fe2e8748a69813539f7dbc5738
/// Prop 21
/// </code>
/// <para>
/// Also supports the <c>PreventSelfReference</c> additional property to log a warning if the current asset is referenced.
/// </para>
/// <para>
/// If an amount is supppled (i.e. "21 x 2") a warning will be logged.
/// </para>
/// </summary>
public sealed class GuidOrIdSpecPropertyType :
    BaseSpecPropertyType<GuidOrId>,
    ISpecPropertyType<GuidOrId>,
    IElementTypeSpecPropertyType,
    ISpecialTypesSpecPropertyType,
    IEquatable<GuidOrIdSpecPropertyType>,
    IStringParseableSpecPropertyType
{
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

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<GuidOrId>()?.ToString();
    }

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

    public override int GetHashCode()
    {
        return 72 ^ HashCode.Combine(ElementType, OtherElementTypes);
    }

    public GuidOrIdSpecPropertyType(QualifiedType elementType, OneOrMore<string> specialTypes = default)
    {
        OtherElementTypes = AssetReferenceSpecPropertyType.NormalizeAssetTypes(ref elementType, specialTypes, out _);
        ElementType = elementType;

        ElementType = elementType;
        DisplayName = elementType.IsNull ? "GUID or ID" : $"GUID or ID ({elementType.GetTypeName()})";
        OtherElementTypes = specialTypes
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => new QualifiedType(x))
            .Remove(elementType);

        DisplayName = AssetReferenceSpecPropertyType.CreateDisplayName("ID or Asset Reference", elementType, OtherElementTypes);
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
        
        ISpecType? specType = parse.Database.FindType(ElementType.Type, parse.FileType);

        if (!ElementType.IsNull && specType is not AssetSpecType)
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

        if (parse.Node is not IValueSourceNode stringNode)
        {
            return FailedToParse(in parse, out value);
        }

        string val = stringNode.Value;

        AssetReferenceSpecPropertyType.CheckInappropriateAmount(in parse, stringNode);

        if (!GuidOrId.TryParse(val, out value))
        {
            return FailedToParse(in parse, out value);
        }

        if (!parse.HasDiagnostics)
            return true;
        
        if (!AssetReferenceSpecPropertyType.GetPreventSelfRef(in parse))
            return true;

        if (!value.IsId)
        {
            AssetReferenceSpecPropertyType.CheckSelfRef(in parse, value.Guid, stringNode, true);
        }
        else
        {
            int category = value.Category;
            if (category == 0)
            {
                category = parse.Database.Information.GetAssetCategory(ElementType, OtherElementTypes);
            }
            if (category > 0)
            {
                AssetReferenceSpecPropertyType.CheckSelfRef(in parse, value.Id, category, stringNode, true);
            }
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(GuidOrIdSpecPropertyType other) => other != null && ElementType.Equals(other.ElementType) && OtherElementTypes.Equals(other.OtherElementTypes);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is GuidOrIdSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<GuidOrId>? other) => other is GuidOrIdSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}
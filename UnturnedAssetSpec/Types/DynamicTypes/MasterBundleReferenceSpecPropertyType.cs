using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class MasterBundleReferenceSpecPropertyType :
    BaseSpecPropertyType<BundleReference>,
    ISpecPropertyType<BundleReference>,
    IElementTypeSpecPropertyType,
    IEquatable<MasterBundleReferenceSpecPropertyType?>,
    IStringParseableSpecPropertyType
{
    public static readonly MasterBundleReferenceSpecPropertyType AudioReference = new MasterBundleReferenceSpecPropertyType();

    static MasterBundleReferenceSpecPropertyType() { }

    public MasterBundleReferenceType ReferenceType { get; }

    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type { get; }

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public Type ValueType => typeof(BundleReference);

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<BundleReference>()?.ToString();
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (KnownTypeValueHelper.TryParseMasterBundleReference(span, out string? name, out string path) && name != null)
        {
            dynamicValue = new SpecDynamicConcreteValue<BundleReference>(new BundleReference(name, path, ReferenceType), this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    private MasterBundleReferenceSpecPropertyType()
        : this(new QualifiedType("UnityEngine.AudioClip, UnityEngine.AudioModule"), MasterBundleReferenceType.AudioReference)
    {

    }

    public MasterBundleReferenceSpecPropertyType(QualifiedType elementType, MasterBundleReferenceType referenceType)
    {
        ReferenceType = referenceType;
        Type = referenceType switch
        {
            MasterBundleReferenceType.AudioReference => "AudioReference",
            MasterBundleReferenceType.ContentReference => "ContentReference",
            MasterBundleReferenceType.MasterBundleReferenceString => "MasterBundleReferenceString",
            _ => "MasterBundleReference"
        };

        DisplayName = referenceType switch
        {
            MasterBundleReferenceType.AudioReference => "Audio Reference",
            MasterBundleReferenceType.ContentReference => "Content Reference",
            MasterBundleReferenceType.MasterBundleReferenceString => "Masterbundle Reference (Legacy)",
            _ => "Masterbundle Reference"
        };

        ElementType = elementType;
        
        if (elementType.Type == null || referenceType == MasterBundleReferenceType.AudioReference)
            return;

        DisplayName += $" for {QualifiedType.ExtractTypeName(elementType.Type.AsSpan()).ToString()}";
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out BundleReference val))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteValue<BundleReference>(val, this);
        return true;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out BundleReference value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is AssetFileStringValueNode stringValue)
        {
            if (!KnownTypeValueHelper.TryParseMasterBundleReference(stringValue.Value, out string name, out string path))
            {
                return FailedToParse(in parse, out value);
            }

            //if (string.IsNullOrEmpty(name))
            //{
            //    // todo: check if in master bundle if name is null
            //    name = "<<current master bundle>>";
            //}

            value = new BundleReference(name, path, ReferenceType);
            return true;
        }

        if (parse.Node is not AssetFileDictionaryValueNode dictionary
            || ReferenceType is not MasterBundleReferenceType.MasterBundleReference and not MasterBundleReferenceType.ContentReference)
        {
            return FailedToParse(in parse, out value);
        }

        string nameProperty = ReferenceType == MasterBundleReferenceType.ContentReference ? "Name" : "MasterBundle";
        string pathProperty = ReferenceType == MasterBundleReferenceType.ContentReference ? "Path" : "AssetPath";

        dictionary.TryGetValue(nameProperty, out AssetFileValueNode? nameNode);

        if (!dictionary.TryGetValue(pathProperty, out AssetFileValueNode? pathNode) || pathNode is not AssetFileStringValueNode pathValue)
        {
            return MissingProperty(in parse, pathProperty, out value);
        }

        value = new BundleReference((nameNode as AssetFileStringValueNode)?.Value ?? string.Empty, pathValue.Value, ReferenceType);
        return true;
    }

    /// <inheritdoc />
    public bool Equals(MasterBundleReferenceSpecPropertyType? other) => other != null && ElementType.Equals(other.ElementType);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is MasterBundleReferenceSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<BundleReference>? other) => other is MasterBundleReferenceSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}

public enum MasterBundleReferenceType
{
    Unspecified,
    MasterBundleReference,
    MasterBundleReferenceString,
    ContentReference,
    AudioReference
}
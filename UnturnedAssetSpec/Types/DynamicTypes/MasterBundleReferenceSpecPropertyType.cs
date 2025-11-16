using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

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
/// </summary>
public sealed class MasterBundleReferenceSpecPropertyType :
    BaseSpecPropertyType<BundleReference>,
    ISpecPropertyType<BundleReference>,
    IElementTypeSpecPropertyType,
    IEquatable<MasterBundleReferenceSpecPropertyType?>,
    IStringParseableSpecPropertyType
{
    public static readonly MasterBundleReferenceSpecPropertyType AudioReference = new MasterBundleReferenceSpecPropertyType(
        new QualifiedType("UnityEngine.AudioClip, UnityEngine.AudioModule"),
        MasterBundleReferenceType.AudioReference
    );

    public static readonly MasterBundleReferenceSpecPropertyType TranslationReference = new MasterBundleReferenceSpecPropertyType(
        QualifiedType.None,
        MasterBundleReferenceType.TranslationReference
    );

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

    public override int GetHashCode()
    {
        return 76 ^ HashCode.Combine(ReferenceType, ElementType);
    }

    public MasterBundleReferenceSpecPropertyType(QualifiedType elementType, MasterBundleReferenceType referenceType)
    {
        ReferenceType = referenceType;
        Type = referenceType switch
        {
            MasterBundleReferenceType.AudioReference => "AudioReference",
            MasterBundleReferenceType.ContentReference => "ContentReference",
            MasterBundleReferenceType.MasterBundleReferenceString => "MasterBundleReferenceString",
            MasterBundleReferenceType.MasterBundleOrContentReference => "MasterBundleOrContentReference",
            MasterBundleReferenceType.TranslationReference => "TranslationReference",
            _ => "MasterBundleReference"
        };

        DisplayName = referenceType switch
        {
            MasterBundleReferenceType.AudioReference => "Audio Reference",
            MasterBundleReferenceType.ContentReference => "Content Reference",
            MasterBundleReferenceType.MasterBundleReferenceString => "Masterbundle Reference (Legacy)",
            MasterBundleReferenceType.MasterBundleOrContentReference => "Masterbundle or Content Reference",
            MasterBundleReferenceType.TranslationReference => "Legacy Translation Token Reference",
            _ => "Masterbundle Reference"
        };

        ElementType = elementType;
        
        if (elementType.Type == null || referenceType is MasterBundleReferenceType.AudioReference or MasterBundleReferenceType.TranslationReference)
            return;

        DisplayName += $" to {QualifiedType.ExtractTypeName(elementType.Type.AsSpan()).ToString()}";
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

        if (parse.Node is IValueSourceNode stringValue)
        {
            string? name, path;
            if (ReferenceType == MasterBundleReferenceType.TranslationReference)
            {
                if (!KnownTypeValueHelper.TryParseTranslationReference(stringValue.Value, out name, out path))
                {
                    return FailedToParse(in parse, out value);
                }

                if (parse.HasDiagnostics)
                {
                    parse.Log(new DatDiagnosticMessage
                    {
                        Range = stringValue.Range,
                        Diagnostic = DatDiagnostics.UNT1018,
                        Message = DiagnosticResources.UNT1018_TranslationReference
                    });
                }
            }
            else
            {
                if (!KnownTypeValueHelper.TryParseMasterBundleReference(stringValue.Value, out name, out path))
                {
                    return FailedToParse(in parse, out value);
                }

                //if (string.IsNullOrEmpty(name))
                //{
                //    // todo: check if in master bundle if name is null
                //    name = "<<current master bundle>>";
                //}
            }

            MasterBundleReferenceType rType = ReferenceType;
            if (rType is MasterBundleReferenceType.MasterBundleReference or MasterBundleReferenceType.MasterBundleOrContentReference)
                rType = MasterBundleReferenceType.MasterBundleReferenceString;

            value = new BundleReference(name, path, rType);
            return true;
        }

        // todo: AudioReference can also reference OneShotAudioDefs (*.asset)
        if (ReferenceType == MasterBundleReferenceType.TranslationReference && parse.HasDiagnostics)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Range = parse.Node.Range,
                Diagnostic = DatDiagnostics.UNT1018,
                Message = DiagnosticResources.UNT1018_TranslationReference
            });
        }

        if (parse.Node is not IDictionarySourceNode dictionary
            || ReferenceType is not MasterBundleReferenceType.MasterBundleReference and not MasterBundleReferenceType.ContentReference and not MasterBundleReferenceType.MasterBundleOrContentReference)
        {
            return FailedToParse(in parse, out value);
        }

        if (ReferenceType != MasterBundleReferenceType.MasterBundleOrContentReference)
            return TryParseReferenceType(dictionary, in parse, out value, ReferenceType);

        SpecPropertyTypeParseContext parseCtx = parse.WithoutDiagnostics();

        bool masterBundleRef = TryParseReferenceType(dictionary, in parse, out BundleReference masterBundleReference, MasterBundleReferenceType.MasterBundleReference);

        if (!TryParseReferenceType(dictionary, in parseCtx, out value, MasterBundleReferenceType.ContentReference))
        {
            value = masterBundleReference;
            return masterBundleRef;
        }

        parse.Log(new DatDiagnosticMessage
        {
            Diagnostic = DatDiagnostics.UNT104,
            Message = DiagnosticResources.UNT104,
            Range = dictionary.Range
        });

        if (masterBundleRef)
        {
            value = masterBundleReference;
        }
        
        return true;

    }

    private bool TryParseReferenceType(IDictionarySourceNode dictionary, in SpecPropertyTypeParseContext parse, out BundleReference value, MasterBundleReferenceType rType)
    {
        string nameProperty = ReferenceType == MasterBundleReferenceType.ContentReference ? "Name" : "MasterBundle";
        string pathProperty = ReferenceType == MasterBundleReferenceType.ContentReference ? "Path" : "AssetPath";

        dictionary.TryGetPropertyValue(nameProperty, out IValueSourceNode? nameNode);

        if (!dictionary.TryGetProperty(pathProperty, out IPropertySourceNode? pathNode))
        {
            return MissingProperty(in parse, pathProperty, out value);
        }

        if (!pathNode.HasValue || pathNode.ValueKind != ValueTypeDataRefType.Value)
        {
            value = new BundleReference(string.Empty, string.Empty, rType);
        }

        value = new BundleReference(nameNode?.Value ?? string.Empty, pathNode.GetValueString(out _)!, rType);
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

/// <summary>
/// Various different variations of a masterbundle name and path structure.
/// </summary>
public enum MasterBundleReferenceType
{
    Unspecified,

    /// <summary>
    /// Supports the string representation or object representation of a master bundle reference.
    /// Represented in-game by the <see cref="T:SDG.Unturned.MasterBundleReference{T}"/> type.
    /// <code>
    /// Path (current masterbundle)
    /// Name:Path
    /// 
    /// // or
    /// 
    /// {
    ///     MasterBundle "Name"
    ///     AssetPath "Path"
    /// }
    /// </code>
    /// </summary>
    MasterBundleReference,

    /// <summary>
    /// Only supports the string representation of a master bundle reference.
    /// Represented in-game by the <see cref="T:SDG.Unturned.MasterBundleReference{T}"/> type.
    /// <code>
    /// Path (current masterbundle)
    /// Name:Path
    /// </code>
    /// </summary>
    MasterBundleReferenceString,

    /// <summary>
    /// Supports the string representation or object representation of a master bundle reference.
    /// Represented in-game by the <see cref="T:SDG.Unturned.ContentReference{T}"/> type.
    /// <code>
    /// Path (current masterbundle)
    /// Name:Path
    /// 
    /// // or
    /// 
    /// {
    ///     Name "Name"
    ///     Path "Path"
    /// }
    /// </code>
    /// </summary>
    ContentReference,

    /// <summary>
    /// Only supports the string representation of a master bundle reference, specifically for Audio Clips or One Shot Audio definitions.
    /// Represented in-game by the <see cref="T:SDG.Unturned.AudioReference"/> type.
    /// <code>
    /// Path (current masterbundle)
    /// Name:Path
    /// </code>
    /// </summary>
    AudioReference,

    /// <summary>
    /// Supports the string representation or object representation of a master bundle reference.
    /// Either <see cref="MasterBundleReference"/> or <see cref="ContentReference"/>, preferring <see cref="MasterBundleReference"/>.
    /// <code>
    /// Path (current masterbundle)
    /// Name:Path
    /// 
    /// // or
    /// 
    /// {
    ///     Name "Name"
    ///     Path "Path"
    /// }
    ///
    /// // or
    /// 
    /// {
    ///     MasterBundle "Name"
    ///     AssetPath "Path"
    /// }
    /// </code>
    /// </summary>
    MasterBundleOrContentReference,

    /// <summary>
    /// TranslationReference is an old structure that was used to reference legacy translation tokens.
    /// <code>
    /// {
	///     Namespace SDG
	///     Token Stereo_Songs.Unturned_Theme.Title
	/// }
    /// </code>
    /// It could also be represented like these:
    /// <para><c>SDG::Stereo_Songs.Unturned_Theme.Title</c></para>
    /// <para><c>SDG#Stereo_Songs.Unturned_Theme.Title</c></para>
    /// </summary>
    /// <remarks>It has been removed from the game but still remains in the documentation for StereoSongAsset.</remarks>
    TranslationReference
}
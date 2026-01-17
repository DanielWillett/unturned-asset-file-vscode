using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data;

/// <summary>
/// A data structure containg a bundle name and file path within the bundle.
/// This is a common format shared between master bundle references, content references, audio references, and more.
/// </summary>
[JsonConverter(typeof(BundleReferenceConverter))]
public readonly struct BundleReference : IEquatable<BundleReference>, IComparable<BundleReference>
{
    /// <summary>
    /// The name of a bundle.
    /// </summary>
    public readonly string Name;
    
    /// <summary>
    /// The path of content within the bundle.
    /// </summary>
    public readonly string Path;

    /// <summary>
    /// The type of reference this reference was created for.
    /// </summary>
    public readonly BundleReferenceKind Type;

    public BundleReference(string name, string path, BundleReferenceKind type)
    {
        Name = name;
        Path = path;
        Type = type;
    }

    public static bool TryParse(string str, out BundleReference bundleReference, BundleReferenceKind type = BundleReferenceKind.MasterBundleReferenceString)
    {
        if (!KnownTypeValueHelper.TryParseMasterBundleReference(str, out string? name, out string? path) || name == null || path == null)
        {
            bundleReference = default;
            return false;
        }

        bundleReference = new BundleReference(name, path, type);
        return true;
    }

    /// <inheritdoc />
    public bool Equals(BundleReference other)
    {
        if (Name == null || Path == null)
        {
            return other.Name == null || other.Path == null;
        }

        if (other.Name == null || other.Path == null)
            return false;

        return string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase)
               && string.Equals(Path, other.Path, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public int CompareTo(BundleReference other)
    {
        if (Name == null || Path == null)
        {
            return other.Name == null || other.Path == null ? 0 : -1;
        }

        if (other.Name == null || other.Path == null)
            return 1;

        int cmp = string.Compare(Name, other.Name, StringComparison.InvariantCultureIgnoreCase);
        return cmp != 0 ? cmp : string.Compare(Path, other.Path, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is BundleReference r && Equals(r);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (Name == null || Path == null)
            return 0;

        return StringComparer.InvariantCultureIgnoreCase.GetHashCode(Name) ^ StringComparer.Ordinal.GetHashCode(Path);
    }

    /// <inheritdoc />
    public override string ToString() => Name == null || Path == null ? string.Empty : (Name + ":" + Path);

    public static bool operator ==(BundleReference left, BundleReference right) => left.Equals(right);
    public static bool operator !=(BundleReference left, BundleReference right) => !left.Equals(right);
}

/// <summary>
/// Various different variations of a masterbundle name and path structure.
/// </summary>
public enum BundleReferenceKind
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
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
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
    public readonly MasterBundleReferenceType Type;

    public BundleReference(string name, string path, MasterBundleReferenceType type)
    {
        Name = name;
        Path = path;
        Type = type;
    }

    public static bool TryParse(string str, out BundleReference bundleReference, MasterBundleReferenceType type = MasterBundleReferenceType.MasterBundleReferenceString)
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
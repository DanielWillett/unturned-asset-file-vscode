using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data;

/// <summary>
/// Stores the type and corresponding database information for a file.
/// </summary>
public readonly struct AssetFileType : IEquatable<AssetFileType>
{
#nullable disable
    /// <summary>
    /// The type being represented.
    /// </summary>
    public QualifiedType Type { get; }

    /// <summary>
    /// The database information for this type.
    /// </summary>
    public DatFileType Information { get; }
#nullable restore

    /// <summary>
    /// The alias used to refer to the file type, if any.
    /// </summary>
    public string? Alias { get; }

    /// <summary>
    /// Whether or not the information was found.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Information))]
    public bool IsValid => Information is not null;

    private AssetFileType(DatFileType information, string? alias)
    {
        Type = information.TypeName;
        Alias = alias;
        Information = information;
    }

    private AssetFileType(QualifiedType type, string? alias)
    {
        Type = type;
        Alias = alias;
        Information = null;
    }

    /// <summary>
    /// Create a <see cref="AssetFileType"/> for the base Asset class.
    /// </summary>
    public static AssetFileType AssetBaseType(IAssetSpecDatabase spec)
    {
        spec.FileTypes.TryGetValue(QualifiedType.AssetBaseType, out DatFileType? info);
        return info != null ? new AssetFileType(info, null) : new AssetFileType(QualifiedType.AssetBaseType, null);
    }

    /// <summary>
    /// Create a <see cref="AssetFileType"/> for a source file.
    /// </summary>
    public static AssetFileType FromFile(ISourceFile file, IAssetSpecDatabase spec)
    {
        QualifiedType type = file.ActualType;
        if (type.IsNull)
            return default;

        spec.FileTypes.TryGetValue(type, out DatFileType? info);
        return info != null ? new AssetFileType(info, null) : new AssetFileType(type, null);
    }

    /// <summary>
    /// Create a <see cref="AssetFileType"/> for a given asset type.
    /// </summary>
    public static AssetFileType FromType(QualifiedOrAliasedType type, IAssetSpecDatabase spec)
    {
        if (type.IsNull)
            return default;

        QualifiedType fullType;
        if (type.IsAlias)
        {
            if (!spec.Information.AssetAliases.TryGetValue(type.Type.Type, out fullType))
                return default;

            fullType = fullType.CaseInsensitive;
        }
        else
        {
            fullType = type.Type.CaseInsensitive;
        }

        spec.FileTypes.TryGetValue(fullType, out DatFileType? info);
        return info != null ? new AssetFileType(info, null) : new AssetFileType(fullType, null);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AssetFileType t && Equals(t);

    /// <inheritdoc />
    public override int GetHashCode() => Type.GetHashCode();

    /// <inheritdoc />
    public bool Equals(AssetFileType other)
    {
        return other.Type.Equals(Type) && string.Equals(Alias, other.Alias, StringComparison.OrdinalIgnoreCase);
    }
}
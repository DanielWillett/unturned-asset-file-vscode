using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public readonly struct AssetFileType : IEquatable<AssetFileType>
{
#nullable disable
    public QualifiedType Type { get; }
    public AssetSpecType Information { get; }
#nullable restore
    public string? Alias { get; }

    public bool IsValid => Information is not null;

    private AssetFileType(AssetSpecType information, string? alias)
    {
        Type = information.Type;
        Alias = alias;
        Information = information;
    }

    private AssetFileType(QualifiedType type, string? alias)
    {
        Type = type;
        Alias = alias;
        Information = null;
    }

    public static AssetFileType AssetBaseType(IAssetSpecDatabase spec)
    {
        spec.Types.TryGetValue(QualifiedType.AssetBaseType, out AssetSpecType? info);
        return info != null ? new AssetFileType(info, null) : new AssetFileType(QualifiedType.AssetBaseType, null);
    }

    public static AssetFileType FromFile(ISourceFile file, IAssetSpecDatabase spec)
    {
        QualifiedType type = file.ActualType;
        if (type.IsNull)
            return default;

        spec.Types.TryGetValue(type, out AssetSpecType? info);
        return info != null ? new AssetFileType(info, null) : new AssetFileType(type, null);
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
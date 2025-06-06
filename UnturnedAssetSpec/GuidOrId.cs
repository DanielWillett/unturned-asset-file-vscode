using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data;

/// <summary>
/// A GUID or Legacy ID reference to an asset.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 20)]
[JsonConverter(typeof(GuidOrIdConverter))]
public readonly struct GuidOrId : IEquatable<GuidOrId>, IComparable, IComparable<GuidOrId>, IEquatable<ushort>, IEquatable<Guid>
{
    public static readonly GuidOrId Empty = default;

    /// <summary>
    /// The GUID, if <see cref="IsId"/> is <see langword="false"/>.
    /// </summary>
    [FieldOffset(0)]
    public readonly Guid Guid;

    /// <summary>
    /// The legacy ID, if <see cref="IsId"/> is <see langword="true"/>.
    /// </summary>
    [FieldOffset(0)]
    public readonly ushort Id;

    /// <summary>
    /// The <see cref="AssetCategory"/> of the <see cref="Id"/>.
    /// </summary>
    [FieldOffset(2)]
    public readonly byte Category;

    /// <summary>
    /// Whether or not the value is a legacy ID.
    /// </summary>
    [FieldOffset(16)]
    public readonly bool IsId;

    /// <summary>
    /// If this value is 0.
    /// </summary>
    public bool IsNull => IsId ? Id == 0 : Guid == Guid.Empty;

    public GuidOrId(Guid guid)
    {
        Guid = guid;
    }

    public GuidOrId(ushort id)
    {
        Id = id;
        IsId = true;
    }

    public GuidOrId(ushort id, in EnumSpecTypeValue category)
    {
        Id = id;
        Category = (byte)category.Index;
        IsId = true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (!IsId)
            return Guid.ToString("N");

        if (Category == 0 || Category >= AssetCategory.TypeOf.Values.Length)
            return Id.ToString(CultureInfo.InvariantCulture);

        return $"{AssetCategory.TypeOf.Values[Category].Value}:{Id.ToString(CultureInfo.InvariantCulture)}";
    }

    /// <inheritdoc />
    public int CompareTo(object? obj) => obj is GuidOrId i ? CompareTo(i) : 1;

    /// <inheritdoc />
    public bool Equals(GuidOrId other) => IsId == other.IsId && (IsId ? Id == other.Id && Category == other.Category : Guid == other.Guid);

    public bool Equals(ushort id) => IsId && Id == id;

    public bool Equals(ushort id, in EnumSpecTypeValue assetCategory) => IsId && Id == id && Category == assetCategory.Index;

    public bool Equals(Guid guid) => !IsId && Guid == guid;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is GuidOrId other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return IsId ? Id : Guid.GetHashCode();
    }

    /// <summary>
    /// Convert a <see cref="string"/> to a <see cref="GuidOrId"/>.
    /// </summary>
    public static bool TryParse(string str, out GuidOrId id)
    {
        if (ushort.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out ushort shortId))
        {
            id = new GuidOrId(shortId);
            return true;
        }

        if (Guid.TryParse(str, out Guid guid))
        {
            id = new GuidOrId(guid);
            return true;
        }

        id = default;
        return false;
    }

    /// <summary>
    /// Convert a <see cref="string"/> to a <see cref="GuidOrId"/>.
    /// </summary>
    /// <exception cref="FormatException"></exception>
    public static GuidOrId Parse(string str)
    {
        if (!TryParse(str, out GuidOrId id))
            throw new FormatException("Failed to parse GuidOrId.");

        return id;
    }

    /// <inheritdoc />
    public int CompareTo(GuidOrId other)
    {
        if (IsId != other.IsId)
            return (IsId ? 0 : 1) * 2 - 1;

        if (IsId)
            return Id - other.Id;

        return Guid.CompareTo(other.Guid);
    }

    public static bool operator ==(GuidOrId left, GuidOrId right) => left.Equals(right);
    public static bool operator !=(GuidOrId left, GuidOrId right) => !left.Equals(right);
}
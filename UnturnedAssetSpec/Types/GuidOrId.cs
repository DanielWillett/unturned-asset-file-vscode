using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A GUID or Legacy ID reference to an asset.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 20)]
public readonly struct GuidOrId : IEquatable<GuidOrId>, IComparable, IComparable<GuidOrId>
{
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

    /// <inheritdoc />
    public override string ToString() => IsId ? Id.ToString(CultureInfo.InvariantCulture) : Guid.ToString("N");

    /// <inheritdoc />
    public int CompareTo(object? obj) => obj is GuidOrId i ? CompareTo(i) : 1;

    /// <inheritdoc />
    public bool Equals(GuidOrId other) => IsId == other.IsId && (IsId ? Id == other.Id : Guid == other.Guid);

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
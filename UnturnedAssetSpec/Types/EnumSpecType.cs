using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public class EnumSpecType : ISpecType, IEquatable<EnumSpecType>, IComparable<EnumSpecType>
{
    string ISpecType.Type => Type.Type;
    public required QualifiedType Type { get; init; }

    public required string DisplayName { get; init; }

    public required string? Docs { get; init; }

    public required EnumSpecTypeValue[] Values { get; init; }
    
    public bool Equals(EnumSpecType other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other == null)
        {
            return false;
        }

        return string.Equals(Type, other.Type, StringComparison.Ordinal);
    }

    public int CompareTo(EnumSpecType other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        return other == null ? 1 : string.Compare(Type, other.Type, StringComparison.Ordinal);
    }

    public bool Equals(ISpecType other) => other is EnumSpecType s && Equals(s);

    public int CompareTo(ISpecType other) => other is EnumSpecType s ? CompareTo(s) : 1;

    public override bool Equals(object? obj) => obj is EnumSpecType s && Equals(s);

    public override int GetHashCode() => Type.GetHashCode();
}

[DebuggerDisplay("{Value,nq}")]
public readonly struct EnumSpecTypeValue : IEquatable<EnumSpecTypeValue>, IComparable, IComparable<EnumSpecTypeValue>
{
    public required int Index { get; init; }
    public required EnumSpecType Type { get; init; }

    public required string Value { get; init; }
    public required string Casing { get; init; }
    public QualifiedType RequiredBaseType { get; init; }
    public QualifiedType CorrespondingType { get; init; }
    public string? Description { get; init; }
    public bool Deprecated { get; init; }

    public IReadOnlyDictionary<string, string>? ExtendedData { get; init; }
    
    /// <inheritdoc />
    public bool Equals(EnumSpecTypeValue other) => Index == other.Index;

    /// <inheritdoc />
    public int CompareTo(EnumSpecTypeValue other) => Index.CompareTo(other.Index);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EnumSpecTypeValue && Equals(obj);

    /// <inheritdoc />
    public override int GetHashCode() => Index;

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    public int CompareTo(object obj) => obj is EnumSpecTypeValue t ? CompareTo(t) : 1;

    public static bool operator ==(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index == right.Index;
    public static bool operator !=(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index != right.Index;
    public static bool operator <(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index < right.Index;
    public static bool operator >(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index > right.Index;
    public static bool operator <=(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index <= right.Index;
    public static bool operator >=(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index >= right.Index;
}
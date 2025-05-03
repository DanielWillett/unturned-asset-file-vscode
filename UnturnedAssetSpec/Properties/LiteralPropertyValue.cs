using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public sealed class LiteralPropertyValue<TValue> : IDefaultValue<TValue>, IEquatable<LiteralPropertyValue<TValue>>, IComparable<LiteralPropertyValue<TValue>> where TValue : IEquatable<TValue?>, IComparable<TValue?>
{
    public TValue? Value { get; }

    public LiteralPropertyValue(TValue value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is LiteralPropertyValue<TValue> v && Equals(v);

    /// <inheritdoc />
    public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();

#pragma warning disable CS0693
    /// <inheritdoc />
    IDefaultValue<TValue>? IDefaultValue.As<TValue>() => this as IDefaultValue<TValue>;
#pragma warning restore CS0693

    /// <inheritdoc />
    public bool Equals(IDefaultValue<TValue> other) => other is LiteralPropertyValue<TValue> v && Equals(v);

    /// <inheritdoc />
    public bool Equals(LiteralPropertyValue<TValue> other) => Value == null ? other.Value == null : Value.Equals(other.Value);

    /// <inheritdoc />
    public int CompareTo(IDefaultValue<TValue> other) => other is not LiteralPropertyValue<TValue> v ? 1 : CompareTo(v);

    /// <inheritdoc />
    public int CompareTo(LiteralPropertyValue<TValue> other)
    {
        if (Value == null)
        {
            return other.Value == null ? 0 : -1;
        }

        return other.Value == null ? 1 : Value.CompareTo(other.Value);
    }

    /// <inheritdoc />
    public TValue? GetDefaultValue(AssetFileTree assetFile, AssetSpecDatabase database)
    {
        return Value;
    }
}
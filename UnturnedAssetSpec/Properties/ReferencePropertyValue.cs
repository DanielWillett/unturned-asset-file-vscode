using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public sealed class ReferencePropertyValue<TValue> : IDefaultValue<TValue>, IEquatable<ReferencePropertyValue<TValue>>, IComparable<ReferencePropertyValue<TValue>> where TValue : IEquatable<TValue?>, IComparable<TValue?>
{
    public string PropertyName { get; }
    public SpecPropertyContext Context { get; }

    public ReferencePropertyValue(string propertyName, SpecPropertyContext context = SpecPropertyContext.Property)
    {
        PropertyName = propertyName;
        Context = context;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ReferencePropertyValue<TValue> v && Equals(v);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return PropertyName.GetHashCode() ^ (int)Context;
    }

    /// <inheritdoc />
    public bool Equals(IDefaultValue<TValue> other) => other is ReferencePropertyValue<TValue> v && Equals(v);

    /// <inheritdoc />
    public bool Equals(ReferencePropertyValue<TValue> other) => Context == other.Context
                                                            && string.Equals(PropertyName, other.PropertyName, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public int CompareTo(IDefaultValue<TValue> other) => other is ReferencePropertyValue<TValue> v ? CompareTo(v) : 1;

    /// <inheritdoc />
    public int CompareTo(ReferencePropertyValue<TValue> other)
    {
        if (Context != other.Context)
            return ((int)Context).CompareTo((int)other.Context);

        return string.Compare(PropertyName, other.PropertyName, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public TValue? GetDefaultValue(AssetFileTree assetFile, AssetSpecDatabase database)
    {
        if (assetFile.TryGetProperty(PropertyName, database, out TValue? val, out _))
            return val;

        assetFile.TryGetProperty(PropertyName, database, out val, out _, context: SpecPropertyContext.Localization);
        return val;

    }

#pragma warning disable CS0693

    /// <inheritdoc />
    IDefaultValue<TValue>? IDefaultValue.As<TValue>() => this as IDefaultValue<TValue>;

#pragma warning restore CS0693
}
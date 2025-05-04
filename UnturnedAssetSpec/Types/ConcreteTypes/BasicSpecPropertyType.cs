using System;
using System.ComponentModel;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class BasicSpecPropertyType<TSpecPropertyType, TValue> :
    BaseSpecPropertyType<TValue>,
    ISpecPropertyType<TValue>,
    IEquatable<BasicSpecPropertyType<TSpecPropertyType, TValue>>
    where TSpecPropertyType : BasicSpecPropertyType<TSpecPropertyType, TValue>
    where TValue : IEquatable<TValue>
{
    /// <inheritdoc />
    public abstract SpecPropertyTypeKind Kind { get; }

    private protected BasicSpecPropertyType() { }

    public bool Equals(ISpecPropertyType other) => other is BasicSpecPropertyType<TSpecPropertyType, TValue>;
    public bool Equals(ISpecPropertyType<TValue> other) => other is BasicSpecPropertyType<TSpecPropertyType, TValue>;
    
    public bool Equals(BasicSpecPropertyType<TSpecPropertyType, TValue> other) => true;

    /// <inheritdoc />
    public abstract bool TryParseValue(in SpecPropertyTypeParseContext parse, out TValue? value);

    public override bool Equals(object? obj) => obj is BasicSpecPropertyType<TSpecPropertyType, TValue>;
    public override int GetHashCode() => 0;
    public override string ToString() => Type;
}
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.ComponentModel;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class BasicSpecPropertyType<TSpecPropertyType, TValue> :
    BaseSpecPropertyType<TValue>,
    ISpecPropertyType<TValue>,
    IEquatable<BasicSpecPropertyType<TSpecPropertyType, TValue>?>
    where TSpecPropertyType : BasicSpecPropertyType<TSpecPropertyType, TValue>
    where TValue : IEquatable<TValue>
{
    /// <inheritdoc />
    public abstract SpecPropertyTypeKind Kind { get; }

    public virtual bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out TValue? val))
        {
            value = null!;
            return false;
        }

        value = val == null ? SpecDynamicValue.Null : CreateValue(val);
        return true;
    }

    protected virtual ISpecDynamicValue CreateValue(TValue? value)
    {
        return new SpecDynamicConcreteValue<TValue>(value, this);
    }

    /// <inheritdoc />
    public Type ValueType => typeof(TValue);

    private protected BasicSpecPropertyType() { }

    public bool Equals(ISpecPropertyType? other) => other is BasicSpecPropertyType<TSpecPropertyType, TValue>;
    public bool Equals(ISpecPropertyType<TValue>? other) => other is BasicSpecPropertyType<TSpecPropertyType, TValue>;
    
    public bool Equals(BasicSpecPropertyType<TSpecPropertyType, TValue>? other) => other != null;

    /// <inheritdoc />
    public abstract bool TryParseValue(in SpecPropertyTypeParseContext parse, out TValue? value);

    public override bool Equals(object? obj) => obj is BasicSpecPropertyType<TSpecPropertyType, TValue>;
    public override int GetHashCode() => 0;
    public override string ToString() => Type;

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}
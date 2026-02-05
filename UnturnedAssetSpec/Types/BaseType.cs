using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// The base class for most typical implementations of <see cref="IType"/>.
/// </summary>
/// <typeparam name="TSelf">The implementing type.</typeparam>
public abstract class BaseType<TSelf> : IType
{
    /// <inheritdoc />
    public abstract string Id { get; }

    /// <inheritdoc />
    public abstract string DisplayName { get; }

    /// <inheritdoc />
    public virtual PropertySearchTrimmingBehavior TrimmingBehavior => PropertySearchTrimmingBehavior.ExactPropertyOnly;

    /// <inheritdoc />
    public abstract void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options);
    protected void WriteTypeName(Utf8JsonWriter writer) => writer.WriteString("Type"u8, Id);

    /// <inheritdoc />
    public abstract void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : ITypeVisitor;

    /// <summary>
    /// Determines whether or not the given value is equal to the current object.
    /// </summary>
    protected abstract bool Equals(TSelf other);

    /// <inheritdoc />
    public override string ToString() => Id;

    /// <inheritdoc />
    public abstract override int GetHashCode();

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TSelf s && Equals(s);
    bool IPropertyType.TryGetConcreteType(out IType type)
    {
        type = this;
        return true;
    }
    bool IPropertyType.TryEvaluateType(out IType type, in FileEvaluationContext ctx)
    {
        type = this;
        return true;
    }
    bool IEquatable<IPropertyType?>.Equals(IPropertyType? other) => other is TSelf s && Equals(s);
    bool IEquatable<IType?>.Equals(IType? other) => other is TSelf s && Equals(s);
}

/// <summary>
/// The base class for most typical implementations of <see cref="IType{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The type of value being parsed.</typeparam>
/// <typeparam name="TSelf">The implementing type.</typeparam>
[DebuggerDisplay("{Id,nq} ({typeof(TValue).Name,nq})")]
public abstract class BaseType<TValue, TSelf>
    : BaseType<TSelf>, IType<TValue>
    where TValue : IEquatable<TValue>
    where TSelf : BaseType<TValue, TSelf>
{
    public abstract ITypeParser<TValue> Parser { get; }

    public virtual IValue<TValue> CreateValue(Optional<TValue> value)
    {
        return value.HasValue
            ? Value.Create(value.Value, this)
            : Value.Null(this);
    }

    public override void Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.Accept(this);
    }
}
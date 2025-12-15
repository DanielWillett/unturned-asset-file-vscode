using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Diagnostics;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.NewTypes;

/// <summary>
/// The base class for most typical implementations of <see cref="IType{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The type of value being parsed.</typeparam>
/// <typeparam name="TSelf">The implementing type.</typeparam>
[DebuggerDisplay("{Id,nq} ({typeof(TValue).Name,nq})")]
public abstract class BaseType<TValue, TSelf>
    : IType<TValue>, IEquatable<TSelf>
    where TValue : IEquatable<TValue>
    where TSelf : BaseType<TValue, TSelf>
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract ITypeParser<TValue> Parser { get; }

    public virtual IValue<TValue> CreateValue(Optional<TValue> value)
    {
        return value.HasValue
            ? new ConcreteValue<TValue>(value.Value, this)
            : new ConcreteValue<TValue>(this);
    }

    public void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : ITypeVisitor
    {
        visitor.Accept(this);
    }

    public abstract void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options);
    protected void WriteTypeName(Utf8JsonWriter writer) => writer.WriteString("Type"u8, Id);

    protected abstract bool Equals(TSelf other);

    public override string ToString() => Id;

    public abstract override int GetHashCode();
    public override bool Equals(object? obj) => obj is TSelf s && Equals(s);
    bool IEquatable<TSelf>.Equals(TSelf? other) => other != null && Equals(other);
}
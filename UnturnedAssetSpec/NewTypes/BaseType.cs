using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.NewTypes;

/// <summary>
/// The base class for most typical implementations of <see cref="IType{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The type of value being parsed.</typeparam>
/// <typeparam name="TSelf">The implementing type.</typeparam>
[DebuggerDisplay("{Id,nq} ({typeof(TValue).Name,nq})")]
public abstract class BaseType<TValue, TSelf>
    : IType<TValue>
    where TValue : IEquatable<TValue>
    where TSelf : BaseType<TValue, TSelf>
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    public abstract ITypeParser<TValue> Parser { get; }

    public bool TryValueFromJson(ref Utf8JsonReader reader, [MaybeNullWhen(false)] out IValue<TValue> value)
    {
        if (!JsonHelper.TryReadGenericValue<TValue>(ref reader, out Optional<TValue> optionalValue))
        {
            value = null;
            return false;
        }

        value = CreateValue(optionalValue);
        return true;
    }

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

    protected abstract bool Equals(TSelf other);

    public override string ToString() => Id;

    public abstract override int GetHashCode();
}

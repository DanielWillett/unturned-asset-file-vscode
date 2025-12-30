using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A <see langword="null"/> value of any type.
/// </summary>
public sealed class NullValue(IType type) : IValue
{
    public IType Type { get; } = type;
    public bool IsNull => true;

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor) where TVisitor : IValueVisitor
    {
        visitor.Accept(Optional<string>.Null);
        return true;
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx) where TVisitor : IValueVisitor
    {
        visitor.Accept(Optional<string>.Null);
        return true;
    }

    public void WriteToJson(Utf8JsonWriter writer)
    {
        writer.WriteNullValue();
    }
}

/// <summary>
/// A <see langword="null"/> value of any strong type.
/// </summary>
public sealed class NullValue<T>(IType<T> type) : IValue<T>, IValueExpressionNode where T : IEquatable<T>
{
    /// <inheritdoc />
    public IType<T> Type { get; } = type;

    /// <inheritdoc />
    public bool IsNull => true;

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<T> value)
    {
        value = Optional<T>.Null;
        return true;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<T> value, in FileEvaluationContext ctx)
    {
        value = Optional<T>.Null;
        return true;
    }

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor) where TVisitor : IValueVisitor
    {
        visitor.Accept(Optional<T>.Null);
        return true;
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx) where TVisitor : IValueVisitor
    {
        visitor.Accept(Optional<T>.Null);
        return true;
    }

    public void WriteToJson(Utf8JsonWriter writer)
    {
        writer.WriteNullValue();
    }

    IType IValue.Type => Type;

    bool IEquatable<IExpressionNode>.Equals(IExpressionNode? other)
    {
        switch (other)
        {
            case null:
                return true;

            case IValue<T> strongValue:
                if (strongValue.IsNull)
                    return true;
                if (!strongValue.TryGetConcreteValue(out Optional<T> optVal))
                    return false;
                return !optVal.HasValue;

            case IValue value:
                if (value.IsNull)
                    return true;

                EqualityVisitor visitor;
                visitor.OtherValue = value;
                visitor.IsNull = false;
                value.Type.Visit(ref visitor);
                return visitor.IsNull;

            default:
                return false;
        }
    }


    private struct EqualityVisitor : ITypeVisitor
    {
        public IValue OtherValue;
        public bool IsNull;

        public void Accept<TOtherValue>(IType<TOtherValue> type)
            where TOtherValue : IEquatable<TOtherValue>
        {
            if (OtherValue is not IValue<TOtherValue> strongValue)
                return;

            if (strongValue.IsNull)
            {
                IsNull = true;
                return;
            }

            if (!strongValue.TryGetConcreteValue(out Optional<TOtherValue> optVal))
                return;

            IsNull = !optVal.HasValue;
        }
    }
}
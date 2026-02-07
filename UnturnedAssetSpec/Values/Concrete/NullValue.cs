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
public sealed class NullValue : IValue
{
    public static readonly NullValue Instance = new NullValue();

    static NullValue() { }
    private NullValue() { }

    public bool IsNull => true;

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        visitor.Accept(Optional<string>.Null);
        return true;
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        visitor.Accept(Optional<string>.Null);
        return true;
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteNullValue();
    }

    /// <inheritdoc />
    public bool Equals(IValue? other)
    {
        return other is { IsNull: true };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is NullValue;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return 1453913317;
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
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        visitor.Accept(Optional<T>.Null);
        return true;
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        visitor.Accept(Optional<T>.Null);
        return true;
    }

    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteNullValue();
    }

    /// <inheritdoc />
    public bool Equals(IValue? other)
    {
        return other is { IsNull: true };
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is NullValue<T> n && n.Type.Equals(Type);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(601500687, Type);
    }

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
                visitor.IsNull = false;
                value.VisitConcreteValue(ref visitor);
                return visitor.IsNull;

            default:
                return false;
        }
    }


    private struct EqualityVisitor : IValueVisitor
    {
        public bool IsNull;

        public void Accept<TOtherValue>(Optional<TOtherValue> optVal)
            where TOtherValue : IEquatable<TOtherValue>
        {
            IsNull = !optVal.HasValue;
        }
    }
}
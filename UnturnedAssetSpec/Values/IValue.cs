using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A dynamic value that can be resolved at runtime.
/// </summary>
public interface IValue
{
    /// <summary>
    /// Whether or not this value represents a <see langword="null"/> value.
    /// </summary>
    /// <remarks>It's still possible for a value to evaluate to <see langword="null"/> even if this is <see langword="false"/>.</remarks>
    bool IsNull { get; }

    /// <summary>
    /// The type stored in this value.
    /// </summary>
    IType Type { get; }

    /// <summary>
    /// Writes this value to a <see cref="Utf8JsonWriter"/> in a way that it can be recreated later.
    /// </summary>
    void WriteToJson(Utf8JsonWriter writer);

    /// <summary>
    /// Attempts to invoke <see cref="IValueVisitor.Accept"/> on the current value, but only when the value can be determined without context.
    /// </summary>
    bool VisitConcreteValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor;

    /// <summary>
    /// Attempts to invoke <see cref="IValueVisitor.Accept"/> on the current value, evaluating with context if necessary.
    /// </summary>
    bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor;
}

/// <summary>
/// A strongly-typed dynamic value that can be resolved at runtime.
/// </summary>
public interface IValue<TValue> : IValue where TValue : IEquatable<TValue>
{
    /// <summary>
    /// The type stored in this value.
    /// </summary>
    new IType<TValue> Type { get; }

    /// <summary>
    /// Attempts to evaluate the value without any workspace context.
    /// </summary>
    bool TryGetConcreteValue(out Optional<TValue> value);

    /// <summary>
    /// Attempts to evaluate the current value of this <see cref="IValue{TValue}"/>.
    /// </summary>
    bool TryEvaluateValue(out Optional<TValue> value, in FileEvaluationContext ctx);
}

/// <summary>
/// Used by <see cref="IValue.VisitConcreteValue"/> and <see cref="IValue.VisitValue"/> to enter a strongly typed context.
/// </summary>
public interface IValueVisitor
{
    /// <summary>
    /// Invoked by <see cref="IValue.VisitConcreteValue"/> and <see cref="IValue.VisitValue"/>.
    /// </summary>
    void Accept<TValue>(Optional<TValue> value) where TValue : IEquatable<TValue>;
}

/// <summary>
/// Extension methods for <see cref="IValue"/> and <see cref="IValue{TValue}"/>
/// </summary>
public static class ValueExtensions
{
    extension(IValue value)
    {
        /// <summary>
        /// Attempts to invoke <see cref="IGenericVisitor.Accept"/> on the current value, evaluating with context if necessary.
        /// </summary>
        /// <remarks>If the value is <see langword="null"/>, the visitor will be invoked using a <see langword="null"/> <see cref="string"/> value.</remarks>
        /// <returns><see langword="false"/> if the visitor didn't get invoked, otherwise <see langword="true"/>.</returns>
        public bool VisitConcreteValueGeneric<TVisitor>(ref TVisitor visitor)
            where TVisitor : IGenericVisitor
        {
            ValueVisitor<TVisitor> v;
            v.Visited = false;
            v.Visitor = visitor;

            value.VisitConcreteValue(ref v);
            if (!v.Visited)
                return false;

            visitor = v.Visitor;
            return true;
        }

        /// <summary>
        /// Attempts to invoke <see cref="IGenericVisitor.Accept"/> on the current value, evaluating with context if necessary.
        /// </summary>
        /// <remarks>If the value is <see langword="null"/>, the visitor will be invoked using a <see langword="null"/> <see cref="string"/> value.</remarks>
        /// <returns><see langword="false"/> if the visitor didn't get invoked, otherwise <see langword="true"/>.</returns>
        public bool VisitValueGeneric<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
            where TVisitor : IGenericVisitor
        {
            ValueVisitor<TVisitor> v;
            v.Visited = false;
            v.Visitor = visitor;

            value.VisitValue(ref v, in ctx);
            if (!v.Visited)
                return false;

            visitor = v.Visitor;
            return true;
        }
    }

    private struct ValueVisitor<TVisitor> : IValueVisitor
        where TVisitor : IGenericVisitor
    {
        public TVisitor Visitor;
        public bool Visited;

        public void Accept<T>(Optional<T> value) where T : IEquatable<T>
        {
            if (value.HasValue)
            {
                Visitor.Accept(value.Value);
            }
            else if (default(T) == null)
            {
                Visitor.Accept<T>(default);
            }
            else
            {
                Visitor.Accept<string>(null);
            }

            Visited = true;
        }
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A dynamic value that can be resolved at runtime.
/// </summary>
public interface IValue : IEquatable<IValue?>
{
    /// <summary>
    /// Whether or not this value represents a <see langword="null"/> value.
    /// </summary>
    /// <remarks>It's still possible for a value to evaluate to <see langword="null"/> even if this is <see langword="false"/>.</remarks>
    bool IsNull { get; }

    /// <summary>
    /// Writes this value to a <see cref="Utf8JsonWriter"/> in a way that it can be recreated later.
    /// </summary>
    void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options);

    /// <summary>
    /// Attempts to invoke <see cref="IValueVisitor.Accept"/> on the current value, but only when the value can be determined without context.
    /// </summary>
    bool VisitConcreteValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    ;

    /// <summary>
    /// Attempts to invoke <see cref="IValueVisitor.Accept"/> on the current value, evaluating with context if necessary.
    /// </summary>
    bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    ;
}

/// <summary>
/// A strongly-typed dynamic value that can be resolved at runtime.
/// </summary>
public interface IValue<TValue> : IValue where TValue : IEquatable<TValue>
{
    /// <summary>
    /// The type stored in this value.
    /// </summary>
    IType<TValue> Type { get; }

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
        /// Attempts to invoke <see cref="IGenericVisitor.Accept"/> on the current value without context.
        /// </summary>
        /// <remarks>If the value is <see langword="null"/>, the visitor will be invoked using a <see langword="null"/> <see cref="string"/> value.</remarks>
        /// <param name="visitor">Visitor which will accept the result.</param>
        /// <returns><see langword="false"/> if the visitor didn't get invoked, otherwise <see langword="true"/>.</returns>
        public unsafe bool VisitConcreteValueGeneric<TVisitor>(ref TVisitor visitor)
            where TVisitor : IGenericVisitor
        {
            ValueVisitor<TVisitor> v;
            v.Visited = false;
            fixed (TVisitor* visitorPtr = &visitor)
            {
                v.Visitor = visitorPtr;
                v.Visited &= value.VisitConcreteValue(ref v);
            }

            return v.Visited;
        }

        /// <summary>
        /// Attempts to invoke <see cref="IGenericVisitor.Accept"/> on the current value, evaluating with context.
        /// </summary>
        /// <remarks>If the value is <see langword="null"/>, the visitor will be invoked using a <see langword="null"/> <see cref="string"/> value.</remarks>
        /// <param name="visitor">Visitor which will accept the result.</param>
        /// <param name="ctx">Workspace context.</param>
        /// <returns><see langword="false"/> if the visitor didn't get invoked, otherwise <see langword="true"/>.</returns>
        public unsafe bool VisitValueGeneric<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
            where TVisitor : IGenericVisitor
        {
            ValueVisitor<TVisitor> v;
            v.Visited = false;
            fixed (TVisitor* visitorPtr = &visitor)
            {
                v.Visitor = visitorPtr;
                v.Visited &= value.VisitValue(ref v, in ctx);
            }

            return v.Visited;
        }

        /// <summary>
        /// Attempts to get the value of the current value without context and convert the result to <typeparamref name="TResult"/> if necessary.
        /// </summary>
        /// <typeparam name="TResult">The destination type.</typeparam>
        /// <param name="ctx">Workspace context.</param>
        /// <param name="result">Converted value.</param>
        /// <returns>Whether or not the value could be determined without context and converted.</returns>
        public bool TryGetConcreteValueAs<TResult>(out Optional<TResult> result) where TResult : IEquatable<TResult>
        {
            ValueConvertVisitor<TResult> v = default;

            if (!value.VisitConcreteValue(ref v))
            {
                result = Optional<TResult>.Null;
                return false;
            }

            result = v.Result;
            return v.Visited;
        }

        /// <summary>
        /// Attempts to get the value of the current value and convert the result to <typeparamref name="TResult"/> if necessary, evaluating with context.
        /// </summary>
        /// <typeparam name="TResult">The destination type.</typeparam>
        /// <param name="ctx">Workspace context.</param>
        /// <param name="result">Converted value.</param>
        /// <returns>Whether or not the value could be determined and converted.</returns>
        public bool TryGetValueAs<TResult>(in FileEvaluationContext ctx, out Optional<TResult> result) where TResult : IEquatable<TResult>
        {
            ValueConvertVisitor<TResult> v = default;

            if (!value.VisitValue(ref v, in ctx))
            {
                result = Optional<TResult>.Null;
                return false;
            }

            result = v.Result;
            return v.Visited;
        }
    }

    private unsafe struct ValueVisitor<TVisitor> : IValueVisitor
        where TVisitor : IGenericVisitor
    {
        public TVisitor* Visitor;
        public bool Visited;

        public void Accept<T>(Optional<T> value) where T : IEquatable<T>
        {
            if (value.HasValue)
            {
                Visitor->Accept(value.Value);
            }
            else if (default(T) == null)
            {
                Visitor->Accept(default(T));
            }
            else
            {
                Visitor->Accept<string>(null);
            }

            Visited = true;
        }
    }

    private struct ValueConvertVisitor<TResult> : IValueVisitor where TResult : IEquatable<TResult>
    {
        public bool Visited;
        public Optional<TResult> Result;

        public void Accept<TValue>(Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            if (!value.HasValue)
            {
                Result = Optional<TResult>.Null;
                Visited = true;
                return;
            }

            Visited = ConvertVisitor<TResult>.TryConvert(value.Value, out Result);
        }
    }
}
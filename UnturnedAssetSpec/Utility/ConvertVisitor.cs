using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Converts the incoming value to <typeparamref name="TResult"/>.
/// </summary>
internal struct ConvertVisitor<TResult> : IGenericVisitor
    where TResult : IEquatable<TResult>
{
    public TResult? Result;
    public bool WasSuccessful;
    public bool IsNull;

    public void Accept<T>(T? value) where T : IEquatable<T>
    {
        if (value == null)
        {
            Result = default;
            WasSuccessful = true;
            IsNull = true;
            return;
        }

        if (typeof(T) == typeof(TResult))
        {
            Result = Unsafe.As<T, TResult>(ref value!);
            WasSuccessful = true;
            IsNull = false;
            return;
        }

        if (TypeConverters.TryGet<T>() is { } typeConverter
            && typeConverter.TryConvertTo(new Optional<T>(value), out Optional<TResult> result))
        {
            Result = result.Value;
            IsNull = !result.HasValue;
            WasSuccessful = true;
        }

        if (typeof(TResult) == typeof(string))
        {
            string? toString = value.ToString();
            Result = MathMatrix.As<string?, TResult?>(toString);
            IsNull = toString == null;
            WasSuccessful = true;
        }
    }

    /// <summary>
    /// Attempts to convert a value of type <typeparamref name="TFrom"/> to a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted value.</param>
    /// <returns>Whether or not the conversion was successful.</returns>
    public static bool TryConvert<TFrom>(TFrom? value, out Optional<TResult> result) where TFrom : IEquatable<TFrom>
    {
        ConvertVisitor<TResult> v = default;
        v.Accept(value);

        if (!v.WasSuccessful)
        {
            result = Optional<TResult>.Null;
            return false;
        }

        result = v.IsNull ? Optional<TResult>.Null : new Optional<TResult>(v.Result);
        return true;
    }

    /// <summary>
    /// Attempts to convert a value of type <typeparamref name="TFrom"/> to a value of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="result">The converted value.</param>
    /// <returns>
    /// Whether or not the conversion was successful.
    /// If a value type attempts to be converted to <see langword="null"/> this will also return <see langword="false"/>, so use the overload which returns an <see cref="Optional{T}"/> instead if needed.
    /// </returns>
    public static bool TryConvert<TFrom>(TFrom? value, out TResult? result) where TFrom : IEquatable<TFrom>
    {
        if (TryConvert(value, out Optional<TResult> optionalResult) && optionalResult.TryGetValueOrNull(out result))
            return true;

        result = default;
        return false;
    }
}
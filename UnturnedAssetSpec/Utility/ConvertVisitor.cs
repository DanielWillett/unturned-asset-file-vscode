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
    }
}
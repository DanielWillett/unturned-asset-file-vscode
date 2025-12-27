using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

public interface IExpressionFunction
{
    string FunctionName { get; }
    int ArgumentCountMask { get; }

    /// <summary>
    /// Evaluate the function with 0 arguments.
    /// </summary>
    bool Evaluate<TOut, TVisitor>(ref TVisitor visitor)
        where TOut : IEquatable<TOut>
        where TVisitor : IGenericVisitor;

    /// <summary>
    /// Evaluate the function with 1 argument.
    /// </summary>
    bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TOut : IEquatable<TOut>
        where TVisitor : IGenericVisitor;

    /// <summary>
    /// Evaluate the function with 2 arguments.
    /// </summary>
    bool Evaluate<TIn1, TIn2, TOut, TVisitor>(TIn1 v1, TIn2 v2, ref TVisitor visitor)
        where TIn1 : IEquatable<TIn1>
        where TIn2 : IEquatable<TIn2>
        where TOut : IEquatable<TOut>
        where TVisitor : IGenericVisitor;

    /// <summary>
    /// Evaluate the function with 3 arguments.
    /// </summary>
    bool Evaluate<TIn1, TIn2, TIn3, TOut, TVisitor>(TIn1 v1, TIn2 v2, TIn3 v3, ref TVisitor visitor)
        where TIn1 : IEquatable<TIn1>
        where TIn2 : IEquatable<TIn2>
        where TIn3 : IEquatable<TIn3>
        where TOut : IEquatable<TOut>
        where TVisitor : IGenericVisitor;
}

public abstract class ExpressionFunction : IExpressionFunction
{
    public abstract string FunctionName { get; }
    public abstract int ArgumentCountMask { get; }

    public virtual bool ReduceToKnownTypes => true;

    public virtual bool Evaluate<TOut, TVisitor>(ref TVisitor visitor)
        where TOut : IEquatable<TOut>
        where TVisitor : IGenericVisitor
    {
        return false;
    }

    public virtual bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TOut : IEquatable<TOut>
        where TVisitor : IGenericVisitor
    {
        return false;
    }

    public virtual bool Evaluate<TIn1, TIn2, TOut, TVisitor>(TIn1 v1, TIn2 v2, ref TVisitor visitor)
        where TIn1 : IEquatable<TIn1>
        where TIn2 : IEquatable<TIn2>
        where TOut : IEquatable<TOut>
        where TVisitor : IGenericVisitor
    {
        return false;
    }

    public virtual bool Evaluate<TIn1, TIn2, TIn3, TOut, TVisitor>(TIn1 v1, TIn2 v2, TIn3 v3, ref TVisitor visitor)
        where TIn1 : IEquatable<TIn1>
        where TIn2 : IEquatable<TIn2>
        where TIn3 : IEquatable<TIn3>
        where TOut : IEquatable<TOut>
        where TVisitor : IGenericVisitor
    {
        return false;
    }

    protected static string ToString<T>(T value)
        where T : IEquatable<T>
    {
        if (typeof(T) == typeof(string))
        {
            return Unsafe.As<T, string>(ref value);
        }

        if (TypeConverters.TryGet<T>() is { } converter)
        {
            TypeConverterFormatArgs f = TypeConverterFormatArgs.Default;

            return converter.Format(value, ref f);
        }

        if (value is IFormattable formattable)
        {
            return formattable.ToString(null, CultureInfo.InvariantCulture);
        }

        return value == null ? string.Empty : (value.ToString() ?? string.Empty);
    }

    protected static bool TryGetDouble<T>(T? value, out double dbl)
        where T : IEquatable<T>
    {
        if (typeof(T) == typeof(double))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, double>(value!);
            return true;
        }
        if (typeof(T) == typeof(float))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, float>(value!);
            return true;
        }
        if (typeof(T) == typeof(decimal))
        {
            dbl = (double)SpecDynamicExpressionTreeValueHelpers.As<T, decimal>(value!);
            return true;
        }
        if (typeof(T) == typeof(byte))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, byte>(value!);
            return true;
        }
        if (typeof(T) == typeof(sbyte))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, sbyte>(value!);
            return true;
        }
        if (typeof(T) == typeof(ushort))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, ushort>(value!);
            return true;
        }
        if (typeof(T) == typeof(short))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, short>(value!);
            return true;
        }
        if (typeof(T) == typeof(uint))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, uint>(value!);
            return true;
        }
        if (typeof(T) == typeof(int))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, int>(value!);
            return true;
        }
        if (typeof(T) == typeof(ulong))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, ulong>(value!);
            return true;
        }
        if (typeof(T) == typeof(long))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, long>(value!);
            return true;
        }
        if (typeof(T) == typeof(bool))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, bool>(value!) ? 1d : 0d;
            return true;
        }
        if (typeof(T) == typeof(char))
        {
            char c = SpecDynamicExpressionTreeValueHelpers.As<T, char>(value!);
            if (c is >= '0' and <= '9')
            {
                dbl = c - '0';
                return true;
            }

            dbl = 0;
            return false;
        }
        if (typeof(T) == typeof(GuidOrId))
        {
            GuidOrId guidOrId = SpecDynamicExpressionTreeValueHelpers.As<T, GuidOrId>(value!);
            dbl = guidOrId.Id;
            return guidOrId.IsId;
        }
        if (typeof(T) == typeof(DateTime))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, DateTime>(value!).Ticks;
            return true;
        }
        if (typeof(T) == typeof(DateTimeOffset))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, DateTimeOffset>(value!).Ticks;
            return true;
        }
        if (typeof(T) == typeof(TimeSpan))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, TimeSpan>(value!).Ticks;
            return true;
        }
        if (typeof(T) == typeof(nint))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, nint>(value!);
            return true;
        }
        if (typeof(T) == typeof(nuint))
        {
            dbl = SpecDynamicExpressionTreeValueHelpers.As<T, nuint>(value!);
            return true;
        }
#if NET5_0_OR_GREATER
        if (typeof(T) == typeof(Half))
        {
            dbl = (double)SpecDynamicExpressionTreeValueHelpers.As<T, Half>(value!);
            return true;
        }
#endif
#if NET7_0_OR_GREATER
        if (typeof(T) == typeof(UInt128))
        {
            dbl = (double)SpecDynamicExpressionTreeValueHelpers.As<T, UInt128>(value!);
            return true;
        }
        if (typeof(T) == typeof(Int128))
        {
            dbl = (double)SpecDynamicExpressionTreeValueHelpers.As<T, Int128>(value!);
            return true;
        }
#endif

        if (typeof(T) == typeof(string))
        {
            return double.TryParse(
                SpecDynamicExpressionTreeValueHelpers.As<T, string>(value!),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out dbl
            );
        }
        dbl = 0;
        return false;
    }
}
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

[SkipLocalsInit]
public static partial class MathMatrix
{
    public static TTo As<TFrom, TTo>(TFrom fromVal)
    {
        return Unsafe.As<TFrom, TTo>(ref fromVal);
    }

    private static void ConvertToNumber<TIdealOut, TVisitor>(string str, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
    {
        if (str == null || str.Equals("NULL", StringComparison.OrdinalIgnoreCase))
        {
            visitor.Accept<string>(null);
            return;
        }

        if (typeof(TIdealOut) == typeof(int))
        {
            if (int.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out int i4))
            {
                visitor.Accept(i4);
                return;
            }
        }
        else if (typeof(TIdealOut) == typeof(uint) || typeof(TIdealOut) == typeof(char) || typeof(TIdealOut) == typeof(bool))
        {
            if (uint.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out uint u4))
            {
                visitor.Accept(u4);
                return;
            }
        }
        else if (typeof(TIdealOut) == typeof(long) || typeof(TIdealOut) == typeof(nint))
        {
            if (long.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out long i8))
            {
                visitor.Accept(i8);
                return;
            }
        }
        else if (typeof(TIdealOut) == typeof(ulong) || typeof(TIdealOut) == typeof(nuint))
        {
            if (ulong.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong u8))
            {
                visitor.Accept(u8);
                return;
            }
        }
        else if (typeof(TIdealOut) == typeof(short))
        {
            if (short.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out short i2))
            {
                visitor.Accept(i2);
                return;
            }
        }
        else if (typeof(TIdealOut) == typeof(ushort) || typeof(TIdealOut) == typeof(GuidOrId))
        {
            if (ushort.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out ushort u2))
            {
                visitor.Accept(u2);
                return;
            }
        }
        else if (typeof(TIdealOut) == typeof(sbyte))
        {
            if (sbyte.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out sbyte i1))
            {
                visitor.Accept(i1);
                return;
            }
        }
        else if (typeof(TIdealOut) == typeof(byte))
        {
            if (byte.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out byte u1))
            {
                visitor.Accept(u1);
                return;
            }
        }
        else if (typeof(TIdealOut) == typeof(float))
        {
            if (float.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out float r4))
            {
                visitor.Accept(r4);
                return;
            }
        }
        else if (typeof(TIdealOut) == typeof(double))
        {
            if (double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out double r8))
            {
                visitor.Accept(r8);
                return;
            }
        }
        else if (typeof(TIdealOut) == typeof(decimal))
        {
            if (decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal r16))
            {
                visitor.Accept(r16);
                return;
            }
        }

        int decimalIndex = str.IndexOf('.');
        if (decimalIndex < 0)
        {
            if (int.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out int i4))
            {
                visitor.Accept(i4);
                return;
            }
            if (uint.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out uint u4))
            {
                visitor.Accept(u4);
                return;
            }
            if (long.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out long i8))
            {
                visitor.Accept(i8);
                return;
            }
            if (ulong.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong u8))
            {
                visitor.Accept(u8);
                return;
            }
            
            if (str.AsSpan().Trim().Equals(NumberFormatInfo.InvariantInfo.PositiveInfinitySymbol, StringComparison.OrdinalIgnoreCase))
            {
                if (typeof(TIdealOut) == typeof(float))
                    visitor.Accept(float.PositiveInfinity);
                else
                    visitor.Accept(double.PositiveInfinity);
            }
            else if (str.AsSpan().Trim().Equals(NumberFormatInfo.InvariantInfo.NegativeInfinitySymbol, StringComparison.OrdinalIgnoreCase))
            {
                if (typeof(TIdealOut) == typeof(float))
                    visitor.Accept(float.NegativeInfinity);
                else
                    visitor.Accept(double.NegativeInfinity);
            }
            else if (str.AsSpan().Trim().Equals(NumberFormatInfo.InvariantInfo.NaNSymbol, StringComparison.OrdinalIgnoreCase))
            {
                if (typeof(TIdealOut) == typeof(float))
                    visitor.Accept(float.NaN);
                else
                    visitor.Accept(double.NaN);
            }
        }
        else
        {
            if (double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out double r8))
            {
                visitor.Accept(r8);
                return;
            }
            if (decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal r16))
            {
                visitor.Accept(r16);
                return;
            }
            if (bool.TryParse(str, out bool b))
            {
                visitor.Accept(b ? 1u : 0u);
                return;
            }
        }
    }
    private static byte AbsSafe(sbyte i1)
    {
        return i1 < 0 ? (byte)-i1 : (byte)i1;
    }
    private static ushort AbsSafe(short i2)
    {
        return i2 < 0 ? (ushort)-i2 : (ushort)i2;
    }
    private static uint AbsSafe(int i4)
    {
        return i4 < 0 ? (uint)-i4 : (uint)i4;
    }
    private static ulong AbsSafe(long i8)
    {
        return i8 < 0 ? (ulong)-i8 : (ulong)i8;
    }

#if SUBSEQUENT_COMPILE
    private static TTo ConvertVector<TTo, TFrom>(IVectorTypeProvider<TTo> vectorProviderTo, IVectorTypeProvider<TFrom> vectorProviderFrom, TFrom fromVector)
        where TTo : IEquatable<TTo>
        where TFrom : IEquatable<TFrom>
    {
        Span<double> components = stackalloc double[vectorProviderFrom.Size];
        vectorProviderFrom.Deconstruct(fromVector, components);
        return vectorProviderTo.Construct(components);
    }
#endif
}

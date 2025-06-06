using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

internal static class BoxedPrimitives
{
    public static readonly object True = true;
    public static readonly object False = false;
    public static readonly object I10 = (sbyte)0;
    public static readonly object U10 = (byte)0;
    public static readonly object I20 = (short)0;
    public static readonly object U20 = (ushort)0;
    public static readonly object I40 = 0;
    public static readonly object U40 = 0u;
    public static readonly object I80 = 0L;
    public static readonly object U80 = 0UL;
    public static readonly object R40 = 0f;
    public static readonly object R80 = 0d;
    public static readonly object R160 = 0m;

    public static object? Box<T>(ref T? value)
    {
        if (typeof(T) == typeof(bool))
        {
            return Unsafe.As<T?, bool>(ref value) ? True : False;
        }
        if (typeof(T) == typeof(sbyte))
        {
            return Unsafe.As<T?, sbyte>(ref value) == 0 ? I10 : value;
        }
        if (typeof(T) == typeof(byte))
        {
            return Unsafe.As<T?, byte>(ref value) == 0 ? U10 : value;
        }
        if (typeof(T) == typeof(short))
        {
            return Unsafe.As<T?, short>(ref value) == 0 ? I20 : value;
        }
        if (typeof(T) == typeof(ushort))
        {
            return Unsafe.As<T?, ushort>(ref value) == 0 ? U20 : value;
        }
        if (typeof(T) == typeof(int))
        {
            return Unsafe.As<T?, int>(ref value) == 0 ? I40 : value;
        }
        if (typeof(T) == typeof(uint))
        {
            return Unsafe.As<T?, uint>(ref value) == 0 ? U40 : value;
        }
        if (typeof(T) == typeof(long))
        {
            return Unsafe.As<T?, long>(ref value) == 0 ? I80 : value;
        }
        if (typeof(T) == typeof(ulong))
        {
            return Unsafe.As<T?, ulong>(ref value) == 0 ? U80 : value;
        }
        if (typeof(T) == typeof(float))
        {
            return Unsafe.As<T?, float>(ref value) == 0f ? R40 : value;
        }
        if (typeof(T) == typeof(double))
        {
            return Unsafe.As<T?, double>(ref value) == 0d ? R80 : value;
        }
        if (typeof(T) == typeof(decimal))
        {
            return Unsafe.As<T?, decimal>(ref value) == decimal.Zero ? R160 : value;
        }

        return value;
    }
}
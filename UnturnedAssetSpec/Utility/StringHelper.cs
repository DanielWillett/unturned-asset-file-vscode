using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class StringHelper
{
    public static bool ContainsWhitespace(string str)
    {
        for (int i = 0; i < str.Length; ++i)
        {
            if (char.IsWhiteSpace(str, i))
                return true;
        }

        return false;
    }

    public static bool ContainsWhitespace(ReadOnlySpan<char> str)
    {
        for (int i = 0; i < str.Length; ++i)
        {
            if (char.IsWhiteSpace(str[i]))
                return true;
        }

        return false;
    }

    public static bool ContainsWhitespace(StringBuilder sb)
    {
        for (int i = 0; i < sb.Length; ++i)
        {
            if (char.IsWhiteSpace(sb[i]))
                return true;
        }

        return false;
    }

    public static int CountDigits(int value)
    {
        return value != 0 ? 1 + (int)Math.Log10(Math.Abs(value)) : 1;
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static void AppendSpan(StringBuilder sb, ReadOnlySpan<char> span)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        sb.Append(span);
#else
        if (span.IsEmpty)
            return;

        unsafe
        {
            fixed (char* ptr = span)
            {
                sb.Append(ptr, span.Length);
            }
        }
#endif
    }
}

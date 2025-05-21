using System;
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
}

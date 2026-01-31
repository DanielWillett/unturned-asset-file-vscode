using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class StringHelper
{
    /// <summary>
    /// Find the next index of an unescaped character.
    /// </summary>
    /// <param name="span">The span of characters to search</param>
    /// <param name="stops">List of all characters to stop on, INCLUDING '\'.</param>
    /// <param name="useDepth">Whether or not to only return values at the current depth. Assumes the first character of <paramref name="span"/> is already within the depth required.</param>
    /// <returns>The index of the next unescaped match, or -1 if none are found.</returns>
    public static int NextUnescapedIndexOf(ReadOnlySpan<char> span, ReadOnlySpan<char> stops, out bool hadEscapeSequences, bool useDepth = false)
    {
        hadEscapeSequences = false;
        int firstIndex = span.IndexOfAny(stops);
        if (firstIndex < 0)
            return -1;

        int index = firstIndex;
        int escCount = 0;
        Span<int> depths = stackalloc int[stops.Length];
        while (true)
        {
            char c = span[index];
            switch (c)
            {
                case '\\':
                    hadEscapeSequences = true;
                    ++escCount;
                    break;

                default:
                    if (escCount % 2 == 1)
                    {
                        escCount = 0;
                        break;
                    }

                    if (!useDepth)
                        return index;
                    
                    int depth = GetDepthChange(c);
                    if (depth != 0)
                    {
                        int foundIndex = stops.IndexOf(c);
                        depths[foundIndex] += depth;
                        for (int i = 0; i < depths.Length; ++i)
                        {
                            if (depths[i] > 0)
                                break;
                        }

                        return index;
                    }

                    break;
            }

            int nextIndex = span.Slice(index + 1).IndexOfAny(stops);
            if (nextIndex >= 0)
                index = nextIndex + index + 1;
            else
                return -1;
        }
    }

    private static int GetDepthChange(char c)
    {
        return c switch
        {
            '(' or '[' or '{' or '<' => 1,
            ')' or ']' or '}' or '>' => -1,
            _ => 0
        };
    }

    /// <summary>
    /// Unescape text. Any forward slash will evaluate to the following character.
    /// </summary>
    public static string Unescape(ReadOnlySpan<char> text)
    {
        return Unescape(text, ReadOnlySpan<char>.Empty);
    }

    /// <summary>
    /// Unescape text. Any forward slash will evaluate to the following character if that character is in <paramref name="validEscapedCharacters"/>.
    /// </summary>
    public static unsafe string Unescape(ReadOnlySpan<char> text, ReadOnlySpan<char> validEscapedCharacters)
    {
        if (text.IsEmpty)
            return string.Empty;

        int length = text.Length;

        int firstSlash = text.IndexOf('\\');
        if (firstSlash == -1 || firstSlash >= length)
        {
            return text.ToString();
        }

        char* newValue = stackalloc char[length];

        int prevIndex = -1;
        int slashCount = 0;
        int writeIndex = 0;
        while (true)
        {
            int nextSlash = prevIndex != length - 1
                ? prevIndex == -1
                    ? firstSlash
                    : text.Slice(prevIndex + 1).IndexOf('\\')
                : -1;
            if (nextSlash >= 0)
                nextSlash += prevIndex + 1;

            if (nextSlash >= length)
                nextSlash = -1;
            if (nextSlash == prevIndex + 1)
            {
                ++slashCount;
            }
            else if (nextSlash == -1)
            {
                if (prevIndex + 1 >= length || slashCount == 0)
                {
                    for (int i = prevIndex + 1; i < length; ++i)
                    {
                        newValue[writeIndex] = text[i];
                        ++writeIndex;
                    }

                    break;
                }
            }
            else
            {
                slashCount = 1;
            }

            int max = nextSlash - slashCount + 1;
            for (int i = prevIndex + 1; i < max; ++i)
            {
                newValue[writeIndex] = text[i];
                ++writeIndex;
            }

            if (slashCount == 1)
            {
                if (nextSlash < length - 1)
                {
                    switch (text[nextSlash + 1])
                    {
                        case 'n':
                            newValue[writeIndex] = '\n';
                            ++writeIndex;
                            break;

                        case 'r':
                            newValue[writeIndex] = '\r';
                            ++writeIndex;
                            break;

                        case 't':
                            newValue[writeIndex] = '\t';
                            ++writeIndex;
                            break;

                        case 'u' when TryParseUnicodeSequence(text, nextSlash, out char unicodeCharacter):
                            newValue[writeIndex] = unicodeCharacter;
                            ++writeIndex;
                            nextSlash += 4;
                            break;

                        default:
                            char v = text[nextSlash + 1];
                            if (!validEscapedCharacters.IsEmpty && validEscapedCharacters.IndexOf(v) < 0)
                            {
                                newValue[writeIndex] = '\\';
                                ++writeIndex;
                            }

                            newValue[writeIndex] = v;
                            ++writeIndex;
                            break;
                    }

                    ++nextSlash;
                }
                else
                {
                    newValue[writeIndex] = '\\';
                    ++writeIndex;
                    break;
                }

                slashCount = 0;
            }

            if (nextSlash == -1)
                break;

            prevIndex = nextSlash;
        }

        return new string(newValue, 0, writeIndex);
    }

    private static readonly char[] Escapables = [ '\n', '\r', '\t', '\\' ];

    public static void EscapeValue(ref string value)
    {
        EscapeValue(ref value, Escapables);
    }

    public static void EscapeValue(ref string value
#if !NETSTANDARD2_1_OR_GREATER && !NET5_0_OR_GREATER
        , char[] escapables
#else
        , ReadOnlySpan<char> escapables
#endif
        , int startIndex = -1
    )
    {
        int c = 0;
        string s = value;
        for (int i = Math.Max(0, startIndex); i < s.Length; ++i)
        {
#if !NETSTANDARD2_1_OR_GREATER && !NET5_0_OR_GREATER
            if (Array.IndexOf(escapables, s[i]) >= 0)
#else
            if (escapables.IndexOf(s[i]) >= 0)
#endif
                ++c;
        }

        if (c <= 0)
        {
            return;
        }

        unsafe
        {
            char* newValue = stackalloc char[s.Length + c];

            int prevIndex = -1;
            int writeIndex = 0;
            while (true)
            {
#if !NETSTANDARD2_1_OR_GREATER && !NET5_0_OR_GREATER
                int index = s.IndexOfAny(escapables, prevIndex + 1);
#else
                int index = s.AsSpan(prevIndex + 1).IndexOfAny(escapables);
                if (index >= 0)
                    index += prevIndex + 1;
#endif
                if (index == -1)
                {
                    for (int i = prevIndex + 1; i < s.Length; ++i)
                    {
                        newValue[writeIndex] = s[i];
                        ++writeIndex;
                    }
                    break;
                }

                for (int i = prevIndex + 1; i < index; ++i)
                {
                    newValue[writeIndex] = s[i];
                    ++writeIndex;
                }

                char self = s[index];
                newValue[writeIndex] = '\\';
                newValue[writeIndex + 1] = self switch
                {
                    '\n' => 'n',
                    '\r' => 'r',
                    '\t' => 't',
                    _ => self
                };

                writeIndex += 2;

                prevIndex = index;
            }

            value = new string(newValue, 0, writeIndex);
        }
    }

    private static bool TryParseUnicodeSequence(ReadOnlySpan<char> value, int slash, out char c)
    {
        if (value.Length - slash - 2 < 4)
        {
            c = '\0';
            return false;
        }

#if NETSTANDARD2_1_OR_GREATER
        ReadOnlySpan<char> num = value.Slice(slash + 2, 4);
#else
        string num = value.Slice(slash + 2, 4).ToString();
#endif
        if (!ushort.TryParse(num, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ushort unicode))
        {
            c = '\0';
            return false;
        }

        c = (char)unicode;
        return true;
    }

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

    /// <summary>
    /// Counts the digits in a <see cref="sbyte"/>.
    /// </summary>
    /// <param name="n">The number to count digits from.</param>
    /// <param name="minus">Whether or not to include the negative sign in the count.</param>
    public static int CountDigits(sbyte n, bool minus = true)
    {
        if (n < 0)
        {
            if (n == sbyte.MinValue)
                return 3 + (minus ? 1 : 0);
            n = (sbyte)-n;
        }
        else
        {
            minus = false;
        }

        int v;
        if (n <= 9) // 0 .. 9
        {
            v = 1;
        }
        else // 10 .. sbyte.MaxValue
        {
            v = n <= 99 ? 2 : 3;
        }

        v += minus ? 1 : 0;

        return v;
    }

    /// <summary>
    /// Counts the digits in a <see cref="byte"/>.
    /// </summary>
    /// <param name="n">The number to count digits from.</param>
    public static int CountDigits(byte n)
    {
        if (n <= 9) // 0 .. 9
        {
            return 1;
        }

        // 10 .. sbyte.MaxValue
        return n <= 99 ? 2 : 3;
    }

    // 'binary search' implementation from https://stackoverflow.com/a/59209589

    /// <summary>
    /// Counts the digits in a <see cref="short"/>.
    /// </summary>
    /// <param name="n">The number to count digits from.</param>
    /// <param name="minus">Whether or not to include the negative sign in the count.</param>
    public static int CountDigits(short n, bool minus = true)
    {
        if (n < 0)
        {
            if (n == short.MinValue)
                return 5 + (minus ? 1 : 0);
            n = (short)-n;
        }
        else
        {
            minus = false;
        }

        int v;
        if (n <= 999) // 0 .. 999
        {
            if (n <= 9) // 0 .. 9
            {
                v = 1;
            }
            else // 10 .. 999
            {
                v = n <= 99 ? 2 : 3;
            }
        }
        else // 1000 .. short.MaxValue
        {
            v = n <= 9999 ? 4 : 5;
        }

        v += minus ? 1 : 0;

        return v;
    }

    /// <summary>
    /// Counts the digits in a <see cref="ushort"/>.
    /// </summary>
    /// <param name="n">The number to count digits from.</param>
    public static int CountDigits(ushort n)
    {
        if (n <= 999) // 0 .. 999
        {
            if (n <= 9) // 0 .. 9
            {
                return 1;
            }

            // 10 .. 999
            return n <= 99 ? 2 : 3;
        }

        // 1000 .. ushort.MaxValue
        return n <= 9999 ? 4 : 5;
    }

    /// <summary>
    /// Counts the digits in a <see cref="int"/>.
    /// </summary>
    /// <param name="n">The number to count digits from.</param>
    /// <param name="minus">Whether or not to include the negative sign in the count.</param>
    public static int CountDigits(int n, bool minus = true)
    {
        if (n < 0)
        {
            if (n == int.MinValue)
                return 10 + (minus ? 1 : 0);
            n = -n;
        }
        else
        {
            minus = false;
        }

        int v;
        if (n <= 9999) // 0 .. 9999
        {
            if (n <= 99) // 0 .. 99
            {
                v = n <= 9 ? 1 : 2;
            }
            else // 100 .. 9999
            {
                v = n <= 999 ? 3 : 4;
            }
        }
        else // 10000 .. uint.MaxValue
        {
            if (n <= 9_999_999) // 10000 .. 9,999,999
            {
                v = n switch
                {
                    <= 99_999 => 5,
                    <= 999_999 => 6,
                    _ => 7
                };
            }
            else // 10,000,000 .. uint.MaxValue
            {
                v = n switch
                {
                    <= 99_999_999 => 8,
                    <= 999_999_999 => 9,
                    _ => 10
                };
            }
        }

        v += minus ? 1 : 0;

        return v;
    }

    /// <summary>
    /// Counts the digits in a <see cref="uint"/>.
    /// </summary>
    /// <param name="n">The number to count digits from.</param>
    public static int CountDigits(uint n)
    {
        if (n <= 9999) // 0 .. 9999
        {
            if (n <= 99) // 0 .. 99
            {
                return n <= 9 ? 1 : 2;
            }

            // 100 .. 9999
            return n <= 999 ? 3 : 4;
        }

        // 10000 .. int.MaxValue

        if (n <= 9_999_999)
        {
            // 10000 .. 9,999,999
            return n switch
            {
                <= 99_999 => 5,
                <= 999_999 => 6,
                _ => 7
            };
        }

        // 10,000,000 .. int.MaxValue
        return n switch
        {
            <= 99_999_999 => 8,
            <= 999_999_999 => 9,
            _ => 10
        };
    }

    /// <summary>
    /// Counts the digits in a <see cref="long"/>.
    /// </summary>
    /// <param name="n">The number to count digits from.</param>
    /// <param name="minus">Whether or not to include the negative sign in the count.</param>
    public static int CountDigits(long n, bool minus = true)
    {
        if (n < 0)
        {
            if (n == long.MinValue)
                return 19 + (minus ? 1 : 0);
            n = -n;
        }
        else
        {
            minus = false;
        }

        int v;
        if (n <= 9_999_999_999) // 0 .. 9,999,999,999
        {
            if (n <= 99_999) // 0 .. 99,999
            {
                if (n <= 999) // 0 .. 999
                {
                    v = n switch
                    {
                        <= 9 => 1,
                        <= 99 => 2,
                        _ => 3
                    };
                }
                else // 1000 .. 99,999
                {
                    v = n switch
                    {
                        <= 9_999 => 4,
                        _ => 5
                    };
                }
            }
            else // 100,000 .. 9,999,999,999
            {
                if (n <= 99_999_999) // 0 .. 99,999,999
                {
                    v = n switch
                    {
                        <= 999_999 => 6,
                        <= 9_999_999 => 7,
                        _ => 8
                    };
                }
                else // 100,000,000 .. 9,999,999,999
                {
                    v = n switch
                    {
                        <= 999_999_999 => 9,
                        _ => 10
                    };
                }
            }
        }
        else // 10,000,000,000 .. long.MaxValue
        {
            if (n <= 999_999_999_999_999) // 10,000,000,000 .. 999,999,999,999,999
            {
                if (n <= 999_999_999_999) // 10,000,000,000 .. 999,999,999,999
                {
                    v = n switch
                    {
                        <= 99_999_999_999 => 11,
                        _ => 12
                    };
                }
                else // 1,000,000,000,000 .. 999,999,999,999,999
                {
                    v = n switch
                    {
                        <= 9_999_999_999_999 => 13,
                        <= 99_999_999_999_999 => 14,
                        _ => 15
                    };
                }
            }
            else // 999,999,999,999,999 .. long.MaxValue
            {
                if (n <= 99_999_999_999_999_999) // 999,999,999,999,999 .. 99,999,999,999,999,999
                {
                    v = n switch
                    {
                        <= 9_999_999_999_999_999 => 16,
                        _ => 17
                    };
                }
                else // 100,000,000,000,000,000 .. long.MaxValue
                {
                    v = n switch
                    {
                        <= 999_999_999_999_999_999 => 18,
                        _ => 19
                    };
                }
            }
        }

        v += minus ? 1 : 0;

        return v;
    }

    /// <summary>
    /// Counts the digits in a <see cref="ulong"/>.
    /// </summary>
    /// <param name="n">The number to count digits from.</param>
    public static int CountDigits(ulong n)
    {
        if (n <= 9_999_999_999) // 0 .. 9,999,999,999
        {
            if (n <= 99_999) // 0 .. 99,999
            {
                if (n <= 999) // 0 .. 999
                {
                    return n switch
                    {
                        <= 9 => 1,
                        <= 99 => 2,
                        _ => 3
                    };
                }

                // 1000 .. 99,999
                return n switch
                {
                    <= 9_999 => 4,
                    _ => 5
                };
            }

            // 100,000 .. 9,999,999,999
            if (n <= 99_999_999) // 0 .. 99,999,999
            {
                return n switch
                {
                    <= 999_999 => 6,
                    <= 9_999_999 => 7,
                    _ => 8
                };
            }

            // 100,000,000 .. 9,999,999,999
            return n switch
            {
                <= 999_999_999 => 9,
                _ => 10
            };
        }

        // 10,000,000,000 .. long.MaxValue

        if (n <= 999_999_999_999_999) // 10,000,000,000 .. 999,999,999,999,999
        {
            if (n <= 999_999_999_999) // 10,000,000,000 .. 999,999,999,999
            {
                return n switch
                {
                    <= 99_999_999_999 => 11,
                    _ => 12
                };
            }

            // 1,000,000,000,000 .. 999,999,999,999,999
            return n switch
            {
                <= 9_999_999_999_999 => 13,
                <= 99_999_999_999_999 => 14,
                _ => 15
            };
        }

        // 999,999,999,999,999 .. long.MaxValue
        if (n <= 99_999_999_999_999_999) // 999,999,999,999,999 .. 99,999,999,999,999,999
        {
            return n switch
            {
                <= 9_999_999_999_999_999 => 16,
                _ => 17
            };
        }

        // 100,000,000,000,000,000 .. long.MaxValue
        return n switch
        {
            <= 999_999_999_999_999_999 => 18,
            <= 9_999_999_999_999_999_999 => 19,
            _ => 20
        };
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

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static void WriteSpan(TextWriter writer, ReadOnlySpan<char> span)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        writer.Write(span);
#else
        switch (span.Length)
        {
            case 0:
                return;

            case 1:
                writer.Write(span[0]);
                return;

            default:
                if (span.Length < 256)
                {
                    char[] arr = System.Buffers.ArrayPool<char>.Shared.Rent(span.Length);
                    try
                    {
                        span.CopyTo(arr.AsSpan());
                        writer.Write(arr, 0, span.Length);
                    }
                    finally
                    {
                        System.Buffers.ArrayPool<char>.Shared.Return(arr);
                    }
                }
                else
                {
                    string str = span.ToString();
                    writer.Write(str);
                }

                return;
        }
#endif
    }

    /// <summary>
    /// Used with TryParse methods to return a span on newer platforms and a string on older platforms.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    internal static ReadOnlySpan<char> AsParsable(ReadOnlySpan<char> c) => c;
#else
    internal static string AsParsable(ReadOnlySpan<char> c) => c.ToString();
#endif

    /// <summary>
    /// Gets the highest formatting argument index ({n}) in the given string.
    /// </summary>
    public static int GetHighestFormattingArgument(ReadOnlySpan<char> str)
    {
        int i = str.IndexOf('{');
        if (i < 0)
            return -1;

        bool inFmt = false;
        int lastNumberStartIndex = -1;
        bool inFmtSpecifier = false;

        int maxValue = -1;

        for (; i < str.Length; ++i)
        {
            char c = str[i];
            int fmt;
            switch (c)
            {
                case '{':
                    if (i + 1 < str.Length && str[i + 1] == '{')
                    {
                        ++i;
                        continue;
                    }

                    inFmt = true;
                    break;

                case '}':
                    if (!inFmt)
                        continue;

                    if (!inFmtSpecifier)
                    {
                        if (lastNumberStartIndex != -1
                            && int.TryParse(AsParsable(str.Slice(lastNumberStartIndex, i - lastNumberStartIndex)), NumberStyles.Number, CultureInfo.InvariantCulture, out fmt))
                        {
                            maxValue = Math.Max(fmt, maxValue);
                        }
                    }
                    inFmtSpecifier = false;
                    inFmt = false;
                    lastNumberStartIndex = -1;
                    int nextIndex = str.Slice(i + 1).IndexOf('{');
                    if (nextIndex < 0)
                        return maxValue;

                    i += nextIndex;
                    break;
                
                case ':':
                    if (!inFmt)
                        continue;
                    inFmtSpecifier = true;
                    if (lastNumberStartIndex != -1
                        && int.TryParse(AsParsable(str.Slice(lastNumberStartIndex, i - lastNumberStartIndex)), NumberStyles.Number, CultureInfo.InvariantCulture, out fmt))
                    {
                        maxValue = Math.Max(fmt, maxValue);
                    }
                    lastNumberStartIndex = -1;
                    break;

                default:
                    if (!inFmt || inFmtSpecifier)
                        continue;

                    if (lastNumberStartIndex == -1 && char.IsDigit(c))
                    {
                        lastNumberStartIndex = i;
                    }

                    break;
            }
        }

        return maxValue;
    }
}

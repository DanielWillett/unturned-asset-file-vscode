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
}

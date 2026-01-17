using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Various methods to parse primitive types in the same manner as Unturned does.
/// </summary>
public static class KnownTypeValueHelper
{
    public static bool TryParseBoolean(string key, out bool value)
    {
        if (!string.IsNullOrEmpty(key))
        {
            if (key.Length != 1)
                return bool.TryParse(key, out value);
            switch (key[0])
            {
                case '0':
                case 'f':
                case 'n':
                    value = false;
                    return true;
                case '1':
                case 't':
                case 'y':
                    value = true;
                    return true;
            }
        }

        value = false;
        return false;
    }

    public static bool TryParseUInt8(string key, out byte value)
    {
        return byte.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseUInt16(string key, out ushort value)
    {
        return ushort.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseUInt32(string key, out uint value)
    {
        return uint.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseUInt64(string key, out ulong value)
    {
        return ulong.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseInt8(string key, out sbyte value)
    {
        return sbyte.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseInt16(string key, out short value)
    {
        return short.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseInt32(string key, out int value)
    {
        return int.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseInt64(string key, out long value)
    {
        return long.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseFloat(string key, out float value)
    {
        return float.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDouble(string key, out double value)
    {
        return double.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDecimal(string key, out decimal value)
    {
        return decimal.TryParse(key, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseCharacter(string key, out char value)
    {
        if (key.Length != 1)
        {
            value = '\0';
            return false;
        }

        value = key[0];
        return true;
    }

    internal static readonly char[] InvalidTypeChars =
    [
        '\\',
        ':',
        '/'
    ];

    public static bool TryParseType(string key, out QualifiedType value)
    {
        if (string.IsNullOrEmpty(key) || key.IndexOfAny(InvalidTypeChars) >= 0)
        {
            value = default;
            return false;
        }

        if (!QualifiedType.ExtractParts(key.AsSpan(), out ReadOnlySpan<char> fullTypeName, out _))
        {
            if (fullTypeName.IsEmpty)
            {
                value = default;
                return false;
            }

            Type systemType = typeof(object).Assembly.GetType(key, false, true);
            if (systemType != null)
            {
                value = new QualifiedType(
                    systemType.AssemblyQualifiedName != null
                        ? QualifiedType.NormalizeType(systemType.AssemblyQualifiedName)
                        : systemType.FullName ?? systemType.Name,
                    isCaseInsensitive: true
                );
            }
            else
            {
                value = new QualifiedType(key + ", Assembly-CSharp", true);
            }
        }

        value = new QualifiedType(key, true);
        return true;
    }

    public static bool TryParseType(ReadOnlySpan<char> key, out QualifiedType value)
    {
        if (key.IsEmpty || key.IndexOfAny(InvalidTypeChars.AsSpan()) >= 0)
        {
            value = default;
            return false;
        }

        if (!QualifiedType.ExtractParts(key, out ReadOnlySpan<char> fullTypeName, out _))
        {
            if (fullTypeName.IsEmpty)
            {
                value = default;
                return false;
            }

            Type systemType = typeof(object).Assembly.GetType(fullTypeName.ToString(), false, true);
            if (systemType != null)
            {
                value = new QualifiedType(
                    systemType.AssemblyQualifiedName != null
                        ? QualifiedType.NormalizeType(systemType.AssemblyQualifiedName)
                        : systemType.FullName ?? systemType.Name
                );
            }
            else
            {
                Span<char> outStr = stackalloc char[key.Length + 17];
                key.CopyTo(outStr);
                ", Assembly-CSharp".AsSpan().CopyTo(outStr.Slice(key.Length));
                value = new QualifiedType(outStr.ToString(), true);
            }
        }

        value = new QualifiedType(key.ToString(), true);
        return true;
    }

    public static bool TryParseGuid(string key, out Guid value)
    {
        return Guid.TryParse(key, out value);
    }

    public static bool TryParseDateTime(string key, out DateTime value)
    {
        // idk man this is how its written
        bool success = DateTime.TryParse(key, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out value);
        value = value.ToUniversalTime();
        return success;
    }
    public static bool TryParseDateTimeOffset(string key, out DateTimeOffset value)
    {
        return DateTimeOffset.TryParse(key, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out value);
    }

    public static bool TryParseTimeSpan(string key, out TimeSpan value)
    {
        return TimeSpan.TryParse(key, CultureInfo.InvariantCulture, out value);
    }


    /// <summary>
    /// Regular expression to check for basic rich text.
    /// </summary>
    private static readonly Regex ContainsRichTextRegex =
        new Regex(
            """\<\/{0,1}(?:(?:color=\"{0,1}[#a-z]{0,9}\"{0,1})|(?:color)|(?:#.{3,8})|(?:[ib]))\>""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

    public static bool ContainsRichText(string str)
    {
        return ContainsRichTextRegex.IsMatch(str);
    }

    public static bool TryParseVector3Components(ReadOnlySpan<char> str, out Vector3 value)
    {
        // stolen from nelson
        if (str.IsEmpty)
        {
            value = default;
            return false;
        }

        int paren1 = str.IndexOf('(');
        int startIndex;
        int endIndex;
        if (paren1 >= 0)
        {
            int paren2 = str.Slice(paren1 + 2).IndexOf(')');
            if (paren2 < 0)
            {
                value = default;
                return false;
            }

            paren2 += paren1 + 2;
            startIndex = paren1 + 1;
            endIndex = paren2 - 1;
        }
        else
        {
            startIndex = 0;
            endIndex = str.Length - 1;
        }

        int comma1 = str.Slice(startIndex).IndexOf(',');
        if (comma1 < 0)
        {
            value = default;
            return false;
        }

        comma1 += startIndex;
        if (comma1 + 2 > endIndex)
        {
            value = default;
            return false;
        }

        int comma2 = str.Slice(comma1 + 2).IndexOf(',');
        if (comma2 < 0)
        {
            value = default;
            return false;
        }

        comma2 += comma1 + 2;
        if (comma2 + 1 > endIndex)
        {
            value = default;
            return false;
        }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (!float.TryParse(str.Slice(startIndex, comma1 - startIndex), out float x))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma1 + 1, comma2 - comma1 - 1), out float y))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma2 + 1, endIndex - comma2), out float z))
        {
            value = default;
            return false;
        }
#else
        if (!float.TryParse(str.Slice(startIndex, comma1 - startIndex).ToString(), out float x))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma1 + 1, comma2 - comma1 - 1).ToString(), out float y))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma2 + 1, endIndex - comma2).ToString(), out float z))
        {
            value = default;
            return false;
        }
#endif

        value = new Vector3(x, y, z);
        return true;
    }

    public static bool TryParseVector4Components(ReadOnlySpan<char> str, out Vector4 value)
    {
        if (str.IsEmpty)
        {
            value = default;
            return false;
        }

        int paren1 = str.IndexOf('(');
        int startIndex;
        int endIndex;
        if (paren1 >= 0)
        {
            int paren2 = str.Slice(paren1 + 2).IndexOf(')');
            if (paren2 < 0)
            {
                value = default;
                return false;
            }

            paren2 += paren1 + 2;
            startIndex = paren1 + 1;
            endIndex = paren2 - 1;
        }
        else
        {
            startIndex = 0;
            endIndex = str.Length - 1;
        }

        int comma1 = str.Slice(startIndex).IndexOf(',');
        if (comma1 < 0)
        {
            value = default;
            return false;
        }

        comma1 += startIndex;
        if (comma1 + 2 > endIndex)
        {
            value = default;
            return false;
        }

        int comma2 = str.Slice(comma1 + 2).IndexOf(',');
        if (comma2 < 0)
        {
            value = default;
            return false;
        }

        comma2 += comma1 + 2;
        if (comma2 + 1 > endIndex)
        {
            value = default;
            return false;
        }

        int comma3 = str.Slice(comma2 + 2).IndexOf(',');
        if (comma3 < 0)
        {
            value = default;
            return false;
        }

        comma3 += comma2 + 2;
        if (comma3 + 1 > endIndex)
        {
            value = default;
            return false;
        }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (!float.TryParse(str.Slice(startIndex, comma1 - startIndex), out float x))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma1 + 1, comma2 - comma1 - 1), out float y))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma2 + 1, comma3 - comma2 - 1), out float z))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma3 + 1, endIndex - comma3), out float w))
        {
            value = default;
            return false;
        }
#else
        if (!float.TryParse(str.Slice(startIndex, comma1 - startIndex).ToString(), out float x))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma1 + 1, comma2 - comma1 - 1).ToString(), out float y))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma2 + 1, comma3 - comma2 - 1).ToString(), out float z))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma3 + 1, endIndex - comma3).ToString(), out float w))
        {
            value = default;
            return false;
        }
#endif

        value = new Vector4(x, y, z, w);
        return true;
    }

    internal static string ToComponentString(Vector2 v2)
    {
        return $"({v2.X.ToString(CultureInfo.InvariantCulture)}, {v2.Y.ToString(CultureInfo.InvariantCulture)})";
    }

    internal static string ToComponentString(Vector3 v3)
    {
        return $"({v3.X.ToString(CultureInfo.InvariantCulture)}, {v3.Y.ToString(CultureInfo.InvariantCulture)}, {v3.Z.ToString(CultureInfo.InvariantCulture)})";
    }

    internal static string ToComponentString(Vector4 v4)
    {
        return $"({v4.X.ToString(CultureInfo.InvariantCulture)}, {v4.Y.ToString(CultureInfo.InvariantCulture)}, {v4.Z.ToString(CultureInfo.InvariantCulture)}, {v4.W.ToString(CultureInfo.InvariantCulture)})";
    }

    public static bool TryParseVector2Components(ReadOnlySpan<char> str, out Vector2 value)
    {
        // stolen from nelson
        if (str.IsEmpty)
        {
            value = default;
            return false;
        }
        int paren1 = str.IndexOf('(');
        int startIndex;
        int num2;
        if (paren1 >= 0)
        {
            int paren2 = str.Slice(paren1 + 2).IndexOf(')');
            if (paren2 < 0)
            {
                value = default;
                return false;
            }

            paren2 += paren1 + 2;
            startIndex = paren1 + 1;
            num2 = paren2 - 1;
        }
        else
        {
            startIndex = 0;
            num2 = str.Length - 1;
        }

        int comma = str.Slice(startIndex).IndexOf(',');
        if (comma < 0)
        {
            value = default;
            return false;
        }

        comma += startIndex;
        if (comma + 1 > num2)
        {
            value = default;
            return false;
        }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (!float.TryParse(str.Slice(startIndex, comma - startIndex), out float x))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma + 1, num2 - comma), out float y))
        {
            value = default;
            return false;
        }
#else
        if (!float.TryParse(str.Slice(startIndex, comma - startIndex).ToString(), out float x))
        {
            value = default;
            return false;
        }

        if (!float.TryParse(str.Slice(comma + 1, num2 - comma).ToString(), out float y))
        {
            value = default;
            return false;
        }
#endif

        value = new Vector2(x, y);
        return true;
    }

    public static bool TryParseMasterBundleReference(string str, out string name, out string path)
    {
        int length = str.IndexOf(':');
        if (length < 0)
        {
            name = string.Empty;
            path = str;
        }
        else
        {
            name = str.Substring(0, length);
            path = str.Substring(length + 1);
        }

        return !string.IsNullOrEmpty(path);
    }
    
    public static bool TryParseTranslationReference(ReadOnlySpan<char> str, [NotNullWhen(true)] out string? @namespace, [NotNullWhen(true)] out string? token)
    {
        @namespace = null;
        token = null;

        if (str.IsEmpty)
            return false;

        // delimeters: '#', '::'
        int index1 = str.IndexOf('#');
        int index2 = str.IndexOf(':');

        if (index1 == 0)
        {
            str = str.Slice(1);
            index1 = str.IndexOf('#');
            if (index2 >= 0)
                --index2;
        }

        if (index1 < 0 && index2 < 0)
            return false;

        int index;
        if ((index1 < 0 || index2 < index1) && index2 > 0 && index2 < str.Length - 2 && str[index2 + 1] == ':')
        {
            index = index2;
        }
        else if (index2 > 0 && index2 < str.Length - 1)
        {
            index = index1;
        }
        else return false;


        @namespace = str.Slice(0, index).ToString();
        token = str.Slice(index + 2).ToString();
        return true;
    }

    public static bool TryParseMasterBundleReference(ReadOnlySpan<char> str, out string name, out string path)
    {
        int length = str.IndexOf(':');
        if (length < 0)
        {
            name = string.Empty;
            path = str.ToString();
        }
        else
        {
            name = str.Slice(0, length).ToString();
            path = str.Slice(length + 1).ToString();
        }

        return !string.IsNullOrEmpty(path);
    }

    public static bool TryParseColorHex(ReadOnlySpan<char> str, out Color32 value, bool allowAlpha)
    {
        if (str.IsEmpty)
        {
            value = Color32.Black;
            return false;
        }

        int startIndex = str[0] == '#' ? 1 : 0;
        bool alpha = false;
        if (str.Length != 6 + startIndex)
        {
            if (str.Length != 8 + startIndex || !allowAlpha)
            {
                value = Color32.Black;
                return false;
            }

            alpha = true;
        }

        if (!CharToHex(str, startIndex, out byte r)
            || !CharToHex(str, startIndex + 2, out byte g)
            || !CharToHex(str, startIndex + 4, out byte b))
        {
            value = Color32.Black;
            return false;
        }

        byte a = 255;
        if (alpha && !CharToHex(str, startIndex, out a))
        {
            value = Color32.Black;
            return false;
        }

        value = new Color32(a, r, g, b);
        return true;
    }

    private static bool CharToHex(ReadOnlySpan<char> c, int ind, out byte val)
    {
        int c2 = c[ind];
        byte b1;
        if (c2 is > 96 and < 103)
            b1 = (byte)((c2 - 87) * 0x10);
        else if (c2 is > 64 and < 71)
            b1 = (byte)((c2 - 55) * 0x10);
        else if (c2 is > 47 and < 58)
            b1 = (byte)((c2 - 48) * 0x10);
        else
        {
            val = 0;
            return false;
        }

        c2 = c[ind + 1];
        if (c2 is > 96 and < 103)
            val = (byte)(b1 + (c2 - 87));
        else if (c2 is > 64 and < 71)
            val = (byte)(b1 + (c2 - 55));
        else if (c2 is > 47 and < 58)
            val = (byte)(b1 + (c2 - 48));
        else
        {
            val = 0;
            return false;
        }

        return true;
    }

    public static bool TryParseGuidOrId(ReadOnlySpan<char> span, out GuidOrId guidOrId)
    {
        return TryParseGuidOrId(span, null, out guidOrId);
    }

    public static bool TryParseGuidOrId(string span, out GuidOrId guidOrId)
    {
        return TryParseGuidOrId(span.AsSpan(), span, out guidOrId);
    }

    public static bool TryParseGuidOrId(ReadOnlySpan<char> span, AssetCategoryValue assetCategory, out GuidOrId guidOrId)
    {
        return TryParseGuidOrId(span, null, out guidOrId, assetCategory);
    }

    public static bool TryParseGuidOrId(string span, AssetCategoryValue assetCategory, out GuidOrId guidOrId)
    {
        return TryParseGuidOrId(span.AsSpan(), span, out guidOrId, assetCategory);
    }

    // ReSharper disable once UnusedParameter.Local
    private static bool TryParseGuidOrId(ReadOnlySpan<char> span, string? stringValue, out GuidOrId guidOrId, AssetCategoryValue assetCategory = default)
    {
        if (span.Length == 1 && span[0] == '0')
        {
            guidOrId = GuidOrId.Empty;
            return true;
        }

        guidOrId = GuidOrId.Empty;

        if (span.IsEmpty)
            return false;

        int index = span.IndexOf(':');
        if (index > 0)
        {
            if (!AssetCategory.TryParse(span.Slice(0, index), out int category))
            {
                return false;
            }

            if (category == 0)
            {
                guidOrId = GuidOrId.Empty;
                return true;
            }

            if (index < span.Length - 1 && ushort.TryParse(span.Slice(index + 1).ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort id))
            {
                guidOrId = id == 0 ? GuidOrId.Empty : new GuidOrId(id, new AssetCategoryValue(category));
                return true;
            }
        }
        else
        {
            if (assetCategory.Index != 0
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                && ushort.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort id)
#else
                && ushort.TryParse(stringValue ?? span.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort id)
#endif
                )
            {
                guidOrId = id == 0 ? GuidOrId.Empty : new GuidOrId(id, assetCategory);
                return true;
            }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            if (Guid.TryParse(span, out Guid guid))
#else
            if (Guid.TryParse(stringValue ?? span.ToString(), out Guid guid))
#endif
            {
                guidOrId = new GuidOrId(guid);
                return true;
            }
        }

        return false;
    }

    public static bool TryParseItemString(ReadOnlySpan<char> input, string? stringInput, out GuidOrId assetRef, out int amount, GuidOrId assetContext)
    {
        // this
        // this x 2
        // 15
        // 15 x 3
        assetRef = GuidOrId.Empty;
        amount = 1;

        ReadOnlySpan<char> itemString = input;
        int index = input.IndexOf('x');
        if (index < 0)
            index = input.IndexOf('X');
        if (index >= 0)
        {
            itemString = input.Slice(0, index);
            if (index == input.Length - 1)
            {
                amount = 0;
                return false;
            }

            ReadOnlySpan<char> number = input.Slice(index + 1);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            if (!int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out amount))
#else
            if (!int.TryParse(number.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out amount))
#endif
            {
                amount = 0;
                return false;
            }

            amount = Math.Max(amount, 1);
        }

        if (TryParseGuidOrId(itemString, itemString.Length == input.Length ? stringInput : null, out assetRef, AssetCategoryValue.Item))
        {
            return index < 0 || input[index] == 'x';
        }

        if (!itemString.Trim().Equals("this".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        assetRef = assetContext;
        return index < 0 || input[index] == 'x';
    }

    public static bool TryParseStrictHex(ReadOnlySpan<char> value, out Color32 color, bool alpha)
    {
        if (!value.IsEmpty
            && value.Length == (alpha ? 9 : 7)
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            && uint.TryParse(value.Slice(1, value.Length - 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result)
#else
            && uint.TryParse(value.Slice(1, value.Length - 1).ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint result)
#endif
            )
        {
            color = new Color32(
                alpha ? (byte)((result >> 24) & byte.MaxValue) : byte.MaxValue,
                (byte)((result >> 16) & byte.MaxValue),
                (byte)((result >> 8) & byte.MaxValue),
                (byte)(result & byte.MaxValue)
            );
        }

        color = Color32.White;
        return false;
    }
}
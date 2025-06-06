using System;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

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

    private static readonly char[] InvalidTypeChars =
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
                        : systemType.FullName ?? systemType.Name
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

        value = new Vector3(x, y, z);
        return true;
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

    public static bool TryParseGuidOrId(ReadOnlySpan<char> span, string? stringValue, out GuidOrId guidOrId, string assetCategory = "NONE")
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
            if (!AssetCategory.TryParse(span.Slice(0, index), out EnumSpecTypeValue category))
            {
                return false;
            }

            if (category == AssetCategory.None)
            {
                guidOrId = GuidOrId.Empty;
                return true;
            }

            if (index < span.Length - 1 && ushort.TryParse(span.Slice(index + 1).ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort id))
            {
                guidOrId = id == 0 ? GuidOrId.Empty : new GuidOrId(id, category);
                return true;
            }
        }
        else
        {
            if (AssetCategory.TryParse(assetCategory, out EnumSpecTypeValue category)
                && category != AssetCategory.None
                && ushort.TryParse(stringValue ?? span.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out ushort id))
            {
                guidOrId = id == 0 ? GuidOrId.Empty : new GuidOrId(id, category);
                return true;
            }

            if (Guid.TryParse(stringValue ?? span.ToString(), out Guid guid))
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
        if (index >= 0)
        {
            itemString = input.Slice(0, index);
            if (index == input.Length - 1)
            {
                return false;
            }

            string number = input.Slice(index + 1).ToString();
            if (!int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out amount))
            {
                return false;
            }

            amount = Math.Max(amount, 1);
        }
        else
        {
            index = input.IndexOf('X');
            if (index >= 0)
            {
                return false;
            }
        }

        if (TryParseGuidOrId(itemString, itemString.Length == input.Length ? stringInput : null, out assetRef, "ITEM"))
        {
            return true;
        }

        if (!itemString.Trim().Equals("this".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        assetRef = assetContext;
        return true;
    }
}
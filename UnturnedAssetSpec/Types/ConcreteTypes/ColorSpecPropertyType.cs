using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class ColorRGBSpecPropertyType : ColorSpecPropertyType
{
    public static readonly ColorRGBSpecPropertyType Instance = new ColorRGBSpecPropertyType();

    static ColorRGBSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ColorRGB";

    /// <inheritdoc />
    public override string DisplayName => "Color (RBG; 0-1)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;

    /// <inheritdoc />
    private protected override bool HasAlpha => false;
}

public sealed class ColorRGBASpecPropertyType : ColorSpecPropertyType
{
    public static readonly ColorRGBASpecPropertyType Instance = new ColorRGBASpecPropertyType();

    static ColorRGBASpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ColorRGBA";

    /// <inheritdoc />
    public override string DisplayName => "Color (RBGA; 0-1)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;

    /// <inheritdoc />
    private protected override bool HasAlpha => true;
}

public sealed class ColorRGBLegacySpecPropertyType : ColorSpecPropertyType
{
    public static readonly ColorRGBLegacySpecPropertyType Instance = new ColorRGBLegacySpecPropertyType();

    static ColorRGBLegacySpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ColorRGBLegacy";

    /// <inheritdoc />
    public override string DisplayName => "Legacy Color (RBG; 0-1)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;

    /// <inheritdoc />
    private protected override bool HasAlpha => false;
}

public sealed class ColorRGBALegacySpecPropertyType : ColorSpecPropertyType
{
    public static readonly ColorRGBALegacySpecPropertyType Instance = new ColorRGBALegacySpecPropertyType();

    static ColorRGBALegacySpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ColorRGBALegacy";

    /// <inheritdoc />
    public override string DisplayName => "Legacy Color (RBGA; 0-1)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;

    /// <inheritdoc />
    private protected override bool HasAlpha => true;
}

public sealed class ColorRGBStrictHexSpecPropertyType : ColorStrictHexSpecPropertyType
{
    public static readonly ColorRGBStrictHexSpecPropertyType Instance = new ColorRGBStrictHexSpecPropertyType();

    static ColorRGBStrictHexSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ColorRGBStrictHex";

    /// <inheritdoc />
    public override string DisplayName => "Color (Hex; '#rrggbb')";

    /// <inheritdoc />
    private protected override bool HasAlpha => true;
}

public sealed class ColorRGBAStrictHexSpecPropertyType : ColorStrictHexSpecPropertyType
{
    public static readonly ColorRGBAStrictHexSpecPropertyType Instance = new ColorRGBAStrictHexSpecPropertyType();

    static ColorRGBAStrictHexSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ColorRGBAStrictHex";

    /// <inheritdoc />
    public override string DisplayName => "Color (Hex; '#rrggbbaa')";

    /// <inheritdoc />
    private protected override bool HasAlpha => true;
}

public abstract class ColorSpecPropertyType :
    BaseColorSpecPropertyType<ColorSpecPropertyType, Color>,
    IStringParseableSpecPropertyType
{
    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<Color>()?.ToHex(HasAlpha);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (KnownTypeValueHelper.TryParseColorHex(span, out Color32 value, HasAlpha))
        {
            dynamicValue = new SpecDynamicConcreteValue<Color>(value, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out Color value)
    {
        VectorTypeParseOptions options = Options;
        if (parse.Node == null)
        {
            if ((options & VectorTypeParseOptions.Legacy) == 0)
            {
                MissingNode(in parse, out value);
                value = Color.Black;
                return false;
            }

            IDictionarySourceNode? dict;
            if (parse.Parent is IPropertySourceNode { Parent: IDictionarySourceNode dict2 })
            {
                dict = dict2;
            }
            else if (parse.Parent is IDictionarySourceNode dict3)
            {
                dict = dict3;
            }
            else
            {
                MissingNode(in parse, out value);
                value = Color.Black;
                return false;
            }

            if (parse.BaseKey == null)
            {
                MissingNode(in parse, out value);
                value = Color.Black;
                return false;
            }

            string rKey = parse.BaseKey + "_R";
            string gKey = parse.BaseKey + "_G";
            string bKey = parse.BaseKey + "_B";

            return TryParseFromDictionary(in parse, dict, rKey, gKey, bKey, HasAlpha ? parse.BaseKey + "_A" : null, false, out value);
        }

        if ((options & VectorTypeParseOptions.Object) != 0 && parse.Node is IDictionarySourceNode v3Struct)
        {
            return TryParseFromDictionary(in parse, v3Struct, "R", "G", "B", HasAlpha ? "A" : null, true, out value);
        }

        if ((options & VectorTypeParseOptions.Composite) != 0 && parse.Node is IValueSourceNode strValNode)
        {
            if (KnownTypeValueHelper.TryParseColorHex(strValNode.Value.AsSpan(), out Color32 c32, HasAlpha))
            {
                value = c32;
                return true;
            }

            value = Color.Black;
            return false;
        }

        FailedToParse(in parse, out value);
        value = Color.Black;
        return false;
    }

    protected bool TryParseFromDictionary(in SpecPropertyTypeParseContext parse, IDictionarySourceNode dict,
        string rKey, string gKey, string bKey, string? aKey,
        bool shouldParseAsColor32,
        out Color value)
    {
        bool rBad = false, gBad = false, bBad = false, aBad = false;

        string? rVal = null, gVal = null, bVal = null, aVal = null;

        if (!dict.TryGetPropertyValue(rKey, out IValueSourceNode? rString))
        {
            rBad = true;
        }
        else
        {
            rVal = rString.Value;
        }

        if (!dict.TryGetPropertyValue(gKey, out IValueSourceNode? gString))
        {
            gBad = true;
        }
        else
        {
            gVal = gString.Value;
        }

        if (!dict.TryGetPropertyValue(bKey, out IValueSourceNode? bString))
        {
            bBad = true;
        }
        else
        {
            bVal = bString.Value;
        }

        if (!dict.TryGetPropertyValue(aKey ?? string.Empty, out IValueSourceNode? aString))
        {
            aBad = aKey != null;
        }
        else
        {
            aVal = aString.Value;
        }

        if (parse.HasDiagnostics)
        {
            if (rBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, rKey),
                    Range = rString?.Range ?? gString?.Range ?? bString?.Range?? aString?.Range ?? dict.Range
                });
            }

            if (gBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, gKey),
                    Range = gString?.Range ?? rString?.Range ?? bString?.Range ?? aString?.Range ?? dict.Range
                });
            }

            if (bBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, bKey),
                    Range = bString?.Range ?? rString?.Range ?? gString?.Range ?? aString?.Range ?? dict.Range
                });
            }

            if (aBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, aKey),
                    Range = aString?.Range ?? rString?.Range ?? gString?.Range ?? bString?.Range ?? dict.Range
                });
            }
        }

        if (rBad | gBad | bBad | aBad)
        {
            value = Color.Black;
            return false;
        }

        bool good = true;
        float r, g, b, a = 1f;
        if (shouldParseAsColor32)
        {
            if (!KnownTypeValueHelper.TryParseUInt8(rVal!, out byte r8))
            {
                good &= FailedToParse(in parse, out value, rString);
                r = 0;
            }
            else
            {
                r = r8 / 255f;
            }
            if (!KnownTypeValueHelper.TryParseUInt8(gVal!, out byte g8))
            {
                good &= FailedToParse(in parse, out value, gString);
                g = 0;
            }
            else
            {
                g = g8 / 255f;
            }
            if (!KnownTypeValueHelper.TryParseUInt8(bVal!, out byte b8))
            {
                good &= FailedToParse(in parse, out value, bString);
                b = 0;
            }
            else
            {
                b = b8 / 255f;
            }
            if (aKey != null)
            {
                if (!KnownTypeValueHelper.TryParseUInt8(aVal!, out byte a8))
                {
                    good &= FailedToParse(in parse, out value, aString);
                }
                else
                {
                    a = a8 / 255f;
                }
            }
        }
        else
        {
            if (!KnownTypeValueHelper.TryParseFloat(rVal!, out r))
            {
                good &= FailedToParse(in parse, out value, rString);
            }
            if (!KnownTypeValueHelper.TryParseFloat(gVal!, out g))
            {
                good &= FailedToParse(in parse, out value, gString);
            }
            if (!KnownTypeValueHelper.TryParseFloat(bVal!, out b))
            {
                good &= FailedToParse(in parse, out value, bString);
            }
            if (aKey != null && !KnownTypeValueHelper.TryParseFloat(aVal!, out a))
            {
                good &= FailedToParse(in parse, out value, aString);
            }
        }

        if (!good)
        {
            value = Color.Black;
            return false;
        }

        if (parse.HasDiagnostics)
        {
            if (a is < 0 or > 1)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = aString!.Range,
                    Diagnostic = DatDiagnostics.UNT1012,
                    Message = DiagnosticResources.UNT1012
                });
            }

            if (r is < 0 or > 1)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = rString!.Range,
                    Diagnostic = DatDiagnostics.UNT1012,
                    Message = DiagnosticResources.UNT1012
                });
            }

            if (g is < 0 or > 1)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = gString!.Range,
                    Diagnostic = DatDiagnostics.UNT1012,
                    Message = DiagnosticResources.UNT1012
                });
            }

            if (b is < 0 or > 1)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = bString!.Range,
                    Diagnostic = DatDiagnostics.UNT1012,
                    Message = DiagnosticResources.UNT1012
                });
            }
        }

        value = new Color(a, r, g, b);
        return true;
    }
}

public abstract class ColorStrictHexSpecPropertyType :
    BaseColorSpecPropertyType<ColorStrictHexSpecPropertyType, Color>,
    IStringParseableSpecPropertyType
{
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite;
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<Color>()?.ToHex(HasAlpha);
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (KnownTypeValueHelper.TryParseColorHex(span, out Color32 value, HasAlpha))
        {
            dynamicValue = new SpecDynamicConcreteValue<Color>(value, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out Color value)
    {
        if (parse.Node == null)
        {
            MissingNode(in parse, out value);
            value = Color.Black;
            return false;
        }

        if (parse.Node is IValueSourceNode strValNode)
        {
            if (KnownTypeValueHelper.TryParseStrictHex(strValNode.Value.AsSpan(), out Color32 c32, HasAlpha))
            {
                value = c32;
                return true;
            }

            value = Color.Black;
            return false;
        }

        FailedToParse(in parse, out value);
        value = Color.Black;
        return false;
    }
}

public abstract class BaseColorSpecPropertyType<TSelf, T>
    : BasicSpecPropertyType<TSelf, T>,
        IVectorSpecPropertyType<Color>,
        IVectorSpecPropertyType<Color32>,
        ILegacyCompositeTypeProvider
    where T : unmanaged, IEquatable<T> where TSelf : BaseColorSpecPropertyType<TSelf, T>
{
    private protected abstract VectorTypeParseOptions Options { get; }
    private protected abstract bool HasAlpha { get; }

    void IVectorSpecPropertyType.Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.Visit<Color>(this);
        visitor.Visit<Color32>(this);
    }

    public Color Multiply(Color val1, Color val2)
    {
        return new Color(val1.A * val2.A, val1.R * val2.R, val1.G * val2.G, val1.B * val2.B);
    }

    public Color Divide(Color val1, Color val2)
    {
        return new Color(val1.A / val2.A, val1.R / val2.R, val1.G / val2.G, val1.B / val2.B);
    }

    public Color Add(Color val1, Color val2)
    {
        return new Color(val1.A + val2.A, val1.R + val2.R, val1.G + val2.G, val1.B + val2.B);
    }

    public Color Subtract(Color val1, Color val2)
    {
        return new Color(val1.A - val2.A, val1.R - val2.R, val1.G - val2.G, val1.B - val2.B);
    }

    public Color Modulo(Color val1, Color val2)
    {
        return new Color(val1.A % val2.A, val1.R % val2.R, val1.G % val2.G, val1.B % val2.B);
    }

    public Color Power(Color val1, Color val2)
    {
        return new Color(MathF.Pow(val1.A, val2.A), MathF.Pow(val1.R, val2.R), MathF.Pow(val1.G, val2.G), MathF.Pow(val1.B, val2.B));
    }

    public Color Min(Color val1, Color val2)
    {
        return new Color(Math.Min(val1.A, val2.A), Math.Min(val1.R, val2.R), Math.Min(val1.G, val2.G), Math.Min(val1.B, val2.B));
    }

    public Color Max(Color val1, Color val2)
    {
        return new Color(Math.Max(val1.A, val2.A), Math.Max(val1.R, val2.R), Math.Max(val1.G, val2.G), Math.Max(val1.B, val2.B));
    }

    public Color Avg(Color val1, Color val2)
    {
        return new Color((val1.A + val2.A) / 2f, (val1.R + val2.R) / 2f, (val1.G + val2.G) / 2f, (val1.B + val2.B) / 2f);
    }

    public Color Absolute(Color val)
    {
        return new Color(Math.Abs(val.A), Math.Abs(val.R), Math.Abs(val.G), Math.Abs(val.B));
    }

    public Color Round(Color val)
    {
        return new Color(MathF.Round(val.A), MathF.Round(val.R), MathF.Round(val.G), MathF.Round(val.B));
    }

    public Color Ceiling(Color val)
    {
        return new Color(MathF.Ceiling(val.A), MathF.Ceiling(val.R), MathF.Ceiling(val.G), MathF.Ceiling(val.B));
    }

    public Color Floor(Color val)
    {
        return new Color(MathF.Floor(val.A), MathF.Floor(val.R), MathF.Floor(val.G), MathF.Floor(val.B));
    }

    public Color TrigOperation(Color val, int op, bool deg)
    {
        return deg
            ? op switch
            {
                0 => new Color(MathF.Sin(val.A * (MathF.PI / 180f)), MathF.Sin(val.R * (MathF.PI / 180f)), MathF.Sin(val.G * (MathF.PI / 180f)), MathF.Sin(val.B * (MathF.PI / 180f))),
                1 => new Color(MathF.Cos(val.A * (MathF.PI / 180f)), MathF.Cos(val.R * (MathF.PI / 180f)), MathF.Cos(val.G * (MathF.PI / 180f)), MathF.Cos(val.B * (MathF.PI / 180f))),
                2 => new Color(MathF.Tan(val.A * (MathF.PI / 180f)), MathF.Tan(val.R * (MathF.PI / 180f)), MathF.Tan(val.G * (MathF.PI / 180f)), MathF.Tan(val.B * (MathF.PI / 180f))),
                3 => new Color(MathF.Asin(val.A) * (180f / MathF.PI), MathF.Asin(val.R) * (180f / MathF.PI), MathF.Asin(val.G) * (180f / MathF.PI), MathF.Asin(val.B) * (180f / MathF.PI)),
                4 => new Color(MathF.Acos(val.A) * (180f / MathF.PI), MathF.Acos(val.R) * (180f / MathF.PI), MathF.Acos(val.G) * (180f / MathF.PI), MathF.Acos(val.B) * (180f / MathF.PI)),
                5 => new Color(MathF.Atan(val.A) * (180f / MathF.PI), MathF.Atan(val.R) * (180f / MathF.PI), MathF.Atan(val.G) * (180f / MathF.PI), MathF.Atan(val.B) * (180f / MathF.PI)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            }
            : op switch
            {
                0 => new Color(MathF.Sin(val.A), MathF.Sin(val.R), MathF.Sin(val.G), MathF.Sin(val.B)),
                1 => new Color(MathF.Cos(val.A), MathF.Cos(val.R), MathF.Cos(val.G), MathF.Cos(val.B)),
                2 => new Color(MathF.Tan(val.A), MathF.Tan(val.R), MathF.Tan(val.G), MathF.Tan(val.B)),
                3 => new Color(MathF.Asin(val.A), MathF.Asin(val.R), MathF.Asin(val.G), MathF.Asin(val.B)),
                4 => new Color(MathF.Acos(val.A), MathF.Acos(val.R), MathF.Acos(val.G), MathF.Acos(val.B)),
                5 => new Color(MathF.Atan(val.A), MathF.Atan(val.R), MathF.Atan(val.G), MathF.Atan(val.B)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };
    }

    public Color Sqrt(Color val)
    {
        return new Color(MathF.Sqrt(val.A), MathF.Sqrt(val.R), MathF.Sqrt(val.G), MathF.Sqrt(val.B));
    }

    Color IVectorSpecPropertyType<Color>.Construct(double scalar) => new Color((float)scalar, (float)scalar, (float)scalar, (float)scalar);

    private static byte Clamp(double db)
    {
        return db < 0 ? (byte)0 : db > 255 ? (byte)255 : (byte)Math.Round(db);
    }
    private static byte Clamp(float fl)
    {
        return fl < 0 ? (byte)0 : fl > 255 ? (byte)255 : (byte)Math.Round(fl);
    }
    private static byte Clamp(int integer)
    {
        return integer < 0 ? (byte)0 : integer > 255 ? (byte)255 : (byte)integer;
    }

    public Color32 Multiply(Color32 val1, Color32 val2)
    {
        return new Color32(Clamp(val1.A * val2.A), Clamp(val1.R * val2.R), Clamp(val1.G * val2.G), Clamp(val1.B * val2.B));
    }

    public Color32 Divide(Color32 val1, Color32 val2)
    {
        return new Color32(Clamp(val1.A / val2.A), Clamp(val1.R / val2.R), Clamp(val1.G / val2.G), Clamp(val1.B / val2.B));
    }

    public Color32 Add(Color32 val1, Color32 val2)
    {
        return new Color32(Clamp(val1.A + val2.A), Clamp(val1.R + val2.R), Clamp(val1.G + val2.G), Clamp(val1.B + val2.B));
    }

    public Color32 Subtract(Color32 val1, Color32 val2)
    {
        return new Color32(Clamp(val1.A - val2.A), Clamp(val1.R - val2.R), Clamp(val1.G - val2.G), Clamp(val1.B - val2.B));
    }

    public Color32 Modulo(Color32 val1, Color32 val2)
    {
        return new Color32(Clamp(val1.A % val2.A), Clamp(val1.R % val2.R), Clamp(val1.G % val2.G), Clamp(val1.B % val2.B));
    }

    public Color32 Power(Color32 val1, Color32 val2)
    {
        return new Color32(Clamp(Math.Pow(val1.A, val2.A)), Clamp(Math.Pow(val1.R, val2.R)), Clamp(Math.Pow(val1.G, val2.G)), Clamp(Math.Pow(val1.B, val2.B)));
    }

    public Color32 Min(Color32 val1, Color32 val2)
    {
        return new Color32(Math.Min(val1.A, val2.A), Math.Min(val1.R, val2.R), Math.Min(val1.G, val2.G), Math.Min(val1.B, val2.B));
    }

    public Color32 Max(Color32 val1, Color32 val2)
    {
        return new Color32(Math.Max(val1.A, val2.A), Math.Max(val1.R, val2.R), Math.Max(val1.G, val2.G), Math.Max(val1.B, val2.B));
    }

    public Color32 Avg(Color32 val1, Color32 val2)
    {
        return new Color32((byte)Math.Round((val1.A + val2.A) / 2f), (byte)Math.Round((val1.R + val2.R) / 2f), (byte)Math.Round((val1.G + val2.G) / 2f), (byte)Math.Round((val1.B + val2.B) / 2f));
    }

    public Color32 Absolute(Color32 val)
    {
        return val;
    }

    public Color32 Round(Color32 val)
    {
        return val;
    }

    public Color32 Ceiling(Color32 val)
    {
        return val;
    }

    public Color32 Floor(Color32 val)
    {
        return val;
    }

    public Color32 TrigOperation(Color32 val, int op, bool deg)
    {
        return deg
            ? op switch
            {
                0 => new Color32(Clamp(MathF.Sin(val.A * (MathF.PI / 180f))), Clamp(MathF.Sin(val.R * (MathF.PI / 180f))), Clamp(MathF.Sin(val.G * (MathF.PI / 180f))), Clamp(MathF.Sin(val.B * (MathF.PI / 180f)))),
                1 => new Color32(Clamp(MathF.Cos(val.A * (MathF.PI / 180f))), Clamp(MathF.Cos(val.R * (MathF.PI / 180f))), Clamp(MathF.Cos(val.G * (MathF.PI / 180f))), Clamp(MathF.Cos(val.B * (MathF.PI / 180f)))),
                2 => new Color32(Clamp(MathF.Tan(val.A * (MathF.PI / 180f))), Clamp(MathF.Tan(val.R * (MathF.PI / 180f))), Clamp(MathF.Tan(val.G * (MathF.PI / 180f))), Clamp(MathF.Tan(val.B * (MathF.PI / 180f)))),
                3 => new Color32(Clamp(MathF.Asin(val.A) * (180f / MathF.PI)), Clamp(MathF.Asin(val.R) * (180f / MathF.PI)), Clamp(MathF.Asin(val.G) * (180f / MathF.PI)), Clamp(MathF.Asin(val.B) * (180f / MathF.PI))),
                4 => new Color32(Clamp(MathF.Acos(val.A) * (180f / MathF.PI)), Clamp(MathF.Acos(val.R) * (180f / MathF.PI)), Clamp(MathF.Acos(val.G) * (180f / MathF.PI)), Clamp(MathF.Acos(val.B) * (180f / MathF.PI))),
                5 => new Color32(Clamp(MathF.Atan(val.A) * (180f / MathF.PI)), Clamp(MathF.Atan(val.R) * (180f / MathF.PI)), Clamp(MathF.Atan(val.G) * (180f / MathF.PI)), Clamp(MathF.Atan(val.B) * (180f / MathF.PI))),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            }
            : op switch
            {
                0 => new Color32(Clamp(MathF.Sin(val.A)), Clamp(MathF.Sin(val.R)), Clamp(MathF.Sin(val.G)), Clamp(MathF.Sin(val.B))),
                1 => new Color32(Clamp(MathF.Cos(val.A)), Clamp(MathF.Cos(val.R)), Clamp(MathF.Cos(val.G)), Clamp(MathF.Cos(val.B))),
                2 => new Color32(Clamp(MathF.Tan(val.A)), Clamp(MathF.Tan(val.R)), Clamp(MathF.Tan(val.G)), Clamp(MathF.Tan(val.B))),
                3 => new Color32(Clamp(MathF.Asin(val.A)), Clamp(MathF.Asin(val.R)), Clamp(MathF.Asin(val.G)), Clamp(MathF.Asin(val.B))),
                4 => new Color32(Clamp(MathF.Acos(val.A)), Clamp(MathF.Acos(val.R)), Clamp(MathF.Acos(val.G)), Clamp(MathF.Acos(val.B))),
                5 => new Color32(Clamp(MathF.Atan(val.A)), Clamp(MathF.Atan(val.R)), Clamp(MathF.Atan(val.G)), Clamp(MathF.Atan(val.B))),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };
    }

    public Color32 Sqrt(Color32 val)
    {
        return new Color32(Clamp(MathF.Sqrt(val.A)), Clamp(MathF.Sqrt(val.R)), Clamp(MathF.Sqrt(val.G)), Clamp(MathF.Sqrt(val.B)));
    }

    Color32 IVectorSpecPropertyType<Color32>.Construct(double scalar)
    {
        byte v = Clamp(scalar);
        return new Color32(v, v, v, v);
    }

    bool ILegacyCompositeTypeProvider.IsEnabled => (Options & VectorTypeParseOptions.Legacy) != 0;

    void ILegacyCompositeTypeProvider.VisitLinkedProperties<TVisitor>(
        in FileEvaluationContext context, SpecProperty property, IDictionarySourceNode propertyRoot, ref TVisitor propertyVisitor, OneOrMore<int> templateGroupIndices)
    {
        if (property.IsTemplate)
        {
            // Color_(\d+)

        }
        if (propertyRoot.TryGetProperty(property.Key + "_R", out IPropertySourceNode? node))
        {
            propertyVisitor.AcceptProperty(node);
        }
        if (propertyRoot.TryGetProperty(property.Key + "_G", out node))
        {
            propertyVisitor.AcceptProperty(node);
        }
        if (propertyRoot.TryGetProperty(property.Key + "_B", out node))
        {
            propertyVisitor.AcceptProperty(node);
        }
        if (HasAlpha && propertyRoot.TryGetProperty(property.Key + "_A", out node))
        {
            propertyVisitor.AcceptProperty(node);
        }
    }
}
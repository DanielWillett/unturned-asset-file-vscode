using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

internal class ColorVectorProvider : IVectorTypeProvider<Color>
{
    public ITypeConverter<Color> Converter => ColorType.Instance;
    public int Size => 4;

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

    public Color Atan2(Color x, double y, bool deg)
    {
        return deg
            ? new Color((float)(Math.Atan2(x.A, y) * (180 / Math.PI)), (float)(Math.Atan2(x.R, y) * (180 / Math.PI)), (float)(Math.Atan2(x.G, y) * (180 / Math.PI)), (float)(Math.Atan2(x.B, y) * (180 / Math.PI)))
            : new Color((float)Math.Atan2(x.A, y), (float)Math.Atan2(x.R, y), (float)Math.Atan2(x.G, y), (float)Math.Atan2(x.B, y));
    }

    public Color Atan2(double x, Color y, bool deg)
    {
        return deg
            ? new Color((float)(Math.Atan2(x, y.A) * (180 / Math.PI)), (float)(Math.Atan2(x, y.R) * (180 / Math.PI)), (float)(Math.Atan2(x, y.G) * (180 / Math.PI)), (float)(Math.Atan2(x, y.B) * (180 / Math.PI)))
            : new Color((float)Math.Atan2(x, y.A), (float)Math.Atan2(x, y.R), (float)Math.Atan2(x, y.G), (float)Math.Atan2(x, y.B));
    }

    public Color Atan2(Color x, Color y, bool deg)
    {
        return deg
            ? new Color(MathF.Atan2(x.A, y.A) * (180f / MathF.PI), MathF.Atan2(x.R, y.R) * (180f / MathF.PI), MathF.Atan2(x.G, y.G) * (180f / MathF.PI), MathF.Atan2(x.B, y.B) * (180f / MathF.PI))
            : new Color(MathF.Atan2(x.A, y.A), MathF.Atan2(x.R, y.R), MathF.Atan2(x.G, y.G), MathF.Atan2(x.B, y.B));
    }

    public Color Sqrt(Color val)
    {
        return new Color(MathF.Sqrt(val.A), MathF.Sqrt(val.R), MathF.Sqrt(val.G), MathF.Sqrt(val.B));
    }

    public Color Construct(double scalar)
    {
        float f = (float)scalar;
        return new Color(f, f, f, f);
    }

    public Color Construct(ReadOnlySpan<double> components)
    {
        return new Color(
            components.Length < 1 ? 0f : (float)components[0],
            components.Length < 2 ? 0f : (float)components[1],
            components.Length < 2 ? 0f : (float)components[2],
            components.Length < 3 ? 0f : (float)components[3]
        );
    }

    public int Deconstruct(Color val, Span<double> components)
    {
        if (components.Length <= 0)
            return 0;
        components[0] = val.R;
        if (components.Length <= 1)
            return 1;
        components[1] = val.G;
        if (components.Length <= 2)
            return 2;
        components[2] = val.B;
        if (components.Length <= 3)
            return 3;
        components[3] = val.A;
        return 4;
    }

    public int Compare(Color left, Color right)
    {
        const float tolerance = 0.0038f;

        float sub = left.R - right.R;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = left.G - right.G;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = left.B - right.B;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = left.A - right.A;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }

        return 0;
    }

    public double GetComponent(Color val, int index)
    {
        return index switch
        {
            0 => val.R,
            1 => val.G,
            2 => val.B,
            3 => val.A,
            _ => 0
        };
    }

    public string ToString(Color val)
    {
        return val.ToString();
    }

    public bool TryParse([NotNullWhen(true)] string? str, out Color vector)
    {
        if (!KnownTypeValueHelper.TryParseColorHex(str, out Color32 c32, allowAlpha: true))
        {
            vector = Color.Black;
            return false;
        }

        vector = c32;
        return true;
    }
}
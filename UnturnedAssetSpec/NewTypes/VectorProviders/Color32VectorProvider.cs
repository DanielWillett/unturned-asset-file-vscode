using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

internal class Color32VectorProvider : IVectorTypeProvider<Color32>
{
    public int Size => 4;

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

    public Color32 Atan2(Color32 x, double y, bool deg)
    {
        return deg
            ? new Color32(Clamp(Math.Atan2(x.A, y) * (180 / Math.PI)), Clamp(Math.Atan2(x.R, y) * (180 / Math.PI)), Clamp(Math.Atan2(x.G, y) * (180 / Math.PI)), Clamp(Math.Atan2(x.B, y) * (180 / Math.PI)))
            : new Color32(Clamp(Math.Atan2(x.A, y)), Clamp(Math.Atan2(x.R, y)), Clamp(Math.Atan2(x.G, y)), Clamp(Math.Atan2(x.B, y)));
    }

    public Color32 Atan2(double x, Color32 y, bool deg)
    {
        return deg
            ? new Color32(Clamp(Math.Atan2(x, y.A) * (180 / Math.PI)), Clamp(Math.Atan2(x, y.R) * (180 / Math.PI)), Clamp(Math.Atan2(x, y.G) * (180 / Math.PI)), Clamp(Math.Atan2(x, y.B) * (180 / Math.PI)))
            : new Color32(Clamp(Math.Atan2(x, y.A)), Clamp(Math.Atan2(x, y.R)), Clamp(Math.Atan2(x, y.G)), Clamp(Math.Atan2(x, y.B)));
    }

    public Color32 Atan2(Color32 x, Color32 y, bool deg)
    {
        return deg
            ? new Color32(Clamp(Math.Atan2(x.A, y.A) * (180 / Math.PI)), Clamp(Math.Atan2(x.R, y.R) * (180 / Math.PI)), Clamp(Math.Atan2(x.G, y.G) * (180 / Math.PI)), Clamp(Math.Atan2(x.B, y.B) * (180 / Math.PI)))
            : new Color32(Clamp(Math.Atan2(x.A, y.A)), Clamp(Math.Atan2(x.R, y.R)), Clamp(Math.Atan2(x.G, y.G)), Clamp(Math.Atan2(x.B, y.B)));
    }

    public Color32 Sqrt(Color32 val)
    {
        return new Color32(Clamp(MathF.Sqrt(val.A)), Clamp(MathF.Sqrt(val.R)), Clamp(MathF.Sqrt(val.G)), Clamp(MathF.Sqrt(val.B)));
    }

    public Color32 Construct(double scalar)
    {
        byte v = Clamp(scalar);
        return new Color32(v, v, v, v);
    }

    public Color32 Construct(ReadOnlySpan<double> components)
    {
        return new Color32(
            components.Length < 1 ? (byte)0 : Clamp(components[0]),
            components.Length < 2 ? (byte)0 : Clamp(components[1]),
            components.Length < 2 ? (byte)0 : Clamp(components[2]),
            components.Length < 3 ? (byte)0 : Clamp(components[3])
        );
    }

    public int Deconstruct(Color32 val, Span<double> components)
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

    public double GetComponent(Color32 val, int index)
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

    public string ToString(Color32 val)
    {
        return val.ToString();
    }

    public bool TryParse([NotNullWhen(true)] string? str, out Color32 vector)
    {
        return KnownTypeValueHelper.TryParseColorHex(str, out vector, allowAlpha: true);
    }
}
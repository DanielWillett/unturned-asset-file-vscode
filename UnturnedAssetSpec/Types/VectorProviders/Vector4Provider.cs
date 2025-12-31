using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

#if NET5_0_OR_GREATER
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
internal class Vector4Provider : IVectorTypeProvider<Vector4>
{
    public int Size => 4;

    public Vector4 Multiply(Vector4 val1, Vector4 val2)
    {
        return new Vector4(val1.X * val2.X, val1.Y * val2.Y, val1.Z * val2.Z, val1.W * val2.W);
    }

    public Vector4 Divide(Vector4 val1, Vector4 val2)
    {
        return new Vector4(val1.X / val2.X, val1.Y / val2.Y, val1.Z / val2.Z, val1.W / val2.W);
    }

    public Vector4 Add(Vector4 val1, Vector4 val2)
    {
        return new Vector4(val1.X + val2.X, val1.Y + val2.Y, val1.Z + val2.Z, val1.W + val2.W);
    }

    public Vector4 Subtract(Vector4 val1, Vector4 val2)
    {
        return new Vector4(val1.X - val2.X, val1.Y - val2.Y, val1.Z - val2.Z, val1.W - val2.W);
    }

    public Vector4 Modulo(Vector4 val1, Vector4 val2)
    {
        return new Vector4(val1.X % val2.X, val1.Y % val2.Y, val1.Z % val2.Z, val1.W % val2.W);
    }

    public Vector4 Power(Vector4 val1, Vector4 val2)
    {
        return new Vector4((float)Math.Pow(val1.X, val2.X), (float)Math.Pow(val1.Y, val2.Y), (float)Math.Pow(val1.Z, val2.Z), (float)Math.Pow(val1.W, val2.W));
    }

    public Vector4 Min(Vector4 val1, Vector4 val2)
    {
        return new Vector4(Math.Min(val1.X, val2.X), Math.Min(val1.Y, val2.Y), Math.Min(val1.Z, val2.Z), Math.Min(val1.W, val2.W));
    }

    public Vector4 Max(Vector4 val1, Vector4 val2)
    {
        return new Vector4(Math.Max(val1.X, val2.X), Math.Max(val1.Y, val2.Y), Math.Max(val1.Z, val2.Z), Math.Max(val1.W, val2.W));
    }

    public Vector4 Avg(Vector4 val1, Vector4 val2)
    {
        return new Vector4((val1.X + val2.X) / 2f, (val1.Y + val2.Y) / 2f, (val1.Z + val2.Z) / 2f, (val1.W + val2.W) / 2f);
    }

    public Vector4 Absolute(Vector4 val)
    {
        return new Vector4(Math.Abs(val.X), Math.Abs(val.Y), Math.Abs(val.Z), Math.Abs(val.W));
    }

    public Vector4 Round(Vector4 val)
    {
        return new Vector4(MathF.Round(val.X), MathF.Round(val.Y), MathF.Round(val.Z), MathF.Round(val.W));
    }

    public Vector4 Ceiling(Vector4 val)
    {
        return new Vector4(MathF.Ceiling(val.X), MathF.Ceiling(val.Y), MathF.Ceiling(val.Z), MathF.Ceiling(val.W));
    }

    public Vector4 Floor(Vector4 val)
    {
        return new Vector4(MathF.Floor(val.X), MathF.Floor(val.Y), MathF.Floor(val.Z), MathF.Floor(val.W));
    }

    public Vector4 TrigOperation(Vector4 val, int op, bool deg)
    {
        return deg
            ? op switch
            {
                0 => new Vector4(MathF.Sin(val.X * (MathF.PI / 180f)), MathF.Sin(val.Y * (MathF.PI / 180f)), MathF.Sin(val.Z * (MathF.PI / 180f)), MathF.Sin(val.W * (MathF.PI / 180f))),
                1 => new Vector4(MathF.Cos(val.X * (MathF.PI / 180f)), MathF.Cos(val.Y * (MathF.PI / 180f)), MathF.Cos(val.Z * (MathF.PI / 180f)), MathF.Cos(val.W * (MathF.PI / 180f))),
                2 => new Vector4(MathF.Tan(val.X * (MathF.PI / 180f)), MathF.Tan(val.Y * (MathF.PI / 180f)), MathF.Tan(val.Z * (MathF.PI / 180f)), MathF.Tan(val.W * (MathF.PI / 180f))),
                3 => new Vector4(MathF.Asin(val.X) * (180f / MathF.PI), MathF.Asin(val.Y) * (180f / MathF.PI), MathF.Asin(val.Z) * (180f / MathF.PI), MathF.Asin(val.W) * (180f / MathF.PI)),
                4 => new Vector4(MathF.Acos(val.X) * (180f / MathF.PI), MathF.Acos(val.Y) * (180f / MathF.PI), MathF.Acos(val.Z) * (180f / MathF.PI), MathF.Acos(val.W) * (180f / MathF.PI)),
                5 => new Vector4(MathF.Atan(val.X) * (180f / MathF.PI), MathF.Atan(val.Y) * (180f / MathF.PI), MathF.Atan(val.Z) * (180f / MathF.PI), MathF.Atan(val.W) * (180f / MathF.PI)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            }
            : op switch
            {
                0 => new Vector4(MathF.Sin(val.X), MathF.Sin(val.Y), MathF.Sin(val.Z), MathF.Sin(val.W)),
                1 => new Vector4(MathF.Cos(val.X), MathF.Cos(val.Y), MathF.Cos(val.Z), MathF.Cos(val.W)),
                2 => new Vector4(MathF.Tan(val.X), MathF.Tan(val.Y), MathF.Tan(val.Z), MathF.Tan(val.W)),
                3 => new Vector4(MathF.Asin(val.X), MathF.Asin(val.Y), MathF.Asin(val.Z), MathF.Asin(val.W)),
                4 => new Vector4(MathF.Acos(val.X), MathF.Acos(val.Y), MathF.Acos(val.Z), MathF.Acos(val.W)),
                5 => new Vector4(MathF.Atan(val.X), MathF.Atan(val.Y), MathF.Atan(val.Z), MathF.Atan(val.W)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };
    }

    public Vector4 Atan2(Vector4 x, double y, bool deg)
    {
        Vector4 v = new Vector4((float)Math.Atan2(x.X, y), (float)Math.Atan2(x.Y, y), (float)Math.Atan2(x.Z, y), (float)Math.Atan2(x.W, y));
        return deg ? v * (180f / MathF.PI) : v;
    }

    public Vector4 Atan2(double x, Vector4 y, bool deg)
    {
        Vector4 v = new Vector4((float)Math.Atan2(x, y.X), (float)Math.Atan2(x, y.Y), (float)Math.Atan2(x, y.Z), (float)Math.Atan2(x, y.W));
        return deg ? v * (180f / MathF.PI) : v;
    }

    public Vector4 Atan2(Vector4 x, Vector4 y, bool deg)
    {
        Vector4 v = new Vector4(MathF.Atan2(x.X, y.X), MathF.Atan2(x.Y, y.Y), MathF.Atan2(x.Z, y.Z), MathF.Atan2(x.W, y.W));
        return deg ? v * (180f / MathF.PI) : v;
    }

    public Vector4 Sqrt(Vector4 val)
    {
        return new Vector4(MathF.Sqrt(val.X), MathF.Sqrt(val.Y), MathF.Sqrt(val.Z), MathF.Sqrt(val.W));
    }

    public Vector4 Construct(double scalar)
    {
        float fl = (float)scalar;
        return new Vector4(fl, fl, fl, fl);
    }

    public Vector4 Construct(ReadOnlySpan<double> components)
    {
        return new Vector4(
            components.Length < 1 ? 0f : (float)components[0],
            components.Length < 2 ? 0f : (float)components[1],
            components.Length < 2 ? 0f : (float)components[2],
            components.Length < 3 ? 0f : (float)components[3]
        );
    }

    public int Deconstruct(Vector4 val, Span<double> components)
    {
        if (components.Length <= 0)
            return 0;
        components[0] = val.X;
        if (components.Length <= 1)
            return 1;
        components[1] = val.Y;
        if (components.Length <= 2)
            return 2;
        components[2] = val.Z;
        if (components.Length <= 3)
            return 3;
        components[3] = val.W;
        return 4;
    }

    public int Compare(Vector4 left, Vector4 right)
    {
        const float tolerance = 0.0001f;

        float sub = left.X - right.X;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = left.Y - right.Y;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = left.Z - right.Z;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = left.W - right.W;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }

        return 0;
    }

    public double GetComponent(Vector4 val, int index)
    {
        return index switch
        {
            0 => val.X,
            1 => val.Y,
            2 => val.Z,
            3 => val.W,
            _ => 0
        };
    }

    public string ToString(Vector4 val)
    {
        return KnownTypeValueHelper.ToComponentString(val);
    }

    public bool TryParse([NotNullWhen(true)] string? str, out Vector4 vector)
    {
        return KnownTypeValueHelper.TryParseVector4Components(str, out vector);
    }
}
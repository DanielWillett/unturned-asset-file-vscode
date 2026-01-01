using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
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
internal class Vector2Provider : IVectorTypeProvider<Vector2>
{
    public ITypeConverter<Vector2> Converter => Vector2Type.Instance;
    public int Size => 2;

    public Vector2 Multiply(Vector2 val1, Vector2 val2)
    {
        return new Vector2(val1.X * val2.X, val1.Y * val2.Y);
    }

    public Vector2 Divide(Vector2 val1, Vector2 val2)
    {
        return new Vector2(val1.X / val2.X, val1.Y / val2.Y);
    }

    public Vector2 Add(Vector2 val1, Vector2 val2)
    {
        return new Vector2(val1.X + val2.X, val1.Y + val2.Y);
    }

    public Vector2 Subtract(Vector2 val1, Vector2 val2)
    {
        return new Vector2(val1.X - val2.X, val1.Y - val2.Y);
    }

    public Vector2 Modulo(Vector2 val1, Vector2 val2)
    {
        return new Vector2(val1.X % val2.X, val1.Y % val2.Y);
    }

    public Vector2 Power(Vector2 val1, Vector2 val2)
    {
        return new Vector2((float)Math.Pow(val1.X, val2.X), (float)Math.Pow(val1.Y, val2.Y));
    }

    public Vector2 Min(Vector2 val1, Vector2 val2)
    {
        return new Vector2(Math.Min(val1.X, val2.X), Math.Min(val1.Y, val2.Y));
    }

    public Vector2 Max(Vector2 val1, Vector2 val2)
    {
        return new Vector2(Math.Max(val1.X, val2.X), Math.Max(val1.Y, val2.Y));
    }

    public Vector2 Avg(Vector2 val1, Vector2 val2)
    {
        return new Vector2((val1.X + val2.X) / 2f, (val1.Y + val2.Y) / 2f);
    }

    public Vector2 Absolute(Vector2 val)
    {
        return new Vector2(Math.Abs(val.X), Math.Abs(val.Y));
    }

    public Vector2 Round(Vector2 val)
    {
        return new Vector2(MathF.Round(val.X), MathF.Round(val.Y));
    }

    public Vector2 Ceiling(Vector2 val)
    {
        return new Vector2(MathF.Ceiling(val.X), MathF.Ceiling(val.Y));
    }

    public Vector2 Floor(Vector2 val)
    {
        return new Vector2(MathF.Floor(val.X), MathF.Floor(val.Y));
    }

    public Vector2 TrigOperation(Vector2 val, int op, bool deg)
    {
        return deg
            ? op switch
            {
                0 => new Vector2(MathF.Sin(val.X * (MathF.PI / 180f)), MathF.Sin(val.Y * (MathF.PI / 180f))),
                1 => new Vector2(MathF.Cos(val.X * (MathF.PI / 180f)), MathF.Cos(val.Y * (MathF.PI / 180f))),
                2 => new Vector2(MathF.Tan(val.X * (MathF.PI / 180f)), MathF.Tan(val.Y * (MathF.PI / 180f))),
                3 => new Vector2(MathF.Asin(val.X) * (180f / MathF.PI), MathF.Asin(val.Y) * (180f / MathF.PI)),
                4 => new Vector2(MathF.Acos(val.X) * (180f / MathF.PI), MathF.Acos(val.Y) * (180f / MathF.PI)),
                5 => new Vector2(MathF.Atan(val.X) * (180f / MathF.PI), MathF.Atan(val.Y) * (180f / MathF.PI)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            }
            : op switch
            {
                0 => new Vector2(MathF.Sin(val.X), MathF.Sin(val.Y)),
                1 => new Vector2(MathF.Cos(val.X), MathF.Cos(val.Y)),
                2 => new Vector2(MathF.Tan(val.X), MathF.Tan(val.Y)),
                3 => new Vector2(MathF.Asin(val.X), MathF.Asin(val.Y)),
                4 => new Vector2(MathF.Acos(val.X), MathF.Acos(val.Y)),
                5 => new Vector2(MathF.Atan(val.X), MathF.Atan(val.Y)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };
    }

    public Vector2 Atan2(Vector2 x, double y, bool deg)
    {
        Vector2 v = new Vector2((float)Math.Atan2(x.X, y), (float)Math.Atan2(x.Y, y));
        return deg ? v * (180f / MathF.PI) : v;
    }

    public Vector2 Atan2(double x, Vector2 y, bool deg)
    {
        Vector2 v = new Vector2((float)Math.Atan2(x, y.X), (float)Math.Atan2(x, y.Y));
        return deg ? v * (180f / MathF.PI) : v;
    }

    public Vector2 Atan2(Vector2 x, Vector2 y, bool deg)
    {
        Vector2 v = new Vector2(MathF.Atan2(x.X, y.X), MathF.Atan2(x.Y, y.Y));
        return deg ? v * (180f / MathF.PI) : v;
    }

    public Vector2 Sqrt(Vector2 val)
    {
        return new Vector2(MathF.Sqrt(val.X), MathF.Sqrt(val.Y));
    }

    public Vector2 Construct(double scalar)
    {
        float fl = (float)scalar;
        return new Vector2(fl, fl);
    }

    public Vector2 Construct(ReadOnlySpan<double> components)
    {
        return new Vector2(
            components.Length < 1 ? 0f : (float)components[0],
            components.Length < 2 ? 0f : (float)components[1]
        );
    }

    public int Deconstruct(Vector2 val, Span<double> components)
    {
        if (components.Length <= 0)
            return 0;
        components[0] = val.X;
        if (components.Length <= 1)
            return 1;
        components[1] = val.Y;
        return 2;
    }

    public int Compare(Vector2 left, Vector2 right)
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

        return 0;
    }

    public double GetComponent(Vector2 val, int index)
    {
        return index switch
        {
            0 => val.X,
            1 => val.Y,
            _ => 0
        };
    }

    public string ToString(Vector2 val)
    {
        return KnownTypeValueHelper.ToComponentString(val);
    }

    public bool TryParse([NotNullWhen(true)] string? str, out Vector2 vector)
    {
        return KnownTypeValueHelper.TryParseVector2Components(str, out vector);
    }
}

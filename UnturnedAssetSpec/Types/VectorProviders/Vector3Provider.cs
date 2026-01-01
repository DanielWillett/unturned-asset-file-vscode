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
internal class Vector3Provider : IVectorTypeProvider<Vector3>
{
    public ITypeConverter<Vector3> Converter => Vector3Type.Instance;
    public int Size => 3;

    public Vector3 Multiply(Vector3 val1, Vector3 val2)
    {
        return new Vector3(val1.X * val2.X, val1.Y * val2.Y, val1.Z * val2.Z);
    }

    public Vector3 Divide(Vector3 val1, Vector3 val2)
    {
        return new Vector3(val1.X / val2.X, val1.Y / val2.Y, val1.Z / val2.Z);
    }

    public Vector3 Add(Vector3 val1, Vector3 val2)
    {
        return new Vector3(val1.X + val2.X, val1.Y + val2.Y, val1.Z + val2.Z);
    }

    public Vector3 Subtract(Vector3 val1, Vector3 val2)
    {
        return new Vector3(val1.X - val2.X, val1.Y - val2.Y, val1.Z - val2.Z);
    }

    public Vector3 Modulo(Vector3 val1, Vector3 val2)
    {
        return new Vector3(val1.X % val2.X, val1.Y % val2.Y, val1.Z % val2.Z);
    }

    public Vector3 Power(Vector3 val1, Vector3 val2)
    {
        return new Vector3((float)Math.Pow(val1.X, val2.X), (float)Math.Pow(val1.Y, val2.Y), (float)Math.Pow(val1.Z, val2.Z));
    }

    public Vector3 Min(Vector3 val1, Vector3 val2)
    {
        return new Vector3(Math.Min(val1.X, val2.X), Math.Min(val1.Y, val2.Y), Math.Min(val1.Z, val2.Z));
    }

    public Vector3 Max(Vector3 val1, Vector3 val2)
    {
        return new Vector3(Math.Max(val1.X, val2.X), Math.Max(val1.Y, val2.Y), Math.Max(val1.Z, val2.Z));
    }

    public Vector3 Avg(Vector3 val1, Vector3 val2)
    {
        return new Vector3((val1.X + val2.X) / 2f, (val1.Y + val2.Y) / 2f, (val1.Z + val2.Z) / 2f);
    }

    public Vector3 Absolute(Vector3 val)
    {
        return new Vector3(Math.Abs(val.X), Math.Abs(val.Y), Math.Abs(val.Z));
    }

    public Vector3 Round(Vector3 val)
    {
        return new Vector3(MathF.Round(val.X), MathF.Round(val.Y), MathF.Round(val.Z));
    }

    public Vector3 Ceiling(Vector3 val)
    {
        return new Vector3(MathF.Ceiling(val.X), MathF.Ceiling(val.Y), MathF.Ceiling(val.Z));
    }

    public Vector3 Floor(Vector3 val)
    {
        return new Vector3(MathF.Floor(val.X), MathF.Floor(val.Y), MathF.Floor(val.Z));
    }

    public Vector3 TrigOperation(Vector3 val, int op, bool deg)
    {
        return deg
            ? op switch
            {
                0 => new Vector3(MathF.Sin(val.X * (MathF.PI / 180f)), MathF.Sin(val.Y * (MathF.PI / 180f)), MathF.Sin(val.Z * (MathF.PI / 180f))),
                1 => new Vector3(MathF.Cos(val.X * (MathF.PI / 180f)), MathF.Cos(val.Y * (MathF.PI / 180f)), MathF.Cos(val.Z * (MathF.PI / 180f))),
                2 => new Vector3(MathF.Tan(val.X * (MathF.PI / 180f)), MathF.Tan(val.Y * (MathF.PI / 180f)), MathF.Tan(val.Z * (MathF.PI / 180f))),
                3 => new Vector3(MathF.Asin(val.X) * (180f / MathF.PI), MathF.Asin(val.Y) * (180f / MathF.PI), MathF.Asin(val.Z) * (180f / MathF.PI)),
                4 => new Vector3(MathF.Acos(val.X) * (180f / MathF.PI), MathF.Acos(val.Y) * (180f / MathF.PI), MathF.Acos(val.Z) * (180f / MathF.PI)),
                5 => new Vector3(MathF.Atan(val.X) * (180f / MathF.PI), MathF.Atan(val.Y) * (180f / MathF.PI), MathF.Atan(val.Z) * (180f / MathF.PI)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            }
            : op switch
            {
                0 => new Vector3(MathF.Sin(val.X), MathF.Sin(val.Y), MathF.Sin(val.Z)),
                1 => new Vector3(MathF.Cos(val.X), MathF.Cos(val.Y), MathF.Cos(val.Z)),
                2 => new Vector3(MathF.Tan(val.X), MathF.Tan(val.Y), MathF.Tan(val.Z)),
                3 => new Vector3(MathF.Asin(val.X), MathF.Asin(val.Y), MathF.Asin(val.Z)),
                4 => new Vector3(MathF.Acos(val.X), MathF.Acos(val.Y), MathF.Acos(val.Z)),
                5 => new Vector3(MathF.Atan(val.X), MathF.Atan(val.Y), MathF.Atan(val.Z)),
                _ => throw new ArgumentOutOfRangeException(nameof(op))
            };
    }

    public Vector3 Atan2(Vector3 x, double y, bool deg)
    {
        Vector3 v = new Vector3((float)Math.Atan2(x.X, y), (float)Math.Atan2(x.Y, y), (float)Math.Atan2(x.Z, y));
        return deg ? v * (180f / MathF.PI) : v;
    }

    public Vector3 Atan2(double x, Vector3 y, bool deg)
    {
        Vector3 v = new Vector3((float)Math.Atan2(x, y.X), (float)Math.Atan2(x, y.Y), (float)Math.Atan2(x, y.Z));
        return deg ? v * (180f / MathF.PI) : v;
    }

    public Vector3 Atan2(Vector3 x, Vector3 y, bool deg)
    {
        Vector3 v = new Vector3(MathF.Atan2(x.X, y.X), MathF.Atan2(x.Y, y.Y), MathF.Atan2(x.Z, y.Z));
        return deg ? v * (180f / MathF.PI) : v;
    }

    public Vector3 Sqrt(Vector3 val)
    {
        return new Vector3(MathF.Sqrt(val.X), MathF.Sqrt(val.Y), MathF.Sqrt(val.Z));
    }

    public Vector3 Construct(double scalar)
    {
        float fl = (float)scalar;
        return new Vector3(fl, fl, fl);
    }

    public Vector3 Construct(ReadOnlySpan<double> components)
    {
        return new Vector3(
            components.Length < 1 ? 0f : (float)components[0],
            components.Length < 2 ? 0f : (float)components[1],
            components.Length < 2 ? 0f : (float)components[2]
        );
    }

    public int Deconstruct(Vector3 val, Span<double> components)
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
        return 3;
    }

    public int Compare(Vector3 left, Vector3 right)
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

        return 0;
    }

    public double GetComponent(Vector3 val, int index)
    {
        return index switch
        {
            0 => val.X,
            1 => val.Y,
            2 => val.Z,
            _ => 0
        };
    }

    public string ToString(Vector3 val)
    {
        return KnownTypeValueHelper.ToComponentString(val);
    }

    public bool TryParse([NotNullWhen(true)] string? str, out Vector3 vector)
    {
        return KnownTypeValueHelper.TryParseVector3Components(str, out vector);
    }
}

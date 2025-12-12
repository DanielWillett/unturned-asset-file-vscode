using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;
using System.Numerics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Base class for all 3-component vector types.
/// </summary>
public abstract class Vector3SpecPropertyType : BaseSpecPropertyType<Vector3SpecPropertyType, Vector3>,
    IStringParseableSpecPropertyType,
    IVectorSpecPropertyType<Vector3>
{
    private protected abstract VectorTypeParseOptions Options { get; }

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    public string ToString(ISpecDynamicValue value)
    {
        Vector3 v = value.AsConcrete<Vector3>();
        return $"({v.X.ToString(CultureInfo.InvariantCulture)},{v.Y.ToString(CultureInfo.InvariantCulture)},{v.Z.ToString(CultureInfo.InvariantCulture)})";
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (KnownTypeValueHelper.TryParseVector3Components(span, out Vector3 value))
        {
            dynamicValue = new SpecDynamicConcreteValue<Vector3>(value, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out Vector3 value)
    {
        VectorTypeParseOptions options = Options;
        if (parse.Node == null)
        {
            if ((options & VectorTypeParseOptions.Legacy) == 0)
            {
                return MissingNode(in parse, out value);
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
                return MissingNode(in parse, out value);
            }

            if (parse.BaseKey == null)
            {
                return MissingNode(in parse, out value);
            }

            string xKey = parse.BaseKey + "_X";
            string yKey = parse.BaseKey + "_Y";
            string zKey = parse.BaseKey + "_Z";

            return TryParseFromDictionary(in parse, dict, xKey, yKey, zKey, out value);
        }

        if ((options & VectorTypeParseOptions.Object) != 0 && parse.Node is IDictionarySourceNode v3Struct)
        {
            return TryParseFromDictionary(in parse, v3Struct, "X", "Y", "Z", out value);
        }

        if ((options & VectorTypeParseOptions.Composite) != 0 && parse.Node is IValueSourceNode strValNode)
        {
            return KnownTypeValueHelper.TryParseVector3Components(strValNode.Value.AsSpan(), out value);
        }

        return FailedToParse(in parse, out value);
    }

    protected bool TryParseFromDictionary(in SpecPropertyTypeParseContext parse, IDictionarySourceNode dict, string xKey, string yKey, string zKey, out Vector3 value)
    {
        bool xBad = false, yBad = false, zBad = false;

        string? xVal = null, yVal = null, zVal = null;

        if (!dict.TryGetPropertyValue(xKey, out IValueSourceNode? xString))
        {
            xBad = true;
        }
        else
        {
            xVal = xString.Value;
        }

        if (!dict.TryGetPropertyValue(yKey, out IValueSourceNode? yString))
        {
            yBad = true;
        }
        else
        {
            yVal = yString.Value;
        }

        if (!dict.TryGetPropertyValue(zKey, out IValueSourceNode? zString))
        {
            zBad = true;
        }
        else
        {
            zVal = zString.Value;
        }

        if (parse.HasDiagnostics)
        {
            if (xBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, xKey),
                    Range = xString?.Range ?? yString?.Range ?? zString?.Range ?? dict.Range
                });
            }

            if (yBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, yKey),
                    Range = yString?.Range ?? zString?.Range ?? xString?.Range ?? dict.Range
                });
            }

            if (zBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, zKey),
                    Range = zString?.Range ?? yString?.Range ?? xString?.Range ?? dict.Range
                });
            }
        }

        if (xBad | yBad | zBad)
        {
            value = default;
            return false;
        }

        if (!KnownTypeValueHelper.TryParseFloat(xVal!, out float x)
            || !KnownTypeValueHelper.TryParseFloat(yVal!, out float y)
            || !KnownTypeValueHelper.TryParseFloat(zVal!, out float z))
        {
            return FailedToParse(in parse, out value);
        }

        value = new Vector3(x, y, z);
        return true;
    }

    void IVectorSpecPropertyType.Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.Visit(this);
    }

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

    public Vector3 Sqrt(Vector3 val)
    {
        return new Vector3(MathF.Sqrt(val.X), MathF.Sqrt(val.Y), MathF.Sqrt(val.Z));
    }

    public Vector3 Construct(double scalar)
    {
        float fl = (float)scalar;
        return new Vector3(fl, fl, fl);
    }
}
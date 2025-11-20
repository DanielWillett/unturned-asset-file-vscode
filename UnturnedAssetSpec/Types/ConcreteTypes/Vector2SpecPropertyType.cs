using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;
using System.Numerics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Vector with X and Y components formatted either as an object or as a composite string.
/// <para>Example: <c>VehicleAsset.CrawlerTrackTilingMaterial.UV_Direction</c></para>
/// <code>
/// // string
/// Prop (1.0, 2.0)
///
/// // object
/// Prop
/// {
///     X 1.0
///     Y 2.0
/// }
/// </code>
/// </summary>
public sealed class Vector2SpecPropertyType : BasicSpecPropertyType<Vector2SpecPropertyType, Vector2>,
    IStringParseableSpecPropertyType,
    IVectorSpecPropertyType<Vector2>
{
    public static readonly Vector2SpecPropertyType Instance = new Vector2SpecPropertyType();

    /// <inheritdoc cref="ISpecPropertyType.Type" />
    public override string Type => "Vector2";

    /// <inheritdoc cref="ISpecPropertyType.DisplayName" />
    public override string DisplayName => "2D Vector";

    static Vector2SpecPropertyType() { }

    private Vector2SpecPropertyType() { }


    /// <inheritdoc />
    public override int GetHashCode() => 89;

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    /// <inheritdoc />
    public string ToString(ISpecDynamicValue value)
    {
        Vector2 v = value.AsConcrete<Vector2>();
        return $"({v.X.ToString(CultureInfo.InvariantCulture)},{v.Y.ToString(CultureInfo.InvariantCulture)})";
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (KnownTypeValueHelper.TryParseVector2Components(span, out Vector2 value))
        {
            dynamicValue = new SpecDynamicConcreteValue<Vector2>(value, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out Vector2 value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is IDictionarySourceNode v2Struct)
        {
            return TryParseFromDictionary(in parse, v2Struct, "X", "Y", out value);
        }

        if (parse.Node is IValueSourceNode strValNode)
        {
            return KnownTypeValueHelper.TryParseVector2Components(strValNode.Value.AsSpan(), out value);
        }

        return FailedToParse(in parse, out value);
    }

    private bool TryParseFromDictionary(in SpecPropertyTypeParseContext parse, IDictionarySourceNode dict, string xKey, string yKey, out Vector2 value)
    {
        bool xBad = false, yBad = false;

        string? xVal = null, yVal = null;

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

        if (parse.HasDiagnostics)
        {
            if (xBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, xKey),
                    Range = xString?.Range ?? yString?.Range ?? dict.Range
                });
            }

            if (yBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, yKey),
                    Range = yString?.Range ?? xString?.Range ?? dict.Range
                });
            }
        }

        if (xBad | yBad)
        {
            value = default;
            return false;
        }

        if (!KnownTypeValueHelper.TryParseFloat(xVal!, out float x)
            || !KnownTypeValueHelper.TryParseFloat(yVal!, out float y))
        {
            return FailedToParse(in parse, out value);
        }

        value = new Vector2(x, y);
        return true;
    }

    void IVectorSpecPropertyType.Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.Visit(this);
    }

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

    public Vector2 Sqrt(Vector2 val)
    {
        return new Vector2(MathF.Sqrt(val.X), MathF.Sqrt(val.Y));
    }

    public Vector2 Construct(double scalar)
    {
        float fl = (float)scalar;
        return new Vector2(fl, fl);
    }
}
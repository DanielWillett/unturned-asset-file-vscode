using System;
using System.Runtime.InteropServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public readonly struct Color : IEquatable<Color>
{
    [FieldOffset(0)]
    public readonly float A;

    [FieldOffset(4)]
    public readonly float R;

    [FieldOffset(8)]
    public readonly float G;

    [FieldOffset(12)]
    public readonly float B;

    public static readonly Color Black = new Color(1f, 0f, 0f, 0f);

    public Color(float a, float r, float g, float b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    /// <inheritdoc />
    public bool Equals(Color other)
    {
        return A == other.A && R == other.R && G == other.G && B == other.B;
    }

    /// <summary>
    /// The two colors are component-wise nearly eqaul.
    /// </summary>
    public bool AlmostEquals(Color other, float tolerance = 0.00390625f)
    {
        return Math.Abs(A - other.A) < tolerance && Math.Abs(R - other.R) < tolerance && Math.Abs(G - other.G) < tolerance && Math.Abs(B - other.B) < tolerance;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Color other && Equals(other);

    /// <inheritdoc />
    public override unsafe int GetHashCode()
    {
        float ttl = A + R + G + B;
        return *(int*)&ttl;
    }

    public static bool operator ==(Color left, Color right) => left.Equals(right);
    public static bool operator !=(Color left, Color right) => !left.Equals(right);

    public static implicit operator Color32(Color c)
    {
        return new Color32(
            (byte)Math.Round(Clamp01(c.A) * 255),
            (byte)Math.Round(Clamp01(c.R) * 255),
            (byte)Math.Round(Clamp01(c.G) * 255),
            (byte)Math.Round(Clamp01(c.B) * 255)
        );
    }

    public static implicit operator Color(Color32 c)
    {
        return new Color(
            c.A / 255f,
            c.R / 255f,
            c.G / 255f,
            c.B / 255f
        );
    }

    private static float Clamp01(float a)
    {
        return a < 1 ? a > 0 ? a : 0 : 1;
    }
}
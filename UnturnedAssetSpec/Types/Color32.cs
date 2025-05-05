using System;
using System.Runtime.InteropServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[StructLayout(LayoutKind.Explicit, Size = 4)]
public readonly struct Color32 : IEquatable<Color32>
{
    [FieldOffset(0)]
    public readonly byte A;

    [FieldOffset(1)]
    public readonly byte R;

    [FieldOffset(2)]
    public readonly byte G;

    [FieldOffset(3)]
    public readonly byte B;

    public static readonly Color32 Black = new Color32(255, 0, 0, 0);

    public Color32(byte a, byte r, byte g, byte b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    /// <inheritdoc />
    public bool Equals(Color32 other)
    {
        return A == other.A && R == other.R && G == other.G && B == other.B;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Color32 other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return A << 24 | R << 16 | G << 8 | B;
    }

    public static bool operator ==(Color32 left, Color32 right) => left.Equals(right);
    public static bool operator !=(Color32 left, Color32 right) => !left.Equals(right);
}
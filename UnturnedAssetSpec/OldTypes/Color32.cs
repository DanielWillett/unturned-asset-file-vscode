using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using System;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A 32-bit color with values ranged 0 to 255.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 4)]
[JsonConverter(typeof(Color32Converter))]
[VectorTypeProvider(typeof(Color32VectorProvider))]
public readonly struct Color32 : IEquatable<Color32>, IComparable<Color32>
{
    [FieldOffset(0)]
    public readonly byte A;

    [FieldOffset(1)]
    public readonly byte R;

    [FieldOffset(2)]
    public readonly byte G;

    [FieldOffset(3)]
    public readonly byte B;

    public static readonly Color32 White = new Color32(255, 255, 255, 255);
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

    /// <summary>
    /// Compares colors in order of A, R, G, B.
    /// </summary>
    /// <returns>1 if this is greater than <paramref name="other"/>, -1 if this is less than <paramref name="other"/>, 0 if they're nearly equal.</returns>
    public int CompareTo(Color32 other)
    {
        int comp = A << 24 | R << 16 | G << 8 | B;
        int otherComp = other.A << 24 | other.R << 16 | other.G << 8 | other.B;
        return Math.Sign(comp - otherComp);
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

    public string ToHex(bool includeAlpha)
    {
        int length = 7 + (includeAlpha ? 1 : 0) * 2;
        Span<char> chars = stackalloc char[length];
        chars[0] = '#';
        WriteByte(chars, 1, R);
        WriteByte(chars, 3, G);
        WriteByte(chars, 5, B);
        if (includeAlpha)
        {
            WriteByte(chars, 7, A);
        }

        return chars.ToString();

        static void WriteByte(Span<char> c, int index, byte b)
        {
            int nibl = (b & 0xF0) >> 4;
            c[index] = nibl > 9 ? (char)(nibl + 87) : (char)(nibl + 48);
            nibl = b & 0xF;
            c[index + 1] = nibl > 9 ? (char)(nibl + 87) : (char)(nibl + 48);
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ToHex(A != byte.MaxValue);
    }
}
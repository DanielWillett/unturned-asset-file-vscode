using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Position of a character in a file.
/// </summary>
public struct FilePosition : IEquatable<FilePosition>, IComparable<FilePosition>, IComparable
{
    /// <summary>
    /// Index from one of the line the character is on.
    /// </summary>
    public int Line;

    /// <summary>
    /// Index from one of the character in its line.
    /// </summary>
    public int Character;

    public bool IsInvalid => Line <= 0 || Character <= 0;

    public FilePosition() { }

    public FilePosition(int line, int character)
    {
        Line = line;
        Character = character;
    }

    public void Deconstruct(out int line, out int character)
    {
        line = Line;
        character = Character;
    }

    /// <inheritdoc />
    public readonly bool Equals(FilePosition other)
    {
        return Line == other.Line && Character == other.Character;
    }

    /// <inheritdoc />
    public readonly override bool Equals(object? obj)
    {
        return obj is FilePosition fp && Equals(fp);
    }

    /// <inheritdoc />
    public readonly override int GetHashCode() => Line ^ Character;

    /// <inheritdoc />
    public readonly int CompareTo(FilePosition other)
    {
        int num = Line.CompareTo(other.Line);
        return num == 0 ? Character.CompareTo(other.Character) : num;
    }

    /// <inheritdoc />
    public readonly int CompareTo(object obj) => obj is FilePosition p ? CompareTo(p) : 1;

    public static bool operator ==(FilePosition left, FilePosition right) => left.Line == right.Line && left.Character == right.Character;
    public static bool operator !=(FilePosition left, FilePosition right) => left.Line != right.Line || left.Character != right.Character;
    public static bool operator <(FilePosition left, FilePosition right) => left.Line == right.Line ? left.Character < right.Character : left.Line < right.Line;
    public static bool operator >(FilePosition left, FilePosition right) => left.Line == right.Line ? left.Character > right.Character : left.Line > right.Line;
    public static bool operator <=(FilePosition left, FilePosition right) => left.Line == right.Line ? left.Character <= right.Character : left.Line <= right.Line;
    public static bool operator >=(FilePosition left, FilePosition right) => left.Line == right.Line ? left.Character >= right.Character : left.Line >= right.Line;

    public static implicit operator FilePosition((int line, int character) tuple) => new FilePosition(tuple.line, tuple.character);

    public override string ToString() => IsInvalid ? "Invalid File Position" : $"L{Line} C{Character}";

}
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Range of a selection in a file.
/// </summary>
public struct FileRange : IEquatable<FileRange>, IComparable<FileRange>, IComparable
{
    /// <summary>
    /// First character in the selection (inclusive).
    /// </summary>
    public FilePosition Start;

    /// <summary>
    /// Last character in the selection (inclusive).
    /// </summary>
    public FilePosition End;

    public FileRange() { }

    public FileRange(FilePosition start, FilePosition end)
    {
        Start = start;
        End = end;
    }

    public FileRange(int startLine, int startCharacter, int endLine, int endCharacter)
    {
        Start.Line = startLine;
        Start.Character = startCharacter;
        End.Line = endLine;
        End.Character = endCharacter;
    }

    /// <inheritdoc />
    public override string ToString() => $"{Start} - {End}";

    public void Deconstruct(out FilePosition start, out FilePosition end)
    {
        start = Start;
        end = End;
    }

    public void Deconstruct(out int startLine, out int startCharacter, out int endLine, out int endCharacter)
    {
        startLine = Start.Line;
        startCharacter = Start.Character;
        endLine = End.Line;
        endCharacter = End.Character;
    }

    public void Encapsulate(FileRange otherRange)
    {
        Start.Line = Math.Min(Start.Line, otherRange.Start.Line);
        Start.Character = Math.Min(Start.Character, otherRange.Start.Character);

        End.Line = Math.Max(End.Line, otherRange.End.Line);
        End.Character = Math.Max(End.Character, otherRange.End.Character);
    }

    public readonly bool Contains(FilePosition position)
    {
        return position.Line >= Start.Line
               && position.Line <= End.Line
               && (position.Line != Start.Line || position.Character >= Start.Character)
               && (position.Line != End.Line || position.Character <= End.Character);
    }

    /// <inheritdoc />
    public readonly bool Equals(FileRange other)
    {
        return Start == other.Start && End == other.End;
    }

    /// <inheritdoc />
    public readonly override bool Equals(object? obj)
    {
        return obj is FileRange fp && Equals(fp);
    }

    /// <inheritdoc />
    public readonly override int GetHashCode()
    {
        return ((Start.Line << 16) | (Start.Line >> 16)) ^ ((Start.Character << 16) | (Start.Character >> 16))
                                                         ^ End.Line
                                                         ^ End.Character;
    }

    /// <inheritdoc />
    public readonly int CompareTo(FileRange other)
    {
        int num = Start.CompareTo(other.Start);
        return num == 0 ? End.CompareTo(other.End) : num;
    }

    /// <inheritdoc />
    public readonly int CompareTo(object obj) => obj is FileRange p ? CompareTo(p) : 1;

    public static bool operator ==(FileRange left, FileRange right) => left.Start == right.Start && left.End == right.End;
    public static bool operator !=(FileRange left, FileRange right) => left.Start != right.Start || left.End != right.End;
    public static bool operator <(FileRange left, FileRange right) => left.Start == right.Start ? left.End < right.End : left.Start < right.Start;
    public static bool operator >(FileRange left, FileRange right) => left.Start == right.Start ? left.End > right.End : left.Start > right.Start;
    public static bool operator <=(FileRange left, FileRange right) => left.Start == right.Start ? left.End <= right.End : left.Start <= right.Start;
    public static bool operator >=(FileRange left, FileRange right) => left.Start == right.Start ? left.End >= right.End : left.Start >= right.Start;
    
    public static implicit operator FileRange((FilePosition start, FilePosition end) tuple) => new FileRange(tuple.start, tuple.end);

    public static implicit operator FileRange((int startLine, int startCharacter, int endLine, int endCharacter) tuple)
        => new FileRange(tuple.startLine, tuple.startCharacter, tuple.endLine, tuple.endCharacter);
}
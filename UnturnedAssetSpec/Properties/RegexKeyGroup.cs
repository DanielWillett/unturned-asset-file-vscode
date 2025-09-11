using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public readonly struct RegexKeyGroup : IEquatable<RegexKeyGroup>
{
    public int Group { get; }
    public string Name { get; }

    /// <summary>
    /// The name of a property with $n formatted for each key group (like $1 for group 1's value).
    /// </summary>
    /// <remarks>The value of this property will be formatted into the key instead of the index.</remarks>
    public string? UseValueOf { get; }

    public RegexKeyGroup(int group, string name, string? useValueOf) : this(group, name)
    {
        UseValueOf = useValueOf;
    }
    public RegexKeyGroup(int group, string name)
    {
        Group = group;
        Name = name;
    }

    public override string ToString() => Name;

    public override int GetHashCode()
    {
        int hash = Name != null ? Name.GetHashCode() ^ (Group * 397) : 0;

        if (UseValueOf != null)
            hash ^= UseValueOf.GetHashCode() * 397;

        return hash;
    }

    public override bool Equals(object? obj) => obj is RegexKeyGroup g && Equals(g);
    public bool Equals(RegexKeyGroup other)
    {
        return Group == other.Group
               && string.Equals(Name, other.Name, StringComparison.Ordinal)
               && string.Equals(UseValueOf, other.UseValueOf, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator ==(RegexKeyGroup left, RegexKeyGroup right) => left.Equals(right);
    public static bool operator !=(RegexKeyGroup left, RegexKeyGroup right) => !left.Equals(right);
}
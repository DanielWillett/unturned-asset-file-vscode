using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public readonly struct RegexKeyGroup : IEquatable<RegexKeyGroup>
{
    public int Group { get; }
    public string Name { get; }

    public RegexKeyGroup(int group, string name)
    {
        Group = group;
        Name = name;
    }

    public override string ToString() => Name;

    public override int GetHashCode() => Name != null ? Name.GetHashCode() ^ (Group << 16) : 0;
    public override bool Equals(object? obj) => obj is RegexKeyGroup g && Equals(g);
    public bool Equals(RegexKeyGroup other)
    {
        return Group == other.Group && string.Equals(Name, other.Name, StringComparison.Ordinal);
    }

    public static bool operator ==(RegexKeyGroup left, RegexKeyGroup right) => left.Equals(right);
    public static bool operator !=(RegexKeyGroup left, RegexKeyGroup right) => !left.Equals(right);
}
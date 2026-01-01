using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public class TemplateGroup : IEquatable<TemplateGroup>
{
    /// <summary>
    /// The one-based index of the group.
    /// </summary>
    public int Group { get; }

    /// <summary>
    /// The name of the group common to an asset tree.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The name of a property with $n formatted for each key group (like $1 for group 1's value).
    /// </summary>
    /// <remarks>The value of this property will be formatted into the key instead of the index.</remarks>
    public string? UseValueOf { get; }

    public TemplateGroup(int group, string name, string useValueOf) : this(group, name)
    {
        UseValueOf = useValueOf ?? throw new ArgumentNullException(nameof(useValueOf));
    }

    public TemplateGroup(int group, string name)
    {
        Group = group < 1 ? throw new ArgumentOutOfRangeException(nameof(group)) : group;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public override string ToString() => Name;

    public override int GetHashCode()
    {
        return HashCode.Combine(Group, Name, UseValueOf);
    }

    public override bool Equals(object? obj) => obj is TemplateGroup g && Equals(g);
    public bool Equals(TemplateGroup other)
    {
        return Group == other.Group
               && string.Equals(Name, other.Name, StringComparison.Ordinal)
               && string.Equals(UseValueOf, other.UseValueOf, StringComparison.OrdinalIgnoreCase);
    }

    public static bool operator ==(TemplateGroup left, TemplateGroup right) => left.Equals(right);
    public static bool operator !=(TemplateGroup left, TemplateGroup right) => !left.Equals(right);

    /// <summary>
    /// Writes a <see cref="TemplateGroup"/> to a JSON string or object.
    /// </summary>
    /// <param name="index">The zero-based index of this group within it's array.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative.</exception>
    /// <exception cref="ArgumentNullException"/>
    public void WriteToJson(int index, Utf8JsonWriter writer)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));

        bool isWrongGroup = index != Group - 1;
        bool hasUvo = !string.IsNullOrEmpty(UseValueOf);
        if (hasUvo || isWrongGroup)
        {
            writer.WriteStartObject();

            if (isWrongGroup)
                writer.WriteNumber("Group"u8, Group);
            writer.WriteString("Name"u8, Name);
            if (hasUvo)
                writer.WriteString("UseValueOf"u8, UseValueOf);

            writer.WriteEndObject();
        }
        else
        {
            writer.WriteStringValue(Name);
        }
    }

    /// <summary>
    /// Attempts to read a <see cref="TemplateGroup"/> from a JSON string or object.
    /// </summary>
    /// <param name="index">The zero-based index of this group within it's array.</param>
    /// <param name="root">The root JSON object or string token.</param>
    /// <param name="group">The output group.</param>
    /// <returns>Whether or not a <see cref="TemplateGroup"/> could be successfully parsed.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative.</exception>
    public static bool TryReadFromJson(int index, in JsonElement root, [NotNullWhen(true)] out TemplateGroup? group)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        group = null;

        switch (root.ValueKind)
        {
            case JsonValueKind.String:
                string str = root.GetString();
                if (string.IsNullOrEmpty(str))
                    return false;

                group = new TemplateGroup(index + 1, str);
                return true;

            case JsonValueKind.Object:
                str = null;
                if (root.TryGetProperty("Name"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
                {
                    if (element.ValueKind != JsonValueKind.String)
                        return false;
                    str = element.GetString();
                }

                string? uvo = null;
                if (root.TryGetProperty("UseValueOf"u8, out element) && element.ValueKind != JsonValueKind.Null)
                {
                    if (element.ValueKind != JsonValueKind.String)
                        return false;
                    uvo = element.GetString();
                    if (uvo.Length == 0)
                        uvo = null;
                }

                if (string.IsNullOrEmpty(str))
                    return false;

                int groupNum = index + 1;
                if (root.TryGetProperty("Group"u8, out element) && element.ValueKind != JsonValueKind.Null)
                {
                    if (!element.TryGetInt32(out int grp) || grp < 1)
                        return false;
                    groupNum = grp;
                }

                group = uvo == null ? new TemplateGroup(groupNum, str) : new TemplateGroup(groupNum, str, uvo);
                return true;

            default:
                return false;
        }
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

[JsonConverter(typeof(SpecPropertyConverter))]
public class SpecProperty
{
    public string Key { get; set; }
    public string? Description { get; set; }
    public string? Markdown { get; set; }
    public string? ElementType { get; set; }
    public string[]? Aliases { get; set; }
    public string[]? SpecialTypes { get; set; }
    public bool CanBeInMetadata { get; set; }
    public bool Deprecated { get; set; }

    public IDefaultValue? DefaultValue { get; set; }
    public ISpecPropertyType Type { get; set; }
}

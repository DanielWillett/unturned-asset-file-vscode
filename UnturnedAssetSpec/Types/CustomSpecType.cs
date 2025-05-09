using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Collections.Generic;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[DebuggerDisplay("{DisplayName,nq}")]
public sealed class CustomSpecType : ISpecType
{
    public QualifiedType Type { get; internal set; }

    public string? Docs { get; set; }

    public QualifiedType Parent { get; set; }

    public string DisplayName { get; set; } = string.Empty;

#nullable disable
    public List<SpecProperty> Properties { get; set; }
    public List<SpecProperty> LocalizationProperties { get; set; }
#nullable restore

    public bool Equals(AssetTypeInformation other) => other != null && Type.Equals(other.Type);
    public bool Equals(ISpecType other) => other is AssetTypeInformation t && Type.Equals(t.Type);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AssetTypeInformation ti && Equals(ti);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Type.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString() => Type.ToString();
}
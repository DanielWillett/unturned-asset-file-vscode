using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// A Unity asset that can be defined in the bundle of a <see cref="DatAssetType"/> or within an instance of a custom type.
/// </summary>
public sealed class DatBundleAsset : DatProperty
{
    /// <summary>
    /// Whether or not this property is a template property that uses <see cref="TemplateGroups"/>.
    /// </summary>
    /// <remarks>Corresponds to the <c>Template</c> property.</remarks>
    [MemberNotNullWhen(true, nameof(TemplateGroups))]
    public bool IsTemplate { get; internal set; }

    /// <summary>
    /// Whether or not every value in this template group should be unique.
    /// </summary>
    /// <remarks>Corresponds to the <c>TemplateGroupUniqueValue</c> property.</remarks>
    public bool TemplateGroupUniqueValue { get; internal set; }

    /// <summary>
    /// List of template grops, if any.
    /// </summary>
    /// <remarks>Corresponds to the <c>TemplateGroups</c> property.</remarks>
    public ImmutableArray<TemplateGroup> TemplateGroups { get; internal set; }

    /// <inheritdoc />
    internal DatBundleAsset(string key, IPropertyType type, DatTypeWithProperties owner, JsonElement element, SpecPropertyContext context)
        : base(key, owner, element, context)
    {
        Type = type;
    }
}

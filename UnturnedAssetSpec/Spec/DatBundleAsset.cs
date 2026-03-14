using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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

    /// <inheritdoc cref="DatProperty.Type"/>
    public new IBundleAssetType Type => (IBundleAssetType)base.Type;

    /// <inheritdoc />
    internal DatBundleAsset(string key, IBundleAssetType type, DatTypeWithProperties owner, JsonElement element, SpecPropertyContext context)
        : base(key, owner, element, context)
    {
        base.Type = type;
    }

    /// <summary>
    /// Creates a new bundle asset property.
    /// </summary>
    /// <param name="key">The name of the bundle asset in the bundle (ex. <c>Barricade</c>).</param>
    /// <param name="type">The type of Unity object to expect.</param>
    /// <param name="owner">The type that defines this bundle asset.</param>
    /// <param name="element">The JSON data behind this element.</param>
    /// <returns>The newly created bundle asset.</returns>
    /// <exception cref="ArgumentNullException">One of the required parameters is <see langword="null"/>.</exception>
    public static DatBundleAsset Create(string key, IBundleAssetType type, DatTypeWithProperties owner, JsonElement element)
    {
        return new DatBundleAsset(
            key ?? throw new ArgumentNullException(nameof(key)),
            type ?? throw new ArgumentNullException(nameof(type)),
            owner ?? throw new ArgumentNullException(nameof(owner)),
            element,
            SpecPropertyContext.BundleAsset
        );
    }
}

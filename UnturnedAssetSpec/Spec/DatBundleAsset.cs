using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// A Unity asset that can be defined in the bundle of a <see cref="DatAssetType"/> or within an instance of a custom type.
/// </summary>
public sealed class DatBundleAsset : DatProperty
{
    /// <inheritdoc />
    internal DatBundleAsset(string key, IPropertyType type, DatTypeWithProperties owner, JsonElement element, SpecPropertyContext context)
        : base(key, owner, element, context)
    {
        Type = type;
    }
}

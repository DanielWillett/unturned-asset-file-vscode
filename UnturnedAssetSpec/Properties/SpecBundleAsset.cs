using System;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

[JsonConverter(typeof(SpecBundleAssetConverter))]
public class SpecBundleAsset : SpecProperty, IEquatable<SpecBundleAsset>
{
    public bool Equals(SpecBundleAsset other)
    {
        return Equals((SpecProperty)other);
    }
}
namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// A reference to a bundle through the eyes of an asset file, applying the asset's path when searching for objects.
/// </summary>
public interface IBundleProxy
{
    /// <summary>
    /// The bundle represented by this <see cref="IBundleProxy"/>.
    /// </summary>
    DiscoveredBundle? Bundle { get; }

    /// <summary>
    /// The path to the root folder for this asset in <see cref="Bundle"/>.
    /// </summary>
    string? Path { get; }

    /// <summary>
    /// Whether or not this bundle will convert all loaded Materials to the <c>Standard</c> shader.
    /// </summary>
    bool ConvertShadersToStandard { get; }

    /// <summary>
    /// Whether or not this bundle will map all shaders to the shaders already loaded by vanilla using their name.
    /// </summary>
    bool ConsolidateShaders { get; }
}

internal sealed class NullBundleProxy : IBundleProxy
{
    internal static readonly NullBundleProxy Instance = new NullBundleProxy();
    static NullBundleProxy() { }
    DiscoveredBundle? IBundleProxy.Bundle => null;
    string? IBundleProxy.Path => null;
    bool IBundleProxy.ConvertShadersToStandard => false;
    bool IBundleProxy.ConsolidateShaders => false;

    public override string ToString() => "Null Bundle";
    public override bool Equals(object? obj) => obj is NullBundleProxy;
    public override int GetHashCode() => 1928236888;
}
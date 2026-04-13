using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// A reference to a bundle through the eyes of an asset file, applying the asset's path when searching for objects.
/// </summary>
public interface IBundleProxy
{
    /// <summary>
    /// Whether or not this proxy references an existing bundle.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Bundle))]
    [MemberNotNullWhen(true, nameof(Path))]
    bool Exists { get; }

    /// <summary>
    /// The bundle represented by this <see cref="IBundleProxy"/>.
    /// </summary>
    /// <value><see langword="null"/> unless <see cref="Exists"/> is <see langword="true"/>.</value>
    DiscoveredBundle? Bundle { get; }

    /// <summary>
    /// The path to the root folder for this asset in <see cref="Bundle"/>.
    /// </summary>
    /// <value><see langword="null"/> unless <see cref="Exists"/> is <see langword="true"/>.</value>
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
    bool IBundleProxy.Exists => false;
    DiscoveredBundle? IBundleProxy.Bundle => null;
    string? IBundleProxy.Path => null;
    bool IBundleProxy.ConvertShadersToStandard => false;
    bool IBundleProxy.ConsolidateShaders => false;

    public override string ToString() => "Null Bundle";
    public override bool Equals(object? obj) => obj is NullBundleProxy;
    public override int GetHashCode() => 1928236888;
}
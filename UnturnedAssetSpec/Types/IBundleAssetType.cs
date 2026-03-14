namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A UnityEngine Object type for a bundle asset.
/// </summary>
public interface IBundleAssetType : IType
{
    /// <summary>
    /// The type name of the object type.
    /// </summary>
    QualifiedType TypeName { get; }
}
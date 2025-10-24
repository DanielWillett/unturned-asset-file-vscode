using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// The root dictionary of a file.
/// </summary>
public interface ISourceFile : IDictionarySourceNode, IAdditionalPropertyProvider
{
    /// <summary>
    /// The file this root source node is from.
    /// </summary>
    IWorkspaceFile WorkspaceFile { get; }

    /// <summary>
    /// Object to lock on when traversing the file tree.
    /// </summary>
    object TreeSync { get; }

    /// <summary>
    /// List of properties at the root level.
    /// </summary>
    ImmutableArray<IPropertySourceNode> Properties { get; }

    /// <summary>
    /// The fully resolved type of the asset, if provided. This will never be a type alias.
    /// </summary>
    QualifiedType ActualType { get; }
}

/// <summary>
/// The source file for an asset dat file.
/// </summary>
public interface IAssetSourceFile : ISourceFile
{
    /// <summary>
    /// List of all available localization files for this asset.
    /// </summary>
    /// <remarks>If present, the English localization file will be first.</remarks>
    ImmutableArray<ILocalizationSourceFile> Localization { get; }

    /// <summary>
    /// The GUID of the asset, if provided.
    /// </summary>
    Guid? Guid { get; }

    /// <summary>
    /// The ID of the asset, if provided.
    /// </summary>
    ushort? Id { get; }
    
    /// <summary>
    /// The internal name given to the asset.
    /// </summary>
    string AssetName { get; }

    /// <summary>
    /// The category of the asset, or NONE if not provided or available.
    /// </summary>
    AssetCategoryValue Category { get; }

    /// <summary>
    /// The type of the asset, if provided. This may be a type alias.
    /// </summary>
    QualifiedOrAliasedType AssetType { get; }

    /// <summary>
    /// If this asset file has some errors that prevent basic metadata from being read (missing GUID, ID, Type, etc.).
    /// </summary>
    bool IsErrored { get; }
}

public interface ILocalizationSourceFile : ISourceFile
{
    /// <summary>
    /// Name of the language, such as 'English'.
    /// </summary>
    string LanguageName { get; }

    /// <summary>
    /// The 
    /// </summary>
    IAssetSourceFile Asset { get; }
}
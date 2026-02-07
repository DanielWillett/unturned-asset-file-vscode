using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// A source file that won't change. Acts as a basic implementation of <see cref="IWorkspaceFile"/>.
/// </summary>
public sealed class StaticSourceFile : IWorkspaceFile
{
    internal StaticSourceFileWorkspaceEnvironment? Environment;

    private readonly ReadOnlyMemory<char> _fileContent;
    private string? _fileContentStr;

    /// <inheritdoc />
    public string File { get; }

    /// <inheritdoc />
    public ISourceFile SourceFile { get; }

    /// <summary>
    /// Create a source file from an asset file.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="database">The information database to use to read the file.</param>
    /// <param name="options">Options to use when reading the file.</param>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="PathTooLongException"><paramref name="filePath"/> is too long.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory containing <paramref name="filePath"/> does not exist.</exception>
    /// <exception cref="FileNotFoundException">The file at <paramref name="filePath"/> does not exist.</exception>
    /// <exception cref="IOException">There was an I/O issue reading the file at <paramref name="filePath"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Unable to access the file at <paramref name="filePath"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="filePath"/> is invalid.</exception>
    /// <exception cref="System.Security.SecurityException">Unable to access the file at <paramref name="filePath"/>.</exception>
    public static StaticSourceFile FromAssetFile(
        string filePath,
        IAssetSpecDatabase database,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Lazy)
    {
        return FromAssetFile(filePath, System.IO.File.ReadAllText(filePath, Encoding.UTF8).AsMemory(), database, options);
    }

    /// <summary>
    /// Create a source file from an asset file, given the already read file contents. File contents can be excluded to read the file directly.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="fileContents">Full text of the asset file to read from.</param>
    /// <param name="database">The information database to use to read the file.</param>
    /// <param name="options">Options to use when reading the file.</param>
    /// <exception cref="ArgumentNullException"/>
    public static StaticSourceFile FromAssetFile(
        string filePath,
        ReadOnlyMemory<char> fileContents,
        IAssetSpecDatabase database,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Lazy)
    {
        if (filePath == null)
            throw new ArgumentNullException(nameof(filePath));
        if (database == null)
            throw new ArgumentNullException(nameof(database));

        return new StaticSourceFile(filePath, database, fileContents, options, static (file, database, fileContents, options, _) =>
        {
            SourceNodeTokenizer.RootInfo info = SourceNodeTokenizer.RootInfo.Asset(file, database);
            using SourceNodeTokenizer tokenizer = new SourceNodeTokenizer(fileContents, options);
            return tokenizer.ReadRootDictionary(info);
        }, null);
    }

    /// <inheritdoc cref="FromOtherFile(string,ReadOnlyMemory{char},IAssetSpecDatabase?,SourceNodeTokenizerOptions)"/>
    public static StaticSourceFile FromAssetFile(
        string filePath,
        string fileContents,
        IAssetSpecDatabase database,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Lazy)
    {
        return FromAssetFile(filePath, fileContents.AsMemory(), database, options);
    }

    /// <summary>
    /// Create a source file from an <paramref name="asset"/>'s localization file.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="database">The information database to use to read the file.</param>
    /// <param name="asset">The asset this localization file belongs to.</param>
    /// <param name="options">Options to use when reading the file.</param>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="PathTooLongException"><paramref name="filePath"/> is too long.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory containing <paramref name="filePath"/> does not exist.</exception>
    /// <exception cref="FileNotFoundException">The file at <paramref name="filePath"/> does not exist.</exception>
    /// <exception cref="IOException">There was an I/O issue reading the file at <paramref name="filePath"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Unable to access the file at <paramref name="filePath"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="filePath"/> is invalid.</exception>
    /// <exception cref="System.Security.SecurityException">Unable to access the file at <paramref name="filePath"/>.</exception>
    public static StaticSourceFile FromLocalizationFile(
        string filePath,
        IAssetSpecDatabase database,
        IAssetSourceFile asset,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Lazy)
    {
        return FromLocalizationFile(filePath, System.IO.File.ReadAllText(filePath, Encoding.UTF8).AsMemory(), database, asset, options);
    }

    /// <summary>
    /// Create a source file from an <paramref name="asset"/>'s localization file, given the already read file contents. File contents can be excluded to read the file directly.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="fileContents">Full text of the localization file to read from.</param>
    /// <param name="database">The information database to use to read the file.</param>
    /// <param name="asset">The asset this localization file belongs to.</param>
    /// <param name="options">Options to use when reading the file.</param>
    /// <exception cref="ArgumentNullException"/>
    public static StaticSourceFile FromLocalizationFile(
        string filePath,
        ReadOnlyMemory<char> fileContents,
        IAssetSpecDatabase database,
        IAssetSourceFile asset,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Lazy)
    {
        if (filePath == null)
            throw new ArgumentNullException(nameof(filePath));
        if (database == null)
            throw new ArgumentNullException(nameof(database));
        if (asset == null)
            throw new ArgumentNullException(nameof(asset));

        return new StaticSourceFile(filePath, database, fileContents, options, static (file, database, fileContents, options, state) =>
        {
            SourceNodeTokenizer.RootInfo info = SourceNodeTokenizer.RootInfo.Localization(file, database, (IAssetSourceFile)state!);
            using SourceNodeTokenizer tokenizer = new SourceNodeTokenizer(fileContents, options);
            return tokenizer.ReadRootDictionary(info);
        }, asset);
    }

    /// <inheritdoc cref="FromOtherFile(string,ReadOnlyMemory{char},IAssetSpecDatabase?,SourceNodeTokenizerOptions)"/>
    public static StaticSourceFile FromLocalizationFile(
        string filePath,
        string fileContents,
        IAssetSpecDatabase database,
        IAssetSourceFile asset,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Lazy)
    {
        return FromLocalizationFile(filePath, fileContents.AsMemory(), database, asset, options);
    }

    /// <summary>
    /// Create a source file from a generic unturned-dat formatted file.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="database">The information database to use to read the file.</param>
    /// <param name="options">Options to use when reading the file.</param>
    /// <exception cref="ArgumentException"><paramref name="filePath"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="PathTooLongException"><paramref name="filePath"/> is too long.</exception>
    /// <exception cref="DirectoryNotFoundException">The directory containing <paramref name="filePath"/> does not exist.</exception>
    /// <exception cref="FileNotFoundException">The file at <paramref name="filePath"/> does not exist.</exception>
    /// <exception cref="IOException">There was an I/O issue reading the file at <paramref name="filePath"/>.</exception>
    /// <exception cref="UnauthorizedAccessException">Unable to access the file at <paramref name="filePath"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="filePath"/> is invalid.</exception>
    /// <exception cref="System.Security.SecurityException">Unable to access the file at <paramref name="filePath"/>.</exception>
    public static StaticSourceFile FromOtherFile(
        string filePath,
        IAssetSpecDatabase? database,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Lazy)
    {
        return FromOtherFile(filePath, System.IO.File.ReadAllText(filePath, Encoding.UTF8).AsMemory(), database, options);
    }

    /// <summary>
    /// Create a source file from a generic unturned-dat formatted file, given the already read file contents. File contents can be excluded to read the file directly.
    /// </summary>
    /// <param name="filePath">Full path to the file.</param>
    /// <param name="fileContents">Full text of the file to read from.</param>
    /// <param name="database">The information database to use to read the file.</param>
    /// <param name="asset">The asset this localization file belongs to.</param>
    /// <param name="options">Options to use when reading the file.</param>
    /// <exception cref="ArgumentNullException"/>
    public static StaticSourceFile FromOtherFile(
        string filePath,
        ReadOnlyMemory<char> fileContents,
        IAssetSpecDatabase? database,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Lazy)
    {
        if (filePath == null)
            throw new ArgumentNullException(nameof(filePath));
        if (database == null)
            throw new ArgumentNullException(nameof(database));

        return new StaticSourceFile(filePath, database, fileContents, options, static (file, database, fileContents, options, _) =>
        {
            SourceNodeTokenizer.RootInfo info = SourceNodeTokenizer.RootInfo.Other(file, database);
            using SourceNodeTokenizer tokenizer = new SourceNodeTokenizer(fileContents, options);
            return tokenizer.ReadRootDictionary(info);
        }, null);
    }

    /// <inheritdoc cref="FromOtherFile(string,ReadOnlyMemory{char},IAssetSpecDatabase?,SourceNodeTokenizerOptions)"/>
    public static StaticSourceFile FromOtherFile(
        string filePath,
        string fileContents,
        IAssetSpecDatabase? database,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Lazy)
    {
        return FromOtherFile(filePath, fileContents.AsMemory(), database, options);
    }

    private StaticSourceFile(string filePath,
        IAssetSpecDatabase database,
        ReadOnlyMemory<char> fileContents,
        SourceNodeTokenizerOptions options,
        ReadFile creationCallback,
        object? state)
    {
        File = filePath;
        SourceFile = creationCallback(this, database, fileContents, options, state);
        _fileContent = fileContents;
    }

    private delegate ISourceFile ReadFile(
        StaticSourceFile file,
        IAssetSpecDatabase database,
        ReadOnlyMemory<char> fileContents,
        SourceNodeTokenizerOptions options,
        object? state);

    public string GetFullText()
    {
        if (_fileContentStr != null)
            return _fileContentStr;

        if (MemoryMarshal.TryGetString(_fileContent, out string text, out int start, out int length) && start == 0 && length == text.Length)
        {
            _fileContentStr = text;
            return text;
        }

        _fileContentStr = _fileContent.ToString();
        return _fileContentStr;
    }

    event Action<IWorkspaceFile, FileRange>? IWorkspaceFile.OnUpdated
    {
        add { }
        remove { }
    }

    public void Dispose()
    {
        Environment?.CloseFile(this);
    }
}

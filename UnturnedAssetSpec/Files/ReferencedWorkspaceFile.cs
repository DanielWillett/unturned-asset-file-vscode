using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.IO;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal class ReferencedWorkspaceFile : IWorkspaceFile
{
    private readonly string _fullText;

    public IAssetSpecDatabase Database { get; }

    /// <inheritdoc />
    public string File { get; }

    /// <inheritdoc />
    public ISourceFile SourceFile { get; }

    /// <inheritdoc />
    public string GetFullText()
    {
        return _fullText;
    }

    public ReferencedWorkspaceFile(string file, IAssetSpecDatabase database, object? state, string text, Func<ReferencedWorkspaceFile, object?, string, ISourceFile> factory)
    {
        Database = database;
        File = Path.GetFullPath(file);
        SourceFile = factory(this, state, text);
        _fullText = text;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (SourceFile is IDisposable disp)
            disp.Dispose();
    }
}

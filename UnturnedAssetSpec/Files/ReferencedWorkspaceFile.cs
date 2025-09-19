using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using System;
using System.IO;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal class ReferencedWorkspaceFile : IWorkspaceFile
{
    public IAssetSpecDatabase Database { get; }

    /// <inheritdoc />
    public string File { get; }

    /// <inheritdoc />
    public ISourceFile SourceFile { get; }

    public ReferencedWorkspaceFile(string file, IAssetSpecDatabase database, object? state, Func<ReferencedWorkspaceFile, object?, ISourceFile> factory)
    {
        Database = database;
        File = Path.GetFullPath(file);
        SourceFile = factory(this, state);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (SourceFile is IDisposable disp)
            disp.Dispose();
    }
}

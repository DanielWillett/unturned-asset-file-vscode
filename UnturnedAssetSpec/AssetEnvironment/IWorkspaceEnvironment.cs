using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;

public interface IWorkspaceEnvironment
{
    IWorkspaceFile? TemporarilyGetOrLoadFile(DiscoveredDatFile datFile);
}

public interface IWorkspaceFile : IDisposable
{
    /// <summary>
    /// Full path to the file.
    /// </summary>
    string File { get; }
    ISourceFile SourceFile { get; }
}
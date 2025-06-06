using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;

public interface IWorkspaceEnvironment
{
    IWorkspaceFile? TemporarilyGetOrLoadFile(DiscoveredDatFile datFile);
}

public interface IWorkspaceFile : IDisposable
{
    string AssetName { get; }
    AssetFileTree File { get; }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Completions;

public struct KeyCompletionState
{
    public ISourceNode? Node { get; }
    public SpecProperty Property { get; set; }
    public OpenedFile File { get; }
    public FilePosition Position { get; }
    public bool IsOnNewLine { get; }
    public InverseTypeHierarchy TypeHierarchy { get; }
    public string? Alias { get; set; }

    public KeyCompletionState(ISourceNode? node, FilePosition position, bool isOnNewLine, InverseTypeHierarchy typeHierarchy, string? alias, SpecProperty property, OpenedFile file)
    {
        Node = node;
        Position = position;
        IsOnNewLine = isOnNewLine;
        TypeHierarchy = typeHierarchy;
        Alias = alias;
        Property = property;
        File = file;
    }
}
using LspServer.Files;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LspServer.Completions;

public struct KeyCompletionState
{
    public AssetFileNode? Node { get; }
    public AssetSpecProperty Property { get; set; }
    public OpenedFile File { get; }
    public Position Position { get; }
    public bool IsOnNewLine { get; }
    public AssetSpec Spec { get; }
    public AssetInformation Information { get; }
    public string? Alias { get; set; }

    public KeyCompletionState(AssetFileNode? node, Position position, bool isOnNewLine, AssetSpec spec, AssetInformation information, string? alias, AssetSpecProperty property, OpenedFile file)
    {
        Node = node;
        Position = position;
        IsOnNewLine = isOnNewLine;
        Spec = spec;
        Information = information;
        Alias = alias;
        Property = property;
        File = file;
    }
}
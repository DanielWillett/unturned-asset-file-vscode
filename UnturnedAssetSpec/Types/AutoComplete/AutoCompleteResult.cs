using System;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;

public readonly struct AutoCompleteResult
{
    public static AutoCompleteResult[] None => Array.Empty<AutoCompleteResult>();
    internal static Task<AutoCompleteResult[]> NoneTask => Task.FromResult(None);

    public string Text { get; }
    public string? Description { get; }
    public bool Deprecated { get; init; }

    public AutoCompleteResult(string text, string? description = null)
    {
        Text = text;
        Description = description;
    }

    /// <inheritdoc />
    public override string ToString() => Text ?? string.Empty;
}
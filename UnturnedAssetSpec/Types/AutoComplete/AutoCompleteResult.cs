using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;

public readonly struct AutoCompleteResult
{
    public static AutoCompleteResult[] None => Array.Empty<AutoCompleteResult>();

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
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;

public interface IWorkspaceEnvironment
{
    IWorkspaceFile? TemporarilyGetOrLoadFile(string filePath);

    /// <summary>
    /// Attempts to get the difficulty of a file based on it's location.
    /// </summary>
    /// <remarks>Doesn't take <see cref="Comment.DifficultyAdditionalProperty"/> into account.</remarks>
    bool TryGetFileDifficulty(string file, out ServerDifficulty difficulty);
}

public interface IWorkspaceFile : IDisposable
{
    /// <summary>
    /// Full path to the file.
    /// </summary>
    string File { get; }
    ISourceFile SourceFile { get; }

    /// <summary>
    /// Gets the entire text of the file as a string in it's current state.
    /// </summary>
    string GetFullText();

    /// <summary>
    /// Invoked when this file's contents are updated.
    /// </summary>
    event Action<IWorkspaceFile, FileRange>? OnUpdated;
}

public delegate void SpanAction<TState>(ReadOnlySpan<char> span, ref TState state);
public delegate void SpanAction(ReadOnlySpan<char> span);

// ReSharper disable once TypeParameterCanBeVariant
public interface IMutableWorkspaceFile : IWorkspaceFile
{
    /// <summary>
    /// Object used to synchronize changes to the file.
    /// </summary>
    object SyncRoot { get; }

    /// <summary>
    /// The range including all text in the file.
    /// </summary>
    FileRange FullRange { get; }

    /// <summary>
    /// Find the line number of a character given it's index in the file.
    /// </summary>
    /// <param name="index">Character index (from 0).</param>
    /// <param name="clampCharacter">If character indices matching newline characters should be clamped to the last character in the line instead of returning an invalid value.</param>
    /// <param name="clampLine">If character indices outside the document should be clamped to the last lines.</param>
    FilePosition GetPosition(int index, bool clampCharacter = true, bool clampLine = false);

    /// <summary>
    /// Gets the number of characters on the given 1-indexed line.
    /// </summary>
    int GetLineLength(int lineNum, bool includeNewLine = false);

    /// <summary>
    /// Get the content of the entire line, or <see langword="null"/> if the line doesn't exist in the document.
    /// </summary>
    /// <param name="lineNum">Line number indexed from 1.</param>
    string? GetLine(int lineNum, bool includeNewLine = false);

    /// <summary>
    /// Apply incremental updates to this document.
    /// </summary>
    void UpdateText(Action<IMutableWorkspaceFileUpdater> fileUpdate);

    /// <summary>
    /// Apply incremental updates to this document, passing a state parameter.
    /// </summary>
    void UpdateText<TState>(TState state, Action<IMutableWorkspaceFileUpdater, TState> fileUpdate);

    /// <summary>
    /// Sets the full text of the document and updates any necessary caches.
    /// </summary>
    void SetFullText(ReadOnlySpan<char> text);

    /// <summary>
    /// Performs an operation on each line of the file, passing a state parameter.
    /// </summary>
    void ForEachLine<TState>(ref TState state, SpanAction<TState> lineAction);

    /// <summary>
    /// Performs an operation on each line of the file.
    /// </summary>
    void ForEachLine(SpanAction lineAction);

    /// <summary>
    /// Performs an operation on the full text of the file.
    /// </summary>
    void OperateOnFullSpan(SpanAction action);

    /// <summary>
    /// Performs an operation on the full text of the file, passing a state parameter.
    /// </summary>
    void OperateOnFullSpan<TState>(ref TState state, SpanAction<TState> action);
}

// ReSharper disable once TypeParameterCanBeVariant
public interface IMutableWorkspaceFileUpdater
{
    /// <summary>
    /// Inserts text at a specific position in the document.
    /// </summary>
    void InsertText(FilePosition position, ReadOnlySpan<char> text, string? annotationId = null);

    /// <summary>
    /// Inserts text at after specific character in the document.
    /// </summary>
    void InsertText(int charIndex, ReadOnlySpan<char> text, string? annotationId = null);

    /// <summary>
    /// Removes text between two positions in the document.
    /// </summary>
    void RemoveText(FileRange range, string? annotationId = null);

    /// <summary>
    /// Removes text between and including two specific characters in the document.
    /// </summary>
    void RemoveText(int startIndex, int endIndex, string? annotationId = null);

    /// <summary>
    /// Removes text between two positions in the document.
    /// </summary>
    void ReplaceText(FileRange range, ReadOnlySpan<char> text, string? annotationId = null);

    /// <summary>
    /// Removes text between two specific characters in the document.
    /// </summary>
    void ReplaceText(int startIndex, int endIndex, ReadOnlySpan<char> text, string? annotationId = null);

    /// <summary>
    /// Sets the full text of the document and updates any necessary caches.
    /// </summary>
    void SetFullText(ReadOnlySpan<char> text, string? annotationId = null);

    /// <summary>
    /// Adds a new annotation to be referenced by other functions.
    /// </summary>
    /// <param name="annotationId">Unique ID of the annotation.</param>
    /// <param name="label">A human-readable string describing the actual change.</param>
    /// <param name="description">A human-readable string which is rendered less prominent in the user interface.</param>
    /// <param name="needsConfirmation">A flag which indicates that user confirmation is needed before applying the change.</param>
    void AddAnnotation(string annotationId, string label, string? description = null, bool? needsConfirmation = null);
}
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Records changes to a file without actually applying them.
/// </summary>
public class MutableVirtualFile : IMutableWorkspaceFile, IMutableWorkspaceFileUpdater
{
    private readonly IMutableWorkspaceFile _parent;
    private readonly IFileUpdateListener _listener;
    public string File => _parent.File;
    public ISourceFile SourceFile => _parent.SourceFile;
    public object SyncRoot => _parent.SyncRoot;
    public FileRange FullRange => _parent.FullRange;

    public MutableVirtualFile(IMutableWorkspaceFile parent, IFileUpdateListener listener)
    {
        _parent = parent;
        _listener = listener;
    }

    public void Dispose()
    {
        
    }

    public void ForEachLine<TState>(ref TState state, SpanAction<TState> lineAction)
    {
        _parent.ForEachLine(ref state, lineAction);
    }

    public void ForEachLine(SpanAction lineAction)
    {
        _parent.ForEachLine(lineAction);
    }

    public string GetFullText()
    {
        return _parent.GetFullText();
    }

    /// <inheritdoc />
    public event Action<IWorkspaceFile, FileRange>? OnUpdated;

    public string? GetLine(int lineNum, bool includeNewLine = false)
    {
        return _parent.GetLine(lineNum, includeNewLine);
    }

    public int GetLineLength(int lineNum, bool includeNewLine = false)
    {
        return _parent.GetLineLength(lineNum, includeNewLine);
    }

    public FilePosition GetPosition(int index, bool clampCharacter = true, bool clampLine = false)
    {
        return _parent.GetPosition(index, clampCharacter, clampLine);
    }

    public void OperateOnFullSpan(SpanAction action)
    {
        _parent.OperateOnFullSpan(action);
    }

    public void OperateOnFullSpan<TState>(ref TState state, SpanAction<TState> action)
    {
        _parent.OperateOnFullSpan(ref state, action);
    }

    public void SetFullText(ReadOnlySpan<char> text)
    {
        _listener.RecordFullReplace(text, null);
        OnUpdated?.Invoke(this, FullRange);
    }

    public void UpdateText(Action<IMutableWorkspaceFileUpdater> fileUpdate)
    {
        lock (_parent.SyncRoot)
        {
            fileUpdate(this);
            OnUpdated?.Invoke(this, FullRange);
        }
    }

    public void UpdateText<TState>(TState state, Action<IMutableWorkspaceFileUpdater, TState> fileUpdate)
    {
        lock (_parent.SyncRoot)
        {
            fileUpdate(this, state);
            OnUpdated?.Invoke(this, FullRange);
        }
    }

    /// <inheritdoc />
    void IMutableWorkspaceFileUpdater.InsertText(FilePosition position, ReadOnlySpan<char> text, string? annotationId)
    {
        _listener.RecordInsert(position, text, annotationId);
    }

    /// <inheritdoc />
    void IMutableWorkspaceFileUpdater.InsertText(int charIndex, ReadOnlySpan<char> text, string? annotationId)
    {
        FilePosition position = _parent.GetPosition(charIndex, clampCharacter: true, clampLine: true);
        _listener.RecordInsert(position, text, annotationId);
    }

    /// <inheritdoc />
    void IMutableWorkspaceFileUpdater.RemoveText(FileRange range, string? annotationId)
    {
        _listener.RecordRemove(range, annotationId);
    }

    /// <inheritdoc />
    void IMutableWorkspaceFileUpdater.RemoveText(int startIndex, int endIndex, string? annotationId)
    {
        FilePosition start = _parent.GetPosition(startIndex, clampCharacter: true, clampLine: true);
        FilePosition end = _parent.GetPosition(endIndex, clampCharacter: true, clampLine: true);
        _listener.RecordRemove(new FileRange(start, end), annotationId);
    }

    /// <inheritdoc />
    void IMutableWorkspaceFileUpdater.ReplaceText(FileRange range, ReadOnlySpan<char> text, string? annotationId)
    {
        _listener.RecordReplace(range, text, annotationId);
    }

    /// <inheritdoc />
    void IMutableWorkspaceFileUpdater.ReplaceText(int startIndex, int endIndex, ReadOnlySpan<char> text, string? annotationId)
    {
        FilePosition start = _parent.GetPosition(startIndex, clampCharacter: true, clampLine: true);
        FilePosition end = _parent.GetPosition(endIndex, clampCharacter: true, clampLine: true);
        _listener.RecordReplace(new FileRange(start, end), text, annotationId);
    }

    /// <inheritdoc />
    void IMutableWorkspaceFileUpdater.SetFullText(ReadOnlySpan<char> text, string? annotationId)
    {
        _listener.RecordFullReplace(text, annotationId);
    }

    /// <inheritdoc />
    void IMutableWorkspaceFileUpdater.AddAnnotation(string annotationId, string label, string? description, bool? needsConfirmation)
    {
        _listener.RecordNewAnnotation(annotationId, label, description, needsConfirmation);
    }
}

public interface IFileUpdateListener
{
    void RecordInsert(FilePosition position, ReadOnlySpan<char> text, string? annotationId);
    void RecordRemove(FileRange range, string? annotationId);
    void RecordReplace(FileRange range, ReadOnlySpan<char> text, string? annotationId);
    void RecordFullReplace(ReadOnlySpan<char> text, string? annotationId);
    void RecordNewAnnotation(string annotationId, string label, string? description, bool? needsConfirmation);
}
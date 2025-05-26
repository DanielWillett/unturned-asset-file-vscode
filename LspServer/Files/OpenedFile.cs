using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

// ReSharper disable LocalizableElement

namespace DanielWillett.UnturnedDataFileLspServer.Files;

/// <summary>
/// Incrementally tracked text file.
/// </summary>
[DebuggerDisplay("{Uri} - {LineCount, nq} L, {_contentSegment.Count,nq} C")]
public class OpenedFile : IWorkspaceFile
{
    private readonly ILogger _logger;
    private AssetFileTree? _tree;

    internal object EditLock = new object();
    internal object UpdateLock = new object();

    private bool _hasChanged;
    private FileRange _changeRange;
    private FileRange _fullRangeBeforeChange;

    private char[] _content;
    private LineInfo[] _lines;
    private int _lineCount;

    private ArraySegment<char> _contentSegment;

    private bool _isFaulted;

    public string GetFullText()
    {
        lock (EditLock)
            return new string(_content, _contentSegment.Offset, _contentSegment.Count);
    }

    /// <summary>
    /// Number of lines in this file, including empty lines.
    /// </summary>
    public int LineCount => _lineCount;

    public AssetFileTree File
    {
        get
        {
            AssetFileTree? tree = _tree;
            if (tree != null)
                return tree;

            DatTokenizer tokenizer = new DatTokenizer(new ReadOnlySpan<char>(_content, _contentSegment.Offset, _contentSegment.Count));
            tree = AssetFileTree.Create(ref tokenizer);
            _tree = tree;
            return tree;
        }
    }

    [ExcludeFromCodeCoverage]
    public DocumentUri Uri { get; }

    public event Action<OpenedFile, FileRange>? OnUpdated;

    public FileRange FullRange
    {
        get
        {
            if (_lineCount == 0)
                return new FileRange(-1, -1, -1, -1);

            return new FileRange(1, 1, _lineCount, _lines[_lineCount - 1].ContentLength + 1);
        }
    }

#pragma warning disable CS8618, CS9264
    public OpenedFile(DocumentUri uri, ReadOnlySpan<char> text, ILogger logger)
    {
        _logger = logger;
        Uri = uri;

        SetFullText(text);
        IndexTextIntl();
    }
#pragma warning restore CS8618, CS9264

    /// <summary>
    /// Sets the full text of the document and updates any necessary caches.
    /// </summary>
    public void SetFullText(ReadOnlySpan<char> text)
    {
        lock (UpdateLock)
        {
            lock (EditLock)
            {
                _isFaulted = false;
                if (!SetFullTextIntl(text))
                    return;

                _tree = null;
                IndexTextIntl();
            }
            
            if (_lineCount > 0)
                BroadcastUpdate(FullRange);
        }

    }

    private bool SetFullTextIntl(ReadOnlySpan<char> text)
    {
        if (text.SequenceEqual(_contentSegment.AsSpan()))
        {
            return false;
        }

        char[] content = new char[text.Length + 16];
        text.CopyTo(content);

        _content = content;
        _contentSegment = new ArraySegment<char>(content, 0, text.Length);
        return true;
    }

    /// <summary>
    /// Apply incremental updates to this document.
    /// </summary>
    public void UpdateText<TState>(TState state, Action<OpenedFileUpdater, TState> fileUpdate)
    {
        lock (UpdateLock)
        {
            bool broadcast;
            FileRange range;

            lock (EditLock)
            {
                _hasChanged = false;
                OpenedFileUpdater file = new OpenedFileUpdater(this);
                fileUpdate(file, state);
                broadcast = _hasChanged;
                range = _changeRange;
            }

            if (broadcast)
                BroadcastUpdate(range);
        }
    }

    /// <summary>
    /// Apply incremental updates to this document.
    /// </summary>
    public void UpdateText(Action<OpenedFileUpdater> fileUpdate)
    {
        lock (UpdateLock)
        {
            bool broadcast;
            FileRange range;

            lock (EditLock)
            {
                _hasChanged = false;
                OpenedFileUpdater file = new OpenedFileUpdater(this);
                fileUpdate(file);
                broadcast = _hasChanged;
                range = _changeRange;
            }

            if (broadcast)
                BroadcastUpdate(range);
        }
    }

    private void BroadcastUpdate(FileRange range)
    {
        try
        {
            OnUpdated?.Invoke(this, range);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking OnUpdated for file \"{0}\".", Uri.ToUnencodedString());
        }
    }

    /// <summary>
    /// Get the content of the entire line, or <see langword="null"/> if the line doesn't exist in the document.
    /// </summary>
    /// <param name="lineNum">Line number indexed from 1.</param>
    public string? GetLine(int lineNum, bool includeNewLine = false)
    {
        lock (EditLock)
        {
            ArraySegment<char> segment = GetLineIntl(lineNum, includeNewLine);
            return segment.Array == null ? null : new string(segment.Array, segment.Offset, segment.Count);
        }
    }

    private ArraySegment<char> GetLineIntl(int lineNum, bool includeNewLine = false)
    {
        --lineNum;

        if (lineNum < 0 || lineNum >= _lineCount)
            return default;

        LineInfo line = _lines[lineNum];
        int length = includeNewLine ? Math.Abs(line.Length) : line.ContentLength;
        return new ArraySegment<char>(_content, line.StartIndex, length);
    }

    private int GetIndexIntl(FilePosition position, bool clamp)
    {
        int lineNum = position.Line - 1;

        if (!clamp)
        {
            if (position.Line < 0 || position.Line >= _lineCount)
                return -1;
        }
        else if (position.Line < 0)
            lineNum = 0;
        else if (position.Line >= _lineCount)
            lineNum = _lineCount - 1;

        LineInfo line = _lines[lineNum];
        int characterIndex = position.Character - 1;
        if (!clamp)
        {                             // shouldnt be >=, includes first newline but not second if present
            if (characterIndex < 0 || characterIndex > Math.Abs(line.Length))
                return -1;
        }
        else
        {
            if (characterIndex < 0)
                characterIndex = 0;
            else
            {
                if (characterIndex > line.ContentLength)
                    characterIndex = Math.Abs(line.Length);
            }
        }

        return line.StartIndex + characterIndex;
    }

    /// <summary>
    /// Find the line number of a character given it's index in the file.
    /// </summary>
    /// <param name="index">Character index (from 0).</param>
    /// <param name="clampCharacter">If character indices matching newline characters should be clamped to the last character in the line instead of returning an invalid value.</param>
    /// <param name="clampLine">If character indices outside the document should be clamped to the last lines.</param>
    public FilePosition GetPosition(int index, bool clampCharacter = true, bool clampLine = false)
    {
        lock (EditLock)
        {
            return GetPositionIntl(index, clampCharacter, clampLine);
        }
    }

    private FilePosition GetPositionIntl(int index, bool clampCharacter = true, bool clampLine = false)
    {
        // binary search
        int low = 0, high = _lineCount;
        if (high == 0)
            return new FilePosition(-1, -1);

        if (clampLine && index <= 0 || index == 0)
            return new FilePosition(1, 1);

        while (true)
        {
            int mid = (low + high) / 2;
            LineInfo line = _lines[mid];
            if (index < line.StartIndex)
            {
                high = mid - 1;
                if (low <= high)
                    continue;
                if (!clampLine)
                    break;
                mid = low;
            }
            else if (index >= line.StartIndex + Math.Abs(line.Length))
            {
                low = mid + 1;
                if (low <= high)
                    continue;
                if (!clampLine)
                    break;
                mid = high;
            }

            while (true)
            {
                int character = index - line.StartIndex;
                int lineContentLength = line.ContentLength;
                if (character > lineContentLength)
                {
                    if (clampCharacter)
                        character = lineContentLength;
                    else
                        return new FilePosition(-1, -1);
                }
                
                return new FilePosition(mid + 1, character + 1);
            }
        }

        return new FilePosition(-1, -1);
    }

    private int GetLineIndexIntl(int charIndex)
    {
        int low = 0, high = _lineCount;
        if (high == 0)
            return -1;

        if (charIndex == 0)
            return 0;


        while (low <= high)
        {
            int mid = (low + high) / 2;

            LineInfo line = _lines[mid];
            if (charIndex < line.StartIndex)
            {
                high = mid - 1;
                continue;
            }
            if (charIndex >= line.StartIndex + Math.Abs(line.Length))
            {
                low = mid + 1;
                continue;
            }

            return mid;
        }

        return -1;
    }

    [ExcludeFromCodeCoverage]
    internal void AssertFileHasValidIndex()
    {
        lock (EditLock)
        {
            LineInfo[] index = new ArraySegment<LineInfo>(_lines, 0, _lineCount).ToArray();
            IndexTextIntl();
            LineInfo[] newIndex = new ArraySegment<LineInfo>(_lines, 0, _lineCount).ToArray();

            if (index.Length != newIndex.Length)
                throw new Exception($"Expected {newIndex.Length} lines but was {index.Length}.");

            if (newIndex.Length == 0)
                throw new Exception("Expected at least one line.");

            for (int i = 0; i < index.Length; ++i)
            {
                LineInfo l0 = index[i], l1 = newIndex[i];
                if (l0.StartIndex != l1.StartIndex)
                    throw new Exception($"Line {i + 1} has wrong start index: {l0.StartIndex}, expected {l1.StartIndex}.");
                if (l0.Length != l1.Length)
                    throw new Exception($"Line {i + 1} has wrong length: {l0.Length}, expected {l1.Length}.");

                if (l1.Length is > -2 and <= 0)
                {
                    // last line may not have a newline
                    if (i != index.Length - 1 || l1.Length != 0)
                        throw new Exception($"Invalid length of line {i + 1}: {Math.Abs(l1.Length)} ({l1.Length})");
                }

                if (i > 0)
                {
                    int end = newIndex[i - 1].StartIndex + Math.Abs(newIndex[i - 1].Length);
                    if (end != l1.StartIndex)
                        throw new Exception($"Previous line {i} does not end at this line ({i + 1})'s start index. This line starts at {l1.StartIndex} but should start at {end}.");
                }
                else if (l1.StartIndex != 0)
                    throw new Exception("Line 1 should start at index 0.");
            }

            if (newIndex[^1].StartIndex + Math.Abs(newIndex[^1].Length) != _contentSegment.Count)
            {
                throw new Exception($"Last line ({newIndex.Length}) should end at the end of the file content.");
            }
        }
    }

    public void IndexTextIntl()
    {
        int lines = _content.AsSpan().Count('\n');

        LineInfo[] lineArray = GrowLines(lines + 1);
        _lineCount = 0;

        int lastLine = 0;
        int lineIndex = 0;
        int size = _contentSegment.Count;
        for (int i = 0; i < size; ++i)
        {
            if (_content[i] != '\n')
                continue;

            bool hasReset = i != 0 && _content[i - 1] == '\r';

            ref LineInfo line = ref lineArray[lineIndex];

            line.StartIndex = lastLine;
            line.Length = i - lastLine + 1;
            lastLine = i + 1;
            if (hasReset)
                line.Length = -line.Length;
            ++lineIndex;
        }

        if (lastLine != size - 1)
        {
            bool hasReset = size > 1 && _content[size - 1] == '\r';
            ref LineInfo line = ref lineArray[lineIndex];
            line.StartIndex = lastLine;
            line.Length = size - lastLine;
            if (hasReset)
                line.Length = -line.Length;
        }

        _lineCount = lineIndex + 1;
    }

    private LineInfo[] GrowLines(int lines)
    {
        if (_lines == null)
            _lines = new LineInfo[lines + 4];
        else if (_lines.Length < lines)
            _lines = new LineInfo[Math.Max(lines + 4, (int)Math.Ceiling(_lines.Length * 1.5))];

        return _lines;
    }

    void IDisposable.Dispose() { }

    [DebuggerDisplay("Start: {StartIndex,nq}, Length: {System.Math.Abs(Length),nq} (Has \\r: {Length < 0,nq})")]
    private struct LineInfo
    {
        public int StartIndex;

        /// <summary>
        /// Negative if has \r character.
        /// </summary>
        public int Length;

        
        public readonly int ContentLength
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Length < 0 ? -Length - 2 : Length - 1;
        }

        public void AddLength(int amt)
        {
            Length += amt * Math.Sign(Length);
        }
    }

    public readonly struct OpenedFileUpdater
    {
        private readonly OpenedFile _file;

        [ExcludeFromCodeCoverage]
        public OpenedFile File => _file;

        public OpenedFileUpdater(OpenedFile file)
        {
            _file = file;
            _file._fullRangeBeforeChange = file.FullRange;
        }

        /// <summary>
        /// Inserts text at a specific position in the document.
        /// </summary>
        public OpenedFileUpdater InsertText(FilePosition position, ReadOnlySpan<char> text)
        {
            int charIndex = _file.GetIndexIntl(position, clamp: true);
            return InsertText(charIndex, text);
        }

        /// <summary>
        /// Inserts text at after specific character in the document.
        /// </summary>
        public unsafe OpenedFileUpdater InsertText(int charIndex, ReadOnlySpan<char> text)
        {
            if (text.Length == 0)
            {
                return this;
            }

            GrowString(text.Length);

            int offset = _file._contentSegment.Offset, count = _file._contentSegment.Count;
            charIndex += offset;
            fixed (char* newText = text)
            fixed (char* ptr = _file._content)
            {
                int leftover = count - charIndex;
                if (leftover > 0)
                {
                    // NOTE: overlapping MemoryCopy call doesn't work properly on all frameworks (older versions of Mono/Unity)
                    // should be fine on .NET though

                    Buffer.MemoryCopy(ptr + charIndex, ptr + charIndex + text.Length, (_file._content.Length - charIndex) * sizeof(char), leftover * sizeof(char));
                }

                Buffer.MemoryCopy(newText, ptr + charIndex, text.Length * sizeof(char), text.Length * sizeof(char));
                IndexTextAddition(charIndex, text);
            }

            _file._contentSegment = new ArraySegment<char>(_file._content, offset, count + text.Length);
            return this;
        }

        /// <summary>
        /// Removes text between two positions in the document.
        /// </summary>
        public OpenedFileUpdater RemoveText(FileRange range)
        {
            if (range.Start == range.End)
                return this;

            int startIndex = _file.GetIndexIntl(range.Start, true);
            int endIndex = _file.GetIndexIntl(range.End, true);
            return RemoveText(startIndex, endIndex);
        }

        /// <summary>
        /// Removes text between and including two specific characters in the document.
        /// </summary>
        public unsafe OpenedFileUpdater RemoveText(int startIndex, int endIndex)
        {
            if (endIndex < startIndex)
                throw new InvalidOperationException("Expected start index to come before or be the same as the end index.");

            uint ct = (uint)(endIndex - startIndex);

            int offset = _file._contentSegment.Offset, count = _file._contentSegment.Count;
            startIndex += offset;
            fixed (char* ptr = _file._content)
            {
                int leftover = count - endIndex;
                if (leftover > 0)
                {
                    IndexTextRemoval(startIndex - offset, new ReadOnlySpan<char>(ptr + startIndex, (int)ct));
                    Buffer.MemoryCopy(ptr + startIndex + ct, ptr + startIndex, (_file._content.Length - startIndex) * sizeof(char), leftover * sizeof(char));
                    Unsafe.InitBlock(ptr + startIndex + leftover, 0, ct * sizeof(char));
                }
            }

            _file._contentSegment = new ArraySegment<char>(_file._content, offset, count - (int)ct);
            return this;
        }

        /// <summary>
        /// Removes text between two positions in the document.
        /// </summary>
        public OpenedFileUpdater ReplaceText(FileRange range, ReadOnlySpan<char> text)
        {
            if (range.Start == range.End)
            {
                return InsertText(range.Start, text);
            }

            int startIndex = _file.GetIndexIntl(range.Start, true);
            int endIndex = _file.GetIndexIntl(range.End, true);
            return ReplaceText(startIndex, endIndex, text);
        }

        /// <summary>
        /// Removes text between two specific characters in the document.
        /// </summary>
        public unsafe OpenedFileUpdater ReplaceText(int startIndex, int endIndex, ReadOnlySpan<char> text)
        {
            if (text.IsEmpty)
            {
                return RemoveText(startIndex, endIndex);
            }

            if (endIndex < startIndex)
                throw new InvalidOperationException("Expected start index to come before or be the same as the end index.");

            uint ct = (uint)(endIndex - startIndex);
            int currentSize = _file._contentSegment.Count;

            int newSize = currentSize - (int)ct + text.Length;
            if (newSize > currentSize)
            {
                GrowString(newSize - currentSize);
            }

            int offset = _file._contentSegment.Offset, count = _file._contentSegment.Count;
            startIndex += offset;
            fixed (char* newText = text)
            fixed (char* ptr = _file._content)
            {
                int moveToIndex = startIndex + text.Length;

                IndexTextReplacement(startIndex, new ReadOnlySpan<char>(ptr + startIndex, (int)ct), text);

                if (newSize != currentSize)
                {
                    int leftover = count - endIndex;
                    if (leftover > 0)
                    {
                        // NOTE: overlapping MemoryCopy call doesn't work properly on all frameworks (older versions of Mono/Unity)
                        // should be fine on .NET though
                        Buffer.MemoryCopy(ptr + startIndex + ct, ptr + moveToIndex, (_file._content.Length - moveToIndex) * sizeof(char), leftover * sizeof(char));

                        if (newSize < currentSize)
                            Unsafe.InitBlock(ptr + moveToIndex + leftover, 0, ct * sizeof(char));
                    }
                }

                Buffer.MemoryCopy(newText, ptr + startIndex, text.Length * sizeof(char), text.Length * sizeof(char));
            }

            _file._contentSegment = new ArraySegment<char>(_file._content, offset, newSize);
            return this;
        }

        private void GrowString(int dChar)
        {
            int oldSize = _file._contentSegment.Count;
            int fullLength = dChar + oldSize;

            if (_file._content.Length >= fullLength)
                return;

            char[] newCharacterArray = new char[Math.Max(fullLength + 16, (int)Math.Ceiling(oldSize * 1.5))];
            Buffer.BlockCopy(_file._content, _file._contentSegment.Offset * sizeof(char), newCharacterArray, 0, oldSize * sizeof(char));
            _file._content = newCharacterArray;
            _file._contentSegment = new ArraySegment<char>(newCharacterArray, 0, oldSize);
        }

        public OpenedFileUpdater SetFullText(ReadOnlySpan<char> text)
        {
            _file._changeRange = _file._fullRangeBeforeChange;
            _file.SetFullTextIntl(text);
            return this;
        }

        private void IndexTextRemoval(int index, ReadOnlySpan<char> text)
        {
            Console.WriteLine($"Removed \"{text.ToString().Replace("\r", "\\r").Replace("\n", "\\n")}\".");

            int firstNewLineIndex = text.IndexOf('\n');
            int lastNewLineIndex = firstNewLineIndex == -1 ? -1 : text.LastIndexOf('\n');

            int firstLine = _file.GetLineIndexIntl(index);
            if (firstLine == -1)
                _file.ThrowInvalidState();

            // edit consists of only a single line
            if (firstNewLineIndex == -1)
            {
                _file._lines[firstLine].AddLength(-text.Length);
                for (int i = firstLine + 1; i < _file._lineCount; ++i)
                    _file._lines[i].StartIndex -= text.Length;
                return;
            }

            //   from:
            // abcdef\r\n
            // abcd[xyz\r\n
            // pqrs\r\n
            // mnop]efgh\r\n

            //   to:
            // -> abcdef\r\n
            //    abcdefgh\r\n

            int linesToCut = firstNewLineIndex == lastNewLineIndex ? 1 : text.Count('\n');
            int lastLine = firstLine + linesToCut;

            ref LineInfo startPartialLine = ref _file._lines[firstLine];
            LineInfo endPartialLine = _file._lines[lastLine];

            int nextLineLength = Math.Abs(endPartialLine.Length) - (text.Length - lastNewLineIndex - 1) + Math.Abs(startPartialLine.Length) - firstNewLineIndex - 1;

            startPartialLine.Length = nextLineLength * Math.Sign(endPartialLine.Length);
            CutLines(firstLine + 1, linesToCut);
            for (int i = firstLine + 1; i < _file._lineCount; ++i)
                _file._lines[i].StartIndex -= text.Length;
        }
        
        private void IndexTextAddition(int index, ReadOnlySpan<char> text)
        {
            Console.WriteLine($"Added \"{text.ToString().Replace("\r", "\\r").Replace("\n", "\\n")}\".");

            int firstNewLineIndex = text.IndexOf('\n');
            int lastNewLineIndex = firstNewLineIndex == -1 ? -1 : text.LastIndexOf('\n');

            int firstLine = _file.GetLineIndexIntl(index);
            if (firstLine == -1)
                _file.ThrowInvalidState();

            // edit consists of only a single line
            if (firstNewLineIndex == -1)
            {
                _file._lines[firstLine].AddLength(text.Length);
                for (int i = firstLine + 1; i < _file._lineCount; ++i)
                    _file._lines[i].StartIndex += text.Length;
                return;
            }

            //   from:
            // abcdef\r\n
            // abcd|efgh\r\n

            //   to:
            // -> abcdef\r\n
            //    abcd[xyz\r\n
            //    pqrs\r\n
            //    mnop]efgh\r\n

            int linesToInsert = firstNewLineIndex == lastNewLineIndex ? 1 : text.Count('\n');
            _file.GrowLines(_file._lineCount + linesToInsert);

            ref LineInfo startPartialLine = ref _file._lines[firstLine];
            int originalNewLineSign = Math.Sign(startPartialLine.Length);

            ShiftDownLines(firstLine + 1, linesToInsert);

            int startLineLength = index - startPartialLine.StartIndex + firstNewLineIndex + 1;
            int endLineLength = text.Length - lastNewLineIndex - 1 + Math.Abs(startPartialLine.Length) - (index - startPartialLine.StartIndex);

            startPartialLine.Length = startLineLength;
            if (firstNewLineIndex > 0 && text[firstNewLineIndex - 1] == '\r')
                startPartialLine.Length = -startPartialLine.Length;

            int lastNewLine = firstNewLineIndex;
            for (int i = 1; i < linesToInsert; ++i)
            {
                ref LineInfo newLine = ref _file._lines[firstLine + i];

                int offset = lastNewLine + 1;
                int nextNewLine = i != linesToInsert - 1 ? text.Slice(offset).IndexOf('\n') + offset : lastNewLineIndex;
                if (nextNewLine == offset - 1)
                    _file.ThrowInvalidState();

                newLine.StartIndex = lastNewLine + 1 + index;
                newLine.Length = nextNewLine - lastNewLine;
                if (text[nextNewLine - 1] == '\r')
                    newLine.Length = -newLine.Length;

                lastNewLine = nextNewLine;
            }

            ref LineInfo lastLine = ref _file._lines[firstLine + linesToInsert];
            lastLine.StartIndex = lastNewLine + 1 + index;
            lastLine.Length = endLineLength * originalNewLineSign;

            for (int i = firstLine + linesToInsert + 1; i < _file._lineCount; ++i)
                _file._lines[i].StartIndex += text.Length;
        }

        private void IndexTextReplacement(int index, ReadOnlySpan<char> oldText, ReadOnlySpan<char> newText)
        {
            if (newText.Length != oldText.Length)
            {
                //PushIndices(index, newText.Length - oldText.Length);
            }

            Console.WriteLine($"Replaced \"{oldText.ToString().Replace("\r", "\\r").Replace("\n", "\\n")}\" -> \"{newText.ToString().Replace("\r", "\\r").Replace("\n", "\\n")}\".");
        }

        private void CutLines(int index, int ct)
        {
            if (index - ct != _file._lineCount)
            {
                Array.Copy(_file._lines, index + ct, _file._lines, index, _file._lineCount - index - ct);
            }
            _file._lineCount -= ct;
        }

        private void ShiftDownLines(int index, int by)
        {
            if (index != _file._lineCount)
            {
                Array.Copy(_file._lines, index, _file._lines, index + by, _file._lineCount - index);
            }
            _file._lineCount += by;
        }

    }

    private void ThrowInvalidState()
    {
        _isFaulted = true;
        SetFullTextIntl(_contentSegment);
        throw new InvalidOperationException("File is corrupted.");
    }
}
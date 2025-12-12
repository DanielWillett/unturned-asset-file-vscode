using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP2_1_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal class NodeWriteToTextWriterVisitor : OrderedNodeVisitor, IDisposable
{
    private int _indent;
    private char[]? _indentBuffer;
    private char[]? _buffer;
    private bool _hasNewLine;
    private bool _previousWasKey;

    public TextWriter Writer { get; }

    public bool UseSpacesAsTabs { get; init; } = true;
    public int SpacesPerTab { get; init; } = 4;

    protected override bool IgnoreMetadata { get; }

    public NodeWriteToTextWriterVisitor(TextWriter writer, bool ignoreMetadata = false)
    {
        Writer = writer;
        _hasNewLine = true;
        IgnoreMetadata = ignoreMetadata;
    }

    protected override void AcceptWhiteSpace(IWhiteSpaceSourceNode node)
    {
        if (IgnoreMetadata)
            return;

        _previousWasKey = false;
        if (!_hasNewLine)
            Writer.WriteLine();

        for (int i = node.Lines; i > 0; --i)
        {
            Writer.WriteLine();
        }

        _hasNewLine = true;
    }

    protected override void AcceptCommentOnly(ICommentSourceNode node)
    {
        if (IgnoreMetadata)
            return;

        _previousWasKey = false;
        foreach (Comment comment in node.Comments)
        {
            WriteIndent();
            WriteComment(in comment);
        }

        _hasNewLine = false;
    }

    protected override void AcceptValue(IValueSourceNode node)
    {
        if (_previousWasKey)
        {
            _previousWasKey = false;
            Writer.Write(' ');
        }
        else
        {
            WriteIndent();
        }

        _hasNewLine = false;

        ICommentSourceNode? commentNode;
        if (node.Parent is IPropertySourceNode prop)
        {
            commentNode = prop as ICommentSourceNode;
        }
        else
        {
            commentNode = Comment;
        }

        bool hasComment = !IgnoreMetadata && commentNode is { Comments.IsSingle: true };
        bool quote = node.IsQuoted || hasComment;

        if (quote)
            Writer.Write('"');

        WriteEscapableString(node.Value);

        if (quote)
            Writer.Write('"');

        if (!hasComment)
            return;

        Writer.Write(' ');

        if (!ReferenceEquals(commentNode, Comment))
            return;

        Comment comment = commentNode!.Comments[0];
        WriteComment(in comment);
    }

    protected override void AcceptProperty(IPropertySourceNode node)
    {
        WriteIndent();

        _hasNewLine = false;

        // the value will write the comment if there is one
        bool quote = node.KeyIsQuoted;

        if (quote)
            Writer.Write('"');

        WriteEscapableString(node.Key);

        if (quote)
            Writer.Write('"');

        _previousWasKey = true;
    }

    protected override void AcceptEndProperty(IPropertySourceNode property)
    {
        if (IgnoreMetadata || property is not ICommentSourceNode commentNode)
        {
            return;
        }

        foreach (Comment comment in commentNode.Comments)
        {
            if (comment.Position == CommentPosition.AfterOpeningBracket)
                continue;

            Writer.Write(' ');
            WriteComment(in comment);
        }
    }

    protected override void AcceptDictionary(IDictionarySourceNode node)
    {
        _previousWasKey = false;
        WriteIndent();

        ICommentSourceNode? commentNode = Comment ?? (node.Parent as IPropertySourceNode) as ICommentSourceNode;

        Writer.Write('{');
        if (!IgnoreMetadata && commentNode != null && !IgnoreMetadata)
        {
            foreach (Comment comment in commentNode.Comments)
            {
                if (comment.Position != CommentPosition.AfterOpeningBracket)
                    continue;

                Writer.Write(' ');
                WriteComment(in comment);
            }
        }
        ++_indent;

        _hasNewLine = false;
    }

    /// <inheritdoc />
    protected override void AcceptEndDictionary(IDictionarySourceNode dictionary)
    {
        _previousWasKey = false;
        if (_indent > 0)
            --_indent;

        WriteIndent();
        Writer.Write('}');

        _hasNewLine = false;

        if (IgnoreMetadata || Comment == null)
            return;

        foreach (Comment comment in Comment.Comments)
        {
            if (comment.Position == CommentPosition.AfterOpeningBracket)
                continue;

            Writer.Write(' ');
            WriteComment(in comment);
        }
    }

    protected override void AcceptList(IListSourceNode node)
    {
        _previousWasKey = false;
        WriteIndent();

        Writer.Write('[');
        if (!IgnoreMetadata && Comment != null)
        {
            foreach (Comment comment in Comment.Comments)
            {
                if (comment.Position != CommentPosition.AfterOpeningBracket)
                    continue;

                Writer.Write(' ');
                WriteComment(in comment);
            }
        }
        ++_indent;

        _hasNewLine = false;
    }

    /// <inheritdoc />
    protected override void AcceptEndList(IListSourceNode list)
    {
        _previousWasKey = false;
        if (_indent > 0)
            --_indent;

        WriteIndent();
        Writer.Write(']');

        _hasNewLine = false;

        if (IgnoreMetadata || Comment == null)
            return;

        foreach (Comment comment in Comment.Comments)
        {
            if (comment.Position == CommentPosition.AfterOpeningBracket)
                continue;

            Writer.Write(' ');
            WriteComment(in comment);
        }

    }

    private void WriteComment(in Comment comment)
    {
        int length = comment.Length;

        RentArray(ref _buffer, length);

        if (!comment.TryWrite(_buffer, out int charsWritten))
        {
            Writer.Write(comment.ToString());
        }
        else
        {
            Writer.Write(_buffer, 0, charsWritten);
        }
    }
    
    private void WriteEscapableString(string unescaped)
    {
        ReadOnlySpan<char> span = unescaped.AsSpan();
#if NET7_0_OR_GREATER
        ReadOnlySpan<char> escapables = [ '\\', '\n', '\t', '"' ];
#else
        ReadOnlySpan<char> escapables = stackalloc[] { '\\', '\n', '\t', '"' };
#endif
        int firstIndex = span.IndexOfAny(escapables);
        if (firstIndex < 0)
        {
            Writer.Write(unescaped);
            return;
        }

        char[] escapeBuffer = ArrayPool<char>.Shared.Rent(2);
        escapeBuffer[0] = '\\';
        int index = 0;
        while (index < unescaped.Length)
        {
            int i;
            if (index == 0)
            {
                i = firstIndex;
            }
            else
            {
                i = span.Slice(index).IndexOfAny(escapables);
                if (i < 0) break;
            }

            if (i > 0)
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                Writer.Write(span.Slice(index, i));
#else
                WriteMemory(unescaped.AsMemory(index, i));
#endif
            }
            i += index;
            index = i + 2;
            escapeBuffer[1] = span[i] switch
            {
                '\n' => 'n',
                '\t' => 't',
                _ => span[i]
            };
            Writer.Write(escapeBuffer, 0, 2);
        }

        ArrayPool<char>.Shared.Return(escapeBuffer);
        if (index < unescaped.Length)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            Writer.Write(span.Slice(index));
#else
            WriteMemory(unescaped.AsMemory(index));
#endif
        }
    }

    private void WriteMemory(ReadOnlyMemory<char> mem)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        Writer.Write(mem.Span);
#else
        if (MemoryMarshal.TryGetArray(mem, out ArraySegment<char> arr))
        {
            Writer.Write(arr.Array!, arr.Offset, arr.Count);
            return;
        }
        if (MemoryMarshal.TryGetString(mem, out string str, out int st, out int ln))
        {
            if (st == 0 && ln == str.Length)
            {
                Writer.Write(str);
                return;
            }
            if (Writer is StringWriter wr)
            {
                wr.GetStringBuilder().Append(str, st, ln);
                return;
            }
        }
        if (Writer is StringWriter sb)
        {
            unsafe
            {
                fixed (char* ptr = mem.Span)
                {
                    sb.GetStringBuilder().Append(ptr, mem.Length);
                }
            }
        }
        else if (mem.Length < 512)
        {
            char[] array = ArrayPool<char>.Shared.Rent(mem.Length);
            mem.CopyTo(array.AsMemory());
            Writer.Write(array, 0, mem.Length);
            ArrayPool<char>.Shared.Return(array);
        }
        else
        {
            char[] array = new char[mem.Length];
            mem.CopyTo(array.AsMemory());
            Writer.Write(array, 0, array.Length);
        }
#endif
    }

    private static readonly int LineEndLength = Environment.NewLine.Length;

    private static readonly int NewLineKind = Environment.NewLine switch
    {
        "\r\n" => 0,
        "\n" => 1,
        _ => 2
    };

    private void WriteIndent()
    {
        bool newLine = !_hasNewLine;
        _hasNewLine = true;

        if (_indent <= 0)
        {
            if (newLine)
                Writer.WriteLine();
            return;
        }

        int newLineOffset = newLine ? LineEndLength : 0;

        int size = newLineOffset + _indent * (UseSpacesAsTabs ? SpacesPerTab : 1);
        RentArray(ref _indentBuffer, size);
        if (newLine)
        {
            switch (NewLineKind)
            {
                case 0:
                    _indentBuffer[0] = '\r';
                    _indentBuffer[1] = '\n';
                    break;

                case 1:
                    _indentBuffer[0] = '\n';
                    break;

                default:
                    Environment.NewLine.CopyTo(0, _indentBuffer, 0, LineEndLength);
                    break;
            }
        }

        if (UseSpacesAsTabs)
        {
            for (int i = newLineOffset; i < size; ++i)
                _indentBuffer[i] = ' ';
        }
        else
        {
            for (int i = newLineOffset; i < size; ++i)
                _indentBuffer[i] = '\t';
        }

        Writer.Write(_indentBuffer, 0, size);
    }

    private static bool RentArray([NotNull] ref char[]? buffer, int length)
    {
        if (buffer == null)
        {
            buffer = ArrayPool<char>.Shared.Rent(length);
            return true;
        }
        if (buffer.Length < length)
        {
            ArrayPool<char>.Shared.Return(buffer);
            buffer = ArrayPool<char>.Shared.Rent(length);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _indentBuffer, null) is { } buffer)
        {
            ArrayPool<char>.Shared.Return(buffer);

        }
    }
}

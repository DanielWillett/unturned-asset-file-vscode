namespace LspServer.Files;

public readonly struct FileLineIndex
{
    private readonly int[] _indices;
    private readonly string _text;

    public bool IsValid => _indices != null;

    public FileLineIndex(string text)
    {
        _text = text;

        int newLineCount = 0;
        ReadOnlySpan<char> span = text;
        for (int i = 0; i < span.Length; ++i)
        {
            if (span[i] == '\n')
                ++newLineCount;
        }

        int[] indices = new int[newLineCount];
        newLineCount = -1;
        for (int i = 0; i < span.Length; ++i)
        {
            if (span[i] == '\n')
                indices[++newLineCount] = i + 1;
        }

        _indices = indices;
    }

    public ReadOnlySpan<char> SliceLine(int line, int startColumn = 1, int endColumn = -1)
    {
        return SliceLine(line, out _, out _, startColumn, endColumn);
    }

    public ReadOnlySpan<char> SliceLine(int line, out int startIndex, out int length, int startColumn = 1, int endColumn = -1)
    {
        int lineIndex = line - 2;
        if (lineIndex < -1 || lineIndex >= _indices.Length)
        {
            startIndex = 0;
            length = 0;
            return ReadOnlySpan<char>.Empty;
        }

        int lineStart = startColumn - 1;

        int lineEnd = (_indices.Length <= lineIndex + 1
            ? _text.Length
            : _indices[lineIndex + 1]) - 1;

        if (lineEnd > 0 && _text[lineEnd - 1] != '\r')
            ++lineEnd;

        if (lineIndex >= 0)
        {
            lineStart += _indices[lineIndex];
        }

        int lineLength = lineEnd - lineStart - 1;
        if (endColumn > 0)
        {
            endColumn -= startColumn - 1;
            lineLength = Math.Min(lineLength, endColumn);
        }

        if (lineLength <= 0 || lineStart + lineLength > _text.Length)
        {
            startIndex = 0;
            length = 0;
            return ReadOnlySpan<char>.Empty;
        }

        startIndex = lineStart;
        length = lineLength;
        return _text.AsSpan(lineStart, lineLength);
    }

    public int GetLineStart(int line)
    {
        int lineIndex = line - 2;
        if (lineIndex < 0)
            return 0;
        if (lineIndex >= _indices.Length)
            return -1;

        return _indices[lineIndex];
    }
}
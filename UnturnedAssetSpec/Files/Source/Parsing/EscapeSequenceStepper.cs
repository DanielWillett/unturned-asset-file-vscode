using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal ref struct EscapeSequenceStepper
{
    private readonly ReadOnlySpan<char> _span;
    private readonly ReadOnlySpan<char> _stops;

    private int _nextIndex;

    public int Index;
    public char Character;
    public bool IsTrailing;

    public EscapeSequenceStepper(ReadOnlySpan<char> span, ReadOnlySpan<char> stops)
    {
        _span = span;
        _stops = stops;
        Index = 0;
        _nextIndex = -1;
    }

    public void Break(out ReadOnlySpan<char> rawText)
    {
        Character = '\0';
        IsTrailing = false;
        rawText = ReadOnlySpan<char>.Empty;
    }

    public void Reset()
    {
        Index = 0;
        _nextIndex = -1;
        Character = '\0';
        IsTrailing = false;
    }

    public bool TryGetNextEscapeSequence(out ReadOnlySpan<char> rawText)
    {
        IsTrailing = false;
        Character = '\0';
        if (_nextIndex + 1 >= _span.Length)
        {
            rawText = _span[(_nextIndex + 1)..];
            Index = _span.Length;
            _nextIndex = Index - 1;
            IsTrailing = true;
            return false;
        }

        ReadOnlySpan<char> workingSpan = _span.Slice(_nextIndex + 1);
        int nextIndex = workingSpan.IndexOfAny(_stops);
        if (nextIndex == -1)
        {
            rawText = workingSpan;
            Index = _span.Length;
            _nextIndex = Index - 1;
            IsTrailing = true;
            return false;
        }

        rawText = workingSpan[..nextIndex];
        if (workingSpan[nextIndex] == '\\')
        {
            if (nextIndex >= workingSpan.Length - 1)
            {
                Index = _span.Length;
                _nextIndex = Index - 1;
                IsTrailing = true;
            }
            else
            {
                Index = _nextIndex + nextIndex + 2;
                _nextIndex = Index;
                Character = workingSpan[nextIndex + 1];
            }

            return true;
        }

        Index = (_nextIndex + 1) + nextIndex;
        _nextIndex = Index - 1;
        Character = _span[Index];
        return false;
    }
}

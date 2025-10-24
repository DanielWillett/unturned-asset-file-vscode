using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public sealed class TemplateProcessor
{
    public static readonly TemplateProcessor None = new TemplateProcessor();

    private readonly OneOrMore<ReadOnlyMemory<char>> _segments;
    private readonly bool _hasEndingTemplateGroup;
    private readonly int _totalSegmentLength;

    /// <summary>
    /// The number of indicies that can be substituted.
    /// </summary>
    public int TemplateCount { get; }

    /// <summary>
    /// Attempts to parse numbers from a key value.
    /// </summary>
    /// <exception cref="ArgumentException">Not enough space in <paramref name="output"/>.</exception>
    public bool TryParseKeyValues(ReadOnlySpan<char> key, Span<int> output, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (_segments.IsNull)
            return true;

        if (TemplateCount > output.Length)
            throw new ArgumentException($"Span not large enough: output. Expected at least {TemplateCount} template spots.");

        if (key.Length < _totalSegmentLength)
            return false;

        int workingKeyIndex = 0;
        int numberIndex = 0;
        for (int i = 0; i < _segments.Length; ++i)
        {
            ReadOnlySpan<char> segment = _segments[i].Span;
            ReadOnlySpan<char> segmentPortion = key.Slice(workingKeyIndex);
            if (!segmentPortion.StartsWith(segment, comparison))
                return false;
            
            workingKeyIndex += segment.Length;
            if (i == _segments.Length - 1 && !_hasEndingTemplateGroup)
                continue;

            if (workingKeyIndex >= key.Length)
                break;

            char nextSpanFirstCharacter = '\0';
            int segIndex = i + 1;
            while (segIndex < _segments.Length)
            {
                if (_segments[segIndex].IsEmpty)
                {
                    ++segIndex;
                    continue;
                }

                nextSpanFirstCharacter = _segments[segIndex].Span[0];
                break;
            }

            int endIndex = -1;
            if (nextSpanFirstCharacter != '\0')
            {
                endIndex = key.Slice(workingKeyIndex).IndexOf(nextSpanFirstCharacter);
            }

            if (endIndex == -1)
                endIndex = key.Length - workingKeyIndex;

            ReadOnlySpan<char> number = key.Slice(workingKeyIndex, endIndex);
            int numberValue;
            if (number.Length == 1 && number[0] is >= '0' and <= '9')
                numberValue = number[0] - '0';
            else
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                if (!int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out numberValue))
                    return false;
#else
                if (!int.TryParse(number.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out numberValue))
                    return false;
#endif
            }

            output[numberIndex] = numberValue;
            ++numberIndex;

            workingKeyIndex += number.Length;
        }

        return numberIndex == TemplateCount;
    }

    public static string EscapeKey(string key, TemplateProcessor processor)
    {
        ReadOnlySpan<char> stops = [ '\\', '*' ];

        OneOrMore<ReadOnlyMemory<char>> segments = processor._segments;

        int firstIndex = -1;
        if (segments.IsNull)
        {
            firstIndex = key.AsSpan().IndexOfAny(stops);
            if (firstIndex == -1)
            {
                return key;
            }

            segments = new OneOrMore<ReadOnlyMemory<char>>(key.AsMemory());
        }

        StringBuilder sb = new StringBuilder(key.Length + 2);
        for (int i = 0; i < segments.Length; ++i)
        {
            ReadOnlySpan<char> text = segments[i].Span;
            int index = firstIndex != -1 ? firstIndex : text.IndexOfAny(stops);
            firstIndex = -1;
            while (index != -1)
            {
                StringHelper.AppendSpan(sb, text.Slice(0, index));

                switch (text[index])
                {
                    case '*':
                        sb.Append(@"\*");
                        break;

                    case '\\':
                        if (index + 1 >= text.Length)
                        {
                            if (i == segments.Length - 1)
                                sb.Append('\\');
                            else
                                sb.Append(@"\\");
                        }
                        else if (text[index + 1] != '*')
                            sb.Append('\\');
                        else
                            sb.Append(@"\\");
                        break;
                }

                if (index >= text.Length - 1)
                {
                    text = ReadOnlySpan<char>.Empty;
                    break;
                }

                text = text.Slice(index + 1);

                index = text.IndexOfAny(stops);
            }

            StringHelper.AppendSpan(sb, text);

            if ((i != 0 || !processor._segments.IsNull) && (i != segments.Length - 1 || processor._hasEndingTemplateGroup))
            {
                sb.Append('*');
            }
        }

        return sb.ToString();
    }

    public static TemplateProcessor CreateForKey(ref string key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        ReadOnlySpan<char> span = key.AsSpan();

        ReadOnlySpan<char> stops = [ '\\', '*' ];
        
        int index = span.IndexOfAny(stops);

        if (index == -1)
            return None;

        OneOrMore<ReadOnlyMemory<char>> segments = OneOrMore<ReadOnlyMemory<char>>.Null;

        StringBuilder? sb = null;
        int lastStartPos = 0;
        int lastAppendPos = 0;
        int outputStringLength = 0;
        bool hasEnd = false;
        while (true)
        {
            bool isStar;
            if (span[index] == '*')
            {
                isStar = true;
            }
            else
            {
                sb ??= new StringBuilder();
                sb.Append(key, lastAppendPos, index - lastAppendPos);
                outputStringLength += index - lastAppendPos;
                int slashCt = 1;
                for (; index < span.Length - slashCt && span[index + slashCt] == '\\'; ++slashCt) ;

                sb.Append('\\', slashCt / 2);
                outputStringLength += (slashCt - 1) / 2 + 1;

                index += slashCt;
                if (index < span.Length)
                {
                    if (slashCt % 2 == 1)
                    {
                        // "...\*..."
                        char c = span[index];
                        if (c == '*')
                        {
                            sb.Append('*');
                            lastAppendPos = index + 1;
                        }
                        // "...\X.."
                        else
                        {
                            sb.Append('\\');
                            lastAppendPos = index;
                            --index;
                        }

                        isStar = false;
                    }
                    else
                    {
                        isStar = span[index] == '*';
                        lastAppendPos = index + (isStar ? 1 : 0);
                    }
                }
                else
                {
                    // "...\"
                    sb.Append('\\');
                    isStar = false;
                    lastAppendPos = span.Length;
                }
            }

            if (isStar)
            {
                if (index == key.Length - 1)
                    hasEnd = true;
                if (sb is { Length: > 0 })
                {
                    if (index > lastAppendPos)
                    {
                        sb.Append(key, lastAppendPos, index - lastAppendPos);
                        outputStringLength += index - lastAppendPos;
                    }
                }
                else
                {
                    outputStringLength += index - lastStartPos;
                }
                SinkSegment(ref segments, sb, key, lastStartPos, index);
                lastStartPos = lastAppendPos = index + 1;
            }

            int nextIndex = index + 1;
            if (nextIndex >= span.Length)
            {
                index = span.Length;
                break;
            }

            index = span.Slice(nextIndex).IndexOfAny(stops);
            if (index < 0)
            {
                index = span.Length;
                break;
            }

            index += nextIndex;
        }

        if (!hasEnd)
        {
            if (sb is { Length: > 0 })
            {
                if (index > lastAppendPos)
                {
                    sb.Append(key, lastAppendPos, index - lastAppendPos);
                    outputStringLength += index - lastAppendPos;
                }
            }
            else
            {
                outputStringLength += index - lastStartPos;
            }
            
            SinkSegment(ref segments, sb, key, lastStartPos, index);
        }

        int totalStringLength = outputStringLength + segments.Length - (!hasEnd ? 1 : 0);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        CreatePreviewKeyState state;
        state.Segments = segments;
        state.HasEnd = hasEnd;
        key = string.Create(totalStringLength, state, static (span, state) =>
        {
            int index = 0;
            OneOrMore<ReadOnlyMemory<char>> segments = state.Segments;
            for (int i = 0; i < segments.Length; ++i)
            {
                ReadOnlySpan<char> segment = segments[i].Span;
                segment.CopyTo(span.Slice(index));
                index += segment.Length;
                if (i == segments.Length - 1 && !state.HasEnd)
                    continue;

                span[index] = '#';
                ++index;
            }
        });
#else
        if (sb == null)
            sb = new StringBuilder(totalStringLength);
        else
        {
            sb.Clear();
            sb.EnsureCapacity(totalStringLength);
        }

        for (int i = 0; i < segments.Length; ++i)
        {
            StringHelper.AppendSpan(sb, segments[i].Span);
            if (i == segments.Length - 1 && !hasEnd)
                continue;
            sb.Append('#');
        }

        key = sb.ToString();
#endif

        return new TemplateProcessor(segments, outputStringLength, hasEnd);

        static void SinkSegment(ref OneOrMore<ReadOnlyMemory<char>> segments, StringBuilder? sb, string key, int lastStartPos, int index)
        {
            ReadOnlyMemory<char> segment;
            if (sb != null && sb.Length > 0)
            {
                segment = sb.ToString().AsMemory();
                sb.Clear();
            }
            else
            {
                segment = key.AsMemory(lastStartPos, index - lastStartPos);
            }

            segments = segments.Add(segment);
        }
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    private struct CreatePreviewKeyState
    {
        public OneOrMore<ReadOnlyMemory<char>> Segments;
        public bool HasEnd;
    }
#endif

    private TemplateProcessor()
    {
        _segments = OneOrMore<ReadOnlyMemory<char>>.Null;
    }

    private TemplateProcessor(OneOrMore<ReadOnlyMemory<char>> segments, int totalSegmentLength, bool hasEndingTemplateGroup)
    {
        _segments = segments;
        _totalSegmentLength = totalSegmentLength;
        _hasEndingTemplateGroup = hasEndingTemplateGroup;
        TemplateCount = segments.Length - (!hasEndingTemplateGroup ? 1 : 0);
    }

    public string CreateKey(string key, OneOrMore<int> indices)
    {
        return CreateKeyIntl(key, indices, ReadOnlySpan<int>.Empty, true);
    }

    public string CreateKey(string key, ReadOnlySpan<int> indices)
    {
        return CreateKeyIntl(key, OneOrMore<int>.Null, indices, false);
    }

    private string CreateKeyIntl(string key, OneOrMore<int> oneOrMore, ReadOnlySpan<int> span, bool useOneOrMore)
    {
        if (_segments.IsNull)
            return key;

        int inputLength = useOneOrMore ? oneOrMore.Length : span.Length;

        if (inputLength < TemplateCount)
            throw new ArgumentOutOfRangeException(nameof(oneOrMore));

        int totalDigitCount = 0;
        for (int i = 0; i < TemplateCount; ++i)
        {
            int num = useOneOrMore ? oneOrMore[i] : span[i];
            totalDigitCount += StringHelper.CountDigits(num);
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        unsafe
        {
            CreateKeyState state;
            if (!useOneOrMore)
            {
                state.Span = &span;
                state.OneOrMore = OneOrMore<int>.Null;
            }
            else
            {
                state.OneOrMore = oneOrMore;
                state.Span = null;
            }

            state.This = this;
            return string.Create(_totalSegmentLength + totalDigitCount, state, static (span, state) =>
            {
                bool useOneOrMore = state.Span == null;
                int index = 0;
                int numIndex = 0;

                OneOrMore<ReadOnlyMemory<char>> segments = state.This._segments;
                ReadOnlySpan<int> numSpan;
                if (useOneOrMore)
                    numSpan = ReadOnlySpan<int>.Empty;
                else
                    numSpan = *state.Span;

                for (int i = 0; i < segments.Length; ++i)
                {
                    ReadOnlySpan<char> segment = segments[i].Span;
                    segment.CopyTo(span.Slice(index));
                    index += segment.Length;

                    if (i == segments.Length - 1 && !state.This._hasEndingTemplateGroup)
                        continue;

                    int num = useOneOrMore ? state.OneOrMore[numIndex] : numSpan[numIndex];
                    ++numIndex;

                    num.TryFormat(span.Slice(index), out int charsWritten, "F0", CultureInfo.InvariantCulture);
                    index += charsWritten;
                }
            });
        }
#else
        int len = _totalSegmentLength + totalDigitCount;
        char[]? arr = null;
        scoped Span<char> allocSpan;
        if (len < 256)
        {
            allocSpan = stackalloc char[len];
        }
        else
        {
            allocSpan = arr = new char[len];
        }

        int index = 0;
        int numIndex = 0;

        for (int i = 0; i < _segments.Length; ++i)
        {
            ReadOnlySpan<char> segment = _segments[i].Span;
            segment.CopyTo(allocSpan.Slice(index));
            index += segment.Length;

            if (i == _segments.Length - 1 && !_hasEndingTemplateGroup)
                continue;

            int num = useOneOrMore ? oneOrMore[numIndex] : span[numIndex];
            ++numIndex;

            if (num is >= 0 and < 10)
            {
                allocSpan[index] = (char)(num + '0');
                ++index;
            }
            else
            {
                string toString = num.ToString("F0", CultureInfo.InvariantCulture);
                toString.AsSpan().CopyTo(allocSpan.Slice(index));
                index += toString.Length;
            }
        }

        return arr != null ? new string(arr, 0, index) : allocSpan.Slice(0, index).ToString();
#endif
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    private unsafe struct CreateKeyState
    {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
        public OneOrMore<int> OneOrMore;
        public ReadOnlySpan<int>* Span;
        public TemplateProcessor This;
#pragma warning restore CS8500
    }
#endif
}

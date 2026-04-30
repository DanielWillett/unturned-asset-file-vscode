using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Enumerate sections in a unity component's property path.
/// </summary>
internal struct UnityPropertyPathEnumerator
{
#if !NET7_0_OR_GREATER
    private static readonly char[] Stops = [ '.', '[', '\\' ];
#endif

    private readonly string _path;
    private int _index;
    
    /// <summary>
    /// The name of the property to read.
    /// </summary>
    public string Property { get; private set; }

    public Index? Index { get; private set; }

    public UnityPropertyPathEnumerator(string path)
    {
        _path = path;
        Reset();
    }

    [MemberNotNull(nameof(Property))]
    public void Reset()
    {
        _index = -1;
        Index = null;
        Property = string.Empty;
    }

    public bool MoveNext()
    {
        int beginIndex = _index + 1;
        bool prevWasEnd = true;
        while (true)
        {
            int startIndex = _index + 1;
            if (startIndex > _path.Length)
                return false;

#if NET7_0_OR_GREATER
            // ReSharper disable once InconsistentNaming (matches static readonly field above)
            ReadOnlySpan<char> Stops = [ '.', '[', '\\' ];
#endif

            int index = StringHelper.NextUnescapedIndexOf(_path.AsSpan(startIndex), Stops, out bool hadEscapeSequences);
            if (index < 0)
            {
                if (beginIndex >= _path.Length - 1)
                {
                    return false;
                }
                _index = _path.Length;
                ReadSegmentWithMaybeEsacpeSequences(hadEscapeSequences, beginIndex, _path.Length, Stops);
                Index = null;
                return true;
            }

            if (index == 0 && prevWasEnd)
            {
                beginIndex = startIndex + 1;
                _index = startIndex;
                continue;
            }

            index += startIndex;
            _index = index;

            if (_path[index] == '.')
            {
                ReadSegmentWithMaybeEsacpeSequences(hadEscapeSequences, beginIndex, index, Stops);
                Index = null;
                return true;
            }

            prevWasEnd = false;
            if (index + 2 >= _path.Length)
            {
                continue;
            }

            int indexerEndIndex = _path.IndexOf(']', index + 1);
            if (indexerEndIndex < 0)
            {
                continue;
            }

            ReadOnlySpan<char> indexer = _path.AsSpan(index + 1, indexerEndIndex - index - 1).Trim();
            bool isFromEnd = !indexer.IsEmpty && indexer[0] == '^';
            if (isFromEnd)
            {
                indexer = indexer[1..].TrimStart();
            }

            NumberStyles styles = NumberStyles.AllowThousands;
            if (indexer.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                indexer = indexer[2..];
                styles = NumberStyles.AllowHexSpecifier;
            }
#if NET9_0_OR_GREATER
            else if (indexer.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
            {
                indexer = indexer[2..];
                styles = NumberStyles.AllowBinarySpecifier;
            }
#endif

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            if (!int.TryParse(indexer, styles, CultureInfo.InvariantCulture, out int ind))
#else
            if (!int.TryParse(indexer.ToString(), styles, CultureInfo.InvariantCulture, out int ind))
#endif
            {
                continue;
            }
            
            Index = isFromEnd ? System.Index.FromEnd(ind) : System.Index.FromStart(ind);
            if (indexerEndIndex + 1 == _path.Length || _path[indexerEndIndex + 1] == '.')
                ++indexerEndIndex;
            _index = indexerEndIndex;
            ReadSegmentWithMaybeEsacpeSequences(hadEscapeSequences, beginIndex, index, Stops);
            return true;
        }
    }

    private void ReadSegmentWithMaybeEsacpeSequences(bool hadEscapeSequences, int start, int end, ReadOnlySpan<char> stops)
    {
        if (!hadEscapeSequences)
        {
            Property = _path.Substring(start, end - start);
            return;
        }

        Property = StringHelper.Unescape(_path.AsSpan(start, end - start), stops);
    }
}
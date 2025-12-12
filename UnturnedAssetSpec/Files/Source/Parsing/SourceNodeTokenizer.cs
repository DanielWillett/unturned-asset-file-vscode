using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

[Flags]
public enum SourceNodeTokenizerOptions
{
    None = 0,

    /// <summary>
    /// Parsed data includes comments and whitespace.
    /// </summary>
    Metadata     = 1 << 0,
    
    /// <summary>
    /// Property values are not resolved until they're first accessed.
    /// </summary>
    Lazy         = 1 << 1,
    
    /// <summary>
    /// Indicates that assets read with this tokenizer shouldn't attempt to load related localization files.
    /// </summary>
    SkipLocalizationInAssets = 1 << 2,

    Default = None
}

public ref partial struct SourceNodeTokenizer : IDisposable
{
    private readonly ReadOnlySpan<char> _file;
    private readonly ReadOnlyMemory<char> _fileMemory;
    private readonly SourceNodeTokenizerOptions _options;
    private readonly IDiagnosticSink? _diagnosticSink;

    private readonly FilePosition _positionOffset;
    private readonly int _indexOffset;
    private readonly int _depthOffset;

    private StringBuilder? _stringBuilder;

    private int _index;
    private int _readLastCharacterIndex;

    // 1-based, line end counts as one character (even if \r\n)
    private int _prevLineCharCount;
    private FilePosition _position;

    private char _char;
    private bool _skipRead;

    private ISourceNode?[]? _nodeList;
    private int _nodeListSize;

    /// <summary>
    /// Maximum depth level before skipping node reading.
    /// </summary>
    /// <remarks>Practically speaking this library can stop working properly after 63 levels of depth so no file should ever exceed that.</remarks>
    public int MaxDepth { get; init; } = 16;

    public readonly bool IsAtEnd => _index >= _file.Length;

    internal readonly char Character => IsAtEnd ? '\0' : _file[_index];

    public readonly FilePosition Position => _position;

    public readonly int Index => _index;

    public SourceNodeTokenizer(
        string text,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Default,
        IDiagnosticSink? diagnosticSink = null)
    : this(text.AsMemory(), options, new FilePosition(0, 0), 0, 0, diagnosticSink)
    {

    }

    public SourceNodeTokenizer(
        ReadOnlyMemory<char> text,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Default,
        FilePosition positionOffset = default,
        int indexOffset = 0,
        int depthOffset = 0,
        IDiagnosticSink? diagnosticSink = null)
    {
        _file = text.Span;
        _fileMemory = text;
        _options = options;
        _diagnosticSink = diagnosticSink;
        _position = FilePosition.One;
        _positionOffset = positionOffset;
        _indexOffset = indexOffset;
        _depthOffset = depthOffset;
    }
    
    public SourceNodeTokenizer(
        ReadOnlySpan<char> text,
        SourceNodeTokenizerOptions options = SourceNodeTokenizerOptions.Default,
        FilePosition positionOffset = default,
        int indexOffset = 0,
        int depthOffset = 0,
        IDiagnosticSink? diagnosticSink = null)
    {
        if ((options & SourceNodeTokenizerOptions.Lazy) != 0)
        {
            throw new ArgumentException(
                "The 'Lazy' option can not be used unless the ReadOnlyMemory or string constructor is used instead.",
                nameof(options)
            );
        }

        _file = text;
        _options = options;
        _diagnosticSink = diagnosticSink;
        _position = FilePosition.One;
        _positionOffset = positionOffset;
        _indexOffset = indexOffset;
        _depthOffset = depthOffset;
    }

    private void AddToNodeList(ISourceNode node)
    {
        if (_nodeList == null || _nodeListSize <= 0)
        {
            _nodeList ??= ArrayPool<ISourceNode>.Shared.Rent(4);
            _nodeList[0] = node;
            _nodeListSize = 1;
            return;
        }
    
        if (_nodeListSize >= _nodeList.Length)
        {
            ISourceNode?[] newStack = ArrayPool<ISourceNode>.Shared.Rent(_nodeListSize + 4);
            for (int i = 0; i < _nodeListSize; ++i)
            {
                newStack[i] = _nodeList[i];
                _nodeList[i] = null;
            }
    
            ArrayPool<ISourceNode?>.Shared.Return(_nodeList);
            _nodeList = newStack;
        }
    
        _nodeList[_nodeListSize] = node;
        ++_nodeListSize;
    }
    
    private void RemoveRangeFromNodeList(int i, int c)
    {
        if (_nodeListSize < i + c)
            throw new InvalidOperationException();

        if (c == 0)
            return;

        for (int j = 0; j < c; ++j)
        {
            int replIndex = j + i + c;
            if (replIndex < _nodeListSize)
            {
                _nodeList![j + i] = _nodeList[replIndex];
                _nodeList[replIndex] = null;
            }
            else
                _nodeList![j + i] = null;
        }

        _nodeListSize -= c;
    }

    private void Restart()
    {
        _index = -1;
        _position = new FilePosition(1, 0);
        _prevLineCharCount = 0;

        // skip UTF8 BOM if present
        if (_file.Length >= 3 && _file[0] == 'ï' && _file[1] == '»' && _file[2] == '¿')
        {
            _index = 2;
        }
    }

    private void SkipToNextToken()
    {
        _skipRead = false;
        if (_index < _file.Length)
        {
            AdvanceIfOnReset();
            SkipWhiteSpace();
            SkipOneNewLine();
        }
        
        _skipRead = _index < _file.Length;
    }
    
    private void SkipWhiteSpace()
    {
        while (_char != '\n' && char.IsWhiteSpace(_char))
        {
            if (!TryMoveNext())
                break;
        }
    }
    private void SkipOneNewLine()
    {
        if (_char == '\n')
        {
            TryMoveNext();
            SkipWhiteSpace();
        }
    }

    private void AdvanceIfOnReset()
    {
        // the current character should never be \r, this skips it and increments the necessary stuff
        int pendingChars = 0;
        while (_index < _file.Length)
        {
            char c = _file[_index];
            switch (c)
            {
                case '\r':
                    ++_index;
                    ++pendingChars;
                    break;

                default:
                    _position.Character += pendingChars;
                    _char = c;
                    return;

                case '\n':
                    _char = '\n';
                    return;
            }
        }
    }

    private bool TryReadComment(out Comment comment, CommentPosition position, ref FileRange range)
    {
        SkipWhiteSpace();
        if (_char != '/')
        {
            comment = default;
            return false;
        }

#if NET7_0_OR_GREATER
        ReadOnlySpan<char> ends = [ '\r', '\n' ];
#else
        ReadOnlySpan<char> ends = stackalloc[] { '\r', '\n' };
#endif

        int allCommentLength = _file.Slice(_index).IndexOfAny(ends);
        if (allCommentLength == -1)
            allCommentLength = _file.Length - _index;

        ReadOnlySpan<char> allComment = _file.Slice(_index, allCommentLength);
        int slashCt = 0;
        while (slashCt < allComment.Length && allComment[slashCt] == '/')
        {
            ++slashCt;
        }

        int spaceCt = slashCt;
        while (spaceCt < allComment.Length && char.IsWhiteSpace(allComment[spaceCt]))
        {
            ++spaceCt;
        }

        comment = new Comment(
            new CommentPrefix(slashCt, spaceCt - slashCt),
            spaceCt == allComment.Length ? string.Empty : allComment.Slice(spaceCt).ToString(),
            position
        );
        
        if (range.Start.IsInvalid)
        {
            range.Start = _position;
        }

        range.End = new FilePosition(_position.Line, _position.Character + allCommentLength - 1);

        _readLastCharacterIndex = _index + allCommentLength - 1;
        _index += allCommentLength;
        _char = _index >= _file.Length ? '\0' : _file[_index];
        _position.Character += allCommentLength;
        SkipToNextToken();
        return true;
    }

    private void SkipComment(out FileRange range)
    {
        SkipWhiteSpace();
        if (_char != '/')
        {
            range = default;
            return;
        }
        
#if NET7_0_OR_GREATER
        ReadOnlySpan<char> ends = [ '\r', '\n' ];
#else
        ReadOnlySpan<char> ends = stackalloc[] { '\r', '\n' };
#endif

        int allCommentLength = _file.Slice(_index).IndexOfAny(ends);
        if (allCommentLength == -1)
            allCommentLength = _file.Length - _index;

        range.Start = _position;
        range.End = new FilePosition(_position.Line, _position.Character + allCommentLength - 1);

        _readLastCharacterIndex = _index + allCommentLength - 1;
        _index += allCommentLength;
        _char = _index >= _file.Length ? '\0' : _file[_index];
        _position.Character += allCommentLength;
        SkipToNextToken();
    }

    private void GetAnyNodeProperties(int depth, out AnySourceNodeProperties props)
    {
        props = default;
        props.FirstCharacterIndex = _index + _indexOffset;
        props.Range.Start = new FilePosition(_position.Line + _positionOffset.Line, _position.Character + _positionOffset.Character);
        props.Depth = depth + _depthOffset;
        props.Index = -1;
    }

    /// <summary>
    /// Parses a full file as a root dictionary given the workspace file.
    /// </summary>
    public ISourceFile ReadRootDictionary(RootInfo rootInfo)
    {
        Restart();
        return (ISourceFile)ParseListOrDictionary(0, true, false, in rootInfo, -1, -1, out _);
    }

    public struct RootInfo
    {
        internal static readonly RootInfo None = new RootInfo(null!, null!, null!, (RootType)(-1));

        public readonly IAssetSourceFile LocalAsset;
        public readonly IWorkspaceFile File;
        public readonly IAssetSpecDatabase Database;
        public readonly RootType Type;

        public static RootInfo Localization(IWorkspaceFile file, IAssetSpecDatabase database, IAssetSourceFile asset)
        {
            return new RootInfo(asset, file, database, RootType.Localization);
        }
        
        public static RootInfo Asset(IWorkspaceFile file, IAssetSpecDatabase database)
        {
            return new RootInfo(null!, file, database, RootType.Asset);
        }
        
        public static RootInfo Other(IWorkspaceFile file, IAssetSpecDatabase database)
        {
            return new RootInfo(null!, file, database, RootType.Other);
        }
        
        private RootInfo(IAssetSourceFile asset, IWorkspaceFile file, IAssetSpecDatabase database, RootType type)
        {
            LocalAsset = asset;
            File = file;
            Database = database;
            Type = type;
        }
    }

    /// <summary>
    /// Parse a value from the full content. Whitespace is skipped in the root level no matter what the options.
    /// </summary>
    /// <param name="isListValue">Whether or not the value is in a list, otherwise the value is parsed as if it's a property value.</param>
    /// <returns>The value, or null if no value is present.</returns>
    public IAnyValueSourceNode? ParseValue(bool isListValue, int depth = 0, int index = -1, int childIndex = -1)
    {
        Restart();

        if (!TryMoveNext())
        {
            return null;
        }

        // skip white space and new lines, dont worry about metadata here
        while (char.IsWhiteSpace(_char))
        {
            if (!TryMoveNext())
                return null;
        }

        return ParseValueIntl(depth, isListValue, index, childIndex, out _);
    }

    private IAnyValueSourceNode ParseValueIntl(int depth, bool isListValue, int index, int childIndex, out OneOrMore<Comment> unhandledComments)
    {
        switch (_char)
        {
            case '{':
                return ParseListOrDictionary(depth, isListValue, isList: false, in RootInfo.None, index, childIndex, out unhandledComments);

            case '[':
                return ParseListOrDictionary(depth, isListValue, isList: true, in RootInfo.None, index, childIndex, out unhandledComments);

            case '"':
                GetAnyNodeProperties(0, out AnySourceNodeProperties props);
                string str = ReadQuotedString(out FileRange range, out _);
                int lastCharIndex = _readLastCharacterIndex;
                Comment comment;
                if ((_options & SourceNodeTokenizerOptions.Metadata) != 0)
                {
                    FileRange commentRange = range;
                    if (!TryReadComment(out comment, CommentPosition.EndOfLine, ref commentRange))
                        comment = Comment.None;

                    if (!isListValue)
                    {
                        unhandledComments = comment.Content == null ? OneOrMore<Comment>.Null : new OneOrMore<Comment>(comment);
                        comment = Comment.None;
                    }
                    else
                    {
                        unhandledComments = OneOrMore<Comment>.Null;
                    }
                }
                else
                {
                    SkipComment(out _);
                    unhandledComments = OneOrMore<Comment>.Null;
                    comment = Comment.None;
                }

                props.Index = index;
                props.ChildIndex = childIndex;
                props.LastCharacterIndex = lastCharIndex + _indexOffset;
                props.Range = range;
                TransformRange(ref props.Range);
                props.Depth = depth + _depthOffset;
                return ValueNode.Create(str, true, comment, in props);

            default:
                GetAnyNodeProperties(depth, out props);
                str = ReadNonQuotedString(out range, out _);
                props.Index = index;
                props.ChildIndex = childIndex;
                props.LastCharacterIndex = _readLastCharacterIndex + _indexOffset;
                props.Range = range;
                TransformRange(ref props.Range);
                props.Depth = depth + _depthOffset;
                unhandledComments = OneOrMore<Comment>.Null;
                return ValueNode.Create(str, false, Comment.None, in props);
        }
    }

    [SkipLocalsInit]
    private IAnyChildrenSourceNode ParseListOrDictionary(
        int depth, bool isListValue, bool isList, in RootInfo rootType, int index, int childIndex,
        out OneOrMore<Comment> unhandledComments)
    {
        bool isRoot = rootType.Type >= 0;
        if (isRoot)
        {
            // root dictionary is kinda treated like a list value since it's not part of a property
            isListValue = true;
            isList = false;
        }
        else if (_char != (isList ? '[' : '{'))
        {
            throw new InvalidOperationException("Not right node.");
        }

        // whitespace
        int ttlWhiteSpace = 0;
        FilePosition startWhiteSpacePos = default;
        int startWhiteSpaceIndex = -1;

        FileRange thisObjRange = default;
        thisObjRange.Start = _position;
        if (isRoot)
            ++thisObjRange.Start.Character;

        OneOrMore<KeyValuePair<string, object?>> additionalProperties = OneOrMore<KeyValuePair<string, object?>>.Null;

        int startNodeListIndex = _nodeListSize;
        int startIndex = _index;

        Comment beginComment = Comment.None;

        int nonMetaDataNodeCount = 0;

        OneOrMore<Comment> comments;
        Comment comment;
        AnySourceNodeProperties props;

        bool didClose = false;

        if (!isRoot)
        {
            _skipRead = false;
        }

        if (TryMoveNext())
        {
            SkipWhiteSpace();

            if (!isRoot)
            {
                SkipOneNewLine();
            }
            
            do
            {
                SkipWhiteSpace();

                char c = _char;

                // flush pending whitespace
                GetAnyNodeProperties(depth + 1, out props);
                if (c != '\n' && ttlWhiteSpace > 0)
                {
                    if ((_options & SourceNodeTokenizerOptions.Metadata) != 0)
                    {
                        props.ChildIndex = _nodeListSize - startNodeListIndex;
                        FilePosition previousPosition = new FilePosition(_position.Line - 1, _prevLineCharCount);
                        props.Range = new FileRange(
                            startWhiteSpacePos.Line + _positionOffset.Line,
                            startWhiteSpacePos.Character + _positionOffset.Character,
                            previousPosition.Line + _positionOffset.Line,
                            previousPosition.Character + _positionOffset.Character
                        );
                        props.FirstCharacterIndex = startWhiteSpaceIndex + _indexOffset;
                        props.LastCharacterIndex = _index + _indexOffset - 1;
                        AddToNodeList(WhitespaceNode.Create(ttlWhiteSpace, in props));

                        // reset node props
                        GetAnyNodeProperties(depth + 1, out props);
                    }

                    ttlWhiteSpace = 0;
                }

                if (!isRoot)
                {
                    if (isList)
                    {
                        if (c == ']')
                        {
                            didClose = true;
                            break;
                        }
                    }
                    else if (c == '}')
                    {
                        didClose = true;
                        break;
                    }
                }

                switch (c)
                {
                    case '/':
                        FilePosition st = _position;
                        bool couldBeAdditionalProp = isRoot && _position.Line == 1 + additionalProperties.Length;
                        if (!couldBeAdditionalProp && (_options & SourceNodeTokenizerOptions.Metadata) == 0)
                        {
                            SkipComment(out _);
                        }
                        else if (TryReadComment(
                                     out comment,
                                     !isRoot && _position.Line == thisObjRange.Start.Line
                                         ? CommentPosition.AfterOpeningBracket
                                         : CommentPosition.NewLine,
                                     ref thisObjRange)
                                )
                        {
                            props.ChildIndex = _nodeListSize - startNodeListIndex;

                            if (!isRoot && _position.Line == thisObjRange.Start.Line)
                                beginComment = comment;
                            else
                            {
                                if (couldBeAdditionalProp && comment.TryParseAsAdditionalProperty(out KeyValuePair<string, object?> prop))
                                {
                                    additionalProperties = additionalProperties.Add(prop);
                                }

                                if ((_options & SourceNodeTokenizerOptions.Metadata) != 0)
                                {
                                    props.Range = new FileRange(st, thisObjRange.End);
                                    TransformRange(ref props.Range);
                                    props.LastCharacterIndex = _readLastCharacterIndex + _indexOffset;
                                    AddToNodeList(CommentOnlyNode.Create(comment, in props));
                                }
                            }
                        }
                        break;

                    case '"':
                        int startLine = _position.Line;
                        string str = ReadQuotedString(out FileRange range, out _);
                        props.Index = nonMetaDataNodeCount;
                        props.ChildIndex = _nodeListSize - startNodeListIndex;
                        props.Range = range;
                        TransformRange(ref props.Range);
                        props.LastCharacterIndex = _readLastCharacterIndex + _indexOffset;
                        if (isList)
                        {
                            FileRange r = default;
                            TryReadComment(out comment, CommentPosition.EndOfLine, ref r);
                            AddToNodeList(ValueNode.Create(str, true, comment, in props));
                        }
                        else
                        {
                            LazySource source;
                            if (_char == '\n')
                                TrySkipToListOrDictStart();
                            if (startLine == _position.Line || _char is '[' or '{')
                            {
                                TryReadPropertyValue(depth + 1, out source, out comments);
                            }
                            else
                            {
                                source = default;
                                comments = OneOrMore<Comment>.Null;
                                // skipped to next line's key (no value)
                                _skipRead = true;
                            }
                            AddToNodeList(PropertyNode.Create(str, true, source, comments, in props));
                        }

                        ++nonMetaDataNodeCount;
                        break;
                    
                    case '\n':
                        if (ttlWhiteSpace == 0)
                        {
                            startWhiteSpacePos = _position;
                            startWhiteSpaceIndex = _index;
                            if (startWhiteSpaceIndex > 0 && _file[startWhiteSpaceIndex - 1] == '\r')
                            {
                                --startWhiteSpaceIndex;
                            }
                        }
                        ++ttlWhiteSpace;
                        break;

                    case '{':
                    case '[':
                        SkipComma();
                        if (!isList)
                        {
                            LogDiagnostic_ValueAndListOrDict();
                            break;
                        }

                        if (depth >= MaxDepth)
                        {
                            SkipToken(out FileRange skipRange);
                            LogDiagnostic_MaximumDepth(skipRange);
                        }
                        else
                        {
                            IAnyChildrenSourceNode dict = ParseListOrDictionary(
                                depth + 1,
                                true,
                                _char == '[',
                                in RootInfo.None,
                                nonMetaDataNodeCount,
                                _nodeListSize - startNodeListIndex,
                                out _
                            );
                            ++nonMetaDataNodeCount;
                            AddToNodeList(dict);
                        }

                        break;

                    default:
                        props.Index = nonMetaDataNodeCount;
                        props.ChildIndex = _nodeListSize - startNodeListIndex;
                        startLine = _position.Line;
                        str = ReadNonQuotedString(out range, out _, isKey: !isList);
                        props.Range = range;
                        TransformRange(ref props.Range);
                        props.LastCharacterIndex = _readLastCharacterIndex + _indexOffset;
                        if (isList)
                        {
                            AddToNodeList(ValueNode.Create(str, false, Comment.None, in props));
                        }
                        else
                        {
                            LazySource source;
                            if (_char == '\n')
                                TrySkipToListOrDictStart();
                            if (_char != '\0' && startLine == _position.Line || _char is '[' or '{')
                            {
                                TryReadPropertyValue(depth + 1, out source, out comments);
                            }
                            else
                            {
                                source = default;
                                comments = OneOrMore<Comment>.Null;
                                // skipped to next line's key (no value)
                                _skipRead = true;
                            }
                            AddToNodeList(PropertyNode.Create(str, false, source, comments, in props));
                        }
                        ++nonMetaDataNodeCount;
                        break;
                }
            }
            while (TryMoveNext());

        }

        if (!isRoot && !didClose)
        {
            LogDiagnostic_ListOrDictMissingClosingBracket(thisObjRange.Start, isList);
        }

        ISourceNode[] array;
        if (_nodeListSize == startNodeListIndex || _nodeListSize == 0)
            array = Array.Empty<ISourceNode>();
        else
            array = new ArraySegment<ISourceNode>(_nodeList!, startNodeListIndex, _nodeListSize - startNodeListIndex).ToArray();

        RemoveRangeFromNodeList(startNodeListIndex, _nodeListSize - startNodeListIndex);
        thisObjRange.End = didClose ? _position : new FilePosition(_position.Line - 1, _prevLineCharCount);

        comments = beginComment.Content == null
            ? OneOrMore<Comment>.Null
            : new OneOrMore<Comment>(beginComment);

        SkipWhiteSpace();
        if ((_options & SourceNodeTokenizerOptions.Metadata) != 0)
        {
            if (TryReadComment(out comment, CommentPosition.EndOfLine, ref thisObjRange))
            {
                comments = comments.Add(comment);
            }
        }
        else
        {
            SkipComment(out _);
        }

        if (!isRoot && !isListValue)
        {
            unhandledComments = comments;
            comments = OneOrMore<Comment>.Null;
        }
        else
        {
            unhandledComments = OneOrMore<Comment>.Null;
        }

        props = new AnySourceNodeProperties
        {
            Index = index,
            ChildIndex = childIndex,
            Range = thisObjRange,
            Depth = depth + _depthOffset,
            FirstCharacterIndex = startIndex + _indexOffset,
            LastCharacterIndex = _index + _indexOffset - (!didClose ? 1 : 0)
        };

        TransformRange(ref props.Range);

        IAnyChildrenSourceNode node = rootType.Type switch
        {
            RootType.Asset => (_options & SourceNodeTokenizerOptions.SkipLocalizationInAssets) != 0
                ? RootAssetNodeSkippedLocalization.Create(rootType.File, rootType.Database, nonMetaDataNodeCount, array, in props, additionalProperties)
                : RootAssetNode.Create(rootType.File, rootType.Database, nonMetaDataNodeCount, array, in props, additionalProperties),
            RootType.Localization => RootLocalizationNode.Create(rootType.File, rootType.LocalAsset, rootType.Database, nonMetaDataNodeCount, array, in props, additionalProperties),
            RootType.Other => RootDictionaryNode.Create(rootType.File, rootType.Database, nonMetaDataNodeCount, array, in props, additionalProperties),
            _ => isList ? ListNode.Create(nonMetaDataNodeCount, array, comments, in props)
                        : DictionaryNode.Create(nonMetaDataNodeCount, array, comments, in props)
        };

        _readLastCharacterIndex = _index;
        TryMoveNext();
        if (didClose)
            SkipComma();
        SkipToNextToken();
        return node;
    }

    // handles whitespace in between a key and dict/list value:
    /*
     *  Key
     *
     *  {
     *
     *  }
     */
    private void TrySkipToListOrDictStart()
    {
        int valueIndex = -1;
        int line = _position.Line + 1;
        int character = 1;
        int prevLineCharCount = _position.Character;
        for (int i = _index + 1; i < _file.Length; ++i)
        {
            char c = _file[i];
            if (c == '\n')
            {
                ++line;
                prevLineCharCount = character;
                character = 1;
                continue;
            }

            ++character;
            if (char.IsWhiteSpace(c))
            {
                continue;
            }

            if (c is not '[' and not ']')
                break;

            valueIndex = i;
        }

        if (valueIndex == -1)
            return;

        _position = new FilePosition(line, character);
        _index = valueIndex;
        _char = _file[valueIndex];
        _prevLineCharCount = prevLineCharCount;
    }

    private void TransformRange(ref FileRange range)
    {
        if (range.Start.Line == 1)
        {
            range.Start.Character += _positionOffset.Character;
        }

        if (range.End.Line == 1)
        {
            range.End.Character += _positionOffset.Character;
        }

        range.Start.Line += _positionOffset.Line;
        range.End.Line += _positionOffset.Line;
    }

    private void TryReadPropertyValue(
        int propertyDepth,
        out LazySource lazySource,
        out OneOrMore<Comment> comments)
    {
        lazySource = default;
        comments = OneOrMore<Comment>.Null;

        ValueTypeDataRefType type = _char switch
        {
            '[' => ValueTypeDataRefType.List,
            '{' => ValueTypeDataRefType.Dictionary,
            _ => ValueTypeDataRefType.Value
        };

        if ((_options & SourceNodeTokenizerOptions.Lazy) != 0)
        {
            SourceNodeTokenizer topCommentReader = this;

            int startIndex = _index;
            FilePosition position = _position;
            SkipToken(out _);

            comments = OneOrMore<Comment>.Null;
            FileRange r = default;
            if ((_options & SourceNodeTokenizerOptions.Metadata) != 0)
            {
                if (topCommentReader.TryMoveNext() && topCommentReader.Character != '\n')
                {
                    SkipWhiteSpace();
                    if (topCommentReader.TryReadComment(out Comment topComment, CommentPosition.AfterOpeningBracket, ref r))
                    {
                        r = default;
                        comments = comments.Add(topComment);
                    }
                }
            }

            long rangeOffset;
            if (_position.Line == 1)
            {
                rangeOffset = ((long)(position.Line + _positionOffset.Line - 1) << 32)
                              | (long)(position.Character + _positionOffset.Character - 1);
            }
            else
            {
                rangeOffset = ((long)(position.Line + _positionOffset.Line - 1) << 32)
                              | (long)(position.Character - 1);
            }

            lazySource = new LazySource(_fileMemory.Slice(startIndex, _readLastCharacterIndex - startIndex + 1), type)
            {
                PositionOffset = rangeOffset,
                Options = _options,
                IndexDepthOffset = ((long)(startIndex + _indexOffset) << 32)
                                   | (long)(propertyDepth + _depthOffset)
            };

            SkipWhiteSpace();
            if ((_options & SourceNodeTokenizerOptions.Metadata) != 0)
            {
                if (TryReadComment(out Comment bottomComment, CommentPosition.EndOfLine, ref r))
                {
                    comments = comments.Add(bottomComment);
                }
            }
            else
            {
                SkipComment(out _);
            }
        }
        else
        {
            IAnyValueSourceNode value = type switch
            {
                ValueTypeDataRefType.List       => ParseListOrDictionary(propertyDepth, false, true, in RootInfo.None, -1, -1, out comments),
                ValueTypeDataRefType.Dictionary => ParseListOrDictionary(propertyDepth, false, false, in RootInfo.None, -1, -1, out comments),
                _ => ParseValueIntl(propertyDepth, false, -1, -1, out comments)
            };

            lazySource = new LazySource(value, type);
        }
    }

    internal string ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan, bool isKey = false)
    {
        int firstChar = _index;
        if (firstChar > _file.Length)
            firstChar = _file.Length;
        
        ReadOnlySpan<char> stops = isKey ? [ '\r', '\n', '\\', ' ' ] : [ '\r', '\n', '\\' ];

        ReadOnlySpan<char> str = _file.Slice(firstChar);

        EscapeSequenceStepper escStepper = new EscapeSequenceStepper(str, stops);

        string outString;
        int readLength;
        if (!escStepper.TryGetNextEscapeSequence(out ReadOnlySpan<char> step))
        {
            switch (escStepper.Index)
            {
                case 0:
                    range = new FileRange(_position, _position);
                    rangeSpan = ReadOnlySpan<char>.Empty;
                    _readLastCharacterIndex = firstChar - 1;
                    // reading at '... |<empty string>...'
                    SkipToNextToken();
                    return string.Empty;

                case 1:
                    range = new FileRange(_position, _position);
                    rangeSpan = _file.Slice(firstChar, 1);
                    _readLastCharacterIndex = firstChar;
                    // reading at '... |V...'
                    _skipRead = false;
                    TryMoveNext();
                    SkipToNextToken();
                    return new string(_file[firstChar], 1);
            }
            if (!_fileMemory.IsEmpty
                && step.Length == _fileMemory.Length
                && MemoryMarshal.TryGetString(_fileMemory, out string originalString, out int st, out int ln) && st == 0 && ln == step.Length)
                outString = originalString;
            else
                outString = step.ToString();
            readLength = outString.Length;
        }
        else
        {
            _stringBuilder ??= new StringBuilder();

            do
            {
                StringHelper.AppendSpan(_stringBuilder, step);
                if (escStepper.IsTrailing || escStepper.Character is '\r' or '\n')
                {
                    LogDiagnostic_UnexpectedEscapeSequence(_position.Character + escStepper.Index - 1, escStepper.Character);
                    _stringBuilder.Append('\\');
                    escStepper.Break(out step);
                    --escStepper.Index;
                    break;
                }

                if (!TryGetEscapeCharacter(escStepper.Character, out char ctrl) || ctrl == '\"')
                {
                    _stringBuilder.Append('\\');
                    LogDiagnostic_UnexpectedEscapeSequence(_position.Character + escStepper.Index - 1, escStepper.Character);
                }

                _stringBuilder.Append(ctrl);
            }
            while (escStepper.TryGetNextEscapeSequence(out step));

            if (escStepper.Character is '\r' or '\n')
            {
                escStepper.IsTrailing = true;
            }

            StringHelper.AppendSpan(_stringBuilder, step);

            outString = _stringBuilder.ToString();
            _stringBuilder.Clear();
            readLength = escStepper.Index + 1;
        }

        if (_index + readLength > _file.Length)
        {
            readLength = _file.Length - _index;
        }
        rangeSpan = _file.Slice(_index, readLength);
        _readLastCharacterIndex = _index + readLength - 1;
        range = new FileRange(_position, new FilePosition(_position.Line, _position.Character + readLength - 1));
        _position.Character += readLength;
        _index += readLength;
        _char = _index >= _file.Length ? '\0' : _file[_index];
        // reading at '... Value|...'
        SkipToNextToken();
        return outString;
    }

    private void SkipNonQuotedString(out FileRange range, bool isKey = false)
    {
        int firstChar = _index;
        if (firstChar > _file.Length)
            firstChar = _file.Length;
        
        ReadOnlySpan<char> stops = isKey ? [ '\r', '\n', '\\', ' ' ] : [ '\r', '\n', '\\' ];

        ReadOnlySpan<char> str = _file.Slice(firstChar);

        EscapeSequenceStepper escStepper = new EscapeSequenceStepper(str, stops);

        int readLength;
        if (!escStepper.TryGetNextEscapeSequence(out ReadOnlySpan<char> step))
        {
            switch (escStepper.Index)
            {
                case 0:
                    range = new FileRange(_position, _position);
                    _readLastCharacterIndex = firstChar - 1;
                    // reading at '... |<empty string>...'
                    SkipToNextToken();
                    return;

                case 1:
                    range = new FileRange(_position, _position);
                    _readLastCharacterIndex = firstChar;
                    // reading at '... |V...'
                    _skipRead = false;
                    TryMoveNext();
                    SkipToNextToken();
                    return;
            }
            readLength = step.Length;
        }
        else
        {
            do
            {
                if (escStepper.IsTrailing || escStepper.Character is '\r' or '\n')
                {
                    LogDiagnostic_UnexpectedEscapeSequence(_position.Character + escStepper.Index - 1, escStepper.Character);
                    escStepper.Break(out step);
                    --escStepper.Index;
                    break;
                }

                if (!TryGetEscapeCharacter(escStepper.Character, out char ctrl) || ctrl == '\"')
                {
                    LogDiagnostic_UnexpectedEscapeSequence(_position.Character + escStepper.Index - 1, escStepper.Character);
                }
            }
            while (escStepper.TryGetNextEscapeSequence(out step));

            if (escStepper.Character is '\r' or '\n')
            {
                escStepper.IsTrailing = true;
            }

            readLength = escStepper.Index + 1;
        }

        if (_index + readLength > _file.Length)
        {
            readLength = _file.Length - _index;
        }

        _readLastCharacterIndex = _index + readLength - 1;
        range = new FileRange(_position, new FilePosition(_position.Line, _position.Character + readLength - 1));
        _position.Character += readLength;
        _index += readLength;
        _char = _index >= _file.Length ? '\0' : _file[_index];
        // reading at '... Value|[,]...'
        SkipToNextToken();
    }

    internal string ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan)
    {
        int firstChar = _index + 1;
        if (firstChar >= _file.Length)
        {
            // reading at '... |"'
            LogDiagnostic_StringQuotesNotTerminated(_position.Character);
            range = new FileRange(_position, _position);
            rangeSpan = _file.Slice(_index, 1);
            _readLastCharacterIndex = _index;
            _skipRead = false;
            TryMoveNext();
            SkipToNextToken();
            return string.Empty;
        }
        
        if (_file[firstChar] == '"')
        {
            // reading at '... |""...'
            range = new FileRange(_position, new FilePosition(_position.Line, _position.Character + 1));
            rangeSpan = _file.Slice(_index, 2);
            _readLastCharacterIndex = firstChar;
            _skipRead = false;
            TryMoveNext();
            // reading at '... "|"...'
            TryMoveNext();
            // reading at '... ""|...'
            SkipComma();
            SkipToNextToken();
            return string.Empty;
        }
#if NET7_0_OR_GREATER
        ReadOnlySpan<char> stops = [ '"', '\r', '\n', '\\' ];
#else
        ReadOnlySpan<char> stops = stackalloc[] { '"', '\r', '\n', '\\' };
#endif

        ReadOnlySpan<char> str = _file.Slice(firstChar);
        EscapeSequenceStepper escStepper = new EscapeSequenceStepper(str, stops);

        string? outString;
        int readLength;
        if (!escStepper.TryGetNextEscapeSequence(out ReadOnlySpan<char> step))
        {
            if (escStepper.IsTrailing || escStepper.Character is '\r' or '\n')
            {
                escStepper.IsTrailing = true;
                LogDiagnostic_StringQuotesNotTerminated(_position.Character + escStepper.Index);
            }

            outString = step.ToString();
            readLength = outString.Length;
        }
        else
        {
            _stringBuilder ??= new StringBuilder();

            do
            {
                StringHelper.AppendSpan(_stringBuilder, step);
                if (escStepper.IsTrailing || escStepper.Character is '\r' or '\n')
                {
                    LogDiagnostic_UnexpectedEscapeSequence(_position.Character + escStepper.Index, escStepper.Character);
                    _stringBuilder.Append('\\');
                    escStepper.Break(out step);
                    --escStepper.Index;
                    break;
                }

                if (!TryGetEscapeCharacter(escStepper.Character, out char ctrl))
                {
                    _stringBuilder.Append('\\');
                    LogDiagnostic_UnexpectedEscapeSequence(_position.Character + escStepper.Index, escStepper.Character);
                }

                _stringBuilder.Append(ctrl);
            }
            while (escStepper.TryGetNextEscapeSequence(out step));

            if (escStepper.IsTrailing || escStepper.Character is '\r' or '\n')
            {
                escStepper.IsTrailing = true;
                LogDiagnostic_StringQuotesNotTerminated(_position.Character + escStepper.Index);
            }

            StringHelper.AppendSpan(_stringBuilder, step);

            outString = _stringBuilder.ToString();
            _stringBuilder.Clear();
            readLength = escStepper.Index;
        }

        int len = readLength + 1 + (!escStepper.IsTrailing ? 1 : 0);
        if (_index + len >= _file.Length)
        {
            len = _file.Length - _index;
        }
        rangeSpan = _file.Slice(_index, len);
        _readLastCharacterIndex = _index + len - 1;
        range = new FileRange(_position, new FilePosition(_position.Line, _position.Character + len - 1));
        _position.Character += len;
        _index += len;
        _char = _index >= _file.Length ? '\0' : _file[_index];
        // reading at '... "Value"|[,]...'
        if (!escStepper.IsTrailing)
            SkipComma();
        SkipToNextToken();
        return outString;
    }

    private void SkipComma()
    {
        if (_char != ',')
            return;

        LogDiagnostic_UnnecessaryComma(_position);
        _skipRead = false;
        TryMoveNext();
    }

    private void SkipQuotedString(out FileRange skipRange)
    {
        int firstChar = _index + 1;
        if (firstChar >= _file.Length)
        {
            LogDiagnostic_StringQuotesNotTerminated(_position.Character);
            skipRange = new FileRange(_position, _position);
            _readLastCharacterIndex = _index;
            _skipRead = false;
            TryMoveNext();
            SkipToNextToken();
            return;
        }
        
        if (_file[firstChar] == '"')
        {
            skipRange = new FileRange(_position, new FilePosition(_position.Line, _position.Character + 1));
            _readLastCharacterIndex = firstChar;
            _skipRead = false;
            TryMoveNext();
            TryMoveNext();
            SkipComma();
            SkipToNextToken();
            return;
        }

#if NET7_0_OR_GREATER
        ReadOnlySpan<char> stops = [ '"', '\r', '\n', '\\' ];
#else
        ReadOnlySpan<char> stops = stackalloc[] { '"', '\r', '\n', '\\' };
#endif

        ReadOnlySpan<char> str = _file.Slice(firstChar);
        EscapeSequenceStepper escStepper = new EscapeSequenceStepper(str, stops);

        int readLength;
        if (!escStepper.TryGetNextEscapeSequence(out ReadOnlySpan<char> step))
        {
            if (escStepper.IsTrailing || escStepper.Character is '\r' or '\n')
            {
                escStepper.IsTrailing = true;
                LogDiagnostic_StringQuotesNotTerminated(_position.Character + escStepper.Index);
            }

            readLength = step.Length;
        }
        else
        {
            do
            {
                if (escStepper.IsTrailing || escStepper.Character is '\r' or '\n')
                {
                    LogDiagnostic_UnexpectedEscapeSequence(_position.Character + escStepper.Index, escStepper.Character);
                    escStepper.Break(out step);
                    --escStepper.Index;
                    break;
                }

                if (!TryGetEscapeCharacter(escStepper.Character, out _))
                {
                    LogDiagnostic_UnexpectedEscapeSequence(_position.Character + escStepper.Index, escStepper.Character);
                }
            }
            while (escStepper.TryGetNextEscapeSequence(out step));

            if (escStepper.IsTrailing || escStepper.Character is '\r' or '\n')
            {
                escStepper.IsTrailing = true;
                LogDiagnostic_StringQuotesNotTerminated(_position.Character + escStepper.Index);
            }

            readLength = escStepper.Index;
        }

        int len = readLength + 1 + (!escStepper.IsTrailing ? 1 : 0);
        if (_index + len >= _file.Length)
        {
            len = _file.Length - _index;
        }
        _readLastCharacterIndex = _index + len - 1;
        skipRange = new FileRange(_position, new FilePosition(_position.Line, _position.Character + len - 1));
        _position.Character += len;
        _index += len;
        _char = _index >= _file.Length ? '\0' : _file[_index];
        if (!escStepper.IsTrailing)
            SkipComma();
        SkipToNextToken();
    }

    private static bool TryGetEscapeCharacter(char c, out char ctrl)
    {
        switch (c)
        {
            case 't':
                ctrl = '\t';
                return true;
            case 'n':
                ctrl = '\n';
                return true;
            case '\\':
                ctrl = '\\';
                return true;
            case '"':
                ctrl = '"';
                return true;
            default:
                ctrl = c;
                return false;
        }
    }

    private bool TryMoveNext()
    {
        if (_skipRead)
        {
            _skipRead = false;
            return _index < _file.Length;
        }

        if (_char == '\n')
        {
            ++_position.Line;
            _prevLineCharCount = _position.Character;
            _position.Character = 0; // will be incremented
        }

        if (_index >= _file.Length - 1)
        {
            _char = '\0';
            _index = _file.Length;
            return false;
        }

        int pendingChars = 0;
        do
        {
            ++_index;
            _char = _file[_index];
            if (_char == '\r')
            {
                ++pendingChars;
            }
            else
            {
                if (_char != '\n' && pendingChars > 0)
                {
                    _position.Character += pendingChars + 1;
                }
                else
                {
                    ++_position.Character;
                }
                
                break;
            }
        } while (_index < _file.Length - 1);

        return _char is not ('\0' or '\r');
    }

    internal void SkipToken(out FileRange skipRange)
    {
        switch (_char)
        {
            case '/':
                SkipComment(out skipRange);
                break;
            
            case '\r':
            case '\n':
                SkipNewLine(out skipRange);
                break;

            case '[':
            case '{':
                skipRange = default;
                skipRange.Start = _position;
                SkipListOrDictionary(out skipRange.End);
                break;

            case '"':
                SkipQuotedString(out skipRange);
                break;

            default:
                SkipNonQuotedString(out skipRange);
                break;
        }
    }

    private void SkipNewLine(out FileRange skipRange)
    {
        switch (_char)
        {
            case '\r':
                skipRange.Start = _position;
                skipRange.End = _position;
                if (TryMoveNext() && _char == '\n')
                    TryMoveNext();
                break;

            case '\n':
                skipRange.Start = _position;
                skipRange.End = _position;
                TryMoveNext();
                break;

            default:
                skipRange = default;
                break;
        }
    }

    private void SkipListOrDictionary(out FilePosition endPos)
    {
        const bool list = true;
        const bool dictionary = false;

        bool isList = _char == '[';

        LightweightBitStack stack = new LightweightBitStack();

#if NET7_0_OR_GREATER
        ReadOnlySpan<char> tokens = [ '\r', '\n', '[', ']', '{', '}' ];
#else
        ReadOnlySpan<char> tokens = stackalloc[] { '\r', '\n', '[', ']', '{', '}' };
#endif

        int index = _index + 1;
        int lastNewLineIndex = _index - 1;

        while (lastNewLineIndex >= 0)
        {
            char lastChar = _file[lastNewLineIndex];
            if (lastChar == '\n')
                break;

            --lastNewLineIndex;
        }

        int line = _position.Line;

        while (true)
        {
            int tokenIndex = _file.Slice(index).IndexOfAny(tokens);
            if (tokenIndex == -1)
            {
                index = _file.Length - 1;
                break;
            }

            tokenIndex += index;

            index = tokenIndex + 1;
            char token = _file[tokenIndex];
            switch (token)
            {
                case '\n':
                    ++line;
                    goto case '\r';

                case '\r':
                    if (lastNewLineIndex != tokenIndex - 1 || _file[lastNewLineIndex] == '\n')
                        _prevLineCharCount = tokenIndex - lastNewLineIndex;

                    lastNewLineIndex = tokenIndex;
                    break;

                case '[':
                case '{':
                case ']':
                case '}':

                    if (lastNewLineIndex < 0)
                        break;

                    bool isOnNewLine = true;
                    for (int spaceInd = lastNewLineIndex + 1; spaceInd < tokenIndex; ++spaceInd)
                    {
                        if (char.IsWhiteSpace(_file[spaceInd]))
                            continue;

                        isOnNewLine = false;
                        break;
                    }

                    if (!isOnNewLine)
                        break;

                    switch (token)
                    {
                        case '[':
                            stack.Push(list);
                            break;

                        case '{':
                            stack.Push(dictionary);
                            break;

                        case ']':
                            if (stack.Count == 0)
                            {
                                if (isList)
                                {
                                    index = tokenIndex;
                                    goto finishedLoop;
                                }

                                LogDiagnostic_ListOrDictMissingClosingBracket(new FilePosition(line, tokenIndex - lastNewLineIndex), true);
                            }
                            else if (stack.Pop() != list)
                            {
                                stack.Push(dictionary);
                                LogDiagnostic_ListOrDictMissingClosingBracket(new FilePosition(line, tokenIndex - lastNewLineIndex), true);
                            }
                            break;

                        case '}':
                            if (stack.Count == 0)
                            {
                                if (!isList)
                                {
                                    index = tokenIndex;
                                    goto finishedLoop;
                                }

                                LogDiagnostic_ListOrDictMissingClosingBracket(new FilePosition(line, tokenIndex - lastNewLineIndex), false);
                            }
                            else if (stack.Pop() != dictionary)
                            {
                                stack.Push(list);
                                LogDiagnostic_ListOrDictMissingClosingBracket(new FilePosition(line, tokenIndex - lastNewLineIndex), false);
                            }
                            break;
                    }
                    break;
            }
        }

        finishedLoop:
        // index is endIndex

        endPos = new FilePosition(line, index - lastNewLineIndex);
        _readLastCharacterIndex = index;
        _index = index + 1;
        _position = endPos;
        _char = _index >= _file.Length ? '\0' : _file[_index];
        _prevLineCharCount = -1;
        SkipToNextToken();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_nodeList != null)
        {
            ArrayPool<ISourceNode?>.Shared.Return(_nodeList);
            _nodeList = null;
            _nodeListSize = 0;
        }
    }

    public enum RootType
    {
        Asset,
        Localization,
        Other
    }
}
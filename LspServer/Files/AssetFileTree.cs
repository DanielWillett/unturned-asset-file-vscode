using System.Collections;
using System.Globalization;
using System.Text;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SDG.Unturned;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace LspServer.Files;

public class AssetFileTree : IEnumerable<AssetFileNode>
{
    private bool _hasMetadata;
    private bool _hasAsset;
    private bool _hasCategory;
    private bool _hasType;
    private bool _typeCanBeAlias;
    private bool _hasId;
    private ushort? _id;
    private bool _hasGuid;
    private Guid? _guid;
    private string? _type;
    private EAssetType _category;
    private AssetFileDictionaryValueNode? _metadata;
    private AssetFileDictionaryValueNode? _asset;
    public AssetFileDictionaryValueNode Root { get; }
    public AssetFileDictionaryValueNode? Metadata
    {
        get
        {
            if (_hasMetadata)
                return _metadata;

            if (Root.TryGetValue("Metadata", out AssetFileValueNode? value)
                && value is AssetFileDictionaryValueNode dict)
            {
                _metadata = dict;
            }

            _hasMetadata = true;
            return _metadata;
        }
    }

    public AssetFileDictionaryValueNode Asset
    {
        get
        {
            if (_hasAsset)
                return _asset!;

            if (Root.TryGetValue("Asset", out AssetFileValueNode? value)
                && value is AssetFileDictionaryValueNode dict)
            {
                _asset = dict;
            }
            else
            {
                _asset = Root;
            }

            _hasAsset = true;
            return _asset;
        }
    }

    public AssetFileTree(AssetFileDictionaryValueNode root)
    {
        Root = root;
    }

    public AssetFileNode? GetNode(int index)
    {
        AssetFileNode? bestMatch = null;
        foreach (AssetFileNode node in this)
        {
            if (node.StartIndex >= index && node.EndIndex <= index)
            {
                if (bestMatch == null || bestMatch.StartIndex <= node.StartIndex)
                    bestMatch = node;
                continue;
            }
            if (bestMatch != null)
            {
                return bestMatch;
            }
        }

        return null;
    }

    public AssetFileNode? GetNode(Position position)
    {
        AssetFileNode? bestMatch = null;
        foreach (AssetFileNode node in this)
        {
            if (node.Range.Contains(position))
            {
                if (bestMatch == null || bestMatch.StartIndex <= node.StartIndex)
                    bestMatch = node;
                continue;
            }
            //if (bestMatch != null)
            //{
            //    return bestMatch;
            //}
        }

        return bestMatch;
    }

    public Guid? GetGuid()
    {
        if (_hasGuid)
        {
            return _guid;
        }

        if (Metadata?.TryGetValue("GUID", out AssetFileValueNode? value) is true && value is AssetFileStringValueNode guidNode)
        {
            _guid = Guid.TryParse(guidNode.Value, CultureInfo.InvariantCulture, out Guid guid) ? guid : null;
            _hasGuid = true;
            return _guid;
        }
        
        if (Root.TryGetValue("GUID", out value) && value is AssetFileStringValueNode guidNode2)
        {
            _guid = Guid.TryParse(guidNode2.Value, CultureInfo.InvariantCulture, out Guid guid) ? guid : null;
            _hasGuid = true;
            return _guid;
        }

        _guid = null;
        _hasGuid = true;
        return null;
    }

    public ushort? GetId()
    {
        if (_hasId)
        {
            return _id;
        }

        _id = Asset.TryGetValue("ID", out AssetFileValueNode? value)
               && value is AssetFileStringValueNode idNode
               && ushort.TryParse(idNode.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out ushort v)
               && v != 0
            ? v
            : null;
        _hasId = true;
        return _id;
    }

    public EAssetType GetCategory(AssetInformation assetInfo)
    {
        if (_hasCategory)
        {
            return _category;
        }

        string? type = GetType(out bool requireSystemType);
        if (type == null)
        {
            _category = EAssetType.NONE;
            _hasCategory = true;
            return EAssetType.NONE;
        }

        EAssetType category = EAssetType.NONE;
        if (type.Contains("SDG.Unturned.RedirectorAsset", StringComparison.OrdinalIgnoreCase))
        {
            if (Asset.TryGetValue("AssetCategory", out AssetFileValueNode? assetCategoryValue)
                && assetCategoryValue is AssetFileStringValueNode categoryStr
                && Enum.TryParse(categoryStr.Value, true, out category))
            {
                _category = category;
                _hasCategory = true;
                return category;
            }

            _category = EAssetType.NONE;
            _hasCategory = true;
            return EAssetType.NONE;
        }

        if (!requireSystemType && assetInfo.AssetAliases != null && assetInfo.AssetAliases.TryGetValue(type, out string? clrType))
            type = clrType;
        else
            type = AssetSpecDictionary.NormalizeAssemblyQualifiedName(type);

        assetInfo.AssetCategories?.TryGetValue(type, out category);
        _category = category;
        _hasCategory = true;
        return category;
    }

    public string? GetType(out bool requireSystemType)
    {
        if (_hasType)
        {
            requireSystemType = _typeCanBeAlias;
            return _type;
        }

        if (Metadata?.TryGetValue("Type", out AssetFileValueNode? value) is true && value is AssetFileStringValueNode typeNode)
        {
            requireSystemType = true;
            _typeCanBeAlias = true;
            _type = AssetSpecDictionary.NormalizeAssemblyQualifiedName(typeNode.Value);
            _hasType = true;
            return _type;
        }

        requireSystemType = false;
        _typeCanBeAlias = false;
        if (!Asset.TryGetValue("Type", out value) || value is not AssetFileStringValueNode typeStrNode)
        {
            _type = null;
            _hasType = true;
            return null;
        }

        _type = typeStrNode.Value.Contains('.')
            ? AssetSpecDictionary.NormalizeAssemblyQualifiedName(typeStrNode.Value)
            : typeStrNode.Value;
        _hasType = true;
        return _type;
    }

    public static AssetFileTree Create(string filePath)
    {
        DatTokenizer tokenizer = new DatTokenizer(File.ReadAllText(filePath, Encoding.UTF8));
        return Create(ref tokenizer);
    }

    public static AssetFileTree Create(ref DatTokenizer tokenizer)
    {
        AssetFileDictionaryValueNode node = ReadDictionary(ref tokenizer, true, null);
        return new AssetFileTree(node);
    }

    public void WriteTo(TextWriter stream)
    {
        int indent = 0;
        bool hasNewLine = true;
        Root.WriteTo(stream, ref indent, ref hasNewLine);
        stream.Flush();
    }

    private static AssetFileDictionaryValueNode ReadDictionary(ref DatTokenizer tokenizer, bool isRoot, AssetFileStringValueNode? value)
    {
        int start = tokenizer.Token.StartIndex;
        Position startPos = new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn);

        List<AssetFileKeyValuePairNode> nodes = new List<AssetFileKeyValuePairNode>();
        bool isClosed = isRoot;
        while (tokenizer.Token.Type == DatTokenType.Key || tokenizer.MoveNext())
        {
            if (!isRoot && tokenizer.Token.Type == DatTokenType.DictionaryEnd)
            {
                isClosed = true;
                break;
            }

            if (tokenizer.Token.Type != DatTokenType.Key)
            {
                if (!RecoverFromInvalidValue(ref tokenizer, true))
                    break;
            }

            nodes.Add(ReadKeyValue(ref tokenizer));
        }

        AssetFileDictionaryValueNode node = new AssetFileDictionaryValueNode
        {
            StartIndex = start,
            EndIndex = tokenizer.Token.StartIndex + tokenizer.Token.Length,
            IsClosed = isClosed,
            Range = new Range(startPos, new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + tokenizer.Token.Length)),
            Pairs = nodes,
            Value = value,
            IsRoot = isRoot
        };

        foreach (AssetFileKeyValuePairNode pair in nodes)
        {
            pair.Parent = node;
        }

        if (value != null)
            value.Parent = node;

        return node;
    }

    private static AssetFileListValueNode ReadList(ref DatTokenizer tokenizer, AssetFileStringValueNode? value)
    {
        int start = tokenizer.Token.StartIndex;
        Position startPos = new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn);

        List<AssetFileValueNode> nodes = new List<AssetFileValueNode>();
        bool isClosed = false;
        while (tokenizer.Token.Type == DatTokenType.Key || tokenizer.MoveNext())
        {
            if (tokenizer.Token.Type == DatTokenType.ListEnd)
            {
                isClosed = true;
                break;
            }

            if (tokenizer.Token.Type is not DatTokenType.ListValue and not DatTokenType.ListStart and not DatTokenType.DictionaryStart)
            {
                if (!RecoverFromInvalidValue(ref tokenizer, false))
                    break;
            }

            nodes.Add(ReadListValue(ref tokenizer));
        }

        AssetFileListValueNode node = new AssetFileListValueNode
        {
            StartIndex = start,
            EndIndex = tokenizer.Token.StartIndex + tokenizer.Token.Length,
            IsClosed = isClosed,
            Range = new Range(startPos, new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + tokenizer.Token.Length)),
            Elements = nodes,
            Value = value
        };

        foreach (AssetFileValueNode element in nodes)
        {
            element.Parent = node;
        }

        if (value != null)
            value.Parent = node;

        return node;
    }

    private static AssetFileValueNode ReadListValue(ref DatTokenizer tokenizer)
    {
        switch (tokenizer.Token.Type)
        {
            case DatTokenType.DictionaryStart:
                return ReadDictionary(ref tokenizer, false, null);
            
            case DatTokenType.ListStart:
                return ReadList(ref tokenizer, null);

            default:
                int start = tokenizer.Token.StartIndex;
                Position startPos = new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn);
                AssetFileStringValueNode value = new AssetFileStringValueNode
                {
                    StartIndex = tokenizer.Token.StartIndex,
                    Value = tokenizer.Token.Content.ToString(),
                    Range = new Range(
                        startPos,
                        new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + tokenizer.Token.Length)
                    ),
                    EndIndex = tokenizer.Token.StartIndex + tokenizer.Token.Length,
                    IsQuoted = tokenizer.Token.Quoted
                };
                return value;
        }
    }

    private static AssetFileKeyValuePairNode ReadKeyValue(ref DatTokenizer tokenizer)
    {
        int start = tokenizer.Token.StartIndex + (tokenizer.Token.Quoted ? -1 : 0);
        Position startPos = new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + (tokenizer.Token.Quoted ? -1 : 0));

        AssetFileKeyNode key = new AssetFileKeyNode
        {
            StartIndex = tokenizer.Token.StartIndex,
            EndIndex = tokenizer.Token.StartIndex + tokenizer.Token.Length,
            Value = tokenizer.Token.Content.ToString(),
            Range = new Range(
                tokenizer.Token.Quoted
                ? new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn)
                : startPos,
                new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + tokenizer.Token.Length)
            ),
            IsQuoted = tokenizer.Token.Quoted
        };

        AssetFileKeyValuePairNode? node;
        AssetFileStringValueNode? value = null;
        while (tokenizer.MoveNext())
        {
            switch (tokenizer.Token.Type)
            {
                case DatTokenType.Value:
                    value = new AssetFileStringValueNode
                    {
                        StartIndex = tokenizer.Token.StartIndex,
                        Value = tokenizer.Token.Content.ToString(),
                        Range = new Range(
                            new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn),
                            new Position(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + tokenizer.Token.Length)
                        ),
                        EndIndex = tokenizer.Token.StartIndex + tokenizer.Token.Length,
                        IsQuoted = tokenizer.Token.Quoted
                    };
                    break;

                case DatTokenType.DictionaryStart:
                    AssetFileDictionaryValueNode dictNode = ReadDictionary(ref tokenizer, false, value);
                    node = new AssetFileKeyValuePairNode
                    {
                        Key = key,
                        Value = dictNode,
                        StartIndex = start,
                        Range = new Range(startPos, dictNode.Range.End),
                        EndIndex = dictNode.EndIndex
                    };
                    key.Parent = node;
                    dictNode.Parent = node;
                    return node;

                case DatTokenType.ListStart:
                    AssetFileListValueNode listNode = ReadList(ref tokenizer, value);
                    node = new AssetFileKeyValuePairNode
                    {
                        Key = key,
                        Value = listNode,
                        StartIndex = start,
                        Range = new Range(startPos, listNode.Range.End),
                        EndIndex = listNode.EndIndex
                    };
                    key.Parent = node;
                    listNode.Parent = node;
                    return node;
            }

            node = new AssetFileKeyValuePairNode
            {
                Key = key,
                Value = value,
                StartIndex = start,
                Range = value == null
                    ? key.IsQuoted
                        ? new Range(startPos, new Position(key.Range.End.Line, key.Range.End.Character + 1))
                        : key.Range
                    : new Range(startPos, value.IsQuoted ? new Position(value.Range.End.Line, value.Range.End.Character + 1) : value.Range.End),
                EndIndex = value?.EndIndex ?? key.EndIndex
            };

            key.Parent = node;
            if (value != null)
                value.Parent = node;

            return node;
        }

        node = new AssetFileKeyValuePairNode
        {
            Key = key,
            Value = value,
            StartIndex = start,
            Range = key.IsQuoted ? new Range(startPos, new Position(key.Range.End.Line, key.Range.End.Character - 1)) : key.Range,
            EndIndex = key.EndIndex
        };
        key.Parent = node;
        if (value != null)
            value.Parent = node;
        return node;
    }

    private static bool RecoverFromInvalidValue(ref DatTokenizer tokenizer, bool isDictionary)
    {
        int dictLevel = 0;
        int listLevel = 0;
        do
        {
            if (dictLevel <= 0 && listLevel <= 0)
            {
                if (isDictionary && tokenizer.Token.Type == DatTokenType.Key)
                    return true;

                if (!isDictionary && tokenizer.Token.Type is DatTokenType.ListValue or DatTokenType.DictionaryStart or DatTokenType.ListStart)
                    return true;
            }

            switch (tokenizer.Token.Type)
            {
                case DatTokenType.DictionaryStart:
                    ++dictLevel;
                    break;
                case DatTokenType.ListStart:
                    ++listLevel;
                    break;
                case DatTokenType.DictionaryEnd:
                    --dictLevel;
                    break;
                case DatTokenType.ListEnd:
                    --listLevel;
                    break;
            }

        } while (tokenizer.MoveNext());

        return false;
    }

    public AssetFileTreeEnumerator GetEnumerator() => new AssetFileTreeEnumerator(Root);

    /// <inheritdoc />
    IEnumerator<AssetFileNode> IEnumerable<AssetFileNode>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
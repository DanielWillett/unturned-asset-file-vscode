using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

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
    private EnumSpecTypeValue _category;
    private AssetFileDictionaryValueNode? _asset;

    public AssetFileDictionaryValueNode Root { get; }
    public AssetFileDictionaryValueNode? Metadata
    {
        get
        {
            if (_hasMetadata)
                return field;

            if (Root.TryGetValue("Metadata", out AssetFileValueNode? value)
                && value is AssetFileDictionaryValueNode dict)
            {
                field = dict;
            }

            _hasMetadata = true;
            return field;
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

    public AssetFileNode? GetNode(FilePosition position)
    {
        AssetFileNode? bestMatch = null;
        foreach (AssetFileNode node in this)
        {
            if (node.Range.Contains(position))
            {
                if (bestMatch == null || bestMatch.StartIndex <= node.StartIndex)
                    bestMatch = node;
            }
            // todo: optimize
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
            _guid = Guid.TryParse(guidNode.Value, out Guid guid) ? guid : null;
            _hasGuid = true;
            return _guid;
        }
        
        if (Root.TryGetValue("GUID", out value) && value is AssetFileStringValueNode guidNode2)
        {
            _guid = Guid.TryParse(guidNode2.Value, out Guid guid) ? guid : null;
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

    public bool TryGetProperty<TValue>(string property, AssetSpecDatabase spec, out TValue? value, out bool isDefault, SpecPropertyContext context = SpecPropertyContext.Property)
        where TValue : IEquatable<TValue?>
    {
        AssetFileType type = AssetFileType.FromFile(this, spec);
        SpecProperty? prop = spec.FindPropertyInfo(property, type, context);
        value = default!;
        isDefault = false;
        if (prop == null)
            return false;

        if (!TryGetProperty(prop, out AssetFileKeyValuePairNode node))
        {
            IDefaultValue<TValue>? defaultValue = prop.DefaultValue?.As<TValue>();
            value = defaultValue == null ? default : defaultValue.GetDefaultValue(this, spec);
            isDefault = true;
            return true;
        }

        ISpecPropertyType<TValue>? propType = prop.Type?.As<TValue>();

        SpecPropertyTypeParseContext ctx = new SpecPropertyTypeParseContext
        {
            Database = spec,
            FileType = AssetFileType.FromFile(this, spec),
            BaseKey = property,
            Node = node.Value,
            Parent = node,
            File = this
        };

        return propType != null && propType.TryParseValue(in ctx, out value);
    }

    public bool TryGetProperty(SpecProperty property, out AssetFileKeyValuePairNode node)
    {
        bool hasValue;
        if (property.CanBeInMetadata)
        {
            if (Metadata != null || Asset == Root)
            {
                AssetFileDictionaryValueNode dict = Metadata ?? Root;

                if (!dict.TryGetValue(property, out node) && dict != Root)
                {
                    hasValue = Root.TryGetValue(property, out node);
                }
                else hasValue = true;
            }
            else if (property.Key != "GUID")
            {
                hasValue = Asset.TryGetValue(property, out node);
            }
            else
            {
                node = null!;
                hasValue = false;
            }
        }
        else
        {
            hasValue = Asset.TryGetValue(property, out node);
        }

        return hasValue;
    }

    public EnumSpecTypeValue GetCategory(AssetInformation assetInfo, AssetSpecDatabase spec)
    {
        if (_hasCategory)
        {
            return _category;
        }

        QualifiedType type = AssetFileType.FromFile(this, spec).Type;
        if (type.IsNull)
        {
            _category = AssetCategory.None;
            _hasCategory = true;
            return AssetCategory.None;
        }

        EnumSpecTypeValue category = AssetCategory.None;
        if (type.Equals("SDG.Unturned.RedirectorAsset, Assembly-CSharp"))
        {
            if (Asset.TryGetValue("AssetCategory", out AssetFileValueNode? assetCategoryValue)
                && assetCategoryValue is AssetFileStringValueNode categoryStr
                && AssetCategory.TryParse(categoryStr.Value, out category))
            {
                _category = category;
                _hasCategory = true;
                return category;
            }

            _category = AssetCategory.None;
            _hasCategory = true;
            return AssetCategory.None;
        }

        assetInfo.AssetCategories.TryGetValue(type, out string? str);
        AssetCategory.TryParse(str, out category);
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
            _type = QualifiedType.NormalizeType(typeNode.Value);
            _hasType = true;
            return _type;
        }

        requireSystemType = false;
        _typeCanBeAlias = false;
        if (!Asset.TryGetValue("Type", out value) || value is not AssetFileStringValueNode typeStrNode)
        {
            _type = null!;
            _hasType = true;
            return _type;
        }

        _type = typeStrNode.Value.IndexOf('.') != -1
            ? QualifiedType.NormalizeType(typeStrNode.Value)
            : typeStrNode.Value;
        _hasType = true;
        return _type;
    }

    public static AssetFileTree Create(string filePath)
    {
        DatTokenizer tokenizer = new DatTokenizer(File.ReadAllText(filePath, Encoding.UTF8).AsSpan());
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
        FilePosition startPos = new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn);

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
            Range = new FileRange(startPos, new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + tokenizer.Token.Length)),
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
        FilePosition startPos = new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn);

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
            Range = new FileRange(startPos, new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + tokenizer.Token.Length)),
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
                FilePosition startPos = new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn);
                AssetFileStringValueNode value = new AssetFileStringValueNode
                {
                    StartIndex = tokenizer.Token.StartIndex,
                    Value = tokenizer.Token.Content.ToString(),
                    Range = new FileRange(
                        startPos,
                        new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + tokenizer.Token.Length)
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
        FilePosition startPos = new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + (tokenizer.Token.Quoted ? -1 : 0));

        AssetFileKeyNode key = new AssetFileKeyNode
        {
            StartIndex = tokenizer.Token.StartIndex,
            EndIndex = tokenizer.Token.StartIndex + tokenizer.Token.Length,
            Value = tokenizer.Token.Content.ToString(),
            Range = new FileRange(
                tokenizer.Token.Quoted
                ? new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn)
                : startPos,
                new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + tokenizer.Token.Length)
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
                        Range = new FileRange(
                            new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn),
                            new FilePosition(tokenizer.Token.StartLine, tokenizer.Token.StartColumn + tokenizer.Token.Length)
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
                        Range = new FileRange(startPos, dictNode.Range.End),
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
                        Range = new FileRange(startPos, listNode.Range.End),
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
                        ? new FileRange(startPos, new FilePosition(key.Range.End.Line, key.Range.End.Character + 1))
                        : key.Range
                    : new FileRange(startPos, value.IsQuoted ? new FilePosition(value.Range.End.Line, value.Range.End.Character + 1) : value.Range.End),
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
            Range = key.IsQuoted ? new FileRange(startPos, new FilePosition(key.Range.End.Line, key.Range.End.Character - 1)) : key.Range,
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
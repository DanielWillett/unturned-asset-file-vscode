using System.Collections;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public struct AssetFileTreeEnumerator : IEnumerator<AssetFileNode>
{
    private readonly AssetFileNode _root;
    private AssetFileNode? _node;
    private int _step;

    public AssetFileNode Current => _node!;

    public AssetFileTreeEnumerator(AssetFileNode root)
    {
        _root = root;
    }

    /// <inheritdoc />
    public bool MoveNext()
    {
        if (_step == 0)
        {
            _node = _root;
            _step = 1;
            return true;
        }

        if (_node == null)
            return false;

        switch (_node)
        {
            case AssetFileKeyValuePairNode kvp:
                _node = kvp.Key;
                return true;

            case AssetFileKeyNode key:
                _node = ((AssetFileKeyValuePairNode)key.Parent!)?.Value;
                if (_node != null)
                    return true;

                _node = key;
                return StepUp(key);

            case AssetFileDictionaryValueNode dict:
                if (dict.Pairs.Count <= 0)
                {
                    return StepUp(dict);
                }

                _node = dict.Pairs[0];
                return true;

            case AssetFileListValueNode list:
                if (list.Elements.Count <= 0)
                {
                    return StepUp(list);
                }

                _node = list.Elements[0];
                return true;

            case AssetFileValueNode value:
                return StepUp(value);

        }

        _node = null;
        return false;
    }

    private bool StepUp(AssetFileNode value)
    {
        while (true)
        {
            switch (_node)
            {
                case AssetFileKeyNode keyNode:
                    if (keyNode.Parent == null)
                    {
                        _node = null;
                        return false;
                    }

                    value = keyNode.Parent;
                    _node = value.Parent;
                    if (_node == null)
                        return false;
                    continue;

                case AssetFileListValueNode list:
                    // empty list
                    if (value == list)
                    {
                        _node = value.Parent;
                        continue;
                    }

                    if (value is not AssetFileValueNode v)
                    {
                        _node = null;
                        return false;
                    }

                    int index = list.Elements.IndexOf(v);
                    if (index < 0)
                    {
                        _node = null;
                        return false;
                    }

                    ++index;
                    if (index < list.Elements.Count)
                    {
                        _node = list.Elements[index];
                        return true;
                    }

                    value = list;
                    _node = list.Parent;
                    continue;

                case AssetFileDictionaryValueNode dict:
                    // empty dictionary
                    if (value == dict)
                    {
                        _node = value.Parent;
                        continue;
                    }

                    if (value is not AssetFileKeyValuePairNode kvp)
                    {
                        _node = null;
                        return false;
                    }

                    index = dict.Pairs.IndexOf(kvp);
                    if (index < 0)
                    {
                        _node = null;
                        return false;
                    }

                    ++index;
                    if (index < dict.Pairs.Count)
                    {
                        _node = dict.Pairs[index];
                        return true;
                    }

                    value = dict;
                    _node = dict.Parent;
                    continue;

                case AssetFileKeyValuePairNode keyPair:
                    if (keyPair.Parent == null)
                    {
                        _node = null;
                        return false;
                    }

                    value = keyPair;
                    _node = keyPair.Parent;
                    continue;

                case AssetFileStringValueNode value2:
                    if (value2.Parent == null)
                    {
                        _node = null;
                        return false;
                    }

                    switch (value2.Parent)
                    {
                        case AssetFileKeyValuePairNode:
                            value = value2.Parent;
                            _node = value.Parent;
                            if (_node == null)
                                return false;
                            continue;

                        default:
                            value = value2.Parent;
                            _node = value;
                            break;
                    }

                    continue;

                case null:
                    return false;
            }

            break;
        }

        _node = null;
        return false;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _step = 0;
    }

    /// <inheritdoc />
    AssetFileNode IEnumerator<AssetFileNode>.Current => Current;

    /// <inheritdoc />
    object? IEnumerator.Current => Current;

    /// <inheritdoc />
    public void Dispose() { }
}

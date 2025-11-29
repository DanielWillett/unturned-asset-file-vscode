using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Utility for gathering relational data about a value node in a source file.
/// </summary>
/// <remarks>Use <see cref="FromNode"/> to describe a node.</remarks>
public readonly struct ValueNodeDescriptor
{
    private readonly byte _flags;
    private readonly IPropertySourceNode _prop;
    private readonly int[]? _indices;

    /// <summary>
    /// Whether or not the value's direct parent is a list.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Property))]
    [MemberNotNullWhen(true, nameof(ListProperty))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool IsListElement => (_flags & 1) != 0;

    /// <summary>
    /// Whether or not this node has a value, instead of just representing a property with no value.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value != null;

    /// <summary>
    /// The value being described.
    /// </summary>
    public IAnyValueSourceNode? Value { get; }

    /// <summary>
    /// The property containing <see cref="Value"/>. If value is in a list this will be null.
    /// </summary>
    public IPropertySourceNode? Property => !IsListElement ? _prop : null;

    /// <summary>
    /// The property of this value's parent list, or the next property up if the parent list is also in a list (and so on).
    /// </summary>
    public IPropertySourceNode? ListProperty => IsListElement ? _prop : null;

    /// <summary>
    /// The number of lists until <see cref="ListProperty"/> is reached. See the following examples:
    /// <code>
    /// Property
    /// [
    ///     "Value"         // depth = 1
    ///
    ///     [
    ///         "Value"     // depth = 2
    ///     ]
    /// ]
    ///
    /// Property "Value"    // depth = 0
    /// </code>
    /// </summary>
    /// <remarks>Depth will always be zero for property values.</remarks>
    public int ListDepth => (_flags >>> 2) & 63;

    /// <summary>
    /// Gets the index of this list based on a depth number, starting from zero.
    /// </summary>
    public int GetListIndexByDepth(int depth)
    {
        int ttlDepth = ListDepth;
        if (depth < 0 || depth >= ttlDepth)
            throw new ArgumentOutOfRangeException(nameof(depth), $"Depth out of range: [0, {ttlDepth}).");

        return _indices == null ? Value!.Index : _indices[depth];
    }

    private ValueNodeDescriptor(byte flags, IAnyValueSourceNode? value, IPropertySourceNode property, int[]? indices)
    {
        Value = value;
        _prop = property;
        _flags = flags;
        _indices = indices;
    }

    /// <summary>
    /// Creates a node descriptor from a value node.
    /// </summary>
    /// <exception cref="ArgumentException"/>
    public static ValueNodeDescriptor FromNode(ISourceNode node, bool lockTreeSync = true)
    {
        bool lockTaken = false;
        if (lockTreeSync)
        {
            Monitor.Enter(node.File.TreeSync, ref lockTaken);
        }

        try
        {
            if (node is not IAnyValueSourceNode value)
            {
                if (node is IPropertySourceNode property)
                {
                    return new ValueNodeDescriptor(0, property.Value, property, null);
                }

                throw new ArgumentException($"Invalid node type: {node.Type}.");
            }
            else
            {
                if (value.Parent is IPropertySourceNode property)
                {
                    return new ValueNodeDescriptor(0, value, property, null);
                }

                if (value.Parent is not IListSourceNode list)
                    throw new ArgumentException("Values should only be parented to lists and properties.");

                int depth = 1;

                // 63 is from the 6 bytes used for depth (2^6 - 1).
                property = null!;
                for (; depth <= 63; ++depth)
                {
                    if (list.Parent is IListSourceNode list2)
                    {
                        list = list2;
                    }
                    else if (list.Parent is IPropertySourceNode property2)
                    {
                        property = property2;
                        break;
                    }
                    else if (ReferenceEquals(list.Parent, list.File))
                    {
                        break;
                    }
                }

                if (property == null)
                {
                    throw new ArgumentException(
                        "Values should only be parented to lists and properties, but a list value didn't have a property owner or had an out of range depth."
                    );
                }

                int[]? indices = null;
                if (depth != 1)
                {
                    indices = new int[depth];
                    ISourceNode list2 = value;
                    for (int i = 0; i < depth; ++i)
                    {
                        indices[indices.Length - i - 1] = list2.Index;
                        list2 = list2.Parent;
                    }
                }

                return new ValueNodeDescriptor((byte)(1 | (depth << 2)), value, property, indices);
            }
        }
        finally
        {
            if (lockTaken)
                Monitor.Exit(node.File.TreeSync);
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (!IsListElement)
        {
            return HasValue ? $"{KeyToString(_prop)} = {_prop.Value}" : _prop.ToString();
        }

        int depth = ListDepth;
        switch (depth)
        {
            case 1:
                return $"{KeyToString(_prop)}[{GetListIndexByDepth(0).ToString(CultureInfo.InvariantCulture)}] = {Value}";

            default:
                StringBuilder sb = new StringBuilder(128);
                if (_prop.KeyIsQuoted)
                    sb.Append('"').Append(_prop.Key).Append('"');
                else
                    sb.Append(_prop.Key);
                for (int i = 0; i < depth; ++i)
                {
                    sb.Append('[').Append(GetListIndexByDepth(i)).Append(']');
                }

                sb.Append(" = ").Append(Value);
                return sb.ToString();
        }

        string KeyToString(IPropertySourceNode prop) => prop.KeyIsQuoted ? $"\"{prop.Key}\"" : prop.Key;
    }
}
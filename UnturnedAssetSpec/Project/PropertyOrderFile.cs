using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// A single orderfile, defining the order of properties in udat files.
/// </summary>
public class PropertyOrderFile(string fileName) : IPropertyOrderFile
{
    internal const int EmptyLine = -1;

    public string FileName { get; } = fileName;

    internal ImmutableDictionary<TypeKey, OrderedPropertyReference[]> Orders = ImmutableDictionary<TypeKey, OrderedPropertyReference[]>.Empty;

    /// <inheritdoc />
    public OrderedPropertyReference[] GetOrderForType(QualifiedType type, SpecPropertyContext context)
    {
        TypeKey tk;
        tk.TypeName = type.Normalized.Type;
        tk.IsLocalization = context switch
        {
            SpecPropertyContext.Property => false,
            SpecPropertyContext.Localization => true,
            _ => throw new ArgumentOutOfRangeException(nameof(context))
        };

        if (!Orders.TryGetValue(tk, out OrderedPropertyReference[] values))
        {
            return Array.Empty<OrderedPropertyReference>();
        }

        return values;
    }

    public static bool TryReadFromFile(
        IAssetSpecDatabase database,
        ISourceFile sourceFile,
        [NotNullWhen(true)] out PropertyOrderFile? orderfile,
        IPropertyOrderFile? parentOrderFile = null)
    {
        PropertyOrderFile pj = new PropertyOrderFile(sourceFile.WorkspaceFile.File);
        if (!pj.TryUpdateFromFile(database, sourceFile, parentOrderFile))
        {
            orderfile = null;
            return false;
        }

        orderfile = pj;
        return true;
    }

    internal struct TypeKey : IEquatable<TypeKey>
    {
        public string TypeName;
        public bool IsLocalization;

        public readonly SpecPropertyContext Context => IsLocalization ? SpecPropertyContext.Localization : SpecPropertyContext.Property;

        /// <inheritdoc />
        public readonly override string ToString()
        {
            return IsLocalization
                ? $"{PropertyReference.CreateContextSpecifier(SpecPropertyContext.Localization)}{TypeName}"
                : TypeName;
        }

        public readonly bool Equals(TypeKey other)
        {
            return string.Equals(TypeName, other.TypeName, StringComparison.OrdinalIgnoreCase)
                   && IsLocalization == other.IsLocalization;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is TypeKey other && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            int hc = StringComparer.OrdinalIgnoreCase.GetHashCode(TypeName);
            if (IsLocalization) hc = ~hc;
            return hc;
        }
    }

    public bool TryUpdateFromFile(
        IAssetSpecDatabase database,
        ISourceFile sourceFile,
        IPropertyOrderFile? parentOrderFile = null)
    {
        ReadState s;
        s.Output = ImmutableDictionary.CreateBuilder<TypeKey, OrderedPropertyReference[]>();
        s.Properties = new Dictionary<TypeKey, IPropertySourceNode>();
        s.Database = database;
        s.Processed = new HashSet<IPropertySourceNode>();
        s.BaseTypeStack = new Stack<DatTypeWithProperties>(8);
        s.ReadBuffer = new List<OrderedPropertyReference>(64);
        s.Parent = parentOrderFile;

        foreach (IPropertySourceNode property in sourceFile.Properties)
        {
            if (property.Value is not IListSourceNode)
            {
                GetLogger(ref s).LogWarning("Expected list value for property {0}.", property.Key);
                continue;
            }

            string key = property.Key;
            ReadOnlySpan<char> trimmedKey = key.AsSpan().Trim();
            TypeKey tk;
            if (trimmedKey.Length == key.Length)
            {
                if (!TryReadKey(key, key, out tk, ref s))
                    continue;
            }
            else
            {
                if (!TryReadKey(null, trimmedKey, out tk, ref s))
                    continue;
            }

            if (s.Properties.ContainsKey(tk))
            {
                GetLogger(ref s).LogWarning("Duplicate key: {0}.", tk.ToString());
                continue;
            }

            s.Properties.Add(tk, property);
        }

        foreach (KeyValuePair<TypeKey, IPropertySourceNode> pair in s.Properties)
        {
            TypeKey key = pair.Key;
            IPropertySourceNode property = pair.Value;
            if (s.Processed.Contains(property))
                continue;

            QualifiedType type = new QualifiedType(key.TypeName, true);

            if (!database.AllTypes.TryGetValue(type, out DatType? fileType)
                || fileType is not DatTypeWithProperties propType
                || key.IsLocalization && fileType is not IDatTypeWithLocalizationProperties)
            {
                GetLogger(ref s).LogWarning("Unknown type reference: {0}.", property.Key);
                continue;
            }

            ReadType(in key, property, propType, ref s);
        }

        Orders = s.Output.ToImmutable();
        return true;
    }

    private static bool TryReadKey(string? propertyKey, ReadOnlySpan<char> propertyKeySpan, out TypeKey typeKey, ref ReadState s, SpecPropertyContext defaultContext = SpecPropertyContext.Unspecified)
    {
        ReadOnlySpan<char> sanitized = PropertyReference.TryRemoveContext(propertyKeySpan, out SpecPropertyContext context);

        QualifiedType type;
        if (propertyKeySpan.Length == sanitized.Length)
        {
            type = new QualifiedType(propertyKey ?? propertyKeySpan.ToString(), true);
            context = defaultContext;
        }
        else
        {
            type = new QualifiedType(sanitized.ToString(), true);
            if (context is not SpecPropertyContext.Localization and not SpecPropertyContext.Property)
            {
                GetLogger(ref s).LogWarning("Invalid type reference: {0}.", sanitized.ToString());
                typeKey = default;
                return false;
            }
        }

        typeKey.TypeName = type.Normalized;
        typeKey.IsLocalization = context == SpecPropertyContext.Localization;
        return true;
    }

    private void ReadType(
        in TypeKey key,
        IPropertySourceNode property,
        DatTypeWithProperties type,
        ref ReadState s)
    {
        for (DatTypeWithProperties? baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            s.BaseTypeStack.Push(baseType);
        }

        while (s.BaseTypeStack.Count > 0)
        {
            DatTypeWithProperties baseType = s.BaseTypeStack.Pop();
            TypeKey k;
            k.IsLocalization = key.IsLocalization;
            k.TypeName = baseType.TypeName.Type;

            if (!s.Properties.TryGetValue(k, out IPropertySourceNode? node)
                || s.Processed.Contains(node))
            {
                continue;
            }

            s.Output[k] = ReadNode(in k, baseType, node, ref s);
            s.Processed.Add(node);
        }

        s.Output[key] = ReadNode(in key, type, property, ref s);
        s.Processed.Add(property);
    }

    private static OrderedPropertyReference[] ReadNode(in TypeKey key, DatTypeWithProperties type, IPropertySourceNode property, ref ReadState state)
    {
        IListSourceNode listNode = (IListSourceNode)property.Value!;

        ImmutableArray<DatProperty> propertyArray = GetPropertyList(type, key.IsLocalization);
        ImmutableArray<DatProperty> altPropertyArray = GetPropertyList(type, !key.IsLocalization);

        int offset = propertyArray.Length, altOffset = altPropertyArray.Length;
        for (DatTypeWithProperties? baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
        {
            TypeKey tk;
            tk.TypeName = baseType.TypeName.Type;
            tk.IsLocalization = key.IsLocalization;
            if (!state.Output.TryGetValue(tk, out OrderedPropertyReference[]? indices))
            {
                indices = state.Parent?.GetOrderForType(tk.TypeName, tk.Context);
                if (indices == null)
                    continue;
            }

            for (int i = 0; i < indices.Length; ++i)
            {
                OrderedPropertyReference ind = indices[i];
                state.ReadBuffer.Add(ind + (ind.IsOppositeContext ? altOffset : offset));
            }

            break;
        }

        int sectionLength = 0;
        int insertIndex = state.ReadBuffer.Count;

        foreach (ISourceNode childNode in listNode.Children)
        {
            IValueSourceNode? value;
            switch (childNode)
            {
                case IWhiteSpaceSourceNode:
                    insertIndex = state.ReadBuffer.Count;
                    sectionLength = 0;
                    continue;

                case IAnyValueSourceNode anyValue:
                    value = anyValue as IValueSourceNode;
                    break;

                default:
                    continue;
            }

            if (value == null)
            {
                GetLogger(ref state).LogWarning("Invalid value in type {0}.", property.Key);
                continue;
            }

            if (value.Value.Equals("_", StringComparison.OrdinalIgnoreCase) && !value.IsQuoted)
            {
                state.ReadBuffer.Insert(insertIndex, OrderedPropertyReference.EmptyLine);
                ++insertIndex;
                ++sectionLength;
            }
            else if (value.Value is [ '@', .. ])
            {
                ReadOnlySpan<char> refKey = value.Value.AsSpan(1).Trim();

                int index = propertyArray.Length;
                int altIndex = altPropertyArray.Length;

                bool found = false;
                bool isAlt = false;
                for (DatTypeWithProperties? baseType = type.BaseType; baseType != null; baseType = baseType.BaseType)
                {
                    ImmutableArray<DatProperty> propertyList = GetPropertyList(baseType, key.IsLocalization);
                    ImmutableArray<DatProperty> altPropertyList = GetPropertyList(baseType, !key.IsLocalization);

                    int foundIndex = IndexOfProperty(propertyList, refKey);
                    if (foundIndex < 0)
                    {
                        foundIndex = IndexOfProperty(altPropertyList, refKey);
                        if (foundIndex >= 0)
                        {
                            altIndex += foundIndex;
                            found = true;
                            isAlt = true;
                            break;
                        }
                    }
                    else 
                    {
                        index += foundIndex;
                        found = true;
                        break;
                    }

                    index += propertyList.Length;
                    altIndex += altPropertyList.Length;
                }

                if (!found)
                {
                    GetLogger(ref state).LogWarning("Invalid base reference in {0}: {1}.", type.TypeName.Type, value.Value);
                    continue;
                }

                OrderedPropertyReference pref = isAlt
                    ? OrderedPropertyReference.FromPropertyInOtherContext(altIndex)
                    : OrderedPropertyReference.FromProperty(index);

                int refIndex = state.ReadBuffer.IndexOf(pref);
                if (refIndex == -1)
                {
                    refIndex = state.ReadBuffer.Count;
                }

                int newInsertIndex = refIndex + sectionLength + 1;
                for (int i = 1; i <= sectionLength; ++i)
                {
                    OrderedPropertyReference v = state.ReadBuffer[insertIndex - i];
                    state.ReadBuffer.RemoveAt(insertIndex - i);
                    state.ReadBuffer.Insert(refIndex, v);
                }

                insertIndex = newInsertIndex;
            }
            else
            {
                ReadOnlySpan<char> sp = value.Value.AsSpan().Trim();
                int index = IndexOfProperty(propertyArray, sp);
                if (index < 0)
                {
                    index = IndexOfProperty(altPropertyArray, sp);
                    if (index < 0)
                    {
                        GetLogger(ref state).LogWarning("Invalid reference in {0}: {1}.", type.TypeName.Type, value.Value);
                        continue;
                    }

                    state.ReadBuffer.Insert(insertIndex, OrderedPropertyReference.FromPropertyInOtherContext(index));
                }
                else
                {
                    state.ReadBuffer.Insert(insertIndex, OrderedPropertyReference.FromProperty(index));
                }
                ++insertIndex;
                ++sectionLength;
            }
        }

        OrderedPropertyReference[] outArray = state.ReadBuffer.ToArray();
        state.ReadBuffer.Clear();
        return outArray;
    }

    private static ImmutableArray<DatProperty> GetPropertyList(
        DatTypeWithProperties type,
        bool isLocalization)
    {
        if (!isLocalization)
            return type.Properties;

        if (type is IDatTypeWithLocalizationProperties lcl)
            return lcl.LocalizationProperties;

        return ImmutableArray<DatProperty>.Empty;
    }

    private static int IndexOfProperty(
        ImmutableArray<DatProperty> propertyList,
        ReadOnlySpan<char> key
    )
    {
        int index = -1;
        for (int i = 0; i < propertyList.Length; i++)
        {
            DatProperty prop = propertyList[i];
            if (!prop.Key.AsSpan().Equals(key, StringComparison.OrdinalIgnoreCase))
                continue;

            index = i;
            break;
        }

        return index;
    }

    private static ILogger<PropertyOrderFile> GetLogger(ref ReadState s)
    {
        return s.Database.ReadContext.LoggerFactory.CreateLogger<PropertyOrderFile>();
    }

    private struct ReadState
    {
        public ImmutableDictionary<TypeKey, OrderedPropertyReference[]>.Builder Output;
        public Stack<DatTypeWithProperties> BaseTypeStack;
        public HashSet<IPropertySourceNode> Processed;
        public Dictionary<TypeKey, IPropertySourceNode> Properties;
        public IAssetSpecDatabase Database;
        public List<OrderedPropertyReference> ReadBuffer;
        public IPropertyOrderFile? Parent;
    }

}

/// <summary>
/// Represents a property reference in a orderfile.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 4)]
public readonly struct OrderedPropertyReference : IEquatable<OrderedPropertyReference>
{
    private readonly int _index;

    private const int NotIndexMask    = unchecked ( (int)0b10000000000000000000000000000000u );
    private const int FlagMask        = unchecked ( (int)0b11000000000000000000000000000000u );
    private const int SpecialValue    = unchecked ( (int)0b11000000000000000000000000000000u );
    private const int SameContext     = unchecked ( (int)0b00000000000000000000000000000000u );
    private const int OppositeContext = unchecked ( (int)0b01000000000000000000000000000000u );
    
    /// <summary>
    /// A property reference representing a blank line.
    /// </summary>
    public static readonly OrderedPropertyReference EmptyLine
        = new OrderedPropertyReference(SpecialValue | PropertyOrderFile.EmptyLine);

    /// <summary>
    /// Whether or not this represents an empty line.
    /// </summary>
    public bool IsEmptyLine => _index == (SpecialValue | PropertyOrderFile.EmptyLine);

    /// <summary>
    /// Whether or not this represents an index in the opposite context of the asset.
    /// </summary>
    public bool IsOppositeContext => (_index & FlagMask) == OppositeContext;

    /// <summary>
    /// Gets the index this property refers to.
    /// </summary>
    public int Index => _index & ~FlagMask;

    private OrderedPropertyReference(int info)
    {
        _index = info;
    }

    /// <summary>
    /// Create a property within the same context.
    /// </summary>
    public static OrderedPropertyReference FromProperty(int index)
    {
        return new OrderedPropertyReference(SameContext | index);
    }

    /// <summary>
    /// Create a property within the opposite context.
    /// </summary>
    public static OrderedPropertyReference FromPropertyInOtherContext(int index)
    {
        return new OrderedPropertyReference(OppositeContext | index);
    }

    /// <summary>
    /// Add a value to a reference's index.
    /// </summary>
    public static OrderedPropertyReference operator +(OrderedPropertyReference a, int b)
    {
        if ((a._index & NotIndexMask) != 0)
        {
            return a;
        }

        return new OrderedPropertyReference((a._index & FlagMask) | ((a._index & ~FlagMask) + b));
    }

    /// <summary>
    /// Subtract a value from a reference's index.
    /// </summary>
    public static OrderedPropertyReference operator -(OrderedPropertyReference a, int b)
    {
        if ((a._index & NotIndexMask) != 0)
        {
            return a;
        }

        return new OrderedPropertyReference((a._index & FlagMask) | ((a._index & ~FlagMask) - b));
    }

    /// <inheritdoc />
    public bool Equals(OrderedPropertyReference other) => _index == other._index;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is OrderedPropertyReference other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _index;

    /// <summary>
    /// Gets a string value representing the referenced property in the given type.
    /// </summary>
    public string GetString(DatTypeWithProperties type, bool isLocalization)
    {
        switch (_index & FlagMask)
        {
            case SpecialValue:
                if (IsEmptyLine)
                    return "_";
                break;

            case SameContext:
                return TryGetReferencedProperty(type, isLocalization, out DatProperty? property)
                    ? (object)property.Owner == type ? property.Key : $"@{property.Key}"
                    : $"<invalid: #{Index}>";

            case OppositeContext:
                return TryGetReferencedProperty(type, isLocalization, out property)
                    ? (object)property.Owner == type ? property.Key : $"@{property.Key}"
                    : $"<invalid opposite: #{Index}>";
        }

        return "<invalid>";
    }

    public bool TryGetReferencedProperty(DatTypeWithProperties type, bool isLocalization, [NotNullWhen(true)] out DatProperty? property)
    {
        switch (_index & FlagMask)
        {
            default:
                property = null;
                return false;

            case SameContext:
                break;

            case OppositeContext:
                isLocalization = !isLocalization;
                break;
        }

        int index = Index;
        for (DatTypeWithProperties? t = type; t != null; t = t.BaseType)
        {
            ImmutableArray<DatProperty> propArray = isLocalization
                ? (t as IDatTypeWithLocalizationProperties)?.LocalizationProperties ?? ImmutableArray<DatProperty>.Empty
                : t.Properties;

            if (index < propArray.Length)
            {
                property = propArray[index];
                return true;
            }

            index -= propArray.Length;
        }

        property = null;
        return false;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        switch (_index & FlagMask)
        {
            case SpecialValue:
                if (IsEmptyLine)
                    return "_";
                break;

            case SameContext:
                return $"Property #{Index}";

            case OppositeContext:
                return $"Property #{Index} in opposite context";
        }

        return "<invalid>";
    }

}
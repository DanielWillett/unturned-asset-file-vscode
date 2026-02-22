using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Helpers and extensions for <see cref="IDataRef"/> values.
/// </summary>
public static class DataRefs
{
    public delegate IDataRefTarget DataRefRootFactory(IType? type, DatProperty owner);

    /// <summary>
    /// Read-only, case-insensitive set of all reserved keywords for data-refs.
    /// </summary>
    public static IImmutableSet<string> Keywords { get; }

    /// <summary>
    /// All built-in data-ref roots (besides contextual ones supplied by <see cref="IDataRefReadContext"/>).
    /// </summary>
    public static IImmutableDictionary<string, DataRefRootFactory> Roots { get; }

    /// <summary>
    /// All built-in data-ref properties.
    /// </summary>
    public static IImmutableDictionary<string, IPropertyFactory> Properties { get; }

    static DataRefs()
    {
        ImmutableHashSet<string>.Builder bldr = ImmutableHashSet.CreateBuilder<string>(StringComparer.OrdinalIgnoreCase);

        bldr.Add("This");
        bldr.Add("Self");
        bldr.Add("Index");
        bldr.Add("Key");
        bldr.Add("Value");

        Keywords = bldr.ToImmutable();

        ImmutableDictionary<string, DataRefRootFactory>.Builder roots
            = ImmutableDictionary.CreateBuilder<string, DataRefRootFactory>(StringComparer.OrdinalIgnoreCase);

        roots["This"] = (_, owner) => new ThisDataRef(owner);
        roots["Self"] = (_, owner) => new SelfDataRef(owner);

        Roots = roots.ToImmutable();

        ImmutableDictionary<string, IPropertyFactory>.Builder properties
            = ImmutableDictionary.CreateBuilder<string, IPropertyFactory>(StringComparer.OrdinalIgnoreCase);

        properties["Excluded"]   = new DataRefPropertyFactory<ExcludedProperty>();
        properties["Included"]   = new DataRefPropertyFactory<IncludedProperty>();
        properties["Key"]        = new DataRefPropertyFactory<KeyProperty>();
        properties["AssetName"]  = new DataRefPropertyFactory<AssetNameProperty>();
        properties["Difficulty"] = new DataRefPropertyFactory<DifficultyProperty>();
        properties["Indices"]    = new DataRefPropertyFactory<IndicesProperty>();
        properties["IsLegacy"]   = new DataRefPropertyFactory<IsLegacyProperty>();
        properties["ValueType"]  = new DataRefPropertyFactory<ValueTypeProperty>();
        properties["Count"]      = new DataRefPropertyFactory<CountProperty>();

        Properties = properties.ToImmutable();
    }

    public static bool TryReadDataRef(
        string text,
        IType? type,
        DatProperty owner,
        [NotNullWhen(true)] out IDataRef? dataRef)
    {
        NilDataRefContext c;
        return TryReadDataRef(text, type, owner, out dataRef, ref c);
    }

    public static bool TryReadDataRef<TDataRefReadContext>(
        string text,
        IType? type,
        DatProperty owner,
        [NotNullWhen(true)] out IDataRef? dataRef,
        ref TDataRefReadContext context
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        dataRef = null;

        if (!TryParseDataRef(text,
                out ReadOnlySpan<char> root,
                out bool isRootEscaped,
                out ReadOnlySpan<char> property,
                out ReadOnlySpan<char> indices,
                out ReadOnlySpan<char> properties))
        {
            return false;
        }

        IDataRefTarget? target = null;
        if (!isRootEscaped)
        {
            foreach (KeyValuePair<string, DataRefRootFactory> factoryPair in Roots)
            {
                if (!root.Equals(factoryPair.Key, StringComparison.OrdinalIgnoreCase))
                    continue;

                target = factoryPair.Value(type, owner);
                break;
            }

            if (context != null)
            {
                if (context.TryReadTarget(root, type, owner, out IDataRefTarget? target2))
                    target = target2;
            }
        }

        if (target == null)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            PropertyReference propRef = PropertyReference.Parse(root, PropertyResolutionContext.Unknown);
            target = new PropertyDataRef(propRef, owner);
        }

        if (property.IsEmpty)
        {
            dataRef = target as IDataRef;
            return dataRef != null;
        }

        string propertyName = property.ToString();
        if (!Properties.TryGetValue(propertyName, out IPropertyFactory? propFactory))
        {
            return false;
        }

        OneOrMore<KeyValuePair<string, object?>> propertyList = OneOrMore<KeyValuePair<string, object?>>.Null;
        OneOrMore<int> indexList = OneOrMore<int>.Null;

        if (!indices.IsEmpty)
        {
            int ct = indices.Count(',') + 1;
            Span<Range> ranges = stackalloc Range[ct];
            // ReSharper disable once InvokeAsExtensionMember
            ct = SpanExtensions.Split(indices, ranges, ',');
            if (ct == 1)
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                if (!int.TryParse(indices, NumberStyles.Number, CultureInfo.InvariantCulture, out int result))
#else
                if (!int.TryParse(indices.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int result))
#endif
                    return false;
                indexList = new OneOrMore<int>(result);
            }
            else
            {
                int[] list = new int[ct];
                for (int i = 0; i < ct; ++i)
                {
                    ReadOnlySpan<char> index = indices[ranges[i]];
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                    if (!int.TryParse(index, NumberStyles.Number, CultureInfo.InvariantCulture, out int result))
#else
                    if (!int.TryParse(index.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int result))
#endif
                        return false;
                    list[i] = result;
                }

                indexList = new OneOrMore<int>(list);
            }
        }

        if (!properties.IsEmpty)
        {
            using JsonDocument jsonDoc = JsonDocument.Parse(properties.ToString());
            if (jsonDoc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            List<KeyValuePair<string, object?>> propertyListBuffer = new List<KeyValuePair<string, object?>>();
            foreach (JsonProperty prop in jsonDoc.RootElement.EnumerateObject())
            {
                JsonElement value = prop.Value;
                if (!JsonHelper.TryReadGenericValue(in value, out object? obj))
                    return false;

                propertyListBuffer.Add(new KeyValuePair<string, object?>(prop.Name, obj));
            }

            propertyList = new OneOrMore<KeyValuePair<string, object?>>(propertyListBuffer);
        }

        if (type != null)
        {
            PropertyTypeVisitor visitor;
            visitor.DataRef = null;
            visitor.Target = target;
            visitor.PropertyFactory = propFactory;
            visitor.Indices = indexList;
            visitor.JsonProperties = propertyList;
            type.Visit(ref visitor);
            if (visitor.DataRef != null)
            {
                dataRef = visitor.DataRef;
                return true;
            }
        }

        dataRef = propFactory.CreateDataRef(target, indexList, propertyList);
        return true;
    }

    internal struct NilDataRefContext : IDataRefReadContext
    {
        public bool TryReadTarget(ReadOnlySpan<char> root, IType? type, DatProperty owner,
            [NotNullWhen(true)] out IDataRefTarget? target)
        {
            target = null;
            return false;
        }
    }

    private struct PropertyTypeVisitor : ITypeVisitor
    {
        public IDataRef? DataRef;
        public IPropertyFactory PropertyFactory;
        public IDataRefTarget Target;
        public OneOrMore<int> Indices;
        public OneOrMore<KeyValuePair<string, object?>> JsonProperties;

        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            DataRef = PropertyFactory.CreateDataRef(type, Target, Indices, JsonProperties);
        }
    }

    internal static bool TryParseDataRef(
        ReadOnlySpan<char> text,
        out ReadOnlySpan<char> root,
        out bool isRootEscaped,
        out ReadOnlySpan<char> property,
        out ReadOnlySpan<char> indices,
        out ReadOnlySpan<char> properties)
    {
        root = ReadOnlySpan<char>.Empty;
        property = ReadOnlySpan<char>.Empty;
        indices = ReadOnlySpan<char>.Empty;
        properties = ReadOnlySpan<char>.Empty;
        isRootEscaped = false;

        if (text.IsEmpty)
        {
            return false;
        }

        if (text[0] == '#')
        {
            text = text[1..];
        }

        if (text.IsEmpty)
        {
            return false;
        }

        int periodIndex = -1;
        if (text[0] == '(')
        {
            int index = StringHelper.NextUnescapedIndexOfParenthesis(text[1..], out bool hadEscapeSequences);
            if (index < 0)
            {
                return false;
            }

            if (index + 2 < text.Length && text[index + 2] == '.')
            {
                // index + 1 == ) since text[1..]
                periodIndex = index + 2;
            }

            root = text.Slice(1, index);
            if (root.Length > 1 && root[0] == '\\' && root[1] != '\\')
            {
                isRootEscaped = true;
                root = root[1..];
                hadEscapeSequences = root.IndexOf('\\') >= 0;
            }
            if (hadEscapeSequences)
            {
                root = StringHelper.Unescape(root);
            }
        }
        else
        {
            int index = text.IndexOf('.');
            if (index < 0)
            {
                index = text.Length;
            }
            else
            {
                periodIndex = index;
            }

            root = text[..index];
            if (root.Length > 0 && root[0] == '\\' && root[1] != '\\')
            {
                isRootEscaped = true;
                root = root[1..];
            }
        }

        if (periodIndex < 0 || periodIndex + 1 >= text.Length)
        {
            return true;
        }

        text = text.Slice(periodIndex + 1);
        int propertiesOrIndiciesIndex = -1;

        if (text[0] == '(')
        {
            int index = StringHelper.NextUnescapedIndexOfParenthesis(text[1..], out bool hadEscapeSequences);
            if (index < 0)
            {
                return false;
            }

            if (text.Length > index + 2 && text[index + 2] is '{' or '[')
            {
                propertiesOrIndiciesIndex = index + 2;
            }

            property = text.Slice(1, index);
            if (hadEscapeSequences)
            {
                property = StringHelper.Unescape(property);
            }
        }
        else
        {
#if NET7_0_OR_GREATER
            ReadOnlySpan<char> endTokens = [ '{', '[' ];
#else
            ReadOnlySpan<char> endTokens = stackalloc char[] { '{', '[' };
#endif
            int index = text.IndexOfAny(endTokens);
            if (index < 0)
            {
                index = text.Length;
            }
            else
            {
                propertiesOrIndiciesIndex = index;
            }

            property = text[..index];
        }

        if (propertiesOrIndiciesIndex < 0 || propertiesOrIndiciesIndex + 1 >= text.Length)
        {
            return true;
        }

        text = text.Slice(propertiesOrIndiciesIndex);
        if (text[0] == '[')
        {
            int closingIndex = text.Slice(1).IndexOf(']');
            if (closingIndex < 0)
            {
                return false;
            }

            if (text.Length > closingIndex + 2 && text[closingIndex + 2] == '{')
            {
                propertiesOrIndiciesIndex = closingIndex + 2;
            }
            else
            {
                propertiesOrIndiciesIndex = -1;
            }

            indices = text.Slice(1, closingIndex).Trim();

            if (propertiesOrIndiciesIndex < 0 || propertiesOrIndiciesIndex + 1 >= text.Length)
            {
                return true;
            }

            text = text.Slice(propertiesOrIndiciesIndex);
        }

        if (text[0] == '{')
        {
            int closingIndex = text.Slice(1).IndexOf('}');
            if (closingIndex < 0)
            {
                return false;
            }

            properties = text.Slice(0, closingIndex + 2).Trim();
        }

        return true;
    }

    public interface IPropertyFactory
    {
        IDataRef CreateDataRef(
            IDataRefTarget target,
            OneOrMore<int> indices,
            OneOrMore<KeyValuePair<string, object?>> properties
        );

        IDataRef<TValue> CreateDataRef<TValue>(
            IType<TValue> type,
            IDataRefTarget target,
            OneOrMore<int> indices,
            OneOrMore<KeyValuePair<string, object?>> properties
        ) where TValue : IEquatable<TValue>;
    }

    private sealed class DataRefPropertyFactory<TProperty> : IPropertyFactory
        where TProperty : struct, IDataRefProperty, IEquatable<TProperty>
    {
#if !NET7_0_OR_GREATER
        private TProperty _instance;
#endif
        public IDataRef CreateDataRef(
            IDataRefTarget target,
            OneOrMore<int> indices,
            OneOrMore<KeyValuePair<string, object?>> properties
        )
        {
#if NET7_0_OR_GREATER
            return TProperty.CreateDataRef(target, indices, properties);
#else
            return _instance.CreateDataRef(target, indices, properties);
#endif
        }

        public IDataRef<TValue> CreateDataRef<TValue>(
            IType<TValue> type,
            IDataRefTarget target,
            OneOrMore<int> indices,
            OneOrMore<KeyValuePair<string, object?>> properties
        ) where TValue : IEquatable<TValue>
        {
#if NET7_0_OR_GREATER
            return TProperty.CreateDataRef(type, target, indices, properties);
#else
            return _instance.CreateDataRef(type, target, indices, properties);
#endif
        }
    }
}

public interface IDataRefReadContext
{
    bool TryReadTarget(ReadOnlySpan<char> root, IType? type, DatProperty owner, [NotNullWhen(true)] out IDataRefTarget? target);
}
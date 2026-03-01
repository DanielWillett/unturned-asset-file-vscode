using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;


/*

General Format (for property 'A'):

   Crumbs: "/Properties[1]/[2]/"
   |Properties
   |[
   |    [
   |        4
   |    ]
   |    [
   |        {
   |            A 7
   |        }
   |        {
   |            A 4
   |        }
   |        {
   |            A 1 <---
   |        }
   |    ]
   |]

   Crumbs: "/Properties/Prop1/"
   |Properties
   |{
   |    Prop1
   |    {
   |        A 1 <---
   |    }
   |}

   Crumbs: "/"
   |A 1 <---

   Crumbs: "/Config/"
   |Config
   |{
   |    A 1 <---
   |}

*/

/// <summary>
/// Traces the 'path' to a property that may be nested in objects.
/// </summary>
public readonly struct PropertyBreadcrumbs : IEquatable<PropertyBreadcrumbs>
{
    /// <summary>
    /// An empty breadcrumb list.
    /// </summary>
    // ReSharper disable once UnassignedReadonlyField
    public static readonly PropertyBreadcrumbs Root;

    private readonly PropertyBreadcrumbSection[]? _sections;

    /// <summary>
    /// If the breadcrumb list is empty, meaning the property is part of the root dictionary (or part of the "Asset" or "Metadata" dictionaries for v2 format).
    /// </summary>
    [MemberNotNullWhen(false, nameof(_sections))]
    public bool IsRoot => Length == 0;

    /// <summary>
    /// Number of sections in the breadcrumb list.
    /// </summary>
    public int Length { get; }

    public PropertyBreadcrumbs(params PropertyBreadcrumbSection[]? sections)
    {
        _sections = sections;
        Length = _sections?.Length ?? 0;
    }

    /// <summary>
    /// Get a section from the breadcrumb list.
    /// </summary>
    public ref readonly PropertyBreadcrumbSection this[int index]
    {
        get
        {
            if (_sections == null || index < 0 || index >= _sections.Length)
                throw new ArgumentOutOfRangeException();

            return ref _sections[index];
        }
    }

    /// <summary>
    /// Parses a property with it's optional breadcrumbs from a string such as 'Blueprints[3].InputItems[0].Id'.
    /// </summary>
    /// <remarks>The value returned by this method needs to be further resolved using <see cref="ResolveFromPropertyRef"/> before it can function properly.</remarks>
    /// <param name="propertyRef">The input string to parse.</param>
    /// <param name="propertyName">The name of the property which is located at the returned breadcrumbs.</param>
    /// <returns>The location of the described property.</returns>
    public static PropertyBreadcrumbs FromPropertyRef(ReadOnlySpan<char> propertyRef, out string propertyName, PropertyResolutionContext context = PropertyResolutionContext.Modern)
    {
        return FromPropertyRef(propertyRef, null, out propertyName, context);
    }

    /// <summary>
    /// Parses a property with it's optional breadcrumbs from a string such as 'Blueprints[3].InputItems[0].Id'.
    /// </summary>
    /// <remarks>The value returned by this method needs to be further resolved using <see cref="ResolveFromPropertyRef"/> before it can function properly.</remarks>
    /// <param name="propertyRef">The input string to parse.</param>
    /// <param name="propertyName">The name of the property which is located at the returned breadcrumbs.</param>
    /// <returns>The location of the described property.</returns>
    public static PropertyBreadcrumbs FromPropertyRef(string propertyRef, out string propertyName, PropertyResolutionContext context = PropertyResolutionContext.Modern)
    {
        return FromPropertyRef(propertyRef.AsSpan(), propertyRef, out propertyName, context);
    }

    private static PropertyBreadcrumbs FromPropertyRef(ReadOnlySpan<char> propertyRef, string? propertyRefStr, out string propertyName, PropertyResolutionContext context = PropertyResolutionContext.Modern)
    {
        if (propertyRef.IsEmpty)
        {
            propertyName = string.Empty;
            return Root;
        }

        int ct = 0;
        int escCount = 0;
        bool hasEsc = false;
        for (int i = 0; i < propertyRef.Length; ++i)
        {
            switch (propertyRef[i])
            {
                case '\\':
                    ++escCount;
                    hasEsc = true;
                    break;

                case '.':
                    if (escCount % 2 == 1)
                        break;

                    escCount = 0;
                    ++ct;
                    break;

                case '[':
                    if (escCount % 2 == 1)
                        goto default;

                    escCount = 0;
                    int p = TryParseIndex(propertyRef, i, out int lastIndex, apply: false);
                    if (p == -1)
                        continue;
                    ++ct;
                    i = lastIndex;
                    break;

                default:
                    escCount = 0;
                    break;
            }
        }

        if (ct == 0 && !hasEsc)
        {
            propertyName = propertyRefStr ?? propertyRef.ToString();
            return Root;
        }

        PropertyBreadcrumbSection[] section = ct == 0 ? Array.Empty<PropertyBreadcrumbSection>() : new PropertyBreadcrumbSection[ct];
        int sectionIndex = 0;

        if (hasEsc)
        {
            StringBuilder sb = new StringBuilder();

            escCount = 0;
            for (int i = 0; i < propertyRef.Length; ++i)
            {
                switch (propertyRef[i])
                {
                    case '\\':
                        ++escCount;
                        if (escCount % 2 == 0)
                            sb.Append('\\');
                        break;

                    case '.':
                        if (escCount % 2 == 1)
                            goto default;

                        escCount = 0;
                        section[sectionIndex] = new PropertyBreadcrumbSection(sb.ToString(), context);
                        ++sectionIndex;
                        sb.Clear();
                        break;

                    case '[':
                        if (escCount % 2 == 1)
                            goto default;

                        escCount = 0;
                        int specifiedIndex = TryParseIndex(propertyRef, i, out int lastIndex, apply: true);
                        if (specifiedIndex == -1)
                            goto default;
                        ref PropertyBreadcrumbSection sec = ref section[sectionIndex];
                        ++sectionIndex;
                        if (sb.Length > 0)
                        {
                            sec = new PropertyBreadcrumbSection(sb.ToString(), context, specifiedIndex);
                            sb.Clear();
                        }
                        else
                        {
                            sec = new PropertyBreadcrumbSection(context, specifiedIndex);
                        }
                        i = lastIndex;
                        break;

                    default:
                        escCount = 0;
                        sb.Append(propertyRef[i]);
                        break;
                }
            }

            propertyName = sb.ToString();
        }
        else
        {
            ReadOnlySpan<char> triggerChars = [ '[', '.' ];
            int firstIndex = propertyRef.IndexOfAny(triggerChars);
            if (firstIndex < 0)
            {
                propertyName = propertyRefStr ?? propertyRef.ToString();
                return Root;
            }

            int lastIndex = -1;
            bool hasDictSection = firstIndex > 0;
            while (true)
            {
                int index = lastIndex < 0 ? firstIndex : propertyRef.Slice(lastIndex + 1).IndexOfAny(triggerChars);
                if (index < 0)
                {
                    break;
                }

                index += lastIndex + 1;
                int prevIndex = lastIndex;
                lastIndex = index;

                char c = propertyRef[index];
                switch (c)
                {
                    case '[':

                        int specifiedIndex = TryParseIndex(propertyRef, index, out lastIndex, apply: true);

                        ref PropertyBreadcrumbSection sec = ref section[sectionIndex];
                        ++sectionIndex;
                        if (hasDictSection)
                        {
                            sec = new PropertyBreadcrumbSection(propertyRef.Slice(prevIndex + 1, index - prevIndex - 1).ToString(), context, specifiedIndex);
                        }
                        else
                        {
                            sec = new PropertyBreadcrumbSection(context, specifiedIndex);
                        }
                        hasDictSection = propertyRef[lastIndex] == '.';
                        break;

                    case '.':
                        hasDictSection = true;
                        section[sectionIndex] = new PropertyBreadcrumbSection(propertyRef.Slice(prevIndex + 1, index - prevIndex - 1).ToString(), context);
                        ++sectionIndex;
                        break;
                }
            }

            if (hasDictSection && lastIndex + 1 < propertyRef.Length)
            {
                propertyName = propertyRef.Slice(lastIndex + 1).ToString();
            }
            else
            {
                propertyName = string.Empty;
            }
        }

        if (sectionIndex < section.Length)
        {
            Array.Resize(ref section, sectionIndex);
        }

        return new PropertyBreadcrumbs(section);

        static int TryParseIndex(ReadOnlySpan<char> propertyRef, int index, out int lastIndex, bool apply)
        {
            lastIndex = index;
            int endIndex = propertyRef.Slice(index + 1).IndexOf(']');
            if (endIndex <= 0)
                return -1;

            ReadOnlySpan<char> indexValue = propertyRef.Slice(index + 1, endIndex);
            indexValue = indexValue.Trim();
            bool isFromEnd = indexValue[0] == '^';
            if (isFromEnd)
            {
                indexValue = indexValue[1..];
                indexValue = indexValue.TrimStart();
            }

            endIndex += index + 1;
            lastIndex = endIndex;
            if (endIndex + 1 < propertyRef.Length && propertyRef[endIndex + 1] == '.')
            {
                lastIndex = endIndex + 1;
            }

            int specifiedIndex;
            if (!apply)
            {
                specifiedIndex = 0;
                for (int j = 0; j < indexValue.Length; ++j)
                {
                    if (char.IsDigit(indexValue[j]))
                        continue;

                    specifiedIndex = -1;
                    break;
                }
            }
            else
            {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                if (!int.TryParse(indexValue, NumberStyles.None, CultureInfo.InvariantCulture, out specifiedIndex))
#else
                if (!int.TryParse(indexValue.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out specifiedIndex))
#endif
                {
                    lastIndex = index;
                    return -1;
                }
            }

            if (specifiedIndex < 0)
            {
                lastIndex = index;
                return -1;
            }

            return isFromEnd ? -specifiedIndex - 1 : specifiedIndex;
        }
    }

    /// <summary>
    /// Resolves possible properties in-place from this property-ref.
    /// </summary>
    public void ResolveFromPropertyRef(DatType baseType, IAssetSpecDatabase database, SpecPropertyContext context = SpecPropertyContext.Property)
    {
        if (IsRoot)
            return;

        DatType? typeOwner = baseType;
        int skipType = 0;

        for (int i = 0; i < _sections.Length; ++i)
        {
            ref PropertyBreadcrumbSection section = ref _sections[i];
            if (typeOwner != null && skipType <= 0)
            {
                DatProperty? property = section.Property as DatProperty;
                if (property == null && section.Property is string propertyName && typeOwner != null)
                {
                    // todo property = database.FindPropertyInfoByKey(propertyName, typeOwner, section.Context, context: context).Property;
                }

                if (property == null)
                {
                    typeOwner = null;
                }
                else
                {
                    section = new PropertyBreadcrumbSection(in section, property);
                    IPropertyType? typeOrListType = property.Type;
                    skipType = 0;
                    typeOwner = null;
                    while (typeOrListType != null)
                    {
                        switch (typeOrListType)
                        {
                            case DatType t:
                                typeOwner = t;
                                break;

                            case IListType list:
                                IType? innerType = list.ElementType;
                                if (innerType == null)
                                    break;
                                ++skipType;
                                typeOrListType = innerType;
                                continue;

                            case IDictionaryType dict:
                                innerType = dict.ValueType;
                                if (innerType == null)
                                    break;
                                ++skipType;
                                typeOrListType = innerType;
                                continue;
                        }

                        break;
                    }
                }
            }
            else if (skipType > 0)
            {
                --skipType;
            }
        }
    }

    /// <summary>
    /// Create breadcrums from a specific property or list value node.
    /// </summary>
    public static PropertyBreadcrumbs FromNode(ISourceNode node)
    {
        // must be a property or value in a list
        if (node is not IPropertySourceNode && node.Parent is not IListSourceNode)
            throw new ArgumentException("Expected either a property or element in a list.");

        lock (node.File.TreeSync)
        {
            // property is in root dictionary
            if (ReferenceEquals(node.Parent, node.File))
            {
                return Root;
            }

            IDictionarySourceNode? assetData = null, metadata = null;

            // property is in "Asset" or "Metadata" sections
            if (node.File is IAssetSourceFile assetFile)
            {
                assetData = assetFile.GetAssetDataDictionary();
                metadata = assetFile.GetMetadataDictionary();
                if (ReferenceEquals(node.Parent, assetData) || ReferenceEquals(node.Parent, metadata))
                {
                    return Root;
                }
            }

            int parentCt = 0;
            int sectionCt = 0;
            for (ISourceNode p = node.Parent; !ReferenceEquals(p, assetData) && !ReferenceEquals(p, metadata) && !ReferenceEquals(p.Parent, p); p = p.Parent)
            {
                ++parentCt;
                if (p is IListSourceNode or IDictionarySourceNode { Parent: IPropertySourceNode })
                    ++sectionCt;
            }

            if (parentCt == 0)
            {
                return Root;
            }

            PropertyBreadcrumbSection[] sections = new PropertyBreadcrumbSection[sectionCt];

            int lastIndex = node.Index;
            IParentSourceNode parent = node.Parent;
            for (int i = 0; i < parentCt; ++i)
            {
                switch (parent)
                {
                    case IListSourceNode list:
                        if (list.Parent is IPropertySourceNode propertyNode)
                        {
                            sections[--sectionCt] = new PropertyBreadcrumbSection(propertyNode, PropertyResolutionContext.Modern, lastIndex, propertyNode.Key);
                        }
                        else
                        {
                            sections[--sectionCt] = new PropertyBreadcrumbSection(list, PropertyResolutionContext.Modern, lastIndex, null);
                        }
                        lastIndex = parent.Index;
                        break;

                    case IDictionarySourceNode dict:
                        if (dict.Parent is IPropertySourceNode prop)
                        {
                            sections[--sectionCt] = new PropertyBreadcrumbSection(prop, PropertyResolutionContext.Modern, prop.Key);
                            lastIndex = -1;
                            break;
                        }

                        lastIndex = dict.Index;
                        break;
                }

                parent = parent.Parent;
            }

            return new PropertyBreadcrumbs(sections);
        }
    }

    /// <summary>
    /// Find the parent dictionary and value type of a value node referred to by breadcrumbs.
    /// </summary>
    /// <param name="root">The root dictionary to search from.</param>
    /// <param name="rootType">The object type of the root dictionary.</param>
    /// <param name="dictionaryOrValue">The value being referred to by these breadcrumbs. This is usually a dictionary.</param>
    /// <param name="valueType">The type of object represented by <paramref name="dictionaryOrValue"/>.</param>
    /// <param name="ctx">Evaluation context.</param>
    /// <param name="context">The type of properties to search for.</param>
    /// <returns>Whether or not the breadcrumbs could be fully traced.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="InvalidEnumArgumentException">Invalid property context. Expected 'Property', 'Localization', or 'BundleAsset'.</exception>
    public bool TryTraceRelativeTo(
        IDictionarySourceNode root,
        DatTypeWithProperties rootType,
        [NotNullWhen(true)] out IAnyValueSourceNode? dictionaryOrValue,
        [NotNullWhen(true)] out IType? valueType,
        ref FileEvaluationContext ctx,
        SpecPropertyContext context = SpecPropertyContext.Property
    )
    {
        if (root == null)
            throw new ArgumentNullException(nameof(root));
        if (rootType == null)
            throw new ArgumentNullException(nameof(rootType));
        if (ctx.Services == null)
            throw new ArgumentNullException(nameof(ctx));

        dictionaryOrValue = null;
        valueType = null;
        if (IsRoot)
        {
            dictionaryOrValue = root;
            valueType = rootType;
            return true;
        }

        int sectionIndex = 0;
        if (_sections.Length > 0 && _sections[0].IsRootReference)
        {
            sectionIndex = 1;
            if (root.File != root)
            {
                root = root.File;
                QualifiedType actualType = root.File.ActualType;

                // rootType = root.File.Type;
                if (actualType.IsNull
                    || !ctx.Services.Database.TryFindType(actualType, out DatType? datType)
                    || (rootType = (datType as DatTypeWithProperties)!) == null
                   )
                {
                    return false;
                }
            }

            if (_sections.Length == 1)
            {
                dictionaryOrValue = root;
                valueType = rootType;
                return true;
            }
        }

        IDictionarySourceNode current = root;
        DatTypeWithProperties? dictionaryType = rootType;

        // an actual Dictionary<,> value, not just an object that uses a dictionary.
        IDictionaryType? lastDictionaryType = null;

        // a list value that doesn't have a property, such as a list in a list.
        IListSourceNode? lastListValue = null;

        IType? previousResolvedType = null;

        for (; sectionIndex < _sections.Length; ++sectionIndex)
        {
            ref PropertyBreadcrumbSection section = ref _sections[sectionIndex];

            bool alreadyAppliedIndex = false;

            DatProperty? property;
            IPropertyType? propertyType;

            IPropertySourceNode? propertyNode;
            IAnyValueSourceNode? valueNode;

            switch (section.Property)
            {
                case string propertyName:
                    if (lastDictionaryType != null)
                    {
                        if (!current.TryGetProperty(propertyName, out propertyNode))
                        {
                            return false;
                        }

                        valueNode = propertyNode.Value;
                        propertyType = lastDictionaryType.ValueType;
                        lastDictionaryType = null;
                        break;
                    }

                    if (dictionaryType.TryFindProperty(propertyName, context, out property))
                    {
                        goto propLbl;
                    }

                    return false;
                    
                case DatProperty prop:
                    if (lastDictionaryType != null || lastListValue != null)
                    {
                        return false;
                    }

                    property = prop;

                    propLbl:
                    propertyType = property.Type;
                    if (!current.TryGetProperty(property, ref ctx, out propertyNode))
                    {
                        return false;
                    }

                    valueNode = propertyNode.Value;
                    break;

                case null when previousResolvedType is IListType listType:
                    if (lastListValue == null
                        || !lastListValue.TryGetElement(section.Index, out valueNode))
                    {
                        return false;
                    }

                    propertyType = listType.ElementType;
                    alreadyAppliedIndex = true;
                    break;

                default:
                    return false;
            }

            if (!propertyType.TryEvaluateType(out IType? type, ref ctx))
            {
                return false;
            }

            if (!alreadyAppliedIndex)
            {
                if (section.Index >= 0)
                {
                    if (type is not IListType listType)
                    {
                        return false;
                    }

                    type = listType.ElementType;
                    if (valueNode is not IListSourceNode listNode || !listNode.TryGetElement(section.Index, out valueNode))
                    {
                        return false;
                    }
                }
                else if (valueNode is IListSourceNode)
                {
                    return false;
                }
            }

            lastListValue = null;

            if (sectionIndex == _sections.Length - 1)
            {
                dictionaryOrValue = valueNode;
                valueType = type;
                return valueNode != null;
            }

            previousResolvedType = type;

            switch (valueNode)
            {
                case IDictionarySourceNode dict:
                    if (type is IDictionaryType dictType)
                    {
                        lastDictionaryType = dictType;
                        previousResolvedType = dictType.ValueType;
                    }
                    else
                    {
                        dictionaryType = type as DatTypeWithProperties;
                    }

                    current = dict;
                    if (dictionaryType == null)
                        return false;
                    break;

                case IListSourceNode list:
                    lastListValue = list;
                    break;

                default:
                    return false;
            }
        }

        throw new InvalidProgramException("Unreachable");
    }

    /// <inheritdoc />
    public bool Equals(PropertyBreadcrumbs other) => Equals(in other);
    public bool Equals(in PropertyBreadcrumbs other)
    {
        if (other.Length != Length) return false;
        if (_sections == null || _sections.Length == 0)
        {
            return other._sections == null || other._sections.Length == 0;
        }

        if (other._sections == null || other._sections.Length == 0)
            return false;

        for (int i = 0; i < _sections.Length; ++i)
        {
            if (!_sections[i].Equals(other._sections[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is PropertyBreadcrumbs c && Equals(in c);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        int hashCode = 0;
        if (_sections != null)
        {
            unchecked
            {
                for (int i = 0; i < _sections.Length; ++i)
                {
                    hashCode ^= _sections[i].GetHashCode() * 397;
                }

                hashCode += _sections.Length * 397;
            }
        }

        return hashCode;
    }

    /// <inheritdoc />
    public override string ToString() => ToString(true);
    public string ToString(bool rootSlash, string? property = null)
    {
        if (_sections == null || _sections.Length == 0)
        {
            if (property != null)
                return rootSlash ? "/" + property : property;
            return rootSlash ? "/" : string.Empty;
        }

        if (_sections.Length == 1)
        {
            if (property != null)
                return rootSlash ? ("/" + _sections[0] + "/" + property) : (_sections[0] + "/" + property);
            return rootSlash ? ("/" + _sections[0] + "/") : (_sections[0] + "/");
        }

        StringBuilder sb = new StringBuilder(_sections.Length * 16);
        for (int i = 0; i < _sections.Length; ++i)
        {
            sb.Append('/');
            _sections[i].WriteToStringBuilder(sb);
        }

        sb.Append('/');
        if (property != null)
            sb.Append(property);
        return rootSlash ? sb.ToString() : sb.ToString(1, sb.Length - 1);
    }
}

/// <summary>
/// A section in a <see cref="PropertyBreadcrumbs"/> list.
/// </summary>
/// <remarks>Lists can optionally define an <see cref="Index"/>, or -1.</remarks>
public struct PropertyBreadcrumbSection : IEquatable<PropertyBreadcrumbSection>
{
    // index or -1
    public int Index;

    // either null, DatProperty, or string (for generic dictionary)
    public readonly object? Property;
    
    // either list or property
    public readonly ISourceNode? ParentNode;
    
    public readonly PropertyResolutionContext Context;

    /// <summary>
    /// Whether or not this section is a 'root' reference, as in it's instructs the path to start at the root of the file.
    /// </summary>
    // ReSharper disable once MergeIntoPattern
    public readonly bool IsRootReference => Property is string { Length: 1 } str && str[0] == '~';
    
    internal PropertyBreadcrumbSection(in PropertyBreadcrumbSection other, object? newProperty)
    {
        Index = other.Index;
        Property = newProperty;
        ParentNode = other.ParentNode;
        Context = other.Context;
    }

    public PropertyBreadcrumbSection(PropertyResolutionContext context, int index)
    {
        Context = context;
        Index = index;
    }

    public PropertyBreadcrumbSection(DatProperty property, PropertyResolutionContext context)
    {
        Property = property;
        Context = context;
        Index = -1;
    }

    public PropertyBreadcrumbSection(DatProperty property, PropertyResolutionContext context, int index)
    {
        Property = property;
        Context = context;
        Index = index;
    }

    public PropertyBreadcrumbSection(string key, PropertyResolutionContext context)
    {
        Property = key;
        Context = context;
        Index = -1;
    }

    public PropertyBreadcrumbSection(string key, PropertyResolutionContext context, int index)
    {
        Property = key;
        Context = context;
        Index = index;
    }

    public PropertyBreadcrumbSection(ISourceNode parentNode, PropertyResolutionContext context, string? propertyName)
    {
        ParentNode = parentNode;
        Context = context;
        Index = -1;
        Property = propertyName;
    }

    public PropertyBreadcrumbSection(ISourceNode parentNode, PropertyResolutionContext context, int index, string? propertyName)
    {
        ParentNode = parentNode;
        Context = context;
        Index = index;
        Property = propertyName;
    }

    internal readonly IAnyChildrenSourceNode? GetValueNode()
    {
        IAnyValueSourceNode? value = ParentNode switch
        {
            IPropertySourceNode property => property.Value,
            _ => ParentNode as IAnyValueSourceNode
        };

        if (value == null)
            return null;

        if (value is IListSourceNode list)
        {
            if (Index >= 0)
            {
                return list.TryGetElement(Index, out IAnyValueSourceNode? av) ? av as IAnyChildrenSourceNode : null;
            }
            if (Index < -1)
            {
                return list.TryGetElement(list.Count + Index + 1, out IAnyValueSourceNode? av) ? av as IAnyChildrenSourceNode : null;
            }
        }

        return value as IAnyChildrenSourceNode;
    }

    internal readonly IAnyChildrenSourceNode? GetValueNode(IAnyChildrenSourceNode parentNode, ref FileEvaluationContext ctx)
    {
        if (ParentNode != null && ReferenceEquals(parentNode.File, ParentNode.File))
            return GetValueNode();

        IAnyValueSourceNode? value;
        switch (parentNode)
        {
            case IDictionarySourceNode dict:
                IPropertySourceNode? propSrc;
                switch (Property)
                {
                    default:
                        return null;

                    case string str:
                        if (!dict.TryGetProperty(str, out propSrc))
                            return null;
                        break;

                    case DatProperty prop:
                        if (!dict.TryGetProperty(prop, ref ctx, out propSrc, LegacyExpansionFilter.Modern))
                            return null;
                        break;
                }
                value = propSrc.Value;
                break;

            default:
                value = parentNode;
                break;
        }

        if (value == null)
            return null;

        if (value is IListSourceNode list)
        {
            if (Index >= 0)
            {
                return list.TryGetElement(Index, out IAnyValueSourceNode? av) ? av as IAnyChildrenSourceNode : null;
            }
            if (Index < -1)
            {
                return list.TryGetElement(list.Count + Index + 1, out IAnyValueSourceNode? av) ? av as IAnyChildrenSourceNode : null;
            }
        }

        return value as IAnyChildrenSourceNode;
    }

    /// <inheritdoc />
    public readonly bool Equals(PropertyBreadcrumbSection other) => Equals(in other);

    public readonly bool Equals(in PropertyBreadcrumbSection other) => Index == other.Index
                                                                       && (Property?.Equals(other.Property) ?? (other.Property == null))
                                                                       && Context == other.Context
                                                                       && ReferenceEquals(ParentNode, other.ParentNode);

    /// <inheritdoc />
    public readonly override bool Equals(object? obj) => obj is PropertyBreadcrumbSection other && Equals(in other);

    /// <inheritdoc />
    public readonly override int GetHashCode()
    {
        unchecked
        {
            return (Index * 397) ^ (Property?.GetHashCode() ?? 0) ^ ((int)Context * 397) ^ (ParentNode?.GetHashCode() ?? 0);
        }
    }

    private readonly string GetKey()
    {
        return ParentNode switch
        {
            IPropertySourceNode prop => prop.Key,
            null => Property as string ?? (Property as DatProperty)?.Key ?? string.Empty,
            _ => string.Empty
        };
    }

    /// <inheritdoc />
    public readonly override string ToString()
    {
        string key = GetKey();
        if (Index < -1)
            return key + "[^" + -(Index + 1) + "]";
        return Index >= 0 ? key + "[" + Index + "]" : key;
    }

    internal readonly void WriteToStringBuilder(StringBuilder sb)
    {
        sb.Append(GetKey());
        if (Index >= 0)
        {
            sb.Append('[').Append(Index).Append(']');
        }
        else if (Index < -1)
        {
            sb.Append("[^").Append(-(Index + 1)).Append(']');
        }
    }
}
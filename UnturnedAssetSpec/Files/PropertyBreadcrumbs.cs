using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// Create breadcrums from a specific property or list value node.
    /// </summary>
    public static PropertyBreadcrumbs FromNode(ISourceNode node)
    {
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
            ISourceNode parent = node.Parent;
            for (int i = 0; i < parentCt; ++i)
            {
                switch (parent)
                {
                    case IListSourceNode list:
                        ISourceNode n = list.Parent as IPropertySourceNode ?? (ISourceNode)list;
                        sections[--sectionCt] = new PropertyBreadcrumbSection(n, PropertyResolutionContext.Modern, lastIndex);
                        lastIndex = parent.Index;
                        break;

                    case IDictionarySourceNode dict:
                        if (dict.Parent is IPropertySourceNode prop)
                        {
                            sections[--sectionCt] = new PropertyBreadcrumbSection(prop, PropertyResolutionContext.Modern);
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
    /// Try to find the property in <paramref name="file"/> in the dictionary this breadcrumb points to.
    /// </summary>
    public bool TryGetProperty(ISourceFile file, SpecProperty property, [MaybeNullWhen(false)] out IPropertySourceNode propertyNode, ICollection<ISourceNode>? nodePath = null)
    {
        lock (file.TreeSync)
        {
            if (IsRoot)
            {
                return file.TryResolveProperty(property, out propertyNode, PropertyResolutionContext.Modern);
            }

            AssetFileType fileType = default;
            if (!TryGetDictionaryAndTypeIntl(file, out IDictionarySourceNode? dictionary, null, in fileType, out _, nodePath))
            {
                propertyNode = null;
                return false;
            }

            return dictionary.TryGetProperty(property, out propertyNode, PropertyResolutionContext.Modern);
        }
    }

    /// <summary>
    /// Try to find the the object type this breadcrumb points to.
    /// </summary>
    public bool TryGetDictionaryAndType(ISourceFile file, in AssetFileType fileType, IAssetSpecDatabase database, [MaybeNullWhen(false)] out IDictionarySourceNode dictionary, [MaybeNullWhen(false)] out ISpecType type, ICollection<ISourceNode>? nodePath = null)
    {
        if (database == null)
            throw new ArgumentNullException(nameof(database));
        lock (file.TreeSync)
        {
            return TryGetDictionaryAndTypeIntl(file, out dictionary, database, in fileType, out type, nodePath) && type != null;
        }
    }

    /// <summary>
    /// Try to find the the dictionary this breadcrumb points to.
    /// </summary>
    public bool TryGetDictionary(ISourceFile file, [MaybeNullWhen(false)] out IDictionarySourceNode dictionary, ICollection<ISourceNode>? nodePath = null)
    {
        lock (file.TreeSync)
        {
            AssetFileType type = default;
            return TryGetDictionaryAndTypeIntl(file, out dictionary, null, in type, out _, nodePath);
        }
    }

    private bool TryGetDictionaryAndTypeIntl(ISourceFile file, [MaybeNullWhen(false)] out IDictionarySourceNode referencedDictionary, IAssetSpecDatabase? database, in AssetFileType fileType, out ISpecType? type, ICollection<ISourceNode>? nodePath)
    {
        type = null;
        if (IsRoot)
        {
            referencedDictionary = file;
            if (database != null)
            {
                type = fileType.Information;
            }
            return true;
        }

        SpecPropertyContext context = file is ILocalizationSourceFile
            ? SpecPropertyContext.Localization
            : SpecPropertyContext.Property;

        ref PropertyBreadcrumbSection lastSection = ref _sections[^1];
        if (database == null && nodePath == null && lastSection.ParentNode != null)
        {
            referencedDictionary = lastSection.GetValueNode() as IDictionarySourceNode;
            return referencedDictionary != null;
        }

        int startIndex = 0;
        if (database == null && nodePath == null)
        {
            for (int i = _sections.Length - 1; i >= 0; --i)
            {
                ISourceNode? node = _sections[i].ParentNode;
                if (node != null && ReferenceEquals(node.Parent, file))
                {
                    startIndex = i;
                    break;
                }
            }
        }
        
        IAnyChildrenSourceNode? dictionary = file;
        if (dictionary is IAssetSourceFile a)
            dictionary = a.AssetData;

        ISpecType? typeOwner = fileType.Information;
        int skipType = 0;

        for (int i = startIndex; i < _sections.Length; ++i)
        {
            ref PropertyBreadcrumbSection section = ref _sections[i];
            if (database != null && typeOwner != null && skipType <= 0)
            {
                SpecProperty? property = section.Property as SpecProperty;
                if (property == null && section.ParentNode is IPropertySourceNode propNode)
                {
                    property = database.FindPropertyInfoByKey(propNode.Key, typeOwner, PropertyResolutionContext.Modern, requireCanBeInMetadata: false, context).Property;
                }

                if (property == null)
                    typeOwner = null;
                else
                {
                    ISpecPropertyType? typeOrListType = property.Type.Type;
                    skipType = 0;
                    typeOwner = null;
                    while (typeOrListType != null)
                    {
                        switch (typeOrListType)
                        {
                            case ISpecType t:
                                typeOwner = t;
                                break;

                            case IListTypeSpecPropertyType list:
                                ISpecPropertyType? innerType = list.GetInnerType();
                                if (innerType == null)
                                    break;
                                ++skipType;
                                typeOrListType = innerType;
                                continue;

                            case IDictionaryTypeSpecPropertyType dict:
                                innerType = dict.GetInnerType(database);
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

            dictionary = section.GetValueNode(dictionary);
            if (dictionary == null)
            {
                referencedDictionary = null;
                return false;
            }

            if (skipType > 0 && dictionary is IDictionarySourceNode { Parent: IListSourceNode })
                --skipType;

            if (nodePath != null)
            {
                if (dictionary.Parent is IListSourceNode l)
                {
                    if (l.Parent is IPropertySourceNode prop)
                        nodePath.Add(prop);
                    nodePath.Add(dictionary);
                }
                else if (dictionary.Parent is IPropertySourceNode prop)
                    nodePath.Add(prop);
                else
                    nodePath.Add(dictionary);
            }
        }

        type = skipType > 0 ? null : typeOwner;
        referencedDictionary = dictionary as IDictionarySourceNode;
        return referencedDictionary != null;
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

    // either null, SpecProperty, or string (for generic dictionary)
    public readonly object? Property;
    
    // either list or property
    public readonly ISourceNode? ParentNode;
    
    public readonly PropertyResolutionContext Context;

    public PropertyBreadcrumbSection(PropertyResolutionContext context, int index)
    {
        Context = context;
        Index = index;
    }

    public PropertyBreadcrumbSection(SpecProperty property, PropertyResolutionContext context)
    {
        Property = property;
        Context = context;
        Index = -1;
    }

    public PropertyBreadcrumbSection(SpecProperty property, PropertyResolutionContext context, int index)
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

    public PropertyBreadcrumbSection(ISourceNode parentNode, PropertyResolutionContext context)
    {
        ParentNode = parentNode;
        Context = context;
        Index = -1;
    }

    public PropertyBreadcrumbSection(ISourceNode parentNode, PropertyResolutionContext context, int index)
    {
        ParentNode = parentNode;
        Context = context;
        Index = index;
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

        if (Index >= 0 && value is IListSourceNode list)
        {
            return list.TryGetElement(Index, out IAnyValueSourceNode? av) ? av as IAnyChildrenSourceNode : null;
        }

        return value as IAnyChildrenSourceNode;
    }

    internal readonly IAnyChildrenSourceNode? GetValueNode(IAnyChildrenSourceNode parentNode)
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

                    case SpecProperty prop:
                        if (!dict.TryGetProperty(prop, out propSrc, PropertyResolutionContext.Modern))
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

        if (Index >= 0 && value is IListSourceNode list)
        {
            return list.TryGetElement(Index, out IAnyValueSourceNode? av) ? av as IAnyChildrenSourceNode : null;
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
            null => Property as string ?? (Property as SpecProperty)?.Key ?? string.Empty,
            _ => string.Empty
        };
    }

    /// <inheritdoc />
    public readonly override string ToString()
    {
        string key = GetKey();
        return Index >= 0 ? key + "[" + Index + "]" : key;
    }

    internal readonly void WriteToStringBuilder(StringBuilder sb)
    {
        sb.Append(GetKey());
        if (Index >= 0)
        {
            sb.Append('[').Append(Index).Append(']');
        }
    }
}
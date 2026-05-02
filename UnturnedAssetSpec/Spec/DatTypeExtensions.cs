using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Immutable;
using System.ComponentModel;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Extension methods for the various subclasses of <see cref="DatType"/>.
/// </summary>
public static class DatTypeExtensions
{
    extension(DatType type)
    {
        /// <inheritdoc cref="IDatTypeWithStringParseableType{T}.StringParseableType"/>
        public QualifiedType StringParseableType
        {
            get
            {
                GetStringParseableTypeVisitor v;
                v.Value = QualifiedType.None;
                
                type.Visit(ref v);

                return v.Value;
            }
        }

        /// <summary>
        /// Gets the properties of the given type, or an empty array if that type doesn't have that kind of properties.
        /// </summary>
        /// <param name="type">The type to get properties from.</param>
        /// <param name="context">The kind of properties to get.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="context"/> is asking for <see cref="SpecPropertyContext.BundleAsset"/> properties which are not returned by this function.</exception>
        /// <exception cref="InvalidEnumArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        public ImmutableArray<DatProperty> GetPropertyArray(SpecPropertyContext context)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            switch (context)
            {
                case SpecPropertyContext.Unspecified:
                case SpecPropertyContext.Property:
                case SpecPropertyContext.CrossReferenceProperty:
                case SpecPropertyContext.CrossReferenceUnspecified:
                    if (type is DatTypeWithProperties props)
                        return props.Properties;
                    break;

                case SpecPropertyContext.Localization:
                case SpecPropertyContext.CrossReferenceLocalization:
                    if (type is IDatTypeWithLocalizationProperties lclProps)
                        return lclProps.LocalizationProperties;
                    break;

                case SpecPropertyContext.BundleAsset:
                    throw new ArgumentOutOfRangeException(nameof(context));

                default:
                    throw new InvalidEnumArgumentException(nameof(context), (int)context, typeof(SpecPropertyContext));
            }

            return ImmutableArray<DatProperty>.Empty;
        }
    }

    extension(DatTypeWithProperties type)
    {
        // todo is this needed?
    //    /// <summary>
    //    /// Try to get a property by it's key or alias.
    //    /// </summary>
    //    public bool TryFindBestPropertyCandidate(
    //        IPropertySourceNode propertyNode,
    //        ref FileEvaluationContext ctx,
    //        IList<PropertySearchCandidate> candidates,
    //        [NotNullWhen(true)] out DatProperty? bestMatch,
    //        [NotNullWhen(true)] out IType? bestMatchPropertyType,
    //        SpecPropertyContext context = SpecPropertyContext.Unspecified,
    //        LegacyExpansionFilter filter = LegacyExpansionFilter.Either
    //    )
    //    {
    //        IEnumerator<PropertySearchCandidate> enumerator = type.EnumeratePropertyCandidates(
    //            propertyNode, context, filter
    //        );
    //        while (enumerator.MoveNext())
    //        {
    //            PropertySearchCandidate candidate = enumerator.Current;
    //            DatProperty property = candidate.Property;

    //            if (!property.Type.TryEvaluateType(out IType? propertyType, ref ctx))
    //            {
    //                continue;
    //            }

    //            if (candidate.IsKeyMatch)
    //            {
    //                if (candidate.AliasIndex >= 0)
    //                {
    //                    DatPropertyKey key = property.Keys[candidate.AliasIndex];
    //                    if (key.Condition != null && !(key.Condition.TryEvaluateValue(out Optional<bool> cond, ref ctx) && cond.Value))
    //                    {
    //                        // fails key condition
    //                        continue;
    //                    }
    //                }

    //                (bestMatch, bestMatchPropertyType) = (property, propertyType);
    //                return true;
    //            }

    //            break;

    //            // todo: ideally this would search for lists, objects, etc and recognize them
    //            //       but i dont think that will be necessary

    //            //if (propertyType is not IReverseLookupPropertyType reverseLookup)
    //            //{
    //            //    continue;
    //            //}
    //            //
    //            //if (!reverseLookup.TryReverseLookupProperty(propertyNode, context, filter))
    //            //{
    //            //    continue;
    //            //}

    //            //(bestMatch, bestMatchPropertyType) = (property, propertyType);
    //            //return true;
    //        }

    //        enumerator.Dispose();
    //        (bestMatch, bestMatchPropertyType) = (null, null);
    //        return false;
    //    }

    //    /// <summary>
    //    /// Try to get a property by it's key or alias.
    //    /// </summary>
    //    public IEnumerator<PropertySearchCandidate> EnumeratePropertyCandidates(
    //        IPropertySourceNode propertyNode,
    //        SpecPropertyContext context = SpecPropertyContext.Unspecified,
    //        LegacyExpansionFilter filter = LegacyExpansionFilter.Either
    //    )
    //    {
    //        if (propertyNode == null)
    //            throw new ArgumentNullException(nameof(propertyNode));

    //        context = context switch
    //        {
    //            SpecPropertyContext.Property or SpecPropertyContext.CrossReferenceProperty => SpecPropertyContext.Property,
    //            SpecPropertyContext.Localization or SpecPropertyContext.CrossReferenceLocalization => SpecPropertyContext.Localization,
    //            _ => propertyNode.File is ILocalizationSourceFile ? SpecPropertyContext.Localization : SpecPropertyContext.Property
    //        };

    //        EnumeratePropertyCandidatesArgs args = default;
    //        if (propertyNode.File is IAssetSourceFile assetFile)
    //        {
    //            if (propertyNode.File == propertyNode.Parent && propertyNode.File is IAssetSourceFile)
    //            {
    //                args.IsAsset = string.Equals(propertyNode.Key, "Asset", StringComparison.OrdinalIgnoreCase);
    //                args.IsMetadata = !args.IsAsset && string.Equals(propertyNode.Key, "Metadata", StringComparison.OrdinalIgnoreCase);
    //            }

    //            IDictionarySourceNode? assetData = assetFile.GetAssetDataDictionary();
    //            IDictionarySourceNode? metadata = assetFile.GetMetadataDictionary();
    //            if (propertyNode.File != propertyNode.Parent)
    //            {
    //                if (propertyNode.Parent == assetData)
    //                {
    //                    args.Position = AssetDatPropertyPosition.Asset;
    //                }
    //                else if (propertyNode.Parent == metadata)
    //                {
    //                    args.Position = AssetDatPropertyPosition.Metadata;
    //                }
    //            }

    //            args.HasAssetData = assetData != null;
    //            args.HasMetadata = metadata != null;
    //        }

    //        return type.EnumeratePropertyCandidatesIntl(propertyNode, context, filter, args, -1);
    //    }

    //    private IEnumerator<PropertySearchCandidate> EnumeratePropertyCandidatesIntl(
    //        IPropertySourceNode propertyNode,
    //        SpecPropertyContext context,
    //        LegacyExpansionFilter filter,
    //        EnumeratePropertyCandidatesArgs args,
    //        int step
    //    )
    //    {
    //        string key = propertyNode.Key;

    //        ImmutableArray<DatProperty> properties;
    //        LightweightBitArray bits;
    //        if (step <= 0)
    //        {
    //            properties = type.GetPropertyArray(context);
    //            bits = new LightweightBitArray(properties.Length);
    //            for (int index = 0; index < properties.Length; index++)
    //            {
    //                DatProperty property = properties[index];
    //                if (property.IsImport)
    //                    continue;

    //                if (!property.AssetPosition.IsValidPosition(args.Position, args.HasAssetData, args.HasMetadata))
    //                    continue;

    //                // Asset/Metadata properties
    //                if (property.Type is NullType)
    //                {
    //                    if (args.IsAsset && property.Key.Equals("Asset", StringComparison.OrdinalIgnoreCase)
    //                        || args.IsMetadata && property.Key.Equals("Metadata", StringComparison.OrdinalIgnoreCase))
    //                    {
    //                        yield return new PropertySearchCandidate(property);
    //                        yield break;
    //                    }

    //                    continue;
    //                }

    //                bits[index] = true;
    //                if (property.Keys.IsDefaultOrEmpty)
    //                {
    //                    if (!string.Equals(key, property.Key, StringComparison.OrdinalIgnoreCase))
    //                        continue;

    //                    yield return new PropertySearchCandidate(property, -1);
    //                }
    //                else
    //                {
    //                    ImmutableArray<DatPropertyKey> keys = property.Keys;
    //                    for (int i = 0; i < keys.Length; ++i)
    //                    {
    //                        DatPropertyKey alias = keys[i];

    //                        if (filter != LegacyExpansionFilter.Either && alias.Filter != LegacyExpansionFilter.Either &&
    //                            alias.Filter != filter)
    //                        {
    //                            continue;
    //                        }

    //                        if (!string.Equals(key, alias.Key, StringComparison.OrdinalIgnoreCase))
    //                        {
    //                            continue;
    //                        }

    //                        yield return new PropertySearchCandidate(property);
    //                    }
    //                }
    //            }

    //            //ImmutableArray<DatProperty> allProperties = type.GetPropertyArray(SpecPropertyContext.Property);
    //            //foreach (DatProperty property in allProperties)
    //            //{
    //            //    if (!property.TryGetImportType(out DatTypeWithProperties? propType))
    //            //        continue;
    //            //
    //            //    IEnumerator<PropertySearchCandidate> enumerator = propType.EnumeratePropertyCandidatesIntl(
    //            //        propertyNode, context, filter, args, step
    //            //    );
    //            //
    //            //    while (enumerator.MoveNext())
    //            //    {
    //            //        yield return enumerator.Current;
    //            //    }
    //            //
    //            //    enumerator.Dispose();
    //            //}
    //        }
    //        else
    //        {
    //            bits = default;
    //            properties = ImmutableArray<DatProperty>.Empty;
    //        }

    //        if (step is < 0 or 1)
    //        {
    //            // search for advanced matches
    //            for (int index = 0; index < properties.Length; index++)
    //            {
    //                DatProperty property = properties[index];
    //                if (step < 0)
    //                {
    //                    if (!bits[index])
    //                        continue;
    //                }
    //                else
    //                {
    //                    if (property.IsImport
    //                        || !property.AssetPosition.IsValidPosition(args.Position, args.HasAssetData, args.HasMetadata)
    //                        || property.Type is NullType)
    //                    {
    //                        continue;
    //                    }
    //                }

    //                PropertySearchTrimmingBehavior trimType = property.Type.TrimmingBehavior;
    //                if (trimType == PropertySearchTrimmingBehavior.ExactPropertyOnly)
    //                    continue;

    //                yield return new PropertySearchCandidate(property, isKeyMatch: false);
    //            }

    //            //ImmutableArray<DatProperty> allProperties = type.GetPropertyArray(SpecPropertyContext.Property);
    //            //foreach (DatProperty property in allProperties)
    //            //{
    //            //    if (!property.TryGetImportType(out DatTypeWithProperties? propType))
    //            //        continue;
    //            //
    //            //    IEnumerator<PropertySearchCandidate> enumerator = propType.EnumeratePropertyCandidatesIntl(
    //            //        propertyNode, context, filter, args, step
    //            //    );
    //            //
    //            //    while (enumerator.MoveNext())
    //            //    {
    //            //        yield return enumerator.Current;
    //            //    }
    //            //
    //            //    enumerator.Dispose();
    //            //}
    //        }

    //        if (step is < 0 or 2)
    //        {
    //            ImmutableArray<DatProperty> otherProperties;
    //            if (context == SpecPropertyContext.Property)
    //            {
    //                otherProperties = type is IDatTypeWithLocalizationProperties lcl
    //                    ? lcl.LocalizationProperties
    //                    : ImmutableArray<DatProperty>.Empty;
    //            }
    //            else
    //            {
    //                otherProperties = type.Properties;
    //            }

    //            if (otherProperties.Length > 0)
    //            {
    //                for (int i = 0; i < otherProperties.Length; ++i)
    //                {
    //                    DatProperty prop = otherProperties[i];
    //                    if (prop.IsImport
    //                        || prop.Type is NullType
    //                        || prop.Type.TrimmingBehavior != PropertySearchTrimmingBehavior.CreatesOtherPropertiesInLinkedFiles)
    //                    {
    //                        continue;
    //                    }

    //                    yield return new PropertySearchCandidate(prop, isKeyMatch: false);
    //                }
    //            }
    //        }
    //    }
    }

    private struct GetStringParseableTypeVisitor : ITypeVisitor
    {
        public QualifiedType Value;

        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            if (type is IDatTypeWithStringParseableType<TValue> parseableType)
            {
                Value = parseableType.StringParseableType;
            }
        }
    }


    //private struct EnumeratePropertyCandidatesArgs
    //{
    //    public int Flags;
    //    public AssetDatPropertyPosition Position;

    //    public bool IsAsset
    //    {
    //        get => (Flags & 1) != 0;
    //        set
    //        {
    //            if (value) Flags |= 1;
    //            else Flags &= ~1;
    //        }
    //    }

    //    public bool IsMetadata
    //    {
    //        get => (Flags & 2) != 0;
    //        set
    //        {
    //            if (value) Flags |= 2;
    //            else Flags &= ~2;
    //        }
    //    }

    //    public bool HasAssetData
    //    {
    //        get => (Flags & 4) != 0;
    //        set
    //        {
    //            if (value) Flags |= 4;
    //            else Flags &= ~4;
    //        }
    //    }

    //    public bool HasMetadata
    //    {
    //        get => (Flags & 8) != 0;
    //        set
    //        {
    //            if (value) Flags |= 8;
    //            else Flags &= ~8;
    //        }
    //    }
    //}
}

public struct PropertySearchCandidate
{
    public DatProperty Property { get; }

    /// <summary>
    /// Indices of conditions that need checked.
    /// </summary>
    public int AliasIndex { get; }

    /// <summary>
    /// Whether or not this property's key matched.
    /// </summary>
    public bool IsKeyMatch { get; }

    public PropertySearchCandidate(DatProperty property, int aliasIndex = -1, bool isKeyMatch = true)
    {
        Property = property;
        AliasIndex = aliasIndex;
        IsKeyMatch = isKeyMatch;
    }
}
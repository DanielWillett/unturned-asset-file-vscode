using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// A property that can be defined in the file of a <see cref="DatFileType"/> or within an instance of a custom type.
/// </summary>
[DebuggerDisplay("{GetDebuggerDisplay()}")]
public class DatProperty : IDatSpecificationObject
{
    /// <summary>
    /// Can either be a <see cref="Tuple{T1,T2}"/>, <see cref="ImmutableDictionary{TKey,TValue}"/>, or <see cref="ImmutableDictionary{TKey,TValue}.Builder"/> with a <see cref="PropertyOrderFile.TypeKey"/> key and <see cref="int"/> value.
    /// </summary>
    private object? _indicesOrBuilder;
    private int _indicesType;

    /// <inheritdoc />
    public JsonElement DataRoot { get; }

    /// <summary>
    /// The context of this property.
    /// </summary>
    public SpecPropertyContext Context { get; }

    /// <summary>
    /// The type that defines this property.
    /// </summary>
    public DatTypeWithProperties Owner { get; }

    /// <summary>
    /// The primary key for this property.
    /// </summary>
    /// <remarks>Corresponds to the <c>Key</c> property.</remarks>
    public string Key { get; }

    /// <summary>
    /// The property being overridden by this property.
    /// </summary>
    public DatProperty? OverriddenProperty { get; internal set; }

    /// <summary>
    /// Whether or not this property is hiding <see cref="OverriddenProperty"/>.
    /// </summary>
    /// <remarks>Corresponds to the <c>HideInherited</c> property.</remarks>
    [MemberNotNullWhen(true, nameof(OverriddenProperty))]
    public bool HideOverridden { get; internal set; }

    /// <summary>
    /// Whether or not this property imports properties from <see cref="Type"/>.
    /// </summary>
    internal bool IsImport { get; set; }

    /// <summary>
    /// The expected location of this property when it's in an asset.
    /// </summary>
    /// <remarks>Corresponds to the <c>AssetPosition</c> property.</remarks>
    public AssetDatPropertyPositionExpectation AssetPosition { get; internal set; }

    /// <summary>
    /// Whether or not this property has to be in the Metadata section if it exists.
    /// </summary>
    /// <remarks>Corresponds to the <c>MustBeInMetadata</c> property.</remarks>
    public bool MustBeInMetadata { get; internal set; }

    /// <summary>
    /// Type of value this property stores.
    /// </summary>
    /// <remarks>Corresponds to the <c>Type</c> property.</remarks>
    public IPropertyType Type { get; internal set; }

    /// <summary>
    /// List of available keys, if extra information is given for any.
    /// </summary>
    /// <remarks>If this is default or empty, refer to <see cref="Key"/>. Corresponds to the <c>Alias</c>/<c>Aliases</c> properties, but also includes the original key.</remarks>
    public ImmutableArray<DatPropertyKey> Keys { get; internal set; }

    /// <summary>
    /// URL to the SDG docs for this property.
    /// </summary>
    /// <remarks>Corresponds to the <c>Docs</c> property.</remarks>
    public IValue<string>? Docs { get; internal set; }

    /// <summary>
    /// Description of how this property works.
    /// </summary>
    /// <remarks>Corresponds to the <c>Description</c> property.</remarks>
    public IValue<string>? Description { get; internal set; }

    /// <summary>
    /// Description of how this property works in markdown.
    /// </summary>
    /// <remarks>Corresponds to the <c>Markdown</c> property.</remarks>
    public IValue<string>? MarkdownDescription { get; internal set; }

    /// <summary>
    /// Name of the C# variable this property assigns. May not line up 1:1, ex. a boolean variable may be the inverse of the property value.
    /// </summary>
    /// <remarks>Corresponds to the <c>Variable</c> property.</remarks>
    public IValue<string>? Variable { get; internal set; }

    /// <summary>
    /// The version of Unturned this property was added in.
    /// </summary>
    /// <remarks>Corresponds to the <c>Version</c> property.</remarks>
    public IValue<Version>? Version { get; internal set; }

    /// <summary>
    /// Whether or not this property is deprecated/obsolete.
    /// </summary>
    /// <remarks>Corresponds to the <c>Deprecated</c> property.</remarks>
    public IValue<bool>? Deprecated { get; internal set; }

    /// <summary>
    /// Whether or not this property is an experimental property that may be removed/tweaked in the future.
    /// </summary>
    /// <remarks>Corresponds to the <c>Experimental</c> property.</remarks>
    public IValue<bool>? Experimental { get; internal set; }

    /// <summary>
    /// Whether or not this property must exist for the current object to make sense.
    /// </summary>
    /// <remarks>Corresponds to the <c>Required</c> property.</remarks>
    public IValue<bool>? Required { get; internal set; }

    /// <summary>
    /// Designates a property that selects the cross-ref file for any properties starting with <c>$cr$::</c>.
    /// </summary>
    /// <remarks>Corresponds to the <c>FileCrossRef</c> property.</remarks>
    public IValue? CrossReferenceTarget { get; internal set; }

    /// <summary>
    /// Reference to a set of values from a list. Example: 'Blueprints.Id'.
    /// </summary>
    /// <remarks>Corresponds to the <c>ListReference</c> property.</remarks>
    public IValue? AvailableValuesTarget { get; internal set; }

    /// <summary>
    /// Minimum value of this property.
    /// </summary>
    /// <remarks>Corresponds to the <c>Minimum</c> or <c>MinimumExclusive</c> properties.</remarks>
    public IValue? Minimum { get; internal set; }

    /// <summary>
    /// Maximum value of this property.
    /// </summary>
    /// <remarks>Corresponds to the <c>Maximum</c> or <c>MaximumExclusive</c> properties.</remarks>
    public IValue? Maximum { get; internal set; }

    /// <summary>
    /// Whether or not <see cref="Minimum"/> is exclusive.
    /// </summary>
    public bool MinimumIsExclusive { get; internal set; }

    /// <summary>
    /// Whether or not <see cref="Maximum"/> is exclusive.
    /// </summary>
    public bool MaximumIsExclusive { get; internal set; }

    /// <summary>
    /// Exceptions to the minimum/maximum range if specified, otherwise a value blacklist.
    /// </summary>
    /// <remarks>Corresponds to the <c>Except</c> property.</remarks>
    public ImmutableArray<IValue> Exceptions { get; internal set; }

    /// <summary>
    /// When <see cref="AvailableValuesTarget"/> is set, indicates that it's an error to not reference a value from that target.
    /// </summary>
    /// <remarks>Corresponds to whether or not the <c>ListReference</c> property ends in an exclamation point.</remarks>
    public bool AvailableValuesTargetIsRequired { get; internal set; }

    /// <summary>
    /// Name of the template group that this property defines a count for.
    /// </summary>
    /// <remarks>Corresponds to the <c>CountForTemplateGroup</c> property.</remarks>
    public string? CountForTemplateGroup { get; internal set; }

    /// <summary>
    /// The default value for this property when the property is not included.
    /// </summary>
    /// <remarks>Corresponds to the <c>DefaultValue</c> property.</remarks>
    public IValue? DefaultValue { get; internal set; }

    /// <summary>
    /// Indicates that the value of **this property** should change this object's type based on this property in the enum.
    /// The specified type should be a subtype of the current type.
    /// </summary>
    /// <remarks>Corresponds to the <c>SubtypeSwitch</c> property.</remarks>
    public string? SubtypeSwitchPropertyName { get; internal set; }

    /// <summary>
    /// Properties that can't exist with this property.
    /// </summary>
    /// <remarks>Corresponds to the <c>ExclusiveWith</c> property.</remarks>
    public ImmutableArray<IExclusionCondition> ExclusionConditions { get; internal set; }

    /// <summary>
    /// Properties that should exist with this property.
    /// </summary>
    /// <remarks>Corresponds to the <c>InclusiveWith</c> property.</remarks>
    public ImmutableArray<IInclusionCondition> InclusionConditions { get; internal set; }

    internal IValue? GetIncludedDefaultValue() => IncludedDefaultValue ?? DefaultValue;
    internal IValue? GetIncludedDefaultValue(bool hasProperty) => hasProperty ? IncludedDefaultValue ?? DefaultValue : DefaultValue;

    /// <summary>
    /// The default value for this property when the property is included without a parsable value. Defaults to <see cref="DefaultValue"/>.
    /// </summary>
    /// <remarks>Corresponds to the <c>IncludedDefaultValue</c> property.</remarks>
    public IValue? IncludedDefaultValue { get; internal set; }

    /// <summary>
    /// For config data, whether or not the field is decorated with the <c>[ConfigWarnIfTrue]</c> attribute, meaning a value of <see langword="true"/> will show a warning in the server menu.
    /// </summary>
    /// <remarks>Corresponds to the <c>ConfigWarnIfTrue</c> property.</remarks>
    public bool ServerMenuWarnsForTrueValues { get; internal set; }

    internal DatProperty(string key, DatTypeWithProperties owner, JsonElement element, SpecPropertyContext context)
    {
        Key = key;
        Owner = owner;
        DataRoot = element;
        Context = context;
        Type = null!;
    }

    /// <summary>
    /// Attempt to get the import type for this property.
    /// </summary>
    /// <returns><see langword="true"/> if this property is an import property and the type could be resolved to a concrete <see cref="DatTypeWithProperties"/>, otherwise <see langword="false"/>.</returns>
    internal bool TryGetImportType([NotNullWhen(true)] out DatTypeWithProperties? importType)
    {
        if (!IsImport || !Type.TryGetConcreteType(out IType? type))
        {
            importType = null;
            return false;
        }

        importType = type as DatTypeWithProperties;
        return importType != null;
    }

    /// <inheritdoc cref="Create(string,IPropertyType,DatTypeWithProperties,JsonElement,SpecPropertyContext)("/>
    internal static DatProperty Create(string key, DatTypeWithProperties owner, JsonElement element, SpecPropertyContext context)
    {
        return new DatProperty(
            key   ?? throw new ArgumentNullException(nameof(key)),
            owner ?? throw new ArgumentNullException(nameof(owner)),
            element,
            context
        );
    }

    /// <summary>
    /// Create a new <see cref="DatProperty"/> instance given a <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The primary key for this property.</param>
    /// <param name="type">The type of value this property stores.</param>
    /// <param name="owner">The type that defines this property.</param>
    /// <param name="element">The JSON element this property was read from.</param>
    /// <returns>The newly-created <see cref="DatProperty"/> instance.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static DatProperty Create(string key, IPropertyType type, DatTypeWithProperties owner, JsonElement element, SpecPropertyContext context)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        DatProperty property = Create(key, owner, element, context);
        property.Type = type;
        return property;
    }

    private const int CachedLocalizationStringTypes = 8;
    private static readonly StringType?[] LocalizationStringTypeCache = new StringType?[CachedLocalizationStringTypes];

    /// <summary>
    /// Create a new <see cref="DatProperty"/> for a localization file.
    /// </summary>
    public static DatProperty CreateLocalizationKey(string key, string? value, DatTypeWithProperties owner)
    {
        int maxFormatArgument = 0;
        bool allowLineBreak = false;
        if (string.IsNullOrEmpty(value))
        {
            value = null;
        }
        else
        {
            allowLineBreak = value.Contains("<br>", StringComparison.Ordinal);
            maxFormatArgument = StringHelper.GetHighestFormattingArgument(value);
        }


        uint maxFormatArguments = maxFormatArgument < 0 ? 0u : (uint)(maxFormatArgument + 1);

        StringType type;
        if (allowLineBreak)
        {
            type = new StringType(0, int.MaxValue, true, true, maxFormatArguments, OneOrMore<Regex>.Null);
        }
        else if (maxFormatArguments < CachedLocalizationStringTypes)
        {
            type = LocalizationStringTypeCache[maxFormatArguments]
                ??= new StringType(0, int.MaxValue, true, false, maxFormatArguments, OneOrMore<Regex>.Null);
        }
        else
        {
            type = new StringType(0, int.MaxValue, true, false, maxFormatArguments, OneOrMore<Regex>.Null);
        }

        // note: these are not considered Localization properties since they're still in the main file.
        return new DatProperty(key, owner, default, SpecPropertyContext.Property)
        {
            DefaultValue = value == null ? null : Value.Create(value, type),
            Type = type
        };
    }

    /// <summary>
    /// Create a new <see cref="DatProperty"/> instance that hides a property in a parent type.
    /// </summary>
    /// <param name="overriding">The property to hide.</param>
    /// <param name="owner">The type that defines this property.</param>
    /// <returns>The newly-created <see cref="DatProperty"/> instance.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static DatProperty Hide(DatProperty overriding, DatTypeWithProperties owner, SpecPropertyContext context)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));
        if (overriding == null)
            throw new ArgumentNullException(nameof(overriding));

        return new DatProperty(overriding.Key, owner, default, context)
        {
            HideOverridden = true,
            OverriddenProperty = overriding,
            Type = overriding.Type
        };
    }

    internal bool TryAddIndex(PropertyOrderFile.TypeKey key, int index)
    {
        switch (_indicesType)
        {
            case 0:
                _indicesOrBuilder = new Tuple<PropertyOrderFile.TypeKey, int>(key, index);
                _indicesType = 1;
                return true;

            case 1:
                Tuple<PropertyOrderFile.TypeKey, int> tuple = (Tuple<PropertyOrderFile.TypeKey, int>)_indicesOrBuilder!;
                if (tuple.Item1.Equals(key))
                    return false;

                ImmutableDictionary<PropertyOrderFile.TypeKey, int>.Builder bldr2
                    = ImmutableDictionary.CreateBuilder<PropertyOrderFile.TypeKey, int>();
                
                bldr2.Add(tuple.Item1, tuple.Item2);
                bldr2.Add(key, index);

                _indicesOrBuilder = bldr2;
                _indicesType = 2;
                return true;

            case 2:
                ImmutableDictionary<PropertyOrderFile.TypeKey, int>.Builder bldr =
                    (ImmutableDictionary<PropertyOrderFile.TypeKey, int>.Builder)_indicesOrBuilder!;

                if (bldr.ContainsKey(key))
                    return false;
                
                bldr.Add(key, index);
                return true;

            default:
                return false;
        }
    }

    internal bool TryGetIndexInType(PropertyOrderFile.TypeKey key, out int index)
    {
        switch (_indicesType)
        {
            case 1:
                Tuple<PropertyOrderFile.TypeKey, int> tuple
                    = (Tuple<PropertyOrderFile.TypeKey, int>)_indicesOrBuilder!;
                if (!tuple.Item1.Equals(key))
                    goto default;

                index = tuple.Item2;
                return true;

            case 2:
                ImmutableDictionary<PropertyOrderFile.TypeKey, int>.Builder bldr
                    = (ImmutableDictionary<PropertyOrderFile.TypeKey, int>.Builder)_indicesOrBuilder!;
                return bldr.TryGetValue(key, out index);

            case 3:
                ImmutableDictionary<PropertyOrderFile.TypeKey, int> dict
                    = (ImmutableDictionary<PropertyOrderFile.TypeKey, int>)_indicesOrBuilder!;
                return dict.TryGetValue(key, out index);

            default:
                index = 0;
                return false;
        }
    }

    public readonly struct KeyMatch
    {
        public static readonly KeyMatch None = new KeyMatch(-1);

        public readonly int KeyIndex;
        public readonly OneOrMore<int> Indices;

        public KeyMatch(int keyIndex)
        {
            KeyIndex = keyIndex;
            Indices = OneOrMore<int>.Null;
        }

        public KeyMatch(int keyIndex, OneOrMore<int> indices)
        {
            KeyIndex = keyIndex;
            Indices = indices;
        }

        /// <summary>
        /// Gets information about the key that was matched.
        /// </summary>
        /// <param name="property">The original property that was searched.</param>
        public DatPropertyKey GetKeyInfo(DatProperty property)
        {
            ImmutableArray<DatPropertyKey> keys = property.Keys;
            if (keys.IsDefaultOrEmpty || keys.Length >= KeyIndex)
            {
                throw new ArgumentException("Property isn't the same as the one that was used in the search.", nameof(property));
            }

            return keys[KeyIndex];
        }

        /// <summary>
        /// Gets the normalized casing of the key that was matched, inserting template arguments where required.
        /// </summary>
        /// <param name="property">The original property that was searched.</param>
        public string GetKey(DatProperty property)
        {
            DatPropertyKey key = GetKeyInfo(property);
            if (key is not DatTemplatePropertyKey template)
                return key.Key;

            return template.TemplateProcessor.CreateKey(template.Key, Indices);
        }
    }

    /// <inheritdoc cref="MatchesKey(string,ref FileEvaluationContext,bool,out DatProperty.KeyMatch)"/>
    public virtual bool MatchesKey(string candidateKey, ref FileEvaluationContext ctx, out KeyMatch match)
    {
        return MatchesKey(candidateKey, ref ctx, true, out match);
    }

    /// <summary>
    /// Checks whether a not a property name could refer to this property.
    /// </summary>
    /// <param name="candidateKey">The property name.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="isCaseInsensitive">Whether or not the comparison should ignore case.
    /// Defaults to <see langword="true"/> on normal properties and <see langword="false"/> on bundle assets.</param>
    /// <param name="match">Information about the key that was matched.</param>
    /// <returns>Whether or not a key was matched.</returns>
    public virtual bool MatchesKey(string candidateKey, ref FileEvaluationContext ctx, bool isCaseInsensitive, out KeyMatch match)
    {
        StringComparison comparison = isCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        if (Keys.IsDefaultOrEmpty)
        {
            if (!Key.Equals(candidateKey, comparison))
            {
                match = KeyMatch.None;
                return false;
            }

            match = new KeyMatch(0);
            return true;
        }

        LegacyExpansionFilter keyFilter = LegacyExpansionFilter.Either;
        bool hasKeyFilter = false;
        for (int i = 0; i < Keys.Length; i++)
        {
            DatPropertyKey key = Keys[i];
            if (!key.Key.Equals(candidateKey, comparison))
            {
                continue;
            }

            if (!hasKeyFilter)
            {
                keyFilter = ctx.GetKeyFilter();
                hasKeyFilter = true;
            }

            if (!SourceNodeExtensions.FilterMatches(key.Filter, keyFilter))
            {
                continue;
            }

            if (key.Condition != null && (!key.Condition.TryEvaluateValue(out Optional<bool> conditionValue, ref ctx) ||
                                          !conditionValue.Value))
            {
                continue;
            }

            match = new KeyMatch(i);
            return true;
        }

        match = KeyMatch.None;
        return false;
    }

    internal void FinalizeIndex()
    {
        if (_indicesOrBuilder is ImmutableDictionary<PropertyOrderFile.TypeKey, int>.Builder bldr)
        {
            _indicesOrBuilder = bldr.ToImmutable();
            _indicesType = 3;
        }
    }

    private string GetDebuggerDisplay()
    {
        return IsImport ? $"Import {Type}" : $"{Type} \"{Key}\"";
    }

    public string FullName => $"{((IDatSpecificationObject)Owner).FullName}.{Key}";
    DatFileType IDatSpecificationObject.Owner => Owner.Owner;
}

/// <summary>
/// Extra information about a property key.
/// </summary>
public class DatPropertyKey
{
    /// <summary>
    /// The key corresponding to the information in this object.
    /// </summary>
    /// <remarks>Corresponds to the <c>Aliases.Alias</c>/<c>Key</c> properties.</remarks>
    public string Key { get; }

    /// <summary>
    /// Modern/legacy filter that has to be met for this key to be useable.
    /// </summary>
    /// <remarks>Corresponds to the <c>Aliases.LegacyExpansionFilter</c>/<c>KeyLegacyExpansionFilter</c> properties.</remarks>
    public LegacyExpansionFilter Filter { get; }

    /// <summary>
    /// Condition that has to be met for this key to be useable.
    /// </summary>
    /// <remarks>Corresponds to the <c>Aliases.Condition</c>/<c>KeyCondition</c> properties.</remarks>
    public IValue<bool>? Condition { get; }

    internal DatPropertyKey(string key, LegacyExpansionFilter filter, IValue<bool>? condition)
    {
        Key = key;
        Filter = filter;
        Condition = condition;
    }

    /// <inheritdoc />
    public override string ToString() => Key;
}

/// <summary>
/// An implementation of <see cref="DatPropertyKey"/> that contains information about template properties.
/// </summary>
public class DatTemplatePropertyKey : DatPropertyKey
{
    /// <summary>
    /// Object used to parse template keys.
    /// </summary>
    public TemplateProcessor TemplateProcessor { get; }

    /// <summary>
    /// The key that includes the '*' wildcards instead of '#'.
    /// </summary>
    public string OriginalKey { get; }

    internal DatTemplatePropertyKey(string key, LegacyExpansionFilter filter, IValue<bool>? condition)
        : base(
            TemplateProcessor.CreateForKey(key, out TemplateProcessor processor),
            filter,
            condition
        )
    {
        OriginalKey = key;
        TemplateProcessor = processor;
    }

    /// <inheritdoc />
    public override string ToString() => OriginalKey;
}

/// <summary>
/// Determines which section a property can be in when a Metadata section is provded.
/// </summary>
public enum AssetDatPropertyPositionExpectation
{
    /// <summary>
    /// Property must appear in the <c>Asset</c> section. If there isn't an <c>Asset</c> section, it must be at the root level.
    /// <example>
    /// <code>
    /// Asset
    /// {
    ///     Prop ...
    /// }
    ///
    /// // or
    ///
    /// Prop ...
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>This is the default value for most properties.</remarks>
    AssetData,

    /// <summary>
    /// If the <c>Metadata</c> section is present, this property can only be read from it. If not it must be at the root level.
    /// <example>
    /// <code>
    /// Metadata
    /// {
    ///     Prop ...
    /// }
    ///
    /// // or
    ///
    /// Prop ...
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>Used by the <c>GUID</c> property.</remarks>
    MetadataOnlyIfExistsOtherwiseRoot,

    /// <summary>
    /// If the <c>Metadata</c> section is present, this property can only be read from it. If not it must be in the <c>Asset</c> section if it exists, otherwise at the root level.
    /// <example>
    /// <code>
    /// Metadata
    /// {
    ///     Prop ...
    /// }
    ///
    /// // or
    ///
    /// Asset
    /// {
    ///     Prop ...
    /// }
    ///
    /// // or
    ///
    /// Prop ...
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>Used by the <c>Type</c> property.</remarks>
    MetadataOnlyIfExistsOtherwiseAssetData,

    /// <summary>
    /// Prioritizes reading the property from the <c>Metadata</c> section, but if it's not present it will be read from the <c>Asset</c> section instead.
    /// If the <c>Asset</c> section is also not present it'll be read at the root level.
    /// <example>
    /// <code>
    /// Metadata
    /// {
    ///     Prop ... &lt;----
    /// }
    /// Asset
    /// {
    ///     Prop ...
    /// }
    ///
    /// // or
    ///
    /// Asset
    /// {
    ///     Prop ...
    /// }
    ///
    /// // or
    ///
    /// Metadata
    /// {
    ///     Prop ... &lt;----
    /// }
    /// 
    /// Prop ...
    ///
    /// // or
    ///
    /// Prop ...
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>Currently unused by Unturned.</remarks>
    MetadataOrAssetData,

    /// <summary>
    /// The property can only exist at the root level, even if an <c>Asset</c> section is present.
    /// <example>
    /// <code>
    /// Metadata
    /// {
    ///     Prop ...
    /// }
    /// Asset
    /// {
    ///     Prop ...
    /// }
    ///
    /// Prop ... &lt;----
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>Currently unused by Unturned.</remarks>
    Root,

    /// <summary>
    /// The property can only exist in the <c>Metadata</c> section.
    /// <example>
    /// <code>
    /// Metadata
    /// {
    ///     Prop ... &lt;----
    /// }
    /// Asset
    /// {
    ///     Prop ...
    /// }
    ///
    /// Prop ...
    ///
    /// // NOT:
    ///
    /// Prop ...
    ///
    /// // NOT:
    ///
    /// Asset
    /// {
    ///     Prop ...
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>Currently unused by Unturned.</remarks>
    Metadata,

    /// <summary>
    /// The property can only exist in the <c>Asset</c> section.
    /// <example>
    /// <code>
    /// Metadata
    /// {
    ///     Prop ...
    /// }
    /// Asset
    /// {
    ///     Prop ... &lt;----
    /// }
    ///
    /// Prop ...
    ///
    /// // NOT:
    ///
    /// Prop ...
    ///
    /// // NOT:
    ///
    /// Metadata
    /// {
    ///     Prop ...
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>Currently unused by Unturned.</remarks>
    Asset
}

/// <summary>
/// Expresses the location of a property within an asset file.
/// </summary>
public enum AssetDatPropertyPosition
{
    /// <summary>
    /// In the root dictionary of the asset file.
    /// <example>
    /// <code>
    /// Prop ...
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>All properties use this value in non-asset files or custom types.</remarks>
    Root,

    /// <summary>
    /// In the <c>Asset</c> section of the asset file.
    /// <example>
    /// <code>
    /// Asset
    /// {
    ///     Prop ...
    /// }
    /// </code>
    /// </example>
    /// </summary>
    Asset,

    /// <summary>
    /// In the <c>Metadata</c> section of the asset file.
    /// <example>
    /// <code>
    /// Metadata
    /// {
    ///     Prop ...
    /// }
    /// </code>
    /// </example>
    /// </summary>
    Metadata
}

public static class AssetDatPropertyPositionExtensions
{
    extension(AssetDatPropertyPositionExpectation expectation)
    {
        /// <summary>
        /// Checks whether a property with this expectation could be possibly satisfied by a node at <paramref name="position"/>.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="assetFile">The file to check in.</param>
        /// <exception cref="InvalidEnumArgumentException">Invalid expectation or position.</exception>
        public bool IsValidPosition(AssetDatPropertyPosition position, IAssetSourceFile assetFile)
        {
            return expectation.IsValidPosition(position, assetFile.GetAssetDataDictionary() != null, assetFile.GetMetadataDictionary() != null);
        }

        /// <summary>
        /// Checks whether a property with this expectation could be possibly satisfied by a node at <paramref name="position"/>.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="hasAssetSection">Whether or not the asset file contains a 'Asset' section.</param>
        /// <param name="hasMetadataSection">Whether or not the asset file contains a 'Metadata' section.</param>
        /// <exception cref="InvalidEnumArgumentException">Invalid expectation or position.</exception>
        public bool IsValidPosition(AssetDatPropertyPosition position, bool hasAssetSection, bool hasMetadataSection)
        {
            if (position is < AssetDatPropertyPosition.Root or > AssetDatPropertyPosition.Metadata)
                throw new InvalidEnumArgumentException(nameof(position), (int)position, typeof(AssetDatPropertyPosition));

            return expectation switch
            {
                AssetDatPropertyPositionExpectation.AssetData
                    => position == AssetDatPropertyPosition.Asset || (!hasAssetSection && position == AssetDatPropertyPosition.Root),
                AssetDatPropertyPositionExpectation.MetadataOnlyIfExistsOtherwiseRoot
                    => position == AssetDatPropertyPosition.Metadata || (!hasMetadataSection && position == AssetDatPropertyPosition.Root),
                AssetDatPropertyPositionExpectation.MetadataOnlyIfExistsOtherwiseAssetData
                    => position switch
                    {
                        AssetDatPropertyPosition.Asset => !hasMetadataSection,
                        AssetDatPropertyPosition.Root => !hasAssetSection && !hasMetadataSection,
                        _ /* Metadata */ => true 
                    },
                AssetDatPropertyPositionExpectation.MetadataOrAssetData
                    => true,
                AssetDatPropertyPositionExpectation.Root
                    => position == AssetDatPropertyPosition.Root,
                AssetDatPropertyPositionExpectation.Metadata
                    => position == AssetDatPropertyPosition.Metadata,
                AssetDatPropertyPositionExpectation.Asset
                    => position == AssetDatPropertyPosition.Asset,
                _
                    => throw new InvalidEnumArgumentException(nameof(expectation), (int)expectation, typeof(AssetDatPropertyPositionExpectation))
            };
        }
    }
}
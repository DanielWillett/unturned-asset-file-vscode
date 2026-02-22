using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// A property that can be defined in the file of a <see cref="DatFileType"/> or within an instance of a custom type.
/// </summary>
public class DatProperty : IDatSpecificationObject
{
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

    internal DatProperty(string key, DatTypeWithProperties owner, JsonElement element, SpecPropertyContext context)
    {
        Key = key;
        Owner = owner;
        DataRoot = element;
        Context = context;
        Type = null!;
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

    public string FullName => $"{((IDatSpecificationObject)Owner).FullName}.{Key}";
    DatFileType IDatSpecificationObject.Owner => Owner.Owner;
}

/// <summary>
/// Extra information about a property key.
/// </summary>
public sealed class DatPropertyKey
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
    /// <remarks>All properties use this value in non-asset files.</remarks>
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
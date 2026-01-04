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
public sealed class DatProperty : IDatSpecificationObject
{
    /// <inheritdoc />
    public JsonElement DataRoot { get; }

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
    /// Whether or not this property can be read from the Metadata section.
    /// </summary>
    /// <remarks>Corresponds to the <c>CanBeInMetadata</c> property.</remarks>
    public bool CanBeInMetadata { get; internal set; }

    /// <summary>
    /// Type of value this property stores.
    /// </summary>
    /// <remarks>Corresponds to the <c>Type</c> property.</remarks>
    public IPropertyType Type { get; }

    /// <summary>
    /// List of available keys, if extra information is given for any.
    /// </summary>
    /// <remarks>If this is default or empty, refer to <see cref="Key"/>. Corresponds to the <c>Alias</c>/<c>Aliases</c> properties, but also includes the original key.</remarks>
    public ImmutableArray<DatPropertyKey> Keys { get; internal set; }

    /// <summary>
    /// List of template grops, if any.
    /// </summary>
    /// <remarks>Corresponds to the <c>TemplateGroups</c> property.</remarks>
    public ImmutableArray<TemplateGroup> TemplateGroups { get; internal set; }

    /// <summary>
    /// Whether or not this property is a template property that uses <see cref="TemplateGroups"/>.
    /// </summary>
    /// <remarks>Corresponds to the <c>Template</c> property.</remarks>
    [MemberNotNullWhen(true, nameof(TemplateGroups))]
    public bool IsTemplate { get; internal set; }

    /// <summary>
    /// Whether or not every value in this template group should be unique.
    /// </summary>
    /// <remarks>Corresponds to the <c>TemplateGroupUniqueValue</c> property.</remarks>
    public bool TemplateGroupUniqueValue { get; internal set; }

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
    /// <remarks>Corresponds to the <c>MarkdownDescription</c> property.</remarks>
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
    /// The default value for this property when the property is included without a parsable value. Defaults to <see cref="DefaultValue"/>.
    /// </summary>
    /// <remarks>Corresponds to the <c>IncludedDefaultValue</c> property.</remarks>
    public IValue? IncludedDefaultValue { get; internal set; }

    internal DatProperty(string key, IPropertyType type, DatTypeWithProperties owner, JsonElement element)
    {
        Key = key;
        Owner = owner;
        Type = type;
        DataRoot = element;
    }

    /// <summary>
    /// Create a new <see cref="DatProperty"/> instance given a <paramref name="key"/> and <paramref name="type"/>.
    /// </summary>
    /// <param name="key">The primary key for this property.</param>
    /// <param name="type">The type of value this property stores.</param>
    /// <param name="owner">The type that defines this property.</param>
    /// <param name="element">The JSON element this property was read from.</param>
    /// <returns>The newly-created <see cref="DatProperty"/> instance.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static DatProperty Create(string key, IPropertyType type, DatTypeWithProperties owner, JsonElement element)
    {
        return new DatProperty(
            key   ?? throw new ArgumentNullException(nameof(key)),
            type  ?? throw new ArgumentNullException(nameof(type)),
            owner ?? throw new ArgumentNullException(nameof(owner)),
            element
        );
    }

    private const int CachedLocalizationStringTypes = 8;
    private static readonly StringType?[] LocalizationStringTypeCache = new StringType?[CachedLocalizationStringTypes];

    /// <summary>
    /// Create a new <see cref="DatProperty"/> for a localization file.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static DatProperty CreateLocalizationKey(string key, string? value, DatTypeWithProperties owner)
    {
        int maxFormatArguments = -1;
        bool allowLineBreak = false;
        if (string.IsNullOrEmpty(value))
        {
            value = null;
        }
        else
        {
            allowLineBreak = value.Contains("<br>", StringComparison.Ordinal);
            maxFormatArguments = StringHelper.GetHighestFormattingArgument(value);
        }


        ++maxFormatArguments;

        StringType type;
        if (allowLineBreak)
        {
            type = new StringType(int.MinValue, int.MaxValue, true, true, maxFormatArguments, OneOrMore<Regex>.Null);
        }
        else if (maxFormatArguments < CachedLocalizationStringTypes)
        {
            type = LocalizationStringTypeCache[maxFormatArguments]
                ??= new StringType(int.MinValue, int.MaxValue, true, false, maxFormatArguments, OneOrMore<Regex>.Null);
        }
        else
        {
            type = new StringType(int.MinValue, int.MaxValue, true, false, maxFormatArguments, OneOrMore<Regex>.Null);
        }

        return new DatProperty(key, type, owner, default)
        {
            DefaultValue = value == null ? null : Value.Create(value, type)
        };
    }

    /// <summary>
    /// Create a new <see cref="DatProperty"/> instance that hides a property in a parent type.
    /// </summary>
    /// <param name="overriding">The property to hide.</param>
    /// <param name="owner">The type that defines this property.</param>
    /// <returns>The newly-created <see cref="DatProperty"/> instance.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static DatProperty Hide(DatProperty overriding, DatTypeWithProperties owner)
    {
        if (owner == null)
            throw new ArgumentNullException(nameof(owner));
        if (overriding == null)
            throw new ArgumentNullException(nameof(overriding));

        return new DatProperty(overriding.Key, overriding.Type, owner, default)
        {
            HideOverridden = true,
            OverriddenProperty = overriding
        };
    }

    string IDatSpecificationObject.FullName => $"{((IDatSpecificationObject)Owner).FullName}.{Key}";
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

    /// <summary>
    /// If this property is a template, the processor for that template.
    /// </summary>
    public TemplateProcessor? TemplateProcessor { get; }

    internal DatPropertyKey(string key, LegacyExpansionFilter filter, IValue<bool>? condition, TemplateProcessor? templateProcessor)
    {
        Key = key;
        Filter = filter;
        Condition = condition;
        TemplateProcessor = templateProcessor;
    }
}
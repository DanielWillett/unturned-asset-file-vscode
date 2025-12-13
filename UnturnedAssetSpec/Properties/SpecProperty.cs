using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

[JsonConverter(typeof(SpecPropertyConverter))]
[DebuggerDisplay("{Key}")]
public class SpecProperty : IEquatable<SpecProperty?>, ICloneable, IAdditionalPropertyProvider
{
    private TemplateProcessor? _keyTemplateProcessor;
    private TemplateProcessor[]? _aliasTemplateProcessors;

    /// <summary>
    /// The key of the flag or property. Will be empty for imported types.
    /// </summary>
    /// <remarks>Contains '#' characters to represent template groups, not '*'. Key is unescaped.</remarks>
    public required string Key { get; set; }

    /// <summary>
    /// Specifies a filter on whether the key is for the legacy format, modern format, or both.
    /// </summary>
    public LegacyExpansionFilter KeyLegacyExpansionFilter { get; set; } = LegacyExpansionFilter.Either;

    /// <summary>
    /// The context of this property, either 'Property', 'Localization', or 'BundleAsset'.
    /// </summary>
    public SpecPropertyContext Context { get; set; } = SpecPropertyContext.Property;

    /// <summary>
    /// Import properties have an empty key and are used to layer one type into another.
    /// </summary>
    /// <remarks>See ServerListCurationAsset for an example.</remarks>
    public bool IsImport => !KeyIsLegacySelfRef && Key.Length == 0 && Type.Type is IPropertiesSpecType;

    /// <summary>
    /// Whether or not the key should be equal to the current object's key in legacy filter mode.
    /// </summary>
    /// <remarks>This is replaced by '#This.Key' in the JSON document.</remarks>
    public bool KeyIsLegacySelfRef { get; internal set; }

    /// <inheritdoc cref="IsImport"/>
    public bool TryGetImportType(out IPropertiesSpecType type)
    {
        if (!IsImport)
        {
            type = null!;
            return false;
        }

        type = (IPropertiesSpecType)Type.Type!;
        return true;
    }

    public bool IsHidden { get; internal set; }

    /// <summary>
    /// The type of the property.
    /// </summary>
    public required PropertyTypeOrSwitch Type { get; set; }

    /// <summary>
    /// If <see cref="IsTemplate"/> is <see langword="true"/>, the singular version of the key which would not be regex (ex. <c>Blade</c> for <c>Blade_#</c>).
    /// </summary>
    public string? SingleKeyOverride { get; set; }

    /// <summary>
    /// Short description of the property.
    /// </summary>
    public ISpecDynamicValue? Description { get; set; }

    /// <summary>
    /// Name of the C# variable storing the value of this property.
    /// </summary>
    public ISpecDynamicValue? Variable { get; set; }

    /// <summary>
    /// Link to the SDG modding docs relating to this property.
    /// </summary>
    public ISpecDynamicValue? Docs { get; set; }

    /// <summary>
    /// Longer markdown description of the property.
    /// </summary>
    public ISpecDynamicValue? Markdown { get; set; }

    /// <summary>
    /// Designates a property that selects the cross-ref file for any properties starting with $cr$::
    /// </summary>
    public string? FileCrossRef { get; set; }

    /// <summary>
    /// The template group for which this property is a count for.
    /// </summary>
    public string? CountForTemplateGroup { get; set; }

    /// <summary>
    /// A data-ref to another key's template group (such as #($cr$::SDG.Unturned.ItemAsset, Assembly-CSharp::Blueprints).TemplateGroups[0] ) to reference the blueprint ID of another item.
    /// </summary>
    /// <remarks>Valid values will be the present indices in the other property's template groups.</remarks>
    public TemplateGroupsDataRef? ValueTemplateGroupReference { get; set; }

    /// <summary>
    /// Reference to a set of values from a list. Example: 'Blueprints.Id'. If it ends with '!' it's an error to not reference a valid value.
    /// </summary>
    public string? ListReference { get; set; }

    /// <summary>
    /// Indicates that the value of this property should change this object's type based on this property in the enum. The type should be a subtype of the current type.
    /// </summary>
    public string? SubtypeSwitch { get; set; }

    /// <summary>
    /// Other keys that can be used for the flag or property.
    /// </summary>
    public OneOrMore<Alias> Aliases { get; set; } = OneOrMore<Alias>.Null;

    /// <summary>
    /// If this property can be read from the Metadata section.
    /// </summary>
    public bool CanBeInMetadata { get; set; }

    /// <summary>
    /// If <see cref="Key"/> is a template expression to match properties.
    /// </summary>
    public bool IsTemplate { get; set; }

    /// <summary>
    /// If each value should be unique in the set of properties with the same key group set.
    /// </summary>
    public bool TemplateGroupUniqueValue { get; set; }

    /// <summary>
    /// If this property shouldn't be used anymore or was left in for legacy features.
    /// </summary>
    public ISpecDynamicValue Deprecated { get; set; } = SpecDynamicValue.False;

    /// <summary>
    /// If this property is experimental and may change in the future.
    /// </summary>
    public ISpecDynamicValue Experimental { get; set; } = SpecDynamicValue.False;

    /// <summary>
    /// Sort priority (descending).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Unturned version when this option was added.
    /// </summary>
    public ISpecDynamicValue? Version { get; set; }

    /// <summary>
    /// If this property is required.
    /// </summary>
    public ISpecDynamicValue? RequiredCondition { get; set; }

    /// <summary>
    /// The default value for this property if applicable.
    /// If <see cref="IncludedDefaultValue"/> is used, this indicates the default value if the property is not included.
    /// </summary>
    public ISpecDynamicValue? DefaultValue { get; set; }

    /// <summary>
    /// The default value for this property if it's included but no value is given (or an unparseable value).
    /// </summary>
    public ISpecDynamicValue? IncludedDefaultValue { get; set; }

    /// <summary>
    /// The minimum allowed value.
    /// </summary>
    public ISpecDynamicValue? MinimumValue { get; set; }

    /// <summary>
    /// The minimum number of values in this property if it's a list or dictionary.
    /// </summary>
    public ISpecDynamicValue? MinimumCount { get; set; }

    /// <summary>
    /// The maximum number of values in this property if it's a list or dictionary.
    /// </summary>
    public ISpecDynamicValue? MaximumCount { get; set; }

    /// <summary>
    /// If <see cref="MinimumValue"/> is an exclusive minimum.
    /// </summary>
    public bool IsMinimumValueExclusive { get; set; }

    /// <summary>
    /// The minimum allowed value.
    /// </summary>
    public ISpecDynamicValue? MaximumValue { get; set; }

    /// <summary>
    /// If <see cref="MaximumValue"/> is an exclusive maximum.
    /// </summary>
    public bool IsMaximumValueExclusive { get; set; }

    /// <summary>
    /// A blacklist of allowed values, or a whitelist of allowed values outside the minimum and/or maximum.
    /// </summary>
    public OneOrMore<ISpecDynamicValue> Exceptions { get; set; } = OneOrMore<ISpecDynamicValue>.Null;

    /// <summary>
    /// If <see cref="Exceptions"/> should be treated as a whitelist of allowed values outside the minimum and/or maximum values instead of a blacklist.
    /// </summary>
    public bool ExceptionsAreWhitelist { get; set; }

    /// <summary>
    /// If <see cref="IsTemplate"/> is <see langword="true"/>, then this is a list of groups by name to match with other properties within the given template group.
    /// </summary>
    public OneOrMore<TemplateGroup> TemplateGroups { get; set; } = OneOrMore<TemplateGroup>.Null;

    /// <summary>
    /// Properties that must also exist if this property exists.
    /// </summary>
    public InclusionCondition? InclusiveProperties { get; set; }

    /// <summary>
    /// Properties that shouldn't exist if this property exists.
    /// </summary>
    public InclusionCondition? ExclusiveProperties { get; set; }

#nullable disable
    /// <summary>
    /// The type that this property is a part of.
    /// </summary>
    public IPropertiesSpecType Owner { get; set; }

#nullable restore

    /// <summary>
    /// The property overridden by this one.
    /// </summary>
    public SpecProperty? Parent { get; internal set; }

    /// <summary>
    /// If this property was copied from the base type.
    /// </summary>
    public bool IsOverride => Parent != null;

    /// <summary>
    /// Additional properties present on the property's definition.
    /// </summary>
    public OneOrMore<KeyValuePair<string, object?>> AdditionalProperties { get; set; } = OneOrMore<KeyValuePair<string, object?>>.Null;

    /// <summary>
    /// Runs a transformation process over every single exposed <see cref="ISpecDynamicValue"/> on this property.
    /// </summary>
    internal void ProcessValues(Func<ISpecDynamicValue, ISpecDynamicValue?> process)
    {
        RequiredCondition    = Process(RequiredCondition, process);
        DefaultValue         = Process(DefaultValue, process);
        IncludedDefaultValue = Process(IncludedDefaultValue, process);
        MinimumValue         = Process(MinimumValue, process);
        MaximumValue         = Process(MaximumValue, process);
        MinimumCount         = Process(MinimumCount, process);
        MaximumCount         = Process(MaximumCount, process);
        Docs                 = Process(Docs, process);
        Markdown             = Process(Markdown, process);
        Version              = Process(Version, process);

        OneOrMore<ISpecDynamicValue> except = Exceptions;
        foreach (ISpecDynamicValue value in except)
        {
            ISpecDynamicValue? newValue = process(value);
            if (ReferenceEquals(newValue, value))
                continue;

            Exceptions = Exceptions.Remove(value);
            if (newValue != null)
                Exceptions = Exceptions.Add(newValue);
        }

        if (InclusiveProperties != null)
        {
            foreach (InclusionConditionProperty condition in InclusiveProperties.Properties)
            {
                SpecDynamicSwitchCaseOrCondition cond = condition.Condition;
                bool anyChanges = false;
                SpecDynamicSwitchCaseOrCondition cOut = ProcessValuesInCaseOrCondition(in cond, ref anyChanges, process);
                if (anyChanges)
                    condition.Condition = cOut;
            }
        }

        if (ExclusiveProperties != null)
        {
            foreach (InclusionConditionProperty condition in ExclusiveProperties.Properties)
            {
                SpecDynamicSwitchCaseOrCondition cond = condition.Condition;
                bool anyChanges = false;
                SpecDynamicSwitchCaseOrCondition cOut = ProcessValuesInCaseOrCondition(in cond, ref anyChanges, process);
                if (anyChanges)
                    condition.Condition = cOut;
            }
        }

        if (Type.IsSwitch)
        {
            Process(Type.TypeSwitch, process);
        }

        return;

        [return: NotNullIfNotNull(nameof(value))]
        static ISpecDynamicValue? Process(ISpecDynamicValue? value, Func<ISpecDynamicValue, ISpecDynamicValue?> process)
        {
            if (value is SpecDynamicSwitchValue sw)
            {
                foreach (SpecDynamicSwitchCaseValue c in sw.Cases)
                {
                    OneOrMore<SpecDynamicSwitchCaseOrCondition> conditions = c.Conditions;
                    for (int i = 0; i < conditions.Length; i++)
                    {
                        SpecDynamicSwitchCaseOrCondition cond = conditions[i];
                        bool anyChanges = false;
                        SpecDynamicSwitchCaseOrCondition cOut =
                            ProcessValuesInCaseOrCondition(in cond, ref anyChanges, process);
                        if (anyChanges)
                        {
                            if (conditions.IsSingle)
                            {
                                c.Conditions = new OneOrMore<SpecDynamicSwitchCaseOrCondition>(cOut);
                            }
                            else
                            {
                                conditions.Values[i] = cOut;
                            }
                        }
                    }
                }
            }
            else if (value != null)
            {
                return process(value);
            }

            return value;
        }

        static SpecDynamicSwitchCaseOrCondition ProcessValuesInCaseOrCondition(
            in SpecDynamicSwitchCaseOrCondition cond,
            ref bool anyChanges,
            Func<ISpecDynamicValue, ISpecDynamicValue?> process)
        {
            if (cond.Case != null)
            {
                if (cond.Case.HasConditions)
                {
                    OneOrMore<SpecDynamicSwitchCaseOrCondition> conditions = cond.Case.Conditions;
                    for (int i = 0; i < conditions.Length; i++)
                    {
                        SpecDynamicSwitchCaseOrCondition c = conditions[i];
                        bool ac = false;
                        SpecDynamicSwitchCaseOrCondition cOut = ProcessValuesInCaseOrCondition(in c, ref ac, process);
                        if (ac)
                        {
                            anyChanges = true;
                            if (conditions.IsSingle)
                            {
                                cond.Case.Conditions = new OneOrMore<SpecDynamicSwitchCaseOrCondition>(cOut);
                            }
                            else
                            {
                                conditions.Values[i] = cOut;
                            }
                        }
                    }
                }

                if (cond.Case.Operation == SpecDynamicSwitchCaseOperation.When)
                {
                    SpecCondition c = cond.Condition;
                    ISpecDynamicValue? v = process(c.Variable);
                    if (!ReferenceEquals(v, c.Variable) && v != null)
                    {
                        anyChanges = true;
                        cond.Case.WhenCondition = new SpecCondition(v, c.Operation, c.Comparand, c.IsInverted);
                    }
                }
            }
            else
            {
                SpecCondition c = cond.Condition;
                ISpecDynamicValue? v = process(c.Variable);
                if (!ReferenceEquals(v, c.Variable) && v != null)
                {
                    anyChanges = true;
                    return new SpecDynamicSwitchCaseOrCondition(new SpecCondition(v, c.Operation, c.Comparand, c.IsInverted));
                }
            }

            return cond;
        }
    }

    public Version? GetReleaseVersion(in FileEvaluationContext ctx)
    {
        if (Version != null && Version.TryEvaluateValue(in ctx, out object? value))
        {
            if (value is string { Length: > 0 } str)
                return System.Version.Parse(str);
            if (value is Version v)
                return v;
        }

        if (Owner.Version != null)
            return Owner.Version;

        AssetSpecType? t = Owner.Owner;
        if (t != null && !ReferenceEquals(t, Owner) && t.Version != null)
            return t.Version;

        return null;
    }

    public string? GetDocumentation(in FileEvaluationContext ctx)
    {
        if (Docs != null && Docs.TryEvaluateValue(in ctx, out object? value))
        {
            if (value is string { Length: > 0 } str)
                return str;
            if (value is Uri uri)
                return uri.ToString();
        }

        return string.IsNullOrEmpty(Owner.Docs) ? null : Owner.Docs;
    }

    public override string ToString()
    {
        return Owner != null ? Owner.Type.Type + "." + Key : Key;
    }

    public TemplateProcessor KeyTemplateProcessor => _keyTemplateProcessor ?? TemplateProcessor.None;

    public TemplateProcessor GetAliasTemplateProcessor(int index)
    {
        if (_aliasTemplateProcessors == null || index < 0 || index >= _aliasTemplateProcessors.Length)
            return TemplateProcessor.None;

        return _aliasTemplateProcessors[index];
    }

    /// <summary>
    /// Determines the filter applied to the key currently being parsed.
    /// </summary>
    public LegacyExpansionFilter GetCorrespondingFilter(in SpecPropertyTypeParseContext parse)
    {
        string? key = parse.BaseKey ?? (parse.Parent as IPropertySourceNode)?.Key;
        if (key == null)
            return KeyLegacyExpansionFilter;

        if (!IsTemplate)
        {
            if (string.Equals(key, Key, StringComparison.OrdinalIgnoreCase))
            {
                return KeyLegacyExpansionFilter;
            }

            foreach (Alias alias in Aliases)
            {
                if (string.Equals(key, alias.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return alias.Filter;
                }
            }

            return LegacyExpansionFilter.Either;
        }

        if (_keyTemplateProcessor != null)
        {
            Span<int> sp = stackalloc int[_keyTemplateProcessor.TemplateCount];
            if (_keyTemplateProcessor.TryParseKeyValues(key, sp))
            {
                return KeyLegacyExpansionFilter;
            }
        }

        if (_aliasTemplateProcessors != null)
        {
            int max = 0;
            for (int i = 0; i < Aliases.Length && i < _aliasTemplateProcessors.Length; i++)
            {
                Alias alias = Aliases[i];
                if (_aliasTemplateProcessors[i] is not { } templateProcessor)
                    continue;

                max = Math.Max(max, templateProcessor.TemplateCount);
            }

            Span<int> sp = stackalloc int[max];
            for (int i = 0; i < Aliases.Length && i < _aliasTemplateProcessors.Length; i++)
            {
                Alias alias = Aliases[i];
                if (_aliasTemplateProcessors[i] is not { } templateProcessor)
                    continue;

                if (templateProcessor.TryParseKeyValues(key, sp))
                {
                    return alias.Filter;
                }
            }
        }

        return LegacyExpansionFilter.Either;
    }

    internal void CreateTemplateProcessors()
    {
        if (!IsTemplate)
            return;

        string key = Key;
        _keyTemplateProcessor = TemplateProcessor.CreateForKey(ref key);
        Key = key;

        if (Aliases.IsNull)
            return;

        Alias[] newAliases = Aliases.ToArray();
        TemplateProcessor[] aliasTemplateProcessors = new TemplateProcessor[newAliases.Length];
        bool allAreNone = true;
        for (int i = 0; i < aliasTemplateProcessors.Length; ++i)
        {
            ref Alias a = ref newAliases[i];
            string aliasName = a.Value;
            TemplateProcessor p = TemplateProcessor.CreateForKey(ref aliasName);
            if (!ReferenceEquals(aliasName, a.Value))
                a = new Alias(aliasName, a.Filter);

            aliasTemplateProcessors[i] = p;
            if (p != TemplateProcessor.None)
                allAreNone = false;
        }

        if (!allAreNone)
            _aliasTemplateProcessors = aliasTemplateProcessors;
    }

    // used for use cases such as Asset.Type <-- ItemAsset.Type
    public SpecProperty CreateOverriddenProperty(SpecProperty overridingProperty)
    {
        if (overridingProperty.CanBeInMetadata != CanBeInMetadata)
        {
            return overridingProperty;
        }

        SpecProperty newProperty = (SpecProperty)overridingProperty.Clone();
        newProperty.Parent = overridingProperty;
        if (newProperty.Priority == 0)
            newProperty.Priority = Priority;
        return newProperty;
    }

    public bool Equals(SpecProperty? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (!string.Equals(Key, other.Key, StringComparison.Ordinal)
              || KeyLegacyExpansionFilter != other.KeyLegacyExpansionFilter
              || KeyIsLegacySelfRef != other.KeyIsLegacySelfRef
              || IsHidden != other.IsHidden
              || !Type.Equals(other.Type)
              || !string.Equals(SingleKeyOverride, other.SingleKeyOverride, StringComparison.Ordinal)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(Description, other.Description)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(Markdown, other.Markdown)
              || !string.Equals(FileCrossRef, other.FileCrossRef, StringComparison.Ordinal)
              || !string.Equals(CountForTemplateGroup, other.CountForTemplateGroup, StringComparison.Ordinal)
              || !EqualityComparer<DataRef?>.Default.Equals(ValueTemplateGroupReference, other.ValueTemplateGroupReference)
              || !string.Equals(ListReference, other.ListReference, StringComparison.Ordinal)
              || !string.Equals(SubtypeSwitch, other.SubtypeSwitch, StringComparison.Ordinal)
              || !Aliases.Equals(other.Aliases)
              || CanBeInMetadata != other.CanBeInMetadata
              || IsTemplate != other.IsTemplate
              || TemplateGroupUniqueValue != other.TemplateGroupUniqueValue
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(Deprecated, other.Deprecated)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(Experimental, other.Experimental)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(Version, other.Version)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(RequiredCondition, other.RequiredCondition)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(DefaultValue, other.DefaultValue)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(IncludedDefaultValue, other.IncludedDefaultValue)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(MinimumCount, other.MinimumCount)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(MaximumCount, other.MaximumCount)
              || !TemplateGroups.Equals(other.TemplateGroups)
              || !EqualityComparer<ISpecType?>.Default.Equals(Owner, other.Owner)
              || Priority != other.Priority
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(Docs, other.Docs)
              || !Exceptions.Equals(other.Exceptions)
              || ExceptionsAreWhitelist != other.ExceptionsAreWhitelist
              || !EqualityComparer<InclusionCondition?>.Default.Equals(ExclusiveProperties, other.ExclusiveProperties)
              || !EqualityComparer<InclusionCondition?>.Default.Equals(InclusiveProperties, other.InclusiveProperties)
              || IsMaximumValueExclusive != other.IsMaximumValueExclusive
              || IsMinimumValueExclusive != other.IsMinimumValueExclusive
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(MaximumValue, other.MaximumValue)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(MinimumValue, other.MinimumValue)
              || !EqualityComparer<SpecProperty?>.Default.Equals(Parent, other.Parent)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(Variable, other.Variable)
              )
        {
            return false;
        }

        return other is not SpecBundleAsset ba2 || this is SpecBundleAsset ba;
    }

    public override bool Equals(object? obj)
    {
        return obj is SpecProperty p && Equals(p);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return (Key.GetHashCode() * 397) ^ Type.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
    }

    /// <inheritdoc />
    public virtual object Clone() => new SpecProperty
    {
        Key = Key,
        KeyLegacyExpansionFilter = KeyLegacyExpansionFilter,
        IsHidden = IsHidden,
        Type = Type,
        SingleKeyOverride = SingleKeyOverride,
        Description = Description,
        Markdown = Markdown,
        FileCrossRef = FileCrossRef,
        CountForTemplateGroup = CountForTemplateGroup,
        ValueTemplateGroupReference = ValueTemplateGroupReference,
        ListReference = ListReference,
        SubtypeSwitch = SubtypeSwitch,
        Aliases = Aliases,
        CanBeInMetadata = CanBeInMetadata,
        IsTemplate = IsTemplate,
        TemplateGroupUniqueValue = TemplateGroupUniqueValue,
        Deprecated = Deprecated,
        Experimental = Experimental,
        Version = Version,
        RequiredCondition = RequiredCondition,
        DefaultValue = DefaultValue,
        IncludedDefaultValue = IncludedDefaultValue,
        TemplateGroups = TemplateGroups,
        Owner = Owner,
        Priority = Priority,
        Docs = Docs,
        Exceptions = Exceptions,
        ExceptionsAreWhitelist = ExceptionsAreWhitelist,
        ExclusiveProperties = ExclusiveProperties,
        InclusiveProperties = InclusiveProperties,
        IsMaximumValueExclusive = IsMaximumValueExclusive,
        IsMinimumValueExclusive = IsMinimumValueExclusive,
        MaximumValue = MaximumValue,
        MinimumValue = MinimumValue,
        MaximumCount = MaximumCount,
        MinimumCount = MinimumCount,
        Parent = Parent,
        Variable = Variable,
        _keyTemplateProcessor = _keyTemplateProcessor,
        _aliasTemplateProcessors = _aliasTemplateProcessors,
        AdditionalProperties = AdditionalProperties,
        Context = Context,
        KeyIsLegacySelfRef = KeyIsLegacySelfRef
    };
}

/// <summary>
/// Represents a key alias.
/// </summary>
public readonly struct Alias : IEquatable<Alias>
{
    /// <summary>
    /// The actual value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The legacy expansion filter for the value.
    /// </summary>
    public LegacyExpansionFilter Filter { get; }

    public Alias(string alias)
    {
        Value = alias;
        Filter = LegacyExpansionFilter.Either;
    }

    public Alias(string alias, LegacyExpansionFilter filter)
    {
        Value = alias;
        Filter = filter;
    }

    /// <inheritdoc />
    public bool Equals(Alias other) => string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Alias a && Equals(a);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return (int)Filter * 397 + (Value == null ? 0 : Value.GetHashCode());
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    public static implicit operator string(Alias a) => a.Value;
    public static implicit operator Alias(string a) => new Alias(a);
}

/// <summary>
/// Specifies a filter on which aliases can be used with each legacy expansion type.
/// </summary>
public enum LegacyExpansionFilter
{
    /// <summary>
    /// Can be used with either format.
    /// </summary>
    Either = 3,
    
    /// <summary>
    /// Can only be used with the legacy format.
    /// </summary>
    Legacy = 1,

    /// <summary>
    /// Can only be used with the modern format.
    /// </summary>
    Modern = 2
}
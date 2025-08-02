using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
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
public class SpecProperty : IEquatable<SpecProperty?>, ICloneable
{
    /// <summary>
    /// The key of the flag or property.
    /// </summary>
    public required string Key { get; set; }

    public bool IsHidden { get; internal set; }

    /// <summary>
    /// The type of the property.
    /// </summary>
    public required PropertyTypeOrSwitch Type { get; set; }

    /// <summary>
    /// If <see cref="KeyIsRegex"/> is <see langword="true"/>, the singular version of the key which would not be regex (ex. <c>Blade</c> for <c>Blade_#</c>).
    /// </summary>
    public string? SingleKeyOverride { get; set; }

    /// <summary>
    /// Short description of the property.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Name of the C# variable storing the value of this property.
    /// </summary>
    public string? Variable { get; set; }

    /// <summary>
    /// Link to the SDG modding docs relating to this property.
    /// </summary>
    public string? Docs { get; set; }

    /// <summary>
    /// Longer markdown description of the property.
    /// </summary>
    public string? Markdown { get; set; }

    /// <summary>
    /// Designates a property that selects the cross-ref file for any properties starting with $cr$::
    /// </summary>
    public string? FileCrossRef { get; set; }

    /// <summary>
    /// The regex-group for which this property is a count for.
    /// </summary>
    public string? CountForRegexGroup { get; set; }

    /// <summary>
    /// The 'Key regex-group[ RelatedKey...]' of which this property references, where key is a reference to another type.
    /// </summary>
    public string? ValueRegexGroupReference { get; set; }

    /// <summary>
    /// Indicates that the value of this property should change this object's type based on this property in the enum. The type should be a subtype of the current type.
    /// </summary>
    public string? SubtypeSwitch { get; set; }

    /// <summary>
    /// Other keys that can be used for the flag or property.
    /// </summary>
    public OneOrMore<string> Aliases { get; set; } = OneOrMore<string>.Null;

    /// <summary>
    /// If this property can be read from the Metadata section.
    /// </summary>
    public bool CanBeInMetadata { get; set; }

    /// <summary>
    /// If <see cref="Key"/> is a regex expression to match properties.
    /// </summary>
    public bool KeyIsRegex { get; set; }

    /// <summary>
    /// If this property shouldn't be used anymore or was left in for legacy features.
    /// </summary>
    public bool Deprecated { get; set; }

    /// <summary>
    /// Sort priority (descending).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Unique ID linking multiple properties that are usually kept together (no whitespace).
    /// </summary>
    public string? SimilarGrouping { get; set; }

    /// <summary>
    /// Unturned version when this option was added.
    /// </summary>
    public Version? Version { get; set; }

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
    /// If <see cref="KeyIsRegex"/> is <see langword="true"/>, then this is a list of groups by name to match with other properties within the given regex group.
    /// </summary>
    public OneOrMore<RegexKeyGroup> KeyGroups { get; set; } = OneOrMore<RegexKeyGroup>.Null;

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
    public ISpecType Owner { get; set; }

#nullable restore

    /// <summary>
    /// The property overridden by this one.
    /// </summary>
    public SpecProperty? Parent { get; internal set; }

    internal void ProcessValues(Func<ISpecDynamicValue, ISpecDynamicValue?> process)
    {
        if (RequiredCondition != null)
            RequiredCondition = process(RequiredCondition);
        if (DefaultValue != null)
            DefaultValue = process(DefaultValue);
        if (IncludedDefaultValue != null)
            IncludedDefaultValue = process(IncludedDefaultValue);
        if (MinimumValue != null)
            MinimumValue = process(MinimumValue);
        if (MaximumValue != null)
            MaximumValue = process(MaximumValue);

        foreach (ISpecDynamicValue value in Exceptions)
        {
            ISpecDynamicValue? newValue = process(value);
            if (ReferenceEquals(newValue, value))
                continue;

            Exceptions = Exceptions.Remove(value);
            if (newValue != null)
                Exceptions = Exceptions.Add(newValue);
        }
    }

    public override string ToString()
    {
        return Owner != null ? Owner.Type.Type + "." + Key : Key;
    }

    public bool Equals(SpecProperty? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        if (!string.Equals(Key, other.Key, StringComparison.Ordinal)
              || IsHidden != other.IsHidden
              || !Type.Equals(other.Type)
              || !string.Equals(SingleKeyOverride, other.SingleKeyOverride, StringComparison.Ordinal)
              || !string.Equals(Description, other.Description, StringComparison.Ordinal)
              || !string.Equals(Markdown, other.Markdown, StringComparison.Ordinal)
              || !string.Equals(FileCrossRef, other.FileCrossRef, StringComparison.Ordinal)
              || !string.Equals(CountForRegexGroup, other.CountForRegexGroup, StringComparison.Ordinal)
              || !string.Equals(ValueRegexGroupReference, other.ValueRegexGroupReference, StringComparison.Ordinal)
              || !string.Equals(SubtypeSwitch, other.SubtypeSwitch, StringComparison.Ordinal)
              || !Aliases.Equals(other.Aliases, StringComparison.Ordinal)
              || CanBeInMetadata != other.CanBeInMetadata
              || KeyIsRegex != other.KeyIsRegex
              || Deprecated != other.Deprecated
              || !EqualityComparer<Version?>.Default.Equals(Version, other.Version)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(RequiredCondition, other.RequiredCondition)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(DefaultValue, other.DefaultValue)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(IncludedDefaultValue, other.IncludedDefaultValue)
              || !KeyGroups.Equals(other.KeyGroups)
              || !EqualityComparer<ISpecType?>.Default.Equals(Owner, other.Owner)
              || Priority != other.Priority
              || !string.Equals(Docs, other.Docs, StringComparison.Ordinal)
              || !Exceptions.Equals(other.Exceptions)
              || ExceptionsAreWhitelist != other.ExceptionsAreWhitelist
              || !EqualityComparer<InclusionCondition?>.Default.Equals(ExclusiveProperties, other.ExclusiveProperties)
              || !EqualityComparer<InclusionCondition?>.Default.Equals(InclusiveProperties, other.InclusiveProperties)
              || IsMaximumValueExclusive != other.IsMaximumValueExclusive
              || IsMinimumValueExclusive != other.IsMinimumValueExclusive
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(MaximumValue, other.MaximumValue)
              || !EqualityComparer<ISpecDynamicValue?>.Default.Equals(MinimumValue, other.MinimumValue)
              || !EqualityComparer<SpecProperty?>.Default.Equals(Parent, other.Parent)
              || !string.Equals(Variable, other.Variable, StringComparison.Ordinal)
              || !string.Equals(SimilarGrouping, other.SimilarGrouping, StringComparison.Ordinal)
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
        IsHidden = IsHidden,
        Type = Type,
        SingleKeyOverride = SingleKeyOverride,
        Description = Description,
        Markdown = Markdown,
        FileCrossRef = FileCrossRef,
        CountForRegexGroup = CountForRegexGroup,
        ValueRegexGroupReference = ValueRegexGroupReference,
        SubtypeSwitch = SubtypeSwitch,
        Aliases = Aliases,
        CanBeInMetadata = CanBeInMetadata,
        KeyIsRegex = KeyIsRegex,
        Deprecated = Deprecated,
        Version = Version,
        RequiredCondition = RequiredCondition,
        DefaultValue = DefaultValue,
        IncludedDefaultValue = IncludedDefaultValue,
        KeyGroups = KeyGroups,
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
        Parent = Parent,
        Variable = Variable,
        SimilarGrouping = SimilarGrouping
    };
}
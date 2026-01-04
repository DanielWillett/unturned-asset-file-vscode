using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// A standardized reference to an existing property.
/// </summary>
public readonly struct PropertyReference : IEquatable<PropertyReference>
{
    /// <summary>The context specifier for <see cref="SpecPropertyContext.Property"/>.</summary>
    public const string PropertyContext = "prop";

    /// <summary>The context specifier for <see cref="SpecPropertyContext.Localization"/>.</summary>
    public const string LocalizationContext = "local";

    /// <summary>The context specifier for <see cref="SpecPropertyContext.BundleAsset"/>.</summary>
    public const string BundleAssetContext = "bndl";

    /// <summary>The context specifier for <see cref="SpecPropertyContext.CrossReferenceProperty"/>.</summary>
    public const string CrossRefererencedPropertyContext = "cr.prop";

    /// <summary>The context specifier for <see cref="SpecPropertyContext.CrossReferenceLocalization"/>.</summary>
    public const string CrossRefererencedLocalizationContext = "cr.local";

    /// <summary>The context specifier for <see cref="SpecPropertyContext.CrossReferenceUnspecified"/>.</summary>
    public const string CrossRefererencedContext = "cr";

    private const string PropertyContextWrapped = "$" + PropertyContext + "$";
    private const string LocalizationContextWrapped = "$" + LocalizationContext + "$";
    private const string BundleAssetContextWrapped = "$" + BundleAssetContext + "$";
    private const string CrossRefererencedPropertyContextWrapped = "$" + CrossRefererencedPropertyContext + "$";
    private const string CrossRefererencedLocalizationContextWrapped = "$" + CrossRefererencedLocalizationContext + "$";
    private const string CrossRefererencedContextWrapped = "$" + CrossRefererencedContext + "$";

    private const string PropertyContextPrefix = PropertyContextWrapped + "::";
    private const string LocalizationContextPrefix = LocalizationContextWrapped + "::";
    private const string BundleAssetContextPrefix = BundleAssetContextWrapped + "::";
    private const string CrossRefererencedPropertyContextPrefix = CrossRefererencedPropertyContextWrapped + "::";
    private const string CrossRefererencedLocalizationContextPrefix = CrossRefererencedLocalizationContextWrapped + "::";
    private const string CrossRefererencedContextPrefix = CrossRefererencedContextWrapped + "::";

    /// <summary>
    /// The type of property being referenced.
    /// </summary>
    public readonly SpecPropertyContext Context;

    /// <summary>
    /// The name of the type this property is in. If this property has breadcrumbs, this is the type of the base dictionary.
    /// </summary>
    public readonly string? TypeName;

    /// <summary>
    /// The name of the property being referenced.
    /// </summary>
    public readonly string PropertyName;

    /// <summary>
    /// Optional breadcrumbs pointing to a property within an object or list.
    /// </summary>
    public readonly PropertyBreadcrumbs Breadcrumbs;

    /// <summary>
    /// Whether or not this property is cross-referencing another file.
    /// </summary>
    public bool IsCrossReference => Context
        is >= SpecPropertyContext.CrossReferenceUnspecified
        and <= SpecPropertyContext.CrossReferenceLocalization;

    /// <summary>
    /// Creates a new <see cref="PropertyReference"/> from scratch pointing to a given property.
    /// </summary>
    /// <param name="context">The type of property being referenced.</param>
    /// <param name="typeName">The name of the type this property is in. If this property has breadcrumbs, this is the type of the base dictionary.</param>
    /// <param name="propertyName">The name of the property being referenced.</param>
    /// <param name="breadcrumbs">Optional breadcrumbs pointing to a property within an object or list.</param>
    public PropertyReference(SpecPropertyContext context, string? typeName, string propertyName, PropertyBreadcrumbs breadcrumbs = default)
    {
        Context = context;
        TypeName = typeName;
        PropertyName = propertyName;
        Breadcrumbs = breadcrumbs;
    }

    /// <summary>
    /// Creates a <see cref="ISpecDynamicValue"/> referencing this property.
    /// </summary>
    /// <param name="database">The asset database to be associated with this property reference value.</param>
    public IPropertyReferenceValue CreateValue(DatProperty owner, IAssetSpecDatabase database)
    {
        PropertyBreadcrumbs breadcrumbs = Breadcrumbs;
        if (IsCrossReference)
        {
            throw new NotImplementedException();
        }
        else
        {
            return new LocalPropertyReference(in this, owner, database);
        }
    }

    /// <summary>
    /// Parses a property reference from a standardized property-ref string. The @ symbol should not be included.
    /// </summary>
    public static PropertyReference Parse(ReadOnlySpan<char> data, PropertyResolutionContext mode = PropertyResolutionContext.Modern)
    {
        return Parse(data, null, mode);
    }

    /// <summary>
    /// Parses a property reference from a standardized property-ref string. The @ symbol should not be included.
    /// </summary>
    public static PropertyReference Parse(string data, PropertyResolutionContext mode = PropertyResolutionContext.Modern)
    {
        return Parse(data, data, mode);
    }

    /// <summary>
    /// Parses a property reference from a standardized property-ref string. The @ symbol should not be included.
    /// </summary>
    public static PropertyReference Parse(ReadOnlySpan<char> data, string? originalString, PropertyResolutionContext mode = PropertyResolutionContext.Modern)
    {
        ReadOnlySpan<char> noContext = TryRemoveContext(data, out SpecPropertyContext context);
        int typeIndex = noContext.IndexOf("::");
        ReadOnlySpan<char> noType = noContext;
        ReadOnlySpan<char> typeStr = ReadOnlySpan<char>.Empty;
        if (typeIndex >= 0)
        {
            typeStr = noContext.Slice(0, typeIndex);
            noType = noContext.Slice(typeIndex + 2);
        }

        PropertyBreadcrumbs crumbs;
        string propertyName;
        if (noType.Length == data.Length && originalString != null)
        {
            crumbs = PropertyBreadcrumbs.FromPropertyRef(originalString, out propertyName, mode);
        }
        else
        {
            crumbs = PropertyBreadcrumbs.FromPropertyRef(noType, out propertyName, mode);
        }

        return new PropertyReference(context, typeStr.IsEmpty ? null : typeStr.ToString(), propertyName, crumbs);
    }

    /// <summary>
    /// Attempts to parse a context specifier from one of the following forms: '<c>prop</c>', '<c>$prop$</c>', or '<c>$prop$::</c>'.
    /// </summary>
    public static bool TryParseContextSpecifier(ReadOnlySpan<char> contextSpecifier, out SpecPropertyContext context)
    {
        if (contextSpecifier.Length > 2 && contextSpecifier[^2] == ':' && contextSpecifier[^1] == ':')
        {
            contextSpecifier = contextSpecifier.Slice(0, contextSpecifier.Length - 2);
        }

        if (contextSpecifier.Length >= 2 && contextSpecifier[0] == '$' || contextSpecifier[^1] == '$')
        {
            contextSpecifier = contextSpecifier.Slice(1, contextSpecifier.Length - 2);
        }

        context = SpecPropertyContext.Unspecified;
        if (contextSpecifier.IsEmpty)
            return false;

        switch (contextSpecifier[0])
        {
            case 'p' or 'P':
                if (contextSpecifier.Equals(PropertyContext, StringComparison.OrdinalIgnoreCase))
                {
                    context = SpecPropertyContext.Property;
                    return true;
                }

                break;

            case 'l' or 'L':
                if (contextSpecifier.Equals(LocalizationContext, StringComparison.OrdinalIgnoreCase))
                {
                    context = SpecPropertyContext.Localization;
                    return true;
                }

                break;

            case 'b' or 'B':
                if (contextSpecifier.Equals(BundleAssetContext, StringComparison.OrdinalIgnoreCase))
                {
                    context = SpecPropertyContext.BundleAsset;
                    return true;
                }

                break;

            case 'c' or 'C':
                if (contextSpecifier.Equals(CrossRefererencedPropertyContext, StringComparison.OrdinalIgnoreCase))
                {
                    context = SpecPropertyContext.CrossReferenceProperty;
                    return true;
                }
                if (contextSpecifier.Equals(CrossRefererencedLocalizationContext, StringComparison.OrdinalIgnoreCase))
                {
                    context = SpecPropertyContext.CrossReferenceLocalization;
                    return true;
                }
                if (contextSpecifier.Equals(CrossRefererencedContext, StringComparison.OrdinalIgnoreCase))
                {
                    context = SpecPropertyContext.CrossReferenceUnspecified;
                    return true;
                }

                break;
        }

        return false;
    }

    public static string CreateContextSpecifier(SpecPropertyContext context, bool isPrefix = true)
    {
        return context switch
        {
            SpecPropertyContext.Property => isPrefix
                ? PropertyContextPrefix : PropertyContextWrapped,
            SpecPropertyContext.Localization => isPrefix
                ? LocalizationContextPrefix : LocalizationContextWrapped,
            SpecPropertyContext.BundleAsset => isPrefix
                ? BundleAssetContextPrefix : BundleAssetContextWrapped,
            SpecPropertyContext.CrossReferenceUnspecified => isPrefix
                ? CrossRefererencedContextPrefix : CrossRefererencedContextWrapped,
            SpecPropertyContext.CrossReferenceProperty => isPrefix
                ? CrossRefererencedPropertyContextPrefix : CrossRefererencedPropertyContextWrapped,
            SpecPropertyContext.CrossReferenceLocalization => isPrefix
                ? CrossRefererencedLocalizationContextPrefix : CrossRefererencedLocalizationContextWrapped,
            _ => string.Empty
        };
    }

    private static ReadOnlySpan<char> TryRemoveContext(ReadOnlySpan<char> data, out SpecPropertyContext context)
    {
        context = SpecPropertyContext.Unspecified;
        int firstIndex = data.IndexOf("::", StringComparison.Ordinal);
        if (firstIndex == -1)
            return data;

        ReadOnlySpan<char> contextSpecifier = data.Slice(0, firstIndex);

        if (contextSpecifier.Length < 2 || contextSpecifier[0] != '$' || contextSpecifier[^1] != '$')
            return data;

        if (!TryParseContextSpecifier(contextSpecifier, out context))
        {
            return data;
        }

        return data.Slice(firstIndex + 2);
    }

#if !NETSTANDARD2_1_OR_GREATER && !NET5_0_OR_GREATER
    private static readonly char[] Escapables = [ '(', ')', '\\' ];
#endif

    public void WriteToJson(Utf8JsonWriter writer)
    {
        string str = ToString();

#if !NETSTANDARD2_1_OR_GREATER && !NET5_0_OR_GREATER
        StringHelper.EscapeValue(ref str, Escapables);
#else
        ReadOnlySpan<char> escapables = [ '(', ')', '\\' ];
        StringHelper.EscapeValue(ref str, escapables);
#endif

        writer.WriteStringValue(StringHelper.ContainsWhitespace(str) ? $"@({str})" : $"@{str}");
    }

    /// <inheritdoc />
    public bool Equals(PropertyReference other)
    {
        return other.Context == Context
               && string.Equals(other.TypeName, TypeName, StringComparison.OrdinalIgnoreCase)
               && string.Equals(other.PropertyName, PropertyName, StringComparison.OrdinalIgnoreCase)
               && other.Breadcrumbs.Equals(in Breadcrumbs);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is PropertyReference r && Equals(r);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Context,
            TypeName == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(TypeName),
            PropertyName == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(PropertyName),
            Breadcrumbs
        );
    }

    /// <inheritdoc />
    public override string ToString()
    {
        // todo
        return PropertyName;
    }
}
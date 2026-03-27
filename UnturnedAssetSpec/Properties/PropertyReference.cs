using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// A standardized reference to an existing property.
/// </summary>
public struct PropertyReference : IEquatable<PropertyReference>
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

    private DiscoveredDatFile? _crTargetCache;

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
    public readonly bool IsCrossReference => Context
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
    public readonly IPropertyReferenceValue CreateValue(DatProperty owner, IAssetSpecDatabase database)
    {
        if (IsCrossReference)
        {
            return new CrossedPropertyReferenceValue(in this, owner);
        }

        return new LocalPropertyReferenceValue(in this, owner, database);
    }

    public readonly IPropertyReferenceValue<TValue> CreateValue<TValue>(IType<TValue> type, DatProperty owner, IAssetSpecDatabase database)
        where TValue : IEquatable<TValue>
    {
        if (IsCrossReference)
        {
            return new CrossedPropertyReferenceValue<TValue>(in this, owner, type);
        }

        return new PropertyReferenceValue<TValue>(in this, owner, database, type);
    }

    /// <summary>
    /// Checks if this reference could possibly reference the given <paramref name="property"/>, assuming they have the same context.
    /// </summary>
    public readonly bool IsReferenceTo(DatProperty property)
    {
        if (Context != SpecPropertyContext.Unspecified && property.Context != Context || !Breadcrumbs.IsRoot)
        {
            return false;
        }

        if (TypeName != null && !QualifiedType.TypesEqual(TypeName, property.Owner.TypeName, true))
        {
            return false;
        }

        return string.Equals(property.Key, PropertyName, StringComparison.OrdinalIgnoreCase);
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

    internal static ReadOnlySpan<char> TryRemoveContext(ReadOnlySpan<char> data, out SpecPropertyContext context)
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

    public readonly void WriteToJson(Utf8JsonWriter writer)
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
    public readonly bool Equals(PropertyReference other)
    {
        return other.Context == Context
               && string.Equals(other.TypeName, TypeName, StringComparison.OrdinalIgnoreCase)
               && string.Equals(other.PropertyName, PropertyName, StringComparison.OrdinalIgnoreCase)
               && other.Breadcrumbs.Equals(in Breadcrumbs);
    }

    /// <inheritdoc />
    public readonly override bool Equals(object? obj)
    {
        return obj is PropertyReference r && Equals(r);
    }

    /// <inheritdoc />
    public readonly override int GetHashCode()
    {
        return HashCode.Combine(
            Context,
            TypeName == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(TypeName),
            PropertyName == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(PropertyName),
            Breadcrumbs
        );
    }

    /// <inheritdoc />
    public readonly override string ToString()
    {
        string specifier = CreateContextSpecifier(Context);
        string propertyName = !Breadcrumbs.IsRoot ? Breadcrumbs.ToString(false, PropertyName) : PropertyName;

        if (TypeName == null && specifier.Length == 0)
            return propertyName;

        return $"{specifier}{TypeName}{propertyName}";
    }

    /// <summary>
    /// Attempts to find the file referenced by this cross-reference property reference.
    /// </summary>
    /// <param name="owner">The property this reference is defined in.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="database">Specification database.</param>
    /// <param name="file">The found file.</param>
    /// <returns>Whether or not a file was found.</returns>
    /// <exception cref="InvalidOperationException">This <see cref="PropertyReference"/> is not a cross-reference property reference.</exception>
    public bool TryGetCrossReferencedTarget(DatProperty owner, ref FileEvaluationContext ctx, IAssetSpecDatabase database, [NotNullWhen(true)] out DiscoveredDatFile? file)
    {
        if (!IsCrossReference)
            throw new InvalidOperationException("This property-reference is not a cross-referencing property.");

        file = null;

        DiscoveredDatFile? target = _crTargetCache;

        FileCrossRefVisitor v;
        v.Services = ctx.Services;
        v.Owner = owner;
        v.Id = GuidOrId.Empty;

        if (owner.CrossReferenceTarget == null || !owner.CrossReferenceTarget.VisitValue(ref v, ref ctx) || v.Id.IsNull)
            return false;

        if (target == null || target.IsRemoved || (v.Id.IsId ? target.Id != v.Id.Id || target.Category != v.Id.Category : target.Guid != v.Id.Guid))
        {
            OneOrMore<DiscoveredDatFile> files = ctx.Services.Installation.FindFile(v.Id);
            if (!files.IsSingle)
                return false;

            file = files.Value;
            Interlocked.CompareExchange(ref _crTargetCache, file, target);
            target = _crTargetCache;
        }

        file = target;
        return target != null;
    }

    /// <summary>
    /// Attempts to resolve the property pointed at by this <see cref="PropertyReference"/>.
    /// </summary>
    /// <param name="owner">The property this reference is defined in.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="property">The resolved property.</param>
    /// <returns>Whether or not a property was found.</returns>
    public bool TryGetProperty(DatProperty owner, ref FileEvaluationContext ctx, [NotNullWhen(true)] out DatProperty? property)
    {
        return TryGetProperty(owner, ref ctx, out property, null);
    }

    /// <inheritdoc cref="TryGetProperty(DatProperty,ref FileEvaluationContext,out DatProperty)"/>
    internal bool TryGetProperty(DatProperty owner, ref FileEvaluationContext ctx, [NotNullWhen(true)] out DatProperty? property, DatFileType? crossRefFileType)
    {
        DatTypeWithProperties objectOwner = owner.Owner;
        IDatSpecificationObject contextObject = owner;
        SpecPropertyContext context = Context;

        if (IsCrossReference)
        {
            if (crossRefFileType == null)
            {
                if (!TryGetCrossReferencedTarget(owner, ref ctx, ctx.Services.Database, out DiscoveredDatFile? target))
                {
                    property = null;
                    return false;
                }

                QualifiedType type = target.Type;
                if (!ctx.Services.Database.FileTypes.TryGetValue(type, out DatFileType? fileType))
                {
                    property = null;
                    return false;
                }

                objectOwner = fileType;
                contextObject = fileType;
            }
            else
            {
                objectOwner = crossRefFileType;
                contextObject = crossRefFileType;
            }

            context = context switch
            {
                SpecPropertyContext.CrossReferenceProperty => SpecPropertyContext.Property,
                SpecPropertyContext.CrossReferenceLocalization => SpecPropertyContext.Localization,
                _ => SpecPropertyContext.Unspecified
            };
        }

        if (TypeName != null)
        {
            if (!ctx.Services.Database.TryFindType(new QualifiedType(TypeName, true), out DatType? type, contextObject)
                || type is not DatTypeWithProperties props)
            {
                ctx.Services.LoggerFactory.CreateLogger<LocalPropertyReferenceValue>().LogError(
                    "Unknown type name in property reference \"{0}\" from property \"{1}\".",
                    ToString(),
                    ((IDatSpecificationObject)owner).FullName
                );
                property = null;
                return false;
            }

            objectOwner = props;
        }

        if (context is not SpecPropertyContext.BundleAsset and not SpecPropertyContext.Localization and not SpecPropertyContext.Property)
            context = owner.Context;

        if (!objectOwner.TryFindProperty(PropertyName, context, out property))
        {
            return false;
        }

        Breadcrumbs.ResolveFromPropertyRef(IsCrossReference ? objectOwner : owner.Owner, ctx.Services.Database, context);
        return true;
    }

    private struct FileCrossRefVisitor : IValueVisitor
    {
        public IParsingServices Services;
        public GuidOrId Id;
        public DatProperty Owner;

        public void Accept<TValue>(IType<TValue> type, Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            if (!value.HasValue)
            {
                return;
            }

            if (typeof(TValue) == typeof(Guid))
            {
                Guid guid = Unsafe.As<Optional<TValue>, Optional<Guid>>(ref value).Value;
                Id = new GuidOrId(guid);
                return;
            }

            AssetCategoryValue category = AssetCategoryValue.None;

            if (Owner.CrossReferenceTarget is IPropertyReferenceValue pref)
            {
                DatProperty referencedProperty = pref.Owner;
                if (referencedProperty.Type is IAssetReferenceType arType)
                {
                    arType.TryGetTargetCategory(Services.Database, out category);
                }
            }
            else if (Owner.DataRoot.ValueKind == JsonValueKind.Object
                     && Owner.DataRoot.TryGetProperty("FileCrossRefCategory"u8, out JsonElement element)
                     && element.ValueKind == JsonValueKind.String
                     && AssetCategory.TryParse(element.GetString(), out int categoryIndex))
            {
                category = new AssetCategoryValue(categoryIndex);
            }

            if (typeof(TValue) == typeof(GuidOrId))
            {
                GuidOrId guidOrId = Unsafe.As<Optional<TValue>, Optional<GuidOrId>>(ref value).Value;
                if (guidOrId.IsNull)
                    return;

                if (!guidOrId.IsId || guidOrId.Category != 0)
                {
                    Id = guidOrId;
                    return;
                }

                if (category.Index == 0)
                    return;

                Id = new GuidOrId(guidOrId.Id, category);
                return;
            }

            if (category.Index == 0 || !ConvertVisitor<ushort>.TryConvert(value.Value, out ushort id) || id == 0)
                return;

            Id = new GuidOrId(id, category);
        }
    }
}
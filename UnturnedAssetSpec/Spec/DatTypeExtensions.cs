using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Collections.Immutable;
using System.ComponentModel;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Extension methods for the various subclasses of <see cref="DatType"/>.
/// </summary>
public static class DatTypeExtensions
{
    /// <summary>
    /// Gets the properties of the given type, or an empty array if that type doesn't have that kind of properties.
    /// </summary>
    /// <param name="type">The type to get properties from.</param>
    /// <param name="context">The kind of properties to get.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="context"/> is asking for <see cref="SpecPropertyContext.BundleAsset"/> properties which are not returned by this function.</exception>
    /// <exception cref="InvalidEnumArgumentException"/>
    /// <exception cref="ArgumentNullException"/>
    public static ImmutableArray<DatProperty> GetPropertyArray(this DatType type, SpecPropertyContext context)
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

using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public interface ISpecType : IEquatable<ISpecType?>, IAdditionalPropertyProvider
{
    QualifiedType Parent { get; }
    QualifiedType Type { get; }
    string DisplayName { get; }
    string? Docs { get; }

    Version? Version { get; }

    AssetSpecType Owner { get; set; }

    SpecProperty? FindProperty(string propertyName, SpecPropertyContext context);
}

public interface IPropertiesSpecType : ISpecType
{
    SpecProperty[] Properties { get; set; }
    SpecProperty[] LocalizationProperties { get; set; }
    SpecBundleAsset[] BundleAssets { get; set; }
}

public static class SpecTypeExtensions
{
    public static SpecProperty[] GetProperties(this ISpecType specType, SpecPropertyContext context)
    {
        return context switch
        {
            SpecPropertyContext.Property or SpecPropertyContext.CrossReferenceProperty
                => (specType as IPropertiesSpecType)?.Properties ?? Array.Empty<SpecProperty>(),
            SpecPropertyContext.Localization or SpecPropertyContext.CrossReferenceLocalization
                => (specType as IPropertiesSpecType)?.LocalizationProperties ?? Array.Empty<SpecProperty>(),
            // ReSharper disable CoVariantArrayConversion
            SpecPropertyContext.BundleAsset
                => (specType as IPropertiesSpecType)?.BundleAssets ?? Array.Empty<SpecBundleAsset>(),
            // ReSharper restore CoVariantArrayConversion
            _ => throw new ArgumentOutOfRangeException(nameof(context))
        };
    }

    public static void SetProperties(this ISpecType specType, SpecProperty[] properties, SpecPropertyContext context)
    {
        switch (context)
        {
            case SpecPropertyContext.CrossReferenceProperty:
            case SpecPropertyContext.Property:
                if (specType is IPropertiesSpecType st)
                    st.Properties = properties;
                break;

            case SpecPropertyContext.CrossReferenceLocalization:
            case SpecPropertyContext.Localization:
                if (specType is IPropertiesSpecType st2)
                    st2.LocalizationProperties = properties;
                break;

            // ReSharper disable CoVariantArrayConversion
            case SpecPropertyContext.BundleAsset:
                if (specType is IPropertiesSpecType st3)
                    st3.BundleAssets = (SpecBundleAsset[])properties;
                break;

            default:
                // ReSharper restore CoVariantArrayConversion
                throw new ArgumentOutOfRangeException(nameof(context));
        }
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

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
}

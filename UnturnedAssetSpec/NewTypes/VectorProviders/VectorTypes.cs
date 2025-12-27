using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Keeps track of <see cref="IVectorTypeProvider{TVector}"/> registrations for each vector type.
/// </summary>
public static class VectorTypes
{
    /// <summary>
    /// Gets the <see cref="IVectorTypeProvider{TVector}"/> associated with a vector type (<typeparamref name="TVector"/>), throwing an <see cref="InvalidOperationException"/> if the type isn't a registered vector type.
    /// </summary>
    /// <remarks>Annotate vector types with a <see cref="VectorTypeProviderAttribute"/> to designate a provider for a vector type, using <see cref="TypeDescriptor"/> if necessary.</remarks>
    /// <exception cref="InvalidOperationException"><typeparamref name="TVector"/> is not a registered vector type.</exception>
    public static IVectorTypeProvider<TVector> GetProvider<TVector>() where TVector : IEquatable<TVector>
    {
        return TryGetProvider<TVector>() ?? throw new InvalidOperationException(
            string.Format(
                Resources.InvalidOperationException_InvalidVectorType,
                typeof(TVector).FullName,
                nameof(VectorTypeProviderAttribute)
            )
        );
    }

    /// <summary>
    /// Gets the <see cref="IVectorTypeProvider{TVector}"/> associated with a vector type (<typeparamref name="TVector"/>), returning <see langword="null"/> if the type isn't a registered vector type.
    /// </summary>
    /// <remarks>Annotate vector types with a <see cref="VectorTypeProviderAttribute"/> to designate a provider for a vector type, using <see cref="TypeDescriptor"/> if necessary.</remarks>
    public static IVectorTypeProvider<TVector>? TryGetProvider<TVector>() where TVector : IEquatable<TVector>
    {
        return VectorTypeCache<TVector>.HasProvider
            ? VectorTypeCache<TVector>.Provider
            : VectorTypeCache<TVector>.CreateProvider();
    }

    internal static bool TryParseFloatArg<T>(ref TypeParserArgs<T> args, out float value, IPropertySourceNode property) where T : IEquatable<T>
    {
        switch (property.Value)
        {
            default:
                args.DiagnosticSink?.UNT2004_NoValue(ref args, property);
                break;

            case IListSourceNode l:
                args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, l, Float32Type.Instance);
                break;

            case IDictionarySourceNode d:
                args.DiagnosticSink?.UNT2004_DictionaryInsteadOfValue(ref args, d, Float32Type.Instance);
                break;

            case IValueSourceNode v:
                if (float.TryParse(v.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                    return true;

                args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, Float32Type.Instance);
                return false;

        }

        value = 0f;
        return false;
    }

    static VectorTypes()
    {
        TypeDescriptor.AddAttributes(typeof(Vector2), new VectorTypeProviderAttribute(typeof(Vector2Provider)));
        TypeDescriptor.AddAttributes(typeof(Vector3), new VectorTypeProviderAttribute(typeof(Vector3Provider)));
        TypeDescriptor.AddAttributes(typeof(Vector4), new VectorTypeProviderAttribute(typeof(Vector4Provider)));
    }

    private static class VectorTypeCache<TVector> where TVector : IEquatable<TVector>
    {
        public static IVectorTypeProvider<TVector>? Provider;
        public static bool HasProvider;

        public static IVectorTypeProvider<TVector>? CreateProvider()
        {
            IVectorTypeProvider<TVector>? provider = null;
            AttributeCollection attrs = TypeDescriptor.GetAttributes(typeof(TVector));
            foreach (Attribute a in attrs)
            {
                if (a is VectorTypeProviderAttribute attr
                    && attr.Type != null
                    && typeof(IVectorTypeProvider<TVector>).IsAssignableFrom(attr.Type)
                    && attr.Type.GetConstructor(Type.EmptyTypes) is { } emptyCtor)
                {
                    provider = (IVectorTypeProvider<TVector>?)emptyCtor.Invoke(Array.Empty<object>());
                    break;
                }
            }

            Provider = provider;
            HasProvider = true;
            return Provider;
        }
    }
}

/// <summary>
/// Defines the <see cref="IVectorTypeProvider{TVector}"/> to use for this type.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public sealed class VectorTypeProviderAttribute(
#if NET5_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
    Type type) : Attribute
{
    /// <summary>
    /// The <see cref="IVectorTypeProvider{TVector}"/> to use for this type.
    /// </summary>
    public Type Type { get; } = type;
}
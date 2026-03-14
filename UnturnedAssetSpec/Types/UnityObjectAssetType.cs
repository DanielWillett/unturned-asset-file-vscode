using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A <see cref="IBundleAssetType"/> for a unity object.
/// </summary>
public sealed class UnityObjectAssetType : IBundleAssetType, ITypeFactory
{
    private readonly QualifiedType _type;

    private static readonly ImmutableDictionary<QualifiedType, UnityObjectAssetType> CommonTypes;

    static UnityObjectAssetType()
    {
        ImmutableDictionary<QualifiedType, UnityObjectAssetType>.Builder bldr = ImmutableDictionary.CreateBuilder<QualifiedType, UnityObjectAssetType>();

        Add(bldr, new QualifiedType("UnityEngine.Object, UnityEngine.CoreModule", true));
        Add(bldr, new QualifiedType("UnityEngine.GameObject, UnityEngine.CoreModule", true));
        Add(bldr, new QualifiedType("UnityEngine.AudioClip, UnityEngine.AudioModule", true));
        Add(bldr, new QualifiedType("UnityEngine.Texture2D, UnityEngine.CoreModule", true));
        Add(bldr, new QualifiedType("UnityEngine.Material, UnityEngine.CoreModule", true));
        Add(bldr, new QualifiedType("UnityEngine.Mesh, UnityEngine.CoreModule", true));

        CommonTypes = bldr.ToImmutable();

        static void Add(ImmutableDictionary<QualifiedType, UnityObjectAssetType>.Builder bldr, QualifiedType type)
        {
            bldr[type] = new UnityObjectAssetType(type);
        }
    }

    /// <inheritdoc />
    public QualifiedType TypeName => _type;

    string IType.Id => _type.Type;
    string IType.DisplayName => _type.GetTypeName();

    /// <summary>
    /// Type factory for <see cref="UnityObjectAssetType"/> types.
    /// </summary>
    public static ITypeFactory Factory { get; } = new UnityObjectAssetType(QualifiedType.None);

    internal UnityObjectAssetType(QualifiedType type)
    {
        _type = type;
    }

    /// <summary>
    /// Create a <see cref="IBundleAssetType"/> for a unity object.
    /// </summary>
    /// <param name="typeId">The type of object to create.</param>
    /// <returns>A <see cref="UnityObjectAssetType"/> implementation of <see cref="IBundleAssetType"/> for the given type name. May be cached.</returns>
    public static UnityObjectAssetType Create(QualifiedType typeId)
    {
        typeId = typeId.CaseInsensitive.Normalized;
        if (CommonTypes.TryGetValue(typeId, out UnityObjectAssetType? t))
            return t;

        return new UnityObjectAssetType(typeId);
    }

    /// <inheritdoc />
    bool IEquatable<IPropertyType?>.Equals(IPropertyType? other)
    {
        return other is UnityObjectAssetType bt && _type.Equals(bt.TypeName);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is UnityObjectAssetType bt && _type.Equals(bt.TypeName);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _type.GetHashCode();
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context = "")
    {
        return Create(new QualifiedType(typeId, true));
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStringValue(_type.Type);
    }

    PropertySearchTrimmingBehavior IPropertyType.TrimmingBehavior => PropertySearchTrimmingBehavior.ExactPropertyOnly;

    void IType.Visit<TVisitor>(ref TVisitor visitor) { }

    bool IPropertyType.TryGetConcreteType([NotNullWhen(true)] out IType? type)
    {
        type = this;
        return true;
    }

    bool IPropertyType.TryEvaluateType([NotNullWhen(true)] out IType? type, ref FileEvaluationContext ctx)
    {
        type = this;
        return true;
    }

    bool IEquatable<IType?>.Equals(IType? other)
    {
        return other is UnityObjectAssetType bt && _type.Equals(bt.TypeName);
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning disable CA2231

/// <summary>
/// Checks whether or not the given bundle asset contains a component.
/// <para>
/// Supported properties:
/// <list type="bullet">
///     <item><see cref="QualifiedType"/> Type - Type of component to check for.</item>
///     <item><see cref="string"/> Path - Path to the target object from the original object (Transform.Find).</item>
///     <item><see cref="string"/> Property - Property to get the value of. Supports nested properties via dot-notation and indexers.</item>
///     <item><see langword="any"/> Fallback - Fallback value if the property or component can't be found.</item>
///     <item><see cref="bool"/> InParents - Include all parent objects in the search. Default: <see langword="false"/>.</item>
///     <item><see cref="bool"/> InChildren - Include all child objects in the search. Default: <see langword="false"/>.</item>
/// </list>
/// </para>
/// <para>
/// Supported targets:
/// <list type="bullet">
///     <item><c>#Self</c></item>
///     <item>Any property reference.</item>
/// </list>
/// </para>
/// <para>
/// Syntax:<br/>
/// <c>#Target.Component</c><br/>
/// <c>#Target.Component{"Type":"UnityEngine.AudioSource, UnityEngine.AudioModule","Property":"m_audioClip.m_Name","Fallback":""}</c><br/>
/// <c>#Target.Component{"Type":"UnityEngine.AudioSource, UnityEngine.AudioModule"}</c>
/// <c>#Target.Component{"Type":"UnityEngine.MeshCollider, UnityEngine.CoreModule","Path":"Alive/Model_0"}</c>
/// </para>
/// </summary>
public readonly struct ComponentProperty : IConfigurableDataRefProperty, IEquatable<ComponentProperty>
{
    /// <summary>
    /// The type of component to get.
    /// </summary>
    /// <remarks>Case sensitive.</remarks>
    public QualifiedType Type { get; }

    /// <summary>
    /// The path to the object to look for the component on.
    /// </summary>
    /// <remarks>Case sensitive.</remarks>
    public string? Path { get; }

    /// <summary>
    /// The properties to get the value of. Can use dot-notation and indexers to access nested objects.
    /// <para>
    /// If <see langword="null"/> or empty, this property will return
    /// a boolean indicating whether or not the component is attached to the object.
    /// </para>
    /// </summary>
    public string? Property { get; }

    /// <summary>
    /// Fallback value if the component isn't attached to the object.
    /// </summary>
    public object? Fallback { get; }

    /// <summary>
    /// Whether or not <see cref="Fallback"/> was assigned.
    /// </summary>
    public bool HasFallback => (Flags & 1) != 0;
    
    /// <summary>
    /// Whether or not to include parent objects in the component search.
    /// </summary>
    public bool InParents => (Flags & 2) != 0;
    
    /// <summary>
    /// Whether or not to include child objects in the component search.
    /// </summary>
    public bool InChildren => (Flags & 4) != 0;
    
    /// <summary>
    /// Various boolean flags for this property.
    /// </summary>
    public int Flags { get; }

    /// <inheritdoc />
    public string PropertyName => "Component";

    public ComponentProperty(
        QualifiedType type,
        string? property,
        string? path,
        int flags,
        object? fallback = null)
    {
        Type = type;
        Property = property;
        Path = path;
        Flags = flags;
        if (HasFallback)
            Fallback = fallback;
    }

    /// <inheritdoc />
    public OneOrMore<KeyValuePair<string, object?>> Options
    {
        get
        {
            KeyValuePair<string, object?> componentType = new KeyValuePair<string, object?>(nameof(Type), Type.Type);
            int ct = 1;
            if (!string.IsNullOrEmpty(Property))
                ++ct;
            if (!string.IsNullOrEmpty(Path))
                ++ct;
            if (HasFallback)
                ++ct;
            if (InParents)
                ++ct;
            if (InChildren)
                ++ct;

            if (ct == 1)
            {
                return new OneOrMore<KeyValuePair<string, object?>>(componentType);
            }

            KeyValuePair<string, object?>[] pairs = new KeyValuePair<string, object?>[ct];

            pairs[0] = componentType;
            int index = 0;
            if (!string.IsNullOrEmpty(Property))
                pairs[++index] = new KeyValuePair<string, object?>(nameof(Property), Property);
            if (!string.IsNullOrEmpty(Path))
                pairs[++index] = new KeyValuePair<string, object?>(nameof(Path), Path);
            if (HasFallback)
                pairs[++index] = new KeyValuePair<string, object?>(nameof(Fallback), Fallback);
            if (InParents)
                pairs[++index] = new KeyValuePair<string, object?>(nameof(InParents), BoxedPrimitives.True);
            if (InChildren)
                pairs[++index] = new KeyValuePair<string, object?>(nameof(InChildren), BoxedPrimitives.True);

            return new OneOrMore<KeyValuePair<string, object?>>(pairs);
        }
    }

    /// <inheritdoc />
    public bool Equals(ComponentProperty other)
    {
        return Type.Equals(other.Type)
               && string.Equals(Property, other.Property, StringComparison.Ordinal)
               && Flags == other.Flags
               && string.Equals(Path, other.Path, StringComparison.Ordinal)
               && (!HasFallback || (Fallback?.Equals(other.Fallback) ?? other.Fallback == null)
        );
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ComponentProperty prop && Equals(prop);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HasFallback
            ? HashCode.Combine(2068625652, Type, Property, Path, Flags, Fallback)
            : HashCode.Combine(2068625652, Type, Property, Path, Flags);
    }

    internal static ComponentProperty Create(OneOrMore<KeyValuePair<string, object?>> properties)
    {
        if (!properties.TryGetValue(nameof(Type), out object? typeId) || typeId is not string typeIdStr)
        {
            throw new InvalidOperationException("Expected string value for \"Type\".");
        }

        string? propNames = null;
        if (properties.TryGetValue(nameof(Property), out object? stringBox))
        {
            propNames = stringBox?.ToString();
        }

        int flags = 0;
        if (properties.TryGetValue(nameof(InParents), out object? bVal) && bVal is true)
        {
            flags |= 2;
        }
        if (properties.TryGetValue(nameof(InChildren), out bVal) && bVal is true)
        {
            flags |= 4;
        }
        if (properties.TryGetValue(nameof(Fallback), out object? fallback))
        {
            flags |= 1;
        }

        string? path = null;
        if (properties.TryGetValue(nameof(Path), out stringBox))
        {
            path = stringBox?.ToString();
        }

        return new ComponentProperty(
            type: new QualifiedType(typeIdStr),
            property: propNames,
            path: path,
            flags: flags,
            fallback: fallback
        );
    }

#pragma warning disable CS8500
    /// <summary>
    /// Visit the result of a <see cref="ComponentProperty"/> for an asset.
    /// </summary>
    /// <param name="bundleAsset">Asset to resolve.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="visitor">Visitor to accept the value.</param>
    /// <returns>Whether or not the visitor was invoked.</returns>
    public unsafe bool GetValue<TVisitor>(DatBundleAsset bundleAsset, ref FileEvaluationContext ctx, ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        IBundleProxy bundle = ctx.File.WorkspaceFile.Bundle;
        using UnityObject? obj = bundle.GetCorrespondingAsset(bundleAsset, ref ctx);

        if (obj == null)
        {
            return VisitFallback(ref visitor);
        }

        UnityTransform.QueryComponentOptions options = UnityTransform.QueryComponentOptions.None;

        if (InParents)
            options |= UnityTransform.QueryComponentOptions.InParents;
        else if (InChildren)
            options |= UnityTransform.QueryComponentOptions.InChildren;

        UnityTransform? transform = obj.Transform;
        if (!string.IsNullOrEmpty(Path) && transform != null)
        {
            transform = transform.Find(Path);
        }

        if (transform == null || !transform.TryGetComponent(Type, out UnityComponent? component, options))
        {
            return VisitFallback(ref visitor);
        }

        if (string.IsNullOrWhiteSpace(Property))
        {
            visitor.Accept(BooleanType.Instance, new Optional<bool>(component != null));
            return true;
        }

        ValueVisitor<TVisitor> valueVisitor;
        valueVisitor.Visited = false;

        bool success;

        fixed (TVisitor* visitorPtr = &visitor)
        {
            valueVisitor.Visitor = visitorPtr;
            success = component.TryReadProperty(Property, ref valueVisitor);
        }

        if (success && valueVisitor.Visited)
        {
            return true;
        }

        return VisitFallback(ref visitor);
    }

    private unsafe struct ValueVisitor<TVisitor> : IGenericVisitor
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        public bool Visited;
        public TVisitor* Visitor;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            IType<T>? type = CommonTypes.TryGetDefaultValueType<T>();
            if (type == null)
                return;

            Visitor->Accept(type, new Optional<T>(value));
            Visited = true;
        }
    }

#pragma warning restore CS8500

    private bool VisitFallback<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (!HasFallback)
        {
            return false;
        }

        object? fb = Fallback;
        if (fb == null)
        {
            visitor.Accept(StringType.Instance, Optional<string>.Null);
            return true;
        }

        if (fb is IConvertible conv)
        {
            TypeCode typeCode = conv.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.DBNull:
                    visitor.Accept(StringType.Instance, Optional<string>.Null);
                    return true;
                case TypeCode.Boolean:
                    visitor.Accept(BooleanType.Instance, (bool)fb);
                    return true;
                case TypeCode.Char:
                    visitor.Accept(CharacterType.Instance, (char)fb);
                    return true;
                case TypeCode.SByte:
                    visitor.Accept(Int8Type.Instance, (sbyte)fb);
                    return true;
                case TypeCode.Byte:
                    visitor.Accept(UInt8Type.Instance, (byte)fb);
                    return true;
                case TypeCode.Int16:
                    visitor.Accept(Int16Type.Instance, (short)fb);
                    return true;
                case TypeCode.UInt16:
                    visitor.Accept(UInt16Type.Instance, (ushort)fb);
                    return true;
                case TypeCode.Int32:
                    visitor.Accept(Int32Type.Instance, (int)fb);
                    return true;
                case TypeCode.UInt32:
                    visitor.Accept(UInt32Type.Instance, (uint)fb);
                    return true;
                case TypeCode.Int64:
                    visitor.Accept(Int64Type.Instance, (long)fb);
                    return true;
                case TypeCode.UInt64:
                    visitor.Accept(UInt64Type.Instance, (ulong)fb);
                    return true;
                case TypeCode.Single:
                    visitor.Accept(Float32Type.Instance, (float)fb);
                    return true;
                case TypeCode.Double:
                    visitor.Accept(Float64Type.Instance, (double)fb);
                    return true;
                case TypeCode.Decimal:
                    visitor.Accept(Float128Type.Instance, (decimal)fb);
                    return true;
                case TypeCode.DateTime:
                    visitor.Accept(DateTimeType.Instance, (DateTime)fb);
                    return true;
                case (TypeCode)17:
                    if (fb is not TimeSpan ts)
                        goto default;
                    visitor.Accept(TimeSpanType.Instance, ts);
                    return true;
                case TypeCode.String:
                    visitor.Accept(StringType.Instance, (string)fb);
                    return true;
                case TypeCode.Object:
                default:
                    return false;
            }
        }

        switch (fb)
        {
            case Guid guid:
                visitor.Accept(GuidType.Instance, guid);
                return true;

            case DateTimeOffset dto:
                visitor.Accept(DateTimeOffsetType.Instance, dto);
                return true;

            // not supporting arrays thats way too much work
        }

        return false;
    }

    /// <inheritdoc />
#if NET7_0_OR_GREATER
    public static IDataRef CreateDataRef(
#else
    public IDataRef CreateDataRef(
#endif
        IDataRefTarget target,
        OneOrMore<int> indices,
        OneOrMore<KeyValuePair<string, object?>> properties)
    {
        return new DataRefProperty<ComponentProperty>(target, Create(properties));
    }

    /// <inheritdoc />
#if NET7_0_OR_GREATER
    public static IDataRef<TValue> CreateDataRef<TValue>(
#else
    public IDataRef<TValue> CreateDataRef<TValue>(
#endif
        IType<TValue> type,
        IDataRefTarget target,
        OneOrMore<int> indices,
        OneOrMore<KeyValuePair<string, object?>> properties
    ) where TValue : IEquatable<TValue>
    {
        return new DataRefProperty<ComponentProperty, TValue>(type, target, Create(properties));
    }
}
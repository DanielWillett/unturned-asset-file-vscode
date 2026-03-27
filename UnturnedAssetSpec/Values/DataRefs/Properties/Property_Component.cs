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
///     <item><see cref="bool"/> RequireValue - Whether or not the property must also have a valid value to count as 'included'.</item>
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
    /// The properties to get the value of, in order.
    /// If empty this property, returns a boolean indicating whether or not the component is attached to the object.
    /// </summary>
    public OneOrMore<string> Properties { get; }

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
        OneOrMore<string> properties,
        string? path,
        int flags,
        object? fallback = null)
    {
        Type = type;
        Properties = properties;
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
            int ct = HasFallback ? 4 : 3;

            if (Properties.IsNull)
                --ct;
            if (Path == null)
                --ct;

            if (ct == 1)
            {
                return new OneOrMore<KeyValuePair<string, object?>>(componentType);
            }

            KeyValuePair<string, object?>[] pairs = new KeyValuePair<string, object?>[ct];

            pairs[0] = componentType;

            pairs[1] = new KeyValuePair<string, object?>(
                nameof(Properties),
                Properties.IsSingle ? Properties[0] : string.Join(".", Properties.Values)
            );

            int index = 1;

            if (!Properties.IsNull)
                pairs[++index] = new KeyValuePair<string, object?>(nameof(Fallback), Fallback);

            if (Path != null)
                pairs[++index] = new KeyValuePair<string, object?>(nameof(Path), Path);

            return new OneOrMore<KeyValuePair<string, object?>>(pairs);
        }
    }

    /// <inheritdoc />
    public bool Equals(ComponentProperty other)
    {
        return Type.Equals(other.Type)
               && Properties.Equals(other.Properties, StringComparison.Ordinal)
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
        return HashCode.Combine(2068625652, Type, Properties, Path, Flags, Fallback);
    }

    internal static ComponentProperty Create(OneOrMore<KeyValuePair<string, object?>> properties)
    {
        if (!properties.TryGetValue(nameof(Type), out object? typeId) || typeId is not string typeIdStr)
        {
            throw new InvalidOperationException("Expected string value for \"Type\".");
        }

        OneOrMore<string> propNames = OneOrMore<string>.Null;
        if (properties.TryGetValue(nameof(Properties), out object? stringBox) && stringBox is string propNameStr)
        {
            if (propNameStr.IndexOf('.') < 0)
            {
                propNames = new OneOrMore<string>(propNameStr);
            }
            else
            {
                string[] split = propNameStr.Split([ '.' ], StringSplitOptions.RemoveEmptyEntries);
                propNames = new OneOrMore<string>(split);
            }
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
        if (properties.TryGetValue(nameof(Properties), out stringBox))
        {
            path = stringBox as string;
        }

        return new ComponentProperty(
            type: new QualifiedType(typeIdStr),
            properties: propNames,
            path: path,
            flags: flags,
            fallback: fallback
        );
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
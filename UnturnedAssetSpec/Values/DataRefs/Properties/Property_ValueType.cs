using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning disable CA2231

/// <summary>
/// Returns a string value indicating which type of value the property provides, which is one of the following: <c>Value</c>, <c>List</c>, or <c>Dictionary</c>.
/// <br/>By default 'Value' will be returned if the property isn't present or doesn't have any kind of value.
/// <para>
/// Supported targets:
/// <list type="bullet">
///     <item><c>#Self</c></item>
///     <item>Any non-cross-referenced property reference.</item>
/// </list>
/// </para>
/// <para>
/// Syntax:<br/>
/// <c>#Target.ValueType</c>
/// </para>
/// </summary>
public readonly struct ValueTypeProperty : IDataRefProperty, IEquatable<ValueTypeProperty>
{
    /// <inheritdoc />
    public string PropertyName => "ValueType";

    /// <inheritdoc />
    public bool Equals(ValueTypeProperty other) => true;

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ValueTypeProperty;

    /// <inheritdoc />
    public override int GetHashCode() => 1490754887;

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
        return new DataRefProperty<ValueTypeProperty>(target, default);
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
        return new DataRefProperty<ValueTypeProperty, TValue>(type, target, default);
    }

    internal static string GetTypeName(SourceValueType valueType)
    {
        return valueType switch
        {
            SourceValueType.Dictionary => "Dictionary",
            SourceValueType.List => "List",
            _ => "Value"
        };
    }
}
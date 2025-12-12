using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Base interface for a parsable property type.
/// </summary>
public interface IType
{
    /// <summary>
    /// ID of the type which would be written in the Type field in JSON.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Localized user-facing display name.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Calls <see cref="ITypeVisitor.Accept"/> on the <paramref name="visitor"/> for this type if it's strongly typed.
    /// </summary>
    /// <remarks>Used to create a generic context for a <see cref="IType{T}"/> implementation.</remarks>
    void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : ITypeVisitor;
}

/// <summary>
/// A strongly-typed parsable property type.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface IType<TValue>
    : IType
    where TValue : IEquatable<TValue>
{
    /// <summary>
    /// The converter used to parse values for this type.
    /// </summary>
    ITypeParser<TValue> Parser { get; }

    /// <summary>
    /// Creates a dynamic value from a concrete value.
    /// </summary>
    IValue<TValue> CreateValue(Optional<TValue> value);
    
    /// <summary>
    /// Attempts to read a value of this type from a <see cref="Utf8JsonReader"/>.
    /// </summary>
    bool TryValueFromJson(ref Utf8JsonReader reader, [MaybeNullWhen(false)] out IValue<TValue> value);
}


public interface ITypeVisitor
{
    void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>;
}
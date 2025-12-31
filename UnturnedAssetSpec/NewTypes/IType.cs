using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Base interface for a parsable property type.
/// </summary>
public interface IType : IPropertyType, IEquatable<IType?>
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

    /// <summary>
    /// Writes all the information for this type instance to a JSON string or object.
    /// </summary>
    void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options);
}

/// <summary>
/// A factory that can create types from JSON information.
/// </summary>
public interface ITypeFactory
{
    /// <summary>
    /// Resolves the type.
    /// </summary>
    /// <remarks>Some factories will just return themselves.</remarks>
    IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context = "");
}

/// <summary>
/// A strongly-typed parsable property type.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public interface IType<TValue> : IType where TValue : IEquatable<TValue>
{
    /// <summary>
    /// The converter used to parse values for this type.
    /// </summary>
    ITypeParser<TValue> Parser { get; }

    /// <summary>
    /// Creates a dynamic value from a concrete value.
    /// </summary>
    IValue<TValue> CreateValue(Optional<TValue> value);
}


public interface ITypeVisitor
{
    void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>;
}
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A single-instance, value-only type which is represented by a built-in CLR type.
/// </summary>
/// <typeparam name="TValue">The CLR type to parse.</typeparam>
/// <typeparam name="TSelf">The implementing type.</typeparam>
public abstract class PrimitiveType<TValue, TSelf>
    : BaseType<TValue, TSelf>, ITypeFactory
    where TValue : IEquatable<TValue>
    where TSelf : PrimitiveType<TValue, TSelf>, new()
{
    /// <summary>
    /// Singleton instance of this type.
    /// </summary>
    public static TSelf Instance { get; } = new TSelf();
    static PrimitiveType() { }

    /// <summary>
    /// Singleton instance of the <see langword="null"/> value of this type.
    /// </summary>
    [field: MaybeNull]
    public static NullValue<TValue> Null => field ??= new NullValue<TValue>(Instance);

    public override ITypeParser<TValue> Parser => TypeParsers.Get<TValue>();

#pragma warning disable CS0659

    /// <inheritdoc cref="IEquatable{T}"/>
    protected override bool Equals(TSelf other) => true;

    /// <inheritdoc />
    public override IValue<TValue> CreateValue(Optional<TValue> value)
    {
        return value.HasValue
            ? Values.Values.Create(value.Value, this)
            : Null;
    }

    /// <inheritdoc cref="ITypeFactory.CreateType"/>
    protected virtual IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context) => this;

    /// <inheritdoc />
    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Id);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context)
    {
        return CreateType(in typeDefinition, typeId, spec, owner, context);
    }

#pragma warning restore CS0659
}
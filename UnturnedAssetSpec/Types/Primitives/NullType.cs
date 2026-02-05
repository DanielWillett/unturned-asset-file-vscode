using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A type that can be any type.
/// </summary>
/// <remarks>Used by the <c>Asset</c> and <c>Metadata</c> pseudo-properties.</remarks>
internal sealed class NullType : IType, ITypeFactory
{
    public static readonly NullType Instance = new NullType();
    public string Id => TypeId;
    public string DisplayName => Resources.Type_Name_NullType;

    public const string TypeId = "Any";

    static NullType() { }

    /// <inheritdoc />
    public bool TryGetConcreteType([NotNullWhen(true)] out IType? type)
    {
        type = this;
        return true;
    }

    /// <inheritdoc />
    public bool TryEvaluateType([NotNullWhen(true)] out IType? type, in FileEvaluationContext ctx)
    {
        type = this;
        return true;
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStringValue(TypeId);
    }

    PropertySearchTrimmingBehavior IType.TrimmingBehavior => PropertySearchTrimmingBehavior.ExactPropertyOnly;

    public bool Equals(IType? other) => other is NullType;
    public bool Equals(IPropertyType? other) => other is NullType;
    public override bool Equals(object? obj) => obj is NullType;
    public override int GetHashCode() => 1968905342;
    void IType.Visit<TVisitor>(ref TVisitor visitor) { }
    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context) => this;
}
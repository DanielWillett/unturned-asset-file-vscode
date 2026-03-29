using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// An object from a unity bundle at a given path.
/// </summary>
public sealed class UnityObject : IValue<UnityObject>, IEquatable<UnityObject?>
{
    public bool IsNull => false;

    public UnityObjectAssetType Type { get; }

    public IBundleProxy Bundle { get; }

    public string Path { get; }

    public UnityObject(UnityObjectAssetType type, string path, IBundleProxy bundle)
    {
        Type = type;
        Bundle = bundle;
        Path = path;
    }

    public bool Equals(IValue? other)
    {
        return other is UnityObject obj && Equals(obj);
    }

    public bool Equals(UnityObject? other)
    {
        if (other == null)
            return false;

        return other.Type.Equals(Type)
            && Bundle.Equals(other.Bundle)
            && string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
    }

    void IValue.WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    bool IValue.VisitConcreteValue<TVisitor>(ref TVisitor visitor)
    {
        visitor.Accept(Type, this);
        return true;
    }

    bool IValue.VisitValue<TVisitor>(ref TVisitor visitor, ref FileEvaluationContext ctx)
    {
        visitor.Accept(Type, this);
        return true;
    }

    bool IValue<UnityObject>.TryGetConcreteValue(out Optional<UnityObject> value)
    {
        value = new Optional<UnityObject>(this);
        return true;
    }

    bool IValue<UnityObject>.TryEvaluateValue(out Optional<UnityObject> value, ref FileEvaluationContext ctx)
    {
        value = new Optional<UnityObject>(this);
        return true;
    }
    IType<UnityObject> IValue<UnityObject>.Type => Type;
}

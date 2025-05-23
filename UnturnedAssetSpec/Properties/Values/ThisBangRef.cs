using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;
using NotSupportedException = System.NotSupportedException;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Represents the object containing the current property.
/// </summary>
/// <remarks>#This</remarks>
public sealed class ThisBangRef : IEquatable<ISpecDynamicValue>, IEquatable<ThisBangRef>, IBangRefTarget
{
    public static readonly ThisBangRef Instance = new ThisBangRef();

    static ThisBangRef() { }
    private ThisBangRef() { }

    public ISpecPropertyType ValueType => KnownTypes.Flag;
    public bool EvaluateCondition(in SpecCondition condition, IAssetSpecDatabase specDatabase)
    {
        return condition.Operation.Evaluate(true, condition.Comparand is true, specDatabase.Information);
    }

    public bool Equals(ISpecDynamicValue other) => other is ThisBangRef;

    public bool Equals(IBangRefTarget other) => other is ThisBangRef;

    public bool Equals(ThisBangRef other) => other != null;

    public override bool Equals(object? obj) => obj is ThisBangRef;

    public override int GetHashCode() => 0;

    public override string ToString() => "This";

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        return condition.Operation.EvaluateNulls(false, condition.Comparand == null);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        value = default;
        isNull = true;
        return false;
    }
    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = null;
        return false;
    }

    public bool EvaluateIsIncluded(in FileEvaluationContext ctx)
    {
        return ctx.File.TryGetProperty(ctx.Self, out _);
    }

    public string EvaluateKey(in FileEvaluationContext ctx)
    {
        return ctx.Self.Key;
    }

    public ISpecDynamicValue? EvaluateValue(in FileEvaluationContext ctx)
    {
        ctx.TryGetValue(out ISpecDynamicValue? value);
        return value;
    }

    public int EvaluateKeyGroup(in FileEvaluationContext ctx, int index)
    {
        // todo;
        throw new NotSupportedException();
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        writer.WriteStringValue("#This");
    }
}
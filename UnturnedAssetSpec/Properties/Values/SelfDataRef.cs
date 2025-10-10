using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Represents the current property.
/// </summary>
/// <remarks>#Self</remarks>
public sealed class SelfDataRef : IEquatable<ISpecDynamicValue>, IEquatable<SelfDataRef>, IDataRefTarget
{
    public static readonly SelfDataRef Instance = new SelfDataRef();

    static SelfDataRef() { }
    private SelfDataRef() { }

    public ISpecPropertyType ValueType => KnownTypes.Flag;

    public bool EvaluateCondition(in SpecCondition condition, IAssetSpecDatabase specDatabase)
    {
        return condition.Evaluate(true, condition.Comparand is true, specDatabase.Information);
    }

    public bool Equals(ISpecDynamicValue other) => other is SelfDataRef;

    public bool Equals(IDataRefTarget other) => other is SelfDataRef;

    public bool Equals(SelfDataRef other) => other != null;

    public override bool Equals(object? obj) => obj is SelfDataRef;

    public override int GetHashCode() => 0;

    public override string ToString() => "Self";

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        PropertyRefInfo info = new PropertyRefInfo(ctx.Self);
        if (condition.Operation is ConditionOperation.Included or ConditionOperation.ValueIncluded)
        {
            bool eval = info.GetIsIncluded(condition.Operation == ConditionOperation.Included, in ctx, ctx.Self, default, null);
            if (condition.IsInverted)
                eval = !eval;
            return eval;
        }

        if (condition.Operation == ConditionOperation.Excluded)
        {
            bool eval = !info.GetIsIncluded(false, in ctx, ctx.Self, default, null);
            if (condition.IsInverted)
                eval = !eval;
            return eval;
        }

        ISpecDynamicValue? val = info.GetValue(in ctx, ctx.Self, default, null);
        return val?.EvaluateCondition(in ctx, in condition) ?? condition.EvaluateNulls(true, condition.Comparand == null);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        PropertyRefInfo info = new PropertyRefInfo(ctx.Self);
        ISpecDynamicValue? val = info.GetValue(in ctx, ctx.Self, default, null);
        if (val != null)
        {
            return val.TryEvaluateValue(in ctx, out value, out isNull);
        }

        isNull = false;
        value = default;
        return false;
    }

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        PropertyRefInfo info = new PropertyRefInfo(ctx.Self);
        ISpecDynamicValue? val = info.GetValue(in ctx, ctx.Self, default, null);
        if (val != null)
        {
            return val.TryEvaluateValue(in ctx, out value);
        }

        value = null;
        return false;
    }

    public bool EvaluateIsLegacy(in FileEvaluationContext ctx)
    {
        return PropertyRefInfo.EvaluateIsLegacy(ctx.Self, in ctx);
    }

    public ValueTypeDataRefType EvaluateValueType(in FileEvaluationContext ctx)
    {
        return PropertyRefInfo.EvaluateValueType(ctx.Self, in ctx);
    }

    public bool EvaluateIsIncluded(bool valueIncluded, in FileEvaluationContext ctx)
    {
        return PropertyRefInfo.EvaluateIsIncluded(ctx.Self, valueIncluded, in ctx);
    }

    public string? EvaluateKey(in FileEvaluationContext ctx)
    {
        return ctx.SourceFile.TryGetProperty(ctx.Self, out IPropertySourceNode? property)
            ? property.Key
            : null;
    }

    public ISpecDynamicValue? EvaluateValue(in FileEvaluationContext ctx)
    {
        PropertyRefInfo info = new PropertyRefInfo(ctx.Self);
        return info.GetValue(in ctx, ctx.Self, default, null);
    }

    public int EvaluateTemplateGroup(in FileEvaluationContext ctx, int index)
    {
        // todo;
        throw new NotImplementedException();
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        writer.WriteStringValue("#Self");
    }
}
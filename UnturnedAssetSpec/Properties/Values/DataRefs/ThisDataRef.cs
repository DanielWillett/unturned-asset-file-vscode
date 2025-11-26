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
public sealed class ThisDataRef : IEquatable<ISpecDynamicValue>, IEquatable<ThisDataRef>, IDataRefTarget
{
    public static readonly ThisDataRef Instance = new ThisDataRef();

    static ThisDataRef() { }
    private ThisDataRef() { }

    public ISpecPropertyType ValueType => KnownTypes.Flag;
    public bool EvaluateCondition(in SpecCondition condition, IAssetSpecDatabase specDatabase)
    {
        return condition.Evaluate(true, condition.Comparand is true, specDatabase.Information);
    }

    public bool Equals(ISpecDynamicValue other) => other is ThisDataRef;

    public bool Equals(IDataRefTarget other) => other is ThisDataRef;

    public bool Equals(ThisDataRef other) => other != null;

    public override bool Equals(object? obj) => obj is ThisDataRef;

    public override int GetHashCode() => 0;

    public override string ToString() => "This";

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Operation == ConditionOperation.ReferenceIsOfType
            && ctx.Self != null
            && condition.Comparand is string str)
        {
            QualifiedType conditionType = new QualifiedType(str, isCaseInsensitive: false);
            QualifiedType thisType = ctx.Self.Owner.Type.CaseSensitive;
            bool val = ctx.Information.Information.IsAssignableFrom(conditionType, thisType);
            return condition.IsInverted ? !val : val;
        }

        return condition.EvaluateNulls(false, condition.Comparand == null);
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

    public string EvaluateKey(in FileEvaluationContext ctx)
    {
        return ctx.Self.Key;
    }

    public ISpecDynamicValue? EvaluateValue(in FileEvaluationContext ctx)
    {
        ctx.TryGetValue(out ISpecDynamicValue? value);
        return value;
    }

    public int EvaluateTemplateGroup(in FileEvaluationContext ctx, int index)
    {
        // todo;
        throw new NotSupportedException();
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        writer.WriteStringValue("#This");
    }
}
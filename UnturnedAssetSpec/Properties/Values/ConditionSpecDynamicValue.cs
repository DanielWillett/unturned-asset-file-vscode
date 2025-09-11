using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public sealed class ConditionSpecDynamicValue : ISpecDynamicValue, IEquatable<ISpecDynamicValue>, IEquatable<ConditionSpecDynamicValue>
{
    public SpecCondition Condition { get; }

    public ConditionSpecDynamicValue(SpecCondition condition)
    {
        Condition = condition;
    }

    public ISpecPropertyType ValueType => KnownTypes.Boolean;

    public bool EvaluateCondition(in FileEvaluationContext ctx)
    {
        SpecCondition c = Condition;
        bool eval = c.Variable != null && c.Variable.EvaluateCondition(in ctx, in c);
        if (c.IsInverted)
            eval = !eval;
        return eval;
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        bool value = EvaluateCondition(in ctx);

        return condition.Comparand is not bool b
            ? condition.EvaluateNulls(false, true)
            : condition.Evaluate(value, b, ctx.Information.Information);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        if (typeof(TValue) != typeof(bool) || Condition.Variable == null)
        {
            value = default;
            isNull = true;
            return false;
        }

        bool v = EvaluateCondition(in ctx);
        value = Unsafe.As<bool, TValue>(ref v);
        isNull = false;
        return true;
    }

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        if (!TryEvaluateValue(in ctx, out bool val, out bool isNull))
        {
            value = null;
            return false;
        }

        value = isNull ? null : val;
        return true;
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        SpecConditionConverter.Write(writer, Condition);
    }

    public bool Equals(ConditionSpecDynamicValue other) => other != null && Condition.Equals(other.Condition);
    public bool Equals(ISpecDynamicValue other) => other is ConditionSpecDynamicValue v && Equals(v);

    public override bool Equals(object? obj) => obj is ConditionSpecDynamicValue v && Equals(v);

    public override int GetHashCode() => Condition.GetHashCode();

    public override string ToString() => Condition.ToString();
}
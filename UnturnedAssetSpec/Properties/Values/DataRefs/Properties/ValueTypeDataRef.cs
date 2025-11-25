using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Evaluates to the type of value provided for a property.
/// </summary>
public sealed class ValueTypeDataRef : DataRef, IEquatable<ValueTypeDataRef>
{
    public ValueTypeDataRef(IDataRefTarget target) : base(target, KnownTypes.Flag) { }

    public override string PropertyName => "ValueType";

    public override bool Equals(DataRef other) => other is ValueTypeDataRef b && Equals(b);

    public bool Equals(ValueTypeDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand == null || !Enum.TryParse(condition.Comparand.ToString(), true, out ValueTypeDataRefType type))
        {
            return condition.EvaluateNulls(false, true);
        }

        return condition.Evaluate(Target.EvaluateValueType(in ctx), type, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        isNull = false;

        ValueTypeDataRefType v = Target.EvaluateValueType(in ctx);

        if (typeof(TValue) == typeof(string))
        {
            value = SpecDynamicExpressionTreeValueHelpers.As<string, TValue>(v switch
            {
                ValueTypeDataRefType.Value => nameof(ValueTypeDataRefType.Value),
                ValueTypeDataRefType.Dictionary => nameof(ValueTypeDataRefType.Dictionary),
                ValueTypeDataRefType.List => nameof(ValueTypeDataRefType.List),
                _ => v.ToString()
            });
            return true;
        }

        if (typeof(TValue) == typeof(ValueTypeDataRefType))
        {
            value = Unsafe.As<ValueTypeDataRefType, TValue>(ref v);
            return true;
        }

        value = default!;
        return false;
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = Target.EvaluateValueType(in ctx);
        return true;
    }
}

public enum ValueTypeDataRefType
{
    Value,
    Dictionary,
    List
}
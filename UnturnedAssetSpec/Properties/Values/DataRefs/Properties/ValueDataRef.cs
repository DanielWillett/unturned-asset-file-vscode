using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Evaluates to the value given for a property or the current object.
/// </summary>
public sealed class ValueDataRef : DataRef, IEquatable<ValueDataRef>
{
    public ValueDataRef(IDataRefTarget target) : base(target, KnownTypes.String) { }

    public override string PropertyName => "Value";

    public override bool Equals(DataRef other) => other is ValueDataRef b && Equals(b);

    public bool Equals(ValueDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        ISpecDynamicValue? val = Target.EvaluateValue(in ctx);
        if (val == null)
        {
            return condition.EvaluateNulls(true, condition.Comparand == null);
        }

        return val.EvaluateCondition(in ctx, in condition);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        return Target.TryEvaluateValue(in ctx, out value!, out isNull);
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        return Target.TryEvaluateValue(in ctx, out value);
    }
}
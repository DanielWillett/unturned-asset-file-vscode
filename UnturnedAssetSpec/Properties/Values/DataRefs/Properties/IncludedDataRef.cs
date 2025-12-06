using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Evaluates to true when a property is included (even if there isn't a value).
/// </summary>
public sealed class IncludedDataRef : DataRef, IEquatable<IncludedDataRef>
{
    public IncludedDataRef(IDataRefTarget target) : base(target, KnownTypes.Flag) { }

    public override string PropertyName => "Included";

    public override bool Equals(DataRef other) => other is IncludedDataRef b && Equals(b);

    public bool Equals(IncludedDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is not bool v)
        {
            return condition.EvaluateNulls(false, true);
        }

        return condition.Evaluate(
            Target.EvaluateIsIncluded(false, in ctx),
            v,
            ctx.Information.Information
        );
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        isNull = false;

        if (typeof(TValue) != typeof(bool))
        {
            if (typeof(TValue) == typeof(string))
            {
                string vStr = Target.EvaluateIsIncluded(false, in ctx).ToString();
                value = Unsafe.As<string, TValue>(ref vStr);
                return true;
            }

            value = default!;
            return false;
        }

        bool v = Target.EvaluateIsIncluded(false, in ctx);
        value = Unsafe.As<bool, TValue>(ref v);
        return true;
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = Target.EvaluateIsIncluded(false, in ctx);
        return true;
    }
}
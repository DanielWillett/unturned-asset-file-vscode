using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Evaluates to whether or not the current property context is <see cref="PropertyResolutionContext.Legacy"/> instead of <see cref="PropertyResolutionContext.Modern"/>.
/// </summary>
public sealed class IsLegacyDataRef : DataRef, IEquatable<IsLegacyDataRef>
{
    public IsLegacyDataRef(IDataRefTarget target) : base(target, KnownTypes.Flag) { }

    public override string PropertyName => "IsLegacy";

    public override bool Equals(DataRef other) => other is IsLegacyDataRef b && Equals(b);

    public bool Equals(IsLegacyDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is not bool v)
        {
            return condition.EvaluateNulls(false, true);
        }

        return condition.Evaluate(Target.EvaluateIsLegacy(in ctx), v, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        isNull = false;

        bool v = Target.EvaluateIsLegacy(in ctx);

        if (typeof(TValue) == typeof(string))
        {
            value = SpecDynamicExpressionTreeValueHelpers.As<string, TValue>(v.ToString());
            return true;
        }

        if (typeof(TValue) == typeof(bool))
        {
            value = Unsafe.As<bool, TValue>(ref v);
            return true;
        }

        value = default!;
        return false;
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = Target.EvaluateIsLegacy(in ctx);
        return true;
    }
}
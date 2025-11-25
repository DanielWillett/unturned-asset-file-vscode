using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Evaluates to the full key used for a property or the current object.
/// </summary>
public sealed class KeyDataRef : DataRef, IEquatable<KeyDataRef>
{
    public KeyDataRef(IDataRefTarget target) : base(target, KnownTypes.String) { }

    public override string PropertyName => "Key";

    public override bool Equals(DataRef other) => other is KeyDataRef b && Equals(b);

    public bool Equals(KeyDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is not string v)
        {
            return condition.EvaluateNulls(false, true);
        }

        return condition.Evaluate(Target.EvaluateKey(in ctx), v, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        if (typeof(TValue) != typeof(string))
        {
            isNull = false;
            value = default!;
            return false;
        }

        string? v = Target.EvaluateKey(in ctx);
        value = Unsafe.As<string?, TValue>(ref v);
        isNull = v == null;
        return true;
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = Target.EvaluateKey(in ctx);
        return value != null;
    }
}
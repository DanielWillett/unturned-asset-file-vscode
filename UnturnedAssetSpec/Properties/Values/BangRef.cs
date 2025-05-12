using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public abstract class BangRef : IBangRefTarget, IEquatable<BangRef>, IEquatable<ISpecDynamicValue>
{
    public IBangRefTarget Target { get; }
    public ISpecPropertyType? ValueType { get; }

    public abstract string PropertyName { get; }

    protected BangRef(IBangRefTarget target, ISpecPropertyType? valueType)
    {
        Target = target;
        ValueType = valueType;
    }

    public virtual bool Equals(BangRef other)
    {
        if (other == null || !Target.Equals(other.Target))
            return false;

        if (ValueType == null)
            return other.ValueType == null;
        return other.ValueType != null && ValueType.Equals(other.ValueType);
    }

    public bool Equals(IBangRefTarget other) => other is BangRef r && Equals(r);

    public bool Equals(ISpecDynamicValue other) => other is BangRef r && Equals(r);

    public abstract bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition);
    public abstract bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull);

    bool IBangRefTarget.EvaluateIsIncluded(in FileEvaluationContext ctx) => Target.EvaluateIsIncluded(in ctx);
    string? IBangRefTarget.EvaluateKey(in FileEvaluationContext ctx) => Target.EvaluateKey(in ctx);
    ISpecDynamicValue? IBangRefTarget.EvaluateValue(in FileEvaluationContext ctx) => Target.EvaluateValue(in ctx);
    int IBangRefTarget.EvaluateKeyGroup(in FileEvaluationContext ctx, int index) => Target.EvaluateKeyGroup(in ctx, index);

    public override string ToString() => Target + "." + PropertyName;
}

public sealed class IncludedBangRef : BangRef, IEquatable<IncludedBangRef>
{
    public IncludedBangRef(IBangRefTarget target) : base(target, KnownTypes.Flag) { }

    public override string PropertyName => "Included";

    public override bool Equals(BangRef other) => other is IncludedBangRef b && Equals(b);

    public bool Equals(IncludedBangRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is not bool v)
        {
            return condition.Operation.EvaluateNulls(false, true);
        }

        return condition.Operation.Evaluate(Target.EvaluateIsIncluded(in ctx), v, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        isNull = false;

        if (typeof(TValue) != typeof(bool))
        {
            if (typeof(TValue) == typeof(string))
            {
                string vStr = Target.EvaluateIsIncluded(in ctx).ToString();
                value = Unsafe.As<string, TValue>(ref vStr);
                return true;
            }

            value = default!;
            return false;
        }

        bool v = Target.EvaluateIsIncluded(in ctx);
        value = Unsafe.As<bool, TValue>(ref v);
        return true;
    }
}

public sealed class ExcludedBangRef : BangRef, IEquatable<ExcludedBangRef>
{
    public ExcludedBangRef(IBangRefTarget target) : base(target, KnownTypes.Flag) { }

    public override string PropertyName => "Excluded";

    public override bool Equals(BangRef other) => other is ExcludedBangRef b && Equals(b);

    public bool Equals(ExcludedBangRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is not bool v)
        {
            return condition.Operation.EvaluateNulls(false, true);
        }

        return condition.Operation.Evaluate(!Target.EvaluateIsIncluded(in ctx), v, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        isNull = false;

        if (typeof(TValue) != typeof(bool))
        {
            if (typeof(TValue) == typeof(string))
            {
                string vStr = (!Target.EvaluateIsIncluded(in ctx)).ToString();
                value = Unsafe.As<string, TValue>(ref vStr);
                return true;
            }

            value = default!;
            return false;
        }

        bool v = !Target.EvaluateIsIncluded(in ctx);
        value = Unsafe.As<bool, TValue>(ref v);
        return true;
    }
}

public sealed class KeyBangRef : BangRef, IEquatable<KeyBangRef>
{
    public KeyBangRef(IBangRefTarget target) : base(target, KnownTypes.String) { }

    public override string PropertyName => "Key";

    public override bool Equals(BangRef other) => other is KeyBangRef b && Equals(b);

    public bool Equals(KeyBangRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is not string v)
        {
            return condition.Operation.EvaluateNulls(false, true);
        }

        return condition.Operation.Evaluate(Target.EvaluateKey(in ctx), v, ctx.Information.Information);
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
}

public sealed class ValueBangRef : BangRef, IEquatable<ValueBangRef>
{
    public ValueBangRef(IBangRefTarget target) : base(target, KnownTypes.String) { }

    public override string PropertyName => "Value";

    public override bool Equals(BangRef other) => other is ValueBangRef b && Equals(b);

    public bool Equals(ValueBangRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        ISpecDynamicValue? val = Target.EvaluateValue(in ctx);
        if (val == null)
        {
            return condition.Operation.EvaluateNulls(true, condition.Comparand == null);
        }

        return val.EvaluateCondition(in ctx, in condition);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        return Target.TryEvaluateValue(in ctx, out value!, out isNull);
    }
}

public sealed class KeyGroupsBangRef : BangRef, IEquatable<KeyGroupsBangRef>, IIndexableBangRef, IPropertiesBangRef
{
    public bool PreventSelfReference { get; set; }

    public int Index { get; set; }

    public KeyGroupsBangRef(IBangRefTarget target) : base(target, KnownTypes.String) { }

    public override string PropertyName => "KeyGroups";

    public override bool Equals(BangRef other) => other is KeyGroupsBangRef b && Equals(b);

    public bool Equals(KeyGroupsBangRef other) => base.Equals(other) && PreventSelfReference == other.PreventSelfReference && Index == other.Index;

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        int keyGroupValue = Target.EvaluateKeyGroup(in ctx, Index);
        if (condition.Comparand is not int expectedValue)
        {
            return condition.Operation.EvaluateNulls(keyGroupValue < 0, true);
        }

        if (keyGroupValue < 0)
        {
            return condition.Operation.EvaluateNulls(true, false);
        }

        return condition.Operation.Evaluate(keyGroupValue, expectedValue, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        isNull = false;

        if (typeof(TValue) != typeof(int))
        {
            if (typeof(TValue) == typeof(string))
            {
                string vStr = Target.EvaluateKeyGroup(in ctx, Index).ToString();
                value = Unsafe.As<string, TValue>(ref vStr);
                return true;
            }

            value = default!;
            return false;
        }

        int v = Target.EvaluateKeyGroup(in ctx, Index);
        value = Unsafe.As<int, TValue>(ref v);
        return true;
    }

    public void SetProperty(ReadOnlySpan<char> property, ReadOnlySpan<char> value)
    {
        if (property.Equals(nameof(PreventSelfReference).AsSpan(), StringComparison.Ordinal))
        {
            PreventSelfReference = value.Equals("true".AsSpan(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}

public interface IIndexableBangRef : IBangRefTarget
{
    int Index { get; set; }
}

public interface IPropertiesBangRef : IBangRefTarget
{
    void SetProperty(ReadOnlySpan<char> property, ReadOnlySpan<char> value);
}
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class GreaterThan : ConditionOperation<GreaterThan>
{
    public override string Name => "gt";
    public override string Symbol => ">";

    public override IConditionOperation Inverse => LessThanOrEqual.Instance;

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ComparerVisitor<TValue> visitor = default;
        visitor.Value = value;

        visitor.Accept(comparand);

        result = visitor.Comparison > 0;
        return visitor.Success;
    }

    public override int GetHashCode() => 839678630;
}

internal sealed class GreaterThanCaseInsensitive : ConditionOperation<GreaterThanCaseInsensitive>
{
    public override string Name => "gt-i";
    public override string Symbol => "¶>";

    public override IConditionOperation Inverse => LessThanOrEqualCaseInsensitive.Instance;

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ComparerVisitor<TValue> visitor = default;
        visitor.Value = value;
        visitor.CaseInsensitive = true;

        visitor.Accept(comparand);

        result = visitor.Comparison > 0;
        return visitor.Success;
    }

    public override int GetHashCode() => 686971039;
}
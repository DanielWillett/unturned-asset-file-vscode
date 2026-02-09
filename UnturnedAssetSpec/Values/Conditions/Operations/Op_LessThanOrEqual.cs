using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class LessThanOrEqual : ConditionOperation<LessThanOrEqual>
{
    public override string Name => "lte";
    public override string Symbol => "≤";

    public override IConditionOperation Inverse => GreaterThan.Instance;

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ComparerVisitor<TValue> visitor = default;
        visitor.Value = value;

        visitor.Accept(comparand);

        result = visitor.Comparison <= 0;
        return visitor.Success;
    }

    public override int GetHashCode() => 989812752;
}

internal sealed class LessThanOrEqualCaseInsensitive : ConditionOperation<LessThanOrEqualCaseInsensitive>
{
    public override string Name => "lte-i";
    public override string Symbol => "¶≤";

    public override IConditionOperation Inverse => GreaterThanOrEqual.Instance;

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ComparerVisitor<TValue> visitor = default;
        visitor.Value = value;
        visitor.CaseInsensitive = true;

        visitor.Accept(comparand);

        result = visitor.Comparison <= 0;
        return visitor.Success;
    }

    public override int GetHashCode() => 1602447246;
}
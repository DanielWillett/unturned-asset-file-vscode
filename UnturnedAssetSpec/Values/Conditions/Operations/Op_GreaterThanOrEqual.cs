using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class GreaterThanOrEqual : ConditionOperation<GreaterThanOrEqual>
{
    public override string Name => "gte";
    public override string Symbol => "≥";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ComparerVisitor<TValue> visitor = default;
        visitor.Value = value;

        visitor.Accept(comparand);

        result = visitor.Comparison >= 0;
        return visitor.Success;
    }

    public override int GetHashCode() => 1253235678;
}

internal sealed class GreaterThanOrEqualCaseInsensitive : ConditionOperation<GreaterThanOrEqualCaseInsensitive>
{
    public override string Name => "gte-i";
    public override string Symbol => "¶≥";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ComparerVisitor<TValue> visitor = default;
        visitor.Value = value;
        visitor.CaseInsensitive = true;

        visitor.Accept(comparand);

        result = visitor.Comparison >= 0;
        return visitor.Success;
    }

    public override int GetHashCode() => 1743781091;
}
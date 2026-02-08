using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class LessThan : ConditionOperation<LessThan>
{
    public override string Name => "lt";
    public override string Symbol => "<";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ComparerVisitor<TValue> visitor = default;
        visitor.Value = value;

        visitor.Accept(comparand);

        result = visitor.Comparison < 0;
        return visitor.Success;
    }

    public override int GetHashCode() => 442320188;
}

internal sealed class LessThanCaseInsensitive : ConditionOperation<LessThanCaseInsensitive>
{
    public override string Name => "lt-i";
    public override string Symbol => "¶<";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ComparerVisitor<TValue> visitor = default;
        visitor.Value = value;
        visitor.CaseInsensitive = true;

        visitor.Accept(comparand);

        result = visitor.Comparison < 0;
        return visitor.Success;
    }

    public override int GetHashCode() => 1978787311;
}
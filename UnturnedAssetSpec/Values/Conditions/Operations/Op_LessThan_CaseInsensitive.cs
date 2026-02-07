using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

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
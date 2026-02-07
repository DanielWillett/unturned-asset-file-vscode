using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class GreaterThan : ConditionOperation<GreaterThan>
{
    public override string Name => "gt";
    public override string Symbol => ">";

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
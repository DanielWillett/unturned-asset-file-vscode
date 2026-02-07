using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class NotEqual : ConditionOperation<NotEqual>
{
    public override string Name => "neq";
    public override string Symbol => "≠";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        EqualityVisitor<TValue> visitor = default;
        visitor.Value = value;

        visitor.Accept(comparand);

        result = visitor.IsEqual;
        return visitor.Success;
    }

    public override int GetHashCode() => 1511999825;
}
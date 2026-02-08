using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class ContainsCaseInsensitive : ConditionOperation<ContainsCaseInsensitive>
{
    public override string Name => "contains-i";
    public override string Symbol => "¶⊇";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ContainedVisitor<TValue> visitor = default;
        visitor.Superset = value;
        visitor.IsCaseInsensitive = true;

        visitor.Accept(comparand);

        result = visitor.Result;
        return visitor.Success;
    }

    public override int GetHashCode() => 335886083;
}
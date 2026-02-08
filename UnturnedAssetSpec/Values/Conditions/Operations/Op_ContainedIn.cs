using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class ContainedIn : ConditionOperation<ContainedIn>
{
    public override string Name => "contained-in";
    public override string Symbol => "⊆";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ContainedVisitor<TComparand> visitor = default;
        visitor.Superset = comparand;

        visitor.Accept(value);

        result = visitor.Result;
        return visitor.Success;
    }

    public override int GetHashCode() => 761487888;
}

internal sealed class ContainedInCaseInsensitive : ConditionOperation<ContainedInCaseInsensitive>
{
    public override string Name => "contained-in-i";
    public override string Symbol => "¶⊆";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ContainedVisitor<TComparand> visitor = default;
        visitor.Superset = comparand;
        visitor.IsCaseInsensitive = true;

        visitor.Accept(value);

        result = visitor.Result;
        return visitor.Success;
    }

    public override int GetHashCode() => 900104746;
}
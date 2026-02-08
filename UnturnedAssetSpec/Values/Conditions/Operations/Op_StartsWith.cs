using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class StartsWith : ConditionOperation<StartsWith>
{
    public override string Name => "starts-with";
    public override string Symbol => "⊑";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        StartsWithVisitor<TValue> visitor = default;
        visitor.Set = value;

        visitor.Accept(comparand);

        result = visitor.Result;
        return visitor.Success;
    }

    public override int GetHashCode() => 593305815;
}

internal sealed class StartsWithCaseInsensitive : ConditionOperation<StartsWithCaseInsensitive>
{
    public override string Name => "starts-with-i";
    public override string Symbol => "¶⊑";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        StartsWithVisitor<TValue> visitor = default;
        visitor.Set = value;
        visitor.IsCaseInsensitive = true;

        visitor.Accept(comparand);

        result = visitor.Result;
        return visitor.Success;
    }

    public override int GetHashCode() => 1958484325;
}
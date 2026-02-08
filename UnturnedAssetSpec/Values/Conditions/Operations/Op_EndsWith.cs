using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class EndsWith : ConditionOperation<EndsWith>
{
    public override string Name => "ends-with";
    public override string Symbol => "⊒";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        EndsWithVisitor<TValue> visitor = default;
        visitor.Set = value;

        visitor.Accept(comparand);

        result = visitor.Result;
        return visitor.Success;
    }

    public override int GetHashCode() => 173706440;
}

internal sealed class EndsWithCaseInsensitive : ConditionOperation<EndsWithCaseInsensitive>
{
    public override string Name => "ends-with-i";
    public override string Symbol => "¶⊒";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        EndsWithVisitor<TValue> visitor = default;
        visitor.Set = value;
        visitor.IsCaseInsensitive = true;

        visitor.Accept(comparand);

        result = visitor.Result;
        return visitor.Success;
    }

    public override int GetHashCode() => 1453845302;
}
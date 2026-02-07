using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class Equal : ConditionOperation<Equal>
{
    public override string Name => "eq";
    public override string Symbol => "=";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        EqualityVisitor<TValue> visitor = default;
        visitor.Value = value;

        visitor.Accept(comparand);

        result = visitor.IsEqual;
        return visitor.Success;
    }

    public override int GetHashCode() => 2094477246;
}
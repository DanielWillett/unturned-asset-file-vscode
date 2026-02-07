using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class Contains : ConditionOperation<Contains>
{
    public override string Name => "contains";
    public override string Symbol => "⊇";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        ContainedVisitor<TValue> visitor = default;
        visitor.Superset = value;

        visitor.Accept(comparand);

        result = visitor.Result;
        return visitor.Success;
    }

    public override int GetHashCode() => 1525507533;
}
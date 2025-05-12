using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Represents the object containing the current property.
/// </summary>
/// <remarks>#This</remarks>
public sealed class ThisBangRef : IEquatable<ISpecDynamicValue>, IEquatable<ThisBangRef>, IBangRefTarget
{
    public static readonly ThisBangRef Instance = new ThisBangRef();

    static ThisBangRef() { }
    private ThisBangRef() { }

    public ISpecPropertyType ValueType => KnownTypes.Flag;
    public bool EvaluateCondition(in SpecCondition condition, AssetSpecDatabase specDatabase)
    {
        return condition.Operation.Evaluate(true, condition.Comparand is true, specDatabase.Information);
    }

    public bool Equals(ISpecDynamicValue other) => other is ThisBangRef;

    public bool Equals(IBangRefTarget other) => other is ThisBangRef;

    public bool Equals(ThisBangRef other) => other != null;

    public override bool Equals(object? obj) => obj is ThisBangRef;

    public override int GetHashCode() => 0;

    public override string ToString() => "This";

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        // todo;
        throw new NotImplementedException();
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        // todo;
        throw new NotImplementedException();
    }

    public bool EvaluateIsIncluded(in FileEvaluationContext ctx)
    {
        // todo;
        throw new NotImplementedException();
    }

    public string? EvaluateKey(in FileEvaluationContext ctx)
    {
        // todo;
        throw new NotImplementedException();
    }

    public ISpecDynamicValue? EvaluateValue(in FileEvaluationContext ctx)
    {
        // todo;
        throw new NotImplementedException();
    }

    public int EvaluateKeyGroup(in FileEvaluationContext ctx, int index)
    {
        // todo;
        throw new NotImplementedException();
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public interface IBangRefTarget : IEquatable<IBangRefTarget>, ISpecDynamicValue
{
    bool EvaluateIsIncluded(bool valueIncluded, in FileEvaluationContext ctx);
    string? EvaluateKey(in FileEvaluationContext ctx);
    ISpecDynamicValue? EvaluateValue(in FileEvaluationContext ctx);
    int EvaluateKeyGroup(in FileEvaluationContext ctx, int index);
}
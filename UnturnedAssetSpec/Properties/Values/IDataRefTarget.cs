using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public interface IDataRefTarget : IEquatable<IDataRefTarget>, ISpecDynamicValue
{
    bool EvaluateIsIncluded(bool valueIncluded, in FileEvaluationContext ctx);
    bool EvaluateIsLegacy(in FileEvaluationContext ctx);
    ValueTypeDataRefType EvaluateValueType(in FileEvaluationContext ctx);
    string? EvaluateKey(in FileEvaluationContext ctx);
    ISpecDynamicValue? EvaluateValue(in FileEvaluationContext ctx);
    int EvaluateTemplateGroup(in FileEvaluationContext ctx, int index);
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Indexable data ref evaluating to the template groups for a property.
/// </summary>
public sealed class TemplateGroupsDataRef : DataRef, IEquatable<TemplateGroupsDataRef>, IIndexableDataRef, IPropertiesDataRef
{
    public bool PreventSelfReference { get; set; }

    public int Index { get; set; } = -1;

    public TemplateGroupsDataRef(IDataRefTarget target) : base(target, KnownTypes.String) { }

    public override string PropertyName => "TemplateGroups";

    public override bool Equals(DataRef other) => other is TemplateGroupsDataRef b && Equals(b);

    public bool Equals(TemplateGroupsDataRef other) => base.Equals(other) && PreventSelfReference == other.PreventSelfReference && Index == other.Index;

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        int templateGroupValue = Target.EvaluateTemplateGroup(in ctx, Index);
        if (condition.Comparand is not int expectedValue)
        {
            return condition.EvaluateNulls(templateGroupValue < 0, true);
        }

        if (templateGroupValue < 0)
        {
            return condition.EvaluateNulls(true, false);
        }

        return condition.Evaluate(templateGroupValue, expectedValue, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        int v = Target.EvaluateTemplateGroup(in ctx, Index);
        if (v < 0)
        {
            value = default!;
            isNull = true;
            return true;
        }

        isNull = false;
        if (typeof(TValue) != typeof(int))
        {
            if (typeof(TValue) == typeof(string))
            {
                value = SpecDynamicExpressionTreeValueHelpers.As<string, TValue>(v.ToString());
                return true;
            }

            value = default!;
            return false;
        }

        value = Unsafe.As<int, TValue>(ref v);
        return true;
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        int v = Target.EvaluateTemplateGroup(in ctx, Index);
        if (v < 0)
        {
            value = null;
            return true;
        }

        value = v;
        return true;
    }

    public void SetProperty(ReadOnlySpan<char> property, ReadOnlySpan<char> value)
    {
        if (property.Equals(nameof(PreventSelfReference).AsSpan(), StringComparison.Ordinal))
        {
            PreventSelfReference = value.Equals("true".AsSpan(), StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public IEnumerable<KeyValuePair<string, string>> EnumerateProperties()
    {
        yield return new KeyValuePair<string, string>(nameof(PreventSelfReference), PreventSelfReference ? "true" : "false");
    }
}
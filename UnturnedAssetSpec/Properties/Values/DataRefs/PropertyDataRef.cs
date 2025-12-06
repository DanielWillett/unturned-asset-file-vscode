using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// #Property, defaulting to <see cref="Property"/>.
/// </summary>
public sealed class PropertyDataRef : IEquatable<ISpecDynamicValue>, IEquatable<PropertyDataRef>, IDataRefTarget
{
    public PropertyRef Property { get; }

    public PropertyDataRef(string nameSpace)
    {
        Property = new PropertyRef(nameSpace.AsSpan(), nameSpace);
    }

    public ISpecPropertyType ValueType => KnownTypes.String;

    public bool EvaluateCondition(in SpecCondition condition, IAssetSpecDatabase specDatabase)
    {
        return condition.Evaluate(Property.PropertyName, condition.Comparand as string, specDatabase.Information);
    }

    public bool Equals(PropertyDataRef other) => other != null && Property.Equals(other.Property);

    public bool Equals(ISpecDynamicValue other) => other is PropertyDataRef r && Equals(r);

    public bool Equals(IDataRefTarget other) => other is PropertyDataRef r && Equals(r);

    public override bool Equals(object? obj) => obj is PropertyDataRef r && Equals(r);

    public override int GetHashCode() => Property == null ? 0 : Property.GetHashCode();

    public override string ToString()
    {
        if (Property.PropertyName.Equals("Self", StringComparison.Ordinal))
        {
            return @"\Self";
        }
        if (Property.PropertyName.Equals("This", StringComparison.Ordinal))
        {
            return @"\This";
        }

        return Property.ToString();
    }

    public bool EvaluateIsIncluded(bool valueIncluded, in FileEvaluationContext context)
    {
        return Property.GetIsIncluded(valueIncluded, in context);
    }

    public ValueTypeDataRefType EvaluateValueType(in FileEvaluationContext ctx)
    {
        return Property.GetValueType(in ctx);
    }

    public bool EvaluateIsLegacy(in FileEvaluationContext ctx)
    {
        return Property.GetIsLegacy(in ctx);
    }

    public string? EvaluateKey(in FileEvaluationContext context)
    {
        SpecProperty? property = Property.ResolveProperty(in context);
        return property != null && context.SourceFile.TryGetProperty(property, out IPropertySourceNode? kvp, context.PropertyContext)
            ? kvp.Key
            : null;
    }

    public ISpecDynamicValue? EvaluateValue(in FileEvaluationContext context)
    {
        return Property.GetValue(in context);
    }

    public int EvaluateTemplateGroup(in FileEvaluationContext context, int index)
    {
        SpecProperty? property = Property.ResolveProperty(in context);
        if (property is not { IsTemplate: true })
            return -1;

        // todo:
        return -1;
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        return Property.EvaluateCondition(in ctx, in condition);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        return Property.TryEvaluateValue(in ctx, out value, out isNull);
    }

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        return Property.TryEvaluateValue(in ctx, out value);
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        Property.WriteToJsonWriter(writer, options);
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// #Property, defaulting to <see cref="Namespace"/>.
/// </summary>
public sealed class PropertyBangRef : IEquatable<ISpecDynamicValue>, IEquatable<PropertyBangRef>, IBangRefTarget
{
    public PropertyRef Property { get; }

    public PropertyBangRef(string nameSpace)
    {
        Property = new PropertyRef(nameSpace.AsSpan(), nameSpace);
    }

    public ISpecPropertyType ValueType => KnownTypes.String;

    public bool EvaluateCondition(in SpecCondition condition, IAssetSpecDatabase specDatabase)
    {
        return condition.Operation.Evaluate(Property.PropertyName, condition.Comparand as string, specDatabase.Information);
    }

    public bool Equals(PropertyBangRef other) => other != null && Property.Equals(other.Property);

    public bool Equals(ISpecDynamicValue other) => other is PropertyBangRef r && Equals(r);

    public bool Equals(IBangRefTarget other) => other is PropertyBangRef r && Equals(r);

    public override bool Equals(object? obj) => obj is PropertyBangRef r && Equals(r);

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

    public bool EvaluateIsIncluded(in FileEvaluationContext context)
    {
        SpecProperty? property = Property.ResolveProperty(in context);
        return property != null && context.File.TryGetProperty(property, out _);
    }

    public string? EvaluateKey(in FileEvaluationContext context)
    {
        SpecProperty? property = Property.ResolveProperty(in context);
        return property != null && context.File.TryGetProperty(property, out AssetFileKeyValuePairNode kvp)
            ? kvp.Key.Value
            : null;
    }

    public ISpecDynamicValue? EvaluateValue(in FileEvaluationContext context)
    {
        return Property.GetValue(in context);
    }

    public int EvaluateKeyGroup(in FileEvaluationContext context, int index)
    {
        // todo;
        throw new NotImplementedException();
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        return Property.EvaluateCondition(in ctx, in condition);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        return Property.TryEvaluateValue(in ctx, out value, out isNull);
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        Property.WriteToJsonWriter(writer, options);
    }
}
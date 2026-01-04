using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

internal class UnresolvedPropertyReference : ISecondPassSpecDynamicValue
{
    /// <inheritdoc />
    public ISpecPropertyType? ValueType
    {
        get
        {
            return null;
        }
    }

    /// <inheritdoc />
    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        return condition.EvaluateNulls(true, true);
    }

    /// <inheritdoc />
    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        value = default;
        isNull = true;
        return false;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = null;
        return false;
    }

    /// <inheritdoc />
    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {

    }

    /// <inheritdoc />
    public ISpecDynamicValue Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile)
    {
        return this;
    }
}

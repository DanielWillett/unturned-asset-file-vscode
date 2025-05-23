using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

internal class UnresolvedDynamicValue : ISecondPassSpecDynamicValue, IDisposable
{
    private readonly UnresolvedSpecPropertyType _unresolvedType;
    private readonly JsonDocument _document;
    private readonly JsonSerializerOptions? _options;
    private readonly string _context;
    private readonly SpecDynamicValueContext _readContext;
    private readonly Func<SpecProperty, IAssetSpecDatabase, AssetSpecType, ISpecPropertyType?>? _expectedTypeGetterOverride;

    public ISpecPropertyType ValueType => _unresolvedType;

    public UnresolvedDynamicValue(
        UnresolvedSpecPropertyType unresolvedType,
        JsonDocument document,
        JsonSerializerOptions? options,
        string context,
        SpecDynamicValueContext readContext,
        Func<SpecProperty, IAssetSpecDatabase, AssetSpecType, ISpecPropertyType?>? expectedTypeGetterOverride = null)
    {
        _unresolvedType = unresolvedType;
        _document = document;
        _options = options;
        _context = context;
        _readContext = readContext;
        _expectedTypeGetterOverride = expectedTypeGetterOverride;
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        return condition.Operation.EvaluateNulls(true, condition.Comparand == null);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        isNull = false;
        value = default;
        return false;
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        _document.WriteTo(writer);
    }

    public ISpecDynamicValue Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile)
    {
        Utf8JsonReader reader = JsonHelper.CreateUtf8JsonReader(_document, new JsonReaderOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip
        });

        if (!reader.Read())
        {
            throw new JsonException($"Failed to read a value while reading property {_context} (no data).");
        }

        ISpecPropertyType? expectedType = property.Type;
        if (_expectedTypeGetterOverride != null)
        {
            expectedType = _expectedTypeGetterOverride(property, database, assetFile);
        }

        ISpecDynamicValue value;
        try
        {
            value = SpecDynamicValue.Read(ref reader, _options, _readContext, expectedType);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Failed to read a value while reading property {_context}.", ex);
        }

        if (value is ISecondPassSpecDynamicValue sp)
        {
            value = sp.Transform(property, database, assetFile);
            if (sp is IDisposable disp)
                disp.Dispose();
        }

        return value;
    }

    public void Dispose()
    {
        _document.Dispose();
    }
}

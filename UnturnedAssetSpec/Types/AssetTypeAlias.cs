using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class AssetTypeAlias : BasicSpecPropertyType<AssetTypeAlias, string>,
    IStringParseableSpecPropertyType,
    IEquatable<AssetTypeAlias>,
    IAutoCompleteSpecPropertyType,
    ISpecType,
    IValueHoverProviderSpecPropertyType
{
    QualifiedType ISpecType.Type => Type;
    Version? ISpecType.Version => null;

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName => "Asset Type";

    public string Docs => "https://docs.smartlydressedgames.com/en/stable/assets/asset-definitions.html#header";

    public AssetSpecType Owner { get => throw new NotSupportedException(); set { } }

    public OneOrMore<KeyValuePair<string, object?>> AdditionalProperties => OneOrMore<KeyValuePair<string, object?>>.Null;

    public SpecProperty? FindProperty(string propertyName, SpecPropertyContext context) => null;

    public QualifiedType Parent => QualifiedType.None;

    /// <inheritdoc cref="ISpecPropertyType.Type" />
    public override string Type => "DanielWillett.UnturnedDataFileLspServer.Data.Types.AssetTypeAlias, UnturnedAssetSpec";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Enum;

    /// <inheritdoc />
    public ValueHoverProviderResult? GetDescription(in SpecPropertyTypeParseContext ctx, ISpecDynamicValue value)
    {
        if (value is not AssetTypeAliasValue assetTypeAlias)
            return null;

        return new ValueHoverProviderResult(
            assetTypeAlias.Value,
            QualifiedType.None, 
            null, null, null, null, false,
            assetTypeAlias.GetCorrespondingType(ctx.Database)
        );
    }

    protected override ISpecDynamicValue CreateValue(string? value) => new AssetTypeAliasValue(value!, this);

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode)
        {
            return FailedToParse(in parse, out value);
        }

        if (parse.Database.Information.AssetAliases.TryGetValue(strValNode.Value, out _))
        {
            value = strValNode.Value;
            return true;
        }

        value = null;
        if (!parse.HasDiagnostics)
            return false;

        parse.Log(new DatDiagnosticMessage
        {
            Range = strValNode.Range,
            Diagnostic = DatDiagnostics.UNT1014,
            Message = string.Format(DiagnosticResources.UNT1014, strValNode.Value)
        });
        return false;
    }

    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (span.IsEmpty)
        {
            dynamicValue = null!;
            return false;
        }

        dynamicValue = new AssetTypeAliasValue(stringValue ?? span.ToString(), this);
        return true;
    }

    /// <inheritdoc />
    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcrete<string>();
    }

    public bool Equals(AssetTypeAlias? other) => other != null;
    public bool Equals(ISpecType? other) => other is AssetTypeAlias;
    public override bool Equals(object? obj) => obj is AssetTypeAlias;
    public override int GetHashCode() => 0;

    public Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters parameters,
        in FileEvaluationContext context)
    {
        Dictionary<string, QualifiedType> dict = parameters.Database.Information.AssetAliases;
        AutoCompleteResult[] results = new AutoCompleteResult[dict.Count];
        int index = -1;
        foreach (KeyValuePair<string, QualifiedType> kvp in dict)
        {
            results[++index] = new AutoCompleteResult(kvp.Key, kvp.Value.GetTypeName());
        }

        return Task.FromResult(results);
    }

}

public class AssetTypeAliasValue : ICorrespondingTypeSpecDynamicValue, IEquatable<AssetTypeAliasValue?>, IEquatable<ISpecDynamicValue?>, ISpecConcreteValue
{
    public string Value { get; }
    public ISpecPropertyType ValueType { get; }

    public AssetTypeAliasValue(string value, AssetTypeAlias type)
    {
        ValueType = type;
        Value = value;
    }

    public QualifiedType GetCorrespondingType(IAssetSpecDatabase database)
    {
        database.Information.AssetAliases.TryGetValue(Value, out QualifiedType t);
        return t;
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Operation is ConditionOperation.Included or ConditionOperation.ValueIncluded)
            return !condition.IsInverted;
        if (condition.Operation is ConditionOperation.AssignableTo or ConditionOperation.AssignableFrom)
        {
            QualifiedType correspondingType = GetCorrespondingType(ctx.Information);
            if (condition.Comparand is not QualifiedType qt || qt.IsNull)
            {
                if (condition.Comparand is string { Length: > 0 } str)
                    qt = new QualifiedType(str);
                else
                    return condition.EvaluateNulls(correspondingType.IsNull, true);
            }

            if (correspondingType.IsNull)
            {
                return condition.EvaluateNulls(true, false);
            }

            return condition.Invert(condition.Operation == ConditionOperation.AssignableTo
                ? ctx.Information.Information.IsAssignableTo(correspondingType, qt)
                : ctx.Information.Information.IsAssignableFrom(correspondingType, qt));
        }

        if (condition.Comparand is not string str2)
        {
            return condition.EvaluateNulls(Value == null, true);
        }

        return Value == null
            ? condition.EvaluateNulls(true, false)
            : condition.Evaluate(Value, str2, ctx.Information.Information);
    }

    public bool TryEvaluateValue<TValue>(out TValue? value, out bool isNull)
    {
        if (typeof(TValue) != typeof(string))
        {
            isNull = Value != null;
            value = default;
            return false;
        }

        value = SpecDynamicEquationTreeValueHelpers.As<QualifiedType, TValue>(Value);
        isNull = Value != null;
        return true;
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        if (typeof(TValue) == typeof(QualifiedType))
        {
            QualifiedType correspondingType = GetCorrespondingType(ctx.Information);
            value = SpecDynamicEquationTreeValueHelpers.As<QualifiedType, TValue>(correspondingType);
            isNull = correspondingType.IsNull;
            return true;
        }

        return TryEvaluateValue(out value, out isNull);
    }

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = Value;
        return true;
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        writer.WriteStringValue(Value);
    }

    public bool Equals(AssetTypeAliasValue? other) => other != null && string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);
    public bool Equals(ISpecDynamicValue? other) => other is AssetTypeAliasValue v && Equals(v);
    public override bool Equals(object? obj) => obj is AssetTypeAliasValue v && Equals(v);
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Value);
    public override string ToString() => Value;
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[DebuggerDisplay("Enum: {Type.GetTypeName()}")]
public class EnumSpecType : ISpecType, ISpecPropertyType<string>, IEquatable<EnumSpecType>, IStringParseableSpecPropertyType, IAutoCompleteSpecPropertyType, IAdditionalPropertyProvider
{
    private AutoCompleteResult[]? _valueResults;

    public required QualifiedType Type { get; init; }
    public required string DisplayName { get; init; }
    public required string? Docs { get; init; }
    public required EnumSpecTypeValue[] Values { get; init; }
    public required OneOrMore<KeyValuePair<string, object?>> AdditionalProperties { get; init; }

#nullable disable
    public AssetSpecType Owner { get; set; }
#nullable restore

    string ISpecPropertyType.Type => Type.Type;
    public Type ValueType => typeof(string);
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Enum;
    QualifiedType ISpecType.Parent => QualifiedType.None;

    public bool Equals(EnumSpecType? other) => other != null && string.Equals(Type, other.Type, StringComparison.Ordinal);
    public bool Equals(ISpecType? other) => other is EnumSpecType s && Equals(s);
    public bool Equals(ISpecPropertyType? other) => other is EnumSpecType s && Equals(s);
    public bool Equals(ISpecPropertyType<string>? other) => other is EnumSpecType s && Equals(s);

    public override bool Equals(object? obj) => obj is EnumSpecType s && Equals(s);

    public override int GetHashCode() => Type.GetHashCode();

    public override string ToString() => Type.ToString();

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out int index))
        {
            value = null!;
            return false;
        }

        value = SpecDynamicValue.Enum(this, index);
        return true;
    }

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out string value)
    {
        if (!TryParseValue(in parse, out int index))
        {
            value = null!;
            return false;
        }

        value = Values[index].Value;
        return true;
    }

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out int index)
    {
        if (parse.Node == null)
        {
            if (parse.HasDiagnostics && parse.Parent is AssetFileKeyValuePairNode { Key: { } key })
            {
                DatDiagnosticMessage message = new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1005,
                    Message = string.Format(DiagnosticResources.UNT1005, key),
                    Range = parse.Parent?.Range ?? parse.Node?.Range ?? default
                };

                parse.Log(message);
            }

            index = -1;
            return false;
        }

        if (parse.Node is not AssetFileStringValueNode strValNode)
        {
            if (parse.HasDiagnostics)
            {
                DatDiagnosticMessage message = new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT2004,
                    Message = string.Format(
                        DiagnosticResources.UNT2004,
                        parse.Node is AssetFileStringValueNode s ? s.Value : parse.Node.ToString(),
                        DisplayName
                    ),
                    Range = parse.Node.Range
                };

                parse.Log(message);
            }

            index = -1;
            return false;
        }

        if (!TryParse(strValNode.Value.AsSpan(), out EnumSpecTypeValue v, ignoreCase: true))
        {
            if (parse.HasDiagnostics)
            {
                DatDiagnosticMessage message = new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1014,
                    Message = string.Format(DiagnosticResources.UNT1014_Specific, strValNode.Value, Type.GetTypeName()),
                    Range = parse.Node.Range
                };

                parse.Log(message);
            }

            index = -1;
            return false;
        }

        index = v.Index;
        return true;
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (TryParse(span, out EnumSpecTypeValue v, ignoreCase: true))
        {
            dynamicValue = SpecDynamicValue.Enum(this, v.Index);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    public bool TryParse(ReadOnlySpan<char> span, out EnumSpecTypeValue v, bool ignoreCase = true)
    {
        span = span.Trim();
        StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        for (int i = 0; i < Values.Length; i++)
        {
            ref EnumSpecTypeValue value = ref Values[i];
            if (!span.Equals(value.Value.AsSpan(), comparison))
                continue;

            v = value;
            return true;
        }

        v = default;
        return false;
    }

    /// <inheritdoc />
    public Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters parameters,
        in FileEvaluationContext context)
    {
        if (_valueResults != null)
        {
            return Task.FromResult(_valueResults);
        }

        AutoCompleteResult[] results = new AutoCompleteResult[Values.Length];
        for (int i = 0; i < results.Length; ++i)
        {
            ref EnumSpecTypeValue val = ref Values[i];
            results[i] = new AutoCompleteResult(val.Casing, val.Description);
        }

        _valueResults = results;
        return Task.FromResult(results);
    }

    SpecProperty? ISpecType.FindProperty(string propertyName, SpecPropertyContext context) => null;

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}

[DebuggerDisplay("{Value,nq}")]
public readonly struct EnumSpecTypeValue : IEquatable<EnumSpecTypeValue>, IComparable, IComparable<EnumSpecTypeValue>
{
    public required int Index { get; init; }
    public required EnumSpecType Type { get; init; }

    public required string Value { get; init; }
    public required string Casing { get; init; }
    public QualifiedType RequiredBaseType { get; init; }
    public QualifiedType CorrespondingType { get; init; }
    public string? Description { get; init; }
    public bool Deprecated { get; init; }

    public required OneOrMore<KeyValuePair<string, object?>> ExtendedData { get; init; }
    
    /// <inheritdoc />
    public bool Equals(EnumSpecTypeValue other) => Index == other.Index;

    /// <inheritdoc />
    public int CompareTo(EnumSpecTypeValue other) => Index.CompareTo(other.Index);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EnumSpecTypeValue && Equals(obj);

    /// <inheritdoc />
    public override int GetHashCode() => Index;

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    public int CompareTo(object obj) => obj is EnumSpecTypeValue t ? CompareTo(t) : 1;

    public static bool operator ==(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index == right.Index;
    public static bool operator !=(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index != right.Index;
    public static bool operator <(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index < right.Index;
    public static bool operator >(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index > right.Index;
    public static bool operator <=(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index <= right.Index;
    public static bool operator >=(EnumSpecTypeValue left, EnumSpecTypeValue right) => left.Index >= right.Index;
}
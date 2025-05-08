using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[DebuggerDisplay("{DisplayName,nq}")]
public class EnumSpecType : ISpecType, IEquatable<EnumSpecType>, IStringParseableSpecPropertyType, IAutoCompleteSpecPropertyType
{
    private AutoCompleteResult[]? _valueResults;

    public required QualifiedType Type { get; init; }

    public required string DisplayName { get; init; }

    public required string? Docs { get; init; }

    public required EnumSpecTypeValue[] Values { get; init; }

    public bool Equals(EnumSpecType other) => other != null && string.Equals(Type, other.Type, StringComparison.Ordinal);

    public bool Equals(ISpecType other) => other is EnumSpecType s && Equals(s);

    public override bool Equals(object? obj) => obj is EnumSpecType s && Equals(s);

    public override int GetHashCode() => Type.GetHashCode();

    public override string ToString() => Type.ToString();

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        for (int i = 0; i < Values.Length; i++)
        {
            ref EnumSpecTypeValue value = ref Values[i];
            if (!span.Equals(value.Value.AsSpan(), StringComparison.OrdinalIgnoreCase))
                continue;

            dynamicValue = SpecDynamicValue.Enum(this, i);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public Task<AutoCompleteResult[]> GetAutoCompleteResults(AutoCompleteParameters parameters)
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

    QualifiedType ISpecType.Parent => QualifiedType.None;
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

    public IReadOnlyDictionary<string, string>? ExtendedData { get; init; }
    
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
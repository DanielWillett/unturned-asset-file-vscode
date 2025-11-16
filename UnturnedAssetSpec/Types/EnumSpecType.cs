using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[DebuggerDisplay("Enum: {Type.GetTypeName()}")]
public class EnumSpecType : ISpecType, ISpecPropertyType<string>, IEquatable<EnumSpecType>, IStringParseableSpecPropertyType, IAutoCompleteSpecPropertyType, IValueHoverProviderSpecPropertyType
{
    private AutoCompleteResult[]? _valueResults;

    public required QualifiedType Type { get; init; }
    public required string DisplayName { get; init; }
    public required string? Docs { get; init; }
    public required EnumSpecTypeValue[] Values { get; init; }
    public Version? Version { get; init; }
    public bool IsFlags { get; init; }
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
            if (parse.HasDiagnostics && parse.Parent is IPropertySourceNode { Key: { } key })
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

        if (parse.Node is not IValueSourceNode strValNode)
        {
            if (parse.HasDiagnostics)
            {
                DatDiagnosticMessage message = new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT2004,
                    Message = string.Format(
                        DiagnosticResources.UNT2004,
                        parse.Node is IValueSourceNode s ? s.Value : parse.Node.ToString(),
                        DisplayName
                    ),
                    Range = parse.Node.Range
                };

                parse.Log(message);
            }

            index = -1;
            return false;
        }

        if (!TryParse(strValNode.Value.AsSpan(), out int v, ignoreCase: true))
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

        index = v;
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

    /// <inheritdoc />
    public string? ToString(ISpecDynamicValue value)
    {
        if (value is SpecDynamicConcreteEnumValue enumValue)
            return enumValue.Name;

        return value.AsConcrete<string>();
    }

    public bool TryParse(ReadOnlySpan<char> span, out int index, bool ignoreCase = true)
    {
        span = span.Trim();
        StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        for (int i = 0; i < Values.Length; i++)
        {
            ref EnumSpecTypeValue value = ref Values[i];
            if (!span.Equals(value.Value.AsSpan(), comparison))
                continue;

            index = i;
            return true;
        }

        index = -1;
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

    public ValueHoverProviderResult? GetDescription(in SpecPropertyTypeParseContext ctx, ISpecDynamicValue value)
    {
        if (value is not SpecDynamicConcreteEnumValue enumVal)
            return null;

        int index = enumVal.Value;
        if (index < 0)
            return null;

        ref EnumSpecTypeValue info = ref Values[index];
        return new ValueHoverProviderResult(info.Casing, info.Type.Type, info.Value, info.Description, info.GetReleaseVersion(), info.GetDocumentationLink(), info.Deprecated, info.CorrespondingType);
    }
}

[DebuggerDisplay("{Value,nq}")]
public readonly struct EnumSpecTypeValue : IEquatable<EnumSpecTypeValue>, IComparable, IComparable<EnumSpecTypeValue>, IAdditionalPropertyProvider
{
    public required int Index { get; init; }
    public required EnumSpecType Type { get; init; }

    public required string Value { get; init; }
    public required string Casing { get; init; }
    public QualifiedType RequiredBaseType { get; init; }
    public QualifiedType CorrespondingType { get; init; }
    public string? Description { get; init; }
    public string? Docs { get; init; }
    public bool Deprecated { get; init; }
    public Version? Version { get; init; }
    public long? NumericValue { get; init; }

    public Version? GetReleaseVersion()
    {
        if (Version != null)
            return Version;
        
        if (Type.Version != null)
            return Type.Version;

        if (Type.Owner.Version != null)
            return Type.Owner.Version;

        return null;
    }

    public string? GetDocumentationLink()
    {
        if (Docs != null)
            return Docs;

        if (Type.Docs != null)
            return Type.Docs;

        if (Type.Owner.Docs != null)
            return Type.Owner.Docs;

        return null;
    }

    /// <summary>
    /// If only <see cref="Value"/> is filled out the JSON can be re-written as just a string.
    /// </summary>
    internal bool CanBeWrittenAsString => string.Equals(Casing, Value, StringComparison.Ordinal)
                                          && RequiredBaseType.IsNull
                                          && CorrespondingType.IsNull
                                          && Description == null
                                          && Docs == null
                                          && !Deprecated
                                          && AdditionalProperties.IsNull
                                          && !NumericValue.HasValue;

    public required OneOrMore<KeyValuePair<string, object?>> AdditionalProperties { get; init; }
    
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

    public bool TryGetAdditionalProperty<T>(string name, out T? val)
    {
        return AdditionalPropertyProviderExtensions.TryGetAdditionalPropertyFromStruct(ref Unsafe.AsRef(in this), name, out val);
    }
}
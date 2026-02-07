using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// A subclass of <see cref="DatType"/> which defines a specific set of values or a set of flags which can be combined (or'd) into a bitwise value.
/// </summary>
public class DatEnumType : DatType, IType<DatEnumValue>, ITypeConverter<DatEnumValue>
{
    /// <summary>
    /// The file that defines this enum type.
    /// </summary>
    public override DatFileType Owner { get; }

    /// <summary>
    /// The null value for this enum type.
    /// </summary>
    [field: MaybeNull]
    public IValue<DatEnumValue> Null => field ??= new NullValue<DatEnumValue>(this);
    
    /// <inheritdoc />
    public override DatSpecificationType Type => DatSpecificationType.Enum;

    /// <summary>
    /// List of available enum values for this enum type.
    /// </summary>
    public ImmutableArray<DatEnumValue> Values { get; internal set; }

    /// <summary>
    /// The parser used for values of this enum type.
    /// </summary>
    [field: MaybeNull]
    public ITypeParser<DatEnumValue> Parser => field ??= new TypeConverterParser<DatEnumValue>(this);

    internal DatEnumType(QualifiedType type, JsonElement element, DatFileType owner) : base(type, null, element)
    {
        Owner = owner;
    }

    /// <summary>
    /// Attempts to parse an enum value from text.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <param name="value">Parsed enum value.</param>
    /// <param name="caseInsensitive">Whether or not to accept values of a different case but the same value.</param>
    /// <returns>Whether or not a match was found.</returns>
    public virtual bool TryParse(ReadOnlySpan<char> text, [NotNullWhen(true)] out DatEnumValue? value, bool caseInsensitive = true)
    {
        return TryParseSingleValue(text, out value, caseInsensitive);
    }

    protected bool TryParseSingleValue(ReadOnlySpan<char> text, [NotNullWhen(true)] out DatEnumValue? value, bool caseInsensitive)
    {
        if (text.IsEmpty)
        {
            value = null;
            return false;
        }

        StringComparison comp = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        foreach (DatEnumValue v in Values)
        {
            if (!text.Equals(v.Value, comp))
                continue;

            value = v;
            return true;
        }

        value = null;
        return false;
    }

    public bool TryReadValueFromJson(ref Utf8JsonReader reader, [NotNullWhen(true)] out IValue<DatEnumValue>? value)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            value = Null;
            return true;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            value = null;
            return false;
        }

        string? str = reader.GetString();
        bool s = TryParse(str, out DatEnumValue? v, caseInsensitive: false);
        value = v;
        return s;
    }

    protected bool TryConvertTo<TTo>(DatEnumValue? v, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (v == null)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(DatEnumValue))
        {
            result = new Optional<TTo>(Unsafe.As<DatEnumValue, TTo>(ref v));
            return true;
        }
        if (typeof(TTo) == typeof(string))
        {
            result = new Optional<TTo>(MathMatrix.As<string, TTo>(v.ToString()));
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    bool ITypeConverter<DatEnumValue>.TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<DatEnumValue> args, [NotNullWhen(true)] out DatEnumValue? parsedValue)
    {
        return TryParse(text, out parsedValue);
    }

    string ITypeConverter<DatEnumValue>.Format(DatEnumValue value, ref TypeConverterFormatArgs args)
    {
        return value.ToString();
    }

    bool ITypeConverter<DatEnumValue>.TryFormat(Span<char> output, DatEnumValue value, out int size, ref TypeConverterFormatArgs args)
    {
        string ts = value.ToString();
        size = ts.Length;
        return ts.AsSpan().TryCopyTo(output);
    }

    bool ITypeConverter<DatEnumValue>.TryConvertTo<TTo>(Optional<DatEnumValue> obj, out Optional<TTo> result)
    {
        return TryConvertTo(obj.Value, out result);
    }

    public void WriteJson(Utf8JsonWriter writer, DatEnumValue value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }

    public bool TryReadJson(in JsonElement json, out Optional<DatEnumValue> value, ref TypeConverterParseArgs<DatEnumValue> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<DatEnumValue>.Null;
                return true;

            case JsonValueKind.String:
                string str = json.GetString()!;
                if (TryParse(str, out DatEnumValue? val, caseInsensitive: false))
                {
                    value = val;
                    return true;
                }
                goto default;

            default:
                value = Optional<DatEnumValue>.Null;
                return false;
        }
    }

    public override void Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.Accept(this);
    }

    private protected override string FullName => $"{Owner.TypeName.GetFullTypeName()}/{TypeName.GetFullTypeName()}";

    string IType.Id => TypeName.Type;
    IValue<DatEnumValue> IType<DatEnumValue>.CreateValue(Optional<DatEnumValue> value) => value.Value ?? Null;
    IType<DatEnumValue> ITypeConverter<DatEnumValue>.DefaultType => this;
}

public class DatFlagEnumType : DatEnumType, IType<DatFlagEnumValue>, ITypeConverter<DatFlagEnumValue>
{
    /// <summary>
    /// The null value for this enum type.
    /// </summary>
    [field: MaybeNull]
    public new IValue<DatFlagEnumValue> Null => field ??= new NullValue<DatFlagEnumValue>(this);

    /// <inheritdoc />
    [field: MaybeNull]
    public new ITypeParser<DatFlagEnumValue> Parser => field ??= new TypeConverterParser<DatFlagEnumValue>(this);

    /// <inheritdoc />
    public override DatSpecificationType Type => DatSpecificationType.FlagEnum;

    internal DatFlagEnumType(QualifiedType type, JsonElement element, DatFileType file) : base(type, element, file) { }

    public bool TryReadValueFromJson(ref Utf8JsonReader reader, [NotNullWhen(true)] out IValue<DatFlagEnumValue>? value)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            value = Null;
            return true;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            value = null;
            return false;
        }

        string? str = reader.GetString();
        bool s = TryParse(str, out DatFlagEnumValue? v, caseInsensitive: false);
        value = v;
        return s;
    }

    /// <inheritdoc cref="TryParse(ReadOnlySpan{char}, out DatFlagEnumValue, bool)"/>
    public override bool TryParse(ReadOnlySpan<char> text, [NotNullWhen(true)] out DatEnumValue? value, bool caseInsensitive = true)
    {
        bool s = TryParse(text, out DatFlagEnumValue? flagValue, caseInsensitive);
        value = flagValue;
        return s;
    }

    /// <summary>
    /// Attempts to parse one or more comma-separated enum values from text.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <param name="value">Parsed enum value.</param>
    /// <param name="caseInsensitive">Whether or not to accept values of a different case but the same value.</param>
    /// <returns>Whether or not a match was found.</returns>
    public bool TryParse(ReadOnlySpan<char> text, [NotNullWhen(true)] out DatFlagEnumValue? value, bool caseInsensitive = true)
    {
        value = null;
        text = text.Trim();

        if (text.IsEmpty)
        {
            value = null;
            return false;
        }

        DatEnumValue? parsedValue;

        text = text.Trim();
        int firstComma = text.IndexOf(',');
        if (firstComma == -1)
        {
            if (!TryParseSingleValue(text, out parsedValue, caseInsensitive))
                return false;

            value = (DatFlagEnumValue)parsedValue;
            return true;
        }

        int commaCount = 1;
        ReadOnlySpan<char> commaSearch = text.Slice(firstComma + 1);
        while (commaSearch.Length > 0)
        {
            int ind = commaSearch.IndexOf(',');
            if (ind < 0)
                break;

            commaCount++;
            commaSearch = commaSearch.Slice(ind + 1);
        }

        DatFlagEnumValue[] outputArray = new DatFlagEnumValue[commaCount + 1];
        ReadOnlySpan<char> element = text.Slice(0, firstComma).Trim();
        if (!TryParseSingleValue(element, out parsedValue, caseInsensitive))
            return false;

        outputArray[0] = (DatFlagEnumValue)parsedValue;
        commaSearch = text.Slice(firstComma + 1);
        commaCount = 1;
        while (commaSearch.Length > 0)
        {
            int ind = commaSearch.IndexOf(',');
            if (ind < 0)
                ind = commaSearch.Length;

            if (!TryParseSingleValue(commaSearch.Slice(0, ind).Trim(), out parsedValue, caseInsensitive))
                return false;

            outputArray[commaCount] = (DatFlagEnumValue)parsedValue;
            commaCount++;
            if (ind >= commaSearch.Length)
                break;
            commaSearch = commaSearch.Slice(ind + 1);
        }

        value = DatFlagEnumValue.Create(new OneOrMore<DatFlagEnumValue>(outputArray), this, default);
        return true;
    }

#pragma warning disable CS8500

    internal static unsafe string CreateValueString(ReadOnlySpan<DatFlagEnumValue> valuesSpan, out string? casing)
    {
        bool hasCasing = false;
        int totalLength = 0;
        foreach (DatFlagEnumValue v in valuesSpan)
        {
            totalLength += v.Value.Length;
            if (!hasCasing)
                hasCasing = v.CasingValue != null;
        }
        totalLength += (valuesSpan.Length - 1) * 2;

        ValueSpanStringState state;
        state.Values = &valuesSpan;
        state.Casing = false;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        string valueStr = string.Create(totalLength, state, WriteToStringWithSpan);
        state.Casing = true;
        casing = hasCasing ? string.Create(totalLength, state, WriteToStringWithSpan) : null;
#else
        char[] arr = new char[totalLength];
        WriteToStringWithSpan(arr, state);
        string valueStr = new string(arr, 0, totalLength);
        if (hasCasing)
        {
            state.Casing = true;
            WriteToStringWithSpan(arr, state);
            casing = new string(arr, 0, totalLength);
        }
        else
        {
            casing = null;
        }
#endif
        return valueStr;
    }

    private static unsafe void WriteToStringWithSpan(Span<char> span, ValueSpanStringState state)
    {
        ReadOnlySpan<DatFlagEnumValue> values = *state.Values;
        int index = 0;
        for (int i = 0; i < values.Length; ++i)
        {
            DatFlagEnumValue val = values[i];
            string value = state.Casing ? val.Casing : val.Value;
            value.AsSpan().CopyTo(span[index..]);
            index += value.Length;
            if (i == values.Length - 1)
                break;

            span[index] = ',';
            ++index;
            span[index] = ' ';
            ++index;
        }
    }

    private unsafe struct ValueSpanStringState
    {
        public ReadOnlySpan<DatFlagEnumValue>* Values;
        public bool Casing;
    }

    internal static string CreateValueString(OneOrMore<DatFlagEnumValue> values, out string? casing)
    {
        bool hasCasing = false;
        int totalLength = 0;
        foreach (DatFlagEnumValue v in values)
        {
            totalLength += v.Value.Length;
            if (!hasCasing)
                hasCasing = v.CasingValue != null;
        }
        totalLength += (values.Length - 1) * 2;

        ValueArrayStringState state;
        state.Values = values;
        state.Casing = false;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        string valueStr = string.Create(totalLength, state, WriteToStringWithArray);
        state.Casing = true;
        casing = hasCasing ? string.Create(totalLength, state, WriteToStringWithArray) : null;
#else
        char[] arr = new char[totalLength];
        WriteToStringWithArray(arr, state);
        string valueStr = new string(arr, 0, totalLength);
        if (hasCasing)
        {
            state.Casing = true;
            WriteToStringWithArray(arr, state);
            casing = new string(arr, 0, totalLength);
        }
        else
        {
            casing = null;
        }
#endif
        return valueStr;
    }

    private static void WriteToStringWithArray(Span<char> span, ValueArrayStringState state)
    {
        OneOrMore<DatFlagEnumValue> values = state.Values;
        int index = 0;
        for (int i = 0; i < values.Length; ++i)
        {
            DatFlagEnumValue val = values[i];
            string value = state.Casing ? val.Casing : val.Value;
            value.AsSpan().CopyTo(span[index..]);
            index += value.Length;
            if (i == values.Length - 1)
                break;

            span[index] = ',';
            ++index;
            span[index] = ' ';
            ++index;
        }
    }

    private struct ValueArrayStringState
    {
        public OneOrMore<DatFlagEnumValue> Values;
        public bool Casing;
    }

#pragma warning restore CS8500

    /// <summary>
    /// Deconstructs a bitwise composite number into an equivalent set of enum values.
    /// </summary>
    /// <param name="composite">The bitwise combination of values to deconstruct.</param>
    /// <param name="strict">Whether or not an exception will be thrown if some bits didn't have matching values.</param>
    /// <exception cref="FormatException">Some bits didn't have matching values, when <paramref name="strict"/> was <see langword="true"/>.</exception>
    public OneOrMore<DatFlagEnumValue> Deconstruct(long composite, bool strict)
    {
        if (composite == 0)
        {
            foreach (DatEnumValue v in Values)
            {
                DatFlagEnumValue f = (DatFlagEnumValue)v;
                if (f.NumericValue == 0)
                    return new OneOrMore<DatFlagEnumValue>(f);
            }

            return OneOrMore<DatFlagEnumValue>.Null;
        }

        ulong comp = unchecked( (ulong)composite );

        int popcnt = PopCount(comp);
        DatFlagEnumValue[]? bits = popcnt == 1 ? null : new DatFlagEnumValue[popcnt];
        DatFlagEnumValue? bit1 = null;

        int bitIndex = 0;

        ulong value = comp;
        // Uses https://www.geeksforgeeks.org/dsa/count-set-bits-in-an-integer/#expected-approach-1-brian-kernighans-algorithm
        while (value != 0)
        {
            ulong oldVal = value;
            value &= value - 1;
            long numericValue = unchecked( (long)(value ^ oldVal) );
            DatFlagEnumValue? match = null;
            ImmutableArray<DatEnumValue> values = Values;
            for (int i = 0; i < values.Length; ++i)
            {
                DatFlagEnumValue enumValue = (DatFlagEnumValue)values[i];
                if (enumValue.NumericValue == numericValue)
                {
                    match = enumValue;
                    break;
                }
            }

            if (match != null)
            {
                if (bits == null)
                    bit1 = match;
                else
                    bits[bitIndex] = match;
                ++bitIndex;
            }
        }

        if (bitIndex < popcnt)
        {
            if (strict)
                throw new FormatException("One or more bits did not have a matching enum value.");

            if (bits == null)
                return OneOrMore<DatFlagEnumValue>.Null;

            if (bitIndex == 1)
                return new OneOrMore<DatFlagEnumValue>(bits[0]);

            Array.Resize(ref bits, bitIndex);
        }

        return bits == null ? new OneOrMore<DatFlagEnumValue>(bit1!) : new OneOrMore<DatFlagEnumValue>(bits);
    }

    private static int PopCount(ulong value)
    {
        // from https://github.com/dotnet/runtime/blob/e2005d178d14ea5816a3b58fc06aacc2b4a7983b/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs#L492
        const ulong c1 = 0x_55555555_55555555ul;
        const ulong c2 = 0x_33333333_33333333ul;
        const ulong c3 = 0x_0F0F0F0F_0F0F0F0Ful;
        const ulong c4 = 0x_01010101_01010101ul;

        value -= (value >> 1) & c1;
        value = (value & c2) + ((value >> 2) & c2);
        value = (((value + (value >> 4)) & c3) * c4) >> 56;

        return (int)value;
    }

    IValue<DatFlagEnumValue> IType<DatFlagEnumValue>.CreateValue(Optional<DatFlagEnumValue> value) => value.Value ?? Null;
    IType<DatFlagEnumValue> ITypeConverter<DatFlagEnumValue>.DefaultType => this;

    bool ITypeConverter<DatFlagEnumValue>.TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<DatFlagEnumValue> args, [MaybeNullWhen(false)] out DatFlagEnumValue parsedValue)
    {
        return TryParse(text, out parsedValue);
    }

    string ITypeConverter<DatFlagEnumValue>.Format(DatFlagEnumValue value, ref TypeConverterFormatArgs args)
    {
        return value.ToString();
    }

    bool ITypeConverter<DatFlagEnumValue>.TryFormat(Span<char> output, DatFlagEnumValue value, out int size, ref TypeConverterFormatArgs args)
    {
        string ts = value.ToString();
        size = ts.Length;
        return ts.AsSpan().TryCopyTo(output);
    }

    bool ITypeConverter<DatFlagEnumValue>.TryConvertTo<TTo>(Optional<DatFlagEnumValue> obj, out Optional<TTo> result)
    {
        return TryConvertTo(obj.Value, out result);
    }

    public void WriteJson(Utf8JsonWriter writer, DatFlagEnumValue value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }

    public bool TryReadJson(in JsonElement json, out Optional<DatFlagEnumValue> value, ref TypeConverterParseArgs<DatFlagEnumValue> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<DatFlagEnumValue>.Null;
                return true;

            case JsonValueKind.String:
                string str = json.GetString()!;
                if (TryParse(str, out DatFlagEnumValue? val, caseInsensitive: false))
                {
                    value = val;
                    return true;
                }
                goto default;

            default:
                value = Optional<DatFlagEnumValue>.Null;
                return false;
        }
    }

}

/// <summary>
/// A single value from an enum type.
/// </summary>
public class DatEnumValue : IValue<DatEnumValue>, IEquatable<DatEnumValue>, IDatSpecificationObject
{
    internal string? CasingValue;

    /// <summary>
    /// The root object of this value, unless it was created at runtime (ex. during a unit test).
    /// </summary>
    /// <remarks>This will also be uninitialized if this value was defined by just a string, ex. <c>"Values": [ "A", "B", "C" ]</c>.</remarks>
    public JsonElement DataRoot { get; }

    /// <summary>
    /// The index of this enum value within it's "Values" array.
    /// </summary>
    /// <remarks>This will be -1 if this value is a bitwise combination of multiple other values. In that case this object will be a <see cref="DatFlagEnumValue"/>.</remarks>
    public int Index { get; }

    /// <summary>
    /// The enum type that defines this value.
    /// </summary>
    public DatEnumType Owner { get; }

    /// <summary>
    /// The value of this enum, as specified in C# code with matching casing.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// A description of this specific enum value.
    /// </summary>
    public string? Description { get; internal set; }

    /// <summary>
    /// A shorter name of abbreviation for the enum value.
    /// </summary>
    public string? Abbreviation { get; internal set; }

    /// <summary>
    /// URL to the SDG docs for this enum value.
    /// </summary>
    public string? Docs { get; internal set; }

    /// <summary>
    /// The version of Unturned this enum value was added in.
    /// </summary>
    public Version? Version { get; internal set; }

    /// <summary>
    /// The numeric value of this enum, used for [Flag] enums.
    /// </summary>
    public long? NumericValue { get; internal set; }

    /// <summary>
    /// The value of this enum with ideal proper-casing.
    /// </summary>
    public string Casing
    {
        get => CasingValue ?? Value;
        internal set
        {
            if (value.Length != Value.Length)
                throw new ArgumentException(Resources.ArgumentException_CasingValueLengthMismatch);

            CasingValue = value;
        }
    }

    /// <summary>
    /// The type that this value corresponds to when using the "TypeOrEnum" type or similar.
    /// </summary>
    public QualifiedType CorrespondingType { get; internal set; }

    /// <summary>
    /// Specifies that, when using the "TypeOrEnum" type or similar, the file type must be assignable to this value.
    /// <para>
    /// Also used by properties that use the "SubtypeSwitch" property.
    /// </para>
    /// </summary>
    public QualifiedType RequiredBaseType { get; internal set; }

    /// <summary>
    /// Whether or not this specific value is deprecated/obsolete.
    /// </summary>
    public bool Deprecated { get; internal set; }

    internal DatEnumValue(string value, int index, DatEnumType owner, JsonElement element)
    {
        Value = value;
        Index = index;
        Owner = owner;
        DataRoot = element;
    }


    /// <summary>
    /// Create a new <see cref="DatEnumValue"/> with the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value of this enum, as specified in C# code with matching casing.</param>
    /// <param name="index">The index of this enum value within it's "Values" array.</param>
    /// <param name="owner">The enum type that defines this value.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static DatEnumValue Create(string value, int index, DatEnumType owner, JsonElement element)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        return new DatEnumValue(value, index, owner, element);
    }

    bool IValue.IsNull => false;

    IType<DatEnumValue> IValue<DatEnumValue>.Type => Owner;

    bool IValue<DatEnumValue>.TryGetConcreteValue(out Optional<DatEnumValue> value)
    {
        value = new Optional<DatEnumValue>(this);
        return true;
    }

    bool IValue<DatEnumValue>.TryEvaluateValue(out Optional<DatEnumValue> value, in FileEvaluationContext ctx)
    {
        value = new Optional<DatEnumValue>(this);
        return true;
    }

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        visitor.Accept(new Optional<DatEnumValue>(this));
        return true;
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        visitor.Accept(new Optional<DatEnumValue>(this));
        return true;
    }

    public bool Equals(DatEnumValue? other)
    {
        if (other is not DatFlagEnumValue v)
            return other == this;

        if (Index == v.Index)
            return true;

        return this is DatFlagEnumValue t && v.Equals(t);

    }

    /// <inheritdoc />
    public bool Equals(IValue? other) => Equals(other as DatEnumValue);

    public override string ToString()
    {
        return Value;
    }

    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Value);
    }

    DatFileType IDatSpecificationObject.Owner => Owner.Owner;
    string IDatSpecificationObject.FullName => Index == -1
        ? $"{Owner.Owner.TypeName.GetFullTypeName()}/{Owner.TypeName.GetFullTypeName()}/[{Value}]"
        : $"{Owner.Owner.TypeName.GetFullTypeName()}/{Owner.TypeName.GetFullTypeName()}.{Value}";
}

/// <summary>
/// One or move values from a flag enum type.
/// </summary>
public class DatFlagEnumValue : DatEnumValue, IValue<DatFlagEnumValue>, IEquatable<DatFlagEnumValue>
{
    /// <summary>
    /// The enum type that defines this value.
    /// </summary>
    public new DatFlagEnumType Owner => (DatFlagEnumType)base.Owner;

    /// <summary>
    /// The bitwise value of this combination of values.
    /// </summary>
    public new long NumericValue { get; }

    /// <summary>
    /// List of all defined values included in this value's bitwise mask.
    /// </summary>
    /// <remarks>All values included in this list are defined values. It is possible that there are extra bits that weren't defined and will not be included in this list.</remarks>
    public OneOrMore<DatFlagEnumValue> Values { get; }

    internal DatFlagEnumValue(string value, int index, DatFlagEnumType owner, long numericValue, JsonElement element)
        : base(value, index, owner, element)
    {
        NumericValue = numericValue;
        base.NumericValue = numericValue;
        Values = new OneOrMore<DatFlagEnumValue>(this);
    }

    internal DatFlagEnumValue(string value, int index, DatFlagEnumType owner, long numericValue, OneOrMore<DatFlagEnumValue> values, JsonElement element)
        : base(value, index, owner, element)
    {
        NumericValue = numericValue;
        base.NumericValue = numericValue;
        Values = values;
    }

    /// <summary>
    /// Create a new <see cref="DatFlagEnumValue"/> with the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value of this enum, as specified in C# code with matching casing.</param>
    /// <param name="index">The index of this enum value within it's "Values" array.</param>
    /// <param name="owner">The enum type that defines this value.</param>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static DatFlagEnumValue Create(string value, int index, DatFlagEnumType owner, long numericValue, JsonElement element)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
     
        return new DatFlagEnumValue(value, index, owner, numericValue, OneOrMore<DatFlagEnumValue>.Null, element);
    }

    /// <summary>
    /// Create a new composite <see cref="DatFlagEnumValue"/> with the given <paramref name="values"/>.
    /// </summary>
    /// <param name="values">List of all included flags.</param>
    /// <param name="owner">The enum type that defines this value.</param>
    /// <exception cref="ArgumentException">A composite value was contained in the <paramref name="values"/> array.</exception>
    public static DatFlagEnumValue Create(OneOrMore<DatFlagEnumValue> values, DatFlagEnumType owner, JsonElement element)
    {
        long actualValue = 0;

        if (values.Length == 0)
            return new DatFlagEnumValue(string.Empty, -1, owner, 0, OneOrMore<DatFlagEnumValue>.Null, element);
        
        if (values.Length == 1)
            return values[0];
        
        foreach (DatFlagEnumValue v in values)
        {
            if (v.Index < 0)
                throw new ArgumentException(Resources.ArgumentException_CompositeFlagValue, nameof(values));

            actualValue |= v.NumericValue;
        }

        string value = DatFlagEnumType.CreateValueString(values, out string? casing);
        return new DatFlagEnumValue(value, values.Length == 1 ? values[0].Index : -1, owner, actualValue, values, element)
        {
            CasingValue = casing
        };
    }

    IType<DatFlagEnumValue> IValue<DatFlagEnumValue>.Type => Owner;

    bool IValue<DatFlagEnumValue>.TryGetConcreteValue(out Optional<DatFlagEnumValue> value)
    {
        value = new Optional<DatFlagEnumValue>(this);
        return true;
    }

    bool IValue<DatFlagEnumValue>.TryEvaluateValue(out Optional<DatFlagEnumValue> value, in FileEvaluationContext ctx)
    {
        value = new Optional<DatFlagEnumValue>(this);
        return true;
    }

    public bool Equals(DatFlagEnumValue? other)
    {
        if (other == null)
            return false;

        if (other.Index != -1 || Index != -1)
        {
            return other.Index == Index;
        }

        return other.Values.Equals(Values);
    }
}
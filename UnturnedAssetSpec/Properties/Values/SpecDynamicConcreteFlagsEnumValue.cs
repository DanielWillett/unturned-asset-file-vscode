using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// A list of bitwise flags as a <see cref="ISpecDynamicValue"/>.
/// </summary>
[DebuggerDisplay("{ToString(),nq} [{CompositeValue:X,h}]")]
public sealed class SpecDynamicConcreteFlagsEnumValue :
    IEquatable<SpecDynamicConcreteFlagsEnumValue>,
    IEquatable<ISpecDynamicValue>,
    ISpecConcreteValue
{
    /// <summary>
    /// The enum type representing this flag. Must have <see cref="EnumSpecType.IsFlags"/> set to <see langword="true"/>.
    /// </summary>
    public EnumSpecType Type { get; }

    /// <summary>
    /// List of indices in <see cref="EnumSpecType.Values"/> for which fields are included.
    /// </summary>
    public OneOrMore<int> Values { get; }

    /// <summary>
    /// The result of the binary operation.
    /// </summary>
    public long CompositeValue { get; }

    public SpecDynamicConcreteFlagsEnumValue(EnumSpecType type, long compositeValue)
    {
        Type = type;
        CompositeValue = compositeValue;
        Values = Deconstruct(type, compositeValue, false);
    }

    public SpecDynamicConcreteFlagsEnumValue(EnumSpecType type, params OneOrMore<int> flagIndices)
    {
        Values = flagIndices;
        long comp = 0;
        foreach (int v in flagIndices)
        {
            if (v < 0 || v >= type.Values.Length)
                throw new ArgumentOutOfRangeException(nameof(flagIndices), $"Flag index {v} is out of range of the enum type {type.Type.GetFullTypeName()}.");

            ref EnumSpecTypeValue value = ref type.Values[v];
            if (!value.NumericValue.HasValue)
                throw new ArgumentOutOfRangeException(nameof(flagIndices), $"Flag index {v} ({value.Value}) does not specify a numeric value in {type.Type.GetFullTypeName()}.");

            comp |= value.NumericValue.Value;
        }

        CompositeValue = comp;
        Type = type;
    }

    /// <inheritdoc />
    public bool Equals(SpecDynamicConcreteFlagsEnumValue other)
    {
        if (!Type.Equals(other.Type))
            return false;

        return CompositeValue == other.CompositeValue;
    }

    /// <inheritdoc />
    public bool Equals(ISpecDynamicValue other)
    {
        if (Values.IsNull && other is SpecDynamicConcreteNullValue)
            return true;

        return other is SpecDynamicConcreteFlagsEnumValue enumValue && Equals(enumValue);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Type.GetHashCode() ^ (int)CompositeValue ^ (int)(CompositeValue >> 32);

    /// <inheritdoc />
    public override string ToString()
    {
        if (Values.IsNull)
            return "null";

        StringBuilder sb = new StringBuilder(Type.Type.GetTypeName(), 64).Append('.');
        if (Values.IsSingle)
            sb.Append(Type.Values[Values[0]].Value);
        else
        {
            sb.Append('[');
            for (int i = 0; i < Values.Length; i++)
            {
                if (i != 0)
                    sb.Append(" | ");
                int v = Values[i];
                sb.Append(Type.Values[Values[v]].Value);
            }

            sb.Append(']');
        }

        return sb.ToString();
    }

    ISpecPropertyType ISpecDynamicValue.ValueType => Type;

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Operation is ConditionOperation.Included or ConditionOperation.ValueIncluded)
            return !condition.IsInverted;

        if (condition.Operation == ConditionOperation.Excluded)
            return condition.IsInverted;

        if (condition.Comparand is not string str
            || !TryParseFlags(Type, str.AsSpan(), out OneOrMore<int> values, ignoreCase: condition.Operation.IsCaseInsensitive()))
        {
            return condition.EvaluateNulls(Values.IsNull, true);
        }

        return Values.IsNull
            ? condition.EvaluateNulls(true, false)
            : condition.Evaluate(Values, values, ctx.Information.Information);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        return TryEvaluateValue(out value, out isNull);
    }

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = GetStringFromIndices(Type, Values);
        return true;
    }

    /// <summary>
    /// Creates a round-trip string for a list of flags.
    /// </summary>
    public static string? GetStringFromIndices(EnumSpecType type, OneOrMore<int> flagIndices, bool useCasing = false)
    {
        return GetStringFromIndices(type, flagIndices, ReadOnlySpan<int>.Empty, false, useCasing);
    }

    /// <inheritdoc cref="GetStringFromIndices(EnumSpecType,OneOrMore{int})"/>
    public static string? GetStringFromIndices(EnumSpecType type, ReadOnlySpan<int> flagIndices, bool useCasing = false)
    {
        return GetStringFromIndices(type, OneOrMore<int>.Null, flagIndices, true, useCasing);
    }

#pragma warning disable CS8500
    private static string? GetStringFromIndices(EnumSpecType type, OneOrMore<int> flagIndicesArray, ReadOnlySpan<int> flagIndicesSpan, bool span, bool useCasing = false)
    {
        int indexLength = span ? flagIndicesSpan.Length : flagIndicesArray.Length;
        if (indexLength == 0)
            return null;

        int ttlLength = indexLength - 1;
        for (int i = 0; i < indexLength; ++i)
        {
            ref EnumSpecTypeValue value = ref type.Values[span ? flagIndicesSpan[i] : flagIndicesArray[i]];
            ttlLength += (useCasing ? value.Casing : value.Value).Length;
        }

#if NETSTANDARD2_1_OR_GREATER
        unsafe
        {
            GetStringFromIndicesState state = default;
            state.Type = type;
            state.IsSpan = span;
            state.UseCasing = useCasing;
            state.IndexLength = indexLength;
            if (span)
                state.Span = &flagIndicesSpan;
            else
                state.Array = flagIndicesArray;

            return string.Create(ttlLength, state, static (span, state) =>
            {
                ReadOnlySpan<int> maybeSpan = *state.Span;
                int index = 0;
                for (int i = 0; i < state.IndexLength; ++i)
                {
                    ref EnumSpecTypeValue value = ref state.Type.Values[state.IsSpan ? maybeSpan[i] : state.Array[i]];
                    if (i != 0)
                    {
                        span[index] = ',';
                        ++index;
                        span[index] = ' ';
                        ++index;
                    }

                    ReadOnlySpan<char> val = (state.UseCasing ? value.Casing : value.Value).AsSpan();
                    val.CopyTo(span[index..]);
                    index += val.Length;
                }
            });
        }
#else
        if (ttlLength > 512)
        {
            StringBuilder sb = new StringBuilder(ttlLength);
            for (int i = 0; i < indexLength; ++i)
            {
                ref EnumSpecTypeValue value = ref type.Values[span ? flagIndicesSpan[i] : flagIndicesArray[i]];
                if (i != 0)
                    sb.Append(", ");

                sb.Append(useCasing ? value.Casing : value.Value);
            }

            return sb.ToString();
        }

        Span<char> alloc = stackalloc char[ttlLength];
        int index = 0;
        for (int i = 0; i < indexLength; ++i)
        {
            ref EnumSpecTypeValue value = ref type.Values[span ? flagIndicesSpan[i] : flagIndicesArray[i]];
            if (i != 0)
            {
                alloc[index] = ',';
                ++index;
                alloc[index] = ' ';
                ++index;
            }

            ReadOnlySpan<char> val = (useCasing ? value.Casing : value.Value).AsSpan();
            val.CopyTo(alloc[index..]);
            index += val.Length;
        }

        return alloc.ToString();
#endif
    }

#if NETSTANDARD2_1_OR_GREATER
    private unsafe struct GetStringFromIndicesState
    {
        public EnumSpecType Type;
        public OneOrMore<int> Array;
        public ReadOnlySpan<int>* Span;
        public bool IsSpan;
        public bool UseCasing;
        public int IndexLength;
    }
#endif
#pragma warning restore CS8500

    public static bool TryParseFlags(EnumSpecType type, ReadOnlySpan<char> toParse, out OneOrMore<int> flags, bool ignoreCase = false)
    {
        int index;

        toParse = toParse.Trim();
        flags = OneOrMore<int>.Null;
        int firstComma = toParse.IndexOf(',');
        if (firstComma == -1)
        {
            if (!type.TryParse(toParse, out index, ignoreCase))
                return false;

            flags = new OneOrMore<int>(index);
            return true;
        }

        int commaCount = 1;
        ReadOnlySpan<char> commaSearch = toParse.Slice(firstComma + 1);
        while (commaSearch.Length > 0)
        {
            int ind = commaSearch.IndexOf(',');
            if (ind < 0)
                break;

            commaCount++;
            commaSearch = commaSearch.Slice(ind + 1);
        }

        int[] outputArray = new int[commaCount + 1];
        ReadOnlySpan<char> element = toParse.Slice(0, firstComma).Trim();
        if (!type.TryParse(element, out index, ignoreCase))
            return false;

        outputArray[0] = index;
        commaSearch = toParse.Slice(firstComma + 1);
        commaCount = 1;
        while (commaSearch.Length > 0)
        {
            int ind = commaSearch.IndexOf(',');
            if (ind < 0)
                ind = commaSearch.Length;

            if (!type.TryParse(commaSearch.Slice(0, ind).Trim(), out index, ignoreCase))
                return false;

            outputArray[commaCount] = index;
            commaCount++;
            if (ind >= commaSearch.Length)
                break;
            commaSearch = commaSearch.Slice(ind + 1);
        }

        flags = new OneOrMore<int>(outputArray);
        return true;
    }

    /// <summary>
    /// Deconstructs a bitwise composite number into an equivalent set of enum values.
    /// </summary>
    public static OneOrMore<int> Deconstruct(EnumSpecType type, long composite, bool strict)
    {
        if (composite == 0)
            return OneOrMore<int>.Null;

        ulong comp = unchecked ( (ulong)composite );

        int popcnt = PopCount(comp);
        int[]? bits = popcnt == 1 ? null : new int[popcnt];
        int bit1 = 0;

        int bitIndex = 0;

        ulong value = comp;
        // Uses https://www.geeksforgeeks.org/dsa/count-set-bits-in-an-integer/#expected-approach-1-brian-kernighans-algorithm
        while (value != 0)
        {
            ulong oldVal = value;
            value &= value - 1;
            long numericValue = unchecked( (long)(value ^ oldVal) );
            int index = -1;
            for (int i = 0; i < type.Values.Length; ++i)
            {
                ref EnumSpecTypeValue enumValue = ref type.Values[i];
                if (enumValue.NumericValue == numericValue)
                {
                    index = i;
                    break;
                }
            }

            if (index != -1)
            {
                if (bits == null)
                    bit1 = index;
                else
                    bits[bitIndex] = index;
                ++bitIndex;
            }
        }

        if (bitIndex < popcnt)
        {
            if (strict)
                throw new FormatException("One or more bits did not have a matching enum value.");

            if (bits == null)
                return OneOrMore<int>.Null;

            if (bitIndex == 1)
                return new OneOrMore<int>(bits[0]);

            Array.Resize(ref bits, bitIndex);
        }

        return bits == null ? new OneOrMore<int>(bit1) : new OneOrMore<int>(bits);
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

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        if (Values.IsNull)
            writer.WriteNumberValue(0);
        else
        {
            StringBuilder sb = new StringBuilder(64);
            for (int i = 0; i < Values.Length; ++i)
            {
                int v = Values[i];
                ref EnumSpecTypeValue value = ref Type.Values[v];
                if (i != 0)
                    sb.Append(',');

                sb.Append(value.Value);
            }

            writer.WriteStringValue(sb.ToString());
        }
    }

    /// <inheritdoc />
    public bool TryEvaluateValue<TValue>(out TValue? value, out bool isNull)
    {
        isNull = Values.IsNull;

        if (typeof(TValue) == typeof(int))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<int, TValue>(checked( (int)CompositeValue ));
        else if (typeof(TValue) == typeof(string))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<string?, TValue>(GetStringFromIndices(Type, Values));
        else if (typeof(TValue) == typeof(uint))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<uint, TValue>(unchecked ( (uint)checked( (int)CompositeValue ) ));
        else if (typeof(TValue) == typeof(long))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<long, TValue>(CompositeValue);
        else if (typeof(TValue) == typeof(ulong))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<ulong, TValue>(unchecked ( (ulong)CompositeValue ));
        else if (typeof(TValue) == typeof(short))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<short, TValue>(checked( (short)CompositeValue ));
        else if (typeof(TValue) == typeof(ushort))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<ushort, TValue>(unchecked ( (ushort)checked( (short)CompositeValue ) ));
        else if (typeof(TValue) == typeof(sbyte))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<sbyte, TValue>(checked( (sbyte)CompositeValue ));
        else if (typeof(TValue) == typeof(byte))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<byte, TValue>(unchecked ( (byte)checked( (sbyte)CompositeValue ) ));
        else if (typeof(TValue) == typeof(float))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<float, TValue>(CompositeValue);
        else if (typeof(TValue) == typeof(double))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<double, TValue>(CompositeValue);
        else if (typeof(TValue) == typeof(decimal))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<decimal, TValue>(CompositeValue);
        else if (typeof(TValue) == typeof(char))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<char, TValue>(unchecked ( (char)checked( (short)CompositeValue ) ));
        else if (typeof(TValue) == typeof(bool))
            value = isNull ? default : SpecDynamicExpressionTreeValueHelpers.As<bool, TValue>(CompositeValue != 0);
        else
        {
            value = default;
            return false;
        }

        return true;
    }
}
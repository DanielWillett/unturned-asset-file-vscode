using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecConditionConverter : JsonConverter<SpecCondition>
{
    private static readonly JsonEncodedText[] Operations =
    [
        JsonEncodedText.Encode("lt"), 
        JsonEncodedText.Encode("gt"), 
        JsonEncodedText.Encode("lte"), 
        JsonEncodedText.Encode("gte"), 
        JsonEncodedText.Encode("eq"), 
        JsonEncodedText.Encode("neq"), 
        JsonEncodedText.Encode("contains"), 
        JsonEncodedText.Encode("starts with"), 
        JsonEncodedText.Encode("ends with"), 
        JsonEncodedText.Encode("matches"), 
        JsonEncodedText.Encode("contains-i"), 
        JsonEncodedText.Encode("eq-i"), 
        JsonEncodedText.Encode("starts with-i"), 
        JsonEncodedText.Encode("ends with-i"), 
        JsonEncodedText.Encode("assignable-to"), 
        JsonEncodedText.Encode("assignable-from"), 
        JsonEncodedText.Encode("included"), 
        JsonEncodedText.Encode("is-type")
    ];

    private static readonly JsonEncodedText VariableProperty = JsonEncodedText.Encode("Variable");
    private static readonly JsonEncodedText OperationProperty = JsonEncodedText.Encode("Operation");
    private static readonly JsonEncodedText ComparandProperty = JsonEncodedText.Encode("Comparand");

    /// <inheritdoc />
    public override SpecCondition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        SpecCondition s = default;
        if (reader.TokenType == JsonTokenType.Null)
        {
            return s;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} while reading SpecCondition.");
        }

        ISpecDynamicValue? variable = null;
        ConditionOperation? operation = null;
        bool hasComparand = false;
        object? comparand = null;
        Utf8JsonReader valueReader = default;
        bool hasValueReader = false;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                reader.Skip();

            if (reader.ValueTextEquals(VariableProperty.EncodedUtf8Bytes))
            {
                if (!reader.Read())
                    throw new JsonException("Failed to read 'Variable' in SpecCondition.");

                variable = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AssumeProperty);
            }
            else if (reader.ValueTextEquals(OperationProperty.EncodedUtf8Bytes))
            {
                if (!reader.Read() || reader.TokenType != JsonTokenType.String)
                    throw new JsonException("Failed to read 'Operation' in SpecCondition.");

                for (int i = 0; i < Operations.Length; ++i)
                {
                    if (!reader.ValueTextEquals(Operations[i].EncodedUtf8Bytes))
                        continue;

                    operation = (ConditionOperation)i;
                    break;
                }

                if (!operation.HasValue)
                    throw new JsonException($"Unknown operation '{reader.GetString()}' while reading SpecCondition.");
            }
            else if (reader.ValueTextEquals(ComparandProperty.EncodedUtf8Bytes))
            {
                if (!reader.Read())
                    throw new JsonException("Failed to read 'Comparand' in SpecCondition.");

                if (variable != null)
                {
                    comparand = SpecDynamicValue.ReadValue(ref reader, variable.ValueType, ThrowInvalidType);
                    hasValueReader = false;
                }
                else
                {
                    valueReader = reader;
                    hasValueReader = true;
                }

                hasComparand = true;
            }
        }

        if (variable == null)
            throw new JsonException("Missing 'Variable' while reading SpecCondition.");

        if (!operation.HasValue)
            throw new JsonException("Missing 'Operation' while reading SpecCondition.");

        if (hasValueReader)
        {
            comparand = SpecDynamicValue.ReadValue(ref valueReader, variable.ValueType, ThrowInvalidType);
            hasComparand = true;
        }

        if (!hasComparand)
            throw new JsonException("Missing 'Comparand' while reading SpecCondition.");

        return new SpecCondition(variable, operation.Value, comparand);
    }

    private static ISpecDynamicValue ThrowInvalidType(Type? t, ISpecPropertyType type)
    {
        if (t == null)
            return ThrowInvalidType(type);

        throw new JsonException($"Expected type {type.DisplayName} ({type.ValueType}) for 'Comparand' in SpecCondition but instead, {t} was given.");
    }

    private static ISpecDynamicValue ThrowInvalidType(ISpecPropertyType type)
    {
        throw new JsonException($"Expected type {type.DisplayName} ({type.ValueType}) for 'Comparand' in SpecCondition, but the given type couldn't be converted.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SpecCondition value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(VariableProperty, value.Variable.ToString());
        writer.WriteString(OperationProperty, Operations[(int)value.Operation]);

        writer.WritePropertyName(ComparandProperty);

        if (value.Comparand == null)
        {
            writer.WriteNullValue();
        }
        else if (value.Comparand is IConvertible c)
        {
            TypeCode typeCode = c.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case (TypeCode)17:
                default:
                    writer.WriteNullValue();
                    break;
                case TypeCode.Boolean:
                    writer.WriteBooleanValue((bool)value.Comparand);
                    break;
                case TypeCode.Char:
                    writer.WriteStringValue(value.Comparand.ToString());
                    break;
                case TypeCode.SByte:
                    writer.WriteNumberValue((sbyte)value.Comparand);
                    break;
                case TypeCode.Byte:
                    writer.WriteNumberValue((byte)value.Comparand);
                    break;
                case TypeCode.Int16:
                    writer.WriteNumberValue((short)value.Comparand);
                    break;
                case TypeCode.UInt16:
                    writer.WriteNumberValue((ushort)value.Comparand);
                    break;
                case TypeCode.Int32:
                    writer.WriteNumberValue((int)value.Comparand);
                    break;
                case TypeCode.UInt32:
                    writer.WriteNumberValue((uint)value.Comparand);
                    break;
                case TypeCode.Int64:
                    writer.WriteNumberValue((long)value.Comparand);
                    break;
                case TypeCode.UInt64:
                    writer.WriteNumberValue((ulong)value.Comparand);
                    break;
                case TypeCode.Single:
                    writer.WriteNumberValue((float)value.Comparand);
                    break;
                case TypeCode.Double:
                    writer.WriteNumberValue((double)value.Comparand);
                    break;
                case TypeCode.Decimal:
                    writer.WriteNumberValue((decimal)value.Comparand);
                    break;
                case TypeCode.DateTime:
                    writer.WriteStringValue((DateTime)value.Comparand);
                    break;
                case TypeCode.String:
                    writer.WriteStringValue((string)value.Comparand);
                    break;
            }
        }
        else
        {
            switch (value.Comparand)
            {
                case Guid g:
                    writer.WriteStringValue(g);
                    break;

                case DateTimeOffset dt:
                    writer.WriteStringValue(dt);
                    break;

                default:
                    writer.WriteStringValue(value.Comparand.ToString());
                    break;
            }
        }

        writer.WriteEndObject();
    }
}
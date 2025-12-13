using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Json;

public class TypeHierarchyConverter : JsonConverter<TypeHierarchy?>
{
    private static readonly JsonEncodedText IsAbstractProperty = JsonEncodedText.Encode("IsAbstract"u8);
    private static readonly JsonEncodedText HasDataFilesProperty = JsonEncodedText.Encode("HasDataFiles"u8);

    private static readonly IReadOnlyDictionary<QualifiedType, TypeHierarchy> EmptyDictionary =
        new ReadOnlyDictionary<QualifiedType, TypeHierarchy>(
            new Dictionary<QualifiedType, TypeHierarchy>()
        );

    /// <inheritdoc />
    public override TypeHierarchy? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        TypeHierarchy hierarchy = new TypeHierarchy();
        Dictionary<QualifiedType, TypeHierarchy>? childTypes = null;
    recheck:
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;

            case JsonTokenType.PropertyName:
                hierarchy.Type = new QualifiedType(reader.GetString());
                if (!reader.Read())
                    throw new JsonException("Expected object after starting on property name.");
                goto recheck;

            case JsonTokenType.StartObject:
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.PropertyName:
                            if (reader.ValueTextEquals(IsAbstractProperty.EncodedUtf8Bytes))
                            {
                                if (!reader.Read())
                                    throw new JsonException("File terminated early.");
                                hierarchy.IsAbstract = reader.TokenType == JsonTokenType.True;
                            }
                            else if (reader.ValueTextEquals(HasDataFilesProperty.EncodedUtf8Bytes))
                            {
                                if (!reader.Read())
                                    throw new JsonException("File terminated early.");
                                hierarchy.HasDataFiles = reader.TokenType == JsonTokenType.True;
                                if (reader.TokenType == JsonTokenType.True)
                                {
                                    ApplyHasDataFiles(hierarchy);
                                }
                            }
                            else
                            {
                                TypeHierarchy? child = Read(ref reader, typeToConvert, options);
                                child ??= new TypeHierarchy();
                                child.Parent = hierarchy;
                                if (hierarchy.HasDataFiles)
                                    ApplyHasDataFiles(child);
                                childTypes ??= new Dictionary<QualifiedType, TypeHierarchy>();
                                childTypes[child.Type] = child;
                            }
                            break;

                        default:
                            throw new JsonException($"Unexpected token {reader.TokenType} reading TypeHierarchy.");
                    }
                }

                break;

            case JsonTokenType.EndObject:
                break;

            default:
                throw new JsonException($"Unexpected token {reader.TokenType} reading TypeHierarchy.");
        }

        hierarchy.ChildTypes = childTypes ?? EmptyDictionary;
        return hierarchy;
    }

    private void ApplyHasDataFiles(TypeHierarchy hierarchy)
    {
        hierarchy.HasDataFiles = true;
        if (hierarchy.ChildTypes == null)
            return;

        foreach (TypeHierarchy child in hierarchy.ChildTypes.Values)
        {
            ApplyHasDataFiles(child);
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TypeHierarchy? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        if (value.IsAbstract)
        {
            writer.WriteBoolean("IsAbstract", true);
        }

        if (value.HasDataFiles && (value.Parent == null || !value.Parent.HasDataFiles))
        {
            writer.WriteBoolean("HasDataFiles", true);
        }

        if (value.ChildTypes is not { Count: > 0 })
        {
            writer.WriteEndObject();
            return;
        }

        foreach (TypeHierarchy child in value.ChildTypes.Values)
        {
            writer.WritePropertyName(child.Type.Type);

            Write(writer, child, options);
        }

        writer.WriteEndObject();
    }
}

using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class BundleReferenceTypeConverter : ITypeConverter<BundleReference>
{
    public IType<BundleReference> DefaultType => BundleReferenceType.GetInstance(BundleReferenceKind.MasterBundleReference);

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<BundleReference> args, out BundleReference parsedValue)
    {
        return BundleReference.TryParse(text, out parsedValue);
    }

    public string Format(BundleReference value, ref TypeConverterFormatArgs args)
    {
        return value.ToString();
    }

    public bool TryFormat(Span<char> output, BundleReference value, out int size, ref TypeConverterFormatArgs args)
    {
        if (value.Name == null || value.Path == null)
        {
            size = 0;
            return true;
        }

        int l = value.Name.Length + 1 + value.Path.Length;
        size = l;
        if (l > output.Length)
        {
            return false;
        }

        value.Name.AsSpan().CopyTo(output);
        output[value.Name.Length] = ':';
        if (value.Path.Length > 0)
        {
            value.Path.AsSpan().CopyTo(output.Slice(value.Name.Length + 1));
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is BundleReferenceTypeConverter;
    public override int GetHashCode() => 594605201;

    public bool TryConvertTo<TTo>(Optional<BundleReference> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(BundleReference))
        {
            result = Unsafe.As<Optional<BundleReference>, Optional<TTo>>(ref obj);
            return true;
        }

        BundleReference value = obj.Value;

        if (typeof(TTo) == typeof(string))
        {
            result = MathMatrix.As<string, TTo>(value.ToString());
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, BundleReference value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public bool TryReadJson(in JsonElement json, out Optional<BundleReference> value, ref TypeConverterParseArgs<BundleReference> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<BundleReference>.Null;
                return true;

            case JsonValueKind.String:
                if (!BundleReference.TryParse(json.GetString()!, out BundleReference bref))
                    goto default;

                value = bref;
                return true;

            case JsonValueKind.Object:
                if (json.TryGetProperty("Name"u8, out JsonElement nameElement)
                    && nameElement.ValueKind == JsonValueKind.String
                    && json.TryGetProperty("Path"u8, out JsonElement pathElement)
                    && pathElement.ValueKind == JsonValueKind.String)
                {
                    if (!json.TryGetProperty("Type"u8, out JsonElement typeElement)
                        || typeElement.ValueKind != JsonValueKind.String
                        || !Enum.TryParse(typeElement.GetString(), ignoreCase: true, out BundleReferenceKind refKind)
                        || refKind is < BundleReferenceKind.Unspecified or > BundleReferenceKind.TranslationReference)
                    {
                        refKind = BundleReferenceKind.Unspecified;
                    }

                    value = new BundleReference(nameElement.GetString()!, pathElement.GetString()!, refKind);
                    return true;
                }

                goto default;

            default:
                value = Optional<BundleReference>.Null;
                return false;
        }
    }
}
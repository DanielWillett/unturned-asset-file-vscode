using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class VersionTypeConverter : ITypeConverter<Version>
{
    public IType<Version> DefaultType => VersionType.Instance;

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<Version> args, [NotNullWhen(true)] out Version? parsedValue)
    {
        return Version.TryParse(args.StringOrSpan(text), out parsedValue);
    }

    public string Format(Version value, ref TypeConverterFormatArgs args)
    {
        return value.ToString();
    }

    public bool TryFormat(Span<char> output, Version value, out int size, ref TypeConverterFormatArgs args)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (value.TryFormat(output, out size))
        {
            return true;
        }

        uint fieldCt = value.Build != -1 ? value.Revision != -1 ? 4u : 3u : 2u;
        size = fieldCt switch
        {
            2 => StringHelper.CountDigits(value.Major) + 1 + StringHelper.CountDigits(value.Minor),
            3 => StringHelper.CountDigits(value.Major) + 1 + StringHelper.CountDigits(value.Minor)
                 + 1 + StringHelper.CountDigits(value.Build),
            _ => StringHelper.CountDigits(value.Major) + 1 + StringHelper.CountDigits(value.Minor)
                 + 1 + StringHelper.CountDigits(value.Build) + 1 + StringHelper.CountDigits(value.Revision)
        };

        return false;
#else
        string str = args.FormatCache ?? value.ToString();
        size = str.Length;
        if (str.AsSpan().TryCopyTo(output))
            return true;
        args.FormatCache = str;
        return false;
#endif
    }

    public override bool Equals(object? obj) => obj is VersionTypeConverter;
    public override int GetHashCode() => 417383781;

    public bool TryConvertTo<TTo>(Optional<Version> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(Version))
        {
            result = Unsafe.As<Optional<Version>, Optional<TTo>>(ref obj);
            return true;
        }

        if (typeof(TTo) == typeof(string))
        {
            result = new Optional<TTo>(MathMatrix.As<string, TTo>(obj.Value.ToString()));
            return true;
        }

        if (typeof(TTo) == typeof(Vector4))
        {
            Vector4 v4 = new Vector4(obj.Value.Major, obj.Value.Minor, obj.Value.Build, obj.Value.Revision);
            result = new Optional<TTo>(Unsafe.As<Vector4, TTo>(ref v4));
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, Version value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public bool TryReadJson(in JsonElement json, out Optional<Version> value, ref TypeConverterParseArgs<Version> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<Version>.Null;
                return true;

            case JsonValueKind.String:
                if (!Version.TryParse(json.GetString()!, out Version? v))
                {
                    value = Optional<Version>.Null;
                    return false;
                }

                value = new Optional<Version>(v);
                return true;

            case JsonValueKind.Array:
                switch (json.GetArrayLength())
                {
                    case 2:
                        int a0 = json[0].GetInt32(), a1 = json[1].GetInt32();
                        if (a0 < 0 || a1 < 0)
                            break;

                        value = new Version(a0, a1);
                        return true;

                    case 3:
                        a0 = json[0].GetInt32();
                        a1 = json[1].GetInt32();
                        int a2 = json[2].GetInt32();
                        if (a0 < 0 || a1 < 0 || a2 < 0)
                            break;

                        value = new Version(a0, a1, a2);
                        return true;

                    case 4:
                        a0 = json[0].GetInt32();
                        a1 = json[1].GetInt32();
                        a2 = json[2].GetInt32();
                        int a3 = json[3].GetInt32();
                        if (a0 < 0 || a1 < 0 || a2 < 0 || a3 < 0)
                            break;

                        value = new Version(a0, a1, a2, a3);
                        return true;
                }

                value = Optional<Version>.Null;
                return false;

            case JsonValueKind.Object:

                if (!json.TryGetProperty("Major"u8, out JsonElement majorElement)
                    || majorElement.ValueKind != JsonValueKind.Number
                    || !majorElement.TryGetInt32(out int major)
                    || !json.TryGetProperty("Minor"u8, out JsonElement minorElement)
                    || minorElement.ValueKind != JsonValueKind.Number
                    || !minorElement.TryGetInt32(out int minor))
                {
                    value = Optional<Version>.Null;
                    return false;
                }

                if (json.TryGetProperty("Build"u8, out JsonElement buildElement) && buildElement.ValueKind != JsonValueKind.Null)
                {
                    if (buildElement.ValueKind != JsonValueKind.Number || !buildElement.TryGetInt32(out int build))
                    {
                        value = Optional<Version>.Null;
                        return false;
                    }

                    if (json.TryGetProperty("Revision"u8, out JsonElement revisionElement) && revisionElement.ValueKind != JsonValueKind.Null)
                    {
                        if (revisionElement.ValueKind != JsonValueKind.Number || !revisionElement.TryGetInt32(out int revision))
                        {
                            value = Optional<Version>.Null;
                            return false;
                        }

                        value = new Version(major, minor, build, revision);
                        return true;
                    }

                    value = new Version(major, minor, build);
                    return true;
                }

                value = new Version(major, minor);
                return true;

            default:
                value = Optional<Version>.Null;
                return false;
        }
    }
}
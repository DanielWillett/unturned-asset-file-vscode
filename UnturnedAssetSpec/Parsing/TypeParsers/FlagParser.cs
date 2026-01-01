using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class FlagParser : ITypeParser<bool>
{
    public bool TryParse(ref TypeParserArgs<bool> args, in FileEvaluationContext ctx, out Optional<bool> value)
    {
        if (args.ValueNode is IValueSourceNode v)
        {
            args.CreateTypeConverterParseArgs(out TypeConverterParseArgs<bool> parseArgs, v.Value);
            if (TypeConverters.Boolean.TryParse(v.Value.AsSpan(), ref parseArgs, out bool parsedValue) && !parsedValue)
            {
                // false value included for flag
                args.DiagnosticSink?.UNT2003(ref args, v);
            }
        }

        if (args is { ShouldIgnoreFailureDiagnostic: false, ValueNode: not null })
        {
            // value included for flag
            args.DiagnosticSink?.UNT1003(ref args, args.ValueNode);
        }

        value = new Optional<bool>(true);
        return true;
    }

    public bool TryReadValueFromJson(in JsonElement json, out Optional<bool> value, IType<bool> valueType)
    {
        return TypeParsers.Boolean.TryReadValueFromJson(in json, out value, valueType);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, bool value, IType<bool> valueType, JsonSerializerOptions options)
    {
        TypeParsers.Boolean.WriteValueToJson(writer, value, valueType, options);
    }

    public override int GetHashCode() => 1959769108;
    public override bool Equals(object? obj) => obj is FlagParser;
}
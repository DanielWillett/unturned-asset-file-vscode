using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class BooleanOrFlagParser : ITypeParser<bool>
{
    public bool TryParse(ref TypeParserArgs<bool> args, in FileEvaluationContext ctx, out Optional<bool> value)
    {
        if (args.ValueNode is IValueSourceNode v)
        {
            args.CreateTypeConverterParseArgs(out TypeConverterParseArgs<bool> parseArgs, v.Value);
            if (TypeConverters.Boolean.TryParse(v.Value.AsSpan(), ref parseArgs, out bool parsedValue))
            {
                value = parsedValue;
                return true;
            }
        }

        if (args is { ShouldIgnoreFailureDiagnostic: false, ValueNode: not null })
        {
            // value included for flag (except for boolean values)
            args.DiagnosticSink?.UNT1003(ref args, args.ValueNode);
        }

        value = new Optional<bool>(true);
        return true;
    }

    public bool TryReadValueFromJson<TDataRefReadContext>(
        in JsonElement json,
        out Optional<bool> value,
        IType<bool> valueType,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        return TypeParsers.Boolean.TryReadValueFromJson(in json, out value, valueType, ref dataRefContext);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, bool value, IType<bool> valueType, JsonSerializerOptions options)
    {
        TypeParsers.Boolean.WriteValueToJson(writer, value, valueType, options);
    }

    public override int GetHashCode() => 2105882869;
    public override bool Equals(object? obj) => obj is BooleanOrFlagParser;
}
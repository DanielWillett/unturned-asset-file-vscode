using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class GuidTypeConverter : ITypeConverter<Guid>
{
    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<Guid> args, out Guid parsedValue)
    {
        return Guid.TryParse(args.StringOrSpan(text), out parsedValue);
    }

    public string Format(Guid value, ref TypeConverterFormatArgs args)
    {
        return value.ToString("N");
    }

    public bool TryFormat(Span<char> output, Guid value, out int size, ref TypeConverterFormatArgs args)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (value.TryFormat(output, out size, "N"))
        {
            return true;
        }

        size = 32;
        return false;
#else
        if (output.Length < 32)
        {
            size = 32;
            return false;
        }

        string str = value.ToString("N");
        size = str.Length;
        str.AsSpan().CopyTo(output);
        return true;
#endif
    }

    public override bool Equals(object? obj) => obj is GuidTypeConverter;
    public override int GetHashCode() => 825300216;

    public bool TryConvertTo<TTo>(Optional<Guid> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(Guid))
        {
            result = Unsafe.As<Optional<Guid>, Optional<TTo>>(ref obj);
            return true;
        }

        Guid value = obj.Value;

        if (typeof(TTo) == typeof(bool))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(value != Guid.Empty);
            return true;
        }
        if (typeof(TTo) == typeof(string))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<string, TTo>(value.ToString("N"));
            return true;
        }
        if (typeof(TTo) == typeof(GuidOrId))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId(value));
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Data-ref referencing the value being parsed from a <see cref="IValueSourceNode"/> when evaluating <see cref="DatCustomType.StringDefaultValue"/>.
/// </summary>
public sealed class ValueDataRef<TValueType> : RootDataRef<ValueDataRef<TValueType>>, IValue<TValueType>
    where TValueType : IEquatable<TValueType>
{
    /// <inheritdoc />
    public IType<TValueType> Type { get; }

    /// <inheritdoc />
    public override string PropertyName => "Value";

    protected override bool IsPropertyNameKeyword => true;

    public ValueDataRef(IType<TValueType> valueType)
    {
        Type = valueType;
    }


    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        if (!TryEvaluateValue(out Optional<TValueType> value, in ctx))
        {
            return false;
        }

        visitor.Accept(value);
        return true;
    }

    /// <inheritdoc />
    protected override bool Equals(ValueDataRef<TValueType> other)
    {
        return Type.Equals(other.Type);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(1056010348, Type);
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TValueType> value)
    {
        // Allowing concrete parsing would give the impression that context doesn't matter.

        // It does in fact matter, it's just being accessed through a static ThreadLocal<string>
        // so it doesn't need a reference to the context.

        value = Optional<TValueType>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TValueType> value, in FileEvaluationContext ctx)
    {
        TypeParserArgs<DatObjectValue> args = DatCustomType.ValueParseInfo.Value;
        if (args.Type == null)
        {
            value = Optional<TValueType>.Null;
            return false;
        }

        IValueSourceNode valueNode = (IValueSourceNode)args.ValueNode!;

        if (typeof(TValueType) == typeof(string))
        {
            value = new Optional<TValueType>(MathMatrix.As<string, TValueType>(valueNode.Value));
            return true;
        }

        ITypeParser<TValueType> typeParser = Type.Parser;
        if (typeParser is TypeConverterParser<TValueType> { CanUseTypeConverterDirectly: true } typeConverterParser)
        {
            args.CreateTypeConverterParseArgs(
                out TypeConverterParseArgs<TValueType> parseArgs,
                Type,
                valueNode.Value
            );

            if (typeConverterParser.TypeConverter.TryParse(valueNode.Value, ref parseArgs, out TValueType? parsedValue))
            {
                value = new Optional<TValueType>(parsedValue);
                return true;
            }
        }
        else
        {
            args.CreateSubTypeParserArgs(
                out TypeParserArgs<TValueType> parseArgs,
                valueNode,
                args.ParentNode,
                Type,
                args.KeyFilter
            );

            if (typeParser.TryParse(ref parseArgs, in ctx, out value))
            {
                return true;
            }
        }

        value = Optional<TValueType>.Null;
        return false;
    }
}
using System;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

/// <summary>
/// An expression value, which is read from any value starting with an equals sign.
/// </summary>
/// <typeparam name="TResult">The resulting type from the expression.</typeparam>
public class ExpressionValue<TResult> : IValue<TResult> where TResult : IEquatable<TResult>
{
    bool IValue.IsNull => false;
    public IType<TResult> Type { get; }

    public ExpressionValue(IType<TResult> resultType)
    {
        Type = resultType;
    }

    public bool TryGetConcreteValue(out Optional<TResult> value)
    {
        value = Optional<TResult>.Null;
        return false;
    }

    public bool TryEvaluateValue(out Optional<TResult> value, in FileEvaluationContext ctx)
    {
        value = Optional<TResult>.Null;
        return false;
    }

    IType IValue.Type => Type;
    public void WriteToJson(Utf8JsonWriter writer)
    {

    }
}
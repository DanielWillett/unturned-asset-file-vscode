using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

/// <summary>
/// An expression value, which is read from any value starting with an equals sign.
/// </summary>
/// <remarks>Create using <see cref="Value.FromExpression"/>.</remarks>
/// <typeparam name="TResult">The resulting type from the expression.</typeparam>
public class ExpressionValue<TResult> : IValue<TResult>, IEquatable<ExpressionValue<TResult>?> where TResult : IEquatable<TResult>
{
    /// <inheritdoc />
    public IType<TResult> Type { get; }

    /// <summary>
    /// The root expression node.
    /// </summary>
    internal IFunctionExpressionNode Root { get; }

    internal ExpressionValue(IType<TResult> resultType, IFunctionExpressionNode rootNode)
    {
        Type = resultType;
        Root = rootNode;
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TResult> value)
    {
        FileEvaluationContext ctx = default;
        return TryEvaluate(out value, true, in ctx);
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TResult> value, in FileEvaluationContext ctx)
    {
        return TryEvaluate(out value, false, in ctx);
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        StringBuilder stringBuilder = new StringBuilder();

        ExpressionNodeFormatter formatter = new ExpressionNodeFormatter(stringBuilder);
        formatter.WriteValue(Root);

        writer.WriteStringValue(stringBuilder.ToString());
    }

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor) where TVisitor : IValueVisitor
    {
        if (!TryGetConcreteValue(out Optional<TResult> r))
            return false;

        visitor.Accept(r);
        return true;
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx) where TVisitor : IValueVisitor
    {
        if (!TryEvaluateValue(out Optional<TResult> r, in ctx))
            return false;

        visitor.Accept(r);
        return true;
    }

    bool IValue.IsNull => false;

    private bool TryEvaluate(out Optional<TResult> value, bool isConcreteOnly, in FileEvaluationContext ctx)
    {
        ExpressionEvaluator evaluator = new ExpressionEvaluator(Root);

        ConvertVisitor<TResult> v;
        v.Result = default;
        v.WasSuccessful = false;
        v.IsNull = false;

        if (evaluator.Evaluate<TResult, ConvertVisitor<TResult>>(ref v, isConcreteOnly, in ctx))
        {
            value = v.IsNull ? Optional<TResult>.Null : v.Result!;
            return true;
        }

        value = Optional<TResult>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(ExpressionValue<TResult>? value)
    {
        if (value == null)
            return false;

        return value.Type.Equals(Type) && value.Root.Equals(Root);
    }

    /// <inheritdoc />
    public bool Equals(IValue? other)
    {
        return Equals(other as ExpressionValue<TResult>);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as ExpressionValue<TResult>);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(1513622216, Type, Root);
    }
}
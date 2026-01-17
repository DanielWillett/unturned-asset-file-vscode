using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Text;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Base type for all root data-refs, that is data-refs that do not have a target.
/// </summary>
/// <typeparam name="TSelf">The extending type.</typeparam>
public abstract class RootDataRef<TSelf> : BaseDataRefTarget, IDataRef
    where TSelf : RootDataRef<TSelf>
{
    internal string? EscapedPropertyName;

    /// <inheritdoc />
    public abstract string PropertyName { get; }

    protected virtual bool IsPropertyNameKeyword => false;

    /// <inheritdoc />
    protected override IDataRef DataRef => this;

    /// <inheritdoc />
    public abstract bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor;

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStringValue(GetExpressionString());
    }

    /// <inheritdoc />
    public StringBuilder AppendExpressionString(StringBuilder sb, bool hash = true)
    {
        if (hash)
            sb.Append('#');

        string propName = PropertyName;
        bool ws = StringHelper.ContainsWhitespace(propName) || propName.IndexOf('.') >= 0;
        if (ws)
            sb.Append('(');

        if (!IsPropertyNameKeyword && DataRefs.Keywords.Contains(propName))
        {
            sb.Append('\\').Append(propName);
        }
        else
        {
            if (EscapedPropertyName != null)
                propName = EscapedPropertyName;
            else
            {
                StringHelper.EscapeValue(ref propName);
                EscapedPropertyName = propName;
            }

            sb.Append(propName);
        }

        if (ws)
            sb.Append(')');

        return sb;
    }

    /// <inheritdoc />
    public string GetExpressionString(bool hash = true)
    {
        string propName = PropertyName;
        int length = 0;
        if (hash)
            ++length;
        bool ws = StringHelper.ContainsWhitespace(propName) || propName.IndexOf('.') >= 0;
        if (ws)
            length += 2;

        bool keyword = !IsPropertyNameKeyword && DataRefs.Keywords.Contains(propName);
        if (keyword)
        {
            length += propName.Length + 1;
        }
        else
        {
            if (EscapedPropertyName != null)
                propName = EscapedPropertyName;
            else
            {
                StringHelper.EscapeValue(ref propName);
                EscapedPropertyName = propName;
            }

            length += propName.Length;
        }

        Span<char> str = stackalloc char[length];
        int index = 0;
        if (hash)
        {
            str[index] = '#';
            ++index;
        }

        if (ws)
        {
            str[index] = '(';
            ++index;
        }

        if (keyword)
        {
            str[index] = '\\';
            ++index;
        }

        propName.AsSpan().CopyTo(str.Slice(index));
        index += propName.Length;
        if (ws)
        {
            str[index] = ')';
            ++index;
        }

        return str.ToString();
    }

    protected abstract bool Equals(TSelf other);

    /// <inheritdoc />
    public bool Equals(IValue? other)
    {
        return other is TSelf self && Equals(self);
    }

    /// <inheritdoc />
    public override bool Equals(IExpressionNode? other)
    {
        return other is TSelf self && Equals(self);
    }

    /// <inheritdoc />
    public sealed override bool Equals(object? obj)
    {
        return obj is TSelf self && Equals(self);
    }

    /// <inheritdoc />
    public abstract override int GetHashCode();

    /// <inheritdoc />
    public override string ToString()
    {
        return GetExpressionString(false);
    }

    bool IValue.IsNull => false;
    bool IValue.VisitConcreteValue<TVisitor>(ref TVisitor visitor) => false;
    IDataRefTarget? IDataRef.Target => null;
}

/// <summary>
/// Base type for all strongly-typed root data-refs, that is data-refs that do not have a target.
/// </summary>
/// <typeparam name="TSelf">The extending type.</typeparam>
public abstract class RootDataRef<TValue, TSelf> : RootDataRef<TSelf>
    where TValue : IEquatable<TValue>
    where TSelf : RootDataRef<TValue, TSelf>
{
    /// <summary>
    /// Attempts to evaluate a value given the workspace context.
    /// </summary>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="value">The evaluated value.</param>
    /// <returns>Whether or not a value could be determined.</returns>
    public abstract bool TryEvaluateValue(in FileEvaluationContext ctx, out Optional<TValue> value);

    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        if (!TryEvaluateValue(in ctx, out Optional<TValue> value))
        {
            return false;
        }

        visitor.Accept(value);
        return true;
    }
}
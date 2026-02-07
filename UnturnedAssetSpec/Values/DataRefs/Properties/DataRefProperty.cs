using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Text;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// An implementation of <see cref="IDataRef"/> that accesses a property of a target.
/// <para>
/// Example: <c>#Property.Included</c>.
/// </para>
/// </summary>
/// <typeparam name="TProperty">The type of property to use.</typeparam>
public class DataRefProperty<TProperty> : IDataRef, IEquatable<DataRefProperty<TProperty>>
    where TProperty : IDataRefProperty, IEquatable<TProperty>
{
    internal string? EscapedPropertyName;
    private readonly TProperty _property;

    public ref readonly TProperty Property => ref _property;

    public IDataRefTarget Target { get; }

    /// <inheritdoc />
    public string PropertyName => _property.PropertyName;

    /// <inheritdoc />
    public StringBuilder AppendExpressionString(StringBuilder sb, bool hash = true)
    {
        int startIndex = sb.Length;
        if (hash)
            sb.Append('#');

        Target.DataRef.AppendExpressionString(sb, hash: false);

        string propName = PropertyName;

        bool ws = StringHelper.ContainsWhitespace(propName) || propName.IndexOf('.') >= 0;
        bool parenthesisWrap = ws || sb.Length - startIndex > (hash ? 1 : 0) && sb[startIndex + (hash ? 1 : 0)] == '(';
        if (parenthesisWrap)
            sb.Insert(startIndex + (hash ? 1 : 0), '(');

        sb.Append('.');

        if (ws)
            sb.Append('(');

        if (EscapedPropertyName != null)
            propName = EscapedPropertyName;
        else
        {
            StringHelper.EscapeValue(ref propName);
            EscapedPropertyName = propName;
        }

        sb.Append(propName);

        if (ws)
            sb.Append(')');
        if (parenthesisWrap)
            sb.Append(')');

        return sb;
    }

    /// <inheritdoc />
    public string GetExpressionString(bool hash = true)
    {
        StringBuilder sb = new StringBuilder(64);
        AppendExpressionString(sb, hash);
        return sb.ToString();
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        return Target.AcceptProperty(in _property, ref visitor, in ctx);
    }

    public DataRefProperty(IDataRefTarget target, TProperty property)
    {
        Target = target;
        _property = property;
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStringValue(GetExpressionString());
    }

    /// <inheritdoc />
    public bool Equals(IExpressionNode? other)
    {
        return other is DataRefProperty<TProperty> prop && Equals(prop);
    }

    /// <inheritdoc />
    public bool Equals(IValue? other)
    {
        return other is DataRefProperty<TProperty> prop && Equals(prop);
    }

    public virtual bool Equals(DataRefProperty<TProperty>? other)
    {
        if (other == null)
            return false;

        return _property.Equals(other._property) && (Target?.Equals(other.Target) ?? other.Target == null);
    }

    bool IValue.VisitConcreteValue<TVisitor>(ref TVisitor visitor) => false;
    IDataRef IDataRefExpressionNode.DataRef => this;
    bool IValue.IsNull => false;
}
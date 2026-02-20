using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// An implementation of <see cref="IDataRef"/> that accesses a property of a target.
/// <para>
/// Example: <c>#Property.Included</c>.
/// </para>
/// </summary>
/// <typeparam name="TProperty">The type of property to use.</typeparam>
[DebuggerDisplay("{GetExpressionString(true),nq}")]
public class DataRefProperty<TProperty> : IDataRef, IEquatable<DataRefProperty<TProperty>>
    where TProperty : IDataRefProperty, IEquatable<TProperty>
{
    internal string? EscapedPropertyName;
    private readonly TProperty _property;
    private string? _propertiesString;

    public ref readonly TProperty Property => ref _property;

    public IDataRefTarget Target { get; }

    /// <inheritdoc />
    public string PropertyName => _property.PropertyName;

    /// <inheritdoc />
    public StringBuilder AppendExpressionString(StringBuilder sb, bool hash = true)
    {
        if (hash)
            sb.Append('#');

        Target.DataRef.AppendExpressionString(sb, hash: false);

        string propName = PropertyName;

        bool ws = StringHelper.ContainsWhitespace(propName) || propName.IndexOf('.') >= 0;

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

        if (_property is IIndexableDataRefProperty indexable)
        {
            OneOrMore<int> indices = indexable.Indices;
            sb.Append('[');
            for (int i = 0; i < indices.Length; i++)
            {
                if (i != 0)
                    sb.Append(',');
                sb.Append(indices[i].ToString(CultureInfo.InvariantCulture));
            }

            sb.Append(']');
        }

        if (_property is IConfigurableDataRefProperty configurable)
        {
            if (_propertiesString == null)
            {
                OneOrMore<KeyValuePair<string, object>> properties = configurable.Options;
                if (properties.Length <= 0)
                {
                    _propertiesString = string.Empty;
                }
                else
                {
                    using MemoryStream ms = new MemoryStream();
                    using Utf8JsonWriter writer = new Utf8JsonWriter(ms, new JsonWriterOptions
                    {
                        Indented = false,
                        SkipValidation = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                    writer.WriteStartObject();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        KeyValuePair<string, object> property = properties[i];
                        writer.WritePropertyName(property.Key);
                        JsonHelper.WriteGenericValue(writer, property.Value);
                    }

                    writer.WriteEndObject();

                    writer.Flush();

                    _propertiesString = StringHelper.Utf8NoBom.GetString(ms.GetBuffer(), 0, checked((int)ms.Length));
                }
            }

            sb.Append(_propertiesString);
        }

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

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is DataRefProperty<TProperty> prop && Equals(prop);
    }

    /// <inheritdoc />
    public override int GetHashCode() => _property.GetHashCode();

    public virtual bool Equals(DataRefProperty<TProperty>? other)
    {
        if (other == null)
            return false;

        return _property.Equals(other._property) && (Target?.Equals(other.Target) ?? other.Target == null);
    }

    bool IValue.VisitConcreteValue<TVisitor>(ref TVisitor visitor) => false;
    IDataRef IDataRefExpressionNode.DataRef => this;
    bool IValue.IsNull => false;

    /// <inheritdoc />
    public override string ToString() => GetExpressionString(true);
}

public class DataRefProperty<TProperty, TValue> : DataRefProperty<TProperty>, IDataRef<TValue>
    where TProperty : IDataRefProperty, IEquatable<TProperty>
    where TValue : IEquatable<TValue>
{
    /// <inheritdoc />
    public IType<TValue> Type { get; }

    public DataRefProperty(IType<TValue> type, IDataRefTarget target, TProperty property) : base(target, property)
    {
        Type = type;
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TValue> value)
    {
        value = Optional<TValue>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TValue> value, in FileEvaluationContext ctx)
    {
        ConvertVisitor<TValue> conv = default;
        VisitValue(ref conv, in ctx);
        value = conv.IsNull ? Optional<TValue>.Null : new Optional<TValue>(conv.Result);
        return conv.WasSuccessful;
    }
}
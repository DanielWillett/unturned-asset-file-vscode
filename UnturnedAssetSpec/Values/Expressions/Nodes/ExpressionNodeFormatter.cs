using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

/// <summary>
/// Formats expressions into <see cref="StringBuilder"/> or <see cref="TextWriter"/> instances.
/// </summary>
internal readonly struct ExpressionNodeFormatter
{
    private static readonly char[] Escapables = [ '(', ')', '\n', '\r', '\t', '\\', '@', '=', '%' ];

    private readonly TextWriter? _writer;
    private readonly StringBuilder? _sb;

    public ExpressionNodeFormatter(StringBuilder sb)
    {
        _sb = sb;
    }

    public ExpressionNodeFormatter(TextWriter writer)
    {
        _writer = writer;
    }

    /// <summary>
    /// Writes a function to a <see cref="StringBuilder"/> or <see cref="TextWriter"/>.
    /// </summary>
    /// <remarks>Doesn't include the '=' prefix, use <see cref="WriteValue"/> instead if that's needed.</remarks>
    public void WriteExpression(IFunctionExpressionNode rootNode)
    {
        WriteEscaped(rootNode.Function.FunctionName);
        int c = rootNode.Count;
        if (c <= 0)
            return;

        Write('(');
        for (int i = 0; i < c; ++i)
        {
            WriteValue(rootNode[i]);
            Write(i == c - 1 ? ')' : ' ');
        }
    }

    /// <summary>
    /// Writes a function to a <see cref="StringBuilder"/> or <see cref="TextWriter"/>.
    /// </summary>
    public void WriteValue(IExpressionNode node, bool? prefix = null)
    {
        switch (node)
        {
            case IFunctionExpressionNode func:
                if (prefix is not false)
                    Write('=');
                WriteExpression(func);
                return;

            case IValueExpressionNode value:
                WriteValueVisitor visitor;
                visitor.ValueString = null;
                visitor.ValueSuffix = null;
                // calls Accept below
                value.VisitConcreteValue(ref visitor);
                string? valueStr = visitor.ValueString;
                if (valueStr == null && prefix is not false)
                {
                    Write("=" + ExpressionFunctions.Null);
                }
                else
                {
                    if (prefix is true)
                        Write('%');
                    if (string.IsNullOrEmpty(valueStr))
                        Write("()");
                    else
                    {
                        WriteEscaped(valueStr);
                        if (visitor.ValueSuffix != null)
                            Write(visitor.ValueSuffix);
                    }
                }
                break;

            case IDataRefExpressionNode dataRef:
                if (prefix is not false)
                    Write('#');
                WriteDataRef(dataRef.DataRef);
                break;

            case IPropertyReferenceExpressionNode propRef:
                if (prefix is not false)
                    Write('@');
                PropertyReference r = propRef.Reference;
                WritePropertyRef(in r);
                break;
        }
    }

    private struct WriteValueVisitor : IValueVisitor
    {
        public string? ValueString;
        public string? ValueSuffix;
        public void Accept<TValue>(Optional<TValue> opt) where TValue : IEquatable<TValue>
        {
            if (!opt.HasValue)
            {
                ValueString = null;
                return;
            }

            if (TypeConverters.TryGet<TValue>() is { } converter)
            {
                TypeConverterFormatArgs args = default;
                ValueString = converter.Format(opt.Value, ref args);
                ValueSuffix = CommonTypes.GetTypeSuffix<TValue>();
            }
            else
            {
                ValueString = opt.Value.ToString();
            }
        }
    }

    private void Write(string str)
    {
        if (_sb != null)
            _sb.Append(str);
        else
            _writer?.Write(str);
    }

    private void Write(char c)
    {
        if (_sb != null)
            _sb.Append(c);
        else
            _writer?.Write(c);
    }

    private static string Escape(string val)
    {
        int index = val.IndexOfAny(Escapables);
        if (index >= 0)
        {
            StringHelper.EscapeValue(ref val, Escapables, startIndex: index);
        }

        return val;
    }

    private void WriteEscaped(string val)
    {
        string esc = Escape(val);

        if (val.IndexOf(' ') >= 0)
        {
            Write('(');
            Write(esc);
            Write(')');
        }
        else
        {
            Write(esc);
        }
    }

    private void WritePropertyRef(in PropertyReference propRef)
    {
        if (propRef.Context != SpecPropertyContext.Unspecified)
        {
            string prefix = PropertyReference.CreateContextSpecifier(propRef.Context, true);
            Write(prefix);
        }

        if (propRef.TypeName != null)
        {
            WriteEscaped(propRef.TypeName);
            Write("::");
        }

        WriteEscaped(propRef.Breadcrumbs.ToString(false, propRef.PropertyName));
    }

    private void WriteDataRef(DataRef dataRef)
    {
        IDataRefTarget target = dataRef.Target;
        switch (target)
        {
            case IExpressionNode node:
                WriteValue(node, false);
                Write('.');
                break;
            
            case not null:
                WriteEscaped(target.ToString()!);
                Write('.');
                break;
        }

        WriteEscaped(dataRef.PropertyName);

        if (dataRef is IIndexableDataRef indexable)
        {
            Write('[');
            if (_sb != null)
                _sb.Append(indexable.Index);
            else
                _writer?.Write(indexable.Index);
            Write(']');
        }

        if (dataRef is not IPropertiesDataRef properties)
            return;

        using IEnumerator<KeyValuePair<string, string>> props = properties.EnumerateProperties().GetEnumerator();
        if (!props.MoveNext())
            return;

        Write('{');
        bool needsComma = false;
        do
        {
            KeyValuePair<string, string> property = props.Current;
            if (needsComma)
                Write(',');
            else
                needsComma = true;
            WriteEscaped(property.Key);
            Write('=');
            WriteEscaped(property.Value);
        } while (props.MoveNext());

        Write('}');
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.ComponentModel;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A base type for normal property types that parse values.
/// </summary>
/// <typeparam name="TValue">Type of value being parsed.</typeparam>
/// <typeparam name="TSpecPropertyType">The property type being defined (self).</typeparam>

// note: this used to be called BasicSpecPropertyType which derived from BaseSpecPropertyType
//       but the original base one was pretty much useless so Base was removed and merged with Basic

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class BaseSpecPropertyType<TSpecPropertyType, TValue> :
    ISpecPropertyType<TValue>,
    IEquatable<BaseSpecPropertyType<TSpecPropertyType, TValue>?>
    where TSpecPropertyType : BaseSpecPropertyType<TSpecPropertyType, TValue>
    where TValue : IEquatable<TValue>
{
    /// <inheritdoc />
    public abstract SpecPropertyTypeKind Kind { get; }

    public abstract string Type { get; }
    public abstract string DisplayName { get; }

    public override string ToString() => Type;

    public abstract override int GetHashCode();

    public virtual bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (parse.AutoDefault)
        {
            value = parse.EvaluationContext.Self.DefaultValue!;
            return value != null;
        }

        if (!TryParseValue(in parse, out TValue? val))
        {
            value = null!;
            return false;
        }

        value = val == null ? SpecDynamicValue.Null : CreateValue(val);
        return true;
    }

    protected virtual ISpecDynamicValue CreateValue(TValue value)
    {
        return new SpecDynamicConcreteValue<TValue>(value, this);
    }

    /// <inheritdoc />
    public Type ValueType => typeof(TValue);

    private protected BaseSpecPropertyType() { }

    public bool Equals(ISpecPropertyType? other) => other is BaseSpecPropertyType<TSpecPropertyType, TValue> bs && Equals(bs);
    public bool Equals(ISpecPropertyType<TValue>? other) => other is BaseSpecPropertyType<TSpecPropertyType, TValue> bs && Equals(bs);
    
    public bool Equals(BaseSpecPropertyType<TSpecPropertyType, TValue>? other)
    {
        if (this is IEquatable<TSpecPropertyType> equatable)
        {
            return other is TSpecPropertyType ts && equatable.Equals(ts);
        }

        return other != null;
    }

    /// <inheritdoc />
    public abstract bool TryParseValue(in SpecPropertyTypeParseContext parse, out TValue? value);

    public override bool Equals(object? obj) => obj is BaseSpecPropertyType<TSpecPropertyType, TValue> bs && Equals(bs);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);

    #region Diagnostics

    protected bool MissingNode(in SpecPropertyTypeParseContext parse, out TValue? value)
    {
        if (parse.HasDiagnostics && parse.Parent is IPropertySourceNode property)
        {
            string key = property.Key;
            if (parse.Node != null && parse.Node.Index >= 0)
            {
                key += $"[{parse.Node.Index}]";
            }

            DatDiagnosticMessage message = new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1005,
                Message = string.Format(DiagnosticResources.UNT1005, key),
                Range = parse.Parent.Range
            };

            parse.Log(message);
        }

        value = default;
        return false;
    }

    protected bool MissingProperty(in SpecPropertyTypeParseContext parse, string property, out TValue? value)
    {
        if (parse.HasDiagnostics)
        {
            DatDiagnosticMessage message = new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1007,
                Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, property),
                Range = parse.Parent?.Range ?? parse.Node?.Range ?? default
            };

            parse.Log(message);
        }

        value = default;
        return false;
    }

    protected bool MissingType(in SpecPropertyTypeParseContext parse, string type, out TValue? value)
    {
        if (parse.HasDiagnostics)
        {
            DatDiagnosticMessage message = new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2005,
                Message = string.Format(DiagnosticResources.UNT2005, parse.BaseKey, type),
                Range = parse.Parent?.Range ?? parse.Node?.Range ?? default
            };

            parse.Log(message);
        }

        value = default;
        return false;
    }

    protected bool FailedToParse(in SpecPropertyTypeParseContext parse, out TValue? value, ISourceNode? node = null)
    {
        node ??= parse.Node;
        if (parse.HasDiagnostics && node != null)
        {
            DatDiagnosticMessage message = new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Message = string.Format(
                    DiagnosticResources.UNT2004,
                    parse.Node is IValueSourceNode s ? s.Value : parse.Node!.ToString(),
                    DisplayName
                ),
                Range = parse.Node.Range
            };

            parse.Log(message);
        }

        value = default;
        return false;
    }

    #endregion
}
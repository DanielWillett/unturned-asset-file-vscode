using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A weakly-typed reference to another property.
/// </summary>
public class PropertyReferenceValue : IValue
{
    private PropertyReference _pRef;

    public PropertyReferenceValue(PropertyReference pRef)
    {
        _pRef = pRef;
    }

    /// <inheritdoc />
    public bool Equals(IValue? other)
    {
        return other is PropertyReferenceValue v && _pRef.Equals(v._pRef);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return _pRef.ToString();
    }

#if !NETSTANDARD2_1_OR_GREATER && !NET5_0_OR_GREATER
    private static readonly char[] Escapables = [ '(', ')', '\\' ];
#endif

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        string str = _pRef.ToString();

#if !NETSTANDARD2_1_OR_GREATER && !NET5_0_OR_GREATER
        StringHelper.EscapeValue(ref str, Escapables);
#else
        ReadOnlySpan<char> escapables = [ '(', ')', '\\' ];
        StringHelper.EscapeValue(ref str, escapables);
#endif

        writer.WriteStringValue(StringHelper.ContainsWhitespace(str) ? $"@({str})" : $"@{str}");
    }

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor) where TVisitor : IValueVisitor => false;

    /// <inheritdoc />
    public virtual bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx) where TVisitor : IValueVisitor
    {
        IPropertyReferenceValue value = _pRef.CreateValue((DatProperty)(object)ctx.Self, ctx.Information);
        // todo: visitor.Accept(value);
        return false;
    }

    bool IValue.IsNull => false;
}

/// <summary>
/// A strongly-typed reference to another property.
/// </summary>
public class PropertyReferenceValue<TReferencedValue> : PropertyReferenceValue, IValue<TReferencedValue>
    where TReferencedValue : IEquatable<TReferencedValue>
{
    /// <inheritdoc />
    public IType<TReferencedValue> Type { get; }

    public PropertyReferenceValue(IType<TReferencedValue> type, PropertyReference pRef) : base(pRef)
    {
        Type = type;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TReferencedValue> value, in FileEvaluationContext ctx)
    {
        ValueVisitor visitor;
        visitor.Value = Optional<TReferencedValue>.Null;
        visitor.Success = false;

        VisitValue(ref visitor, in ctx);
        if (visitor.Success)
        {
            value = visitor.Value;
            return true;
        }

        value = Optional<TReferencedValue>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TReferencedValue> value)
    {
        value = Optional<TReferencedValue>.Null;
        return false;
    }

    private struct ValueVisitor : IValueVisitor
    {
        public Optional<TReferencedValue> Value;
        public bool Success;

        /// <inheritdoc />
        public void Accept<TValue>(Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            if (typeof(TValue) == typeof(TReferencedValue))
            {
                Value = Unsafe.As<Optional<TValue>, Optional<TReferencedValue>>(ref value);
                Success = true;
                return;
            }

            ConvertVisitor<TReferencedValue> converter = default;
            converter.IsNull = !value.HasValue;

            converter.Accept(value.Value);
            if (!converter.WasSuccessful)
                return;

            Value = new Optional<TReferencedValue>(converter.Result);
            Success = true;
        }
    }
}

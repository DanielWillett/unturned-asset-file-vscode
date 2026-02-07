using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A weakly-typed default/fallback case of a switch case.
/// </summary>
public class DefaultSwitchCase : ISwitchCase
{
    /// <summary>
    /// The value this case will evaluate to.
    /// </summary>
    public IValue Value { get; }

    public DefaultSwitchCase(IValue value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("Value"u8);
        Value.WriteToJson(writer, options);

        writer.WriteEndObject();
    }

    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        return Value.VisitConcreteValue(ref visitor);
    }

    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        return Value.VisitValue(ref visitor, in ctx);
    }


    /// <inheritdoc />
    public virtual bool Equals(IValue? other)
    {
        return other is DefaultSwitchCase d && Value.Equals(d.Value);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as DefaultSwitchCase);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(870799789, Value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }

    bool ISwitchCase.TryCheckConditionsConcrete(out bool doesPassConditions)
    {
        doesPassConditions = true;
        return true;
    }
    bool ISwitchCase.TryCheckConditions(in FileEvaluationContext ctx, out bool doesPassConditions)
    {
        doesPassConditions = true;
        return true;
    }
    bool IValue.IsNull => Value.IsNull;
}

/// <summary>
/// A strongly-typed default/fallback case of a switch case.
/// </summary>
public class DefaultSwitchCase<TResult> : DefaultSwitchCase, ISwitchCase<TResult>
    where TResult : IEquatable<TResult>
{
    /// <inheritdoc />
    public IType<TResult> Type { get; }

    /// <inheritdoc />
    public new IValue<TResult> Value => (IValue<TResult>)base.Value;

    public DefaultSwitchCase(IValue<TResult> value) : base(value)
    {
        Type = value.Type;
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TResult> value)
    {
        return Value.TryGetConcreteValue(out value);
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TResult> value, in FileEvaluationContext ctx)
    {
        return Value.TryEvaluateValue(out value, in ctx);
    }

    /// <inheritdoc />
    public override bool Equals(IValue? other)
    {
        return other is DefaultSwitchCase<TResult> d && Type.Equals(d.Type) && base.Equals(d);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, base.GetHashCode());
    }
}
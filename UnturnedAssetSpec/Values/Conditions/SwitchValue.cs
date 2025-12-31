using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A weakly-typed switch value that can be used alongside type switches.
/// </summary>
/// <remarks>When possible use <see cref="SwitchValue{TResult}"/> instead.</remarks>
public class SwitchValue : IValue, IEquatable<SwitchValue?>
{
    private int? _hashCode;

    /// <summary>
    /// All cases in this switch value.
    /// </summary>
    public ImmutableArray<ISwitchCase> Cases { get; }

    public SwitchValue(ImmutableArray<ISwitchCase> cases)
    {
        Cases = cases.IsDefault ? ImmutableArray<ISwitchCase>.Empty : cases;
    }

    /// <summary>
    /// Attempts to read a switch value from a JSON array.
    /// </summary>
    public static bool TryRead<TResult>(in JsonElement element, IType<TResult> type, [NotNullWhen(true)] out SwitchValue<TResult>? value)
        where TResult : IEquatable<TResult>
    {
        value = null;
        if (element.ValueKind != JsonValueKind.Array)
            return false;

        int cases = element.GetArrayLength();
        if (cases <= 0)
            return false;

        ImmutableArray<ISwitchCase<TResult>>.Builder bldr = ImmutableArray.CreateBuilder<ISwitchCase<TResult>>(cases);
        for (int i = 0; i < cases; ++i)
        {
            JsonElement obj = element[i];
            if (SwitchCase.TryReadSwitchCase(type, in obj) is not { } sc)
            {
                for (int j = 0; j < i; ++j)
                {
                    if (bldr[j] is IDisposable d)
                        d.Dispose();
                }

                return false;
            }

            bldr.Add(sc);
        }

        value = new SwitchValue<TResult>(type, bldr.MoveToImmutable());
        return true;
    }

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor) where TVisitor : IValueVisitor
    {
        foreach (ISwitchCase c in Cases)
        {
            if (!c.TryCheckConditionsConcrete(out bool doesPassConditions))
                return false;

            if (!doesPassConditions)
                continue;

            c.Value.VisitConcreteValue(ref visitor);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx) where TVisitor : IValueVisitor
    {
        foreach (ISwitchCase c in Cases)
        {
            if (!c.TryCheckConditions(in ctx, out bool doesPassConditions))
                return false;

            if (!doesPassConditions)
                continue;

            c.Value.VisitValue(ref visitor, in ctx);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (ISwitchCase c in Cases)
        {
            c.WriteToJson(writer, options);
        }
        writer.WriteEndArray();
    }

    /// <inheritdoc />
    public bool Equals(IValue? other)
    {
        return Equals(other as SwitchValue);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as SwitchValue);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        if (_hashCode.HasValue)
            return _hashCode.Value;

        HashCode hc = new HashCode();
        hc.Add(388121854);
        foreach (ISwitchCase c in Cases)
        {
            hc.Add(c);
        }

        int code = hc.ToHashCode();
        _hashCode = code;
        return code;
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }

    /// <inheritdoc />
    public virtual bool Equals(SwitchValue? other)
    {
        if (other == null)
            return false;
        if ((object)other == this)
            return true;

        ImmutableArray<ISwitchCase> otherCases = other.Cases;
        ImmutableArray<ISwitchCase> thisCases = Cases;
        if (otherCases.Length != thisCases.Length)
            return false;

        for (int i = 0; i < thisCases.Length; ++i)
        {
            if (!thisCases[i].Equals(otherCases[i]))
                return false;
        }

        return true;
    }

    bool IValue.IsNull => false;
}

/// <summary>
/// A strongly-typed switch value.
/// </summary>
/// <typeparam name="TResult">The type of value that all cases will evaluate to.</typeparam>
public class SwitchValue<TResult> : SwitchValue, IValue<TResult>, IEquatable<SwitchValue<TResult>?>
    where TResult : IEquatable<TResult>
{
    /// <summary>
    /// The type of value that all cases will evalate to.
    /// </summary>
    public IType<TResult> Type { get; }

    public SwitchValue(IType<TResult> type, ImmutableArray<ISwitchCase<TResult>> cases)
        : base(cases.UnsafeConvert<ISwitchCase<TResult>, ISwitchCase>())
    {
        Type = type;
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TResult> value)
    {
        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (ISwitchCase<TResult> c in Cases)
        {
            if (!c.TryCheckConditionsConcrete(out bool doesPassConditions))
            {
                value = Optional<TResult>.Null;
                return false;
            }

            if (!doesPassConditions)
                continue;

            return c.Value.TryGetConcreteValue(out value);
        }

        value = Optional<TResult>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TResult> value, in FileEvaluationContext ctx)
    {
        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (ISwitchCase<TResult> c in Cases)
        {
            if (!c.TryCheckConditions(in ctx, out bool doesPassConditions))
            {
                value = Optional<TResult>.Null;
                return false;
            }

            if (!doesPassConditions)
                continue;

            return c.Value.TryEvaluateValue(out value, in ctx);
        }

        value = Optional<TResult>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(SwitchValue<TResult>? other)
    {
        return (object)this == other || (other != null && Type.Equals(other.Type) && base.Equals(other));
    }

    /// <inheritdoc />
    public override bool Equals(SwitchValue? other)
    {
        if ((object?)other == this)
            return true;

        if (other is not SwitchValue<TResult> r || !Type.Equals(r.Type))
            return false;

        return base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, base.GetHashCode());
    }
}
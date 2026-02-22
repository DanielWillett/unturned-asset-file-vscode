using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

internal interface ICondition : IValue<bool>
{
    /// <summary>
    /// Creates a condition that evaluates to the exact opposite (NOT) of this condition and invokes a visitor with the result.
    /// </summary>
    void GetOpposite<TVisitor>(ref TVisitor visitor)
        where TVisitor : Conditions.IConditionVisitor;
}

/// <summary>
/// A condition between an <see cref="IValue"/> and any type of value.
/// </summary>
public readonly struct Condition<TComparand> : IEquatable<Condition<TComparand>>, ICondition
    where TComparand : IEquatable<TComparand>
{
    /// <summary>
    /// The value being compared against.
    /// </summary>
    public IValue Variable { get; }

    /// <summary>
    /// The operator being used to compare.
    /// </summary>
    public IConditionOperation Operation { get; }

    /// <summary>
    /// The value being compared to.
    /// </summary>
    public Optional<TComparand> Comparand { get; }

    /// <summary>
    /// Whether or not the result of this condition should be inverted.
    /// </summary>
    public bool IsInverted { get; }


    public Condition(IValue variable, IConditionOperation operation, Optional<TComparand> comparand, bool isInverted = false)
    {
        Variable = variable;
        Operation = operation;
        Comparand = comparand;
        IsInverted = isInverted;
    }

    /// <inheritdoc />
    public bool Equals(Condition<TComparand> other)
    {
        return Variable.Equals(other.Variable)
               && Operation.Equals(other.Operation)
               && IsInverted == other.IsInverted
               && Comparand.Equals(other.Comparand);
    }

    /// <summary>
    /// Creates a condition that evaluates to the exact opposite (NOT) of this condition.
    /// </summary>
    public Condition<TComparand> GetOpposite()
    {
        if (IsInverted)
        {
            return new Condition<TComparand>(Variable, Operation, Comparand);
        }

        IConditionOperation? inverse = Operation.Inverse;
        if (inverse != null)
        {
            return new Condition<TComparand>(Variable, inverse, Comparand);
        }

        return new Condition<TComparand>(Variable, Operation, Comparand, isInverted: true);
    }

    /// <summary>
    /// Creates a condition that evaluates to the exact opposite (NOT) of this condition and invokes a visitor with the result.
    /// </summary>
    public void GetOpposite<TVisitor>(ref TVisitor visitor)
        where TVisitor : Conditions.IConditionVisitor
    {
        Condition<TComparand> opposite = GetOpposite();
        visitor.Accept(in opposite);
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (Operation == null || Variable == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();
        
        writer.WritePropertyName("Variable"u8);
        Variable.WriteToJson(writer, options);

        writer.WriteString("Operation"u8, Operation.Name);

        writer.WritePropertyName("Comparand"u8);
        JsonHelper.WriteGenericValue(writer, Comparand);

        if (IsInverted)
        {
            writer.WriteBoolean("Inverted"u8, true);
        }

        writer.WriteEndObject();
    }

    public static bool operator ==(in Condition<TComparand> left, in Condition<TComparand> right) => left.Equals(right);
    public static bool operator !=(in Condition<TComparand> left, in Condition<TComparand> right) => !left.Equals(right);

    /// <inheritdoc />
    public bool Equals(IValue? other)
    {
        return other is Condition<TComparand> c && Equals(c);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Condition<TComparand> c && Equals(c);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(808528140, Variable, Operation, Comparand, IsInverted);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{(IsInverted ? "!" : string.Empty)}(<{Variable}> {Operation.Symbol} {(!Comparand.HasValue ? "=NULL" : $"%({Comparand.Value})")})";
    }

    /// <inheritdoc />
    public unsafe bool TryGetConcreteValue(out Optional<bool> value)
    {
        EvaluateVisitor v;
        v.Visited = false;
        v.IsConditionMet = false;
        v.ConcreteOnly = true;
        v.Context = null;
        bool success;
        fixed (Condition<TComparand>* ptr = &this)
        {
            v.Condition = ptr;
            success = Variable.VisitConcreteValue(ref v);
        }

        if (!success || !v.Visited)
        {
            value = Optional<bool>.Null;
            return false;
        }

        value = IsInverted ^ v.IsConditionMet;
        return true;
    }

    /// <inheritdoc />
    public unsafe bool TryEvaluateValue(out Optional<bool> value, in FileEvaluationContext ctx)
    {
        EvaluateVisitor v;
        v.Visited = false;
        v.IsConditionMet = false;
        v.ConcreteOnly = false;
        bool success;
        fixed (Condition<TComparand>* ptr = &this)
        fixed (FileEvaluationContext* ctxPtr = &ctx)
        {
            v.Condition = ptr;
            v.Context = ctxPtr;
            success = Variable.VisitValue(ref v, in ctx);
        }

        if (!success || !v.Visited)
        {
            value = Optional<bool>.Null;
            return false;
        }

        value = IsInverted ^ v.IsConditionMet;
        return true;
    }

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (!TryGetConcreteValue(out Optional<bool> v) || !v.HasValue)
            return false;

        visitor.Accept(v);
        return true;
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (!TryEvaluateValue(out Optional<bool> v, in ctx) || !v.HasValue)
            return false;

        visitor.Accept(v);
        return true;
    }

    private unsafe struct EvaluateVisitor : IValueVisitor
    {
        public bool Visited;
        public bool IsConditionMet;
        public bool ConcreteOnly;
        public FileEvaluationContext* Context;
        public Condition<TComparand>* Condition;

        public void Accept<TValue>(Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            Optional<TComparand> comparand = Condition->Comparand;
            if (!value.HasValue)
            {
                Visited = true;
                IsConditionMet = Condition->Operation.EvaluateNullValues(true, !comparand.HasValue);
                return;
            }

            if (!comparand.HasValue)
            {
                Visited = true;
                IsConditionMet = Condition->Operation.EvaluateNullValues(false, true);
                return;
            }

            Visited = Condition->Operation.TryEvaluate(
                value.Value,
                comparand.Value,
                ConcreteOnly,
                in Unsafe.AsRef<FileEvaluationContext>(Context),
                out IsConditionMet
            );
        }
    }

    bool IValue.IsNull => false;
    IType<bool> IValue<bool>.Type => Conditions.Type;
}


/// <summary>
/// Extensions for the <see cref="Condition{TComparand}"/> type.
/// </summary>
public static class Conditions
{
    /// <summary>
    /// The type of value represented by <see cref="Condition{TComparand}"/> values.
    /// </summary>
    public static IType<bool> Type => BooleanType.Instance;

    /// <summary>
    /// A condition that always evaluates to <see langword="true"/>.
    /// </summary>
    public static Condition<bool> True { get; } = new Condition<bool>(Value.True, Operations.Equal.Instance, new Optional<bool>(true));

    /// <summary>
    /// A condition that always evaluates to <see langword="false"/>.
    /// </summary>
    public static Condition<bool> False { get; } = new Condition<bool>(Value.True, Operations.Equal.Instance, new Optional<bool>(false));

    /// <summary>
    /// Attempt to read a condition from a JSON object and return it as a boolean value.
    /// </summary>
    /// <remarks>Some values returned may not be a <see cref="Condition{TComparand}"/> object. For example, a boolean token results in a <see cref="BooleanType"/> value.</remarks>
    public static bool TryReadConditionFromJson<TDataRefReadContext>(
        in JsonElement root,
        IAssetSpecDatabase database,
        IDatSpecificationObject owner,
        [NotNullWhen(true)] out IValue<bool>? condition,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        switch (root.ValueKind)
        {
            case JsonValueKind.True:
                condition = Value.True;
                return true;

            case JsonValueKind.False:
                condition = Value.False;
                return true;

            case JsonValueKind.Null:
                condition = Value.Null(BooleanType.Instance);
                return true;
        }

        BoxConditionVisitor visitor;
        visitor.Condition = null;
        if (!TryReadConditionFromJson(in root, database, owner, ref visitor, ref dataRefContext))
        {
            condition = null;
            return false;
        }

        condition = visitor.Condition!;
        return true;
    }

    private struct BoxConditionVisitor : IConditionVisitor
    {
        public ICondition? Condition;
        public void Accept<TComparand>(in Condition<TComparand> condition) where TComparand : IEquatable<TComparand>
        {
            Condition = condition;
        }
    }

    /// <summary>
    /// Attempt to read a condition from a JSON object and report it to a <see cref="IConditionVisitor"/>.
    /// </summary>
    public static unsafe bool TryReadConditionFromJson<TVisitor, TDataRefReadContext>(
        in JsonElement root,
        IAssetSpecDatabase database,
        IDatSpecificationObject owner,
        ref TVisitor visitor,
        ref TDataRefReadContext dataRefContext
    ) where TVisitor : IConditionVisitor
      where TDataRefReadContext : IDataRefReadContext?
    {
        switch (root.ValueKind)
        {
            case JsonValueKind.True:
                visitor.Accept(True);
                return true;

            case JsonValueKind.False:
                visitor.Accept(False);
                return true;

            case JsonValueKind.Object:
                break;

            default:
                return false;
        }

        if (root.TryGetProperty("$udat-type"u8, out _))
        {
            return false;
        }

        if (!root.TryGetProperty("Operation"u8, out JsonElement element)
            || element.ValueKind != JsonValueKind.String
            || !ConditionOperations.TryGetOperation(element.GetString(), out IConditionOperation? operation))
        {
            return false;
        }

        bool inverted = root.TryGetProperty("Inverted"u8, out element) && element.ValueKind == JsonValueKind.True;

        if (!root.TryGetProperty("Variable"u8, out element)
            || element.ValueKind is not (JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False))
        {
            return false;
        }

        ValueVisitor<TVisitor> valueVisitor;
        valueVisitor.DidFullyParseCondition = false;
        valueVisitor.Operation = operation;
        valueVisitor.Inverted = inverted;
        IValue? variable;
        fixed (JsonElement* elementPtr = &root)
        fixed (TVisitor* visitorPtr = &visitor)
        {
            valueVisitor.Visitor = visitorPtr;
            valueVisitor.Element = elementPtr;
            variable = Value.TryReadValueFromJson(element, ValueReadOptions.AssumeProperty | ValueReadOptions.Default, ref valueVisitor, valueType: null, database, owner, ref dataRefContext);
            if (variable == null && element.ValueKind == JsonValueKind.String && element.GetString() is [ '=', .. ])
            {
                // guess value type from comparand for expressions, since they require a target type
                if (!root.TryGetProperty("Comparand"u8, out element))
                {
                    return false;
                }

                IType? type = element.ValueKind switch
                {
                    JsonValueKind.Null => StringType.Instance,
                    JsonValueKind.True or JsonValueKind.False => BooleanType.Instance,
                    JsonValueKind.Number => element.TryGetInt32(out _)
                        ? Int32Type.Instance
                        : element.TryGetInt64(out _)
                            ? Int64Type.Instance
                            : element.TryGetDouble(out _)
                                ? Float64Type.Instance
                                : Float128Type.Instance,
                    JsonValueKind.String => StringType.Instance,
                    _ => null
                };

                if (type != null)
                {
                    variable = Value.TryReadValueFromJson(element, ValueReadOptions.AssumeProperty | ValueReadOptions.Default, ref valueVisitor, valueType: type, database, owner, ref dataRefContext);
                }
            }

            if (variable == null)
            {
                return false;
            }

            if (valueVisitor.DidFullyParseCondition)
                return true;
        }

        if (!root.TryGetProperty("Comparand"u8, out element))
        {
            return false;
        }

        ComparandVisitor<TVisitor> v;
        v.Operation = operation;
        v.Inverted = inverted;
        v.Variable = variable;
        v.DidReportValue = false;
        v.DidFullyParseCondition = false;
        fixed (TVisitor* visitorPtr = &visitor)
        {
            v.Visitor = visitorPtr;
            IValue? comparand = Value.TryReadValueFromJson(element, ValueReadOptions.AssumeValue, ref v, null, database, owner, ref dataRefContext);
            if (comparand == null)
                return false;

            if (!v.DidReportValue)
                comparand.VisitConcreteValue(ref v);
            
            return v.DidFullyParseCondition;
        }
    }

    private unsafe struct ValueVisitor<TVisitor> : Value.IReadValueVisitor
        where TVisitor : IConditionVisitor
    {
        public TVisitor* Visitor;
        public JsonElement* Element;
        public bool DidFullyParseCondition;
        public IConditionOperation Operation;
        public bool Inverted;

        /// <inheritdoc />
        public void Accept<TValue>(IValue<TValue> value) where TValue : IEquatable<TValue>
        {
            if (!Element->TryGetProperty("Comparand"u8, out JsonElement comparandProperty))
            {
                return;
            }

            if (JsonHelper.TryReadGenericValue<TValue>(in comparandProperty, out Optional<TValue> comparand))
            {
                DidFullyParseCondition = true;
                Visitor->Accept(new Condition<TValue>(value, Operation, comparand, Inverted));
            }
        }
    }

    private unsafe struct ComparandVisitor<TVisitor> : Value.IReadValueVisitor, IValueVisitor
        where TVisitor : IConditionVisitor
    {
        public TVisitor* Visitor;
        public IConditionOperation Operation;
        public bool Inverted;
        public IValue Variable;
        public bool DidReportValue;
        public bool DidFullyParseCondition;

        public void Accept<TValue>(IValue<TValue> value) where TValue : IEquatable<TValue>
        {
            DidReportValue = true;
            value.VisitConcreteValue(ref this);
        }

        public void Accept<TValue>(Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            DidFullyParseCondition = true;
            Visitor->Accept(new Condition<TValue>(Variable, Operation, value, Inverted));
        }
    }

    /// <inheritdoc cref="TryReadComplexOrBasicConditionFromJson{TDataRefReadContext}"/>
    public static bool TryReadComplexOrBasicConditionFromJson(
        in JsonElement root,
        IAssetSpecDatabase database,
        IDatSpecificationObject owner,
        [NotNullWhen(true)] out IValue<bool>? condition
    )
    {
        DataRefs.NilDataRefContext c;
        return TryReadComplexOrBasicConditionFromJson(in root, database, owner, out condition, ref c);
    }

    /// <summary>
    /// Attempts to read either a <see cref="Condition{TComparand}"/> or a <see cref="ComplexConditionalValue"/> from a JSON object.
    /// </summary>
    /// <returns>Whether or not the value could be read.</returns>
    public static bool TryReadComplexOrBasicConditionFromJson<TDataRefReadContext>(
        in JsonElement root,
        IAssetSpecDatabase database,
        IDatSpecificationObject owner,
        [NotNullWhen(true)] out IValue<bool>? condition,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        condition = null;
        if (TryReadConditionFromJson(in root, database, owner, out IValue<bool>? cond, ref dataRefContext))
        {
            condition = cond;
            return true;
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (root.TryGetProperty("$udat-type"u8, out _))
        {
            return false;
        }

        if (root.TryGetProperty("Case"u8, out JsonElement element))
        {
            if (!TryReadComplexOrBasicConditionFromJson(in element, database, owner, out cond, ref dataRefContext))
                return false;

            condition = new ComplexConditionalValue(ImmutableArray.Create(cond), JointConditionOperation.Or);
            return true;
        }

        bool isAnd = root.TryGetProperty("And"u8, out element);

        if (!isAnd && !root.TryGetProperty("Or"u8, out element))
            return false;

        int cases = element.GetArrayLength();
        if (cases <= 0)
            return false;

        ImmutableArray<IValue<bool>>.Builder bldr = ImmutableArray.CreateBuilder<IValue<bool>>(cases);
        for (int i = 0; i < cases; ++i)
        {
            JsonElement item = element[i];
            if (!TryReadComplexOrBasicConditionFromJson(in item, database, owner, out cond, ref dataRefContext))
                return false;

            bldr.Add(cond);
        }

        condition = new ComplexConditionalValue(
            bldr.MoveToImmutable(),
            !isAnd ? JointConditionOperation.Or : JointConditionOperation.And
        );
        return true;
    }

    /// <summary>
    /// Creates a condition that evaluates to the exact opposite (NOT) of this condition.
    /// </summary>
    internal static ICondition GetOpposite(this ICondition condition)
    {
        BoxConditionVisitor box;
        box.Condition = null;
        condition.GetOpposite(ref box);
        return box.Condition!;
    }


    /// <summary>
    /// A visitor that accepts the result of <see cref="Conditions.TryReadConditionFromJson{TVisitor}"/>.
    /// </summary>
    public interface IConditionVisitor
    {
        /// <summary>
        /// Invoked by <see cref="Conditions.TryReadConditionFromJson{TVisitor}"/> to accept the parsed condition.
        /// </summary>
        void Accept<TComparand>(in Condition<TComparand> condition) where TComparand : IEquatable<TComparand>;
    }
}
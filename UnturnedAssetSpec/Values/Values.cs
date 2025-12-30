using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Utilities for working with the <see cref="IValue"/> system.
/// </summary>
public static class Values
{
    /// <summary>
    /// A <see langword="null"/> value of a given <paramref name="type"/>.
    /// </summary>
    public static IValue Null(IType type)
    {
        NullValueVisitor v;
        v.Value = null;
        type.Visit(ref v);
        return v.Value ?? new NullValue(type);
    }

    /// <summary>
    /// A <see langword="null"/> value of a given <paramref name="type"/>.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static NullValue<T> Null<T>(IType<T> type) where T : IEquatable<T>
    {
        if (typeof(T) == typeof(long))
        {
            if (ReferenceEquals(type, Int64Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<long>, NullValue<T>>(Int64Type.Null);
        }
        else if (typeof(T) == typeof(ulong))
        {
            if (ReferenceEquals(type, UInt64Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<ulong>, NullValue<T>>(UInt64Type.Null);
        }
        else if (typeof(T) == typeof(int))
        {
            if (ReferenceEquals(type, Int32Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<int>, NullValue<T>>(Int32Type.Null);
        }
        else if (typeof(T) == typeof(uint))
        {
            if (ReferenceEquals(type, UInt32Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<uint>, NullValue<T>>(UInt32Type.Null);
        }
        else if (typeof(T) == typeof(short))
        {
            if (ReferenceEquals(type, Int16Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<short>, NullValue<T>>(Int16Type.Null);
        }
        else if (typeof(T) == typeof(ushort))
        {
            if (ReferenceEquals(type, UInt16Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<ushort>, NullValue<T>>(UInt16Type.Null);
        }
        else if (typeof(T) == typeof(sbyte))
        {
            if (ReferenceEquals(type, Int8Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<sbyte>, NullValue<T>>(Int8Type.Null);
        }
        else if (typeof(T) == typeof(byte))
        {
            if (ReferenceEquals(type, UInt8Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<byte>, NullValue<T>>(UInt8Type.Null);
        }
        else if (typeof(T) == typeof(float))
        {
            if (ReferenceEquals(type, Float32Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<float>, NullValue<T>>(Float32Type.Null);
        }
        else if (typeof(T) == typeof(double))
        {
            if (ReferenceEquals(type, Float64Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<double>, NullValue<T>>(Float64Type.Null);
        }
        else if (typeof(T) == typeof(decimal))
        {
            if (ReferenceEquals(type, Float128Type.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<decimal>, NullValue<T>>(Float128Type.Null);
        }
        else if (typeof(T) == typeof(string))
        {
            if (ReferenceEquals(type, StringType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<string>, NullValue<T>>(StringType.Null);
            if (ReferenceEquals(type, RegexStringType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<string>, NullValue<T>>(RegexStringType.Null);
        }
        else if (typeof(T) == typeof(bool))
        {
            if (ReferenceEquals(type, BooleanType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<bool>, NullValue<T>>(BooleanType.Null);
            if (ReferenceEquals(type, FlagType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<bool>, NullValue<T>>(FlagType.Null);
            if (ReferenceEquals(type, BooleanOrFlagType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<bool>, NullValue<T>>(BooleanOrFlagType.Null);
        }
        else if (typeof(T) == typeof(char))
        {
            if (ReferenceEquals(type, CharacterType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<char>, NullValue<T>>(CharacterType.Null);
        }
        else if (typeof(T) == typeof(DateTime))
        {
            if (ReferenceEquals(type, DateTimeType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<DateTime>, NullValue<T>>(DateTimeType.Null);
        }
        else if (typeof(T) == typeof(DateTimeOffset))
        {
            if (ReferenceEquals(type, DateTimeOffsetType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<DateTimeOffset>, NullValue<T>>(DateTimeOffsetType.Null);
        }
        else if (typeof(T) == typeof(IPv4Filter))
        {
            if (ReferenceEquals(type, IPv4FilterType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<IPv4Filter>, NullValue<T>>(IPv4FilterType.Null);
        }
        else if (typeof(T) == typeof(TimeSpan))
        {
            if (ReferenceEquals(type, TimeSpanType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<TimeSpan>, NullValue<T>>(TimeSpanType.Null);
        }
        else if (typeof(T) == typeof(GuidOrId))
        {
            if (ReferenceEquals(type, GuidOrIdType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<GuidOrId>, NullValue<T>>(GuidOrIdType.Null);
        }
        else if (typeof(T) == typeof(Guid))
        {
            if (ReferenceEquals(type, GuidType.Instance))
                return SpecDynamicExpressionTreeValueHelpers.As<NullValue<Guid>, NullValue<T>>(GuidType.Null);
        }

        return new NullValue<T>(type);
    }

    /// <summary>
    /// A <see langword="null"/> value of a given <paramref name="type"/>.
    /// </summary>
    [OverloadResolutionPriority(2)]
    public static NullValue<T> Null<TType, T>(PrimitiveType<T, TType> type) where TType : PrimitiveType<T, TType>, new() where T : IEquatable<T>
    {
        return PrimitiveType<T, TType>.Null;
    }

    /// <summary>
    /// The <see langword="true"/> concrete value of type <see cref="BooleanType"/>.
    /// </summary>
    public static ConcreteValue<bool> True { get; } = new ConcreteValue<bool>(true, BooleanType.Instance);

    /// <summary>
    /// The <see langword="false"/> concrete value of type <see cref="BooleanType"/>.
    /// </summary>
    public static ConcreteValue<bool> False { get; } = new ConcreteValue<bool>(false, BooleanType.Instance);

    /// <summary>
    /// The <see langword="true"/> concrete value of type <see cref="FlagType"/>.
    /// </summary>
    public static ConcreteValue<bool> Included { get; } = new ConcreteValue<bool>(true, FlagType.Instance);

    /// <summary>
    /// The <see langword="false"/> concrete value of type <see cref="FlagType"/>.
    /// </summary>
    public static ConcreteValue<bool> Excluded { get; } = new ConcreteValue<bool>(false, FlagType.Instance);

    /// <summary>
    /// A value of type <see cref="FlagType"/>.
    /// </summary>
    public static ConcreteValue<bool> Flag(bool v) => v ? Included : Excluded;

    /// <summary>
    /// A value of type <see cref="BooleanType"/>.
    /// </summary>
    public static ConcreteValue<bool> Boolean(bool v) => v ? True : False;

    /// <summary>
    /// A boolean type which can be of type <see cref="KnownTypes.Flag"/>, <see cref="KnownTypes.Boolean"/>, or some other boolean type.
    /// </summary>
    public static ConcreteValue<bool> Boolean(bool v, IType<bool>? type)
    {
        if ((object?)type == FlagType.Instance)
        {
            return Flag(v);
        }

        if (type == null || (object?)type == BooleanType.Instance)
        {
            return Boolean(v);
        }

        return new ConcreteValue<bool>(v, type);
    }

    /// <summary>
    /// Creates a concrete value of a generic type.
    /// </summary>
    public static ConcreteValue<TValue> Create<TValue>(TValue v, IType<TValue> type) where TValue : IEquatable<TValue>
    {
        if (typeof(TValue) == typeof(bool))
        {
            if (ReferenceEquals(type, BooleanType.Instance))
            {
                return SpecDynamicExpressionTreeValueHelpers.As<ConcreteValue<bool>, ConcreteValue<TValue>>(
                    Boolean(SpecDynamicExpressionTreeValueHelpers.As<TValue, bool>(v))
                );
            }
            if (ReferenceEquals(type, FlagType.Instance))
            {
                return SpecDynamicExpressionTreeValueHelpers.As<ConcreteValue<bool>, ConcreteValue<TValue>>(
                    Flag(SpecDynamicExpressionTreeValueHelpers.As<TValue, bool>(v))
                );
            }
        }

        return new ConcreteValue<TValue>(v, type);
    }

    /// <summary>
    /// Creates a new expression value from an expression string.
    /// </summary>
    /// <returns>
    /// An instance of <see cref="ExpressionValue{TResult}"/>,
    /// unless the expression is a constant expression and <paramref name="simplifyConstantExpressions"/> is <see langword="true"/>,
    /// then a <see cref="ConcreteValue{TValue}"/> is returned instead.
    /// </returns>
    /// <typeparam name="TResult">
    /// The type of value to create.
    /// If the expression can be simplified it will be simplified into a value of this type, otherwise the expression will return this type when evaluated.
    /// </typeparam>
    public static IValue<TResult> FromExpression<TResult>(IType<TResult> resultType, string expression, bool simplifyConstantExpressions = true)
        where TResult : IEquatable<TResult>
    {
        // trim '='
        if (expression.Length > 0 && expression[0] == '=')
        {
            return FromExpression(resultType, expression.AsSpan(1));
        }

        IExpressionNode rootNode;
        using (ExpressionNodeParser parser = new ExpressionNodeParser(expression, simplifyConstantExpressions))
        {
            rootNode = parser.Parse<TResult>();
        }

        if (simplifyConstantExpressions && rootNode is IValue<TResult> value)
        {
            return value;
        }

        return new ExpressionValue<TResult>(resultType, (IFunctionExpressionNode)rootNode);
    }

    /// <inheritdoc cref="FromExpression{TResult}(IType{TResult},string,bool)"/>
    public static IValue<TResult> FromExpression<TResult>(IType<TResult> resultType, ReadOnlySpan<char> expression, bool simplifyConstantExpressions = true)
        where TResult : IEquatable<TResult>
    {
        // trim '='
        if (!expression.IsEmpty && expression[0] == '=')
            expression = expression.Slice(1);

        IExpressionNode rootNode;
        using (ExpressionNodeParser parser = new ExpressionNodeParser(expression, simplifyConstantExpressions))
        {
            rootNode = parser.Parse<TResult>();
        }

        if (simplifyConstantExpressions && rootNode is IValue<TResult> value)
        {
            return value;
        }

        return new ExpressionValue<TResult>(resultType, (IFunctionExpressionNode)rootNode);
    }

    private struct NullValueVisitor : ITypeVisitor
    {
        public IValue? Value;
        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            Value = new NullValue<TValue>(type);
        }
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

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

    /// <summary>
    /// Parse a value from a JSON token.
    /// </summary>
    /// <returns>The parsed value, or <see langword="null"/> if the value couldn't be parsed.</returns>
    public static IValue? TryReadValueFromJson(in JsonElement root, SpecDynamicValueContext context, IPropertyType? valueType)
    {
        NullReadValueVisitor v;
        return TryReadValueFromJson(in root, context, ref v, valueType);
    }

    /// <summary>
    /// Parse a value from a JSON token. The visitor will only be invoked if the value is strongly typed.
    /// </summary>
    /// <returns>The parsed value, or <see langword="null"/> if the value couldn't be parsed.</returns>
    public static unsafe IValue? TryReadValueFromJson<TVisitor>(in JsonElement root, SpecDynamicValueContext context, ref TVisitor visitor, IPropertyType? valueType)
        where TVisitor : IReadValueVisitor
    {
        IType? type = valueType as IType;
        switch (root.ValueKind)
        {
            case JsonValueKind.Number:
                if (type != null)
                {
                    ReadTypedNumberVisitor<TVisitor> v;
                    v.Success = false;
                    v.Value = null;
                    fixed (TVisitor* ptr = &visitor)
                    fixed (JsonElement* elementPtr = &root)
                    {
                        v.Visitor = ptr;
                        v.Element = elementPtr;
                        type.Visit(ref v);
                        if (v.Success)
                            return v.Value;
                    }
                }

                if (root.TryGetInt64(out long i8))
                {
                    IValue<long> v = Create(i8, Int64Type.Instance);
                    visitor.Accept(v);
                    return v;
                }
                if (root.TryGetUInt64(out ulong u8))
                {
                    IValue<ulong> v = Create(u8, UInt64Type.Instance);
                    visitor.Accept(v);
                    return v;
                }
                if (root.TryGetDouble(out double r8))
                {
                    IValue<double> v = Create(r8, Float64Type.Instance);
                    visitor.Accept(v);
                    return v;
                }

                return null;

            case JsonValueKind.String:
                if (type != null)
                {
                    ReadTypedStringVisitor<TVisitor> v;
                    v.Success = false;
                    v.Value = null;
                    fixed (TVisitor* ptr = &visitor)
                    fixed (JsonElement* elementPtr = &root)
                    {
                        v.Visitor = ptr;
                        v.Element = elementPtr;
                        type.Visit(ref v);
                        if (v.Success)
                            return v.Value;
                    }
                }

                string str = root.GetString();
                IValue value;
                if (Guid.TryParse(str, out Guid guid))
                {
                    IValue<Guid> v = Create(guid, GuidType.Instance);
                    visitor.Accept(v);
                    return v;
                }
                if (TimeSpan.TryParse(str, CultureInfo.InvariantCulture, out TimeSpan ts))
                {
                    IValue<TimeSpan> v = Create(ts, TimeSpanType.Instance);
                    visitor.Accept(v);
                    return v;
                }
                if (DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dt))
                {
                    IValue<DateTime> v = Create(dt, DateTimeType.Instance);
                    visitor.Accept(v);
                    return v;
                }
                if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset dto))
                {
                    IValue<DateTimeOffset> v = Create(dto, DateTimeOffsetType.Instance);
                    visitor.Accept(v);
                    return v;
                }
                IValue<string> strVal = Create(str, StringType.Instance);
                visitor.Accept(strVal);
                return strVal;

            case JsonValueKind.True:
                IValue<bool> boolVal = Create(true, type as IType<bool> ?? BooleanType.Instance);
                visitor.Accept(boolVal);
                return boolVal;

            case JsonValueKind.False:
                boolVal = Create(true, type as IType<bool> ?? BooleanType.Instance);
                visitor.Accept(boolVal);
                return boolVal;

            // todo
            case JsonValueKind.Array:
                break;
        }

        return null;
    }

    private unsafe struct ReadTypedNumberVisitor<TVisitor> : ITypeVisitor
        where TVisitor : IReadValueVisitor
    {
        public bool Success;
        public IValue? Value;
        public TVisitor* Visitor;
        public JsonElement* Element;

        [SkipLocalsInit]
        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            Success = false;
            if (typeof(TValue) == typeof(decimal))
            {
                if (Element->TryGetDecimal(out decimal r16))
                {
                    IValue<decimal> value = Create(r16, Unsafe.As<IType<TValue>, IType<decimal>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(double))
            {
                if (Element->TryGetDouble(out double r8))
                {
                    IValue<double> value = Create(r8, Unsafe.As<IType<TValue>, IType<double>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(float))
            {
                if (Element->TryGetSingle(out float r4))
                {
                    IValue<float> value = Create(r4, Unsafe.As<IType<TValue>, IType<float>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(long))
            {
                if (Element->TryGetInt64(out long i8))
                {
                    IValue<long> value = Create(i8, Unsafe.As<IType<TValue>, IType<long>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(ulong))
            {
                if (Element->TryGetUInt64(out ulong u8))
                {
                    IValue<ulong> value = Create(u8, Unsafe.As<IType<TValue>, IType<ulong>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(int))
            {
                if (Element->TryGetInt32(out int i4))
                {
                    IValue<int> value = Create(i4, Unsafe.As<IType<TValue>, IType<int>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(uint))
            {
                if (Element->TryGetUInt32(out uint u4))
                {
                    IValue<uint> value = Create(u4, Unsafe.As<IType<TValue>, IType<uint>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(short))
            {
                if (Element->TryGetInt16(out short i2))
                {
                    IValue<short> value = Create(i2, Unsafe.As<IType<TValue>, IType<short>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(ushort))
            {
                if (Element->TryGetUInt16(out ushort u2))
                {
                    IValue<ushort> value = Create(u2, Unsafe.As<IType<TValue>, IType<ushort>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(GuidOrId))
            {
                if (Element->TryGetUInt16(out ushort u2))
                {
                    IValue<GuidOrId> value = Create(new GuidOrId(u2), Unsafe.As<IType<TValue>, IType<GuidOrId>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(sbyte))
            {
                if (Element->TryGetSByte(out sbyte i1))
                {
                    IValue<sbyte> value = Create(i1, Unsafe.As<IType<TValue>, IType<sbyte>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(byte))
            {
                if (Element->TryGetByte(out byte u1))
                {
                    IValue<byte> value = Create(u1, Unsafe.As<IType<TValue>, IType<byte>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(bool))
            {
                if (Element->TryGetDouble(out double r8))
                {
                    IValue<bool> value = Create(r8 != 0, Unsafe.As<IType<TValue>, IType<bool>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (typeof(TValue) == typeof(char))
            {
                if (Element->TryGetByte(out byte i1) && i1 < 10)
                {
                    IValue<char> value = Create((char)(i1 + '0'), Unsafe.As<IType<TValue>, IType<char>>(ref type));
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
            else if (VectorTypes.TryGetProvider<TValue>() is { } vectorProvider)
            {
                if (Element->TryGetDouble(out double r8))
                {
                    IValue<TValue> value = Create(vectorProvider.Construct(r8), type);
                    Value = value;
                    Visitor->Accept(value);
                    Success = true;
                }
            }
        }
    }

    private unsafe struct ReadTypedStringVisitor<TVisitor> : ITypeVisitor
        where TVisitor : IReadValueVisitor
    {
        public bool Success;
        public IValue? Value;
        public TVisitor* Visitor;
        public JsonElement* Element;

        [SkipLocalsInit]
        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            Success = false;
            if (typeof(TValue) == typeof(string))
            {
                string str = Element->GetString();
                IValue<string> v = Create(str, Unsafe.As<IType<TValue>, IType<string>>(ref type));
                Value = v;
                Visitor->Accept(v);
                Success = true;
                return;
            }

            if (type.Parser.TryReadValueFromJson(in Unsafe.AsRef<JsonElement>(Element), out Optional<TValue> value, type))
            {
                IValue<TValue> v = !value.HasValue ? Null(type) : Create(value.Value, type);
                Value = v;
                Visitor->Accept<TValue>(v);
                Success = true;
                return;
            }

            if (TypeConverters.TryGet<TValue>() is { } converter)
            {
                TypeConverterParseArgs<TValue> args = default;
                args.Type = type;
                if (converter.TryReadJson(in Unsafe.AsRef<JsonElement>(Element), out value, ref args))
                {
                    IValue<TValue> v = !value.HasValue ? Null(type) : Create(value.Value, type);
                    Value = v;
                    Visitor->Accept<TValue>(v);
                    Success = true;
                }
                else if (converter.TryParse(Element->GetString(), ref args, out TValue? val))
                {
                    IValue<TValue> v = val == null ? Null(type) : Create(val, type);
                    Value = v;
                    Visitor->Accept<TValue>(v);
                    Success = true;
                }
            }
        }
    }

    private struct NullReadValueVisitor : IReadValueVisitor
    {
        public void Accept<TValue>(IValue<TValue> value) where TValue : IEquatable<TValue> { }
    }

    /// <summary>
    /// A visitor that accepts the result of <see cref="Values.TryReadValueFromJson{TVisitor}"/>.
    /// </summary>
    public interface IReadValueVisitor
    {
        /// <summary>
        /// Invoked by <see cref="Values.TryReadValueFromJson{TVisitor}"/> to accept the parsed value as a strongly typed value.
        /// </summary>
        void Accept<TValue>(IValue<TValue> value) where TValue : IEquatable<TValue>;
    }
}
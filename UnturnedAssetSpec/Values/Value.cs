using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Utilities for working with the <see cref="IValue"/> system.
/// </summary>
public static class Value
{
    /// <summary>
    /// A <see langword="null"/> value of a given <paramref name="type"/>.
    /// </summary>
    public static IValue Null(IType type)
    {
        NullValueVisitor v;
        v.Value = null;
        type.Visit(ref v);
        return v.Value ?? NullValue.Instance;
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
                return MathMatrix.As<NullValue<long>, NullValue<T>>(Int64Type.Null);
        }
        else if (typeof(T) == typeof(ulong))
        {
            if (ReferenceEquals(type, UInt64Type.Instance))
                return MathMatrix.As<NullValue<ulong>, NullValue<T>>(UInt64Type.Null);
        }
        else if (typeof(T) == typeof(int))
        {
            if (ReferenceEquals(type, Int32Type.Instance))
                return MathMatrix.As<NullValue<int>, NullValue<T>>(Int32Type.Null);
        }
        else if (typeof(T) == typeof(uint))
        {
            if (ReferenceEquals(type, UInt32Type.Instance))
                return MathMatrix.As<NullValue<uint>, NullValue<T>>(UInt32Type.Null);
        }
        else if (typeof(T) == typeof(short))
        {
            if (ReferenceEquals(type, Int16Type.Instance))
                return MathMatrix.As<NullValue<short>, NullValue<T>>(Int16Type.Null);
        }
        else if (typeof(T) == typeof(ushort))
        {
            if (ReferenceEquals(type, UInt16Type.Instance))
                return MathMatrix.As<NullValue<ushort>, NullValue<T>>(UInt16Type.Null);
        }
        else if (typeof(T) == typeof(sbyte))
        {
            if (ReferenceEquals(type, Int8Type.Instance))
                return MathMatrix.As<NullValue<sbyte>, NullValue<T>>(Int8Type.Null);
        }
        else if (typeof(T) == typeof(byte))
        {
            if (ReferenceEquals(type, UInt8Type.Instance))
                return MathMatrix.As<NullValue<byte>, NullValue<T>>(UInt8Type.Null);
        }
        else if (typeof(T) == typeof(float))
        {
            if (ReferenceEquals(type, Float32Type.Instance))
                return MathMatrix.As<NullValue<float>, NullValue<T>>(Float32Type.Null);
        }
        else if (typeof(T) == typeof(double))
        {
            if (ReferenceEquals(type, Float64Type.Instance))
                return MathMatrix.As<NullValue<double>, NullValue<T>>(Float64Type.Null);
        }
        else if (typeof(T) == typeof(decimal))
        {
            if (ReferenceEquals(type, Float128Type.Instance))
                return MathMatrix.As<NullValue<decimal>, NullValue<T>>(Float128Type.Null);
        }
        else if (typeof(T) == typeof(string))
        {
            if (ReferenceEquals(type, StringType.Instance))
                return MathMatrix.As<NullValue<string>, NullValue<T>>(StringType.Null);
            if (ReferenceEquals(type, RegexStringType.Instance))
                return MathMatrix.As<NullValue<string>, NullValue<T>>(RegexStringType.Null);
        }
        else if (typeof(T) == typeof(bool))
        {
            if (ReferenceEquals(type, BooleanType.Instance))
                return MathMatrix.As<NullValue<bool>, NullValue<T>>(BooleanType.Null);
            if (ReferenceEquals(type, FlagType.Instance))
                return MathMatrix.As<NullValue<bool>, NullValue<T>>(FlagType.Null);
            if (ReferenceEquals(type, BooleanOrFlagType.Instance))
                return MathMatrix.As<NullValue<bool>, NullValue<T>>(BooleanOrFlagType.Null);
        }
        else if (typeof(T) == typeof(char))
        {
            if (ReferenceEquals(type, CharacterType.Instance))
                return MathMatrix.As<NullValue<char>, NullValue<T>>(CharacterType.Null);
        }
        else if (typeof(T) == typeof(DateTime))
        {
            if (ReferenceEquals(type, DateTimeType.Instance))
                return MathMatrix.As<NullValue<DateTime>, NullValue<T>>(DateTimeType.Null);
        }
        else if (typeof(T) == typeof(DateTimeOffset))
        {
            if (ReferenceEquals(type, DateTimeOffsetType.Instance))
                return MathMatrix.As<NullValue<DateTimeOffset>, NullValue<T>>(DateTimeOffsetType.Null);
        }
        else if (typeof(T) == typeof(IPv4Filter))
        {
            if (ReferenceEquals(type, IPv4FilterType.Instance))
                return MathMatrix.As<NullValue<IPv4Filter>, NullValue<T>>(IPv4FilterType.Null);
        }
        else if (typeof(T) == typeof(TimeSpan))
        {
            if (ReferenceEquals(type, TimeSpanType.Instance))
                return MathMatrix.As<NullValue<TimeSpan>, NullValue<T>>(TimeSpanType.Null);
        }
        else if (typeof(T) == typeof(GuidOrId))
        {
            if (ReferenceEquals(type, GuidOrIdType.Instance))
                return MathMatrix.As<NullValue<GuidOrId>, NullValue<T>>(GuidOrIdType.Null);
        }
        else if (typeof(T) == typeof(Guid))
        {
            if (ReferenceEquals(type, GuidType.Instance))
                return MathMatrix.As<NullValue<Guid>, NullValue<T>>(GuidType.Null);
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
                return MathMatrix.As<ConcreteValue<bool>, ConcreteValue<TValue>>(
                    Boolean(MathMatrix.As<TValue, bool>(v))
                );
            }
            if (ReferenceEquals(type, FlagType.Instance))
            {
                return MathMatrix.As<ConcreteValue<bool>, ConcreteValue<TValue>>(
                    Flag(MathMatrix.As<TValue, bool>(v))
                );
            }
        }

        return new ConcreteValue<TValue>(v, type);
    }

    /// <summary>
    /// Creates a concrete value of a type.
    /// </summary>
    public static ConcreteValue<IType> Type(IType type)
    {
        return new ConcreteValue<IType>(type, TypeOfType.Factory);
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
    public static IValue? TryReadValueFromJson(in JsonElement root, ValueReadOptions options, IPropertyType? valueType, IAssetSpecDatabase database, DatProperty owner)
    {
        NullReadValueVisitor v;
        return TryReadValueFromJson(in root, options, ref v, valueType, database, owner);
    }

    /// <summary>
    /// Parse a value from a JSON token. The visitor will only be invoked if the value is strongly typed.
    /// </summary>
    /// <returns>The parsed value, or <see langword="null"/> if the value couldn't be parsed.</returns>
    public static unsafe IValue? TryReadValueFromJson<TVisitor>(in JsonElement root, ValueReadOptions options, ref TVisitor visitor, IPropertyType? valueType, IAssetSpecDatabase database, DatProperty owner)
        where TVisitor : IReadValueVisitor
    {
        IType? type = valueType as IType;
        if (type != null)
        {
            ReadStronglyTypedValueVisitor<TVisitor> v;
            v.Value = null;
            v.Options = options;
            v.Database = database;
            v.Owner = owner;
            fixed (TVisitor* ptr = &visitor)
            fixed (JsonElement* elementPtr = &root)
            {
                v.Visitor = ptr;
                v.Element = elementPtr;
                type.Visit(ref v);
                if (v.Value != null)
                    return v.Value;
            }
        }

        switch (root.ValueKind)
        {
            case JsonValueKind.Number:
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

                break;

            case JsonValueKind.String:
                string str = root.GetString()!;
                if (str.Length == 0)
                {
                    if ((options & (ValueReadOptions.AssumeProperty | ValueReadOptions.AssumeDataRef)) != 0)
                    {
                        return null;
                    }

                    IValue<string> v = Create(string.Empty, StringType.Instance);
                    visitor.Accept(v);
                    return v;
                }

                ExpressionValueType defType = ParseDefType(options, ref str, out _, out _);

                switch (defType)
                {
                    case ExpressionValueType.Value:
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

                    case ExpressionValueType.DataRef:
                        // todo
                        throw new NotImplementedException();

                    case ExpressionValueType.PropertyRef:
                        PropertyReference pRef;
                        if (str.Length > 0 && (options & ValueReadOptions.AllowExclamationSuffix) != 0 && str[^1] == '!')
                            pRef = PropertyReference.Parse(str.AsSpan(0, str.Length - 1), null);
                        else
                            pRef = PropertyReference.Parse(str, str);

                        return pRef.CreateValue(owner, database);

                    case ExpressionValueType.Expression:
                        if (valueType == null || !valueType.TryGetConcreteType(out IType? concreteType))
                        {
                            break;
                        }

                        ExpressionVisitor expressionVisitor;
                        expressionVisitor.String = str;
                        expressionVisitor.ExpressionValue = null;
                        concreteType.Visit(ref expressionVisitor);
                        return expressionVisitor.ExpressionValue;
                }

                break;

            case JsonValueKind.True:
                IValue<bool> boolVal = Create(true, type as IType<bool> ?? BooleanType.Instance);
                visitor.Accept(boolVal);
                return boolVal;

            case JsonValueKind.False:
                boolVal = Create(true, type as IType<bool> ?? BooleanType.Instance);
                visitor.Accept(boolVal);
                return boolVal;

            case JsonValueKind.Array:
                if (valueType != null && SwitchValue.TryRead(in root, valueType, database, owner, out SwitchValue? value))
                {
                    return value;
                }

                break;
        }

        return null;
    }

    /// <summary>
    /// Parse a value from a JSON token.
    /// </summary>
    /// <returns>The parsed value, or <see langword="null"/> if the value couldn't be parsed.</returns>
    public static IValue<TValue>? TryReadValueFromJson<TValue>(in JsonElement root, ValueReadOptions options, IType<TValue> valueType, IAssetSpecDatabase database, DatProperty owner)
        where TValue : IEquatable<TValue>
    {
        switch (root.ValueKind)
        {
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Number:
            case JsonValueKind.Object:
                if (valueType.TryReadFromJson(in root, database, owner, out IValue<TValue>? val))
                {
                    return val;
                }

                break;

            case JsonValueKind.String:
                string str = root.GetString()!;
                if (str.Length == 0)
                {
                    if ((options & (ValueReadOptions.AssumeProperty | ValueReadOptions.AssumeDataRef)) != 0)
                    {
                        return null;
                    }

                    return valueType.TryReadFromJson(in root, database, owner, out val)
                        ? val
                        : Null(valueType);
                }

                ExpressionValueType defType = ParseDefType(options, ref str, out bool hasPrefix, out bool hasEscapeSequences);

                switch (defType)
                {
                    case ExpressionValueType.Value:
                        if (TryReadValueFromString(
                                in root,
                                !hasEscapeSequences ? hasPrefix ? str : null : StringHelper.Unescape(str),
                                valueType,
                                out val))
                        {
                            return val;
                        }

                        return null;

                    case ExpressionValueType.DataRef:
                        // todo
                        throw new NotImplementedException();

                    case ExpressionValueType.PropertyRef:
                        PropertyReference pRef;
                        if (str.Length > 0 && (options & ValueReadOptions.AllowExclamationSuffix) != 0 && str[^1] == '!')
                            pRef = PropertyReference.Parse(str.AsSpan(0, str.Length - 1), null);
                        else
                            pRef = PropertyReference.Parse(str, str);

                        return pRef.CreateValue(valueType, owner, database);

                    case ExpressionValueType.Expression:
                        try
                        {
                            return FromExpression(valueType, str);
                        }
                        catch
                        {
                            if ((options & ValueReadOptions.ThrowExceptions) != 0)
                                throw;

                            return null;
                        }
                }

                break;

            case JsonValueKind.Array:
                if (SwitchValue.TryRead(in root, valueType, database, owner, out SwitchValue<TValue>? value))
                {
                    return value;
                }
                if (valueType.TryReadFromJson(in root, database, owner, out val))
                {
                    return val;
                }
                break;

        }

        return null;
    }

    private static ExpressionValueType ParseDefType(ValueReadOptions options, ref string str, out bool hasPrefix, out bool hasEscapeSequences)
    {
        ExpressionValueType defType = ExpressionValueType.Value;
        if ((options & ValueReadOptions.AssumeProperty) != 0)
            defType = ExpressionValueType.PropertyRef;
        else if ((options & ValueReadOptions.AssumeDataRef) != 0)
            defType = ExpressionValueType.DataRef;

        hasPrefix = false;
        if (str[0] == '@')
        {
            defType = ExpressionValueType.PropertyRef;
            hasPrefix = true;
        }
        else if (str[0] == '%')
        {
            defType = ExpressionValueType.Value;
            hasPrefix = true;
        }
        else if (str[0] == '#')
        {
            defType = ExpressionValueType.DataRef;
            hasPrefix = true;
        }
        else if (str[0] == '=')
        {
            defType = ExpressionValueType.Expression;
            hasPrefix = true;
        }

        bool hasParenthesis = hasPrefix && str.Length > 1 && str[1] == '(';
        if (hasParenthesis && str.Length > 2)
        {
            int valueEndIndex = StringHelper.NextUnescapedIndexOf(str.AsSpan(2), [ '(', ')', '\\' ], out hasEscapeSequences, useDepth: true);
            if (valueEndIndex == -1)
            {
                //hasParenthesis = false;
            }
            else
            {
                str = str.Substring(2, valueEndIndex);
            }
        }
        else
        {
            //hasParenthesis = false;
            hasEscapeSequences = str.IndexOf('\\') >= 0;
        }

        return defType;
    }

    private unsafe struct ReadStronglyTypedValueVisitor<TVisitor> : ITypeVisitor
        where TVisitor : IReadValueVisitor
    {
        public IValue? Value;
        public TVisitor* Visitor;
        public JsonElement* Element;
        public ValueReadOptions Options;
        public IAssetSpecDatabase Database;
        public DatProperty Owner;
        public void Accept<TValue>(IType<TValue> type)
            where TValue : IEquatable<TValue>
        {
            IValue<TValue>? value = TryReadValueFromJson(in Unsafe.AsRef<JsonElement>(Element), Options, type, Database, Owner);
            if (value != null)
            {
                Visitor->Accept(value);
                Value = value;
            }
        }
    }

    private static bool TryReadValueFromString<TValue>(in JsonElement element, string? str, IType<TValue> type, [NotNullWhen(true)] out IValue<TValue>? readValue)
        where TValue : IEquatable<TValue>
    {
        if (typeof(TValue) == typeof(string))
        {
            IValue<string> v = Create(str ?? element.GetString()!, Unsafe.As<IType<TValue>, IType<string>>(ref type));
            readValue = Unsafe.As<IValue<string>, IValue<TValue>>(ref v);
            return true;
        }

        if (str == null && type.Parser.TryReadValueFromJson(in element, out Optional<TValue> value, type))
        {
            IValue<TValue> v = !value.HasValue ? Null(type) : Create(value.Value, type);
            readValue = v;
            return true;
        }

        if (TypeConverters.TryGet<TValue>() is { } converter)
        {
            TypeConverterParseArgs<TValue> args = default;
            args.Type = type;
            if (str == null && converter.TryReadJson(in element, out value, ref args))
            {
                IValue<TValue> v = !value.HasValue ? Null(type) : Create(value.Value, type);
                readValue = v;
                return true;
            }
            if (converter.TryParse(str ?? element.GetString(), ref args, out TValue? val))
            {
                IValue<TValue> v = val == null ? Null(type) : Create(val, type);
                readValue = v;
                return true;
            }
        }

        readValue = null;
        return false;
    }

    private struct ExpressionVisitor : ITypeVisitor
    {
        public string String;
        public IValue? ExpressionValue;

        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            ExpressionValue = FromExpression(type, String);
        }
    }

    private struct NullReadValueVisitor : IReadValueVisitor
    {
        public void Accept<TValue>(IValue<TValue> value) where TValue : IEquatable<TValue> { }
    }

    /// <summary>
    /// A visitor that accepts the result of <see cref="Value.TryReadValueFromJson{TVisitor}"/>.
    /// </summary>
    public interface IReadValueVisitor
    {
        /// <summary>
        /// Invoked by <see cref="Value.TryReadValueFromJson{TVisitor}"/> to accept the parsed value as a strongly typed value.
        /// </summary>
        void Accept<TValue>(IValue<TValue> value) where TValue : IEquatable<TValue>;
    }
}

/// <summary>
/// Context for evaluating a dynamic value.
/// </summary>
[Flags]
public enum ValueReadOptions
{
    /// <summary>
    /// Assumes that the value is a raw value (%).
    /// </summary>
    AssumeValue = 0,

    /// <summary>
    /// Assumes that the value is a property reference (@).
    /// </summary>
    AssumeProperty = 1,

    /// <summary>
    /// Assumes that the value is a data-ref (#).
    /// </summary>
    AssumeDataRef = 2,

    /// <summary>
    /// Allows for a switch-case object (<see cref="SpecDynamicSwitchCaseValue"/>) to be supplied.
    /// </summary>
    AllowSwitchCase = 4,

    /// <summary>
    /// Allows for a condition object (<see cref="SpecCondition"/>) to be supplied.
    /// </summary>
    AllowCondition = 8,

    /// <summary>
    /// Allows for a switch expression (<see cref="SpecDynamicSwitchValue"/>) to be supplied.
    /// </summary>
    AllowSwitch = 16,

    /// <summary>
    /// Allows all conditional objects (switch-case, switches, and conditions).
    /// </summary>
    AllowConditionals = AllowSwitchCase | AllowSwitch | AllowCondition,

    /// <summary>
    /// The default options for parsing values.
    /// </summary>
    Default = AssumeValue | AllowConditionals,

    /// <summary>
    /// Whether or not a '!' can be at the end of a property reference.
    /// </summary>
    /// <remarks>Used for ListReference to indicate that it's an error to not use a value in the referened list.</remarks>
    AllowExclamationSuffix = 32,

    /// <summary>
    /// Whether exceptions from other parsing systems like expressions should be thrown. The default behavior is to catch them and return <see langword="null"/>.
    /// </summary>
    ThrowExceptions = 64
}
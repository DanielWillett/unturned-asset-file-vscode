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
        if (typeof(T).IsValueType)
        {
            if (typeof(T).IsPrimitive)
            {
                if (typeof(T) == typeof(long))
                {
                    if ((object)type == Int64Type.Instance)
                        return MathMatrix.As<NullValue<long>, NullValue<T>>(Int64Type.Null);
                }
                else if (typeof(T) == typeof(ulong))
                {
                    if ((object)type == UInt64Type.Instance)
                        return MathMatrix.As<NullValue<ulong>, NullValue<T>>(UInt64Type.Null);
                }
                else if (typeof(T) == typeof(int))
                {
                    if ((object)type == Int32Type.Instance)
                        return MathMatrix.As<NullValue<int>, NullValue<T>>(Int32Type.Null);
                }
                else if (typeof(T) == typeof(uint))
                {
                    if ((object)type == UInt32Type.Instance)
                        return MathMatrix.As<NullValue<uint>, NullValue<T>>(UInt32Type.Null);
                }
                else if (typeof(T) == typeof(short))
                {
                    if ((object)type == Int16Type.Instance)
                        return MathMatrix.As<NullValue<short>, NullValue<T>>(Int16Type.Null);
                }
                else if (typeof(T) == typeof(ushort))
                {
                    if ((object)type == UInt16Type.Instance)
                        return MathMatrix.As<NullValue<ushort>, NullValue<T>>(UInt16Type.Null);
                }
                else if (typeof(T) == typeof(sbyte))
                {
                    if ((object)type == Int8Type.Instance)
                        return MathMatrix.As<NullValue<sbyte>, NullValue<T>>(Int8Type.Null);
                }
                else if (typeof(T) == typeof(byte))
                {
                    if ((object)type == UInt8Type.Instance)
                        return MathMatrix.As<NullValue<byte>, NullValue<T>>(UInt8Type.Null);
                }
                else if (typeof(T) == typeof(float))
                {
                    if ((object)type == Float32Type.Instance)
                        return MathMatrix.As<NullValue<float>, NullValue<T>>(Float32Type.Null);
                }
                else if (typeof(T) == typeof(double))
                {
                    if ((object)type == Float64Type.Instance)
                        return MathMatrix.As<NullValue<double>, NullValue<T>>(Float64Type.Null);
                }
                else if (typeof(T) == typeof(bool))
                {
                    if ((object)type == BooleanType.Instance)
                        return MathMatrix.As<NullValue<bool>, NullValue<T>>(BooleanType.Null);
                    if ((object)type == FlagType.Instance)
                        return MathMatrix.As<NullValue<bool>, NullValue<T>>(FlagType.Null);
                    if ((object)type == BooleanOrFlagType.Instance)
                        return MathMatrix.As<NullValue<bool>, NullValue<T>>(BooleanOrFlagType.Null);
                }
                else if (typeof(T) == typeof(char))
                {
                    if ((object)type == CharacterType.Instance)
                        return MathMatrix.As<NullValue<char>, NullValue<T>>(CharacterType.Null);
                }
            }
            else if (typeof(T) == typeof(decimal))
            {
                if ((object)type == Float128Type.Instance)
                    return MathMatrix.As<NullValue<decimal>, NullValue<T>>(Float128Type.Null);
            }
            else if (typeof(T) == typeof(DateTime))
            {
                if ((object)type == DateTimeType.Instance)
                    return MathMatrix.As<NullValue<DateTime>, NullValue<T>>(DateTimeType.Null);
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                if ((object)type == DateTimeOffsetType.Instance)
                    return MathMatrix.As<NullValue<DateTimeOffset>, NullValue<T>>(DateTimeOffsetType.Null);
            }
            else if (typeof(T) == typeof(IPv4Filter))
            {
                if ((object)type == IPv4FilterType.Instance)
                    return MathMatrix.As<NullValue<IPv4Filter>, NullValue<T>>(IPv4FilterType.Null);
            }
            else if (typeof(T) == typeof(TimeSpan))
            {
                if ((object)type == TimeSpanType.Instance)
                    return MathMatrix.As<NullValue<TimeSpan>, NullValue<T>>(TimeSpanType.Null);
            }
            else if (typeof(T) == typeof(GuidOrId))
            {
                if ((object)type == GuidOrIdType.Instance)
                    return MathMatrix.As<NullValue<GuidOrId>, NullValue<T>>(GuidOrIdType.Null);
            }
            else if (typeof(T) == typeof(Guid))
            {
                if ((object)type == GuidType.Instance)
                    return MathMatrix.As<NullValue<Guid>, NullValue<T>>(GuidType.Null);
            }
        }
        else if (typeof(T) == typeof(string))
        {
            if ((object)type == StringType.Instance)
                return MathMatrix.As<NullValue<string>, NullValue<T>>(StringType.Null);
            if ((object)type == RegexStringType.Instance)
                return MathMatrix.As<NullValue<string>, NullValue<T>>(RegexStringType.Null);
        }

        return new NullValue<T>(type);
    }

    /// <summary>
    /// A <see langword="null"/> value of a given <paramref name="type"/>.
    /// </summary>
    [OverloadResolutionPriority(2)]
    public static NullValue<T> Null<TType, T>(PrimitiveType<T, TType> type) where TType : PrimitiveType<T, TType>, new() where T : IEquatable<T>
    {
        return (object)type == PrimitiveType<T, TType>.Instance ? PrimitiveType<T, TType>.Null : new NullValue<T>(type);
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

    /// <inheritdoc cref="FromExpression{TResult,TDataRefReadContext}(IType{TResult},string,DatProperty,ref TDataRefReadContext,bool)"/>
    public static IValue<TResult> FromExpression<TResult>(
        IType<TResult> resultType,
        string expression,
        DatProperty owner,
        bool simplifyConstantExpressions = true
    ) where TResult : IEquatable<TResult>
    {
        DataRefs.NilDataRefContext c;
        return FromExpression(resultType, expression, owner, ref c, simplifyConstantExpressions);
    }

    /// <inheritdoc cref="FromExpression{TResult,TDataRefReadContext}(IType{TResult},string,DatProperty,ref TDataRefReadContext,bool)"/>
    public static IValue<TResult> FromExpression<TResult>(
        IType<TResult> resultType,
        ReadOnlySpan<char> expression,
        DatProperty owner,
        bool simplifyConstantExpressions = true
    ) where TResult : IEquatable<TResult>
    {
        DataRefs.NilDataRefContext c;
        return FromExpression(resultType, expression, owner, ref c, simplifyConstantExpressions);
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
    /// <typeparam name="TDataRefReadContext">
    /// Context used to read data-refs.
    /// </typeparam>
    public static unsafe IValue<TResult> FromExpression<TResult, TDataRefReadContext>(
        IType<TResult> resultType,
        string expression,
        DatProperty owner,
        ref TDataRefReadContext dataRefContext,
        bool simplifyConstantExpressions = true
    ) where TResult : IEquatable<TResult>
      where TDataRefReadContext : IDataRefReadContext?
    {
        // trim '='
        if (expression.Length > 0 && expression[0] == '=')
        {
            return FromExpression(resultType, expression.AsSpan(1), owner, ref dataRefContext, simplifyConstantExpressions);
        }

        IExpressionNode rootNode;
#if !NET7_0_OR_GREATER
        fixed (TDataRefReadContext* dataRefContextPtr = &dataRefContext)
        {
#endif
        using (ExpressionNodeParser<TDataRefReadContext> parser = new ExpressionNodeParser<TDataRefReadContext>(
                   expression,
                   owner,
#if NET7_0_OR_GREATER
                   ref dataRefContext,
#else
                   dataRefContextPtr,
#endif
                   simplifyConstantExpressions
        ))
        {
            rootNode = parser.Parse<TResult>();
        }

#if !NET7_0_OR_GREATER
        }
#endif

        if (simplifyConstantExpressions && rootNode is IValue<TResult> value)
        {
            return value;
        }

        return new ExpressionValue<TResult>(resultType, (IFunctionExpressionNode)rootNode);
    }

    /// <inheritdoc cref="FromExpression{TResult,TDataRefReadContext}(IType{TResult},string,DatProperty,ref TDataRefReadContext,bool)"/>
    public static unsafe IValue<TResult> FromExpression<TResult, TDataRefReadContext>(
        IType<TResult> resultType,
        ReadOnlySpan<char> expression,
        DatProperty owner,
        ref TDataRefReadContext dataRefContext,
        bool simplifyConstantExpressions = true
    ) where TResult : IEquatable<TResult>
      where TDataRefReadContext : IDataRefReadContext?
    {
        // trim '='
        if (!expression.IsEmpty && expression[0] == '=')
            expression = expression.Slice(1);
        
        IExpressionNode rootNode;
#if !NET7_0_OR_GREATER
        fixed (TDataRefReadContext* dataRefContextPtr = &dataRefContext)
        {
#endif
        using (ExpressionNodeParser<TDataRefReadContext> parser = new ExpressionNodeParser<TDataRefReadContext>(
                   expression,
                   owner,
#if NET7_0_OR_GREATER
                   ref dataRefContext,
#else
                   dataRefContextPtr,
#endif
                   simplifyConstantExpressions
        ))
        {
            rootNode = parser.Parse<TResult>();
        }

#if !NET7_0_OR_GREATER
        }
#endif

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

    /// <inheritdoc cref="TryReadValueFromJson{TDataRefReadContext}(in JsonElement,ValueReadOptions,IPropertyType,IAssetSpecDatabase,IDatSpecificationObject,ref TDataRefReadContext)"/>
    public static IValue? TryReadValueFromJson(
        in JsonElement root,
        ValueReadOptions options,
        IPropertyType? valueType,
        IAssetSpecDatabase database,
        IDatSpecificationObject owner
    )
    {
        NullReadValueVisitor v;
        DataRefs.NilDataRefContext c;
        return TryReadValueFromJson(in root, options, ref v, valueType, database, owner, ref c);
    }

    /// <summary>
    /// Parse a value from a JSON token.
    /// </summary>
    /// <returns>The parsed value, or <see langword="null"/> if the value couldn't be parsed.</returns>
    public static IValue? TryReadValueFromJson<TDataRefReadContext>(
        in JsonElement root,
        ValueReadOptions options,
        IPropertyType? valueType,
        IAssetSpecDatabase database,
        IDatSpecificationObject owner,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        NullReadValueVisitor v;
        return TryReadValueFromJson(in root, options, ref v, valueType, database, owner, ref dataRefContext);
    }

    /// <inheritdoc cref="TryReadValueFromJson{TVisitor,TDataRefReadContext}(in JsonElement,ValueReadOptions,ref TVisitor,IPropertyType,IAssetSpecDatabase,IDatSpecificationObject,ref TDataRefReadContext)"/>
    public static IValue? TryReadValueFromJson<TVisitor>(
        in JsonElement root,
        ValueReadOptions options,
        ref TVisitor visitor,
        IPropertyType? valueType,
        IAssetSpecDatabase database,
        IDatSpecificationObject owner
    ) where TVisitor : IReadValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        DataRefs.NilDataRefContext c;
        return TryReadValueFromJson(in root, options, ref visitor, valueType, database, owner, ref c);
    }

    /// <summary>
    /// Parse a value from a JSON token. The visitor will only be invoked if the value is strongly typed.
    /// </summary>
    /// <returns>The parsed value, or <see langword="null"/> if the value couldn't be parsed.</returns>
    public static unsafe IValue? TryReadValueFromJson<TVisitor, TDataRefReadContext>(
        in JsonElement root,
        ValueReadOptions options,
        ref TVisitor visitor,
        IPropertyType? valueType,
        IAssetSpecDatabase database,
        IDatSpecificationObject owner,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
        where TVisitor : IReadValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
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

                ExpressionValueType defType = ParseDefType(options, ref str, out _, out _, out ReadOnlySpan<char> data);

                switch (defType)
                {
                    case ExpressionValueType.Value:
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                        // ReSharper disable once InlineTemporaryVariable
                        ReadOnlySpan<char> strParsed = data;
#else
                        string strParsed = str.Length == data.Length ? str : data.ToString();
#endif
                        if (Guid.TryParse(strParsed, out Guid guid))
                        {
                            IValue<Guid> v = Create(guid, GuidType.Instance);
                            visitor.Accept(v);
                            return v;
                        }
                        if (TimeSpan.TryParse(strParsed, CultureInfo.InvariantCulture, out TimeSpan ts))
                        {
                            IValue<TimeSpan> v = Create(ts, TimeSpanType.Instance);
                            visitor.Accept(v);
                            return v;
                        }
                        if (DateTime.TryParse(strParsed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dt))
                        {
                            IValue<DateTime> v = Create(dt, DateTimeType.Instance);
                            visitor.Accept(v);
                            return v;
                        }
                        if (DateTimeOffset.TryParse(strParsed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset dto))
                        {
                            IValue<DateTimeOffset> v = Create(dto, DateTimeOffsetType.Instance);
                            visitor.Accept(v);
                            return v;
                        }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                        IValue<string> strVal = Create(str.Length == data.Length ? str : new string(data), StringType.Instance);
#else
                        IValue<string> strVal = Create(strParsed, StringType.Instance);
#endif
                        visitor.Accept(strVal);
                        return strVal;

                    case ExpressionValueType.DataRef:
                        DatProperty? propertyOwner = owner as DatProperty;
                        if (propertyOwner == null
                            || !DataRefs.TryReadDataRef(
                                    str.Length == data.Length
                                        ? str
                                        : data.ToString(),
                                    type,
                                    propertyOwner,
                                    out IDataRef? dataRef,
                                    ref dataRefContext
                                )
                            )
                        {
                            return null;
                        }

                        return dataRef;

                    case ExpressionValueType.PropertyRef:
                        propertyOwner = owner as DatProperty;
                        if (propertyOwner == null)
                            return null;
                        PropertyReference pRef;
                        if ((options & ValueReadOptions.AllowExclamationSuffix) != 0 && data.Length > 0 && data[^1] == '!')
                            pRef = PropertyReference.Parse(data[..^1], null);
                        else if (str.Length == data.Length)
                            pRef = PropertyReference.Parse(data, str);
                        else
                            pRef = PropertyReference.Parse(data, null);

                        return pRef.CreateValue(propertyOwner, database);

                    case ExpressionValueType.Expression:
                        if (valueType == null || !valueType.TryGetConcreteType(out IType? concreteType))
                        {
                            break;
                        }

                        propertyOwner = owner as DatProperty;
                        if (propertyOwner == null)
                            return null;

                        ExpressionVisitor<TDataRefReadContext> expressionVisitor;
                        expressionVisitor.String = str.Length == data.Length ? str : data.ToString();
                        expressionVisitor.ExpressionValue = null;
                        expressionVisitor.Property = propertyOwner;
                        fixed (TDataRefReadContext* dataRefContextPtr = &dataRefContext)
                        {
                            expressionVisitor.DataRefContext = dataRefContextPtr;
                            concreteType.Visit(ref expressionVisitor);
                        }
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
                if (root.GetArrayLength() == 0)
                    return NullValue.Instance;

                if (valueType != null && SwitchValue.TryRead(in root, valueType, database, owner, out SwitchValue? value, ref dataRefContext))
                {
                    return value;
                }

                break;

            case JsonValueKind.Null:
                if (type != null)
                    return Null(type);

                return NullValue.Instance;
        }

        return null;
    }

    /// <inheritdoc cref="TryReadValueFromJson{TValue,TDataRefReadContext}(in JsonElement,ValueReadOptions,IType{TValue},IAssetSpecDatabase,IDatSpecificationObject,ref TDataRefReadContext)"/>
    public static IValue<TValue>? TryReadValueFromJson<TValue>(
        in JsonElement root,
        ValueReadOptions options,
        IType<TValue> valueType,
        IAssetSpecDatabase database,
        IDatSpecificationObject owner
    ) where TValue : IEquatable<TValue>
    {
        DataRefs.NilDataRefContext c;
        return TryReadValueFromJson(in root, options, valueType, database, owner, ref c);
    }

    /// <summary>
    /// Parse a value from a JSON token.
    /// </summary>
    /// <returns>The parsed value, or <see langword="null"/> if the value couldn't be parsed.</returns>
    public static IValue<TValue>? TryReadValueFromJson<TValue, TDataRefReadContext>(
        in JsonElement root,
        ValueReadOptions options,
        IType<TValue> valueType,
        IAssetSpecDatabase database,
        IDatSpecificationObject owner,
        ref TDataRefReadContext dataRefContext
    ) where TValue : IEquatable<TValue>
      where TDataRefReadContext : IDataRefReadContext?
    {
        switch (root.ValueKind)
        {
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Number:
            case JsonValueKind.Object:
                if (valueType.TryReadFromJson(in root, database, owner, out IValue<TValue>? val, ref dataRefContext))
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

                    return valueType.TryReadFromJson(in root, database, owner, out val, ref dataRefContext)
                        ? val
                        : Null(valueType);
                }

                ExpressionValueType defType = ParseDefType(options, ref str, out bool hasPrefix, out bool hasEscapeSequences, out ReadOnlySpan<char> data);

                switch (defType)
                {
                    case ExpressionValueType.Value:
                        if (TryReadValueFromString(
                                in root,
                                // when null, read from the original JSON object, allows calling ReadValueFromJson instead of the type converter
                                hasPrefix || hasEscapeSequences ? str.Length == data.Length ? str : data.ToString() : null,
                                valueType,
                                out val))
                        {
                            return val;
                        }

                        return null;

                    case ExpressionValueType.DataRef:
                        DatProperty? propertyOwner = owner as DatProperty;
                        if (propertyOwner == null)
                            return null;
                        string stringVal = str.Length == data.Length ? str : data.ToString();
                        return DataRefs.TryReadDataRef(stringVal, valueType, propertyOwner, out IDataRef? dataRef, ref dataRefContext) ? dataRef as IValue<TValue> : null;

                    case ExpressionValueType.PropertyRef:
                        propertyOwner = owner as DatProperty;
                        if (propertyOwner == null)
                            return null;
                        PropertyReference pRef;
                        if ((options & ValueReadOptions.AllowExclamationSuffix) != 0 && data.Length > 0 && data[^1] == '!')
                            pRef = PropertyReference.Parse(data[..^1], null);
                        else if (str.Length == data.Length)
                            pRef = PropertyReference.Parse(data, str);
                        else
                            pRef = PropertyReference.Parse(data, null);

                        return pRef.CreateValue(valueType, propertyOwner, database);

                    case ExpressionValueType.Expression:
                        propertyOwner = owner as DatProperty;
                        if (propertyOwner == null)
                            return null;
                        try
                        {
                            return FromExpression(valueType, str, propertyOwner, ref dataRefContext);
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
                if (SwitchValue.TryRead(in root, valueType, database, owner, out SwitchValue<TValue>? value, ref dataRefContext))
                {
                    return value;
                }
                if (valueType.TryReadFromJson(in root, database, owner, out val, ref dataRefContext))
                {
                    return val;
                }
                break;

            case JsonValueKind.Null:
                return Null(valueType);
        }

        return null;
    }

    private static ExpressionValueType ParseDefType(ValueReadOptions options, ref string str, out bool hasPrefix, out bool hasEscapeSequences, out ReadOnlySpan<char> data)
    {
        ExpressionValueType defType = ExpressionValueType.Value;
        if ((options & ValueReadOptions.AssumeProperty) != 0)
            defType = ExpressionValueType.PropertyRef;
        else if ((options & ValueReadOptions.AssumeDataRef) != 0)
            defType = ExpressionValueType.DataRef;

        data = str;

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

        // DataRef parenthesis are handled differently
        bool hasParenthesis = defType != ExpressionValueType.DataRef && hasPrefix && str.Length > 1 && str[1] == '(';
        if (hasParenthesis && str.Length > 2)
        {
            int valueEndIndex = StringHelper.NextUnescapedIndexOfParenthesis(str.AsSpan(2), out hasEscapeSequences);
            if (valueEndIndex == -1)
            {
                data = str.AsSpan(1);
                //hasParenthesis = false;
            }
            else
            {
                data = str.AsSpan(2, valueEndIndex);
            }
        }
        else
        {
            //hasParenthesis = false;
            if (hasPrefix)
                data = str.AsSpan(1);
            hasEscapeSequences = str.IndexOf('\\') >= 0;
        }

        if (hasEscapeSequences)
        {
            str = StringHelper.Unescape(data, [ '(', ')', '\\' ]);
            data = str.AsSpan();
        }

        return defType;
    }

    private unsafe struct ReadStronglyTypedValueVisitor<TVisitor> : ITypeVisitor
        where TVisitor : IReadValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        public IValue? Value;
        public TVisitor* Visitor;
        public JsonElement* Element;
        public ValueReadOptions Options;
        public IAssetSpecDatabase Database;
        public IDatSpecificationObject Owner;
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

    private unsafe struct ExpressionVisitor<TDataRefReadContext> : ITypeVisitor
        where TDataRefReadContext : IDataRefReadContext?
    {
        public string String;
        public IValue? ExpressionValue;
        public DatProperty Property;
        public TDataRefReadContext* DataRefContext;

        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            ExpressionValue = FromExpression(type, String, Property, ref Unsafe.AsRef<TDataRefReadContext>(DataRefContext));
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
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

internal static class SpecDynamicEquationTreeValueHelpers
{
    public static ISpecPropertyType GetValueType(ISpecDynamicValue argument, SpecDynamicEquationTreeUnaryOperation operation, ISpecPropertyType? expectedType)
    {
        if (operation
            is >= SpecDynamicEquationTreeUnaryOperation.SineRad
            and <= SpecDynamicEquationTreeUnaryOperation.SquareRoot)
        {
            if (expectedType != null && (expectedType is IVectorSpecPropertyType || expectedType.ValueType == typeof(float) || expectedType.ValueType == typeof(double) || expectedType.ValueType == typeof(decimal)))
            {
                return expectedType;
            }

            // trig operations, must be decimal
            if (argument.ValueType == null)
                return KnownTypes.Float64;

            Type t = argument.ValueType.ValueType;
            if (t == typeof(float) || t == typeof(double) || t == typeof(decimal) || t == typeof(string))
                return argument.ValueType;

            return KnownTypes.Float64;
        }

        if (expectedType != null)
        {
            if (expectedType is IVectorSpecPropertyType
                || expectedType.ValueType == typeof(string)
                || IsInteger(expectedType.ValueType)
                || expectedType.ValueType == typeof(float)
                || expectedType.ValueType == typeof(double)
                || expectedType.ValueType == typeof(decimal))
            {
                return expectedType;
            }
        }

        return argument.ValueType ?? KnownTypes.Float64;
    }

    public static ISpecPropertyType GetValueType(ISpecDynamicValue left, ISpecDynamicValue? right, SpecDynamicEquationTreeBinaryOperation operation, ISpecPropertyType? expectedType)
    {
        if (operation == SpecDynamicEquationTreeBinaryOperation.Concat)
        {
            return expectedType != null && (expectedType is EnumSpecType || expectedType.ValueType == typeof(string)) ? expectedType : KnownTypes.String;
        }

        if (expectedType != null)
        {
            if (expectedType is IVectorSpecPropertyType
                || expectedType.ValueType == typeof(string)
                || IsInteger(expectedType.ValueType)
                || expectedType.ValueType == typeof(float)
                || expectedType.ValueType == typeof(double)
                || expectedType.ValueType == typeof(decimal))
            {
                return expectedType;
            }
        }

        if (right == null)
        {
            return left.ValueType ?? KnownTypes.Float64;
        }

        if (left.ValueType != null && left.ValueType.Equals(right.ValueType))
        {
            return left.ValueType ?? KnownTypes.Float64;
        }
        if (left.ValueType != null && right.ValueType != null && IsInteger(left.ValueType.ValueType) && IsInteger(right.ValueType.ValueType))
        {
            return GetLargestInteger(left.ValueType, right.ValueType);
        }

        return KnownTypes.Float64;
    }

    public static ISpecPropertyType GetValueType(ISpecDynamicValue arg1, ISpecDynamicValue arg2, ISpecDynamicValue arg3, SpecDynamicEquationTreeTertiaryOperation operation, ISpecPropertyType? expectedType)
    {
        if (operation == SpecDynamicEquationTreeTertiaryOperation.BallisticGravityMultiplierCalculation)
        {
            if (expectedType != null && (expectedType.ValueType == typeof(float) || expectedType.ValueType == typeof(double) || expectedType.ValueType == typeof(decimal) || expectedType.ValueType == typeof(string)))
            {
                return expectedType;
            }

            return KnownTypes.Float32;
        }

        if (operation == SpecDynamicEquationTreeTertiaryOperation.Replace)
        {
            return expectedType != null && (expectedType is EnumSpecType || expectedType.ValueType == typeof(string)) ? expectedType : KnownTypes.String;
        }

        return expectedType ?? KnownTypes.String;
    }

    private static ISpecPropertyType GetLargestInteger(ISpecPropertyType left, ISpecPropertyType right)
    {
        Type t0 = left.ValueType;
        Type t1 = right.ValueType;
        if (t0 == typeof(ulong) || t0 == typeof(long))
        {
            return left;
        }
        if (t0 == typeof(uint) || t0 == typeof(int))
        {
            return t1 == typeof(ulong) || t1 == typeof(long)
                ? right : left;
        }
        if (t0 == typeof(ushort) || t0 == typeof(short))
        {
            return t1 == typeof(ulong) || t1 == typeof(long) || t1 == typeof(uint) || t1 == typeof(int)
                ? right : left;
        }
        if (t0 == typeof(byte) || t0 == typeof(sbyte))
        {
            return t1 == typeof(ulong) || t1 == typeof(long) || t1 == typeof(uint) || t1 == typeof(int) || t1 == typeof(ushort) || t1 == typeof(short)
                ? right : left;
        }

        return left;
    }

    public static bool IsInteger(Type type)
    {
        return type.IsPrimitive &&
               (type == typeof(byte)
                || type == typeof(sbyte)
                || type == typeof(ushort)
                || type == typeof(short)
                || type == typeof(uint)
                || type == typeof(int)
                || type == typeof(ulong)
                || type == typeof(long)
            );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInteger<T>()
    {
        return typeof(T) == typeof(byte)
                || typeof(T) == typeof(sbyte)
                || typeof(T) == typeof(ushort)
                || typeof(T) == typeof(short)
                || typeof(T) == typeof(uint)
                || typeof(T) == typeof(int)
                || typeof(T) == typeof(ulong)
                || typeof(T) == typeof(long);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTo As<TFrom, TTo>(TFrom value)
    {
        return Unsafe.As<TFrom, TTo>(ref value);
    }

    public static bool TryConvertVector<TIn, TOut>(TIn? rawValue, bool rawIsNull, out TOut? value, out bool isNull)
    {
        if (rawIsNull || rawValue == null)
        {
            isNull = true;
            value = default;
            return true;
        }

        isNull = rawIsNull;

        if (typeof(TIn) == typeof(TOut))
        {
            value = As<TIn, TOut>(rawValue);
            return true;
        }

        if (typeof(TIn) == typeof(Color))
        {
            if (typeof(TOut) == typeof(Color32))
            {
                value = As<Color32, TOut>(As<TIn, Color>(rawValue));
                return true;
            }
        }
        else if (typeof(TIn) == typeof(Color32))
        {
            if (typeof(TOut) == typeof(Color))
            {
                value = As<Color, TOut>(As<TIn, Color32>(rawValue));
                return true;
            }
        }
        else if (typeof(TIn) == typeof(Vector2))
        {
            if (typeof(TOut) == typeof(Vector3))
            {
                Vector2 v2 = As<TIn, Vector2>(rawValue);
                value = As<Vector3, TOut>(new Vector3(v2.X, v2.Y, 0f));
                return true;
            }
            if (typeof(TOut) == typeof(Vector4))
            {
                Vector2 v2 = As<TIn, Vector2>(rawValue);
                value = As<Vector4, TOut>(new Vector4(v2.X, v2.Y, 0f, 0f));
                return true;
            }
        }
        else if (typeof(TIn) == typeof(Vector3))
        {
            if (typeof(TOut) == typeof(Vector2))
            {
                Vector3 v3 = As<TIn, Vector3>(rawValue);
                value = As<Vector2, TOut>(new Vector2(v3.X, v3.Y));
                return true;
            }
            if (typeof(TOut) == typeof(Vector4))
            {
                Vector3 v3 = As<TIn, Vector3>(rawValue);
                value = As<Vector4, TOut>(new Vector4(v3.X, v3.Y, v3.Z, 0f));
                return true;
            }
        }
        else if (typeof(TIn) == typeof(Vector4))
        {
            if (typeof(TOut) == typeof(Vector2))
            {
                Vector4 v4 = As<TIn, Vector4>(rawValue);
                value = As<Vector2, TOut>(new Vector2(v4.X, v4.Y));
                return true;
            }
            if (typeof(TOut) == typeof(Vector3))
            {
                Vector4 v4 = As<TIn, Vector4>(rawValue);
                value = As<Vector3, TOut>(new Vector3(v4.X, v4.Y, v4.Z));
                return true;
            }
        }

        isNull = true;
        value = default;
        return false;
    }

    public static bool TryConvert<TIn, TOut>(TIn? rawValue, bool rawIsNull, out TOut? value, out bool isNull) where TIn : IConvertible
    {
        if (rawIsNull || rawValue == null)
        {
            isNull = true;
            value = default;
            return true;
        }

        if (typeof(TIn) == typeof(TOut))
        {
            value = As<TIn, TOut>(rawValue);
            isNull = rawIsNull;
            return true;
        }

        if (typeof(TOut) == typeof(Vector2))
        {
            if (TryConvert(rawValue, rawIsNull, out float comp, out isNull))
            {
                value = As<Vector2, TOut>(new Vector2(comp, comp));
                return true;
            }

            value = default;
            isNull = true;
            return false;
        }
        if (typeof(TOut) == typeof(Vector3))
        {
            if (TryConvert(rawValue, rawIsNull, out float comp, out isNull))
            {
                value = As<Vector3, TOut>(new Vector3(comp, comp, comp));
                return true;
            }

            value = default;
            isNull = true;
            return false;
        }
        if (typeof(TOut) == typeof(Vector4))
        {
            if (TryConvert(rawValue, rawIsNull, out float comp, out isNull))
            {
                value = As<Vector4, TOut>(new Vector4(comp, comp, comp, comp));
                return true;
            }

            value = default;
            isNull = true;
            return false;
        }

        isNull = false;
        // this isnt as slow as it looks, most of it will be branch-eliminated during JIT
        if (typeof(TOut) == typeof(bool))
            value = As<bool, TOut>(rawValue.ToBoolean(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(char))
            value = As<char, TOut>(rawValue.ToChar(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(DateTime))
            value = As<DateTime, TOut>(rawValue.ToDateTime(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(DBNull))
        {
            isNull = true;
            value = default;
            return false;
        }
        else if (typeof(TOut) == typeof(float))
            value = As<float, TOut>(rawValue.ToSingle(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(double))
            value = As<double, TOut>(rawValue.ToDouble(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(decimal))
            value = As<decimal, TOut>(rawValue.ToDecimal(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(byte))
            value = As<byte, TOut>(rawValue.ToByte(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(sbyte))
            value = As<sbyte, TOut>(rawValue.ToSByte(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(short))
            value = As<short, TOut>(rawValue.ToInt16(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(ushort))
            value = As<ushort, TOut>(rawValue.ToUInt16(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(int))
            value = As<int, TOut>(rawValue.ToInt32(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(uint))
            value = As<uint, TOut>(rawValue.ToUInt32(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(long))
            value = As<long, TOut>(rawValue.ToInt64(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(ulong))
            value = As<ulong, TOut>(rawValue.ToUInt64(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(ulong))
            value = As<ulong, TOut>(rawValue.ToUInt64(CultureInfo.InvariantCulture));
        else if (typeof(TOut) == typeof(string))
        {
            value = As<string, TOut>(rawValue.ToString(CultureInfo.InvariantCulture));
            isNull = value != null;
        }
        else
        {
            isNull = true;
            value = default;
            return false;
        }

        return true;
    }

    public static bool TryConvert<TIn, TOut>(in FileEvaluationContext ctx, ISpecDynamicValue arg, out TOut? value, out bool isNull) where TIn : IConvertible
    {
        if (arg.TryEvaluateValue(in ctx, out TIn? rawValue, out isNull))
        {
            return TryConvert(rawValue, isNull, out value, out isNull);
        }

        value = default;
        isNull = true;
        return false;
    }
}

public abstract class SpecDynamicEquationTreeValue : ISpecDynamicValue
{
    public ISpecPropertyType ValueType { get; }
    public abstract string FunctionName { get; }

    protected SpecDynamicEquationTreeValue(ISpecPropertyType valueType)
    {
        ValueType = valueType;
    }

    protected static string ArgToString(ISpecDynamicValue arg)
    {
        string? prefix;
        switch (arg)
        {
            case DataRef:
                prefix = "#";
                break;

            case PropertyRef:
                prefix = "$";
                break;

            default:
                prefix = null;
                break;
        }

        string str = arg.ToString();
        if (str.IndexOf(' ') >= 0)
            return $"{prefix}({str})";

        return prefix + str;
    }

    public abstract bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull);

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = null;

        TryEvaluateBoxedValueVisitor visitor = new TryEvaluateBoxedValueVisitor
        {
            Context = ctx,
            Tree = this
        };

        ValueType.Visit(ref visitor);

        value = visitor.Value;
        return visitor.HasValue;
    }

    private struct TryEvaluateBoxedValueVisitor : ISpecPropertyTypeVisitor
    {
        public FileEvaluationContext Context;
        public SpecDynamicEquationTreeValue Tree;
        public bool HasValue;
        public object? Value;

        public void Visit<T>(ISpecPropertyType<T> type) where T : IEquatable<T>
        {
            if (HasValue)
                return;
            HasValue = Tree.TryEvaluateValue(in Context, out T? value, out bool isNull);

            if (HasValue)
                Value = isNull ? null : value;
        }
    }

    private bool EvaluateCondition<T>(in FileEvaluationContext ctx, in SpecCondition condition, T value)
    {
        if (!TryEvaluateValue(in ctx, out T? val, out bool isNull))
        {
            return condition.EvaluateNulls(isNull, false);
        }

        return condition.Evaluate(val, value, ctx.Information.Information);
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        switch (condition.Comparand)
        {
            case IConvertible conv:
                TypeCode tc = conv.GetTypeCode();
                switch (tc)
                {
                    case TypeCode.Boolean:
                        return EvaluateCondition(in ctx, in condition, (bool)condition.Comparand);

                    case TypeCode.Byte:
                        return EvaluateCondition(in ctx, in condition, (byte)condition.Comparand);

                    case TypeCode.Char:
                        return EvaluateCondition(in ctx, in condition, (char)condition.Comparand);

                    case TypeCode.DateTime:
                        return EvaluateCondition(in ctx, in condition, (DateTime)condition.Comparand);

                    case TypeCode.Decimal:
                        return EvaluateCondition(in ctx, in condition, (decimal)condition.Comparand);

                    case TypeCode.Double:
                        return EvaluateCondition(in ctx, in condition, (double)condition.Comparand);

                    case TypeCode.Int16:
                        return EvaluateCondition(in ctx, in condition, (short)condition.Comparand);

                    case TypeCode.Int32:
                        return EvaluateCondition(in ctx, in condition, (int)condition.Comparand);

                    case TypeCode.Int64:
                        return EvaluateCondition(in ctx, in condition, (long)condition.Comparand);

                    case TypeCode.SByte:
                        return EvaluateCondition(in ctx, in condition, (sbyte)condition.Comparand);

                    case TypeCode.Single:
                        return EvaluateCondition(in ctx, in condition, (float)condition.Comparand);

                    case TypeCode.String:
                        string str = (string)condition.Comparand;
                        if (ValueType.ValueType == typeof(Vector2))
                        {
                            if (KnownTypeValueHelper.TryParseVector2Components(str.AsSpan(), out Vector2 v2))
                            {
                                return EvaluateCondition(in ctx, in condition, v2);
                            }
                        }
                        else if (ValueType.ValueType == typeof(Vector3))
                        {
                            if (KnownTypeValueHelper.TryParseVector3Components(str.AsSpan(), out Vector3 v3))
                            {
                                return EvaluateCondition(in ctx, in condition, v3);
                            }
                        }
                        else if (ValueType.ValueType == typeof(Vector4))
                        {
                            if (KnownTypeValueHelper.TryParseVector4Components(str.AsSpan(), out Vector4 v4))
                            {
                                return EvaluateCondition(in ctx, in condition, v4);
                            }
                        }
                        else if (ValueType.ValueType == typeof(Color32))
                        {
                            if (KnownTypeValueHelper.TryParseColorHex(str.AsSpan(), out Color32 clr, allowAlpha: true))
                            {
                                return EvaluateCondition(in ctx, in condition, clr);
                            }
                        }
                        else if (ValueType.ValueType == typeof(Color))
                        {
                            if (KnownTypeValueHelper.TryParseColorHex(str.AsSpan(), out Color32 clr, allowAlpha: true))
                            {
                                return EvaluateCondition(in ctx, in condition, (Color)clr);
                            }
                        }
                        else if (ValueType.ValueType == typeof(Guid))
                        {
                            if (KnownTypeValueHelper.TryParseGuid(str, out Guid guid))
                            {
                                return EvaluateCondition(in ctx, in condition, guid);
                            }
                        }

                        return EvaluateCondition(in ctx, in condition, str);

                    case TypeCode.UInt16:
                        return EvaluateCondition(in ctx, in condition, (ushort)condition.Comparand);

                    case TypeCode.UInt32:
                        return EvaluateCondition(in ctx, in condition, (uint)condition.Comparand);

                    case TypeCode.UInt64:
                        return EvaluateCondition(in ctx, in condition, (ulong)condition.Comparand);
                }

                break;

            case GuidOrId { IsId: true } guidOrId:
                return EvaluateCondition(in ctx, in condition, guidOrId.Id);

            case Vector2 v2:
                return EvaluateCondition(in ctx, in condition, v2);

            case Vector3 v3:
                return EvaluateCondition(in ctx, in condition, v3);

            case Vector4 v4:
                return EvaluateCondition(in ctx, in condition, v4);

            case Color clr:
                return EvaluateCondition(in ctx, in condition, clr);

            case Color32 clr32:
                return EvaluateCondition(in ctx, in condition, clr32);
        }

        return condition.EvaluateNulls(true, true);
    }

    public abstract ISpecDynamicValue GetArgument(int index);

    public bool TryGetArgumentValue<TValue>(in FileEvaluationContext ctx, int index, out TValue? value, out bool isNull)
    {
        return TryGetArgumentValue(in ctx, GetArgument(index), out value, out isNull);
    }

    public bool TryGetArgumentValue<TValue>(in FileEvaluationContext ctx, ISpecDynamicValue arg, out TValue? value, out bool isNull)
    {
        ISpecPropertyType? type = arg.ValueType;

        if (type == null || type.ValueType == typeof(TValue))
        {
            return arg.TryEvaluateValue(in ctx, out value, out isNull);
        }

        return SpecDynamicValue.TryGetValueAsType(in ctx, arg, out value, out isNull);
    }


    public abstract void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options);
}

public class SpecDynamicEquationTreeBinaryValue : SpecDynamicEquationTreeValue
{
    private static readonly string[] OperationFunctionNames =
    [
        "ADD",          // Add
        "SUB",          // Subtract
        "MUL",          // Multiply
        "DIV",          // Divide
        "MOD",          // Modulo
        "MIN",          // Minimum
        "MAX",          // Maximum
        "AVG",          // Average
        "CAT",          // Concat
        "POW"           // Power
    ];

    public ISpecDynamicValue Left { get; }
    public ISpecDynamicValue Right { get; }
    public SpecDynamicEquationTreeBinaryOperation Operation { get; }
    public override string FunctionName { get; }

    public SpecDynamicEquationTreeBinaryValue(ISpecDynamicValue left, ISpecDynamicValue right, SpecDynamicEquationTreeBinaryOperation operation, ISpecPropertyType? expectedType)
        : base(SpecDynamicEquationTreeValueHelpers.GetValueType(left, right, operation, expectedType))
    {
        Left = left;
        Right = right;
        Operation = operation;
        FunctionName = GetOperationName(operation);
    }

    public static bool TryParseOperation(ReadOnlySpan<char> span, out SpecDynamicEquationTreeBinaryOperation operation)
    {
        for (int i = 0; i < OperationFunctionNames.Length; ++i)
        {
            if (!span.Equals(OperationFunctionNames[i].AsSpan(), StringComparison.OrdinalIgnoreCase))
                continue;

            operation = (SpecDynamicEquationTreeBinaryOperation)i;
            return true;
        }

        operation = (SpecDynamicEquationTreeBinaryOperation)(-1);
        return false;
    }

    public static string GetOperationName(SpecDynamicEquationTreeBinaryOperation operation)
    {
        return OperationFunctionNames[(int)operation];
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull) where TValue : default
    {
        bool leftIsNull, rightIsNull;
        if (typeof(TValue) == typeof(string) && Operation == SpecDynamicEquationTreeBinaryOperation.Concat)
        {
            if (TryGetArgumentValue(in ctx, 0, out string? leftStr, out leftIsNull)
                && TryGetArgumentValue(in ctx, 0, out string? rightStr, out rightIsNull))
            {
                if (leftStr == null || leftIsNull)
                    leftStr = string.Empty;
                if (rightStr == null || rightIsNull)
                    rightStr = string.Empty;

                isNull = leftIsNull && rightIsNull;
                value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<string, TValue>(leftStr + rightStr);
                return true;
            }

            value = default;
            isNull = true;
            return false;
        }

        if (ValueType is IVectorSpecPropertyType<TValue> vectorType)
        {
            value = default;
            isNull = true;

            ISpecDynamicValue leftArg = GetArgument(0);
            ISpecDynamicValue rightArg = GetArgument(1);
            bool leftIsVector = leftArg.ValueType is IVectorSpecPropertyType<TValue>;
            bool rightIsVector = leftArg.ValueType is IVectorSpecPropertyType<TValue>;
            TValue? leftVector = default, rightVector = default;
            double leftNum = 0, rightNum = 0;
            if (leftIsVector)
            {
                if (!TryGetArgumentValue(in ctx, leftArg, out leftVector, out leftIsNull))
                    return false;
            }
            else if (!TryGetArgumentValue(in ctx, leftArg, out leftNum, out leftIsNull))
                return false;
            if (rightIsVector)
            {
                if (!TryGetArgumentValue(in ctx, rightArg, out rightVector, out rightIsNull))
                    return false;
            }
            else if (!TryGetArgumentValue(in ctx, rightArg, out rightNum, out rightIsNull))
                return false;

            isNull = leftIsNull && rightIsNull;

            if (leftIsNull)
            {
                if (leftIsVector)
                    leftVector = default;
                else
                    leftNum = 0;
            }
            if (rightIsNull)
            {
                if (rightIsVector)
                    rightVector = default;
                else
                    rightNum = 0;
            }

            TValue? v;
            if (leftIsVector ^ rightIsVector)
            {
                TValue vector = vectorType.Construct(leftIsVector ? rightNum : leftNum);
                v = Operation switch
                {
                    SpecDynamicEquationTreeBinaryOperation.Add
                        => leftIsVector ? vectorType.Add(leftVector, vector) : vectorType.Add(vector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Subtract
                        => leftIsVector ? vectorType.Subtract(leftVector, vector) : vectorType.Subtract(vector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Multiply
                        => leftIsVector ? vectorType.Multiply(leftVector, vector) : vectorType.Multiply(vector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Divide
                        => leftIsVector ? vectorType.Divide(leftVector, vector) : vectorType.Divide(vector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Minimum
                        => leftIsVector ? vectorType.Min(leftVector, vector) : vectorType.Min(vector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Maximum
                        => leftIsVector ? vectorType.Max(leftVector, vector) : vectorType.Max(vector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Average
                        => leftIsVector ? vectorType.Avg(leftVector, vector) : vectorType.Avg(vector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Power
                        => leftIsVector ? vectorType.Power(leftVector, vector) : vectorType.Power(vector, rightVector),
                    _ => leftIsVector ? leftVector : vector
                };
            }
            else
            {
                if (!leftIsVector /* && !rightIsVector */) // implied
                {
                    leftVector = vectorType.Construct(leftNum);
                    rightVector = vectorType.Construct(rightNum);
                }

                v = Operation switch
                {
                    SpecDynamicEquationTreeBinaryOperation.Add => vectorType.Add(leftVector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Subtract => vectorType.Subtract(leftVector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Multiply => vectorType.Multiply(leftVector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Divide => vectorType.Divide(leftVector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Minimum => vectorType.Min(leftVector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Maximum => vectorType.Max(leftVector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Average => vectorType.Avg(leftVector, rightVector),
                    SpecDynamicEquationTreeBinaryOperation.Power => vectorType.Power(leftVector, rightVector),
                    _ => leftVector
                };
            }

            return SpecDynamicEquationTreeValueHelpers.TryConvertVector(v, isNull, out value, out isNull);
        }
        else
        {
            if (!TryGetArgumentValue(in ctx, 0, out double leftNum, out leftIsNull)
                || !TryGetArgumentValue(in ctx, 1, out double rightNum, out rightIsNull))
            {
                value = default;
                isNull = true;

                return false;
            }

            isNull = leftIsNull && rightIsNull;

            if (leftIsNull)
                leftNum = 0;
            if (rightIsNull)
                rightNum = 0;

            double v = Operation switch
            {
                SpecDynamicEquationTreeBinaryOperation.Add => leftNum + rightNum,
                SpecDynamicEquationTreeBinaryOperation.Subtract => leftNum - rightNum,
                SpecDynamicEquationTreeBinaryOperation.Multiply => leftNum * rightNum,
                SpecDynamicEquationTreeBinaryOperation.Divide => leftNum / rightNum,
                SpecDynamicEquationTreeBinaryOperation.Minimum => Math.Min(leftNum, rightNum),
                SpecDynamicEquationTreeBinaryOperation.Maximum => Math.Max(leftNum, rightNum),
                SpecDynamicEquationTreeBinaryOperation.Average => (leftNum + rightNum) / 2d,
                SpecDynamicEquationTreeBinaryOperation.Power => Math.Pow(leftNum, rightNum),
                _ => leftNum
            };

            return SpecDynamicEquationTreeValueHelpers.TryConvert(v, isNull, out value, out isNull);
        }
    }

    public override ISpecDynamicValue GetArgument(int index)
    {
        if (index is not 0 and not 1)
            throw new ArgumentOutOfRangeException(nameof(index));

        return index == 0 ? Left : Right;
    }

    public override void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        writer.WriteStringValue(ToString());
    }

    public override string ToString() => $"={FunctionName}({ArgToString(Left)} {ArgToString(Right)})";
}

public enum SpecDynamicEquationTreeBinaryOperation
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Minimum,
    Maximum,
    Average,
    Concat,
    Power
}


public class SpecDynamicEquationTreeUnaryValue : SpecDynamicEquationTreeValue
{
    private static readonly string[] OperationFunctionNames =
    [
        "ABS",          // Absolute
        "ROUND",        // Round
        "FLOOR",        // Floor
        "CEIL",         // Ceiling
        "SINR",         // SineRad
        "COSR",         // CosineRad
        "TANR",         // TangentRad
        "ASINR",        // ArcSineRad
        "ACOSR",        // ArcCosineRad
        "ATANR",        // ArcTangentRad
        "SIND",         // SineDeg
        "COSD",         // CosineDeg
        "TAND",         // TangentDeg
        "ASIND",        // ArcSineDeg
        "ACOSD",        // ArcCosineDeg
        "ATAND",        // ArcTangentDeg
        "SQRT"          // SquareRoot
    ];

    public ISpecDynamicValue Argument { get; }
    public SpecDynamicEquationTreeUnaryOperation Operation { get; }
    public override string FunctionName { get; }

    public SpecDynamicEquationTreeUnaryValue(ISpecDynamicValue argument, SpecDynamicEquationTreeUnaryOperation operation, ISpecPropertyType? expectedType)
        : base(SpecDynamicEquationTreeValueHelpers.GetValueType(argument, operation, expectedType))
    {
        Argument = argument;
        Operation = operation;
        FunctionName = GetOperationName(operation);
    }

    public static bool TryParseOperation(ReadOnlySpan<char> span, out SpecDynamicEquationTreeUnaryOperation operation)
    {
        for (int i = 0; i < OperationFunctionNames.Length; ++i)
        {
            if (!span.Equals(OperationFunctionNames[i].AsSpan(), StringComparison.OrdinalIgnoreCase))
                continue;

            operation = (SpecDynamicEquationTreeUnaryOperation)i;
            return true;
        }

        operation = (SpecDynamicEquationTreeUnaryOperation)(-1);
        return false;
    }

    public static string GetOperationName(SpecDynamicEquationTreeUnaryOperation operation)
    {
        return OperationFunctionNames[(int)operation];
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull) where TValue : default
    {
        if (Operation
            is >= SpecDynamicEquationTreeUnaryOperation.SineRad
            and <= SpecDynamicEquationTreeUnaryOperation.SquareRoot)
        {
            return TryEvaluateFuncValue(in ctx, out value, out isNull);
        }

        if (!TryGetArgumentValue(in ctx, 0, out TValue? argValue, out bool argIsNull))
        {
            value = default;
            isNull = argIsNull;
            return false;
        }

        if (argIsNull)
        {
            isNull = true;
            value = default;
            return true;
        }

        if (ValueType is IVectorSpecPropertyType<TValue> vectorType)
        {
            TValue? v = Operation switch
            {
                SpecDynamicEquationTreeUnaryOperation.Absolute => vectorType.Absolute(argValue),
                SpecDynamicEquationTreeUnaryOperation.Round => vectorType.Round(argValue),
                SpecDynamicEquationTreeUnaryOperation.Floor => vectorType.Floor(argValue),
                SpecDynamicEquationTreeUnaryOperation.Ceiling => vectorType.Ceiling(argValue),
                _ => argValue
            };

            return SpecDynamicEquationTreeValueHelpers.TryConvertVector(v, argIsNull, out value, out isNull);
        }

        // none of the operations have any effect on unsigned integers at this point
        if (typeof(TValue) == typeof(uint) || typeof(TValue) == typeof(ulong) || typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(ushort))
        {
            value = argValue;
            isNull = argIsNull;
            return true;
        }

        isNull = false;
        if (typeof(TValue) == typeof(sbyte) || typeof(TValue) == typeof(short) || typeof(TValue) == typeof(int))
        {
            int argValueAsInt32;
            if (typeof(TValue) == typeof(byte))
            {
                argValueAsInt32 = Unsafe.As<TValue, byte>(ref argValue!);
            }
            else if (typeof(TValue) == typeof(sbyte))
            {
                argValueAsInt32 = Unsafe.As<TValue, sbyte>(ref argValue!);
            }
            else if (typeof(TValue) == typeof(ushort))
            {
                argValueAsInt32 = Unsafe.As<TValue, ushort>(ref argValue!);
            }
            else if (typeof(TValue) == typeof(short))
            {
                argValueAsInt32 = Unsafe.As<TValue, short>(ref argValue!);
            }
            else
            {
                argValueAsInt32 = Unsafe.As<TValue, int>(ref argValue!);
            }

            int v = Operation switch
            {
                SpecDynamicEquationTreeUnaryOperation.Absolute => Math.Abs(argValueAsInt32),
                _ => argValueAsInt32,
            };

            return SpecDynamicEquationTreeValueHelpers.TryConvert(v, argIsNull, out value, out isNull);
        }

        if (typeof(TValue) == typeof(long))
        {
            long argValueAsInt64 = Unsafe.As<TValue, long>(ref argValue!);

            long v = Operation switch
            {
                SpecDynamicEquationTreeUnaryOperation.Absolute => Math.Abs(argValueAsInt64),
                _ => argValueAsInt64,
            };

            return SpecDynamicEquationTreeValueHelpers.TryConvert(v, argIsNull, out value, out isNull);
        }

        if (typeof(TValue) == typeof(float))
        {
            float argValueAsFloat = Unsafe.As<TValue, float>(ref argValue!);

            float v = Operation switch
            {
                SpecDynamicEquationTreeUnaryOperation.Absolute => MathF.Abs(argValueAsFloat),
                SpecDynamicEquationTreeUnaryOperation.Round => MathF.Round(argValueAsFloat),
                SpecDynamicEquationTreeUnaryOperation.Floor => MathF.Floor(argValueAsFloat),
                SpecDynamicEquationTreeUnaryOperation.Ceiling => MathF.Ceiling(argValueAsFloat),
                _ => argValueAsFloat,
            };

            value = Unsafe.As<float, TValue>(ref v);
            return true;
        }

        if (typeof(TValue) == typeof(double))
        {
            double argValueAsDouble = Unsafe.As<TValue, double>(ref argValue!);

            double v = Operation switch
            {
                SpecDynamicEquationTreeUnaryOperation.Absolute => Math.Abs(argValueAsDouble),
                SpecDynamicEquationTreeUnaryOperation.Round => Math.Round(argValueAsDouble),
                SpecDynamicEquationTreeUnaryOperation.Floor => Math.Floor(argValueAsDouble),
                SpecDynamicEquationTreeUnaryOperation.Ceiling => Math.Ceiling(argValueAsDouble),
                _ => argValueAsDouble,
            };

            value = Unsafe.As<double, TValue>(ref v);
            return true;
        }

        if (typeof(TValue) == typeof(decimal))
        {
            decimal argValueAsDecimal = Unsafe.As<TValue, decimal>(ref argValue!);

            decimal v = Operation switch
            {
                SpecDynamicEquationTreeUnaryOperation.Absolute => argValueAsDecimal < 0 ? -argValueAsDecimal : argValueAsDecimal,
                SpecDynamicEquationTreeUnaryOperation.Round => decimal.Round(argValueAsDecimal),
                SpecDynamicEquationTreeUnaryOperation.Floor => decimal.Floor(argValueAsDecimal),
                SpecDynamicEquationTreeUnaryOperation.Ceiling => decimal.Ceiling(argValueAsDecimal),
                _ => argValueAsDecimal,
            };

            return SpecDynamicEquationTreeValueHelpers.TryConvert(v, argIsNull, out value, out isNull);
        }

        if (typeof(TValue) == typeof(string))
        {
            string? str = Unsafe.As<TValue?, string?>(ref argValue);
            if (str == null)
            {
                isNull = true;
                value = default;
                return true;
            }

            // unsigned integers aren't affected
            if (ulong.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
            {
                value = argValue;
                isNull = argIsNull;
                return true;
            }

            if (long.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out long argValueAsInt64))
            {
                argValueAsInt64 = Operation switch
                {
                    SpecDynamicEquationTreeUnaryOperation.Absolute => Math.Abs(argValueAsInt64),
                    _ => argValueAsInt64
                };
                return SpecDynamicEquationTreeValueHelpers.TryConvert(argValueAsInt64, argIsNull, out value, out isNull);
            }

            if (double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out double argValueAsDouble))
            {
                argValueAsDouble = Operation switch
                {
                    SpecDynamicEquationTreeUnaryOperation.Absolute => Math.Abs(argValueAsDouble),
                    SpecDynamicEquationTreeUnaryOperation.Round => Math.Round(argValueAsDouble),
                    SpecDynamicEquationTreeUnaryOperation.Floor => Math.Floor(argValueAsDouble),
                    SpecDynamicEquationTreeUnaryOperation.Ceiling => Math.Ceiling(argValueAsDouble),
                    _ => argValueAsDouble,
                };
                return SpecDynamicEquationTreeValueHelpers.TryConvert(argValueAsDouble, argIsNull, out value, out isNull);
            }
        }

        value = argValue;
        isNull = argIsNull;
        return true;
    }

    private bool TryEvaluateFuncValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        if (ValueType is IVectorSpecPropertyType<TValue> vectorType)
        {
            ISpecDynamicValue arg = GetArgument(0);
            TValue? vector;
            bool vectorIsNull;
            if (arg.ValueType is IVectorSpecPropertyType<TValue>)
            {
                if (!TryGetArgumentValue(in ctx, arg, out vector, out vectorIsNull))
                {
                    value = vector;
                    isNull = true;
                    return false;
                }
            }
            else
            {
                if (!TryGetArgumentValue(in ctx, arg, out double val, out vectorIsNull))
                {
                    value = default;
                    isNull = true;
                    return false;
                }

                vector = vectorIsNull ? default : vectorType.Construct(val);
            }

            if (vectorIsNull)
            {
                isNull = true;
                value = vector;
                return true;
            }

            TValue? v;
            if (Operation < SpecDynamicEquationTreeUnaryOperation.SquareRoot)
            {
                bool deg = Operation >= SpecDynamicEquationTreeUnaryOperation.SineDeg;
                int op = deg
                    ? (Operation - SpecDynamicEquationTreeUnaryOperation.SineDeg)
                    : (Operation - SpecDynamicEquationTreeUnaryOperation.SineRad);

                v = vectorType.TrigOperation(vector, op, deg);
            }
            else
            {
                v = Operation switch
                {
                    SpecDynamicEquationTreeUnaryOperation.SquareRoot => vectorType.Sqrt(vector),
                    _ => vector
                };
            }
            
            return SpecDynamicEquationTreeValueHelpers.TryConvertVector(v, vectorIsNull, out value, out isNull);
        }

        isNull = false;
        value = default;

        if (typeof(TValue) == typeof(float) && ValueType.ValueType == typeof(float))
        {
            if (!TryGetArgumentValue(in ctx, 0, out float r32, out bool argIsNull))
            {
                isNull = argIsNull;
                return false;
            }

            if (isNull)
                return true;

            float v = Operation switch
            {
                SpecDynamicEquationTreeUnaryOperation.SineRad => MathF.Sin(r32),
                SpecDynamicEquationTreeUnaryOperation.CosineRad => MathF.Cos(r32),
                SpecDynamicEquationTreeUnaryOperation.TangentRad => MathF.Tan(r32),
                SpecDynamicEquationTreeUnaryOperation.ArcSineRad => MathF.Asin(r32),
                SpecDynamicEquationTreeUnaryOperation.ArcCosineRad => MathF.Acos(r32),
                SpecDynamicEquationTreeUnaryOperation.ArcTangentRad => MathF.Atan(r32),
                SpecDynamicEquationTreeUnaryOperation.SineDeg => MathF.Sin(r32 * (MathF.PI / 180f)),
                SpecDynamicEquationTreeUnaryOperation.CosineDeg => MathF.Cos(r32 * (MathF.PI / 180f)),
                SpecDynamicEquationTreeUnaryOperation.TangentDeg => MathF.Tan(r32 * (MathF.PI / 180f)),
                SpecDynamicEquationTreeUnaryOperation.ArcSineDeg => MathF.Asin(r32) * (180f / MathF.PI),
                SpecDynamicEquationTreeUnaryOperation.ArcCosineDeg => MathF.Acos(r32) * (180 / MathF.PI),
                SpecDynamicEquationTreeUnaryOperation.ArcTangentDeg => MathF.Atan(r32) * (180 / MathF.PI),
                SpecDynamicEquationTreeUnaryOperation.SquareRoot => MathF.Sqrt(r32),
                _ => r32
            };

            if (typeof(TValue) == typeof(float))
            {
                value = Unsafe.As<float, TValue>(ref v);
            }
            else if (!SpecDynamicEquationTreeValueHelpers.TryConvert(v, false, out value, out isNull))
            {
                isNull = true;
                return false;
            }
        }
        else // double or other
        {
            if (!TryGetArgumentValue(in ctx, 0, out double r64, out bool argIsNull))
            {
                isNull = argIsNull;
                return false;
            }

            if (isNull)
                return true;

            double v = Operation switch
            {
                SpecDynamicEquationTreeUnaryOperation.SineRad => Math.Sin(r64),
                SpecDynamicEquationTreeUnaryOperation.CosineRad => Math.Cos(r64),
                SpecDynamicEquationTreeUnaryOperation.TangentRad => Math.Tan(r64),
                SpecDynamicEquationTreeUnaryOperation.ArcSineRad => Math.Asin(r64),
                SpecDynamicEquationTreeUnaryOperation.ArcCosineRad => Math.Acos(r64),
                SpecDynamicEquationTreeUnaryOperation.ArcTangentRad => Math.Atan(r64),
                SpecDynamicEquationTreeUnaryOperation.SineDeg => Math.Sin(r64 * (Math.PI / 180)),
                SpecDynamicEquationTreeUnaryOperation.CosineDeg => Math.Cos(r64 * (Math.PI / 180)),
                SpecDynamicEquationTreeUnaryOperation.TangentDeg => Math.Tan(r64 * (Math.PI / 180)),
                SpecDynamicEquationTreeUnaryOperation.ArcSineDeg => Math.Asin(r64) * (180 / Math.PI),
                SpecDynamicEquationTreeUnaryOperation.ArcCosineDeg => Math.Acos(r64) * (180 / Math.PI),
                SpecDynamicEquationTreeUnaryOperation.ArcTangentDeg => Math.Atan(r64) * (180 / Math.PI),
                SpecDynamicEquationTreeUnaryOperation.SquareRoot => Math.Sqrt(r64),
                _ => r64
            };

            if (typeof(TValue) == typeof(double))
            {
                value = Unsafe.As<double, TValue>(ref v);
            }
            else if (!SpecDynamicEquationTreeValueHelpers.TryConvert(v, false, out value, out isNull))
            {
                isNull = true;
                return false;
            }
        }

        return true;
    }

    public override ISpecDynamicValue GetArgument(int index)
    {
        if (index != 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        return Argument;
    }

    public override void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        writer.WriteStringValue(ToString());
    }

    public override string ToString() => $"={FunctionName}({ArgToString(Argument)})";
}

public enum SpecDynamicEquationTreeUnaryOperation
{
    Absolute,
    Round,
    Floor,
    Ceiling,
    SineRad,
    CosineRad,
    TangentRad,
    ArcSineRad,
    ArcCosineRad,
    ArcTangentRad,
    SineDeg,
    CosineDeg,
    TangentDeg,
    ArcSineDeg,
    ArcCosineDeg,
    ArcTangentDeg,
    SquareRoot
}

public class SpecDynamicEquationTreeTertiaryValue : SpecDynamicEquationTreeValue
{
    private static readonly string[] OperationFunctionNames =
    [
        "REP",                    // Replace
        "CUSTOM_BALLISTIC_GRAV"   // BallisticGravityMultiplierCalculation
    ];

    public ISpecDynamicValue Arg1 { get; }
    public ISpecDynamicValue Arg2 { get; }
    public ISpecDynamicValue Arg3 { get; }
    public SpecDynamicEquationTreeTertiaryOperation Operation { get; }
    public override string FunctionName { get; }

    public SpecDynamicEquationTreeTertiaryValue(
        ISpecDynamicValue arg1,
        ISpecDynamicValue arg2,
        ISpecDynamicValue arg3,
        SpecDynamicEquationTreeTertiaryOperation operation,
        ISpecPropertyType? expectedType)
        : base(SpecDynamicEquationTreeValueHelpers.GetValueType(arg1, arg2, arg3, operation, expectedType))
    {
        Arg1 = arg1;
        Arg2 = arg2;
        Arg3 = arg3;
        Operation = operation;
        FunctionName = GetOperationName(operation);
    }

    public static bool TryParseOperation(ReadOnlySpan<char> span, out SpecDynamicEquationTreeTertiaryOperation operation)
    {
        for (int i = 0; i < OperationFunctionNames.Length; ++i)
        {
            if (!span.Equals(OperationFunctionNames[i].AsSpan(), StringComparison.OrdinalIgnoreCase))
                continue;

            operation = (SpecDynamicEquationTreeTertiaryOperation)i;
            return true;
        }

        operation = (SpecDynamicEquationTreeTertiaryOperation)(-1);
        return false;
    }

    public static string GetOperationName(SpecDynamicEquationTreeTertiaryOperation operation)
    {
        return OperationFunctionNames[(int)operation];
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull) where TValue : default
    {
        if (Operation == SpecDynamicEquationTreeTertiaryOperation.BallisticGravityMultiplierCalculation)
        {
            return TryEvaluateBallisticGravityMultiplier(in ctx, out value, out isNull);
        }

        value = default;
        isNull = true;

        if (Operation != SpecDynamicEquationTreeTertiaryOperation.Replace || typeof(TValue) != typeof(string))
            return false;

        if (!TryGetArgumentValue(in ctx, 0, out string? arg1, out bool arg1Null)
            || !TryGetArgumentValue(in ctx, 1, out string? arg2, out bool arg2Null)
            || !TryGetArgumentValue(in ctx, 2, out string? arg3, out bool arg3Null))
            return false;

        if (arg1Null && arg2Null && arg3Null)
            return true;

        string res;

        if (string.IsNullOrEmpty(arg1))
            res = string.Empty;
        else if (string.IsNullOrEmpty(arg2))
            res = arg1!;
        else
            res = arg1!.Replace(arg2, arg3 ?? string.Empty);

        value = SpecDynamicEquationTreeValueHelpers.As<string, TValue>(res);
        isNull = false;
        return true;
    }

    private bool TryEvaluateBallisticGravityMultiplier<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        if (!TryGetArgumentValue(in ctx, 0, out float ballisticTravel, out bool btIsNull)
            || !TryGetArgumentValue(in ctx, 1, out byte ballisticSteps, out bool bsIsNull)
            || !TryGetArgumentValue(in ctx, 1, out float ballisticDrop, out bool bdIsNull))
        {
            isNull = true;
            value = default;
            return false;
        }

        if (btIsNull || bsIsNull || bdIsNull)
        {
            isNull = true;
            value = default;
            return true;
        }

        isNull = false;

        // copied from ItemGunAsset.PopulateAsset
        float totalBallisticRise = 0.0f;
        Vector2 right = new Vector2(1f, 0f);
        for (int index = 0; index < ballisticSteps; ++index)
        {
            totalBallisticRise += right.Y * ballisticTravel;
            right.Y -= ballisticDrop;
            right = Vector2.Normalize(right);
        }

        float totalTimeSec = ballisticSteps * 0.02f;
        float bulletGravityMultiplier = 2f * totalBallisticRise / (totalTimeSec * totalTimeSec) / -9.81f;

        if (typeof(TValue) == typeof(float))
        {
            value = Unsafe.As<float, TValue>(ref bulletGravityMultiplier);
            return true;
        }

        return SpecDynamicEquationTreeValueHelpers.TryConvert(bulletGravityMultiplier, false, out value, out isNull);
    }

    public override ISpecDynamicValue GetArgument(int index)
    {
        return index switch
        {
            0 => Arg1,
            1 => Arg2,
            2 => Arg3,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }

    public override void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        writer.WriteStringValue(ToString());
    }

    public override string ToString() => $"={FunctionName}({ArgToString(Arg1)} {ArgToString(Arg2)} {ArgToString(Arg3)})";
}

public enum SpecDynamicEquationTreeTertiaryOperation
{
    Replace,
    BallisticGravityMultiplierCalculation
}
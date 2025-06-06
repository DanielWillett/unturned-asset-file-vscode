using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

internal static class SpecDynamicEquationTreeValueHelpers
{
    public static ISpecPropertyType GetValueType(ISpecDynamicValue left, ISpecDynamicValue? right, SpecDynamicEquationTreeBinaryOperation operation)
    {
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

    public static ISpecPropertyType GetValueType(ISpecDynamicValue argument, SpecDynamicEquationTreeUnaryOperation operation)
    {
        // may use later
        _ = operation;
        return argument.ValueType ?? KnownTypes.Float64;
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

    public static bool TryConvert<TIn, TOut>(TIn? rawValue, bool rawIsNull, out TOut? value, out bool isNull) where TIn : IConvertible
    {
        if (rawIsNull || rawValue == null)
        {
            isNull = true;
            value = default;
            return true;
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

    public abstract bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull);
    public abstract bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value);

    private bool EvaluateCondition<T>(in FileEvaluationContext ctx, in SpecCondition condition, T value) where T : IConvertible
    {
        if (!TryEvaluateValue(in ctx, out T? val, out bool isNull))
        {
            return condition.Operation.EvaluateNulls(isNull, false);
        }

        return condition.Operation.Evaluate(val, value, ctx.Information.Information);
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is IConvertible conv)
        {
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
                    return EvaluateCondition(in ctx, in condition, (string)condition.Comparand);

                case TypeCode.UInt16:
                    return EvaluateCondition(in ctx, in condition, (ushort)condition.Comparand);

                case TypeCode.UInt32:
                    return EvaluateCondition(in ctx, in condition, (uint)condition.Comparand);

                case TypeCode.UInt64:
                    return EvaluateCondition(in ctx, in condition, (ulong)condition.Comparand);
            }
        }
        else if (condition.Comparand is GuidOrId { IsId: true } guidOrId)
        {
            return EvaluateCondition(in ctx, in condition, guidOrId.Id);
        }

        return condition.Operation.EvaluateNulls(true, true);
    }

    public abstract ISpecDynamicValue GetArgument(int index);

    public bool TryGetArgumentValue<TValue>(in FileEvaluationContext ctx, int index, out TValue? value, out bool isNull)
    {
        ISpecDynamicValue arg = GetArgument(index);
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
        "AVG"           // Average
    ];

    public ISpecDynamicValue Left { get; }
    public ISpecDynamicValue Right { get; }
    public SpecDynamicEquationTreeBinaryOperation Operation { get; }
    public override string FunctionName { get; }

    public SpecDynamicEquationTreeBinaryValue(ISpecDynamicValue left, ISpecDynamicValue right, SpecDynamicEquationTreeBinaryOperation operation)
        : base(SpecDynamicEquationTreeValueHelpers.GetValueType(left, right, operation))
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
        value = default;
        isNull = true;

        if (!TryGetArgumentValue(in ctx, 0, out double leftNum, out bool leftIsNull)
            || !TryGetArgumentValue(in ctx, 1, out double rightNum, out bool rightIsNull))
            return false;

        if (leftIsNull && rightIsNull)
            isNull = true;
        
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
            _ => leftNum
        };

        return SpecDynamicEquationTreeValueHelpers.TryConvert(v, isNull, out value, out isNull);
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = null;

        if (!TryGetArgumentValue(in ctx, 0, out double leftNum, out bool leftIsNull)
            || !TryGetArgumentValue(in ctx, 1, out double rightNum, out bool rightIsNull))
            return false;

        if (leftIsNull && rightIsNull)
            return true;
        
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
            _ => leftNum
        };

        if (ValueType.ValueType == typeof(double))
        {
            value = v;
            return true;
        }

        try
        {
            value = Convert.ChangeType(v, ValueType.ValueType, CultureInfo.InvariantCulture);
            return true;
        }
        catch (InvalidCastException)
        {
            return false;
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

    public override string ToString() => $"={FunctionName}({Left} {Right})";
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
    Average
}


public class SpecDynamicEquationTreeUnaryValue : SpecDynamicEquationTreeValue
{
    private static readonly string[] OperationFunctionNames =
    [
        "ABS",          // Absolute
        "ROUND",        // Round
        "FLOOR",        // Floor
        "CEIL"          // Ceiling
    ];

    public ISpecDynamicValue Argument { get; }
    public SpecDynamicEquationTreeUnaryOperation Operation { get; }
    public override string FunctionName { get; }

    public SpecDynamicEquationTreeUnaryValue(ISpecDynamicValue argument, SpecDynamicEquationTreeUnaryOperation operation)
        : base(SpecDynamicEquationTreeValueHelpers.GetValueType(argument, operation))
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

        // none of the operations have any effect on unsigned integers at this point
        if (typeof(TValue) == typeof(uint) || typeof(TValue) == typeof(ulong) || typeof(TValue) == typeof(byte) || typeof(TValue) == typeof(ushort))
        {
            value = argValue;
            isNull = argIsNull;
            return true;
        }

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

        if (typeof(TValue) == typeof(float) || typeof(TValue) == typeof(double))
        {
            double argValueAsDouble = typeof(TValue) == typeof(float)
                ? Unsafe.As<TValue, float>(ref argValue!)
                : Unsafe.As<TValue, double>(ref argValue!);

            double v = Operation switch
            {
                SpecDynamicEquationTreeUnaryOperation.Absolute => Math.Abs(argValueAsDouble),
                SpecDynamicEquationTreeUnaryOperation.Round => Math.Round(argValueAsDouble),
                SpecDynamicEquationTreeUnaryOperation.Floor => Math.Floor(argValueAsDouble),
                SpecDynamicEquationTreeUnaryOperation.Ceiling => Math.Ceiling(argValueAsDouble),
                _ => argValueAsDouble,
            };

            return SpecDynamicEquationTreeValueHelpers.TryConvert(v, argIsNull, out value, out isNull);
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
            if (ulong.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong argValueAsUInt64))
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

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        Type? type = Argument.ValueType?.ValueType;

        if (type == null)
        {
            if (!TryEvaluateValue(in ctx, out double val, out bool isNull))
            {
                return Argument.TryEvaluateValue(in ctx, out value);
            }

            value = isNull ? null : val;
            return true;
        }

        if (type == typeof(double))
        {
            if (!TryEvaluateValue(in ctx, out double val, out bool isNull))
            {
                value = null;
                return false;
            }

            value = isNull ? null : val;
            return true;
        }
        if (type == typeof(int))
        {
            if (!TryEvaluateValue(in ctx, out int val, out bool isNull))
            {
                value = null;
                return false;
            }

            value = isNull ? null : val;
            return true;
        }
        if (type == typeof(float))
        {
            if (!TryEvaluateValue(in ctx, out float val, out bool isNull))
            {
                value = null;
                return false;
            }

            value = isNull ? null : val;
            return true;
        }

        // unsigned ints aren't affected by these operations
        if (type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong))
        {
            return Argument.TryEvaluateValue(in ctx, out value);
        }

        if (type == typeof(sbyte))
        {
            if (!TryEvaluateValue(in ctx, out sbyte val, out bool isNull))
            {
                value = null;
                return false;
            }

            value = isNull ? null : val;
            return true;
        }
        if (type == typeof(short))
        {
            if (!TryEvaluateValue(in ctx, out short val, out bool isNull))
            {
                value = null;
                return false;
            }

            value = isNull ? null : val;
            return true;
        }
        if (type == typeof(long))
        {
            if (!TryEvaluateValue(in ctx, out long val, out bool isNull))
            {
                value = null;
                return false;
            }

            value = isNull ? null : val;
            return true;
        }
        if (type == typeof(decimal))
        {
            if (!TryEvaluateValue(in ctx, out decimal val, out bool isNull))
            {
                value = null;
                return false;
            }

            value = isNull ? null : val;
            return true;
        }

        return Argument.TryEvaluateValue(in ctx, out value);
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

    public override string ToString() => $"={FunctionName}({Argument})";
}

public enum SpecDynamicEquationTreeUnaryOperation
{
    Absolute,
    Round,
    Floor,
    Ceiling
}
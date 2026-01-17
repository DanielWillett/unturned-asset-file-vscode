using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal ref struct ExpressionNodeParser : IDisposable
{
    private ExpressionTokenizer _tokenizer;
    private ExpressionNodeBuffer[]? _stack;
    private int _stackSize;
    private ExpressionNodeBuffer _current;
    private readonly int _flags;

    public bool SimplifyConstantExpressions => (_flags & 1) != 0;
    public bool LeaveTokenizerOpen => (_flags & 2) != 0;

    public ExpressionNodeParser(string expression, bool simplifyConstantExpressions = true)
    {
        _tokenizer = new ExpressionTokenizer(expression);
        _flags = simplifyConstantExpressions ? 1 : 0;
    }

    public ExpressionNodeParser(ReadOnlySpan<char> expression, bool simplifyConstantExpressions = true)
    {
        _tokenizer = new ExpressionTokenizer(expression);
        _flags = simplifyConstantExpressions ? 1 : 0;
    }

    public ExpressionNodeParser(ExpressionTokenizer tokenizer, bool simplifyConstantExpressions = true, bool leaveOpen = false)
    {
        _tokenizer = tokenizer;
        _flags = (simplifyConstantExpressions ? 1 : 0) | ((leaveOpen ? 1 : 0) * 2);
    }

    private struct ExpressionNodeBuffer
    {
#nullable disable
        public int Count;
        public IExpressionFunction Function;
        private IExpressionNode _node1;
        private IExpressionNode _node2;
        private IExpressionNode _node3;
        public bool IsInParams;
#nullable restore

        public readonly void AssertNotFull()
        {
            if (Count >= 3)
                throw new FormatException($"Too many arguments specified for function \"{Function.FunctionName}\".");
        }

        public readonly IExpressionNode Build<TResult>(IExpressionFunction? parent, int parentArg, bool simplify)
            where TResult : IEquatable<TResult>
        {
#if DEBUG
            if (Function == null)
                throw new Exception("Not open.");
            if (IsInParams)
                throw new Exception($"Not closed ({Function.FunctionName}).");
#endif
            if (Count == 0 ? Function.ArgumentCountMask != 0 : (Function.ArgumentCountMask & (1 << (Count - 1))) == 0)
            {
                throw new FormatException($"Function {Function.FunctionName} does not accept {Count} arguments.");
            }

            IFunctionExpressionNode n = Count switch
            {
                0 => Function as IFunctionExpressionNode ?? new FunctionExpressionNode0(Function),
                1 => new FunctionExpressionNode1(Function, _node1),
                2 => new FunctionExpressionNode2(Function, _node1, _node2),
                _ => new FunctionExpressionNode3(Function, _node1, _node2, _node3)
            };

            return simplify ? SimplifyExpression<TResult>(n, parent, parentArg) : n;
        }

        public IExpressionNode this[int ind]
        {
            //readonly get => ind switch
            //{
            //    0 => _node1,
            //    1 => _node2,
            //    2 => _node3,
            //    _ => throw new ArgumentOutOfRangeException(nameof(ind))
            //};
            set
            {
                switch (ind)
                {
                    case 0: _node1 = value; break;
                    case 1: _node2 = value; break;
                    case 2: _node3 = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(ind));
                }
            }
        }
    }

    /// <summary>
    /// Parses a root expression node from this parser's expression.
    /// </summary>
    /// <returns>The new expression node.</returns>
    /// <exception cref="FormatException">Invalid expression.</exception>
    public IExpressionNode Parse<TResult>()
        where TResult : IEquatable<TResult>
    {
        if (_stack != null)
        {
            Array.Clear(_stack, 0, _stackSize);
        }

        _stackSize = 0;
        _current = default;
        _tokenizer.Reset();
        while (_tokenizer.MoveNext())
        {
            ref ExpressionToken token = ref _tokenizer.Current;

            switch (token.Type)
            {
                case ExpressionTokenType.FunctionName:
                    if (!ExpressionFunctions.TryGetFunction(token.GetContent(), out IExpressionFunction? func))
                    {
                        throw new FormatException($"Unknown expression function: \"{token.GetContent()}\".");
                    }

                    if (_current.Function != null)
                    {
                        Push() = _current;
                        _current = default;
                    }

                    _current.Function = func;
                    _current.Count = 0;
                    break;

                case ExpressionTokenType.OpenParams:
                    _current.IsInParams = true;
                    break;

                case ExpressionTokenType.CloseParams:
                    bool wasInParams;
                    do
                    {
                        wasInParams = _current.IsInParams;
                        _current.IsInParams = false;
                        if (_stackSize > 0)
                        {
                            ExpressionNodeBuffer bufferState = _current;
                            _current = Peek();
                            Pop();
                            _current.AssertNotFull();
                            IExpressionNode node = bufferState.Build<TResult>(_current.Function, _current.Count, SimplifyConstantExpressions);
                            _current[_current.Count] = node;
                            ++_current.Count;
                        }
                        else
                        {
                            return _current.Build<TResult>(null, 0, SimplifyConstantExpressions);
                        }

                    } while (!wasInParams);
                    break;

                case ExpressionTokenType.Value:
                    _current.AssertNotFull();
                    _current[_current.Count] = CreateValue(ref token);
                    ++_current.Count;
                    break;
            }
        }

        if (_current is { Function: not null, IsInParams: false })
        {
            return _current.Build<TResult>(null, 0, SimplifyConstantExpressions);
        }

        throw new FormatException("Function not closed.");
    }

#pragma warning disable CS8500

    private unsafe IExpressionNode CreateValue(ref ExpressionToken token)
    {
        IType? idealType = _current.Function.GetIdealArgumentType(_current.Count);
        ReadOnlySpan<char> span = token.Content;
        switch (token.ValueType)
        {
            case ExpressionValueType.Value:
                switch (idealType)
                {
                    case null: break;

                    case NumericAnyType:
                        return ParseValueAny(span, numericAny: true);

                    default:
                        ReadRawValueAsTypeVisitor v;
                        v.Value = null;
                        v.SpanAddr = &span;
                        v.ValueAsString = token.ContentAsString;
                        idealType.Visit(ref v);
                        if (v.Value is not IExpressionNode node)
                            throw new FormatException($"Failed to parse {idealType.Id}: \"{span.ToString()}\".");
                        return node;
                }

                return ParseValueAny(span, numericAny: false);


            case ExpressionValueType.PropertyRef:
                PropertyReference propRef = PropertyReference.Parse(span, token.ContentAsString);
                return new PropertyReferenceExpressionNode(propRef);
            
            case ExpressionValueType.DataRef:
                //if (!Values.TryParseDataRef(span, token.ContentAsString, out ISpecDynamicValue? val) || val is not DataRef dr)
                //{
                //    throw new FormatException($"Failed to parse data-ref: \"{token.GetContent()}\".");
                //}

                //return dr;
                throw new NotImplementedException();

            default:
                throw new FormatException("Invalid value type, shouldn't happen.");
        }
    }

    private unsafe struct ReadRawValueAsTypeVisitor : ITypeVisitor
    {
        public IValue? Value;
        public ReadOnlySpan<char>* SpanAddr;
        public string? ValueAsString;

        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            if (TypeConverters.TryGet<TValue>() is not { } converter)
                return;

            TypeConverterParseArgs<TValue> args = default;
            args.Type = type;
            args.TextAsString = ValueAsString;

            // remove type suffix if available (3f, 4d, etc).
            ReadOnlySpan<char> span = *SpanAddr;
            string? suffix = CommonTypes.GetTypeSuffix<TValue>();
            if (suffix != null
                && span.Length > suffix.Length
                && span.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                span = span[..^suffix.Length];
            }

            if (converter.TryParse(span, ref args, out TValue? value))
            {
                Value = Values.Value.Create(value, type);
            }
        }
    }
    private IExpressionNode ParseValueAny(ReadOnlySpan<char> span, bool numericAny)
    {
        ReadOnlySpan<char> spanTrimmed = span.Trim();

        if (!numericAny)
        {
            if (spanTrimmed.Equals("true", StringComparison.OrdinalIgnoreCase))
                return Value.True;

            if (spanTrimmed.Equals("false", StringComparison.OrdinalIgnoreCase))
                return Value.False;
        }
        
        ulong u8;
        long i8;
        if (spanTrimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            spanTrimmed = spanTrimmed[2..];
            if (spanTrimmed.EndsWith("ul", StringComparison.OrdinalIgnoreCase))
            {
                if (ulong.TryParse(StringHelper.AsParsable(spanTrimmed[..^2]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out u8))
                    return new ConcreteValue<ulong>(u8, UInt64Type.Instance);

                throw new FormatException($"Unable to parse hexadecimal UInt64: \"{spanTrimmed.ToString()}\".");
            }
            if (spanTrimmed.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            {
                if (long.TryParse(StringHelper.AsParsable(spanTrimmed[..^1]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out i8))
                    return new ConcreteValue<long>(i8, Int64Type.Instance);

                throw new FormatException($"Unable to parse hexadecimal Int64: \"{spanTrimmed.ToString()}\".");
            }
            if (spanTrimmed.EndsWith("u", StringComparison.OrdinalIgnoreCase))
            {
                if (uint.TryParse(StringHelper.AsParsable(spanTrimmed[..^1]), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint u4))
                    return new ConcreteValue<uint>(u4, UInt32Type.Instance);

                throw new FormatException($"Unable to parse hexadecimal UInt32: \"{spanTrimmed.ToString()}\".");
            }
            if (ulong.TryParse(StringHelper.AsParsable(spanTrimmed), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out u8))
            {
                return u8 <= uint.MaxValue
                    ? new ConcreteValue<uint>((uint)u8, UInt32Type.Instance)
                    : new ConcreteValue<ulong>(u8, UInt64Type.Instance);
            }
            if (long.TryParse(StringHelper.AsParsable(spanTrimmed), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out i8))
            {
                return i8 is >= int.MinValue and <= int.MaxValue
                    ? new ConcreteValue<int>((int)i8, Int32Type.Instance)
                    : new ConcreteValue<long>(i8, Int64Type.Instance);
            }
        }
        else if (spanTrimmed.StartsWith("0b", StringComparison.OrdinalIgnoreCase))
        {
            spanTrimmed = spanTrimmed[2..];
            if (spanTrimmed.EndsWith("ul", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    ulong binary = Convert.ToUInt64(spanTrimmed.ToString(), 2);
                    return new ConcreteValue<ulong>(binary, UInt64Type.Instance);
                }
                catch (FormatException ex)
                {
                    throw new FormatException($"Unable to parse binary UInt64: \"{spanTrimmed.ToString()}\".", ex);
                }
            }
            if (spanTrimmed.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    long binary = Convert.ToInt64(spanTrimmed.ToString(), 2);
                    return new ConcreteValue<long>(binary, Int64Type.Instance);
                }
                catch (FormatException ex)
                {
                    throw new FormatException($"Unable to parse binary Int64: \"{spanTrimmed.ToString()}\".", ex);
                }
            }
            if (spanTrimmed.EndsWith("u", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    uint binary = Convert.ToUInt32(spanTrimmed.ToString(), 2);
                    return new ConcreteValue<uint>(binary, UInt32Type.Instance);
                }
                catch (FormatException ex)
                {
                    throw new FormatException($"Unable to parse binary UInt32: \"{spanTrimmed.ToString()}\".", ex);
                }
            }

            try
            {
                ulong binary = Convert.ToUInt64(spanTrimmed.ToString(), 2);
                return binary < uint.MaxValue ? new ConcreteValue<uint>((uint)binary, UInt32Type.Instance) : new ConcreteValue<ulong>(binary, UInt64Type.Instance);
            }
            catch (FormatException) { }
        }
        else if (!spanTrimmed.IsEmpty && (char.IsDigit(spanTrimmed[0]) || spanTrimmed[0] is '-' or '+' or '.' or ','))
        {
            double r8;

            if (spanTrimmed.EndsWith("ul", StringComparison.OrdinalIgnoreCase))
            {
                if (ulong.TryParse(StringHelper.AsParsable(spanTrimmed[..^2]), NumberStyles.Number & ~NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out u8))
                    return new ConcreteValue<ulong>(u8, UInt64Type.Instance);

                throw new FormatException($"Unable to parse UInt64: \"{spanTrimmed.ToString()}\".");
            }
            if (spanTrimmed.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            {
                if (long.TryParse(StringHelper.AsParsable(spanTrimmed[..^1]), NumberStyles.Number & ~NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out i8))
                    return new ConcreteValue<long>(i8, Int64Type.Instance);

                throw new FormatException($"Unable to parse Int64: \"{spanTrimmed.ToString()}\".");
            }
            if (spanTrimmed.EndsWith("u", StringComparison.OrdinalIgnoreCase))
            {
                if (uint.TryParse(StringHelper.AsParsable(spanTrimmed[..^1]), NumberStyles.Number & ~NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out uint u4))
                    return new ConcreteValue<uint>(u4, UInt32Type.Instance);

                throw new FormatException($"Unable to parse UInt32: \"{spanTrimmed.ToString()}\".");
            }
            if (spanTrimmed.EndsWith("f", StringComparison.OrdinalIgnoreCase))
            {
                if (float.TryParse(StringHelper.AsParsable(spanTrimmed[..^1]), NumberStyles.Number, CultureInfo.InvariantCulture, out float r4))
                    return new ConcreteValue<float>(r4, Float32Type.Instance);

                throw new FormatException($"Unable to parse Float32: \"{spanTrimmed.ToString()}\".");
            }
            if (spanTrimmed.EndsWith("d", StringComparison.OrdinalIgnoreCase))
            {
                if (double.TryParse(StringHelper.AsParsable(spanTrimmed[..^1]), NumberStyles.Number, CultureInfo.InvariantCulture, out r8))
                    return new ConcreteValue<double>(r8, Float64Type.Instance);

                throw new FormatException($"Unable to parse Float64: \"{spanTrimmed.ToString()}\".");
            }
            if (spanTrimmed.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            {
                if (decimal.TryParse(StringHelper.AsParsable(spanTrimmed[..^1]), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal r16))
                    return new ConcreteValue<decimal>(r16, Float128Type.Instance);

                throw new FormatException($"Unable to parse Decimal: \"{spanTrimmed.ToString()}\".");
            }

            // ReSharper disable once SuggestVarOrType_BuiltInTypes (changes depending on target)
            // ReSharper disable once SuggestVarOrType_Elsewhere
            var parsable = _tokenizer.Current.ContentParsable;
            if (ulong.TryParse(parsable, NumberStyles.Number & ~NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out u8))
            {
                return u8 <= uint.MaxValue
                    ? new ConcreteValue<uint>((uint)u8, UInt32Type.Instance)
                    : new ConcreteValue<ulong>(u8, UInt64Type.Instance);
            }
            if (long.TryParse(parsable, NumberStyles.Number & ~NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out i8))
            {
                return i8 is >= int.MinValue and <= int.MaxValue
                    ? new ConcreteValue<int>((int)i8, Int32Type.Instance)
                    : new ConcreteValue<long>(i8, Int64Type.Instance);
            }
            if (double.TryParse(parsable, NumberStyles.Number, CultureInfo.InvariantCulture, out r8))
            {
                return new ConcreteValue<double>(r8, Float64Type.Instance);
            }
        }

        if (numericAny)
            throw new FormatException($"Expected numeric value: \"{spanTrimmed.ToString()}\".");

        if (spanTrimmed.Length >= 32 && spanTrimmed[0] is >= '0' and <= '9' or >= 'A' and <= 'F' or >= 'a' and <= 'f')
        {
            if (Guid.TryParse(_tokenizer.Current.ContentParsable, out Guid guid))
            {
                return new ConcreteValue<Guid>(guid, AssetReferenceType.GetInstance(AssetReferenceKind.Object));
            }
        }

        return new ConcreteValue<string>(_tokenizer.Current.GetContent(), StringType.Instance);
    }


    private static IExpressionNode SimplifyExpression<TResult>(IFunctionExpressionNode expr, IExpressionFunction? parent, int parentArg)
        where TResult : IEquatable<TResult>
    {
        ExpressionSimplifier<TResult> v;
        v.Node = null;
        v.RequireExactType = parent == null;
        if (parent?.GetIdealArgumentType(parentArg) is { } argType)
        {
            v.Type = argType;
            if (argType is NumericAnyType)
            {
#if NET5_0_OR_GREATER
                System.Runtime.CompilerServices.Unsafe.SkipInit(out FileEvaluationContext ctx);
#else
                FileEvaluationContext ctx = default;
#endif
                ExpressionEvaluator evaluator = new ExpressionEvaluator(expr);
                if (evaluator.Evaluate<double, ExpressionSimplifier<TResult>>(ref v, concreteOnly: true, in ctx) && v.Node != null)
                    return v.Node;

                return expr;
            }

            v.Node = expr;
            argType.Visit(ref v);
        }
        else
        {
#if NET5_0_OR_GREATER
            System.Runtime.CompilerServices.Unsafe.SkipInit(out FileEvaluationContext ctx);
#else
            FileEvaluationContext ctx = default;
#endif
            ExpressionEvaluator evaluator = new ExpressionEvaluator(expr);
            if (CommonTypes.TryGetDefaultValueType<TResult>() is { } resultType)
            {
                v.Type = resultType;
                if (!evaluator.Evaluate<TResult, ExpressionSimplifier<TResult>>(ref v, concreteOnly: true, in ctx))
                    v.Node = null;
            }
        }

        return v.Node ?? expr;
    }

    private struct ExpressionSimplifier<TResult> : IGenericVisitor, ITypeVisitor
        where TResult : IEquatable<TResult>
    {
        public IExpressionNode? Node;
        public IType Type;
        public bool RequireExactType;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            if (RequireExactType && typeof(T) != typeof(TResult))
            {
                IType<TResult>? type = Type as IType<TResult> ?? CommonTypes.TryGetDefaultValueType<TResult>();
                if (type == null)
                    return;

                ConvertVisitor<TResult> v = default;
                v.Accept(value);
                if (v.WasSuccessful)
                {
                    Node = v.IsNull ? Value.Null(type) : Value.Create(v.Result!, type);
                }
            }
            else
            {
                IType<T>? type = Type as IType<T> ?? CommonTypes.TryGetDefaultValueType<T>();
                if (type == null)
                    return;

                Node = value == null ? Value.Null(type) : Value.Create(value, type);
            }
        }

        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            Type = type;
#if NET5_0_OR_GREATER
            System.Runtime.CompilerServices.Unsafe.SkipInit(out FileEvaluationContext ctx);
#else
            FileEvaluationContext ctx = default;
#endif
            ExpressionEvaluator evaluator = new ExpressionEvaluator((IFunctionExpressionNode)Node!);
            if (!evaluator.Evaluate<double, ExpressionSimplifier<TResult>>(ref this, concreteOnly: true, in ctx))
            {
                Node = null;
            }
        }
    }

#pragma warning restore CS8500

    private ref ExpressionNodeBuffer Push()
    {
        if (_stack == null)
        {
            _stack = new ExpressionNodeBuffer[4];
            _stackSize = 1;
            return ref _stack[0];
        }

        if (_stackSize == _stack.Length)
        {
            ExpressionNodeBuffer[] newArr = new ExpressionNodeBuffer[_stackSize * 2];
            Array.Copy(_stack, newArr, _stackSize);
            _stack = newArr;
        }

        ++_stackSize;
        return ref _stack[_stackSize - 1];
    }

    private void Pop()
    {
        if (_stack == null || _stackSize == 0)
        {
            return;
        }

        --_stackSize;
        _stack[_stackSize] = default;
    }

    private ref ExpressionNodeBuffer Peek()
    {
        if (_stack == null || _stackSize == 0)
        {
            return ref Push();
        }

        return ref _stack[_stackSize - 1];
    }

    public void Dispose()
    {
        if (!LeaveTokenizerOpen)
        {
            _tokenizer.Dispose();
        }
    }
}

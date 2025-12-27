using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal ref struct ExpressionNodeParser : IDisposable
{
    private ExpressionTokenizer _tokenizer;
    private ExpressionNodeBuffer[]? _stack;
    private int _stackSize;

    public ExpressionNodeParser(ExpressionTokenizer tokenizer)
    {
        _tokenizer = tokenizer;
    }

    private struct ExpressionNodeBuffer
    {
#nullable disable
        public int Count;
        public IExpressionFunction Function;
        public IExpressionNode Node1;
        public IExpressionNode Node2;
        public IExpressionNode Node3;
#nullable restore
    }

    public IFunctionExpressionNode Parse()
    {
        if (_stack != null)
        {
            Array.Clear(_stack, 0, _stackSize);
        }

        _stackSize = 0;

        _tokenizer.Reset();
        while (_tokenizer.MoveNext())
        {
            ref readonly ExpressionToken token = ref _tokenizer.Current;

            switch (token.Type)
            {
                case ExpressionTokenType.FunctionName:
                    if (!ExpressionFunctions.TryGetFunction(token.GetContent(), out IExpressionFunction? func))
                    {
                        throw new FormatException($"Unknown expression function: {token.GetContent()}.");
                    }

                    break;
            }
        }
        // todo
        return null;
    }

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
        _tokenizer.Dispose();
    }
}

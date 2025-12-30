using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal struct ExpressionEvaluator
{
    private readonly IFunctionExpressionNode _root;
    private readonly IExpressionFunction _func;
    private int _arg;

    internal ExpressionEvaluator(IFunctionExpressionNode root, int arg = -1)
    {
        _root = root;
        _func = root.Function;
        _arg = arg;
    }

    public unsafe bool Evaluate<TIdealOut, TVisitor>(ref TVisitor resultVisitor, bool concreteOnly, in FileEvaluationContext ctx)
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor
    {
        if (_root.Count <= 0)
        {
            return _func.Evaluate<TIdealOut, TVisitor>(ref resultVisitor);
        }

        _arg = 0;

        Arg0Eval<TIdealOut, TVisitor> v;
        v.WasSuccessful = false;
        v.ConcreteOnly = concreteOnly;
        fixed (TVisitor* resultVisitorPtr = &resultVisitor)
        {
            v.ResultVisitor = resultVisitorPtr;
            fixed (FileEvaluationContext* ctxPtr = &ctx)
            {
                v.EvalCtx = ctxPtr;
                fixed (ExpressionEvaluator* e = &this)
                {
                    v.Evaluator = e;
                    EvaluateArgument(ref v, concreteOnly, in ctx);
                }
            }
        }

        return v.WasSuccessful;
    }

    private unsafe bool EvaluateArgument<TArgVisitor>(ref TArgVisitor argVisitor, bool concreteOnly, in FileEvaluationContext ctx)
        where TArgVisitor : IGenericVisitor
    {
        switch (_root[_arg])
        {
            case IFunctionExpressionNode function:
                IType? idealType = _func.GetIdealArgumentType(_arg);
                if (idealType is NumericAnyType or null)
                {
                    ExpressionEvaluator evaluator = new ExpressionEvaluator(function);
                    return evaluator.Evaluate<double, TArgVisitor>(ref argVisitor, concreteOnly, in ctx);
                }

                TypeEvaluateVisitor<TArgVisitor> evalVisitor;
                evalVisitor.Function = function;
                evalVisitor.ConcreteOnly = concreteOnly;
                evalVisitor.Visited = false;
                fixed (TArgVisitor* argVisitorPtr = &argVisitor)
                {
                    evalVisitor.Visitor = argVisitorPtr;
                    fixed (FileEvaluationContext* c = &ctx)
                    {
                        evalVisitor.EvalCtx = c;
                        idealType.Visit(ref evalVisitor);
                    }
                }

                return evalVisitor.Visited;

            case IValueExpressionNode simpleValue:
                return concreteOnly
                    ? simpleValue.VisitConcreteValueGeneric(ref argVisitor)
                    : simpleValue.VisitValueGeneric(ref argVisitor, in ctx);

            case IPropertyReferenceExpressionNode propRef:
                if (concreteOnly)
                    return false;

                throw new NotImplementedException();

            case IDataRefExpressionNode dataRef:
                if (concreteOnly)
                    return false;

                throw new NotImplementedException();
        }

        return false;
    }

    private unsafe struct TypeEvaluateVisitor<TVisitor> : ITypeVisitor
        where TVisitor : IGenericVisitor
    {
        public IFunctionExpressionNode Function;
        public TVisitor* Visitor;
        public FileEvaluationContext* EvalCtx;
        public bool ConcreteOnly;
        public bool Visited;

        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            ref readonly FileEvaluationContext ctx = ref Unsafe.AsRef<FileEvaluationContext>(EvalCtx);

            ExpressionEvaluator evaluator = new ExpressionEvaluator(Function);
            Visited = evaluator.Evaluate<TValue, TVisitor>(ref Unsafe.AsRef<TVisitor>(Visitor), ConcreteOnly, in ctx);
        }
    }

    private unsafe struct Arg0Eval<TIdealOut, TResultVisitor> : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TResultVisitor : IGenericVisitor
    {
        public TResultVisitor* ResultVisitor;
        public ExpressionEvaluator* Evaluator;
        public FileEvaluationContext* EvalCtx;
        public bool ConcreteOnly;
        public bool WasSuccessful;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            ReducerVisitor rv;
            rv.ResultVisitor = ResultVisitor;
            rv.WasSuccessful = false;
            rv.Evaluator = Evaluator;
            rv.ConcreteOnly = ConcreteOnly;
            rv.EvalCtx = EvalCtx;

            if (!Evaluator->_root.Function.ReduceToKnownTypes
                || MathMatrix.IsValidMathExpressionInputType<T>()
                || !MathMatrix.TryReduce(value!, ref rv))
            {
                rv.Accept(value);
            }

            WasSuccessful = rv.WasSuccessful;
        }

        private struct ReducerVisitor : IGenericVisitor
        {
            public TResultVisitor* ResultVisitor;
            public ExpressionEvaluator* Evaluator;
            public FileEvaluationContext* EvalCtx;
            public bool WasSuccessful;
            public bool ConcreteOnly;

            public void Accept<T>(T? value) where T : IEquatable<T>
            {
                if (Evaluator->_root.Count == 1)
                {
                    WasSuccessful = Evaluator->_func.Evaluate<T, TIdealOut, TResultVisitor>(value!, ref Unsafe.AsRef<TResultVisitor>(ResultVisitor));
                    return;
                }

                Arg1Eval<T, TIdealOut, TResultVisitor> v;
                v.ResultVisitor = ResultVisitor;
                v.WasSuccessful = false;
                v.Evaluator = Evaluator;
                v.ConcreteOnly = ConcreteOnly;
                v.EvalCtx = EvalCtx;
                v.Arg0 = value;
                Evaluator->_arg = 1;
                Evaluator->EvaluateArgument(ref v, ConcreteOnly, in Unsafe.AsRef<FileEvaluationContext>(EvalCtx));
                WasSuccessful = v.WasSuccessful;
            }
        }
    }

    private unsafe struct Arg1Eval<TArg0, TIdealOut, TResultVisitor> : IGenericVisitor
        where TArg0 : IEquatable<TArg0>
        where TIdealOut : IEquatable<TIdealOut>
        where TResultVisitor : IGenericVisitor
    {
        public TArg0? Arg0;
        public TResultVisitor* ResultVisitor;
        public ExpressionEvaluator* Evaluator;
        public FileEvaluationContext* EvalCtx;
        public bool WasSuccessful;
        public bool ConcreteOnly;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            ReducerVisitor rv;
            rv.ResultVisitor = ResultVisitor;
            rv.WasSuccessful = false;
            rv.Evaluator = Evaluator;
            rv.ConcreteOnly = ConcreteOnly;
            rv.EvalCtx = EvalCtx;
            rv.Arg0 = Arg0;

            if (!Evaluator->_root.Function.ReduceToKnownTypes
                || MathMatrix.IsValidMathExpressionInputType<T>()
                || !MathMatrix.TryReduce(value!, ref rv))
            {
                rv.Accept(value);
            }

            WasSuccessful = rv.WasSuccessful;
        }

        private struct ReducerVisitor : IGenericVisitor
        {
            public TArg0? Arg0;
            public TResultVisitor* ResultVisitor;
            public ExpressionEvaluator* Evaluator;
            public FileEvaluationContext* EvalCtx;
            public bool WasSuccessful;
            public bool ConcreteOnly;

            public void Accept<T>(T? value) where T : IEquatable<T>
            {
                if (Evaluator->_root.Count == 2)
                {
                    WasSuccessful = Evaluator->_func.Evaluate<TArg0, T, TIdealOut, TResultVisitor>(Arg0!, value!, ref Unsafe.AsRef<TResultVisitor>(ResultVisitor));
                    return;
                }

                Arg2Eval<TArg0, T, TIdealOut, TResultVisitor> v;
                v.ResultVisitor = ResultVisitor;
                v.WasSuccessful = false;
                v.Evaluator = Evaluator;
                v.Arg0 = Arg0;
                v.Arg1 = value;
                Evaluator->_arg = 2;
                Evaluator->EvaluateArgument(ref v, ConcreteOnly, in Unsafe.AsRef<FileEvaluationContext>(EvalCtx));
                WasSuccessful = v.WasSuccessful;
            }
        }
    }

    private unsafe struct Arg2Eval<TArg0, TArg1, TIdealOut, TResultVisitor> : IGenericVisitor
        where TArg0 : IEquatable<TArg0>
        where TArg1 : IEquatable<TArg1>
        where TIdealOut : IEquatable<TIdealOut>
        where TResultVisitor : IGenericVisitor
    {
        public TArg0? Arg0;
        public TArg1? Arg1;
        public TResultVisitor* ResultVisitor;
        public bool WasSuccessful;
        public ExpressionEvaluator* Evaluator;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            ReducerVisitor rv;
            rv.ResultVisitor = ResultVisitor;
            rv.WasSuccessful = false;
            rv.Evaluator = Evaluator;
            rv.Arg0 = Arg0;
            rv.Arg1 = Arg1;

            if (!Evaluator->_root.Function.ReduceToKnownTypes
                || MathMatrix.IsValidMathExpressionInputType<T>()
                || !MathMatrix.TryReduce(value!, ref rv))
            {
                rv.Accept(value);
            }

            WasSuccessful = rv.WasSuccessful;
        }

        private struct ReducerVisitor : IGenericVisitor
        {
            public TArg0? Arg0;
            public TArg1? Arg1;
            public TResultVisitor* ResultVisitor;
            public ExpressionEvaluator* Evaluator;
            public bool WasSuccessful;

            public void Accept<T>(T? value) where T : IEquatable<T>
            {
                if (Evaluator->_root.Count == 3)
                {
                    WasSuccessful = Evaluator->_func.Evaluate<TArg0, TArg1, T, TIdealOut, TResultVisitor>(Arg0!, Arg1!, value!, ref Unsafe.AsRef<TResultVisitor>(ResultVisitor));
                    return;
                }

                throw new FormatException($"Too many arguments supplied for function \"{Evaluator->_func.FunctionName}\".");
            }
        }
    }
}
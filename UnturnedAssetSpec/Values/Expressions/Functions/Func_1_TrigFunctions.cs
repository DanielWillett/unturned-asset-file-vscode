using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class SineRad : ExpressionFunction
{
    public static readonly SineRad Instance = new SineRad();
    static SineRad() { }

    public override string FunctionName => ExpressionFunctions.SineRad;
    public override int ArgumentCountMask => 1 << 0;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument == 0 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.SinRad<TIn, TVisitor, TOut>(v, ref visitor);
    }
}

internal sealed class CosineRad : ExpressionFunction
{
    public static readonly CosineRad Instance = new CosineRad();
    static CosineRad() { }

    public override string FunctionName => ExpressionFunctions.CosineRad;
    public override int ArgumentCountMask => 1 << 0;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument == 0 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.CosRad<TIn, TVisitor, TOut>(v, ref visitor);
    }
}

internal sealed class TangentRad : ExpressionFunction
{
    public static readonly TangentRad Instance = new TangentRad();
    static TangentRad() { }

    public override string FunctionName => ExpressionFunctions.TangentRad;
    public override int ArgumentCountMask => 1 << 0;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument == 0 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.TanRad<TIn, TVisitor, TOut>(v, ref visitor);
    }
}

internal sealed class SineDeg : ExpressionFunction
{
    public static readonly SineDeg Instance = new SineDeg();
    static SineDeg() { }

    public override string FunctionName => ExpressionFunctions.SineDeg;
    public override int ArgumentCountMask => 1 << 0;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument == 0 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.SinDeg<TIn, TVisitor, TOut>(v, ref visitor);
    }
}

internal sealed class CosineDeg : ExpressionFunction
{
    public static readonly CosineDeg Instance = new CosineDeg();
    static CosineDeg() { }

    public override string FunctionName => ExpressionFunctions.CosineDeg;
    public override int ArgumentCountMask => 1 << 0;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument == 0 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.CosDeg<TIn, TVisitor, TOut>(v, ref visitor);
    }
}

internal sealed class TangentDeg : ExpressionFunction
{
    public static readonly TangentDeg Instance = new TangentDeg();
    static TangentDeg() { }

    public override string FunctionName => ExpressionFunctions.TangentDeg;
    public override int ArgumentCountMask => 1 << 0;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument == 0 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.TanDeg<TIn, TVisitor, TOut>(v, ref visitor);
    }
}

internal sealed class ArcSineRad : ExpressionFunction
{
    public static readonly ArcSineRad Instance = new ArcSineRad();
    static ArcSineRad() { }

    public override string FunctionName => ExpressionFunctions.ArcSineRad;
    public override int ArgumentCountMask => 1 << 0;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument == 0 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.AsinRad<TIn, TVisitor, TOut>(v, ref visitor);
    }
}

internal sealed class ArcCosineRad : ExpressionFunction
{
    public static readonly ArcCosineRad Instance = new ArcCosineRad();
    static ArcCosineRad() { }

    public override string FunctionName => ExpressionFunctions.ArcCosineRad;
    public override int ArgumentCountMask => 1 << 0;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument == 0 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.AcosRad<TIn, TVisitor, TOut>(v, ref visitor);
    }
}

internal sealed class ArcTangentRad : ExpressionFunction
{
    public static readonly ArcTangentRad Instance = new ArcTangentRad();
    static ArcTangentRad() { }

    public override string FunctionName => ExpressionFunctions.ArcTangentRad;
    public override int ArgumentCountMask => (1 << 0) | (1 << 1);
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument is 0 or 1 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.AtanRad<TIn, TVisitor, TOut>(v, ref visitor);
    }

    public override bool Evaluate<TIn1, TIn2, TOut, TVisitor>(TIn1 v1, TIn2 v2, ref TVisitor visitor)
    {
        return MathMatrix.Atan2Rad<TIn1, TIn2, TVisitor, TOut>(v1, v2, ref visitor);
    }
}

internal sealed class ArcSineDeg : ExpressionFunction
{
    public static readonly ArcSineDeg Instance = new ArcSineDeg();
    static ArcSineDeg() { }

    public override string FunctionName => ExpressionFunctions.ArcSineDeg;
    public override int ArgumentCountMask => 1 << 0;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument == 0 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.AsinDeg<TIn, TVisitor, TOut>(v, ref visitor);
    }
}

internal sealed class ArcCosineDeg : ExpressionFunction
{
    public static readonly ArcCosineDeg Instance = new ArcCosineDeg();
    static ArcCosineDeg() { }

    public override string FunctionName => ExpressionFunctions.ArcCosineDeg;
    public override int ArgumentCountMask => 1 << 0;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument == 0 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.AcosDeg<TIn, TVisitor, TOut>(v, ref visitor);
    }
}

internal sealed class ArcTangentDeg : ExpressionFunction
{
    public static readonly ArcTangentDeg Instance = new ArcTangentDeg();
    static ArcTangentDeg() { }

    public override string FunctionName => ExpressionFunctions.ArcTangentDeg;
    public override int ArgumentCountMask => (1 << 0) | (1 << 1);
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument is 0 or 1 ? Float64Type.Instance : null;
    }

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.AtanDeg<TIn, TVisitor, TOut>(v, ref visitor);
    }

    public override bool Evaluate<TIn1, TIn2, TOut, TVisitor>(TIn1 v1, TIn2 v2, ref TVisitor visitor)
    {
        return MathMatrix.Atan2Deg<TIn1, TIn2, TVisitor, TOut>(v1, v2, ref visitor);
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Numerics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class BallisticGravityMultiplierCalculation : ExpressionFunction
{
    public static readonly BallisticGravityMultiplierCalculation Instance = new BallisticGravityMultiplierCalculation();
    static BallisticGravityMultiplierCalculation() { }

    public override string FunctionName => ExpressionFunctions.BallisticGravityMultiplierCalculation;
    public override int ArgumentCountMask => 1 << 2;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument is 0 or 1 or 2 ? Float32Type.Instance : null;
    }

    public override bool Evaluate<TIn1, TIn2, TIn3, TOut, TVisitor>(TIn1 v1, TIn2 v2, TIn3 v3, ref TVisitor visitor)
    {
        if (!TryGetDouble(v1, out double ballisticTravel)
            || !TryGetDouble(v2, out double ballisticSteps)
            || !TryGetDouble(v3, out double ballisticDrop))
        {
            return false;
        }

        if (typeof(TOut) == typeof(double))
        {
            double totalBallisticRise = 0;
            double rightX = 1, rightY = 0;
            for (int index = 0; index < ballisticSteps; ++index)
            {
                totalBallisticRise += rightY * ballisticTravel;
                rightY -= ballisticDrop;
                double len = Math.Sqrt(rightX * rightX + rightY * rightY);
                rightX /= len;
                rightY /= len;
            }

            double totalTimeSec = ballisticSteps * 0.02;
            double bulletGravityMultiplier = 2 * totalBallisticRise / (totalTimeSec * totalTimeSec) / -9.81;
            visitor.Accept(bulletGravityMultiplier);
        }
        else
        {
            float totalBallisticRise = 0.0f;
            Vector2 right = new Vector2(1f, 0f);
            for (int index = 0; index < ballisticSteps; ++index)
            {
                totalBallisticRise += right.Y * (float)ballisticTravel;
                right.Y -= (float)ballisticDrop;
                right = Vector2.Normalize(right);
            }

            float totalTimeSec = (float)ballisticSteps * 0.02f;
            float bulletGravityMultiplier = 2f * totalBallisticRise / (totalTimeSec * totalTimeSec) / -9.81f;

            visitor.Accept(bulletGravityMultiplier);
        }

        return true;
    }
}
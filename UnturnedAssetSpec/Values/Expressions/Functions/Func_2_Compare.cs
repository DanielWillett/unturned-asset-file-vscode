using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Compare : ExpressionFunction
{
    public static readonly Compare CaseSensitiveInstance = new Compare(false);
    public static readonly Compare CaseInsensitiveInstance = new Compare(true);

    private readonly bool _caseInsensitive;

    private Compare(bool caseInsensitive)
    {
        _caseInsensitive = caseInsensitive;
    }

    static Compare() { }

    public override string FunctionName => _caseInsensitive ? ExpressionFunctions.CompareIgnoreCase : ExpressionFunctions.Compare;
    public override int ArgumentCountMask => 1 << 1;
    public override IType? GetIdealArgumentType(int argument)
    {
        return null;
    }

    public override bool Evaluate<TIn1, TIn2, TOut, TVisitor>(TIn1 v1, TIn2 v2, ref TVisitor visitor)
    {
        if (MathMatrix.Compare(v1, v2, _caseInsensitive, ref visitor))
        {
            return true;
        }

        if (!ComparerVisitor<TIn1>.TryCompare(v1, v2, _caseInsensitive, out int cmp))
        {
            return false;
        }

        visitor.Accept(cmp);
        return true;

    }
}
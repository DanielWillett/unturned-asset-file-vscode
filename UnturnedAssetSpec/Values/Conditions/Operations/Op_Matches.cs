using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Operations;

internal sealed class Matches : ConditionOperation<Matches>
{
    public override string Name => "matches";
    public override string Symbol => "~";

    protected override bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
    {
        Regex? regex = null;
        string? expr = null;
        if (typeof(TComparand) == typeof(Regex))
        {
            regex = Unsafe.As<TComparand, Regex>(ref comparand);
        }
        else if (typeof(TComparand) == typeof(string))
        {
            regex = null;
            expr = Unsafe.As<TComparand, string>(ref comparand);
        }
        else
        {
            ConvertVisitor<string>.TryConvert(comparand, out expr);
        }

        if (regex == null && expr == null)
        {
            result = false;
            return false;
        }

        string? check;
        if (typeof(TValue) == typeof(string))
        {
            check = Unsafe.As<TValue, string?>(ref value);
        }
        else
        {
            ConvertVisitor<string>.TryConvert(value, out check);
        }

        if (check == null)
        {
            result = false;
            return false;
        }

        if (regex != null)
        {
            result = regex.IsMatch(check);
            return true;
        }

        try
        {
            result = Regex.IsMatch(check, expr!, RegexOptions.CultureInvariant);
            return true;
        }
        catch (ArgumentException) { }
        catch (RegexMatchTimeoutException) { }

        result = false;
        return false;
    }

    public override int GetHashCode() => 1241563256;
}
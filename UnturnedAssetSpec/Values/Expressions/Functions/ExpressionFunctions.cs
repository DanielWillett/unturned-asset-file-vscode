using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

/// <summary>
/// Manages the list of all available functions and variables that can be used in expressions.
/// </summary>
public static class ExpressionFunctions
{
    #region Expressions
    /* Constants */
    /// <summary>
    /// The co-effecient relationship between the diameter of a circle and it's circumference.
    /// </summary>
    /// <remarks><c>PI → number</c></remarks>
    public const string Pi   = "PI";
    /// <summary>
    /// The co-effecient relationship between the radius of a circle and it's circumference.
    /// </summary>
    /// <remarks><c>TAU → number</c></remarks>
    public const string Tau  = "TAU";
    /// <summary>
    /// The natrual log base.
    /// </summary>
    /// <remarks><c>E → number</c></remarks>
    public const string E    = "E";
    /// <summary>
    /// A <see langword="null"/> value.
    /// </summary>
    /// <remarks><c>NULL → null</c></remarks>
    public const string Null = "NULL";

    /* 1-arg */
    /// <summary>
    /// Drop the sign from a negative value, or leave a positive value as-is.
    /// </summary>
    /// <remarks><c>ABS(number) → number</c></remarks>
    public const string Absolute        = "ABS";
    /// <summary>
    /// Get the nearest integer, rounding up for 0.5.
    /// </summary>
    /// <remarks><c>ROUND(number) → number</c></remarks>
    public const string Round           = "ROUND";
    /// <summary>
    /// Get the highest integer &lt;= to a value.
    /// </summary>
    /// <remarks><c>FLOOR(number) → number</c></remarks>
    public const string Floor           = "FLOOR";
    /// <summary>
    /// Get the lowest integer &gt;= to a value.
    /// </summary>
    /// <remarks><c>CEIL(number) → number</c></remarks>
    public const string Ceiling         = "CEIL";
    /// <summary>
    /// Take the sine of an angle in radians.
    /// </summary>
    /// <remarks><c>SINR(number) → number</c></remarks>
    public const string SineRad         = "SINR";
    /// <summary>
    /// Take the cosine of an angle in radians.
    /// </summary>
    /// <remarks><c>COSR(number) → number</c></remarks>
    public const string CosineRad       = "COSR";
    /// <summary>
    /// Take the tangent of an angle in radians.
    /// </summary>
    /// <remarks><c>TANR(number) → number</c></remarks>
    public const string TangentRad      = "TANR";
    /// <summary>
    /// Take the inverse sine of a number, returning the value in radians.
    /// </summary>
    /// <remarks><c>ASINR(number) → number</c></remarks>
    public const string ArcSineRad      = "ASINR";
    /// <summary>
    /// Take the inverse cosine of a number, returning the value in radians.
    /// </summary>
    /// <remarks><c>ACOSR(number) → number</c></remarks>
    public const string ArcCosineRad    = "ACOSR";
    /// <summary>
    /// Take the inverse tangent of a number, returning the value in radians.
    /// </summary>
    /// <remarks>
    /// <c>ATANR(number) → number</c><br/>
    /// <c>ATANR(number[X] number[Y]) → number</c>
    /// </remarks>
    public const string ArcTangentRad   = "ATANR";
    /// <summary>
    /// Take the sine of an angle in degrees.
    /// </summary>
    /// <remarks><c>SIND(number) → number</c></remarks>
    public const string SineDeg         = "SIND";
    /// <summary>
    /// Take the cosine of an angle in degrees.
    /// </summary>
    /// <remarks><c>COSD(number) → number</c></remarks>
    public const string CosineDeg       = "COSD";
    /// <summary>
    /// Take the tangent of an angle in degrees.
    /// </summary>
    /// <remarks><c>TAND(number) → number</c></remarks>
    public const string TangentDeg      = "TAND";
    /// <summary>
    /// Take the inverse sine of a number, returning the value in degrees.
    /// </summary>
    /// <remarks><c>ASIND(number) → number</c></remarks>
    public const string ArcSineDeg      = "ASIND";
    /// <summary>
    /// Take the inverse cosine of a number, returning the value in degrees.
    /// </summary>
    /// <remarks><c>ACOSD(number) → number</c></remarks>
    public const string ArcCosineDeg    = "ACOSD";
    /// <summary>
    /// Take the inverse tangent of a number, returning the value in degrees.
    /// </summary>
    /// <remarks>
    /// <c>ATAND(number) → number</c><br/>
    /// <c>ATAND(number[X] number[Y]) → number</c>
    /// </remarks>
    public const string ArcTangentDeg   = "ATAND";
    /// <summary>
    /// Take the square-root of a number.
    /// </summary>
    /// <remarks><c>SQRT(number) → number</c></remarks>
    public const string SquareRoot      = "SQRT";


    /* 2-arg */
    /// <summary>
    /// Add two values.
    /// </summary>
    /// <remarks><c>ADD(number number) → number</c></remarks>
    public const string Add             = "ADD";
    /// <summary>
    /// Subtract two values.
    /// </summary>
    /// <remarks><c>SUB(number number) → number</c></remarks>
    public const string Subtract        = "SUB";
    /// <summary>
    /// Multiply two values.
    /// </summary>
    /// <remarks><c>MUL(number number) → number</c></remarks>
    public const string Multiply        = "MUL";
    /// <summary>
    /// Divide two values.
    /// </summary>
    /// <remarks><c>DIV(number number) → number</c></remarks>
    public const string Divide          = "DIV";
    /// <summary>
    /// Get the remainder of the division of two values.
    /// </summary>
    /// <remarks><c>MOD(number number) → number</c></remarks>
    public const string Modulo          = "MOD";
    /// <summary>
    /// Get the lowest of two values.
    /// </summary>
    /// <remarks><c>MIN(number number) → number</c></remarks>
    public const string Minimum         = "MIN";
    /// <summary>
    /// Get the highest of two values.
    /// </summary>
    /// <remarks><c>MAX(number number) → number</c></remarks>
    public const string Maximum         = "MAX";
    /// <summary>
    /// Raise a number to the exponent of another.
    /// </summary>
    /// <remarks><c>POW(number number) → number</c></remarks>
    public const string Power           = "POW";
    /// <summary>
    /// Combine two values together as strings.
    /// </summary>
    /// <remarks>
    /// <c>CAT(any) → string</c><br/>
    /// <c>CAT(any any) → string</c><br/>
    /// <c>CAT(any any any) → string</c>
    /// </remarks>
    public const string Concatenate     = "CAT";


    /* 3-arg */
    /// <summary>
    /// Replace all instances of <c>Replaced</c> with <c>NewValue</c> in <c>Base</c>.
    /// </summary>
    /// <remarks><c>REP(any[Base] any[Replaced] any[NewValue]) → string</c></remarks>
    public const string Replace         = "REP";

    /// <summary>
    /// Custom implementation for the <c>Bullet_Gravity_Multiplier</c> property in <see cref="T:SDG.Unturned.ItemGunAsset"/>.
    /// </summary>
    /// <remarks><c>CUSTOM_BALLISTIC_GRAV(number[BallisticTravel] number[BallisticSteps] number[BallisticDrop]) → number</c></remarks>
    internal const string BallisticGravityMultiplierCalculation = "CUSTOM_BALLISTIC_GRAV";

    #endregion

    private static readonly ConcurrentDictionary<string, OneOrMore<IExpressionFunction>> Functions
        = new ConcurrentDictionary<string, OneOrMore<IExpressionFunction>>(StringComparer.OrdinalIgnoreCase);
    
    static ExpressionFunctions()
    {
        RegisterFunction(Expressions.Pi.Instance);
        RegisterFunction(Expressions.Tau.Instance);
        RegisterFunction(Expressions.E.Instance);
        RegisterFunction(Expressions.Null.Instance);
        
        RegisterFunction(Expressions.Absolute.Instance);
        RegisterFunction(Expressions.Round.Instance);
        RegisterFunction(Expressions.Floor.Instance);
        RegisterFunction(Expressions.Ceiling.Instance);
        RegisterFunction(Expressions.SineRad.Instance);
        RegisterFunction(Expressions.SineDeg.Instance);
        RegisterFunction(Expressions.CosineRad.Instance);
        RegisterFunction(Expressions.CosineDeg.Instance);
        RegisterFunction(Expressions.TangentRad.Instance);
        RegisterFunction(Expressions.TangentDeg.Instance);
        RegisterFunction(Expressions.ArcSineRad.Instance);
        RegisterFunction(Expressions.ArcSineDeg.Instance);
        RegisterFunction(Expressions.ArcCosineRad.Instance);
        RegisterFunction(Expressions.ArcCosineDeg.Instance);
        RegisterFunction(Expressions.ArcTangentRad.Instance);
        RegisterFunction(Expressions.ArcTangentDeg.Instance);

        RegisterFunction(Expressions.Add.Instance);
        RegisterFunction(Expressions.Subtract.Instance);
        RegisterFunction(Expressions.Multiply.Instance);
        RegisterFunction(Expressions.Divide.Instance);
        RegisterFunction(Expressions.Modulo.Instance);
        RegisterFunction(Expressions.Minimum.Instance);
        RegisterFunction(Expressions.Maximum.Instance);
        RegisterFunction(Expressions.Power.Instance);
        RegisterFunction(Expressions.Concatenate.Instance);

        RegisterFunction(Expressions.Replace.Instance);
        RegisterFunction(Expressions.BallisticGravityMultiplierCalculation.Instance);
    }

    /// <summary>
    /// Enumerates through all active functions.
    /// </summary>
    /// <remarks>This requires copying data so should be avoided if possible.</remarks>
    public static IEnumerator<IExpressionFunction> EnumerateFunctions()
    {
        // ReSharper disable once NotDisposedResourceIsReturned
        return Functions.Values.Where(x => !x.IsNull).Select(x => x.Last()).GetEnumerator();
    }

    /// <summary>
    /// Adds a new function to the function registration list.
    /// </summary>
    public static void RegisterFunction(IExpressionFunction funcType)
    {
#if NETSTANDARD2_1_OR_GREATER || NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        Functions.AddOrUpdate(
            funcType.FunctionName,
            static (_, funcType) => new OneOrMore<IExpressionFunction>(funcType),
            static (_, e, funcType) => e.Add(funcType),
            funcType
        );
#else
        Functions.AddOrUpdate(
            funcType.FunctionName,
            _ => new OneOrMore<IExpressionFunction>(funcType),
            (_, e) => e.Add(funcType)
        );
#endif
    }

    /// <summary>
    /// Removes a function from the function registration list.
    /// </summary>
    public static void DeregisterFunction(IExpressionFunction funcType)
    {
        string funcName = funcType.FunctionName;
        while (true)
        {
#if NETSTANDARD2_1_OR_GREATER || NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            OneOrMore<IExpressionFunction> m = Functions.AddOrUpdate(
                funcName,
                static (_, _) => OneOrMore<IExpressionFunction>.Null,
                static (_, e, funcType) => e.Remove(funcType),
                funcType
            );
#else
            OneOrMore<IExpressionFunction> m = Functions.AddOrUpdate(
                funcName,
                _ => OneOrMore<IExpressionFunction>.Null,
                (_, e) => e.Remove(funcType)
            );
#endif
            if (!m.IsNull || !Functions.TryRemove(funcName, out OneOrMore<IExpressionFunction> functions) || functions.IsNull)
                break;

#if NETSTANDARD2_1_OR_GREATER || NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            Functions.AddOrUpdate(
                funcName,
                static (_, functions) => functions,
                static (_, e, functions) =>
                {
                    foreach (IExpressionFunction f in functions)
                        e = e.Add(f);
                    return e;
                },
                functions
            );
#else
            Functions.AddOrUpdate(
                funcName,
                _ => functions,
                (_, e) =>
                {
                    foreach (IExpressionFunction f in functions)
                        e = e.Add(f);
                    return e;
                }
            );
#endif
        }
    }

    /// <summary>
    /// Attempts to find the most recently added function with the given name.
    /// </summary>
    public static bool TryGetFunction([NotNullWhen(true)] string? functionName, [NotNullWhen(true)] out IExpressionFunction? function)
    {
        if (functionName == null)
        {
            function = null;
            return false;
        }

        if (Functions.TryGetValue(functionName, out OneOrMore<IExpressionFunction> functions) && functions.Length > 0)
        {
            function = functions[^1];
            return true;
        }

        function = null;
        return false;
    }
}
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    // note: most of these functions will fail with very large numbers due to the way decimals are represented
    // see https://stackoverflow.com/questions/66030225/different-results-between-c-and-c-sharp-sin-function-with-large-values
    // for more info

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        SinRadVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, SinRadVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Sin(inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin((double)inVal));
        return true;
    }

    /// <inheritdoc cref="SinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool SinRad<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return SinRad<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs a sine operation returning a value in radians.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool SinRad<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return SinRad(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return SinRad(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return SinRad(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return SinRad(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return SinRad(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return SinRad(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return SinRad(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return SinRad(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return SinRad(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return SinRad(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return SinRad(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.TrigOperation(inVal, op: 0, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return SinRad<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct SinRadVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = SinRad<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
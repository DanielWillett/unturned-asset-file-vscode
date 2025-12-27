using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    // note: most of these functions will fail with very large numbers due to the way decimals are represented
    // see https://stackoverflow.com/questions/66030225/different-results-between-c-and-c-sharp-sin-function-with-large-values
    // for more info

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        TanRadVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, TanRadVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Tan(inVal));
        return true;
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Tan(inVal));
        return true;
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Tan(inVal));
        return true;
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Tan(inVal));
        return true;
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Tan(inVal));
        return true;
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Tan(inVal));
        return true;
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Tan(inVal));
        return true;
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Tan(inVal));
        return true;
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        inVal = (inVal % (MathF.PI * 2f) + MathF.PI * 2f) % (MathF.PI * 2f);
        visitor.Accept(inVal switch
        {
            >= 1.5707963f and <= 1.57079649f => float.PositiveInfinity, // 90deg
            >= 4.712388f and <= 4.7123899f => float.NegativeInfinity,   // 270deg
            _ => MathF.Tan(inVal)
        });
        return true;
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        inVal = (inVal % (Math.PI * 2d) + Math.PI * 2d) % (Math.PI * 2d);
        visitor.Accept(inVal switch
        {
            >= 1.5707963267948 and <= 1.57079632679499 => double.PositiveInfinity,  // 90deg
            >= 4.7123889803846 and <= 4.71238898038479 => double.NegativeInfinity,   // 270deg
            _ => Math.Tan(inVal)
        });
        return true;
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return TanRad((double)inVal, ref visitor);
    }

    /// <inheritdoc cref="TanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool TanRad<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return TanRad<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs a tangent operation returning a value in radians.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool TanRad<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return TanRad(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return TanRad(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return TanRad(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return TanRad(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return TanRad(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return TanRad(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return TanRad(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return TanRad(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return TanRad(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return TanRad(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return TanRad(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.TrigOperation(inVal, op: 2, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return TanRad<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct TanRadVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = TanRad<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
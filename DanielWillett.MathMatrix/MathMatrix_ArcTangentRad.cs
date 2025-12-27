using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        AtanRadVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, AtanRadVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Atan(inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan((double)inVal));
        return true;
    }

    /// <inheritdoc cref="AtanRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanRad<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return AtanRad<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation returning a value in radians.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool AtanRad<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return AtanRad(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return AtanRad(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return AtanRad(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return AtanRad(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return AtanRad(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return AtanRad(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return AtanRad(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return AtanRad(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return AtanRad(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return AtanRad(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return AtanRad(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.TrigOperation(inVal, op: 5, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return AtanRad<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct AtanRadVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = AtanRad<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
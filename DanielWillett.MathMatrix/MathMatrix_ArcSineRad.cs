using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        AsinRadVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, AsinRadVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Asin(inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Asin(inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Asin(inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Asin(inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Asin(inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Asin(inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Asin(inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Asin(inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Asin(inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Asin(inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Asin((double)inVal));
        return true;
    }

    /// <inheritdoc cref="AsinRad{TIn,TVisitor,TIdealOut}"/>
    public static bool AsinRad<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return AsinRad<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs an inverse sine operation returning a value in radians.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool AsinRad<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return AsinRad(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return AsinRad(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return AsinRad(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return AsinRad(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return AsinRad(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return AsinRad(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return AsinRad(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return AsinRad(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return AsinRad(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return AsinRad(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return AsinRad(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.TrigOperation(inVal, op: 3, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return AsinRad<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct AsinRadVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = AsinRad<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
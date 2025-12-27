using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        RoundVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, RoundVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }
    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Round(inVal));
        return true;
    }

    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Round(inVal));
        return true;
    }

    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(decimal.Round(inVal));
        return true;
    }

    /// <inheritdoc cref="Round{TIn,TVisitor,TIdealOut}"/>
    public static bool Round<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return Round<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs a round operation.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Round<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return Round(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return Round(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return Round(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return Round(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return Round(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return Round(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return Round(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return Round(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return Round(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return Round(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return Round(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.Round(inVal);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return Round<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct RoundVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Round<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
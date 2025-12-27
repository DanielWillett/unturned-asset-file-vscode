using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        AbsVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, AbsVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(AbsSafe(inVal));
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(AbsSafe(inVal));
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(AbsSafe(inVal));
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(AbsSafe(inVal));
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Abs(inVal));
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Abs(inVal));
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
#if NET7_0_OR_GREATER
        visitor.Accept(decimal.Abs(inVal));
#else
        visitor.Accept(Math.Abs(inVal));
#endif
        return true;
    }

    /// <inheritdoc cref="Abs{TIn,TVisitor,TIdealOut}"/>
    public static bool Abs<TIn, TVisitor>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return Abs<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs an absolute value operation.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Abs<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return Abs(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return Abs(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return Abs(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return Abs(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return Abs(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return Abs(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return Abs(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return Abs(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return Abs(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return Abs(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return Abs(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.Absolute(inVal);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return Abs<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct AbsVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Abs<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}

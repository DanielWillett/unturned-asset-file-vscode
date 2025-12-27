using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        CeilingVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, CeilingVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Ceiling(inVal));
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Ceiling(inVal));
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(decimal.Ceiling(inVal));
        return true;
    }

    /// <inheritdoc cref="Ceiling{TIn,TVisitor,TIdealOut}"/>
    public static bool Ceiling<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return Ceiling<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs a ceiling operation.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Ceiling<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return Ceiling(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return Ceiling(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return Ceiling(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return Ceiling(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return Ceiling(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return Ceiling(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return Ceiling(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return Ceiling(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return Ceiling(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return Ceiling(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return Ceiling(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.Ceiling(inVal);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return Ceiling<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct CeilingVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Ceiling<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
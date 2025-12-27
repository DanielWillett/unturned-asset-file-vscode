using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        SqrtVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, SqrtVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sqrt(inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sqrt(inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sqrt(inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sqrt(inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sqrt(inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sqrt(inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sqrt(inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sqrt(inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Sqrt(inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sqrt(inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sqrt((double)inVal));
        return true;
    }

    /// <inheritdoc cref="Sqrt{TIn,TVisitor,TIdealOut}"/>
    public static bool Sqrt<TIn, TVisitor>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return Sqrt<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs an absolute value operation.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Sqrt<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return Sqrt(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return Sqrt(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return Sqrt(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return Sqrt(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return Sqrt(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return Sqrt(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return Sqrt(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return Sqrt(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return Sqrt(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return Sqrt(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return Sqrt(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.Sqrt(inVal);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return Sqrt<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct SqrtVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Sqrt<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}

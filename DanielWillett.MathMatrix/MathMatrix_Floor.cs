using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        FloorVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, FloorVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inVal);
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Floor(inVal));
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Floor(inVal));
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(decimal.Floor(inVal));
        return true;
    }

    /// <inheritdoc cref="Floor{TIn,TVisitor,TIdealOut}"/>
    public static bool Floor<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return Floor<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs a floor operation.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Floor<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return Floor(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return Floor(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return Floor(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return Floor(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return Floor(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return Floor(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return Floor(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return Floor(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return Floor(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return Floor(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return Floor(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.Floor(inVal);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return Floor<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct FloorVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Floor<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
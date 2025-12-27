using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        AtanDegVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, AtanDegVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Atan(inVal) * (180f / MathF.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan((double)inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AtanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AtanDeg<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return AtanDeg<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs an inverse sine operation returning a value in degrees.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool AtanDeg<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return AtanDeg(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return AtanDeg(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return AtanDeg(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return AtanDeg(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return AtanDeg(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return AtanDeg(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return AtanDeg(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return AtanDeg(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return AtanDeg(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return AtanDeg(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return AtanDeg(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.TrigOperation(inVal, op: 3, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return AtanDeg<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct AtanDegVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = AtanDeg<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
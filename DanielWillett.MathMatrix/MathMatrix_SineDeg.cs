using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    // note: most of these functions will fail with very large numbers due to the way decimals are represented
    // see https://stackoverflow.com/questions/66030225/different-results-between-c-and-c-sharp-sin-function-with-large-values
    // for more info

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        SinDegVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, SinDegVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Sin(inVal * (MathF.PI / 180f)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sin((double)inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="SinDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool SinDeg<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return SinDeg<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs a sine operation returning a value in degrees.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool SinDeg<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return SinDeg(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return SinDeg(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return SinDeg(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return SinDeg(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return SinDeg(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return SinDeg(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return SinDeg(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return SinDeg(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return SinDeg(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return SinDeg(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return SinDeg(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.TrigOperation(inVal, op: 0, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return SinDeg<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct SinDegVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = SinDeg<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
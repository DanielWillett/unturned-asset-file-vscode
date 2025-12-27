using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    // note: most of these functions will fail with very large numbers due to the way decimals are represented
    // see https://stackoverflow.com/questions/66030225/different-results-between-c-and-c-sharp-sin-function-with-large-values
    // for more info

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        CosDegVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, CosDegVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Cos(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Cos(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Cos(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Cos(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Cos(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Cos(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Cos(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Cos(inVal * (Math.PI / 180)));
        return true;
    }
    
    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Cos(inVal * (MathF.PI / 180f)));
        return true;
    }

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Cos(inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Cos((double)inVal * (Math.PI / 180)));
        return true;
    }

    /// <inheritdoc cref="CosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool CosDeg<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return CosDeg<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs a cosine operation returning a value in degrees.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool CosDeg<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return CosDeg(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return CosDeg(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return CosDeg(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return CosDeg(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return CosDeg(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return CosDeg(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return CosDeg(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return CosDeg(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return CosDeg(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return CosDeg(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return CosDeg(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.TrigOperation(inVal, op: 1, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return CosDeg<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct CosDegVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = CosDeg<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
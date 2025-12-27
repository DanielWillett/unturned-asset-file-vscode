using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    // note: most of these functions will fail with very large numbers due to the way decimals are represented
    // see https://stackoverflow.com/questions/66030225/different-results-between-c-and-c-sharp-sin-function-with-large-values
    // for more info

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        TanDegVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, TanDegVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((inVal % 360) switch
        {
            90 => double.PositiveInfinity,
            270 => double.NegativeInfinity,
            _ => Math.Tan(inVal * (Math.PI / 180))
        });
        return true;
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((inVal % 360) switch
        {
            90 => double.PositiveInfinity,
            270 => double.NegativeInfinity,
            _ => Math.Tan(inVal * (Math.PI / 180))
        });
        return true;
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((inVal % 360) switch
        {
            90 => double.PositiveInfinity,
            270 => double.NegativeInfinity,
            _ => Math.Tan(inVal * (Math.PI / 180))
        });
        return true;
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((inVal % 360) switch
        {
            90 => double.PositiveInfinity,
            270 => double.NegativeInfinity,
            _ => Math.Tan(inVal * (Math.PI / 180))
        });
        return true;
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((inVal % 360) switch
        {
            90 or -270 => double.PositiveInfinity,
            270 or -90 => double.NegativeInfinity,
            _ => Math.Tan(inVal * (Math.PI / 180))
        });
        return true;
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((inVal % 360) switch
        {
            90 or -270 => double.PositiveInfinity,
            270 or -90 => double.NegativeInfinity,
            _ => Math.Tan(inVal * (Math.PI / 180))
        });
        return true;
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((inVal % 360) switch
        {
            90 or -270 => double.PositiveInfinity,
            270 or -90 => double.NegativeInfinity,
            _ => Math.Tan(inVal * (Math.PI / 180))
        });
        return true;
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((inVal % 360) switch
        {
            90 or -270 => double.PositiveInfinity,
            270 or -90 => double.NegativeInfinity,
            _ => Math.Tan(inVal * (Math.PI / 180))
        });
        return true;
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        inVal = (inVal % 360f + 360f) % 360f;
        visitor.Accept(inVal switch
        {
            >= 89.99999f and <= 90.000001f => float.PositiveInfinity,
            >= 269.9999f and <= 270.000001f => float.NegativeInfinity,
            _ => MathF.Tan(inVal * (MathF.PI / 180f))
        });
        return true;
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        inVal = (inVal % 360d + 360d) % 360d;
        visitor.Accept(inVal switch
        {
            >= 89.999999999999999d and <= 90.00000000000001d => double.PositiveInfinity,
            >= 269.99999999999999d and <= 270.0000000000001d => double.NegativeInfinity,
            _ => Math.Tan(inVal * (Math.PI / 180))
        });
        return true;
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return TanDeg((double)inVal, ref visitor);
    }

    /// <inheritdoc cref="TanDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool TanDeg<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return TanDeg<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs a tangent operation returning a value in degrees.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool TanDeg<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return TanDeg(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return TanDeg(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return TanDeg(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return TanDeg(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return TanDeg(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return TanDeg(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return TanDeg(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return TanDeg(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return TanDeg(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return TanDeg(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return TanDeg(As<TIn, decimal>(inVal!), ref visitor);

        if (inVal == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TIn>? vectorProvider = VectorTypes.TryGetProvider<TIn>();

        if (vectorProvider != null)
        {
            TIn abs = vectorProvider.TrigOperation(inVal, op: 2, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TIn) == typeof(string))
        {
            return TanDeg<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct TanDegVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = TanDeg<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
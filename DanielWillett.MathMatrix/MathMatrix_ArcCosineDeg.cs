using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

partial class MathMatrix
{
    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor, TIdealOut>(string inVal, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        AcosDegVisitor<TVisitor, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        ConvertToNumber<TIdealOut, AcosDegVisitor<TVisitor, TIdealOut>>(inVal, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(ulong inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Acos(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(uint inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Acos(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(ushort inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Acos(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(byte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Acos(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(long inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Acos(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(int inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Acos(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(short inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Acos(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(sbyte inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Acos(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(float inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Acos(inVal) * (180f / MathF.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(double inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Acos(inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TVisitor>(decimal inVal, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Acos((double)inVal) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="AcosDeg{TIn,TVisitor,TIdealOut}"/>
    public static bool AcosDeg<TIn, TVisitor>(TIn inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TVisitor : IGenericVisitor
    {
        return AcosDeg<TIn, TVisitor, TIn>(inVal, ref visitor);
    }

    /// <summary>
    /// Performs an inverse sine operation returning a value in degrees.
    /// </summary>
    /// <typeparam name="TIn">Input value.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool AcosDeg<TIn, TVisitor, TIdealOut>(TIn? inVal, ref TVisitor visitor)
        where TIn : IEquatable<TIn>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TIn) == typeof(ulong))
            return AcosDeg(As<TIn, ulong>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(uint))
            return AcosDeg(As<TIn, uint>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(ushort))
            return AcosDeg(As<TIn, ushort>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(byte))
            return AcosDeg(As<TIn, byte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(long))
            return AcosDeg(As<TIn, long>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(int))
            return AcosDeg(As<TIn, int>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(short))
            return AcosDeg(As<TIn, short>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(sbyte))
            return AcosDeg(As<TIn, sbyte>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(float))
            return AcosDeg(As<TIn, float>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(double))
            return AcosDeg(As<TIn, double>(inVal!), ref visitor);
        if (typeof(TIn) == typeof(decimal))
            return AcosDeg(As<TIn, decimal>(inVal!), ref visitor);

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
            return AcosDeg<TVisitor, TIdealOut>(As<TIn, string>(inVal), ref visitor);
        }

        return false;
    }

    private struct AcosDegVisitor<TVisitor, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
    {
        public TVisitor Visitor;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = AcosDeg<T, TVisitor, TIdealOut>(value, ref Visitor);
        }
    }
}
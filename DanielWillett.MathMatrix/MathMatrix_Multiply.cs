using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        MultiplyXVisitor<TVisitor, TInY, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<TIdealOut, MultiplyXVisitor<TVisitor, TInY, TIdealOut>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInX, TVisitor, TIdealOut>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        MultiplyYVisitor<TVisitor, TInX, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<TIdealOut, MultiplyYVisitor<TVisitor, TInX, TIdealOut>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Multiply<ulong, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX * inValY)); }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX * inValY)); }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX * inValY)); }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX * inValY)); }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValY >= 0)
            {
                ulong y = (ulong)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else if (inValX <= long.MaxValue)
            {
                long x = (long)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else
                return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValY >= 0)
            {
                ulong y = (ulong)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else if (inValX <= long.MaxValue)
            {
                long x = (long)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else
                return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValY >= 0)
            {
                ulong y = (ulong)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else if (inValX <= long.MaxValue)
            {
                long x = (long)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else
                return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValY >= 0)
            {
                ulong y = (ulong)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else if (inValX <= long.MaxValue)
            {
                long x = (long)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else
                return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Multiply<uint, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX * inValY)); }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        ulong res = (ulong)inValX * inValY;
        if (res <= uint.MaxValue)
            visitor.Accept((uint)res);
        else
            visitor.Accept(res);
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        ulong res = (ulong)inValX * inValY;
        if (res <= uint.MaxValue)
            visitor.Accept((uint)res);
        else
            visitor.Accept(res);
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        ulong res = (ulong)inValX * inValY;
        if (res <= uint.MaxValue)
            visitor.Accept((uint)res);
        else
            visitor.Accept(res);
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValY >= 0)
            {
                ulong y = (ulong)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else
            {
                long x = inValX;
                visitor.Accept(checked(x * inValY));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((double)inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Multiply<ushort, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX * inValY)); }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        ulong res = (ulong)inValX * inValY;
        if (res <= uint.MaxValue)
            visitor.Accept((uint)res);
        else
            visitor.Accept(res);
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((uint)inValX * inValY);
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((uint)inValX * inValY);
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValY >= 0)
            {
                ulong y = (ulong)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else
            {
                long x = inValX;
                visitor.Accept(checked(x * inValY));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = inValX * (long)inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Multiply<byte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX * inValY)); }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        ulong res = (ulong)inValX * inValY;
        if (res <= uint.MaxValue)
            visitor.Accept((uint)res);
        else
            visitor.Accept(res);
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((uint)inValX * inValY);
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((uint)inValX * inValY);
        return true;
    }
    
    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValY >= 0)
            {
                ulong y = (ulong)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else
            {
                long x = inValX;
                visitor.Accept(checked(x * inValY));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = inValX * (long)inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Multiply<long, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0)
            {
                ulong x = (ulong)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else if (inValY <= long.MaxValue)
            {
                long y = (long)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else
                return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0)
            {
                ulong x = (ulong)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else
            {
                long y = inValY;
                visitor.Accept(checked(inValX * y));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0)
            {
                ulong x = (ulong)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else
            {
                long y = inValY;
                visitor.Accept(checked(inValX * y));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0)
            {
                ulong x = (ulong)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else
            {
                long y = inValY;
                visitor.Accept(checked(inValX * y));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0 && inValY >= 0)
            {
                ulong x = (ulong)inValX, y = (ulong)inValY;
                visitor.Accept(checked(x * y));
            }
            else
            {
                visitor.Accept(checked(inValX * inValY));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0 && inValY >= 0)
            {
                ulong x = (ulong)inValX, y = (ulong)inValY;
                visitor.Accept(checked(x * y));
            }
            else
            {
                visitor.Accept(checked(inValX * inValY));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0 && inValY >= 0)
            {
                ulong x = (ulong)inValX, y = (ulong)inValY;
                visitor.Accept(checked(x * y));
            }
            else
            {
                visitor.Accept(checked(inValX * inValY));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0 && inValY >= 0)
            {
                ulong x = (ulong)inValX, y = (ulong)inValY;
                visitor.Accept(checked(x * y));
            }
            else
            {
                visitor.Accept(checked(inValX * inValY));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Multiply<int, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0)
            {
                ulong x = (ulong)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else if (inValY <= long.MaxValue)
            {
                long y = (long)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else
                return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = (long)inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = (long)inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0 && inValY >= 0)
            {
                ulong x = (ulong)inValX, y = (ulong)inValY;
                visitor.Accept(checked(x * y));
            }
            else
            {
                visitor.Accept(checked(inValX * inValY));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = (long)inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = (long)inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = (long)inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Multiply<short, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0)
            {
                ulong x = (ulong)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else if (inValY <= long.MaxValue)
            {
                long y = (long)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else
                return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0 && inValY >= 0)
            {
                ulong x = (ulong)inValX, y = (ulong)inValY;
                visitor.Accept(checked(x * y));
            }
            else
            {
                visitor.Accept(checked(inValX * inValY));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = (long)inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Multiply<sbyte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0)
            {
                ulong x = (ulong)inValX;
                visitor.Accept(checked(x * inValY));
            }
            else if (inValY <= long.MaxValue)
            {
                long y = (long)inValY;
                visitor.Accept(checked(inValX * y));
            }
            else
                return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try
        {
            if (inValX >= 0 && inValY >= 0)
            {
                ulong x = (ulong)inValX, y = (ulong)inValY;
                visitor.Accept(checked(x * y));
            }
            else
            {
                visitor.Accept(checked(inValX * inValY));
            }
        }
        catch (OverflowException)
        {
            return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
        }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        long res = (long)inValX * inValY;
        if (res is <= int.MaxValue and >= int.MinValue)
            visitor.Accept((int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Multiply<float, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Multiply<double, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX * inValY);
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInY, TVisitor, TIdealOut>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Multiply(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Multiply(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Multiply(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Multiply(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Multiply(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Multiply(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Multiply(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Multiply(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Multiply(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Multiply(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Multiply(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Multiply(vectorProvider.Construct((double)inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Multiply<decimal, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Multiply(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(inValX * inValY); }
        catch (OverflowException) { visitor.Accept((double)inValX * (double)inValY); }
        return true;
    }

    /// <inheritdoc cref="Multiply{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Multiply<TInX, TInY, TVisitor>(TInX inValX, TInY inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInX) == typeof(string))
        {
            return Multiply<TInX, TInY, TVisitor, TInY>(inValX, inValY, ref visitor);
        }

        return Multiply<TInX, TInY, TVisitor, TInX>(inValX, inValY, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Multiply<TInX, TInY, TVisitor, TIdealOut>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, decimal>(inValX!), inValY, ref visitor);

        if (inValX == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInX>? vectorProviderX = VectorTypes.TryGetProvider<TInX>();

        if (vectorProviderX != null)
        {
            if (typeof(TInX) == typeof(TInY))
            {
                TInX abs = vectorProviderX.Multiply(inValX, As<TInY, TInX>(inValY));
                visitor.Accept(abs);
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                if (typeof(TIdealOut) == typeof(TInY))
                {
                    TInY converted = ConvertVector<TInY, TInX>(vectorProviderY, vectorProviderX, inValX);
                    TInY abs = vectorProviderY.Multiply(converted, inValY)!;
                    visitor.Accept(abs);
                    return true;
                }
                if (typeof(TIdealOut) != typeof(TInX) && VectorTypes.TryGetProvider<TIdealOut>() is { } idealVectorProvider)
                {
                    TIdealOut convertedX = ConvertVector<TIdealOut, TInX>(idealVectorProvider, vectorProviderX, inValX);
                    TIdealOut convertedY = ConvertVector<TIdealOut, TInY>(idealVectorProvider, vectorProviderY, inValY);
                    TIdealOut abs = idealVectorProvider.Multiply(convertedX, convertedY)!;
                    visitor.Accept(abs);
                    return true;
                }
                else
                {
                    TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                    TInX abs = vectorProviderX.Multiply(inValX, converted)!;
                    visitor.Accept(abs);
                    return true;
                }
            }
            else
            {
                TInX abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct(As<TInY, ulong>(inValY!)));
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct(As<TInY, uint>(inValY!)));
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct(As<TInY, ushort>(inValY!)));
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct(As<TInY, byte>(inValY!)));
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct(As<TInY, long>(inValY!)));
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct(As<TInY, int>(inValY!)));
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct(As<TInY, short>(inValY!)));
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct(As<TInY, sbyte>(inValY!)));
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct(As<TInY, float>(inValY!)));
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct(As<TInY, double>(inValY!)));
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Multiply(inValX, vectorProviderX.Construct((double)As<TInY, decimal>(inValY!)));
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Multiply<TInY, TVisitor, TIdealOut>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct MultiplyXVisitor<TVisitor, TInY, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Multiply<T, TInY, TVisitor, TIdealOut>(value, ValueY, ref Visitor);
        }
    }

    private struct MultiplyYVisitor<TVisitor, TInX, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Multiply<TInX, T, TVisitor, TIdealOut>(ValueX, value, ref Visitor);
        }
    }
}
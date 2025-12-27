using System;
// ReSharper disable IntVariableOverflowInUncheckedContext

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        DivideXVisitor<TVisitor, TInY, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<TIdealOut, DivideXVisitor<TVisitor, TInY, TIdealOut>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInX, TVisitor, TIdealOut>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        DivideYVisitor<TVisitor, TInX, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<TIdealOut, DivideYVisitor<TVisitor, TInX, TIdealOut>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Divide<ulong, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    private static bool DivideByZero<TVisitor>(ref TVisitor visitor, bool isZero, bool positive = true) where TVisitor : IGenericVisitor
    {
        visitor.Accept(isZero ? double.NaN : positive ? double.PositiveInfinity : double.NegativeInfinity);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        ulong yOpp = (ulong)-inValY;
        ulong res = inValX / yOpp;
        if (res <= long.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(long)res);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (uint)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        ulong res = inValX / yOpp;
        if (res <= long.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(long)res);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (ushort)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        ulong res = inValX / yOpp;
        if (res <= long.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(long)res);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (byte)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        ulong res = inValX / yOpp;
        if (res <= long.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(long)res);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Divide<uint, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        long yOpp = -inValY;
        uint res = (uint)(inValX / yOpp);
        if (inValY >= -uint.MaxValue && res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (uint)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        uint res = inValX / yOpp;
        if (res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (ushort)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        uint res = inValX / yOpp;
        if (res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (byte)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        uint res = inValX / yOpp;
        if (res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Divide<ushort, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        long yOpp = -inValY;
        uint res = (uint)(inValX / yOpp);
        if (inValY >= -uint.MaxValue && res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (uint)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        uint res = inValX / yOpp;
        if (res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (ushort)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        uint res = inValX / yOpp;
        if (res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (byte)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        uint res = inValX / yOpp;
        if (res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Divide<byte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }
    
    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        long yOpp = -inValY;
        uint res = (uint)(inValX / yOpp);
        if (inValY >= -uint.MaxValue && res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (uint)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        uint res = inValX / yOpp;
        if (res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (ushort)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        uint res = inValX / yOpp;
        if (res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0);
        if (inValY > 0)
            return Divide(inValX, (ushort)inValY, ref visitor);

        uint yOpp = (uint)-inValY;
        uint res = inValX / yOpp;
        if (res <= int.MaxValue + 1ul && inValX % yOpp == 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Divide<long, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        long y = (long)inValY;
        if (inValY <= long.MaxValue && inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        long y = inValY;
        if (inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        long y = inValY;
        if (inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        long y = inValY;
        if (inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Divide<int, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        long y = (long)inValY;
        if (inValY <= long.MaxValue && inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        int y = (int)inValY;
        if (inValY <= int.MaxValue && inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        int y = inValY;
        if (inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        int y = inValY;
        if (inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (uint)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (ushort)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (byte)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Divide<short, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        long y = (long)inValY;
        if (inValY <= long.MaxValue && inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        int y = (int)inValY;
        if (inValY <= int.MaxValue && inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        int y = inValY;
        if (inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        int y = inValY;
        if (inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (uint)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (ushort)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (ushort)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Divide<sbyte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        long y = (long)inValY;
        if (inValY <= long.MaxValue && inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        int y = (int)inValY;
        if (inValY <= int.MaxValue && inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        int y = inValY;
        if (inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        int y = inValY;
        if (inValX % y == 0)
            visitor.Accept(inValX / y);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (ulong)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept(new decimal(inValX) / new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (uint)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (uint)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor, inValX == 0, inValX >= 0);
        if (inValY > 0)
            return Divide(inValX, (uint)inValY, ref visitor);

        if (inValX % inValY == 0)
            visitor.Accept(inValX / inValY);
        else
            visitor.Accept((double)inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Divide<float, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Divide<double, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInY, TVisitor, TIdealOut>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Divide(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Divide(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Divide(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Divide(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Divide(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Divide(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Divide(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Divide(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Divide(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Divide(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Divide(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Divide(vectorProvider.Construct((double)inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Divide<decimal, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Divide(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == decimal.Zero)
            return DivideByZero(ref visitor, inValX == decimal.Zero, inValX >= decimal.Zero);
        visitor.Accept(inValX / inValY);
        return true;
    }

    /// <inheritdoc cref="Divide{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Divide<TInX, TInY, TVisitor>(TInX inValX, TInY inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInX) == typeof(string))
        {
            return Divide<TInX, TInY, TVisitor, TInY>(inValX, inValY, ref visitor);
        }

        return Divide<TInX, TInY, TVisitor, TInX>(inValX, inValY, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Divide<TInX, TInY, TVisitor, TIdealOut>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, decimal>(inValX!), inValY, ref visitor);

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
                TInX abs = vectorProviderX.Divide(inValX, As<TInY, TInX>(inValY));
                visitor.Accept(abs);
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                if (typeof(TIdealOut) == typeof(TInY))
                {
                    TInY converted = ConvertVector<TInY, TInX>(vectorProviderY, vectorProviderX, inValX);
                    TInY abs = vectorProviderY.Divide(converted, inValY)!;
                    visitor.Accept(abs);
                    return true;
                }
                if (typeof(TIdealOut) != typeof(TInX) && VectorTypes.TryGetProvider<TIdealOut>() is { } idealVectorProvider)
                {
                    TIdealOut convertedX = ConvertVector<TIdealOut, TInX>(idealVectorProvider, vectorProviderX, inValX);
                    TIdealOut convertedY = ConvertVector<TIdealOut, TInY>(idealVectorProvider, vectorProviderY, inValY);
                    TIdealOut abs = idealVectorProvider.Divide(convertedX, convertedY)!;
                    visitor.Accept(abs);
                    return true;
                }
                else
                {
                    TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                    TInX abs = vectorProviderX.Divide(inValX, converted)!;
                    visitor.Accept(abs);
                    return true;
                }
            }
            else
            {
                TInX abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct(As<TInY, ulong>(inValY!)));
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct(As<TInY, uint>(inValY!)));
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct(As<TInY, ushort>(inValY!)));
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct(As<TInY, byte>(inValY!)));
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct(As<TInY, long>(inValY!)));
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct(As<TInY, int>(inValY!)));
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct(As<TInY, short>(inValY!)));
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct(As<TInY, sbyte>(inValY!)));
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct(As<TInY, float>(inValY!)));
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct(As<TInY, double>(inValY!)));
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Divide(inValX, vectorProviderX.Construct((double)As<TInY, decimal>(inValY!)));
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Divide<TInY, TVisitor, TIdealOut>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct DivideXVisitor<TVisitor, TInY, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Divide<T, TInY, TVisitor, TIdealOut>(value, ValueY, ref Visitor);
        }
    }

    private struct DivideYVisitor<TVisitor, TInX, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Divide<TInX, T, TVisitor, TIdealOut>(ValueX, value, ref Visitor);
        }
    }
}
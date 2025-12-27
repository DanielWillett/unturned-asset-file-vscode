using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        SubtractXVisitor<TVisitor, TInY, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<TIdealOut, SubtractXVisitor<TVisitor, TInY, TIdealOut>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInX, TVisitor, TIdealOut>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        SubtractYVisitor<TVisitor, TInX, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<TIdealOut, SubtractYVisitor<TVisitor, TInX, TIdealOut>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Subtract<ulong, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
        {
            visitor.Accept(inValX - inValY);
        }
        else
        {
            ulong opposite = inValY - inValX;
            if (opposite <= long.MaxValue + 1ul)
                visitor.Accept(-(long)opposite);
            else
                visitor.Accept(-new decimal(opposite));
        }
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
        {
            visitor.Accept(inValX - inValY);
        }
        else
        {
            uint opposite = inValY - (uint)inValX;
            if (opposite <= int.MaxValue + 1u)
                visitor.Accept(-(int)opposite);
            else
                visitor.Accept(-opposite);
        }
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
            visitor.Accept(inValX - inValY);
        else
            visitor.Accept(-(int)(inValY - (uint)inValX));
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
            visitor.Accept(inValX - inValY);
        else
            visitor.Accept(-(int)(inValY - (uint)inValX));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (ulong)-inValY, ref visitor)
            : Subtract(inValX, (ulong)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (uint)-inValY, ref visitor)
            : Subtract(inValX, (uint)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (ushort)-inValY, ref visitor)
            : Subtract(inValX, (ushort)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (byte)-inValY, ref visitor)
            : Subtract(inValX, (byte)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Subtract<uint, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
        {
            visitor.Accept(inValX - (uint)inValY);
        }
        else
        {
            ulong opposite = inValY - inValX;
            if (opposite <= long.MaxValue + 1ul)
                visitor.Accept(-(long)opposite);
            else
                visitor.Accept(-new decimal(opposite));
        }
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
        {
            visitor.Accept(inValX - inValY);
        }
        else
        {
            uint opposite = inValY - inValX;
            if (opposite <= int.MaxValue + 1u)
                visitor.Accept(-(int)opposite);
            else
                visitor.Accept(-opposite);
        }
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
            visitor.Accept(inValX - inValY);
        else
            visitor.Accept(-(int)(inValY - inValX));
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
            visitor.Accept(inValX - inValY);
        else
            visitor.Accept(-(int)(inValY - inValX));
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (ulong)-inValY, ref visitor)
            : Subtract(inValX, (ulong)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (uint)-inValY, ref visitor)
            : Subtract(inValX, (uint)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (ushort)-inValY, ref visitor)
            : Subtract(inValX, (ushort)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (byte)-inValY, ref visitor)
            : Subtract(inValX, (byte)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((double)inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Subtract<ushort, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
        {
            visitor.Accept(inValX - (uint)inValY);
        }
        else
        {
            ulong opposite = inValY - inValX;
            if (opposite <= long.MaxValue + 1ul)
                visitor.Accept(-(long)opposite);
            else
                visitor.Accept(-new decimal(opposite));
        }
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
        {
            visitor.Accept(inValX - inValY);
        }
        else
        {
            uint opposite = inValY - inValX;
            if (opposite <= int.MaxValue + 1u)
                visitor.Accept(-(int)opposite);
            else
                visitor.Accept(-opposite);
        }
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (ulong)-inValY, ref visitor)
            : Subtract(inValX, (ulong)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (uint)-inValY, ref visitor)
            : Subtract(inValX, (uint)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Subtract<byte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
        {
            visitor.Accept(inValX - (uint)inValY);
        }
        else
        {
            ulong opposite = inValY - inValX;
            if (opposite <= long.MaxValue + 1ul)
                visitor.Accept(-(long)opposite);
            else
                visitor.Accept(-new decimal(opposite));
        }
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= inValY)
        {
            visitor.Accept(inValX - inValY);
        }
        else
        {
            uint opposite = inValY - inValX;
            if (opposite <= int.MaxValue + 1u)
                visitor.Accept(-(int)opposite);
            else
                visitor.Accept(-opposite);
        }
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }
    
    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (ulong)-inValY, ref visitor)
            : Subtract(inValX, (ulong)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return inValY < 0
            ? Add(inValX, (uint)-inValY, ref visitor)
            : Subtract(inValX, (uint)inValY, ref visitor);
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Subtract<long, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            ulong x2 = (ulong)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - inValY);
            }
            else
            {
                ulong opposite = inValY - x2;
                if (opposite <= long.MaxValue + 1ul)
                    visitor.Accept(-(long)opposite);
                else
                    visitor.Accept(-new decimal(opposite));
            }
        }
        else
        {
            ulong xOpposite = (ulong)-inValX;
            try
            {
                ulong diff = checked(xOpposite + inValY);
                if (diff > long.MaxValue + 1ul)
                    visitor.Accept(-new decimal(diff));
                else
                    visitor.Accept(-(long)diff);
            }
            catch (OverflowException) { visitor.Accept(-(new decimal(xOpposite) + new decimal(inValY))); }
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            ulong x2 = (ulong)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - inValY);
            }
            else
            {
                uint opposite = inValY - (uint)x2;
                if (opposite <= int.MaxValue + 1ul)
                    visitor.Accept(-(int)opposite);
                else
                    visitor.Accept(-opposite);
            }
        }
        else
        {
            ulong xOpposite = (ulong)-inValX;
            ulong diff = xOpposite + inValY;
            if (diff > long.MaxValue + 1ul)
                visitor.Accept(-new decimal(diff));
            else
                visitor.Accept(-(long)diff);
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            ulong x2 = (ulong)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - inValY);
            }
            else
            {
                visitor.Accept(-(int)(inValY - (uint)x2));
            }
        }
        else
        {
            ulong xOpposite = (ulong)-inValX;
            ulong diff = xOpposite + inValY;
            if (diff > long.MaxValue + 1ul)
                visitor.Accept(-new decimal(diff));
            else
                visitor.Accept(-(long)diff);
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            ulong x2 = (ulong)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - inValY);
            }
            else
            {
                visitor.Accept(-(int)(inValY - (uint)x2));
            }
        }
        else
        {
            ulong xOpposite = (ulong)-inValX;
            ulong diff = xOpposite + inValY;
            if (diff > long.MaxValue + 1ul)
                visitor.Accept(-new decimal(diff));
            else
                visitor.Accept(-(long)diff);
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((ulong)inValX, inValY, ref visitor);
        }

        ulong xOpposite = (ulong)-inValX;
        if (inValY < 0)
        {
            // n-n
            ulong yOpposite = (ulong)-inValY;
            if (xOpposite >= yOpposite)
            {
                ulong diff = xOpposite - yOpposite;
                if (diff > long.MaxValue + 1ul)
                    visitor.Accept(-new decimal(diff));
                else
                    visitor.Accept(-(long)diff);
            }
            else
            {
                visitor.Accept(yOpposite - xOpposite);
            }
        }
        else
        {
            // n-p
            ulong y2 = (ulong)inValY;
            ulong diff = xOpposite + y2;
            if (diff > long.MaxValue + 1ul)
                visitor.Accept(-new decimal(diff));
            else
                visitor.Accept(-(long)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((ulong)inValX, inValY, ref visitor);
        }

        ulong xOpposite = (ulong)-inValX;
        if (inValY < 0)
        {
            // n-n
            uint yOpposite = (uint)-inValY;
            if (xOpposite >= yOpposite)
            {
                ulong diff = xOpposite - yOpposite;
                if (diff > long.MaxValue + 1ul)
                    visitor.Accept(-new decimal(diff));
                else
                    visitor.Accept(-(long)diff);
            }
            else
            {
                visitor.Accept(yOpposite - (uint)xOpposite);
            }
        }
        else
        {
            // n-p
            uint y2 = (uint)inValY;
            ulong diff = xOpposite + y2;
            if (diff > long.MaxValue + 1ul)
                visitor.Accept(-new decimal(diff));
            else
                visitor.Accept(-(long)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((ulong)inValX, inValY, ref visitor);
        }

        ulong xOpposite = (ulong)-inValX;
        if (inValY < 0)
        {
            // n-n
            uint yOpposite = (uint)-inValY;
            if (xOpposite >= yOpposite)
            {
                ulong diff = xOpposite - yOpposite;
                if (diff > long.MaxValue + 1ul)
                    visitor.Accept(-new decimal(diff));
                else
                    visitor.Accept(-(long)diff);
            }
            else
            {
                visitor.Accept(yOpposite - (uint)xOpposite);
            }
        }
        else
        {
            // n-p
            uint y2 = (uint)inValY;
            ulong diff = xOpposite + y2;
            if (diff > long.MaxValue + 1ul)
                visitor.Accept(-new decimal(diff));
            else
                visitor.Accept(-(long)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((ulong)inValX, inValY, ref visitor);
        }

        ulong xOpposite = (ulong)-inValX;
        if (inValY < 0)
        {
            // n-n
            uint yOpposite = (uint)-inValY;
            if (xOpposite >= yOpposite)
            {
                ulong diff = xOpposite - yOpposite;
                if (diff > long.MaxValue + 1ul)
                    visitor.Accept(-new decimal(diff));
                else
                    visitor.Accept(-(long)diff);
            }
            else
            {
                visitor.Accept(yOpposite - (uint)xOpposite);
            }
        }
        else
        {
            // n-p
            uint y2 = (uint)inValY;
            ulong diff = xOpposite + y2;
            if (diff > long.MaxValue + 1ul)
                visitor.Accept(-new decimal(diff));
            else
                visitor.Accept(-(long)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Subtract<int, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            uint x2 = (uint)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - (uint)inValY);
            }
            else
            {
                ulong opposite = inValY - x2;
                if (opposite <= long.MaxValue + 1ul)
                    visitor.Accept(-(long)opposite);
                else
                    visitor.Accept(-new decimal(opposite));
            }
        }
        else
        {
            uint xOpposite = (uint)-inValX;
            try
            {
                ulong diff = checked(xOpposite + inValY);
                if (diff > long.MaxValue + 1ul)
                    visitor.Accept(-new decimal(diff));
                else
                    visitor.Accept(-(long)diff);
            }
            catch (OverflowException) { visitor.Accept(-(new decimal(xOpposite) + new decimal(inValY))); }
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            uint x2 = (uint)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - inValY);
            }
            else
            {
                uint opposite = inValY - x2;
                if (opposite <= int.MaxValue + 1ul)
                    visitor.Accept(-(int)opposite);
                else
                    visitor.Accept(-opposite);
            }
        }
        else
        {
            uint xOpposite = (uint)-inValX;
            ulong diff = (ulong)xOpposite + inValY;
            if (diff > int.MaxValue + 1u)
                visitor.Accept(-(long)diff);
            else
                visitor.Accept(-(int)diff);
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            uint x2 = (uint)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - inValY);
            }
            else
            {
                visitor.Accept(-(int)(inValY - x2));
            }
        }
        else
        {
            uint xOpposite = (uint)-inValX;
            uint diff = xOpposite + inValY;
            if (diff > int.MaxValue + 1u)
                visitor.Accept(-diff);
            else
                visitor.Accept((int)-diff);
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            uint x2 = (uint)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - inValY);
            }
            else
            {
                visitor.Accept(-(int)(inValY - x2));
            }
        }
        else
        {
            uint xOpposite = (uint)-inValX;
            uint diff = xOpposite + inValY;
            if (diff > int.MaxValue + 1u)
                visitor.Accept(-diff);
            else
                visitor.Accept((int)-diff);
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((uint)inValX, inValY, ref visitor);
        }

        uint xOpposite = (uint)-inValX;
        if (inValY < 0)
        {
            // n-n
            ulong yOpposite = (ulong)-inValY;
            if (xOpposite >= yOpposite)
            {
                uint diff = xOpposite - (uint)yOpposite;
                if (diff > int.MaxValue + 1ul)
                    visitor.Accept(-diff);
                else
                    visitor.Accept(-(int)diff);
            }
            else
            {
                visitor.Accept(yOpposite - xOpposite);
            }
        }
        else
        {
            // n-p
            ulong y2 = (ulong)inValY;
            ulong diff = xOpposite + y2;
            if (diff > long.MaxValue + 1ul)
                visitor.Accept(-new decimal(diff));
            else
                visitor.Accept(-(long)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((uint)inValX, inValY, ref visitor);
        }

        uint xOpposite = (uint)-inValX;
        if (inValY < 0)
        {
            // n-n
            uint yOpposite = (uint)-inValY;
            if (xOpposite >= yOpposite)
            {
                uint diff = xOpposite - yOpposite;
                if (diff > int.MaxValue + 1u)
                    visitor.Accept(-diff);
                else
                    visitor.Accept(-(int)diff);
            }
            else
            {
                visitor.Accept(yOpposite - xOpposite);
            }
        }
        else
        {
            // n-p
            uint diff = xOpposite + (uint)inValY;
            if (diff > int.MaxValue + 1u)
                visitor.Accept(-diff);
            else
                visitor.Accept(-(int)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((uint)inValX, inValY, ref visitor);
        }

        uint xOpposite = (uint)-inValX;
        if (inValY < 0)
        {
            // n-n
            uint yOpposite = (uint)-inValY;
            if (xOpposite >= yOpposite)
            {
                uint diff = xOpposite - yOpposite;
                if (diff > int.MaxValue + 1u)
                    visitor.Accept(-diff);
                else
                    visitor.Accept(-(int)diff);
            }
            else
            {
                visitor.Accept(yOpposite - xOpposite);
            }
        }
        else
        {
            // n-p
            uint diff = xOpposite + (uint)inValY;
            if (diff > int.MaxValue + 1u)
                visitor.Accept(-diff);
            else
                visitor.Accept(-(int)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((uint)inValX, inValY, ref visitor);
        }

        uint xOpposite = (uint)-inValX;
        if (inValY < 0)
        {
            // n-n
            uint yOpposite = (uint)-inValY;
            if (xOpposite >= yOpposite)
            {
                uint diff = xOpposite - yOpposite;
                if (diff > int.MaxValue + 1u)
                    visitor.Accept(-diff);
                else
                    visitor.Accept(-(int)diff);
            }
            else
            {
                visitor.Accept(yOpposite - xOpposite);
            }
        }
        else
        {
            // n-p
            uint diff = xOpposite + (uint)inValY;
            if (diff > int.MaxValue + 1u)
                visitor.Accept(-diff);
            else
                visitor.Accept(-(int)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Subtract<short, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            uint x2 = (uint)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - (uint)inValY);
            }
            else
            {
                ulong opposite = inValY - x2;
                if (opposite <= long.MaxValue + 1ul)
                    visitor.Accept(-(long)opposite);
                else
                    visitor.Accept(-new decimal(opposite));
            }
        }
        else
        {
            uint xOpposite = (uint)-inValX;
            try
            {
                ulong diff = checked(xOpposite + inValY);
                if (diff > long.MaxValue + 1ul)
                    visitor.Accept(-new decimal(diff));
                else
                    visitor.Accept(-(long)diff);
            }
            catch (OverflowException) { visitor.Accept(-(new decimal(xOpposite) + new decimal(inValY))); }
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            uint x2 = (uint)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - inValY);
            }
            else
            {
                uint opposite = inValY - x2;
                if (opposite <= int.MaxValue + 1ul)
                    visitor.Accept(-(int)opposite);
                else
                    visitor.Accept(-opposite);
            }
        }
        else
        {
            uint xOpposite = (uint)-inValX;
            ulong diff = (ulong)xOpposite + inValY;
            if (diff > int.MaxValue + 1u)
                visitor.Accept(-(long)diff);
            else
                visitor.Accept(-(int)diff);
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((ushort)inValX, inValY, ref visitor);
        }

        uint xOpposite = (uint)-inValX;
        if (inValY < 0)
        {
            // n-n
            ulong yOpposite = (ulong)-inValY;
            if (xOpposite >= yOpposite)
            {
                uint diff = xOpposite - (uint)yOpposite;
                if (diff > int.MaxValue + 1ul)
                    visitor.Accept(-diff);
                else
                    visitor.Accept(-(int)diff);
            }
            else
            {
                visitor.Accept(yOpposite - xOpposite);
            }
        }
        else
        {
            // n-p
            ulong y2 = (ulong)inValY;
            ulong diff = xOpposite + y2;
            if (diff > long.MaxValue + 1ul)
                visitor.Accept(-new decimal(diff));
            else
                visitor.Accept(-(long)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((ushort)inValX, inValY, ref visitor);
        }

        uint xOpposite = (uint)-inValX;
        if (inValY < 0)
        {
            // n-n
            uint yOpposite = (uint)-inValY;
            if (xOpposite >= yOpposite)
            {
                uint diff = xOpposite - yOpposite;
                if (diff > int.MaxValue + 1u)
                    visitor.Accept(-diff);
                else
                    visitor.Accept(-(int)diff);
            }
            else
            {
                visitor.Accept(yOpposite - xOpposite);
            }
        }
        else
        {
            // n-p
            uint diff = xOpposite + (uint)inValY;
            if (diff > int.MaxValue + 1u)
                visitor.Accept(-diff);
            else
                visitor.Accept(-(int)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Subtract<sbyte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            uint x2 = (uint)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - (uint)inValY);
            }
            else
            {
                ulong opposite = inValY - x2;
                if (opposite <= long.MaxValue + 1ul)
                    visitor.Accept(-(long)opposite);
                else
                    visitor.Accept(-new decimal(opposite));
            }
        }
        else
        {
            uint xOpposite = (uint)-inValX;
            try
            {
                ulong diff = checked(xOpposite + inValY);
                if (diff > long.MaxValue + 1ul)
                    visitor.Accept(-new decimal(diff));
                else
                    visitor.Accept(-(long)diff);
            }
            catch (OverflowException) { visitor.Accept(-(new decimal(xOpposite) + new decimal(inValY))); }
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            uint x2 = (uint)inValX;
            if (x2 >= inValY)
            {
                visitor.Accept(x2 - inValY);
            }
            else
            {
                uint opposite = inValY - x2;
                if (opposite <= int.MaxValue + 1ul)
                    visitor.Accept(-(int)opposite);
                else
                    visitor.Accept(-opposite);
            }
        }
        else
        {
            uint xOpposite = (uint)-inValX;
            ulong diff = (ulong)xOpposite + inValY;
            if (diff > int.MaxValue + 1u)
                visitor.Accept(-(long)diff);
            else
                visitor.Accept(-(int)diff);
        }
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((byte)inValX, inValY, ref visitor);
        }

        uint xOpposite = (uint)-inValX;
        if (inValY < 0)
        {
            // n-n
            ulong yOpposite = (ulong)-inValY;
            if (xOpposite >= yOpposite)
            {
                uint diff = xOpposite - (uint)yOpposite;
                if (diff > int.MaxValue + 1ul)
                    visitor.Accept(-diff);
                else
                    visitor.Accept(-(int)diff);
            }
            else
            {
                visitor.Accept(yOpposite - xOpposite);
            }
        }
        else
        {
            // n-p
            ulong y2 = (ulong)inValY;
            ulong diff = xOpposite + y2;
            if (diff > long.MaxValue + 1ul)
                visitor.Accept(-new decimal(diff));
            else
                visitor.Accept(-(long)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX >= 0)
        {
            return Subtract((byte)inValX, inValY, ref visitor);
        }

        uint xOpposite = (uint)-inValX;
        if (inValY < 0)
        {
            // n-n
            uint yOpposite = (uint)-inValY;
            if (xOpposite >= yOpposite)
            {
                uint diff = xOpposite - yOpposite;
                if (diff > int.MaxValue + 1u)
                    visitor.Accept(-diff);
                else
                    visitor.Accept(-(int)diff);
            }
            else
            {
                visitor.Accept(yOpposite - xOpposite);
            }
        }
        else
        {
            // n-p
            uint diff = xOpposite + (uint)inValY;
            if (diff > int.MaxValue + 1u)
                visitor.Accept(-diff);
            else
                visitor.Accept(-(int)diff);
        }

        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Subtract<float, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Subtract<double, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInY, TVisitor, TIdealOut>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Subtract(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Subtract(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Subtract(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Subtract(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Subtract(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Subtract(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Subtract(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Subtract(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Subtract(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Subtract(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Subtract(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Subtract(vectorProvider.Construct((double)inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Subtract<decimal, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX - inValY);
        return true;
    }

    /// <inheritdoc cref="Subtract{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Subtract<TInX, TInY, TVisitor>(TInX inValX, TInY inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        return Subtract<TInX, TInY, TVisitor, TInX>(inValX, inValY, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Subtract<TInX, TInY, TVisitor, TIdealOut>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, decimal>(inValX!), inValY, ref visitor);

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
                TInX abs = vectorProviderX.Subtract(inValX, As<TInY, TInX>(inValY));
                visitor.Accept(abs);
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                if (typeof(TIdealOut) == typeof(TInY))
                {
                    TInY converted = ConvertVector<TInY, TInX>(vectorProviderY, vectorProviderX, inValX);
                    TInY abs = vectorProviderY.Subtract(converted, inValY)!;
                    visitor.Accept(abs);
                    return true;
                }
                if (typeof(TIdealOut) != typeof(TInX) && VectorTypes.TryGetProvider<TIdealOut>() is { } idealVectorProvider)
                {
                    TIdealOut convertedX = ConvertVector<TIdealOut, TInX>(idealVectorProvider, vectorProviderX, inValX);
                    TIdealOut convertedY = ConvertVector<TIdealOut, TInY>(idealVectorProvider, vectorProviderY, inValY);
                    TIdealOut abs = idealVectorProvider.Subtract(convertedX, convertedY)!;
                    visitor.Accept(abs);
                    return true;
                }
                else
                {
                    TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                    TInX abs = vectorProviderX.Subtract(inValX, converted)!;
                    visitor.Accept(abs);
                    return true;
                }
            }
            else
            {
                TInX abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct(As<TInY, ulong>(inValY!)));
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct(As<TInY, uint>(inValY!)));
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct(As<TInY, ushort>(inValY!)));
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct(As<TInY, byte>(inValY!)));
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct(As<TInY, long>(inValY!)));
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct(As<TInY, int>(inValY!)));
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct(As<TInY, short>(inValY!)));
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct(As<TInY, sbyte>(inValY!)));
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct(As<TInY, float>(inValY!)));
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct(As<TInY, double>(inValY!)));
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Subtract(inValX, vectorProviderX.Construct((double)As<TInY, decimal>(inValY!)));
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Subtract<TInY, TVisitor, TIdealOut>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct SubtractXVisitor<TVisitor, TInY, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Subtract<T, TInY, TVisitor, TIdealOut>(value, ValueY, ref Visitor);
        }
    }

    private struct SubtractYVisitor<TVisitor, TInX, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Subtract<TInX, T, TVisitor, TIdealOut>(ValueX, value, ref Visitor);
        }
    }
}
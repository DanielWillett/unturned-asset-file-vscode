using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        MinXVisitor<TVisitor, TInY, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<TIdealOut, MinXVisitor<TVisitor, TInY, TIdealOut>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInX, TVisitor, TIdealOut>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        MinYVisitor<TVisitor, TInX, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<TIdealOut, MinYVisitor<TVisitor, TInX, TIdealOut>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Min<ulong, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY <= 0 ? inValY : Math.Min(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Min<uint, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY <= 0 ? inValY : Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY <= 0 ? inValY : Math.Min(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Min<ushort, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (ushort)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (ushort)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY <= 0 ? inValY : Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY <= 0 ? inValY : Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY <= 0 ? inValY : Math.Min(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Min<byte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (ushort)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Min(inValX, (byte)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY <= 0 ? inValY : Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY <= 0 ? inValY : Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY <= 0 ? inValY : Math.Min(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Min<long, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((ulong)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((ulong)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((ulong)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((ulong)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Min<int, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((ulong)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((uint)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((uint)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((uint)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Min<short, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((ulong)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((uint)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((ushort)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((ushort)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Min<sbyte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((ulong)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((uint)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((ushort)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((byte)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Min<float, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? inValX : Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? inValX : Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Min<double, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Min(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? inValX : Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? inValX : Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? inValX : Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInY, TVisitor, TIdealOut>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Min(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Min(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Min(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Min(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Min(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Min(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Min(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Min(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Min(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Min(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Min(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Min(vectorProvider.Construct((double)inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Min<decimal, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? inValX : Math.Min(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? inValX : Math.Min(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? inValX : Math.Min(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? inValX : Math.Min(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Min(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Min{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Min<TInX, TInY, TVisitor>(TInX inValX, TInY inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInX) == typeof(string))
        {
            return Min<TInX, TInY, TVisitor, TInY>(inValX, inValY, ref visitor);
        }

        return Min<TInX, TInY, TVisitor, TInX>(inValX, inValY, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Min<TInX, TInY, TVisitor, TIdealOut>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, decimal>(inValX!), inValY, ref visitor);

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
                TInX abs = vectorProviderX.Min(inValX, As<TInY, TInX>(inValY));
                visitor.Accept(abs);
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                if (typeof(TIdealOut) == typeof(TInY))
                {
                    TInY converted = ConvertVector<TInY, TInX>(vectorProviderY, vectorProviderX, inValX);
                    TInY abs = vectorProviderY.Min(converted, inValY)!;
                    visitor.Accept(abs);
                    return true;
                }
                if (typeof(TIdealOut) != typeof(TInX) && VectorTypes.TryGetProvider<TIdealOut>() is { } idealVectorProvider)
                {
                    TIdealOut convertedX = ConvertVector<TIdealOut, TInX>(idealVectorProvider, vectorProviderX, inValX);
                    TIdealOut convertedY = ConvertVector<TIdealOut, TInY>(idealVectorProvider, vectorProviderY, inValY);
                    TIdealOut abs = idealVectorProvider.Min(convertedX, convertedY)!;
                    visitor.Accept(abs);
                    return true;
                }
                else
                {
                    TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                    TInX abs = vectorProviderX.Min(inValX, converted)!;
                    visitor.Accept(abs);
                    return true;
                }
            }
            else
            {
                TInX abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct(As<TInY, ulong>(inValY!)));
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct(As<TInY, uint>(inValY!)));
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct(As<TInY, ushort>(inValY!)));
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct(As<TInY, byte>(inValY!)));
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct(As<TInY, long>(inValY!)));
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct(As<TInY, int>(inValY!)));
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct(As<TInY, short>(inValY!)));
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct(As<TInY, sbyte>(inValY!)));
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct(As<TInY, float>(inValY!)));
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct(As<TInY, double>(inValY!)));
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Min(inValX, vectorProviderX.Construct((double)As<TInY, decimal>(inValY!)));
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Min<TInY, TVisitor, TIdealOut>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct MinXVisitor<TVisitor, TInY, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Min<T, TInY, TVisitor, TIdealOut>(value, ValueY, ref Visitor);
        }
    }

    private struct MinYVisitor<TVisitor, TInX, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Min<TInX, T, TVisitor, TIdealOut>(ValueX, value, ref Visitor);
        }
    }
}
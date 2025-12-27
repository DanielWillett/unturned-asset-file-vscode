using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        Atan2RadXVisitor<TVisitor, TInY, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<TIdealOut, Atan2RadXVisitor<TVisitor, TInY, TIdealOut>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInX, TVisitor, TIdealOut>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        Atan2RadYVisitor<TVisitor, TInX, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<TIdealOut, Atan2RadYVisitor<TVisitor, TInX, TIdealOut>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<ulong, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<uint, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<ushort, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<byte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<long, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<int, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<short, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<sbyte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<float, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<double, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInY, TVisitor, TIdealOut>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Rad(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Rad(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Rad(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Rad(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Rad(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Rad(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Rad(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Rad(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Rad(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Rad(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Rad(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2((double)inValX, inValY, deg: false);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Rad<decimal, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Atan2Rad{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Rad<TInX, TInY, TVisitor>(TInX inValX, TInY inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        return Atan2Rad<TInX, TInY, TVisitor, TInX>(inValX, inValY, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Atan2Rad<TInX, TInY, TVisitor, TIdealOut>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, decimal>(inValX!), inValY, ref visitor);

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
                TInX abs = vectorProviderX.Atan2(inValX, As<TInY, TInX>(inValY), deg: false);
                visitor.Accept(abs);
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                if (typeof(TIdealOut) == typeof(TInY))
                {
                    TInY converted = ConvertVector<TInY, TInX>(vectorProviderY, vectorProviderX, inValX);
                    TInY abs = vectorProviderY.Atan2(converted, inValY, deg: false)!;
                    visitor.Accept(abs);
                    return true;
                }
                if (typeof(TIdealOut) != typeof(TInX) && VectorTypes.TryGetProvider<TIdealOut>() is { } idealVectorProvider)
                {
                    TIdealOut convertedX = ConvertVector<TIdealOut, TInX>(idealVectorProvider, vectorProviderX, inValX);
                    TIdealOut convertedY = ConvertVector<TIdealOut, TInY>(idealVectorProvider, vectorProviderY, inValY);
                    TIdealOut abs = idealVectorProvider.Atan2(convertedX, convertedY, deg: false)!;
                    visitor.Accept(abs);
                    return true;
                }
                else
                {
                    TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                    TInX abs = vectorProviderX.Atan2(inValX, converted, deg: false)!;
                    visitor.Accept(abs);
                    return true;
                }
            }
            else
            {
                TInX abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, ulong>(inValY!), deg: false);
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, uint>(inValY!), deg: false);
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, ushort>(inValY!), deg: false);
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, byte>(inValY!), deg: false);
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, long>(inValY!), deg: false);
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, int>(inValY!), deg: false);
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, short>(inValY!), deg: false);
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, sbyte>(inValY!), deg: false);
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, float>(inValY!), deg: false);
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, double>(inValY!), deg: false);
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Atan2(inValX, (double)As<TInY, decimal>(inValY!), deg: false);
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Atan2Rad<TInY, TVisitor, TIdealOut>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct Atan2RadXVisitor<TVisitor, TInY, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Atan2Rad<T, TInY, TVisitor, TIdealOut>(value, ValueY, ref Visitor);
        }
    }

    private struct Atan2RadYVisitor<TVisitor, TInX, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Atan2Rad<TInX, T, TVisitor, TIdealOut>(ValueX, value, ref Visitor);
        }
    }
}
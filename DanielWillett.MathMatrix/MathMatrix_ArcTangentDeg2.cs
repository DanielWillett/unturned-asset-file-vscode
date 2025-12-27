using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        Atan2DegXVisitor<TVisitor, TInY, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<TIdealOut, Atan2DegXVisitor<TVisitor, TInY, TIdealOut>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInX, TVisitor, TIdealOut>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        Atan2DegYVisitor<TVisitor, TInX, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<TIdealOut, Atan2DegYVisitor<TVisitor, TInX, TIdealOut>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<ulong, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<uint, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<ushort, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<byte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }
    
    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<long, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<int, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<short, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<sbyte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<float, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Atan2(inValX, inValY) * (180f / MathF.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2(inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<double, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2(inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInY, TVisitor, TIdealOut>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Atan2Deg(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Atan2Deg(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Atan2Deg(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Atan2Deg(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Atan2Deg(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Atan2Deg(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Atan2Deg(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Atan2Deg(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Atan2Deg(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Atan2Deg(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Atan2Deg(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Atan2((double)inValX, inValY, deg: true);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Atan2Deg<decimal, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Atan2((double)inValX, (double)inValY) * (180 / Math.PI));
        return true;
    }

    /// <inheritdoc cref="Atan2Deg{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Atan2Deg<TInX, TInY, TVisitor>(TInX inValX, TInY inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        return Atan2Deg<TInX, TInY, TVisitor, TInX>(inValX, inValY, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Atan2Deg<TInX, TInY, TVisitor, TIdealOut>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, decimal>(inValX!), inValY, ref visitor);

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
                TInX abs = vectorProviderX.Atan2(inValX, As<TInY, TInX>(inValY), deg: true);
                visitor.Accept(abs);
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                if (typeof(TIdealOut) == typeof(TInY))
                {
                    TInY converted = ConvertVector<TInY, TInX>(vectorProviderY, vectorProviderX, inValX);
                    TInY abs = vectorProviderY.Atan2(converted, inValY, deg: true)!;
                    visitor.Accept(abs);
                    return true;
                }
                if (typeof(TIdealOut) != typeof(TInX) && VectorTypes.TryGetProvider<TIdealOut>() is { } idealVectorProvider)
                {
                    TIdealOut convertedX = ConvertVector<TIdealOut, TInX>(idealVectorProvider, vectorProviderX, inValX);
                    TIdealOut convertedY = ConvertVector<TIdealOut, TInY>(idealVectorProvider, vectorProviderY, inValY);
                    TIdealOut abs = idealVectorProvider.Atan2(convertedX, convertedY, deg: true)!;
                    visitor.Accept(abs);
                    return true;
                }
                else
                {
                    TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                    TInX abs = vectorProviderX.Atan2(inValX, converted, deg: true)!;
                    visitor.Accept(abs);
                    return true;
                }
            }
            else
            {
                TInX abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, ulong>(inValY!), deg: true);
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, uint>(inValY!), deg: true);
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, ushort>(inValY!), deg: true);
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, byte>(inValY!), deg: true);
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, long>(inValY!), deg: true);
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, int>(inValY!), deg: true);
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, short>(inValY!), deg: true);
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, sbyte>(inValY!), deg: true);
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, float>(inValY!), deg: true);
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Atan2(inValX, As<TInY, double>(inValY!), deg: true);
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Atan2(inValX, (double)As<TInY, decimal>(inValY!), deg: true);
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Atan2Deg<TInY, TVisitor, TIdealOut>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct Atan2DegXVisitor<TVisitor, TInY, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Atan2Deg<T, TInY, TVisitor, TIdealOut>(value, ValueY, ref Visitor);
        }
    }

    private struct Atan2DegYVisitor<TVisitor, TInX, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Atan2Deg<TInX, T, TVisitor, TIdealOut>(ValueX, value, ref Visitor);
        }
    }
}
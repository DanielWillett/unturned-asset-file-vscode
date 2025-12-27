using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        PowXVisitor<TVisitor, TInY, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<TIdealOut, PowXVisitor<TVisitor, TInY, TIdealOut>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInX, TVisitor, TIdealOut>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        PowYVisitor<TVisitor, TInX, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<TIdealOut, PowYVisitor<TVisitor, TInX, TIdealOut>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Pow<ulong, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Pow<uint, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Pow<ushort, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Pow<byte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Pow<long, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Pow<int, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Pow<short, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Pow<sbyte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Pow<float, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(MathF.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Pow<double, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInY, TVisitor, TIdealOut>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Pow(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Pow(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Pow(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Pow(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Pow(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Pow(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Pow(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Pow(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Pow(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Pow(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Pow(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Power(vectorProvider.Construct((double)inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Pow<decimal, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Pow((double)inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Pow{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Pow<TInX, TInY, TVisitor>(TInX inValX, TInY inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInX) == typeof(string))
        {
            return Pow<TInX, TInY, TVisitor, TInY>(inValX, inValY, ref visitor);
        }

        return Pow<TInX, TInY, TVisitor, TInX>(inValX, inValY, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Pow<TInX, TInY, TVisitor, TIdealOut>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, decimal>(inValX!), inValY, ref visitor);

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
                TInX abs = vectorProviderX.Power(inValX, As<TInY, TInX>(inValY));
                visitor.Accept(abs);
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                if (typeof(TIdealOut) == typeof(TInY))
                {
                    TInY converted = ConvertVector<TInY, TInX>(vectorProviderY, vectorProviderX, inValX);
                    TInY abs = vectorProviderY.Power(converted, inValY)!;
                    visitor.Accept(abs);
                    return true;
                }
                if (typeof(TIdealOut) != typeof(TInX) && VectorTypes.TryGetProvider<TIdealOut>() is { } idealVectorProvider)
                {
                    TIdealOut convertedX = ConvertVector<TIdealOut, TInX>(idealVectorProvider, vectorProviderX, inValX);
                    TIdealOut convertedY = ConvertVector<TIdealOut, TInY>(idealVectorProvider, vectorProviderY, inValY);
                    TIdealOut abs = idealVectorProvider.Power(convertedX, convertedY)!;
                    visitor.Accept(abs);
                    return true;
                }
                else
                {
                    TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                    TInX abs = vectorProviderX.Power(inValX, converted)!;
                    visitor.Accept(abs);
                    return true;
                }
            }
            else
            {
                TInX abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct(As<TInY, ulong>(inValY!)));
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct(As<TInY, uint>(inValY!)));
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct(As<TInY, ushort>(inValY!)));
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct(As<TInY, byte>(inValY!)));
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct(As<TInY, long>(inValY!)));
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct(As<TInY, int>(inValY!)));
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct(As<TInY, short>(inValY!)));
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct(As<TInY, sbyte>(inValY!)));
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct(As<TInY, float>(inValY!)));
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct(As<TInY, double>(inValY!)));
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Power(inValX, vectorProviderX.Construct((double)As<TInY, decimal>(inValY!)));
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Pow<TInY, TVisitor, TIdealOut>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct PowXVisitor<TVisitor, TInY, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Pow<T, TInY, TVisitor, TIdealOut>(value, ValueY, ref Visitor);
        }
    }

    private struct PowYVisitor<TVisitor, TInX, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Pow<TInX, T, TVisitor, TIdealOut>(ValueX, value, ref Visitor);
        }
    }
}
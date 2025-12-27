using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        ModuloXVisitor<TVisitor, TInY, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<TIdealOut, ModuloXVisitor<TVisitor, TInY, TIdealOut>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInX, TVisitor, TIdealOut>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        ModuloYVisitor<TVisitor, TInX, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<TIdealOut, ModuloYVisitor<TVisitor, TInX, TIdealOut>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Modulo<ulong, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    private static bool DivideByZero<TVisitor>(ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(double.NaN);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Modulo<uint, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((double)inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Modulo<ushort, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Modulo<byte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }
    
    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % AbsSafe(inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Modulo<long, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        ulong res = AbsSafe(inValX) % inValY;
        if (inValX < 0)
            visitor.Accept(-(long)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        ulong res = AbsSafe(inValX) % inValY;
        if (inValX < 0)
            visitor.Accept(-(long)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        ulong res = AbsSafe(inValX) % inValY;
        if (inValX < 0)
            visitor.Accept(-(long)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        ulong res = AbsSafe(inValX) % inValY;
        if (inValX < 0)
            visitor.Accept(-(long)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Modulo<int, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        uint res = (uint)(AbsSafe(inValX) % inValY);
        if (inValX < 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        uint res = AbsSafe(inValX) % inValY;
        if (inValX < 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        uint res = AbsSafe(inValX) % inValY;
        if (inValX < 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        uint res = AbsSafe(inValX) % inValY;
        if (inValX < 0)
            visitor.Accept(-(int)res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept((int)(inValX % inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Modulo<short, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        ushort res = (ushort)(AbsSafe(inValX) % inValY);
        if (inValX < 0)
            visitor.Accept(-res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        ushort res = (ushort)(AbsSafe(inValX) % inValY);
        if (inValX < 0)
            visitor.Accept(-res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        ushort res = (ushort)(AbsSafe(inValX) % inValY);
        if (inValX < 0)
            visitor.Accept(-res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        ushort res = (ushort)(AbsSafe(inValX) % inValY);
        if (inValX < 0)
            visitor.Accept(-res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept((short)(inValX % inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept((short)(inValX % inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept((short)(inValX % inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept((short)(inValX % inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Modulo<sbyte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        byte res = (byte)(AbsSafe(inValX) % inValY);
        if (inValX < 0)
            visitor.Accept(-res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        byte res = (byte)(AbsSafe(inValX) % inValY);
        if (inValX < 0)
            visitor.Accept(-res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        byte res = (byte)(AbsSafe(inValX) % inValY);
        if (inValX < 0)
            visitor.Accept(-res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        byte res = (byte)(AbsSafe(inValX) % inValY);
        if (inValX < 0)
            visitor.Accept(-res);
        else
            visitor.Accept(res);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept((short)(inValX % inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept((short)(inValX % inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept((short)(inValX % inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == 0)
            return DivideByZero(ref visitor);
        visitor.Accept((short)(inValX % inValY));
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Modulo<float, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Modulo<double, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(new decimal(inValX), inValY, ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInY, TVisitor, TIdealOut>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Modulo(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Modulo(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Modulo(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Modulo(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Modulo(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Modulo(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Modulo(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Modulo(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Modulo(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Modulo(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Modulo(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Modulo(vectorProvider.Construct((double)inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Modulo<decimal, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        return Modulo(inValX, new decimal(inValY), ref visitor);
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY == decimal.Zero)
            return DivideByZero(ref visitor);
        visitor.Accept(inValX % inValY);
        return true;
    }

    /// <inheritdoc cref="Modulo{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Modulo<TInX, TInY, TVisitor>(TInX inValX, TInY inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInX) == typeof(string))
        {
            return Modulo<TInX, TInY, TVisitor, TInY>(inValX, inValY, ref visitor);
        }

        return Modulo<TInX, TInY, TVisitor, TInX>(inValX, inValY, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Modulo<TInX, TInY, TVisitor, TIdealOut>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, decimal>(inValX!), inValY, ref visitor);

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
                TInX abs = vectorProviderX.Modulo(inValX, As<TInY, TInX>(inValY));
                visitor.Accept(abs);
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                if (typeof(TIdealOut) == typeof(TInY))
                {
                    TInY converted = ConvertVector<TInY, TInX>(vectorProviderY, vectorProviderX, inValX);
                    TInY abs = vectorProviderY.Modulo(converted, inValY)!;
                    visitor.Accept(abs);
                    return true;
                }
                if (typeof(TIdealOut) != typeof(TInX) && VectorTypes.TryGetProvider<TIdealOut>() is { } idealVectorProvider)
                {
                    TIdealOut convertedX = ConvertVector<TIdealOut, TInX>(idealVectorProvider, vectorProviderX, inValX);
                    TIdealOut convertedY = ConvertVector<TIdealOut, TInY>(idealVectorProvider, vectorProviderY, inValY);
                    TIdealOut abs = idealVectorProvider.Modulo(convertedX, convertedY)!;
                    visitor.Accept(abs);
                    return true;
                }
                else
                {
                    TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                    TInX abs = vectorProviderX.Modulo(inValX, converted)!;
                    visitor.Accept(abs);
                    return true;
                }
            }
            else
            {
                TInX abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct(As<TInY, ulong>(inValY!)));
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct(As<TInY, uint>(inValY!)));
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct(As<TInY, ushort>(inValY!)));
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct(As<TInY, byte>(inValY!)));
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct(As<TInY, long>(inValY!)));
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct(As<TInY, int>(inValY!)));
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct(As<TInY, short>(inValY!)));
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct(As<TInY, sbyte>(inValY!)));
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct(As<TInY, float>(inValY!)));
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct(As<TInY, double>(inValY!)));
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Modulo(inValX, vectorProviderX.Construct((double)As<TInY, decimal>(inValY!)));
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Modulo<TInY, TVisitor, TIdealOut>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct ModuloXVisitor<TVisitor, TInY, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Modulo<T, TInY, TVisitor, TIdealOut>(value, ValueY, ref Visitor);
        }
    }

    private struct ModuloYVisitor<TVisitor, TInX, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Modulo<TInX, T, TVisitor, TIdealOut>(ValueX, value, ref Visitor);
        }
    }
}
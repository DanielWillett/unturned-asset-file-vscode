using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        MaxXVisitor<TVisitor, TInY, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<TIdealOut, MaxXVisitor<TVisitor, TInY, TIdealOut>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInX, TVisitor, TIdealOut>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        MaxYVisitor<TVisitor, TInX, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<TIdealOut, MaxYVisitor<TVisitor, TInX, TIdealOut>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Max<ulong, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0 || inValX > long.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((long)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0 || inValX > int.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((int)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0 || inValX > (ulong)short.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((short)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0 || inValX > (ulong)sbyte.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((sbyte)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Max<uint, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else if (inValY > uint.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0 || inValX > int.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((int)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0 || inValX > (uint)short.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((short)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0 || inValX > (uint)sbyte.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((sbyte)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Max<ushort, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else if (inValY > ushort.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (ushort)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else if (inValY > ushort.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (ushort)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0 || inValX > short.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((short)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0 || inValX > sbyte.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((sbyte)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Max<byte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }
    
    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else if (inValY > byte.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (byte)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else if (inValY > byte.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (byte)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else if (inValY > byte.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (byte)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0 || inValX > sbyte.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((sbyte)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY <= 0)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Max<long, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max((ulong)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else if (inValX > uint.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((uint)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else if (inValX > ushort.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((ushort)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else if (inValX > byte.MaxValue)
            visitor.Accept(inValX);
        else
            visitor.Accept(Math.Max((byte)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Max<int, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0 || inValY > int.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (int)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0 || inValY > int.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (int)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, (double)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Max<short, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0 || inValY > (ulong)short.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (short)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0 || inValY > (uint)short.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (short)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0 || inValY > (uint)short.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (short)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Max<sbyte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0 || inValY > (ulong)sbyte.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (sbyte)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0 || inValY > (uint)sbyte.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (sbyte)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0 || inValY > (uint)sbyte.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (sbyte)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0 || inValY > (uint)sbyte.MaxValue)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, (sbyte)inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Max<float, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max((double)inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Max<double, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(new decimal(inValX), new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(new decimal(inValX), inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInY, TVisitor, TIdealOut>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Max(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Max(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Max(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Max(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Max(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Max(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Max(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Max(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Max(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Max(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Max(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Max(vectorProvider.Construct((double)inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Max<decimal, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX <= 0)
            visitor.Accept(inValY);
        else
            visitor.Accept(Math.Max(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Max(inValX, inValY));
        return true;
    }

    /// <inheritdoc cref="Max{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Max<TInX, TInY, TVisitor>(TInX inValX, TInY inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInX) == typeof(string))
        {
            return Max<TInX, TInY, TVisitor, TInY>(inValX, inValY, ref visitor);
        }

        return Max<TInX, TInY, TVisitor, TInX>(inValX, inValY, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Max<TInX, TInY, TVisitor, TIdealOut>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, decimal>(inValX!), inValY, ref visitor);

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
                TInX abs = vectorProviderX.Max(inValX, As<TInY, TInX>(inValY));
                visitor.Accept(abs);
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                if (typeof(TIdealOut) == typeof(TInY))
                {
                    TInY converted = ConvertVector<TInY, TInX>(vectorProviderY, vectorProviderX, inValX);
                    TInY abs = vectorProviderY.Max(converted, inValY)!;
                    visitor.Accept(abs);
                    return true;
                }
                if (typeof(TIdealOut) != typeof(TInX) && VectorTypes.TryGetProvider<TIdealOut>() is { } idealVectorProvider)
                {
                    TIdealOut convertedX = ConvertVector<TIdealOut, TInX>(idealVectorProvider, vectorProviderX, inValX);
                    TIdealOut convertedY = ConvertVector<TIdealOut, TInY>(idealVectorProvider, vectorProviderY, inValY);
                    TIdealOut abs = idealVectorProvider.Max(convertedX, convertedY)!;
                    visitor.Accept(abs);
                    return true;
                }
                else
                {
                    TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                    TInX abs = vectorProviderX.Max(inValX, converted)!;
                    visitor.Accept(abs);
                    return true;
                }
            }
            else
            {
                TInX abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct(As<TInY, ulong>(inValY!)));
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct(As<TInY, uint>(inValY!)));
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct(As<TInY, ushort>(inValY!)));
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct(As<TInY, byte>(inValY!)));
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct(As<TInY, long>(inValY!)));
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct(As<TInY, int>(inValY!)));
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct(As<TInY, short>(inValY!)));
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct(As<TInY, sbyte>(inValY!)));
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct(As<TInY, float>(inValY!)));
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct(As<TInY, double>(inValY!)));
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Max(inValX, vectorProviderX.Construct((double)As<TInY, decimal>(inValY!)));
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Max<TInY, TVisitor, TIdealOut>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct MaxXVisitor<TVisitor, TInY, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Max<T, TInY, TVisitor, TIdealOut>(value, ValueY, ref Visitor);
        }
    }

    private struct MaxYVisitor<TVisitor, TInX, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Max<TInX, T, TVisitor, TIdealOut>(ValueX, value, ref Visitor);
        }
    }
}
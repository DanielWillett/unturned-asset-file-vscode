using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TInY : IEquatable<TInY>
    {
        if (typeof(TInY) == typeof(string))
        {
            visitor.Accept(string.Equals(inValX, As<TInY, string>(inValY), StringComparison.Ordinal));
            return true;
        }

        EqualsXVisitor<TVisitor, TInY> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<object, EqualsXVisitor<TVisitor, TInY>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInX, TVisitor>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TInX : IEquatable<TInX>
    {
        if (typeof(TInX) == typeof(string))
        {
            visitor.Accept(string.Equals(As<TInX, string>(inValX), inValY, StringComparison.Ordinal));
            return true;
        }

        EqualsYVisitor<TVisitor, TInX> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<object, EqualsYVisitor<TVisitor, TInX>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct(inValX).Equals(inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Equals<ulong, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (ulong)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (ulong)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (ulong)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (ulong)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(new decimal(inValX) == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(new decimal(inValX) == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY >= 0 && (new decimal(inValX) == inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct(inValX).Equals(inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Equals<uint, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (ulong)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (uint)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (uint)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (uint)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept((double)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY >= 0 && inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY >= 0 && new decimal(inValX) == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct(inValX).Equals(inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Equals<ushort, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (ulong)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (uint)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (ushort)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (ushort)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY >= 0 && inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY >= 0 && inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY >= 0 && new decimal(inValX) == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct(inValX).Equals(inValY));
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Equals<byte, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }
    
    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (ulong)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (uint)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (ushort)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(false);
        else
            visitor.Accept(inValX == (byte)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY >= 0 && inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY >= 0 && inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY >= 0 && new decimal(inValX) == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct(inValX).Equals(inValY));
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Equals<long, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((ulong)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((ulong)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((ulong)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((ulong)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct(inValX).Equals(inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Equals<int, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((ulong)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((uint)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((uint)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((uint)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct(inValX).Equals(inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Equals<short, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((ulong)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((uint)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((ushort)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((ushort)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct(inValX).Equals(inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Equals<sbyte, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((ulong)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((uint)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((ushort)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((byte)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct(inValX).Equals(inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Equals<float, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept(new decimal(inValX) == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept((double)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX >= 0 && inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX >= 0 && inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((double)inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct(inValX).Equals(inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Equals<double, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(false);
        else
            visitor.Accept(new decimal(inValX) == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX >= 0 && inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX >= 0 && inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX >= 0 && inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) == inValY);
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TInY, TVisitor>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Equals(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Equals(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Equals(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Equals(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Equals(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Equals(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Equals(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Equals(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Equals(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Equals(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Equals(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Construct((double)inValX).Equals(inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Equals<decimal, TVisitor>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX >= 0 && inValX == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX >= 0 && inValX == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX >= 0 && inValX == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX >= 0 && inValX == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Equals{TInX,TInY,TVisitor}"/>
    public static bool Equals<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX == inValY);
        return true;
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Equals<TInX, TInY, TVisitor>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Equals(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Equals(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Equals(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Equals(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Equals(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Equals(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Equals(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Equals(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Equals(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Equals(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Equals(As<TInX, decimal>(inValX!), inValY, ref visitor);

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
                visitor.Accept(inValX.Equals(As<TInY, TInX>(inValY)));
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                visitor.Accept(inValX.Equals(converted));
                return true;
            }
            else
            {
                bool abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = inValX.Equals(vectorProviderX.Construct(As<TInY, ulong>(inValY!)));
                else if (typeof(TInY) == typeof(uint))
                    abs = inValX.Equals(vectorProviderX.Construct(As<TInY, uint>(inValY!)));
                else if (typeof(TInY) == typeof(ushort))
                    abs = inValX.Equals(vectorProviderX.Construct(As<TInY, ushort>(inValY!)));
                else if (typeof(TInY) == typeof(byte))
                    abs = inValX.Equals(vectorProviderX.Construct(As<TInY, byte>(inValY!)));
                else if (typeof(TInY) == typeof(long))
                    abs = inValX.Equals(vectorProviderX.Construct(As<TInY, long>(inValY!)));
                else if (typeof(TInY) == typeof(int))
                    abs = inValX.Equals(vectorProviderX.Construct(As<TInY, int>(inValY!)));
                else if (typeof(TInY) == typeof(short))
                    abs = inValX.Equals(vectorProviderX.Construct(As<TInY, short>(inValY!)));
                else if (typeof(TInY) == typeof(sbyte))
                    abs = inValX.Equals(vectorProviderX.Construct(As<TInY, sbyte>(inValY!)));
                else if (typeof(TInY) == typeof(float))
                    abs = inValX.Equals(vectorProviderX.Construct(As<TInY, float>(inValY!)));
                else if (typeof(TInY) == typeof(double))
                    abs = inValX.Equals(vectorProviderX.Construct(As<TInY, double>(inValY!)));
                else if (typeof(TInY) == typeof(decimal))
                    abs = inValX.Equals(vectorProviderX.Construct((double)As<TInY, decimal>(inValY!)));
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Equals<TInY, TVisitor>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct EqualsXVisitor<TVisitor, TInY> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Equals<T, TInY, TVisitor>(value, ValueY, ref Visitor);
        }
    }

    private struct EqualsYVisitor<TVisitor, TInX> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Equals<TInX, T, TVisitor>(ValueX, value, ref Visitor);
        }
    }
}
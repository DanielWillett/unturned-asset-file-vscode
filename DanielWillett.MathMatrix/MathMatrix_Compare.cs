using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(string inValX, TInY inValY, bool caseInsensitive, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TInY : IEquatable<TInY>
    {
        if (typeof(TInY) == typeof(string))
        {
            visitor.Accept(Math.Sign(string.Compare(inValX, As<TInY, string>(inValY), caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)));
            return true;
        }

        CompareXVisitor<TVisitor, TInY> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        visitorProxy.CaseInsensitive = caseInsensitive;
        ConvertToNumber<object, CompareXVisitor<TVisitor, TInY>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInX, TVisitor>(TInX inValX, string inValY, bool caseInsensitive, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TInX : IEquatable<TInX>
    {
        if (typeof(TInX) == typeof(string))
        {
            visitor.Accept(Math.Sign(string.Compare(As<TInX, string>(inValX), inValY, caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)));
            return true;
        }

        CompareYVisitor<TVisitor, TInX> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        visitorProxy.CaseInsensitive = caseInsensitive;
        ConvertToNumber<object, CompareYVisitor<TVisitor, TInX>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(ulong inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct(inValX), inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Compare<ulong, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(inValX.CompareTo((ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(inValX.CompareTo((ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(inValX.CompareTo((ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(inValX.CompareTo((ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(new decimal(inValX).CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(new decimal(inValX).CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY < 0 ? 1 : new decimal(inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(uint inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct(inValX), inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Compare<uint, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((ulong)inValX).CompareTo(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(((ulong)inValX).CompareTo((ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(inValX.CompareTo((uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(inValX.CompareTo((uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(inValX.CompareTo((uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY < 0 ? 1 : ((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY < 0 ? 1 : new decimal(inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(ushort inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct(inValX), inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Compare<ushort, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((ulong)inValX).CompareTo(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((uint)inValX).CompareTo(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sign(inValX.CompareTo(inValY)));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sign(inValX.CompareTo(inValY)));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(((ulong)inValX).CompareTo((ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(((uint)inValX).CompareTo((uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(Math.Sign(inValX.CompareTo((ushort)inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(Math.Sign(inValX.CompareTo((ushort)inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY < 0 ? 1 : ((float)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY < 0 ? 1 : ((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY < 0 ? 1 : new decimal(inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(byte inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct(inValX), inValY));
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Compare<byte, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((ulong)inValX).CompareTo(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((uint)inValX).CompareTo(inValY));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sign(((ushort)inValX).CompareTo(inValY)));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sign(inValX.CompareTo(inValY)));
        return true;
    }
    
    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(((ulong)inValX).CompareTo((ulong)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(((uint)inValX).CompareTo((uint)inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(Math.Sign(((ushort)inValX).CompareTo((ushort)inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
            visitor.Accept(1);
        else
            visitor.Accept(Math.Sign(inValX.CompareTo((byte)inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY < 0 ? 1 : ((float)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY < 0 ? 1 : ((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValY < 0 ? 1 : new decimal(inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(long inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct(inValX), inValY));
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Compare<long, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((ulong)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((ulong)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((ulong)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((ulong)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX).CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX).CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(int inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct(inValX), inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Compare<int, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((ulong)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((uint)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((uint)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((uint)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((long)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(short inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct(inValX), inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Compare<short, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((ulong)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((uint)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(Math.Sign(((ushort)inValX).CompareTo(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(Math.Sign(((ushort)inValX).CompareTo(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((long)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((int)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sign(inValX.CompareTo(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sign(inValX.CompareTo(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((float)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(sbyte inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct(inValX), inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Compare<sbyte, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((ulong)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((uint)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(Math.Sign(((ushort)inValX).CompareTo(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(Math.Sign(((byte)inValX).CompareTo(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((long)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((int)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sign(((short)inValX).CompareTo(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(Math.Sign(inValX.CompareTo(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((float)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(float inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct(inValX), inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Compare<float, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(new decimal(inValX).CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? -1 : inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? -1 : inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX).CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(((double)inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(double inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct(inValX), inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Compare<double, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
            visitor.Accept(-1);
        else
            visitor.Accept(new decimal(inValX).CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? -1 : inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? -1 : inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? -1 : inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX).CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX).CompareTo(inValY));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TInY, TVisitor>(decimal inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Compare(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Compare(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Compare(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Compare(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Compare(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Compare(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Compare(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Compare(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Compare(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Compare(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Compare(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            visitor.Accept(vectorProvider.Compare(vectorProvider.Construct((double)inValX), inValY));
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Compare<decimal, TVisitor>(inValX, As<TInY, string>(inValY), caseInsensitive, ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? -1 : inValX.CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? -1 : inValX.CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? -1 : inValX.CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX < 0 ? -1 : inValX.CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(new decimal(inValY)));
        return true;
    }

    /// <inheritdoc cref="Compare{TInX,TInY,TVisitor}"/>
    public static bool Compare<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX.CompareTo(inValY));
        return true;
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Compare<TInX, TInY, TVisitor>(TInX? inValX, TInY? inValY, bool caseInsensitive, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Compare(As<TInX, ulong>(inValX!), inValY, caseInsensitive, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Compare(As<TInX, uint>(inValX!), inValY, caseInsensitive, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Compare(As<TInX, ushort>(inValX!), inValY, caseInsensitive, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Compare(As<TInX, byte>(inValX!), inValY, caseInsensitive, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Compare(As<TInX, long>(inValX!), inValY, caseInsensitive, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Compare(As<TInX, int>(inValX!), inValY, caseInsensitive, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Compare(As<TInX, short>(inValX!), inValY, caseInsensitive, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Compare(As<TInX, sbyte>(inValX!), inValY, caseInsensitive, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Compare(As<TInX, float>(inValX!), inValY, caseInsensitive, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Compare(As<TInX, double>(inValX!), inValY, caseInsensitive, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Compare(As<TInX, decimal>(inValX!), inValY, caseInsensitive, ref visitor);

        if (inValX == null)
        {
            if (inValY == null)
            {
                visitor.Accept(0);
            }
            else
            {
                visitor.Accept(-1);
            }
            return true;
        }
        if (inValY == null)
        {
            visitor.Accept(1);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInX>? vectorProviderX = VectorTypes.TryGetProvider<TInX>();

        if (vectorProviderX != null)
        {
            if (typeof(TInX) == typeof(TInY))
            {
                visitor.Accept(vectorProviderX.Compare(inValX, As<TInY, TInX>(inValY)));
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                visitor.Accept(vectorProviderX.Compare(inValX, converted));
                return true;
            }
            else
            {
                int abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct(As<TInY, ulong>(inValY!)));
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct(As<TInY, uint>(inValY!)));
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct(As<TInY, ushort>(inValY!)));
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct(As<TInY, byte>(inValY!)));
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct(As<TInY, long>(inValY!)));
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct(As<TInY, int>(inValY!)));
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct(As<TInY, short>(inValY!)));
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct(As<TInY, sbyte>(inValY!)));
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct(As<TInY, float>(inValY!)));
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct(As<TInY, double>(inValY!)));
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Compare(inValX, vectorProviderX.Construct((double)As<TInY, decimal>(inValY!)));
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Compare<TInY, TVisitor>(As<TInX, string>(inValX), inValY, caseInsensitive, ref visitor);
        }

        return false;
    }

    private struct CompareXVisitor<TVisitor, TInY> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool CaseInsensitive;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Compare(value, ValueY, CaseInsensitive, ref Visitor);
        }
    }

    private struct CompareYVisitor<TVisitor, TInX> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool CaseInsensitive;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Compare(ValueX, value, CaseInsensitive, ref Visitor);
        }
    }
}
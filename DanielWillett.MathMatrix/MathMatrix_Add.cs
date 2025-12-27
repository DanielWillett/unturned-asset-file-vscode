using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

#pragma warning disable CS8604
#pragma warning disable CS8600

partial class MathMatrix
{
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(string inValX, TInY inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        AddXVisitor<TVisitor, TInY, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueY = inValY;
        ConvertToNumber<TIdealOut, AddXVisitor<TVisitor, TInY, TIdealOut>>(inValX, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInX, TVisitor, TIdealOut>(TInX inValX, string inValY, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        AddYVisitor<TVisitor, TInX, TIdealOut> visitorProxy;
        visitorProxy.Result = false;
        visitorProxy.Visitor = visitor;
        visitorProxy.ValueX = inValX;
        ConvertToNumber<TIdealOut, AddYVisitor<TVisitor, TInX, TIdealOut>>(inValY, ref visitorProxy);
        visitor = visitorProxy.Visitor;
        return visitorProxy.Result;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(ulong inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Add<ulong, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            ulong inValYu = (ulong)-inValY;
            if (inValYu > inValX)
                visitor.Accept(-(long)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            ulong inValYu = (ulong)-(long)inValY;
            if (inValYu > inValX)
                visitor.Accept(-(long)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            ulong inValYu = (ulong)-inValY;
            if (inValYu > inValX)
                visitor.Accept(-(long)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            ulong inValYu = (ulong)-inValY;
            if (inValYu > inValX)
                visitor.Accept(-(long)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ulong inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(uint inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Add<uint, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept((ulong)inValX + inValY); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept((ulong)inValX + inValY); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept((ulong)inValX + inValY); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            ulong inValYu = (ulong)-inValY;
            if (inValYu > inValX)
                visitor.Accept(-(long)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            ulong inValYu = (ulong)-(long)inValY;
            if (inValYu > inValX)
                visitor.Accept((int)-(long)(inValYu - inValX));
            else
                visitor.Accept((uint)(inValX - inValYu));
        }
        else
        {
            uint inValYu = (uint)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept((ulong)inValX + inValYu); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            uint inValYu = (uint)-inValY;
            if (inValYu > inValX)
                visitor.Accept(-(int)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            uint inValYu = (uint)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept((ulong)inValX + inValYu); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            uint inValYu = (uint)-inValY;
            if (inValYu > inValX)
                visitor.Accept(-(int)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            uint inValYu = (uint)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept((ulong)inValX + inValYu); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((double)inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(uint inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(ushort inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Add<ushort, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept((ulong)inValX + inValY); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((uint)inValX + inValY);
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((uint)inValX + inValY);
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            ulong inValYu = (ulong)-inValY;
            if (inValYu > inValX)
                visitor.Accept(-(long)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            uint inValYu = (uint)-(long)inValY;
            if (inValYu > inValX)
                visitor.Accept((int)-(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            uint inValYu = (uint)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept((ulong)inValX + inValYu); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            uint inValYu = (uint)-inValY;
            if (inValYu > inValX)
                visitor.Accept(-(int)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            visitor.Accept(inValX + (uint)inValY);
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            uint inValYu = (uint)-inValY;
            if (inValYu > inValX)
                visitor.Accept(-(int)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            visitor.Accept(inValX + (uint)inValY);
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(ushort inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(byte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Add<byte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        try { visitor.Accept(checked(inValX + inValY)); }
        catch (OverflowException) { visitor.Accept((ulong)inValX + inValY); }
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((uint)inValX + inValY);
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept((uint)inValX + inValY);
        return true;
    }
    
    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            ulong inValYu = (ulong)-inValY;
            if (inValYu > inValX)
                visitor.Accept(-(long)(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            uint inValYu = (uint)-(long)inValY;
            if (inValYu > inValX)
                visitor.Accept((int)-(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            uint inValYu = (uint)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept((ulong)inValX + inValYu); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            uint inValYu = (uint)-inValY;
            if (inValYu > inValX)
                visitor.Accept((int)-(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            uint inValYu = (uint)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept((ulong)inValX + inValYu); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            uint inValYu = (uint)-inValY;
            if (inValYu > inValX)
                visitor.Accept((int)-(inValYu - inValX));
            else
                visitor.Accept(inValX - inValYu);
        }
        else
        {
            uint inValYu = (uint)inValY;
            try { visitor.Accept(checked(inValX + inValYu)); }
            catch (OverflowException) { visitor.Accept((ulong)inValX + inValYu); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(byte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(long inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }
#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif
        if (typeof(TInY) == typeof(string))
        {
            return Add<long, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            ulong inValXNeg = (ulong)-inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(long)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            ulong inValXu = (ulong)inValX;
            try { visitor.Accept(checked(inValXu + inValY)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValXu) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            ulong inValXNeg = (ulong)-inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(long)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            visitor.Accept((ulong)inValX + inValY);
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            ulong inValXNeg = (ulong)-inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(long)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            visitor.Accept((ulong)inValX + inValY);
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            ulong inValXNeg = (ulong)-inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(long)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            visitor.Accept((ulong)inValX + inValY);
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                ulong inValYNeg = (ulong)-inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(long)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            if (inValX < 0)
            {
                ulong inValXNeg = (ulong)-inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(long)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                visitor.Accept(inValXu + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                ulong inValYNeg = (ulong)-(long)inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(long)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            if (inValX < 0)
            {
                ulong inValXNeg = (ulong)-inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(long)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                visitor.Accept(inValXu + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                ulong inValYNeg = (ulong)-inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(long)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            if (inValX < 0)
            {
                ulong inValXNeg = (ulong)-inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(long)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                visitor.Accept(inValXu + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                ulong inValYNeg = (ulong)-inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(long)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            if (inValX < 0)
            {
                ulong inValXNeg = (ulong)-inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(long)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                visitor.Accept(inValXu + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(long inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(int inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Add<int, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            uint inValXNeg = (uint)-inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(int)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            uint inValXu = (uint)inValX;
            try { visitor.Accept(checked(inValXu + inValY)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValXu) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            uint inValXNeg = (uint)-(long)inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(int)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            uint inValXu = (uint)inValX;
            try { visitor.Accept(checked(inValXu + inValY)); }
            catch (OverflowException) { visitor.Accept((ulong)inValXu + inValY); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            uint inValXNeg = (uint)-(long)inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(int)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            uint inValXu = (uint)inValX;
            try { visitor.Accept(checked(inValXu + inValY)); }
            catch (OverflowException) { visitor.Accept((ulong)inValXu + inValY); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            uint inValXNeg = (uint)-(long)inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(int)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            uint inValXu = (uint)inValX;
            try { visitor.Accept(checked(inValXu + inValY)); }
            catch (OverflowException) { visitor.Accept((ulong)inValXu + inValY); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                ulong inValYNeg = (ulong)-inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(long)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            if (inValX < 0)
            {
                uint inValXNeg = (uint)-(long)inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(long)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                uint inValXu = (uint)inValX;
                visitor.Accept(inValXu + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept((long)inValX + inValY); }
            }
            else
            {
                uint inValXu = (uint)inValX;
                uint inValYNeg = (uint)-(long)inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(int)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            uint inValYu = (uint)inValY;
            if (inValX < 0)
            {
                uint inValXNeg = (uint)-(long)inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(int)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                uint inValXu = (uint)inValX;
                visitor.Accept(inValXu + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept((long)inValX + inValY); }
            }
            else
            {
                uint inValXu = (uint)inValX;
                uint inValYNeg = (uint)-(long)inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(int)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            uint inValYu = (uint)inValY;
            if (inValX < 0)
            {
                uint inValXNeg = (uint)-(long)inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(int)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                uint inValXu = (uint)inValX;
                visitor.Accept(inValXu + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept((long)inValX + inValY); }
            }
            else
            {
                uint inValXu = (uint)inValX;
                uint inValYNeg = (uint)-(long)inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(int)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            uint inValYu = (uint)inValY;
            if (inValX < 0)
            {
                uint inValXNeg = (uint)-(long)inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(int)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                uint inValXu = (uint)inValX;
                visitor.Accept(inValXu + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(int inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(short inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Add<short, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            uint inValXNeg = (uint)-inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(int)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            uint inValXu = (uint)inValX;
            try { visitor.Accept(checked(inValXu + inValY)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValXu) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            uint inValXNeg = (uint)-inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(int)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            uint inValXu = (uint)inValX;
            try { visitor.Accept(checked(inValXu + inValY)); }
            catch (OverflowException) { visitor.Accept((ulong)inValXu + inValY); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                ulong inValYNeg = (ulong)-inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(long)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            if (inValX < 0)
            {
                uint inValXNeg = (uint)-inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(long)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                visitor.Accept((ulong)inValX + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept((long)inValX + inValY); }
            }
            else
            {
                uint inValXu = (uint)inValX;
                uint inValYNeg = (uint)-(long)inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(int)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            uint inValYu = (uint)inValY;
            if (inValX < 0)
            {
                uint inValXNeg = (uint)-inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(int)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                uint inValXu = (uint)inValX;
                visitor.Accept(inValXu + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(short inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(sbyte inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Add<sbyte, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            uint inValXNeg = (uint)-inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(int)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            uint inValXu = (uint)inValX;
            try { visitor.Accept(checked(inValXu + inValY)); }
            catch (OverflowException) { visitor.Accept(new decimal(inValXu) + new decimal(inValY)); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValX < 0)
        {
            uint inValXNeg = (uint)-inValX;
            if (inValXNeg > inValY)
                visitor.Accept(-(int)(inValXNeg - inValY));
            else
                visitor.Accept(inValY - inValXNeg);
        }
        else
        {
            uint inValXu = (uint)inValX;
            try { visitor.Accept(checked(inValXu + inValY)); }
            catch (OverflowException) { visitor.Accept((ulong)inValXu + inValY); }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept(new decimal(inValX) + new decimal(inValY)); }
            }
            else
            {
                ulong inValXu = (ulong)inValX;
                ulong inValYNeg = (ulong)-inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(long)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            ulong inValYu = (ulong)inValY;
            if (inValX < 0)
            {
                uint inValXNeg = (uint)-inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(long)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                visitor.Accept((ulong)inValX + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        if (inValY < 0)
        {
            if (inValX < 0)
            {
                try { visitor.Accept(checked(inValX + inValY)); }
                catch (OverflowException) { visitor.Accept((long)inValX + inValY); }
            }
            else
            {
                uint inValXu = (uint)inValX;
                uint inValYNeg = (uint)-(long)inValY;
                if (inValYNeg > inValXu)
                    visitor.Accept(-(int)(inValYNeg - inValXu));
                else
                    visitor.Accept(inValXu - inValYNeg);
            }
        }
        else
        {
            uint inValYu = (uint)inValY;
            if (inValX < 0)
            {
                uint inValXNeg = (uint)-inValX;
                if (inValXNeg > inValYu)
                    visitor.Accept(-(int)(inValXNeg - inValYu));
                else
                    visitor.Accept(inValYu - inValXNeg);
            }
            else
            {
                uint inValXu = (uint)inValX;
                visitor.Accept(inValXu + inValYu);
            }
        }

        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(sbyte inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(float inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Add<float, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + (double)inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(float inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(double inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct(inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Add<double, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(double inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(new decimal(inValX) + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInY, TVisitor, TIdealOut>(decimal inValX, TInY? inValY, ref TVisitor visitor)
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInY) == typeof(ulong))
            return Add(inValX, As<TInY, ulong>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(uint))
            return Add(inValX, As<TInY, uint>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(ushort))
            return Add(inValX, As<TInY, ushort>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(byte))
            return Add(inValX, As<TInY, byte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(long))
            return Add(inValX, As<TInY, long>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(int))
            return Add(inValX, As<TInY, int>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(short))
            return Add(inValX, As<TInY, short>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(sbyte))
            return Add(inValX, As<TInY, sbyte>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(float))
            return Add(inValX, As<TInY, float>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(double))
            return Add(inValX, As<TInY, double>(inValY!), ref visitor);
        if (typeof(TInY) == typeof(decimal))
            return Add(inValX, As<TInY, decimal>(inValY!), ref visitor);

        if (inValY == null)
        {
            visitor.Accept<string>(null);
            return true;
        }

#if SUBSEQUENT_COMPILE
        IVectorTypeProvider<TInY>? vectorProvider = VectorTypes.TryGetProvider<TInY>();

        if (vectorProvider != null)
        {
            TInY abs = vectorProvider.Add(vectorProvider.Construct((double)inValX), inValY);
            visitor.Accept(abs);
            return true;
        }
#endif

        if (typeof(TInY) == typeof(string))
        {
            return Add<decimal, TVisitor, TIdealOut>(inValX, As<TInY, string>(inValY), ref visitor);
        }

        return false;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, ulong inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, uint inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, ushort inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, byte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, long inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, int inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, short inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, sbyte inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, float inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, double inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + new decimal(inValY));
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TVisitor>(decimal inValX, decimal inValY, ref TVisitor visitor) where TVisitor : IGenericVisitor
    {
        visitor.Accept(inValX + inValY);
        return true;
    }

    /// <inheritdoc cref="Add{TInX,TInY,TVisitor,TIdealOut}"/>
    public static bool Add<TInX, TInY, TVisitor>(TInX inValX, TInY inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TVisitor : IGenericVisitor
    {
        if (typeof(TInX) == typeof(string))
        {
            return Add<TInX, TInY, TVisitor, TInY>(inValX, inValY, ref visitor);
        }

        return Add<TInX, TInY, TVisitor, TInX>(inValX, inValY, ref visitor);
    }

    /// <summary>
    /// Performs an inverse tangent operation given an X and Y value returning a value in radians.
    /// </summary>
    /// <typeparam name="TInX">Input value for the X parameter.</typeparam>
    /// <typeparam name="TInY">Input value for the Y parameter.</typeparam>
    /// <typeparam name="TVisitor">Visitor type which will accept the result.</typeparam>
    /// <typeparam name="TIdealOut">The ideal type to return. This is usually used when the input type is a <see cref="string"/>.</typeparam>
    public static bool Add<TInX, TInY, TVisitor, TIdealOut>(TInX? inValX, TInY? inValY, ref TVisitor visitor)
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
        where TIdealOut : IEquatable<TIdealOut>
        where TVisitor : IGenericVisitor

    {
        if (typeof(TInX) == typeof(ulong))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, ulong>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(uint))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, uint>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(ushort))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, ushort>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(byte))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, byte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(long))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, long>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(int))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, int>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(short))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, short>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(sbyte))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, sbyte>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(float))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, float>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(double))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, double>(inValX!), inValY, ref visitor);
        if (typeof(TInX) == typeof(decimal))
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, decimal>(inValX!), inValY, ref visitor);

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
                TInX abs = vectorProviderX.Add(inValX, As<TInY, TInX>(inValY));
                visitor.Accept(abs);
                return true;
            }
            if (VectorTypes.TryGetProvider<TInY>() is { } vectorProviderY)
            {
                if (typeof(TIdealOut) == typeof(TInY))
                {
                    TInY converted = ConvertVector<TInY, TInX>(vectorProviderY, vectorProviderX, inValX);
                    TInY abs = vectorProviderY.Add(converted, inValY)!;
                    visitor.Accept(abs);
                    return true;
                }
                if (typeof(TIdealOut) != typeof(TInX) && VectorTypes.TryGetProvider<TIdealOut>() is { } idealVectorProvider)
                {
                    TIdealOut convertedX = ConvertVector<TIdealOut, TInX>(idealVectorProvider, vectorProviderX, inValX);
                    TIdealOut convertedY = ConvertVector<TIdealOut, TInY>(idealVectorProvider, vectorProviderY, inValY);
                    TIdealOut abs = idealVectorProvider.Add(convertedX, convertedY)!;
                    visitor.Accept(abs);
                    return true;
                }
                else
                {
                    TInX converted = ConvertVector<TInX, TInY>(vectorProviderX, vectorProviderY, inValY);
                    TInX abs = vectorProviderX.Add(inValX, converted)!;
                    visitor.Accept(abs);
                    return true;
                }
            }
            else
            {
                TInX abs;
                if (typeof(TInY) == typeof(ulong))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct(As<TInY, ulong>(inValY!)));
                else if (typeof(TInY) == typeof(uint))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct(As<TInY, uint>(inValY!)));
                else if (typeof(TInY) == typeof(ushort))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct(As<TInY, ushort>(inValY!)));
                else if (typeof(TInY) == typeof(byte))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct(As<TInY, byte>(inValY!)));
                else if (typeof(TInY) == typeof(long))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct(As<TInY, long>(inValY!)));
                else if (typeof(TInY) == typeof(int))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct(As<TInY, int>(inValY!)));
                else if (typeof(TInY) == typeof(short))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct(As<TInY, short>(inValY!)));
                else if (typeof(TInY) == typeof(sbyte))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct(As<TInY, sbyte>(inValY!)));
                else if (typeof(TInY) == typeof(float))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct(As<TInY, float>(inValY!)));
                else if (typeof(TInY) == typeof(double))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct(As<TInY, double>(inValY!)));
                else if (typeof(TInY) == typeof(decimal))
                    abs = vectorProviderX.Add(inValX, vectorProviderX.Construct((double)As<TInY, decimal>(inValY!)));
                else goto next;
                visitor.Accept(abs);
                return true;
            }
        }
        next:
#endif

        if (typeof(TInX) == typeof(string))
        {
            return Add<TInY, TVisitor, TIdealOut>(As<TInX, string>(inValX), inValY, ref visitor);
        }

        return false;
    }

    private struct AddXVisitor<TVisitor, TInY, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInY : IEquatable<TInY>
    {
        public TVisitor Visitor;
        public TInY ValueY;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Add<T, TInY, TVisitor, TIdealOut>(value, ValueY, ref Visitor);
        }
    }

    private struct AddYVisitor<TVisitor, TInX, TIdealOut> : IGenericVisitor
        where TVisitor : IGenericVisitor
        where TIdealOut : IEquatable<TIdealOut>
        where TInX : IEquatable<TInX>
    {
        public TVisitor Visitor;
        public TInX ValueX;
        public bool Result;

        public void Accept<T>(T? value) where T : IEquatable<T>
        {
            Result = Add<TInX, T, TVisitor, TIdealOut>(ValueX, value, ref Visitor);
        }
    }
}
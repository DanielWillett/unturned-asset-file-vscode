using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public static class CommonTypes
{
    /// <summary>
    /// Gets the default integer type from <typeparamref name="TCountType"/>, throwing an exception if the type isn't an integer type. Doesn't support native integers.
    /// </summary>
    /// <typeparam name="TCountType">An integer type.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="TCountType"/> is not an integer type.</exception>
    public static IType<TCountType> GetIntegerType<TCountType>() where TCountType : IEquatable<TCountType>
    {
        if (typeof(TCountType) == typeof(int))
            return (IType<TCountType>)(object)Int32Type.Instance;
        if (typeof(TCountType) == typeof(byte))
            return (IType<TCountType>)(object)UInt8Type.Instance;
        if (typeof(TCountType) == typeof(uint))
            return (IType<TCountType>)(object)UInt32Type.Instance;
        if (typeof(TCountType) == typeof(short))
            return (IType<TCountType>)(object)Int16Type.Instance;
        if (typeof(TCountType) == typeof(ushort))
            return (IType<TCountType>)(object)UInt16Type.Instance;
        if (typeof(TCountType) == typeof(sbyte))
            return (IType<TCountType>)(object)Int8Type.Instance;
        if (typeof(TCountType) == typeof(long))
            return (IType<TCountType>)(object)Int64Type.Instance;
        if (typeof(TCountType) == typeof(ulong))
            return (IType<TCountType>)(object)UInt64Type.Instance;
        
        throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_InvalidCountType, typeof(TCountType).Name));
    }
}

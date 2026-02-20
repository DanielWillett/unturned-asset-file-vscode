using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Data-ref referencing the current element in a list's index
/// in <see cref="ListTypeArgs{TCountType,TElementType}.LegacyDefaultElementTypeValue"/>
/// and <see cref="ListTypeArgs{TCountType,TElementType}.LegacyIncludedDefaultElementTypeValue"/>.
/// </summary>
public sealed class IndexDataRef<TCountType> : RootDataRef<IndexDataRef<TCountType>>, IValue<TCountType>
    where TCountType : IEquatable<TCountType>
{
    /// <inheritdoc />
    public IType<TCountType> Type { get; }

    /// <inheritdoc />
    public override string PropertyName => "Index";

    protected override bool IsPropertyNameKeyword => true;

    public IndexDataRef(IType<TCountType> type)
    {
        Type = type;
    }


    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        if (!TryEvaluateValue(out Optional<TCountType> value, in ctx))
        {
            return false;
        }

        visitor.Accept(value);
        return true;
    }

    /// <inheritdoc />
    protected override bool Equals(IndexDataRef<TCountType> other)
    {
        return Type.Equals(other.Type);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(1989220716, Type);
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TCountType> value)
    {
        // Allowing concrete parsing would give the impression that context doesn't matter.

        // It does in fact matter, it's just being accessed through a static ThreadLocal<long>
        // so it doesn't need a reference to the context.

        value = Optional<TCountType>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TCountType> value, in FileEvaluationContext ctx)
    {
        long index = ListType.Index.Value;
        if (index < 0)
        {
            value = Optional<TCountType>.Null;
            return false;
        }

        if (typeof(TCountType) == typeof(int))
        {
            value = new Optional<TCountType>(MathMatrix.As<int, TCountType>(unchecked((int)index)));
            return index <= int.MaxValue;
        }
        if (typeof(TCountType) == typeof(byte))
        {
            value = new Optional<TCountType>(MathMatrix.As<byte, TCountType>(unchecked((byte)index)));
            return index <= byte.MaxValue;
        }
        if (typeof(TCountType) == typeof(uint))
        {
            value = new Optional<TCountType>(MathMatrix.As<uint, TCountType>(unchecked((uint)index)));
            return index <= uint.MaxValue;
        }
        if (typeof(TCountType) == typeof(ushort))
        {
            value = new Optional<TCountType>(MathMatrix.As<ushort, TCountType>(unchecked((ushort)index)));
            return index <= ushort.MaxValue;
        }
        if (typeof(TCountType) == typeof(sbyte))
        {
            value = new Optional<TCountType>(MathMatrix.As<sbyte, TCountType>(unchecked((sbyte)index)));
            return index <= sbyte.MaxValue;
        }
        if (typeof(TCountType) == typeof(short))
        {
            value = new Optional<TCountType>(MathMatrix.As<short, TCountType>(unchecked((short)index)));
            return index <= short.MaxValue;
        }
        if (typeof(TCountType) == typeof(long))
        {
            value = new Optional<TCountType>(MathMatrix.As<long, TCountType>(index));
            return true;
        }
        if (typeof(TCountType) == typeof(ulong))
        {
            value = new Optional<TCountType>(MathMatrix.As<ulong, TCountType>(unchecked((ulong)index)));
            return true;
        }
        if (typeof(TCountType) == typeof(nint))
        {
#pragma warning disable IDE0049
            // ReSharper disable once RedundantOverflowCheckingContext
            value = new Optional<TCountType>(MathMatrix.As<nint, TCountType>(unchecked((nint)index)));
            return IntPtr.Size == 8 || index <= int.MaxValue;
#pragma warning restore IDE0049
        }
        if (typeof(TCountType) == typeof(nuint))
        {
#pragma warning disable IDE0049
            // ReSharper disable once RedundantOverflowCheckingContext
            value = new Optional<TCountType>(MathMatrix.As<nuint, TCountType>(unchecked((nuint)index)));
            return IntPtr.Size == 8 || index <= uint.MaxValue;
#pragma warning restore IDE0049
        }
        if (typeof(TCountType) == typeof(bool))
        {
            value = new Optional<TCountType>(MathMatrix.As<bool, TCountType>(index > 0));
            return true;
        }
        if (typeof(TCountType) == typeof(char))
        {
            value = new Optional<TCountType>(MathMatrix.As<char, TCountType>((char)(index % 10 + '0')));
            return index < 10;
        }
        if (typeof(TCountType) == typeof(GuidOrId))
        {
            value = new Optional<TCountType>(MathMatrix.As<GuidOrId, TCountType>(new GuidOrId(unchecked((ushort)index))));
            return index <= ushort.MaxValue;
        }

        value = Optional<TCountType>.Null;
        return false;
    }
}
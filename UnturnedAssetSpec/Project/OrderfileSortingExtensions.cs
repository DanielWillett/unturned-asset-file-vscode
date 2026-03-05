using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Collections.Generic;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// Extensions for sorting using orderfiles.
/// </summary>
public static class OrderFileSortingExtensions
{
    extension(IPropertyOrderFile orderfile)
    {
        /// <summary>
        /// Creates a comparer that orders properties based on the given orderfile.
        /// </summary>
        /// <remarks>Does not support <see langword="null"/> values.</remarks>
        public IComparer<DatProperty> CreateComparer(QualifiedType type, SpecPropertyContext context)
        {
            return new OrderFileComparer(type, context, orderfile);
        }

        /// <summary>
        /// Creates a comparer that orders properties based on the given orderfile using a selector.
        /// </summary>
        /// <remarks>Does not support <see langword="null"/> values.</remarks>
        public IComparer<T> CreateComparer<T>(
            QualifiedType type,
            SpecPropertyContext context,
            Func<T, DatProperty> selector
        ) where T : notnull
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
        {
            return new OrderFileComparer<T>(type, context, orderfile, selector);
        }
    }
}

internal class OrderFileComparer : IComparer<DatProperty>
{
    private readonly PropertyOrderFile.TypeKey _key;
    private readonly int[] _orderMap;
    private readonly int _orderAltOffset;

    public OrderFileComparer(QualifiedType typeName, SpecPropertyContext context, IPropertyOrderFile orderfile)
    {
        _key.TypeName = typeName.Type;
        _key.IsLocalization = context == SpecPropertyContext.Localization;
        (_orderMap, _orderAltOffset) = orderfile.GetRelativePositions(typeName, context);
    }

    public int Compare(DatProperty x, DatProperty y)
    {
        if (x == y) return 0;

        int yOrder;
        if (!TryGetOrder(x, out int xOrder))
        {
            return TryGetOrder(y, out yOrder) ? -1 : 0;
        }

        if (!TryGetOrder(y, out yOrder))
            return 1;

        // will never be large enough for overflow to matter here
        return xOrder - yOrder;
    }

    private bool TryGetOrder(DatProperty prop, out int order)
    {
        if (!prop.TryGetIndexInType(_key, out int index))
        {
            PropertyOrderFile.TypeKey tk2 = _key;
            tk2.IsLocalization = !tk2.IsLocalization;
            if (!prop.TryGetIndexInType(tk2, out index))
            {
                order = 0;
                return false;
            }

            index += _orderAltOffset;
        }

        if (index >= _orderMap.Length)
        {
            order = 0;
            return false;
        }

        order = _orderMap[index];
        return true;
    }
}

internal class OrderFileComparer<T> : OrderFileComparer, IComparer<T>
#if NET9_0_OR_GREATER
        where T : allows ref struct
#endif
{
    private readonly Func<T, DatProperty> _selector;

    public OrderFileComparer(QualifiedType type, SpecPropertyContext context, IPropertyOrderFile orderfile, Func<T, DatProperty> selector)
        : base(type, context, orderfile)
    {
        _selector = selector;
    }

    public int Compare(T x, T y)
    {
        return Compare(_selector(x), _selector(y));
    }
}
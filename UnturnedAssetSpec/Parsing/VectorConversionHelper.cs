using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal static class VectorConversionHelper
{
    /// <summary>
    /// Attempts to convert a <see cref="IConvertible"/> type to any of the known vector types.
    /// </summary>
    /// <typeparam name="TFrom">The type being converted from.</typeparam>
    /// <typeparam name="TTo">The vector type being converted to.</typeparam>
    /// <param name="from">The value being converted from</param>
    /// <param name="value">The converted vector, or <see langword="default"/> if the conversion was unsuccessful.</param>
    /// <returns>Whether or not the conversion was successful.</returns>
    public static bool TryConvertToVector<TFrom, TTo>(TFrom from, [MaybeNullWhen(false)] out TTo value)
        where TTo : IEquatable<TTo>
        where TFrom : IConvertible, IEquatable<TFrom>
    {
        if (!typeof(TTo).IsValueType)
        {
            value = default;
            return false;
        }

        try
        {
            if (typeof(TTo) == typeof(Vector2))
            {
                float comp = from.ToSingle(CultureInfo.InvariantCulture);
                value = SpecDynamicExpressionTreeValueHelpers.As<Vector2, TTo>(new Vector2(comp, comp));
                return true;
            }

            if (typeof(TTo) == typeof(Vector3))
            {
                float comp = from.ToSingle(CultureInfo.InvariantCulture);
                value = SpecDynamicExpressionTreeValueHelpers.As<Vector3, TTo>(new Vector3(comp, comp, comp));
                return true;
            }

            if (typeof(TTo) == typeof(Vector4))
            {
                float comp = from.ToSingle(CultureInfo.InvariantCulture);
                value = SpecDynamicExpressionTreeValueHelpers.As<Vector4, TTo>(new Vector4(comp, comp, comp, comp));
                return true;
            }

            if (typeof(TTo) == typeof(Color))
            {
                float comp = from.ToSingle(CultureInfo.InvariantCulture);
                value = SpecDynamicExpressionTreeValueHelpers.As<Color, TTo>(new Color(1f, comp, comp, comp));
                return true;
            }

            if (typeof(TTo) == typeof(Color32))
            {
                byte comp = from.ToByte(CultureInfo.InvariantCulture);
                value = SpecDynamicExpressionTreeValueHelpers.As<Color32, TTo>(new Color32(255, comp, comp, comp));
                return true;
            }
        }
        catch (InvalidCastException) { }
        catch (OverflowException) { }

        value = default;
        return false;
    }
}
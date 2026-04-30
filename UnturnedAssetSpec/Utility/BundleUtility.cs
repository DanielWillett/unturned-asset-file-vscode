using System;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class BundleUtility
{
    /// <summary>
    /// Tries to read the m_PathID field from an object.
    /// </summary>
    /// <param name="childGameObject"></param>
    /// <param name="pathId"></param>
    /// <returns></returns>
    internal static bool TryReadPathId(AssetTypeValueField? childGameObject, out long pathId)
    {
        pathId = 0;
        if (childGameObject is not { Value: null })
            return false;

        AssetTypeValueField pathIdField = childGameObject["m_PathID"];
        if (pathIdField.IsDummy || pathIdField.Value.ValueType != AssetValueType.Int64)
            return false;

        pathId = pathIdField.AsLong;
        return true;
    }

    internal static bool TryLockBundle(IBundleProxy proxy,
        IParsingServices services,
        ref TfmLock? @lock,
        ref bool hasLock,
        [NotNullWhen(true)] out DiscoveredBundle? bndl,
        [NotNullWhen(true)] out AssetsFileInstance? file,
        [NotNullWhen(true)] out AssetsManager? assetsManager)
    {
        while (true)
        {
            bndl = proxy.Bundle;
            if (bndl == null)
                break;

            @lock = bndl.GetLock(services);
            PlatformLockHelper.EnterLock(@lock, ref hasLock);
            if (bndl != proxy.Bundle)
            {
                PlatformLockHelper.ExitLock(@lock);
                hasLock = false;
                continue;
            }

            break;
        }

        assetsManager = services.Installation.AssetBundleManager;
        file = bndl?.Openedfile.AssetBundle;
        if (file == null)
        {
            return false;
        }

        return assetsManager != null;
    }

    /// <summary>
    /// Handler used by <see cref="BundleUtility.RunOperationOnBundle{TState}"/>.
    /// </summary>
    /// <typeparam name="TState">Generic state object.</typeparam>
    /// <param name="bundle">The bundle extracted from the passed in <see cref="IBundleProxy"/>.</param>
    /// <param name="services">Parsing services.</param>
    /// <param name="assetsManager">The <see cref="AssetsManager"/> for this operation.</param>
    /// <param name="state">The state passed to <see cref="BundleUtility.RunOperationOnBundle{TState}"/>.</param>
    internal delegate void RunOperationOnBundleHandler<TState>(DiscoveredBundle bundle, IParsingServices services, AssetsManager assetsManager, ref TState state)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    ;

    /// <summary>
    /// Runs a synchronized operation on the bundle of a <see cref="IBundleProxy"/>.
    /// </summary>
    /// <typeparam name="TState">Generic state object.</typeparam>
    /// <param name="state">The state to pass to the callback.</param>
    /// <param name="proxy">The bundle proxy to extract the <see cref="DiscoveredBundle"/> from.</param>
    /// <param name="services">Parsing services.</param>
    /// <param name="operation">The action to perform on the bundle.</param>
    /// <returns>Whether or not the action could be performed.</returns>
    internal static bool RunOperationOnBundle<TState>(ref TState state, IBundleProxy proxy, IParsingServices services, RunOperationOnBundleHandler<TState> operation)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    {
        bool hasLock = false;
        TfmLock? @lock = null;

        try
        {
            if (!TryLockBundle(proxy, services, ref @lock, ref hasLock, out DiscoveredBundle? bndl, out _, out AssetsManager? assetsManager))
            {
                return false;
            }

            operation(bndl, services, assetsManager, ref state);
            return true;
        }
        finally
        {
            if (hasLock)
                PlatformLockHelper.ExitLock(@lock!);
        }
    }

    /// <summary>
    /// Handler used by <see cref="BundleUtility.RunOperationOnBundle{TState}"/>.
    /// </summary>
    /// <typeparam name="TState">Generic state object.</typeparam>
    /// <param name="bundle">The bundle extracted from the passed in <see cref="IBundleProxy"/>.</param>
    /// <param name="services">Parsing services.</param>
    /// <param name="assetsManager">The <see cref="AssetsManager"/> for this operation.</param>
    /// <param name="state">The state passed to <see cref="BundleUtility.RunOperationOnBundle{TState}"/>.</param>
    internal delegate void RunOperationOnBundleHandlerNoRef<in TState>(DiscoveredBundle bundle, IParsingServices services, AssetsManager assetsManager, TState state)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    ;

    /// <summary>
    /// Runs a synchronized operation on the bundle of a <see cref="IBundleProxy"/>.
    /// </summary>
    /// <typeparam name="TState">Generic state object.</typeparam>
    /// <param name="state">The state to pass to the callback.</param>
    /// <param name="proxy">The bundle proxy to extract the <see cref="DiscoveredBundle"/> from.</param>
    /// <param name="services">Parsing services.</param>
    /// <param name="operation">The action to perform on the bundle.</param>
    /// <returns>Whether or not the action could be performed.</returns>
    internal static bool RunOperationOnBundle<TState>(TState state, IBundleProxy proxy, IParsingServices services, RunOperationOnBundleHandlerNoRef<TState> operation)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
    {
        bool hasLock = false;
        TfmLock? @lock = null;

        try
        {
            if (!TryLockBundle(proxy, services, ref @lock, ref hasLock, out DiscoveredBundle? bndl, out _, out AssetsManager? assetsManager))
            {
                return false;
            }

            operation(bndl, services, assetsManager, state);
            return true;
        }
        finally
        {
            if (hasLock)
                PlatformLockHelper.ExitLock(@lock!);
        }
    }

    internal static Vector4 ConstructVector4(AssetTypeValueField field, string propertyName)
    {
        AssetTypeValueField v4Field = field[propertyName];
        if (v4Field.IsDummy || v4Field.Value != null)
        {
            return default;
        }

        return ConstructVector4(v4Field);
    }

    internal static Vector4 ConstructVector4(AssetTypeValueField v4Field)
    {
        List<AssetTypeValueField> children = v4Field.Children;
        if (children.Count != 4)
        {
            return default;
        }

        AssetTypeValueField x = children[0], y = children[1], z = children[2], w = children[3];
        Vector4 v4;

        if (x == null || x.IsDummy || x.Value.ValueType != AssetValueType.Float)
            v4.X = 0f;
        else
            v4.X = x.AsFloat;

        if (y == null || y.IsDummy || y.Value.ValueType != AssetValueType.Float)
            v4.Y = 0f;
        else
            v4.Y = y.AsFloat;

        if (z == null || z.IsDummy || z.Value.ValueType != AssetValueType.Float)
            v4.Z = 0f;
        else
            v4.Z = z.AsFloat;

        if (w == null || w.IsDummy || w.Value.ValueType != AssetValueType.Float)
            v4.W = 0f;
        else
            v4.W = w.AsFloat;

        return v4;
    }
    

    internal static Vector3 ConstructVector3(AssetTypeValueField field, string propertyName)
    {
        AssetTypeValueField v3Field = field[propertyName];
        if (v3Field.IsDummy || v3Field.Value != null)
        {
            return default;
        }

        return ConstructVector3(v3Field);
    }

    internal static Vector3 ConstructVector3(AssetTypeValueField v3Field)
    {
        List<AssetTypeValueField> children = v3Field.Children;
        if (children.Count != 3)
        {
            return default;
        }

        AssetTypeValueField x = children[0], y = children[1], z = children[2];
        Vector3 v3;

        if (x == null || x.IsDummy || x.Value.ValueType != AssetValueType.Float)
            v3.X = 0f;
        else
            v3.X = x.AsFloat;

        if (y == null || y.IsDummy || y.Value.ValueType != AssetValueType.Float)
            v3.Y = 0f;
        else
            v3.Y = y.AsFloat;

        if (z == null || z.IsDummy || z.Value.ValueType != AssetValueType.Float)
            v3.Z = 0f;
        else
            v3.Z = z.AsFloat;

        return v3;
    }

    internal static Vector2 ConstructVector2(AssetTypeValueField field, string propertyName)
    {
        AssetTypeValueField v2Field = field[propertyName];
        if (v2Field.IsDummy || v2Field.Value != null)
        {
            return default;
        }

        return ConstructVector2(v2Field);
    }

    internal static Vector2 ConstructVector2(AssetTypeValueField v2Field)
    {
        List<AssetTypeValueField> children = v2Field.Children;
        if (children.Count != 2)
        {
            return default;
        }

        AssetTypeValueField x = children[0], y = children[1];
        Vector2 v2;

        if (x == null || x.IsDummy || x.Value.ValueType != AssetValueType.Float)
            v2.X = 0f;
        else
            v2.X = x.AsFloat;

        if (y == null || y.IsDummy || y.Value.ValueType != AssetValueType.Float)
            v2.Y = 0f;
        else
            v2.Y = y.AsFloat;

        return v2;
    }

    internal static Quaternion ConstructQuaternion(AssetTypeValueField field, string propertyName)
    {
        AssetTypeValueField quatField = field[propertyName];
        if (quatField.IsDummy || quatField.Value != null)
        {
            return default;
        }

        return ConstructQuaternion(quatField);
    }

    internal static Quaternion ConstructQuaternion(AssetTypeValueField quatField)
    {
        List<AssetTypeValueField> children = quatField.Children;
        if (children.Count != 4)
        {
            return default;
        }

        AssetTypeValueField x = children[0], y = children[1], z = children[2], w = children[3];
        Quaternion v4;

        if (x == null || x.IsDummy || x.Value.ValueType != AssetValueType.Float)
            v4.X = 0f;
        else
            v4.X = x.AsFloat;

        if (y == null || y.IsDummy || y.Value.ValueType != AssetValueType.Float)
            v4.Y = 0f;
        else
            v4.Y = y.AsFloat;

        if (z == null || z.IsDummy || z.Value.ValueType != AssetValueType.Float)
            v4.Z = 0f;
        else
            v4.Z = z.AsFloat;

        if (w == null || w.IsDummy || w.Value.ValueType != AssetValueType.Float)
            v4.W = 0f;
        else
            v4.W = w.AsFloat;

        return v4;
    }

    /// <summary>
    /// Invoke a visitor with the correctly typed value from a field.
    /// </summary>
    /// <typeparam name="TVisitor">The type of visitor to accept the value.</typeparam>
    /// <param name="field">The field to read a value from.</param>
    /// <param name="visitor">The visitor to call <see cref="IGenericVisitor.Accept{T}"/> on.</param>
    /// <returns>Whether or not the visitor could be invoked.</returns>
    internal static bool VisitFieldValue<TVisitor>(AssetTypeValueField field, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (field.IsDummy)
        {
            return false;
        }

        AssetTypeValue? value = field.Value;
        if (value == null)
        {
            if (string.Equals(field.TypeName, "Vector3f", StringComparison.Ordinal))
            {
                Vector3 v3 = ConstructVector3(field);
                visitor.Accept(v3);
                return true;
            }
            
            if (string.Equals(field.TypeName, "Vector2f", StringComparison.Ordinal))
            {
                Vector2 v2 = ConstructVector2(field);
                visitor.Accept(v2);
                return true;
            }
            
            if (string.Equals(field.TypeName, "Vector4f", StringComparison.Ordinal))
            {
                Vector4 v4 = ConstructVector4(field);
                visitor.Accept(v4);
                return true;
            }
            
            if (string.Equals(field.TypeName, "Quaternionf", StringComparison.Ordinal))
            {
                Quaternion qt = ConstructQuaternion(field);
                visitor.Accept(qt);
                return true;
            }

            // object
            return false;
        }

        switch (value.ValueType)
        {
            case AssetValueType.None:
            case AssetValueType.ManagedReferencesRegistry:
                break;

            case AssetValueType.Bool:
                visitor.Accept(value.AsBool);
                return true;

            case AssetValueType.Int8:
                visitor.Accept(value.AsSByte);
                return true;

            case AssetValueType.UInt8:
                visitor.Accept(value.AsByte);
                return true;

            case AssetValueType.Int16:
                visitor.Accept(value.AsShort);
                return true;

            case AssetValueType.UInt16:
                visitor.Accept(value.AsUShort);
                return true;

            case AssetValueType.Int32:
                visitor.Accept(value.AsInt);
                return true;

            case AssetValueType.UInt32:
                visitor.Accept(value.AsUInt);
                return true;

            case AssetValueType.Int64:
                visitor.Accept(value.AsLong);
                return true;

            case AssetValueType.UInt64:
                visitor.Accept(value.AsULong);
                return true;

            case AssetValueType.Float:
                visitor.Accept(value.AsFloat);
                return true;

            case AssetValueType.Double:
                visitor.Accept(value.AsDouble);
                return true;

            case AssetValueType.String:
                visitor.Accept(value.AsString);
                return true;

            case AssetValueType.Array:
                List<AssetTypeValueField> children = field.Children;
                if (children.Count == 0)
                {
                    visitor.Accept(new EquatableArray<int>(0));
                    return true;
                }

                AssetValueType t = children[0].Value?.ValueType ?? AssetValueType.None;
                for (int i = 1; i < children.Count; ++i)
                {
                    AssetTypeValueField child = children[i];
                    if (child.IsDummy || t != (child.Value?.ValueType ?? AssetValueType.None))
                    {
                        return false;
                    }
                }

                switch (t)
                {
                    case AssetValueType.None:
                    case AssetValueType.Array:
                    case AssetValueType.ManagedReferencesRegistry:
                        break;

                    case AssetValueType.Bool:
                        bool[] arr1 = new bool[children.Count];
                        for (int i = 0; i < arr1.Length; ++i)
                            arr1[i] = children[i].AsBool;
                        visitor.Accept(new EquatableArray<bool>(arr1));
                        return true;

                    case AssetValueType.Int8:
                        sbyte[] arr2 = new sbyte[children.Count];
                        for (int i = 0; i < arr2.Length; ++i)
                            arr2[i] = children[i].AsSByte;
                        visitor.Accept(new EquatableArray<sbyte>(arr2));
                        return true;

                    case AssetValueType.UInt8:
                        byte[] arr3 = new byte[children.Count];
                        for (int i = 0; i < arr3.Length; ++i)
                            arr3[i] = children[i].AsByte;
                        visitor.Accept(new EquatableArray<byte>(arr3));
                        return true;

                    case AssetValueType.Int16:
                        short[] arr4 = new short[children.Count];
                        for (int i = 0; i < arr4.Length; ++i)
                            arr4[i] = children[i].AsShort;
                        visitor.Accept(new EquatableArray<short>(arr4));
                        return true;

                    case AssetValueType.UInt16:
                        ushort[] arr5 = new ushort[children.Count];
                        for (int i = 0; i < arr5.Length; ++i)
                            arr5[i] = children[i].AsUShort;
                        visitor.Accept(new EquatableArray<ushort>(arr5));
                        return true;

                    case AssetValueType.Int32:
                        int[] arr6 = new int[children.Count];
                        for (int i = 0; i < arr6.Length; ++i)
                            arr6[i] = children[i].AsInt;
                        visitor.Accept(new EquatableArray<int>(arr6));
                        return true;

                    case AssetValueType.UInt32:
                        uint[] arr7 = new uint[children.Count];
                        for (int i = 0; i < arr7.Length; ++i)
                            arr7[i] = children[i].AsUInt;
                        visitor.Accept(new EquatableArray<uint>(arr7));
                        return true;

                    case AssetValueType.Int64:
                        long[] arr8 = new long[children.Count];
                        for (int i = 0; i < arr8.Length; ++i)
                            arr8[i] = children[i].AsLong;
                        visitor.Accept(new EquatableArray<long>(arr8));
                        return true;

                    case AssetValueType.UInt64:
                        ulong[] arr9 = new ulong[children.Count];
                        for (int i = 0; i < arr9.Length; ++i)
                            arr9[i] = children[i].AsULong;
                        visitor.Accept(new EquatableArray<ulong>(arr9));
                        return true;

                    case AssetValueType.Float:
                        float[] arr10 = new float[children.Count];
                        for (int i = 0; i < arr10.Length; ++i)
                            arr10[i] = children[i].AsFloat;
                        visitor.Accept(new EquatableArray<float>(arr10));
                        return true;

                    case AssetValueType.Double:
                        double[] arr11 = new double[children.Count];
                        for (int i = 0; i < arr11.Length; ++i)
                            arr11[i] = children[i].AsDouble;
                        visitor.Accept(new EquatableArray<double>(arr11));
                        return true;

                    case AssetValueType.String:
                        string[] arr12 = new string[children.Count];
                        for (int i = 0; i < arr12.Length; ++i)
                            arr12[i] = children[i].AsString;
                        visitor.Accept(new EquatableArray<string>(arr12));
                        return true;


                    case AssetValueType.ByteArray:
                        EquatableArray<byte>[] arr13 = new EquatableArray<byte>[children.Count];
                        for (int i = 0; i < arr13.Length; ++i)
                            arr13[i] = new EquatableArray<byte>(children[i].AsByteArray);
                        visitor.Accept(new EquatableArray<EquatableArray<byte>>(arr13));
                        return true;
                }

                break;

            case AssetValueType.ByteArray:
                visitor.Accept(new EquatableArray<byte>(value.AsByteArray));
                return true;
        }

        return false;
    }
}
using System.Collections.Generic;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
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
        if (childGameObject == null)
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
}
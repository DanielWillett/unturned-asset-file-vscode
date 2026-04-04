using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning disable CA2231

/// <summary>
/// Whether or not the current file is for a singleplayer instance. Usually this is used in a server config to get the correct default values.
/// <para>
/// Supported targets:
/// <list type="bullet">
///     <item><c>#This</c></item>
/// </list>
/// </para>
/// <para>
/// Syntax:<br/>
/// <c>#This.IsSingleplayer</c>
/// </para>
/// </summary>
public readonly struct IsSingleplayerProperty : IDataRefProperty, IEquatable<IsSingleplayerProperty>
{
    /// <inheritdoc />
    public string PropertyName => "IsSingleplayer";

    /// <inheritdoc />
    public bool Equals(IsSingleplayerProperty other) => true;

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is IsSingleplayerProperty;

    /// <inheritdoc />
    public override int GetHashCode() => 739307191;

    /// <inheritdoc />
#if NET7_0_OR_GREATER
    public static IDataRef CreateDataRef(
#else
    public IDataRef CreateDataRef(
#endif
        IDataRefTarget target,
        OneOrMore<int> indices,
        OneOrMore<KeyValuePair<string, object?>> properties)
    {
        return new DataRefProperty<IsSingleplayerProperty>(target, default);
    }

    /// <inheritdoc />
#if NET7_0_OR_GREATER
    public static IDataRef<TValue> CreateDataRef<TValue>(
#else
    public IDataRef<TValue> CreateDataRef<TValue>(
#endif
        IType<TValue> type,
        IDataRefTarget target,
        OneOrMore<int> indices,
        OneOrMore<KeyValuePair<string, object?>> properties
    ) where TValue : IEquatable<TValue>
    {
        return new DataRefProperty<IsSingleplayerProperty, TValue>(type, target, default);
    }

    /// <summary>
    /// Attempt to determine whether or not an opened file is a singleplayer file based on its path and additional properties.
    /// </summary>
    public static bool IsConfigFileForSingleplayer(ref FileEvaluationContext ctx)
    {
        ISourceFile sourceFile = ctx.File;

        if (sourceFile.TryGetAdditionalProperty(Comment.SingleplayerAdditionalProperty, out bool isSingleplayer))
        {
            return isSingleplayer;
        }

        string configFilePath = ctx.File.WorkspaceFile.File;

        ReadOnlySpan<char> parentPath = OSPathHelper.GetDirectoryName(configFilePath);
        ReadOnlySpan<char> grandparentPath = OSPathHelper.GetDirectoryName(parentPath);

        // singleplayer configs are in Unturned/Worlds/Singleplayer_#/Config_XyzDifficulty.txt

        if (grandparentPath.IsEmpty
            || !OSPathHelper.GetFileName(parentPath).StartsWith("Singleplayer_", OSPathHelper.PathComparison)
            || !OSPathHelper.GetFileName(grandparentPath).Equals("Worlds", OSPathHelper.PathComparison))
        {
            return false;
        }

        if (!ctx.Services.GameDirectory.TryGetInstallDirectory(out GameInstallDir dir))
            return false;

        if (!grandparentPath.StartsWith(dir.BaseFolder, OSPathHelper.PathComparison) || grandparentPath.Length != dir.BaseFolder.Length + 6)
            return false;

        char sep = grandparentPath[dir.BaseFolder.Length];

        return (sep == Path.DirectorySeparatorChar || sep == Path.AltDirectorySeparatorChar)
               && ServerDifficultyCache.IsValidConfigFileName(OSPathHelper.GetFileName(configFilePath), out _);
    }
}
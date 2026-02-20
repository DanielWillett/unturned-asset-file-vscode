using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning disable CA2231

/// <summary>
/// The contextual difficulty of the current file. Usually this is used in a server config to get the correct default values.
/// <para>
/// Supported targets:
/// <list type="bullet">
///     <item><c>#This</c></item>
/// </list>
/// </para>
/// <para>
/// Syntax:<br/>
/// <c>#This.Difficulty</c>
/// </para>
/// </summary>
public readonly struct DifficultyProperty : IDataRefProperty, IEquatable<DifficultyProperty>
{
    /// <inheritdoc />
    public string PropertyName => "Difficulty";

    /// <inheritdoc />
    public bool Equals(DifficultyProperty other) => true;

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is DifficultyProperty;

    /// <inheritdoc />
    public override int GetHashCode() => 976892135;

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
        return new DataRefProperty<DifficultyProperty>(target, default);
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
        return new DataRefProperty<DifficultyProperty, TValue>(type, target, default);
    }


    /// <summary>
    /// Attempt to get a contextual difficulty from an opened file.
    /// </summary>
    public static bool TryGetFileDifficultyContext(in FileEvaluationContext ctx, out ServerDifficulty difficulty)
    {
        ISourceFile sourceFile = ctx.File;

        if (sourceFile.TryGetAdditionalProperty(Comment.DifficultyAdditionalProperty, out string? diffStr) && !string.IsNullOrEmpty(diffStr))
        {
            switch (diffStr[0])
            {
                case 'e':
                case 'E':
                    if (diffStr.Length == 4 && diffStr.Equals("easy", StringComparison.OrdinalIgnoreCase))
                    {
                        difficulty = ServerDifficulty.Easy;
                        return true;
                    }

                    break;

                case 'n':
                case 'N':
                    if (diffStr.Length == 6 && diffStr.Equals("normal", StringComparison.OrdinalIgnoreCase))
                    {
                        difficulty = ServerDifficulty.Normal;
                        return true;
                    }

                    break;

                case 'h':
                case 'H':
                    if (diffStr.Length == 4 && diffStr.Equals("hard", StringComparison.OrdinalIgnoreCase))
                    {
                        difficulty = ServerDifficulty.Hard;
                        return true;
                    }

                    break;
            }
        }

        if (ctx.Services != null)
        {
            return ctx.Services.Workspace.TryGetFileDifficulty(sourceFile.WorkspaceFile.File, out difficulty);
        }

        difficulty = 0;
        return false;
    }

    internal static string GetDifficultyName(ServerDifficulty difficulty)
    {
        return difficulty switch
        {
            ServerDifficulty.Easy => "EASY",
            ServerDifficulty.Normal => "NORMAL",
            ServerDifficulty.Hard => "HARD",
            _ => "ANY"
        };
    }
}
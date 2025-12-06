using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Evaluates to the difficulty of the current config file (based on the file name and relative Commands.dat).
/// </summary>
public sealed class DifficultyDataRef : DataRef, IEquatable<DifficultyDataRef>
{
    public static readonly DifficultyDataRef Instance = new DifficultyDataRef();
    static DifficultyDataRef() { }
    private DifficultyDataRef() : base(ThisDataRef.Instance, KnownTypes.String) { }

    public override string PropertyName => "Difficulty";

    public override bool Equals(DataRef other) => other is DifficultyDataRef b && Equals(b);

    public bool Equals(DifficultyDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        bool isNull = !TryGetFileDifficultyContext(in ctx, out ServerDifficulty difficulty);

        if (condition.Comparand is not string str)
        {
            if (condition.Comparand == null)
                return condition.EvaluateNulls(isNull, true);
            str = condition.Comparand.ToString();
        }

        return isNull
            ? condition.EvaluateNulls(true, false)
            : condition.Evaluate(GetEnumValue(difficulty), str, ctx.Information.Information);
    }

    private static string GetEnumValue(ServerDifficulty difficulty)
    {
        return difficulty switch
        {
            ServerDifficulty.Easy => "EASY",
            ServerDifficulty.Normal => "NORMAL",
            ServerDifficulty.Hard => "HARD",
            _ => "ANY"
        };
    }

    /// <summary>
    /// Attempt to get a contextual difficulty from an opened file.
    /// </summary>
    public static bool TryGetFileDifficultyContext(in FileEvaluationContext ctx, out ServerDifficulty difficulty)
    {
        ISourceFile sourceFile = ctx.SourceFile;

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

        if (ctx.Workspace != null)
        {
            return ctx.Workspace.TryGetFileDifficulty(sourceFile.WorkspaceFile.File, out difficulty);
        }

        difficulty = 0;
        return false;
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, [MaybeNullWhen(false)] out TValue value, out bool isNull)
    {
        isNull = !TryGetFileDifficultyContext(in ctx, out ServerDifficulty difficulty);

        if (typeof(TValue) == typeof(ServerDifficulty))
        {
            value = SpecDynamicExpressionTreeValueHelpers.As<ServerDifficulty, TValue>(difficulty);
            return true;
        }

        if (typeof(TValue) == typeof(string))
        {
            value = SpecDynamicExpressionTreeValueHelpers.As<string, TValue>(GetEnumValue(difficulty));
            return true;
        }

        return SpecDynamicExpressionTreeValueHelpers.TryConvert((int)difficulty, isNull, out value, out isNull);
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        if (TryGetFileDifficultyContext(in ctx, out ServerDifficulty difficulty))
        {
            value = GetEnumValue(difficulty);
            return true;
        }

        value = null;
        return false;
    }
}

/// <summary>
/// The difficulty of a <see cref="T:SDG.Unturned.ConfigData"/> config file.
/// </summary>
public enum ServerDifficulty
{
    /// <summary>
    /// Corresponds to <see cref="F:SDG.Unturned.EGameMode.EASY"/>.
    /// </summary>
    Easy,

    /// <summary>
    /// Corresponds to <see cref="F:SDG.Unturned.EGameMode.NORMAL"/>.
    /// </summary>
    Normal,

    /// <summary>
    /// Corresponds to <see cref="F:SDG.Unturned.EGameMode.HARD"/>.
    /// </summary>
    Hard
}
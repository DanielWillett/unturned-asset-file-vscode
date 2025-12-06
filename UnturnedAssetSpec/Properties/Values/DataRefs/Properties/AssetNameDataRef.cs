using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Evaluates to the internal asset name of the current file, which is usually the name of the file or folder the asset is in.
/// </summary>
public sealed class AssetNameDataRef : DataRef, IEquatable<AssetNameDataRef>
{
    public static readonly AssetNameDataRef Instance = new AssetNameDataRef();
    static AssetNameDataRef() { }
    private AssetNameDataRef() : base(ThisDataRef.Instance, KnownTypes.String) { }

    public override string PropertyName => "AssetName";

    public override bool Equals(DataRef other) => other is AssetNameDataRef b && Equals(b);

    public bool Equals(AssetNameDataRef other) => base.Equals(other);

    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        TryEvaluateValue(in ctx, out string? assetName, out bool isNull);

        if (condition.Comparand is not string str)
        {
            if (condition.Comparand == null)
                return condition.EvaluateNulls(isNull, true);
            str = condition.Comparand.ToString();
        }

        return isNull
            ? condition.EvaluateNulls(true, false)
            : condition.Evaluate(assetName, str, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        isNull = false;

        if (typeof(TValue) != typeof(string))
        {
            value = default!;
            return false;
        }

        try
        {
            string? name = (ctx.SourceFile as IAssetSourceFile)?.AssetName;
            if (string.IsNullOrEmpty(name))
            {
                name = null;
                isNull = true;
            }
            value = Unsafe.As<string?, TValue>(ref name);
            return true;
        }
        catch
        {
            value = default!;
            isNull = true;
            return true;
        }
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        try
        {
            string? name = (ctx.SourceFile as IAssetSourceFile)?.AssetName;
            value = string.IsNullOrEmpty(name) ? null : name;
            return true;
        }
        catch
        {
            value = null;
            return true;
        }
    }
}
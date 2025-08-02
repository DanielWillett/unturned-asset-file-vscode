using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class AssetBundleVersionSpecPropertyType : BasicSpecPropertyType<AssetBundleVersionSpecPropertyType, int>, IAutoCompleteSpecPropertyType, IStringParseableSpecPropertyType
{
    public static readonly AssetBundleVersionSpecPropertyType Instance = new AssetBundleVersionSpecPropertyType();

    private static AutoCompleteResult[]? _autoComplete;

    static AssetBundleVersionSpecPropertyType() { }
    private AssetBundleVersionSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "AssetBundleVersion";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (int.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result)
            && result >= 0)
        {
            dynamicValue = SpecDynamicValue.Int32(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override string DisplayName => "Asset Bundle Version";

    /// <inheritdoc />
    public Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters parameters,
        in FileEvaluationContext context)
    {
        if (_autoComplete != null)
            return Task.FromResult(_autoComplete);

        AssetInformation.AssetBundleVersionInfo?[]? info = parameters.Database.Information.AssetBundleVersions;
        if (info is not { Length: > 0 })
        {
            return Task.FromResult(AutoCompleteResult.None);
        }

        int ttlCt = 0;
        for (int i = 0; i < info.Length; ++i)
        {
            if (info[i] != null)
                ++ttlCt;
        }

        AutoCompleteResult[] arr = new AutoCompleteResult[ttlCt];
        ttlCt = -1;
        for (int i = 0; i < info.Length; ++i)
        {
            AssetInformation.AssetBundleVersionInfo? version = info[i];
            if (version != null)
            {
                arr[++ttlCt] = new AutoCompleteResult(i.ToString(), version.DisplayName + " ( <= " + version.EndVersion + ")");
            }
        }

        _autoComplete = arr;
        return Task.FromResult(arr);
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out int value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseInt32(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        if (parse.Database.Information.TryGetAssetBundleVersionInfo(value, out _, out _))
        {
            return true;
        }

        if (!parse.HasDiagnostics)
            return false;

        bool isMasterBundleDatFile = !parse.FileType.Type.Equals("SDG.Unturned.MasterBundleConfig, Assembly-CSharp");
        int min, max;
        if (parse.Database.Information.AssetBundleVersions is not { Length: > 0 })
        {
            min = 1;
            max = 5;
        }
        else
        {
            min = !isMasterBundleDatFile
                ? 2
                : Array.FindIndex(parse.Database.Information.AssetBundleVersions, x => x != null);
            max = Array.FindLastIndex(parse.Database.Information.AssetBundleVersions, x => x != null);
        }

        parse.Log(new DatDiagnosticMessage
        {
            Range = parse.Node.Range,
            Diagnostic = isMasterBundleDatFile ? DatDiagnostics.UNT2009 : DatDiagnostics.UNT1009,
            Message = string.Format(isMasterBundleDatFile ? DiagnosticResources.UNT2009 : DiagnosticResources.UNT1009, min, max)
        });

        return false;
    }
}
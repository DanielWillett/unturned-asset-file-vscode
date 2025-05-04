using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class AssetBundleVersionSpecPropertyType : BasicSpecPropertyType<AssetBundleVersionSpecPropertyType, int>
{
    public static readonly AssetBundleVersionSpecPropertyType Instance = new AssetBundleVersionSpecPropertyType();

    static AssetBundleVersionSpecPropertyType() { }
    private AssetBundleVersionSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "AssetBundleVersion";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Asset Bundle Version";

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
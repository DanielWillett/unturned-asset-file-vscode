using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class CaliberIdSpecPropertyType : BasicSpecPropertyType<CaliberIdSpecPropertyType, ushort>, IStringParseableSpecPropertyType, IAutoCompleteSpecPropertyType
{
    public static readonly CaliberIdSpecPropertyType Instance = new CaliberIdSpecPropertyType();

    static CaliberIdSpecPropertyType() { }
    private CaliberIdSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "CaliberId";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Caliber ID";

    protected override ISpecDynamicValue CreateValue(ushort value) => new SpecDynamicConcreteConvertibleValue<ushort>(value, this);

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (ushort.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out ushort result))
        {
            dynamicValue = SpecDynamicValue.UInt16(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<ushort>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out ushort value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseUInt16(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }

    /// <inheritdoc />
    public Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters parameters, in FileEvaluationContext context)
    {
        List<AutoCompleteResult> calibers = new List<AutoCompleteResult>();

        calibers.Add(new AutoCompleteResult("0", "Default Caliber"));

        bool isMagazine = parameters.FileType.Type.Equals("SDG.Unturned.ItemMagazineAsset, Assembly-CSharp")
                          || parameters.Property.Key.Equals("Magazine_Calibers", StringComparison.OrdinalIgnoreCase);

        bool isAttachment = !parameters.FileType.Type.Equals("SDG.Unturned.ItemMagazineAsset, Assembly-CSharp")
                          && !parameters.Property.Key.Equals("Magazine_Calibers", StringComparison.OrdinalIgnoreCase);

        bool isGun = parameters.FileType.Type.Equals("SDG.Unturned.ItemGunAsset, Assembly-CSharp");

        const int relevantFileCt = 4;

        DiscoveredDatFile[] relevantFileBuffer = new DiscoveredDatFile[relevantFileCt];
        string[] relevantFileNameBuffer = new string[relevantFileCt];

        context.Environment.ForEachCaliber((c, files) =>
        {
            bool any = false;
            int relevantFiles = 0;
            for (int i = 0; i < files.Count; ++i)
            {
                DiscoveredDatFile f = files[i];
                if (isGun != f.Type.Equals("SDG.Unturned.ItemGunAsset, Assembly-CSharp"))
                {
                    if (relevantFiles < relevantFileBuffer.Length)
                    {
                        relevantFileBuffer[relevantFiles] = f;
                        ++relevantFiles;
                    }
                    else if (f.Id != 0)
                    {
                        for (int j = 0; j < relevantFiles; ++j)
                        {
                            if (relevantFileBuffer[j].Id <= f.Id)
                                continue;

                            relevantFileBuffer[j] = f;
                            break;
                        }
                    }
                }
                if (isMagazine && f.MagazineCalibers.Contains(c))
                {
                    any = true;
                    break;
                }

                if (isAttachment && f.Calibers.Contains(c))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
                return;

            string? desc = null;
            if (relevantFiles > 0)
            {
                Array.Sort(relevantFileBuffer, 0, relevantFiles, DiscoveredDatFile.AscendingIdComparer);
                for (int i = 0; i < relevantFiles; ++i)
                {
                    relevantFileNameBuffer[i] = relevantFileBuffer[i].GetDisplayName();
                }

                desc = string.Join(", ", relevantFileNameBuffer);
            }

            calibers.Add(new AutoCompleteResult(c.ToString(CultureInfo.InvariantCulture), desc));
        });

        return Task.FromResult(calibers.ToArray());
    }
}
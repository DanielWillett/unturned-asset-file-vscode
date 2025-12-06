using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A unique ID used to indicate which weapons can damage which entities.
/// <para>Example: <c>ObjectAsset.Rubble_Blade_ID</c></para>
/// <code>
/// Prop 123
/// </code>
/// </summary>
public sealed class BladeIdSpecPropertyType : BaseSpecPropertyType<BladeIdSpecPropertyType, byte>, IStringParseableSpecPropertyType, IAutoCompleteSpecPropertyType
{
    public static readonly BladeIdSpecPropertyType Instance = new BladeIdSpecPropertyType();

    public override int GetHashCode() => 4;

    static BladeIdSpecPropertyType() { }
    private BladeIdSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "BladeId";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Blade ID";

    protected override ISpecDynamicValue CreateValue(byte value) => new SpecDynamicConcreteConvertibleValue<byte>(value, this);

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (byte.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte result))
        {
            dynamicValue = SpecDynamicValue.UInt8(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<byte>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out byte value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseUInt8(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }

    /// <inheritdoc />
    public Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters parameters, in FileEvaluationContext context)
    {
        List<AutoCompleteResult> calibers = new List<AutoCompleteResult>(512);

        calibers.Add(new AutoCompleteResult("0", "Default Blade ID"));

        AssetInformation info = parameters.Database.Information;
        bool isWeapon = info.IsAssignableFrom(
            new QualifiedType("SDG.Unturned.ItemWeaponAsset, Assembly-CSharp", true),
            parameters.FileType.Type
        );

        const int relevantFileCt = 4;

        DiscoveredDatFile[] relevantFileBuffer = new DiscoveredDatFile[relevantFileCt];
        string[] relevantFileNameBuffer = new string[relevantFileCt];

        context.Environment.ForEachBladeId((c, files) =>
        {
            bool any = false;
            int relevantFiles = 0;
            for (int i = 0; i < files.Count; ++i)
            {
                DiscoveredDatFile f = files[i];
                if (isWeapon != info.IsAssignableFrom(
                        new QualifiedType("SDG.Unturned.ItemWeaponAsset, Assembly-CSharp", true),
                        f.Type
                    ))
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

                if (f.BladeIds.Contains(c))
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
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class BlueprintIdSpecPropertyType :
    BackwardsCompatibleAssetReferenceSpecPropertyType,
    IEquatable<BlueprintIdSpecPropertyType>
{
    public static readonly BlueprintIdSpecPropertyType Instance = new BlueprintIdSpecPropertyType(true);
    public static readonly BlueprintIdSpecPropertyType StringInstance = new BlueprintIdSpecPropertyType(false);
    static BlueprintIdSpecPropertyType() { }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "BlueprintId";

    private BlueprintIdSpecPropertyType(bool canParseDictionary) : base(AssetCategory.Item.Value, canParseDictionary, OneOrMore<string>.Null) { }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out GuidOrId value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        Guid? guid = null;
        ushort? id = null;
        if (parse.File is IAssetSourceFile asset)
        {
            guid = asset.Guid;
            id = asset.Id;
        }

        if (parse.Node is IValueSourceNode strValNode && strValNode.Value.Equals("this", StringComparison.OrdinalIgnoreCase))
        {
            if (guid.HasValue && guid.Value != Guid.Empty)
            {
                value = new GuidOrId(guid.Value);
            }
            else if (id.HasValue && id.Value != 0)
            {
                value = new GuidOrId(id.Value, AssetCategory.Item);
            }
            else
            {
                if (parse.HasDiagnostics)
                {
                    parse.Log(new DatDiagnosticMessage
                    {
                        Range = strValNode.Range,
                        Diagnostic = id is 0 ? DatDiagnostics.UNT2011 : DatDiagnostics.UNT2010,
                        Message = id is 0 ? DiagnosticResources.UNT2011 : DiagnosticResources.UNT2010
                    });
                }

                value = default;
                return false;
            }

            return true;
        }

        if (!base.TryParseValue(in parse, out value))
        {
            return false;
        }

        if (parse.HasDiagnostics)
        {
            if (id.HasValue && id.Value != 0 && value.Equals(id.Value)
                || guid.HasValue && guid.Value != Guid.Empty && value.Equals(guid.Value))
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = parse.Node?.Range ?? parse.Parent?.Range ?? default,
                    Diagnostic = DatDiagnostics.UNT101,
                    Message = DiagnosticResources.UNT101
                });
            }
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(BlueprintIdSpecPropertyType other) => other != null;
}
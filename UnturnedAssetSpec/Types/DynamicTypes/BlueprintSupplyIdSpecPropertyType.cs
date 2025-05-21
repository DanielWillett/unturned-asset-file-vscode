using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class BlueprintSupplyIdSpecPropertyType :
    IdSpecPropertyType,
    IEquatable<BlueprintSupplyIdSpecPropertyType>
{
    public static readonly BlueprintSupplyIdSpecPropertyType Instance = new BlueprintSupplyIdSpecPropertyType();

    static BlueprintSupplyIdSpecPropertyType() { }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "BlueprintSupplyId";

    public BlueprintSupplyIdSpecPropertyType() : base(AssetCategory.Item, OneOrMore<string>.Null) { }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out ushort value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode)
        {
            return FailedToParse(in parse, out value);
        }

        bool wasThis = false;
        ushort id = parse.File?.GetId() ?? 0;
        if (!KnownTypeValueHelper.TryParseUInt16(strValNode.Value, out value))
        {
            if (id != 0 && string.Equals(strValNode.Value, "this", StringComparison.InvariantCultureIgnoreCase))
            {
                wasThis = true;
                value = id;
            }
            else
            {
                return FailedToParse(in parse, out value);
            }
        }

        if (id != 0 && id == value && !wasThis && parse.HasDiagnostics)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Range = strValNode.Range,
                Diagnostic = DatDiagnostics.UNT101,
                Message = DiagnosticResources.UNT101
            });
        }

        // todo: ID resolution

        return true;
    }

    /// <inheritdoc />
    public bool Equals(BlueprintSupplyIdSpecPropertyType other) => other != null && Category.Equals(other.Category);
}
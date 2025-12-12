using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A backwards-compatable reference to one or more types of assets formatted as either a string or sometimes as an object (see <see cref="CanParseDictionary"/>).
/// Accepts both <see cref="Guid"/> and <see cref="ushort"/> IDs, although in some cases the ID can't be assumed and a <see cref="AssetCategory"/> also has to be specified ('Type').
/// <para>Used for blueprint supply item references.</para>
/// <para>Assumes the category is ITEM for <see cref="ushort"/> IDs.</para>
/// <para>Example: <c>ItemAsset.BlueprintSupply.ID</c></para>
/// <code>
/// // string
/// Prop fe71781c60314468b22c6b0642a51cd9
/// Prop 1374
///
/// // object
/// Prop
/// {
///     GUID fe71781c60314468b22c6b0642a51cd9
/// }
/// Prop
/// {
///     ID 1374
/// }
///
/// // this
/// Prop this
/// </code>
/// <para>
/// If an amount is supppled (i.e. "102 x 3") a warning will be logged.
/// </para>
/// </summary>
public sealed class BlueprintIdSpecPropertyType :
    BackwardsCompatibleAssetReferenceSpecPropertyType,
    IEquatable<BlueprintIdSpecPropertyType>
{
    public override int GetHashCode()
    {
        return 64;
    }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "BlueprintId";

    public BlueprintIdSpecPropertyType(IAssetSpecDatabase database, bool canParseDictionary)
        : base(database, QualifiedType.ItemAssetType, canParseDictionary, OneOrMore<string>.Null) { }

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
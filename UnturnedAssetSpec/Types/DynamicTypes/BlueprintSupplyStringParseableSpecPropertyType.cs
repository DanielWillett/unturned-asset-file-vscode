using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Parses a blueprint item expression from it's string value.
/// <para>Example: <c>ItemAsset.Blueprint.InputItems</c></para>
/// <code>
/// Prop fe71781c60314468b22c6b0642a51cd9
/// Prop 1374
/// Prop this
/// 
/// Prop fe71781c60314468b22c6b0642a51cd9 x 5
/// Prop 1374 x 5
/// Prop this x 5
/// </code>
/// </summary>
public sealed class BlueprintSupplyStringParseableSpecPropertyType :
    BlueprintItemStringParseableSpecPropertyType,
    ISpecPropertyType<CustomSpecTypeInstance>,
    IEquatable<BlueprintSupplyStringParseableSpecPropertyType?>
{
    public BlueprintSupplyStringParseableSpecPropertyType(IAssetSpecDatabase database)
        : base(database) { }

    public override int GetHashCode()
    {
        return 65;
    }

    public override string Type => "DanielWillett.UnturnedDataFileLspServer.Data.Types.BlueprintSupplyStringParseableSpecPropertyType, UnturnedAssetSpec";

    public override string DisplayName => "Blueprint Supply Item";

    public override string BackingType => "SDG.Unturned.BlueprintSupply, Assembly-CSharp";

    public override bool Equals(ISpecPropertyType? other) => other is BlueprintSupplyStringParseableSpecPropertyType;
    public override bool Equals(ISpecPropertyType<CustomSpecTypeInstance>? other) => other is BlueprintSupplyStringParseableSpecPropertyType;
    public override bool Equals(BlueprintItemStringParseableSpecPropertyType? other) => other is BlueprintSupplyStringParseableSpecPropertyType;
    public bool Equals(BlueprintSupplyStringParseableSpecPropertyType? other) => other != null;

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}
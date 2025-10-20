using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class BlueprintSupplyStringParseableSpecPropertyType :
    BlueprintItemStringParseableSpecPropertyType,
    ISpecPropertyType<CustomSpecTypeInstance>,
    IEquatable<BlueprintSupplyStringParseableSpecPropertyType?>
{
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
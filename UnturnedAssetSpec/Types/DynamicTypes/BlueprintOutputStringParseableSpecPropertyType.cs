using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class BlueprintOutputStringParseableSpecPropertyType :
    BlueprintItemStringParseableSpecPropertyType,
    ISpecPropertyType<CustomSpecTypeInstance>,
    IEquatable<BlueprintOutputStringParseableSpecPropertyType?>
{
    public override string Type => "DanielWillett.UnturnedDataFileLspServer.Data.Types.BlueprintOutputStringParseableSpecPropertyType, UnturnedAssetSpec";

    public override string DisplayName => "Blueprint Output Item";

    public override string BackingType => "SDG.Unturned.BlueprintOutput, Assembly-CSharp";

    public override bool Equals(ISpecPropertyType? other) => other is BlueprintOutputStringParseableSpecPropertyType;
    public override bool Equals(ISpecPropertyType<CustomSpecTypeInstance>? other) => other is BlueprintOutputStringParseableSpecPropertyType;
    public override bool Equals(BlueprintItemStringParseableSpecPropertyType? other) => other is BlueprintOutputStringParseableSpecPropertyType;
    public bool Equals(BlueprintOutputStringParseableSpecPropertyType? other) => other != null;

    void ISpecPropertyType.Visit<TVisitor>(TVisitor visitor) => visitor.Visit(this);
}
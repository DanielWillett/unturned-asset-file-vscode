using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Parses a blueprint item expression from it's string value.
/// <para>Example: <c>ItemAsset.Blueprint.OutputItems</c></para>
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
public sealed class BlueprintOutputStringParseableSpecPropertyType :
    BlueprintItemStringParseableSpecPropertyType,
    ISpecPropertyType<CustomSpecTypeInstance>,
    IEquatable<BlueprintOutputStringParseableSpecPropertyType?>
{
    public override int GetHashCode()
    {
        return 63;
    }

    public BlueprintOutputStringParseableSpecPropertyType(IAssetSpecDatabase database)
        : base(database)
    {
        
    }

    public override string Type => "DanielWillett.UnturnedDataFileLspServer.Data.Types.BlueprintOutputStringParseableSpecPropertyType, UnturnedAssetSpec";

    public override string DisplayName => "Blueprint Output Item";

    public override string BackingType => "SDG.Unturned.BlueprintOutput, Assembly-CSharp";

    public override bool Equals(ISpecPropertyType? other) => other is BlueprintOutputStringParseableSpecPropertyType;
    public override bool Equals(ISpecPropertyType<CustomSpecTypeInstance>? other) => other is BlueprintOutputStringParseableSpecPropertyType;
    public override bool Equals(BlueprintItemStringParseableSpecPropertyType? other) => other is BlueprintOutputStringParseableSpecPropertyType;
    public bool Equals(BlueprintOutputStringParseableSpecPropertyType? other) => other != null;

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}
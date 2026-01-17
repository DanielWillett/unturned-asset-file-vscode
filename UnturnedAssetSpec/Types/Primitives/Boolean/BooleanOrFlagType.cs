using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A boolean/flag property with the following truth table:
/// <list type="table">
///     <listheader>
///         <term>Property</term>
///         <term>Value</term>
///     </listheader>
///     <item>
///         <term>Prop</term>
///         <term>True</term>
///     </item>
///     <item>
///         <term>Prop True</term>
///         <term>True</term>
///     </item>
///     <item>
///         <term>Prop False</term>
///         <term>False</term>
///     </item>
/// </list>
/// It's usually written in Unturned as
/// <code>
/// Property = !p.data.TryParseBool("Prop", out bool b) ? p.data.ContainsKey("Prop") : b;
/// </code>
/// Example: <c>ItemBarricadeAsset.Bypass_Claim</c>
/// </summary>
public sealed class BooleanOrFlagType : PrimitiveType<bool, BooleanOrFlagType>, IFlagType
{
    public const string TypeId = "BooleanOrFlag";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_BooleanOrFlag;

    public override ITypeParser<bool> Parser => TypeParsers.BooleanOrFlag;

    public override int GetHashCode() => 2025699832;
}
namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable InconsistentNaming

/// <summary>
/// A time or time span.
/// <para>Example: <c>ServerListCurationFile.ServerListCurationRule.Filter</c></para>
/// <code>
/// // one of:
/// Prop 1.2.3.4/24:27015-27016
/// Prop 1.2.3.4/24:27015
/// Prop 1.2.3.4/24
/// Prop 1.2.3.4:27015-27016
/// Prop 1.2.3.4:27015
/// Prop 1.2.3.4
/// </code>
/// </summary>
public sealed class IPv4FilterType : PrimitiveType<IPv4Filter, IPv4FilterType>
{
    public const string TypeId = "IPv4Filter";
    
    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_IPv4Filter;

    public override int GetHashCode() => 1397376879;
}
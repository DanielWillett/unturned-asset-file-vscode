using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A 128-bit globally unique identifier (<see cref="Guid"/>). Not an asset reference.
/// <para>Example: <c>Asset.GUID</c></para>
/// <code>
/// Prop 7a85ae60118140a7a9289eeba5ac5772
/// </code>
/// </summary>
/// <remarks>This type should not be used for asset references, instead <see cref="AssetReferenceType"/> should be used instead ('AssetReferenceString').</remarks>
public sealed class GuidType : PrimitiveType<Guid, GuidType>
{
    public const string TypeId = "Guid";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Guid;

    public override int GetHashCode() => 629138186;
}
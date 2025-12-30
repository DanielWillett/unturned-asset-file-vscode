using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A 128-bit globally unique identifier (<see cref="Guid"/>) or 16-bit unsigned integer (<see cref="ushort"/>). Not an asset reference.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop 7a85ae60118140a7a9289eeba5ac5772
/// Prop 31902
/// </code>
/// </summary>
/// <remarks>This type should not be used for asset references, instead <see cref="AssetReferenceType"/> should be used instead ('LegacyAssetReferenceString').</remarks>
public sealed class GuidOrIdType : PrimitiveType<GuidOrId, GuidOrIdType>
{
    // this type is mostly unused but is needed for the
    // 'DefaultType' property of the type converter for GuidOrId

    public const string TypeId = "GuidOrId";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_GuidOrId;

    public override int GetHashCode() => 840656742;
}
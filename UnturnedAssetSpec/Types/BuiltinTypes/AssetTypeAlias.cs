using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// The auto-generated enum type for the asset type property (<c>Asset.Type</c>).
/// </summary>
[SpecificationType(FactoryMethod = nameof(Create))]
#if NET5_0_OR_GREATER
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicMethods)]
#endif
public sealed class AssetTypeAlias : DatEnumType
{
    public const string TypeId = "DanielWillett.UnturnedDataFileLspServer.Data.Types.AssetTypeAlias, UnturnedAssetSpec";

    /// <summary>
    /// Factory method for the <see cref="AssetTypeAlias"/> type.
    /// </summary>
    private static AssetTypeAlias Create(in SpecificationTypeFactoryArgs args)
    {
        return new AssetTypeAlias(args.Context.DatabaseFacade, args.Owner.Owner);
    }

    internal AssetTypeAlias(IAssetSpecDatabase database, DatFileType owner)
        : base(TypeId, default, owner)
    {
        AssetInformation information = database.Information;
        Dictionary<string, QualifiedType> dict = information.AssetAliases;
        ImmutableArray<DatEnumValue>.Builder bldr = ImmutableArray.CreateBuilder<DatEnumValue>(dict.Count);

        int index = -1;
        JsonElement element = default;
        foreach (KeyValuePair<string, QualifiedType> enumValue in dict)
        {
            DatEnumValue autoCreatedValue = new DatEnumValue(enumValue.Value, ++index, this, element)
            {
                CorrespondingType = enumValue.Value.CaseInsensitive
            };
            autoCreatedValue.RequiredBaseType = autoCreatedValue.CorrespondingType;
            bldr.Add(autoCreatedValue);
        }

        Values = bldr.MoveToImmutableOrCopy();
        DisplayNameIntl = Resources.Type_Name_AssetTypeAlias;
        Docs = "https://docs.smartlydressedgames.com/en/stable/assets/asset-definitions.html";

        database.OnInitialize(UpdateDescriptionsOnDatabaseInitialize);
    }

    private Task UpdateDescriptionsOnDatabaseInitialize(IAssetSpecDatabase database)
    {
        foreach (DatEnumValue value in Values)
        {
            if (!database.FileTypes.TryGetValue(value.CorrespondingType, out DatFileType? type))
                continue;

            value.Description = string.Format(Resources.Type_AssetTypeAlias_Description, type.DisplayName);
            value.Docs = type.Docs;
            value.Version = type.Version;
        }

        return Task.CompletedTask;
    }
}

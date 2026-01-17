using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
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
        return new AssetTypeAlias(args.Context, args.Owner.Owner);
    }

    internal AssetTypeAlias(IDatSpecificationReadContext context, DatFileType owner)
        : base(TypeId, default, owner)
    {
        AssetInformation information = context.Information;
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

        context.Database.OnInitialize(UpdateDescriptionsOnDatabaseInitialize);
    }

    private Task UpdateDescriptionsOnDatabaseInitialize(IAssetSpecDatabase database, ILoggerFactory loggerFactory)
    {
        ILogger<AssetTypeAlias>? logger = null;
        foreach (DatEnumValue value in Values)
        {
            if (!database.FileTypes.TryGetValue(value.CorrespondingType, out DatFileType? type))
            {
                logger ??= loggerFactory.CreateLogger<AssetTypeAlias>();
                logger.LogError("Unknown asset type {0} configured for asset alias {1}.", value.CorrespondingType.Type, value.Value);
                continue;
            }

            value.Description = string.Format(Resources.Type_AssetTypeAlias_Description, type.DisplayName);
            value.Docs = type.Docs;
            value.Version = type.Version;
        }

        return Task.CompletedTask;
    }
}

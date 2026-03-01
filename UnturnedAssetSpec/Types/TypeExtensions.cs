using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public static class TypeExtensions
{
    extension(IAssetReferenceType assetRefType)
    {
        /// <inheritdoc cref="TypeExtensions.TryGetTargetCategory(IAssetReferenceType,AssetInformation,out AssetCategoryValue)"/>
        public bool TryGetTargetCategory(IAssetSpecDatabase database, out AssetCategoryValue category)
        {
            return assetRefType.TryGetTargetCategory(database.Information, out category);
        }

        /// <summary>
        /// Gets the only target category of a <see cref="IAssetReferenceType"/>.
        /// Will fail if multiple categories can be chosen from.
        /// </summary>
        public bool TryGetTargetCategory(AssetInformation info, out AssetCategoryValue category)
        {
            OneOrMore<QualifiedType> baseTypes = assetRefType.BaseTypes;
            int runningCategory = -1;
            foreach (QualifiedType type in baseTypes)
            {
                int c = AssetCategory.GetCategoryFromType(type, info);
                if (c == 0 || runningCategory == c)
                    continue;

                if (runningCategory < 0)
                {
                    runningCategory = c;
                    continue;
                }

                // multiple categories can be chosen
                runningCategory = -1;
                break;
            }

            if (runningCategory < 0)
            {
                category = AssetCategoryValue.None;
                return false;
            }

            category = new AssetCategoryValue(runningCategory);
            return true;
        }
    }
}

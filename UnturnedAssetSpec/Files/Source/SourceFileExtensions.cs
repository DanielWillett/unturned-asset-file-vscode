using System;
using System.Collections.Immutable;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public static class SourceFileExtensions
{
    extension(IAssetSourceFile file)
    {
        /// <summary>
        /// Gets the localization file for the given language, falling back to English, then to the first available localization file.
        /// </summary>
        /// <param name="preferredLanguage">The language to prioritize, defaulting to 'English'. If this parameter is overridden and the language isn't available, it will fall back to English.</param>
        /// <returns>The first localization file available, or <see langword="null"/> if there are no localization files for this asset.</returns>
        public ILocalizationSourceFile? GetDefaultSourceFile(string preferredLanguage = "English")
        {
            ImmutableArray<ILocalizationSourceFile> locals = file.Localization;

            if (locals.IsDefaultOrEmpty)
                return null;

            foreach (ILocalizationSourceFile local in locals)
            {
                if (string.Equals(local.LanguageName, preferredLanguage, StringComparison.OrdinalIgnoreCase))
                    return local;
            }

            if (!string.Equals(preferredLanguage, "English", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ILocalizationSourceFile local in locals)
                {
                    if (string.Equals(local.LanguageName, "English", StringComparison.OrdinalIgnoreCase))
                        return local;
                }
            }

            return locals[0];
        }
    }
}

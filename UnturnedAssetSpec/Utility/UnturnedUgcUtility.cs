using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal class UnturnedUgcUtility
{
    public static bool TryGetUgcType(string directory, out UgcType type)
    {
        if (File.Exists(Path.Combine(directory, "Localization.meta")))
        {
            type = UgcType.Localization;
            return true;
        }

        if (File.Exists(Path.Combine(directory, "Object.meta")))
        {
            type = UgcType.Object;
            return true;
        }

        if (File.Exists(Path.Combine(directory, "Item.meta")))
        {
            type = UgcType.Item;
            return true;
        }

        if (File.Exists(Path.Combine(directory, "Vehicle.meta")))
        {
            type = UgcType.Vehicle;
            return true;
        }

        if (File.Exists(Path.Combine(directory, "Skin.meta")))
        {
            type = UgcType.Skin;
            return true;
        }

        if (File.Exists(Path.Combine(directory, "Map.meta")))
        {
            type = UgcType.Map;
            return true;
        }

        type = 0;
        return false;
    }

    public static bool IsUgcEnabled(ulong modId, string installDir, ref List<ulong>? disabledMods)
    {
        if (disabledMods != null)
        {
            return !disabledMods.Contains(modId);
        }

        string convenientSavedata = Path.Combine(installDir, "Cloud", "ConvenientSavedata.json");
        disabledMods = new List<ulong>(0);
        if (!File.Exists(convenientSavedata))
        {
            return false;
        }

        JsonDocument document;
        using (FileStream fs = new FileStream(convenientSavedata, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            document = JsonDocument.Parse(fs, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
        }

        using (document)
        {
            if (document.RootElement.ValueKind == JsonValueKind.Object
                && document.RootElement.TryGetProperty("Booleans", out JsonElement element)
                && element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.False
                        && property.Name.StartsWith("Enabled_Workshop_Item_", StringComparison.Ordinal)
                        && property.Name.Length > 22
                        && ulong.TryParse(property.Name.Substring(22), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong fileId))
                    {
                        if (property.Value.ValueKind == JsonValueKind.False)
                            disabledMods.Add(fileId);
                    }
                }
            }
        }

        return !disabledMods.Contains(modId);
    }

    public enum UgcType
    {
        Map,
        Localization,
        Object,
        Item,
        Vehicle,
        Skin
    }
}

using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Special-case enum type for EAssetType.
/// </summary>
public sealed class AssetCategory : EnumSpecType, IEquatable<AssetCategory>, IEquatable<EnumSpecType>, IComparable<AssetCategory>, IComparable<EnumSpecType>
{
    public static readonly AssetCategory TypeOf = new AssetCategory
    {
        DisplayName = "Asset Category",
        Type = "SDG.Unturned.EAssetType, Assembly-CSharp",
        Docs = "https://docs.smartlydressedgames.com/en/stable/data/enum/eassettype.html",
        Values = new EnumSpecTypeValue[11],
        ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
    };

    public static readonly EnumSpecTypeValue None;
    public static readonly EnumSpecTypeValue Item;
    public static readonly EnumSpecTypeValue Effect;
    public static readonly EnumSpecTypeValue Object;
    public static readonly EnumSpecTypeValue Resource;
    public static readonly EnumSpecTypeValue Vehicle;
    public static readonly EnumSpecTypeValue Animal;
    public static readonly EnumSpecTypeValue Mythic;
    public static readonly EnumSpecTypeValue Skin;
    public static readonly EnumSpecTypeValue Spawn;
    public static readonly EnumSpecTypeValue NPC;

    private AssetCategory() { }
    static AssetCategory()
    {
        TypeOf.Values[0] = None = new EnumSpecTypeValue
        {
            Value = "NONE",
            Casing = "None",
            Description = "Doesn't fall into a legacy category. In this case short IDs are not used.",
            Index = 0,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
        TypeOf.Values[1] = Item = new EnumSpecTypeValue
        {
            Value = "ITEM",
            Casing = "Item",
            Description = "Any item.",
            Index = 1,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
        TypeOf.Values[2] = Effect = new EnumSpecTypeValue
        {
            Value = "EFFECT",
            Casing = "Effect",
            Description = "Any world effect or UI.",
            Index = 2,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
        TypeOf.Values[3] = Object = new EnumSpecTypeValue
        {
            Value = "OBJECT",
            Casing = "Object",
            Description = "Any level object or NPC.",
            Index = 3,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
        TypeOf.Values[4] = Resource = new EnumSpecTypeValue
        {
            Value = "RESOURCE",
            Casing = "Resource",
            Description = "A resource that can spawn on the map and be harvested.",
            Index = 4,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
        TypeOf.Values[5] = Vehicle = new EnumSpecTypeValue
        {
            Value = "VEHICLE",
            Casing = "Vehicle",
            Description = "Any vehicle.",
            Index = 5,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
        TypeOf.Values[6] = Animal = new EnumSpecTypeValue
        {
            Value = "ANIMAL",
            Casing = "Animal",
            Description = "Any animal.",
            Index = 6,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
        TypeOf.Values[7] = Mythic = new EnumSpecTypeValue
        {
            Value = "MYTHIC",
            Casing = "Mythic",
            Description = "A mythical cosmetic effect.",
            Index = 7,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
        TypeOf.Values[8] = Skin = new EnumSpecTypeValue
        {
            Value = "SKIN",
            Casing = "Skin",
            Description = "An item or vehicle skin.",
            Index = 8,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
        TypeOf.Values[9] = Spawn = new EnumSpecTypeValue
        {
            Value = "SPAWN",
            Casing = "Spawn",
            Description = "An asset spawn for a map.",
            Index = 9,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
        TypeOf.Values[10] = NPC = new EnumSpecTypeValue
        {
            Value = "NPC",
            Casing = "NPC",
            Description = "A dialogue, quest, or vendor configuration.",
            Index = 10,
            Type = TypeOf,
            ExtendedData = OneOrMore<KeyValuePair<string, object?>>.Null
        };
    }

    public static bool HasFriendlyName(int category)
    {
        return category is 1 or 3 or 4 or 5 or 6 or 10;
    }

    /// <summary>
    /// Gets the asset category from the given type, with special handling for redirector assets.
    /// </summary>
    /// <returns>The index of the type, or -1 if this type is a redirector asset.</returns>
    public static int GetCategoryFromType(QualifiedType type, IAssetSpecDatabase database)
    {
        if (type.Equals("SDG.Unturned.RedirectorAsset, Assembly-CSharp"))
        {
            return -1;
        }

        InverseTypeHierarchy typeHierarchy = database.Information.GetParentTypes(type);

        QualifiedType[] types = typeHierarchy.ParentTypes;
        for (int i = types.Length - 1; i >= 0; --i)
        {
            if (!database.Information.AssetCategories.TryGetValue(typeHierarchy.ParentTypes[i], out string category))
                continue;

            if (TryParse(category, out EnumSpecTypeValue categoryType))
            {
                return categoryType.Index;
            }
        }

        return 0;
    }

    public static bool TryParse(string? str, out EnumSpecTypeValue category)
    {
        if (str == null)
        {
            category = default;
            return false;
        }

        str = str.Trim();

        if (str.Length < 3)
        {
            category = default;
            return false;
        }

        switch (str[0])
        {
            case 'N':
            case 'n':
                if (str.Equals("NONE", StringComparison.OrdinalIgnoreCase))
                {
                    category = None;
                    return true;
                }
                if (str.Equals("NPC", StringComparison.OrdinalIgnoreCase))
                {
                    category = NPC;
                    return true;
                }

                break;

            case 'I':
            case 'i':
                if (str.Equals("ITEM", StringComparison.OrdinalIgnoreCase))
                {
                    category = Item;
                    return true;
                }

                break;

            case 'E':
            case 'e':
                if (str.Equals("EFFECT", StringComparison.OrdinalIgnoreCase))
                {
                    category = Effect;
                    return true;
                }

                break;

            case 'O':
            case 'o':
                if (str.Equals("OBJECT", StringComparison.OrdinalIgnoreCase))
                {
                    category = Effect;
                    return true;
                }

                break;

            case 'R':
            case 'r':
                if (str.Equals("RESOURCE", StringComparison.OrdinalIgnoreCase))
                {
                    category = Resource;
                    return true;
                }

                break;

            case 'V':
            case 'v':
                if (str.Equals("VEHICLE", StringComparison.OrdinalIgnoreCase))
                {
                    category = Vehicle;
                    return true;
                }

                break;

            case 'M':
            case 'm':
                if (str.Equals("MYTHIC", StringComparison.OrdinalIgnoreCase))
                {
                    category = Mythic;
                    return true;
                }

                break;

            case 'A':
            case 'a':
                if (str.Equals("ANIMAL", StringComparison.OrdinalIgnoreCase))
                {
                    category = Animal;
                    return true;
                }

                break;

            case 'S':
            case 's':
                if (str.Equals("SKIN", StringComparison.OrdinalIgnoreCase))
                {
                    category = Skin;
                    return true;
                }
                if (str.Equals("SPAWN", StringComparison.OrdinalIgnoreCase))
                {
                    category = Spawn;
                    return true;
                }

                break;
        }

        if (char.IsDigit(str[0]) && int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out int num) && num is >= 0 and < 11)
        {
            category = TypeOf.Values[num];
            return true;
        }

        category = default;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(AssetCategory other) => true;

    /// <inheritdoc />
    public int CompareTo(AssetCategory other) => 0;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    bool IEquatable<EnumSpecType>.Equals(EnumSpecType other)
    {
        return other is AssetCategory;
    }

    int IComparable<EnumSpecType>.CompareTo(EnumSpecType other)
    {
        return other is AssetCategory ? 0 : 1;
    }
}
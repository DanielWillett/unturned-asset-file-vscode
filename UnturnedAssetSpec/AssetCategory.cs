using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data;

/// <summary>
/// A reference to an instance of the <see cref="AssetCategory"/> enum.
/// </summary>
[DebuggerDisplay("{Value,nq}")]
public readonly struct AssetCategoryValue : IEquatable<AssetCategoryValue>, IComparable<AssetCategoryValue>
{
    public static readonly AssetCategoryValue None = new AssetCategoryValue(0);
    public static readonly AssetCategoryValue Item = new AssetCategoryValue(1);
    public static readonly AssetCategoryValue Effect = new AssetCategoryValue(2);
    public static readonly AssetCategoryValue Object = new AssetCategoryValue(3);
    public static readonly AssetCategoryValue Resource = new AssetCategoryValue(4);
    public static readonly AssetCategoryValue Vehicle = new AssetCategoryValue(5);
    public static readonly AssetCategoryValue Animal = new AssetCategoryValue(6);
    public static readonly AssetCategoryValue Mythic = new AssetCategoryValue(7);
    public static readonly AssetCategoryValue Skin = new AssetCategoryValue(8);
    public static readonly AssetCategoryValue Spawn = new AssetCategoryValue(9);
    public static readonly AssetCategoryValue NPC = new AssetCategoryValue(10);

    private readonly byte _index;

    /// <summary>
    /// The index of this category within the <see cref="AssetCategory"/> (EAssetType) enum.
    /// </summary>
    public int Index => _index;

    /// <summary>
    /// Information about the asset category enum entry referenced by this value.
    /// </summary>
    public DatEnumValue Enum => AssetCategory.Instance.Values[_index];

    /// <summary>
    /// The exact string value of this category.
    /// </summary>
    public string Value => Enum.Value;

    /// <summary>
    /// The preferred casing of this category.
    /// </summary>
    public string Casing => Enum.Casing;

    /// <summary>
    /// Construct a reference to the asset category at index <paramref name="index"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public AssetCategoryValue(int index)
    {
        if (index < 0 || index >= AssetCategory.Instance.Values.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        _index = (byte)index;
    }

    /// <summary>
    /// Construct a reference to the asset category named <paramref name="str"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public AssetCategoryValue(string str)
    {
        if (!AssetCategory.TryParse(str, out int index))
            throw new ArgumentOutOfRangeException(nameof(str));

        _index = (byte)index;
    }

    /// <inheritdoc />
    public int CompareTo(AssetCategoryValue other)
    {
        return _index.CompareTo(other._index);
    }

    /// <inheritdoc />
    public bool Equals(AssetCategoryValue other)
    {
        return _index == other._index;
    }

    /// <inheritdoc />
    public override string ToString() => Value;

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is AssetCategoryValue v && Equals(v);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _index;
    }

    /// <summary>
    /// Checks whether or not two <see cref="AssetCategoryValue"/> values reference the same category.
    /// </summary>
    public static bool operator ==(AssetCategoryValue left, AssetCategoryValue right) => left.Equals(right);

    /// <summary>
    /// Checks whether or not two <see cref="AssetCategoryValue"/> values reference different categories.
    /// </summary>
    public static bool operator !=(AssetCategoryValue left, AssetCategoryValue right) => !left.Equals(right);
}

/// <summary>
/// Special-case enum type for EAssetType.
/// </summary>
public sealed class AssetCategory : DatEnumType, IEquatable<AssetCategory>, IComparable<AssetCategory>
{
    /// <summary>
    /// The singleton instance of the <see cref="AssetCategory"/> type.
    /// </summary>
    public static readonly AssetCategory Instance;

    /// <summary>
    /// The type ID of <see cref="AssetCategory"/>.
    /// </summary>
    public const string TypeId = "SDG.Unturned.EAssetType, Assembly-CSharp";

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.NONE</c>.
    /// </summary>
    public static DatEnumValue None { get; }

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.ITEM</c>.
    /// </summary>
    public static DatEnumValue Item { get; }

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.EFFECT</c>.
    /// </summary>
    public static DatEnumValue Effect { get; }

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.OBJECT</c>.
    /// </summary>
    public static DatEnumValue Object { get; }

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.RESOURCE</c>.
    /// </summary>
    public static DatEnumValue Resource { get; }

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.VEHICLE</c>.
    /// </summary>
    public static DatEnumValue Vehicle { get; }

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.ANIMAL</c>.
    /// </summary>
    public static DatEnumValue Animal { get; }

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.MYTHIC</c>.
    /// </summary>
    public static DatEnumValue Mythic { get; }

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.SKIN</c>.
    /// </summary>
    public static DatEnumValue Skin { get; }

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.SPAWN</c>.
    /// </summary>
    public static DatEnumValue Spawn { get; }

    /// <summary>
    /// The <see cref="AssetCategory"/> enum value for <c>EAssetType.NPC</c>.
    /// </summary>
    public static DatEnumValue NPC { get; }

    private AssetCategory(QualifiedType typeName, DatFileType owner) : base(typeName, default, owner) { }
    static AssetCategory()
    {
        QualifiedType typeName = new QualifiedType(TypeId, true);
        
        Instance = new AssetCategory(typeName, CreateFileType(typeName, false, default, null))
        {
            Docs = "https://docs.smartlydressedgames.com/en/stable/data/enum/eassettype.html",
            DisplayNameIntl = Resources.Type_Name_EAssetType
        };

        ImmutableArray<DatEnumValue>.Builder values = ImmutableArray.CreateBuilder<DatEnumValue>(11);

        values.Add(None = DatEnumValue.Create("NONE", 0, Instance, default));
        None.Casing = "None";
        None.Description = Resources.Description_EAssetType_NONE;
        None.NumericValue = 0;

        values.Add(Item = DatEnumValue.Create("ITEM", 1, Instance, default));
        Item.Casing = "Item";
        Item.Description = Resources.Description_EAssetType_ITEM;
        Item.NumericValue = 1;

        values.Add(Effect = DatEnumValue.Create("EFFECT", 2, Instance, default));
        Effect.Casing = "Effect";
        Effect.Description = Resources.Description_EAssetType_EFFECT;
        Effect.NumericValue = 2;

        values.Add(Object = DatEnumValue.Create("OBJECT", 3, Instance, default));
        Object.Casing = "Object";
        Object.Description = Resources.Description_EAssetType_OBJECT;
        Object.NumericValue = 3;

        values.Add(Resource = DatEnumValue.Create("RESOURCE", 4, Instance, default));
        Resource.Casing = "Resource";
        Resource.Description = Resources.Description_EAssetType_RESOURCE;
        Resource.NumericValue = 4;

        values.Add(Vehicle = DatEnumValue.Create("VEHICLE", 5, Instance, default));
        Vehicle.Casing = "Vehicle";
        Vehicle.Description = Resources.Description_EAssetType_VEHICLE;
        Vehicle.NumericValue = 5;

        values.Add(Animal = DatEnumValue.Create("ANIMAL", 6, Instance, default));
        Animal.Casing = "Animal";
        Animal.Description = Resources.Description_EAssetType_ANIMAL;
        Animal.NumericValue = 6;

        values.Add(Mythic = DatEnumValue.Create("MYTHIC", 7, Instance, default));
        Mythic.Casing = "Mythic";
        Mythic.Description = Resources.Description_EAssetType_MYTHIC;
        Mythic.NumericValue = 7;

        values.Add(Skin = DatEnumValue.Create("SKIN", 8, Instance, default));
        Skin.Casing = "Skin";
        Skin.Description = Resources.Description_EAssetType_SKIN;
        Skin.NumericValue = 8;

        values.Add(Spawn = DatEnumValue.Create("SPAWN", 9, Instance, default));
        Spawn.Casing = "Spawn";
        Spawn.Description = Resources.Description_EAssetType_SPAWN;
        Spawn.NumericValue = 9;

        values.Add(NPC = DatEnumValue.Create("NPC", 10, Instance, default));
        NPC.Casing = "NPC";
        NPC.Description = Resources.Description_EAssetType_NPC;
        NPC.NumericValue = 10;

        Instance.Values = values.MoveToImmutable();
    }

    /// <summary>
    /// Whether or not the given <paramref name="category"/> has a friendly/localized name.
    /// </summary>
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
        return GetCategoryFromType(type, database.Information);
    }

    /// <summary>
    /// Gets the asset category from the given type, with special handling for redirector assets.
    /// </summary>
    /// <returns>The index of the type, or -1 if this type is a redirector asset.</returns>
    public static int GetCategoryFromType(QualifiedType type, AssetInformation info)
    {
        if (type.Equals("SDG.Unturned.RedirectorAsset, Assembly-CSharp"))
        {
            return -1;
        }

        InverseTypeHierarchy typeHierarchy = info.GetParentTypes(type);

        QualifiedType[] types = typeHierarchy.ParentTypes;
        for (int i = types.Length - 1; i >= 0; --i)
        {
            if (!info.AssetCategories.TryGetValue(typeHierarchy.ParentTypes[i], out string? category))
                continue;

            if (TryParse(category, out DatEnumValue? categoryType))
            {
                return categoryType.Index;
            }
        }

        return 0;
    }

    /// <inheritdoc />
    public override bool TryParse(ReadOnlySpan<char> text, [NotNullWhen(true)] out DatEnumValue? value, bool caseInsensitive = true)
    {
        if (!caseInsensitive)
        {
            return base.TryParse(text, out value, caseInsensitive);
        }

        if (TryParse(text, out int index))
        {
            value = Values[index];
            return true;
        }

        value = null;
        return false;
    }

    public static bool TryParse(string? str, [NotNullWhen(true)] out DatEnumValue? category)
    {
        return TryParse(str.AsSpan(), out category);
    }

    public static bool TryParse(string? str, out int index)
    {
        return TryParse(str.AsSpan(), out index);
    }

    public static bool TryParse(ReadOnlySpan<char> str, [NotNullWhen(true)] out DatEnumValue? category)
    {
        if (!TryParse(str, out int index))
        {
            category = null;
            return false;
        }

        category = Instance.Values[index];
        return true;
    }

    public static bool TryParse(ReadOnlySpan<char> str, out int index)
    {
        str = str.Trim();

        if (str.Length < 3)
        {
            index = 0;
            return false;
        }

        switch (str[0])
        {
            case 'N':
            case 'n':
                if (str.Equals("NONE".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = None.Index;
                    return true;
                }
                if (str.Equals("NPC".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = NPC.Index;
                    return true;
                }

                break;

            case 'I':
            case 'i':
                if (str.Equals("ITEM".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = Item.Index;
                    return true;
                }

                break;

            case 'E':
            case 'e':
                if (str.Equals("EFFECT".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = Effect.Index;
                    return true;
                }

                break;

            case 'O':
            case 'o':
                if (str.Equals("OBJECT".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = Effect.Index;
                    return true;
                }

                break;

            case 'R':
            case 'r':
                if (str.Equals("RESOURCE".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = Resource.Index;
                    return true;
                }

                break;

            case 'V':
            case 'v':
                if (str.Equals("VEHICLE".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = Vehicle.Index;
                    return true;
                }

                break;

            case 'M':
            case 'm':
                if (str.Equals("MYTHIC".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = Mythic.Index;
                    return true;
                }

                break;

            case 'A':
            case 'a':
                if (str.Equals("ANIMAL".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = Animal.Index;
                    return true;
                }

                break;

            case 'S':
            case 's':
                if (str.Equals("SKIN".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = Skin.Index;
                    return true;
                }
                if (str.Equals("SPAWN".AsSpan(), StringComparison.OrdinalIgnoreCase))
                {
                    index = Spawn.Index;
                    return true;
                }

                break;
        }

        if (char.IsDigit(str[0]) && int.TryParse(str.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int num) && num >= 0 && num < Instance.Values.Length)
        {
            index = num;
            return true;
        }

        index = 0;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(AssetCategory? other) => other != null;

    /// <inheritdoc />
    public int CompareTo(AssetCategory? other) => other == null ? 1 : 0;

    /// <inheritdoc />
    public override int GetHashCode() => 543963484;
}
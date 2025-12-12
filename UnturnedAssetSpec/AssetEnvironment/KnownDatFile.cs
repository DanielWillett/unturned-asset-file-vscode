using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;

[DebuggerDisplay("{Name} [{DanielWillett.UnturnedDataFileLspServer.Data.Types.AssetCategory.TypeOf.Values[Category],nq}]")]
public class DiscoveredDatFile : IEquatable<DiscoveredDatFile>
{
    private readonly int _nameIndexStart, _nameLength;
    private string? _name;

    internal DiscoveredDatFile? Next;
    internal DiscoveredDatFile? Prev;

    public static IComparer<DiscoveredDatFile> AscendingIdComparer => IdComparer.Instance;

    public bool IsRemoved { get; internal set; }

    public string FilePath { get; }
    public Guid Guid { get; }
    public ushort Id { get; }
    public int Category { get; }

    /// <summary>
    /// Note: blade ids are currently bytes but i feel this will be changed at some point.
    /// </summary>
    public OneOrMore<byte> BladeIds { get; private set; }
    public OneOrMore<ushort> Calibers { get; private set; }
    public OneOrMore<ushort> MagazineCalibers { get; private set; }

    public ReadOnlySpan<char> Name => FilePath.AsSpan(_nameIndexStart, _nameLength);

    public string? LocalizationFilePath { get; private set; }

    public QualifiedType Type { get; }
    
    public string? FriendlyName { get; internal set; }
    public string AssetName => _name ??= Name.ToString();

    public string GetDisplayName()
    {
        if (FriendlyName != null)
            return FriendlyName;

        return _name ??= Name.ToString();
    }

    private enum NextValueType { None = -1, Metadata, Asset, Guid, Id, Type, AssetCategory }
    private enum NextCaliberType { None = -1, MagazineCaliber, AttachmentCaliber, MagazineCalibers, AttachmentCalibers, LegacyCaliber }
    private enum NextBladeType { None = -1, Rubble, Interactability, BladeIds, BladeId }

    public DiscoveredDatFile(string fileName, ReadOnlySpan<char> text, IAssetSpecDatabase database, ICollection<DatDiagnosticMessage>? diagMessages, Action<string, string>? log)
    {
        FilePath = fileName;

        int lastIndex = fileName.LastIndexOf(Path.DirectorySeparatorChar);
        if (fileName.AsSpan(lastIndex + 1).Equals("Asset.dat".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            int secondToLastFolder = lastIndex <= 1 ? -1 : fileName.LastIndexOf(Path.DirectorySeparatorChar, lastIndex - 1);
            _nameIndexStart = secondToLastFolder + 1;
            _nameLength = lastIndex == -1 ? fileName.Length - _nameIndexStart : lastIndex - _nameIndexStart;
        }
        else
        {
            int lastDot = fileName.LastIndexOf('.');
            _nameIndexStart = lastIndex + 1;
            _nameLength = lastDot == -1 ? fileName.Length - _nameIndexStart : lastDot - _nameIndexStart;
        }

        DatTokenizer tokenizer = new DatTokenizer(text, diagMessages);
        DatTokenizer original = tokenizer;
        int dictionaryDepth = 0, listDepth = 0;
        bool isInMetadata = false, isExplicitlyInAsset = false, ignoreExplicitAsset = false, hasAssets = false;
        bool hasMetadata = false, ignoreMetadata = false;
        ReadOnlySpan<char> metaKey = "Metadata".AsSpan();
        ReadOnlySpan<char> assetKey = "Asset".AsSpan();
        ReadOnlySpan<char> guidKey = "GUID".AsSpan();
        ReadOnlySpan<char> typeKey = "Type".AsSpan();
        ReadOnlySpan<char> idKey = "ID".AsSpan();
        ReadOnlySpan<char> assetCategoryKey = "AssetCategory".AsSpan();

        NextValueType nextValueType = NextValueType.None;

        bool hasGuid = false, hasId = false, hasType = false, hasTypeInMetadata = false, hasAssetCategory = false;

        bool isErrored = false;

        int overrideAssetCategory = -1;

        while (tokenizer.MoveNext())
        {
            switch (tokenizer.Token.Type)
            {
                case DatTokenType.Key:

                    ReadOnlySpan<char> content = tokenizer.Token.Content;
                    if (dictionaryDepth == 0 && listDepth == 0 && !ignoreMetadata && content.Equals(metaKey, StringComparison.OrdinalIgnoreCase))
                    {
                        nextValueType = NextValueType.Metadata;
                    }
                    else if (dictionaryDepth == 0 && listDepth == 0 && !ignoreExplicitAsset && content.Equals(assetKey, StringComparison.OrdinalIgnoreCase))
                    {
                        nextValueType = NextValueType.Asset;
                    }
                    else if (!hasType && (!hasMetadata && dictionaryDepth == 0 || isInMetadata && dictionaryDepth == 1)
                                      && (isInMetadata || !hasTypeInMetadata)
                                      && listDepth == 0
                                      && content.Equals(typeKey, StringComparison.OrdinalIgnoreCase))
                    {
                        nextValueType = NextValueType.Type;
                    }
                    else if (!hasGuid && (!hasMetadata && dictionaryDepth == 0 || isInMetadata && dictionaryDepth == 1) && listDepth == 0 && content.Equals(guidKey, StringComparison.OrdinalIgnoreCase))
                    {
                        nextValueType = NextValueType.Guid;
                    }
                    else if (!hasId && dictionaryDepth == (isExplicitlyInAsset ? 1 : 0) && listDepth == 0 && content.Equals(idKey, StringComparison.OrdinalIgnoreCase))
                    {
                        nextValueType = NextValueType.Id;
                    }
                    else if (!hasAssetCategory && dictionaryDepth == (isExplicitlyInAsset ? 1 : 0) && listDepth == 0 && content.Equals(assetCategoryKey, StringComparison.OrdinalIgnoreCase))
                    {
                        nextValueType = NextValueType.AssetCategory;
                    }
                    else
                    {
                        nextValueType = NextValueType.None;
                    }

                    break;

                case DatTokenType.Value:
                    content = tokenizer.Token.Content;
                    switch (nextValueType)
                    {
                        case NextValueType.Metadata:
                            isInMetadata = false;
                            hasMetadata = false;
                            ignoreMetadata = true;
                            log?.Invoke(fileName, "Unexpected value for \"Metadata\" tag.");
                            isErrored = true;
                            break;

                        case NextValueType.Asset:
                            isExplicitlyInAsset = false;
                            hasAssets = false;
                            ignoreExplicitAsset = true;
                            log?.Invoke(fileName, "Unexpected value for \"Asset\" tag.");
                            isErrored = true;
                            break;

                        case NextValueType.Guid:
                            if (KnownTypeValueHelper.TryParseGuid(content.ToString(), out Guid guid))
                            {
                                Guid = guid;
                            }
                            else
                            {
                                log?.Invoke(fileName, "Can't parse \"Guid\" tag.");
                                isErrored = true;
                            }

                            hasGuid = true;
                            break;

                        case NextValueType.Id:
                            if (KnownTypeValueHelper.TryParseUInt16(content.ToString(), out ushort id))
                            {
                                Id = id;
                            }
                            else
                            {
                                log?.Invoke(fileName, "Can't parse \"ID\" tag, defaulting to 0.");
                                Id = 0;
                            }

                            hasId = true;
                            break;

                        case NextValueType.Type:
                            if (isInMetadata)
                            {
                                hasTypeInMetadata = true;
                                if (KnownTypeValueHelper.TryParseType(content, out QualifiedType type))
                                {
                                    Type = type;
                                }
                                else
                                {
                                    log?.Invoke(fileName, "Can't parse \"Type\" tag.");
                                    isErrored = true;
                                }
                            }
                            else
                            {
                                string str = content.ToString();
                                if (database.Information.AssetAliases.TryGetValue(str, out QualifiedType aliasedType))
                                {
                                    Type = aliasedType;
                                }
                                else if (KnownTypeValueHelper.TryParseType(content, out QualifiedType type))
                                {
                                    Type = type;
                                }
                                else
                                {
                                    log?.Invoke(fileName, "Can't parse \"Type\" tag.");
                                    isErrored = true;
                                }
                            }

                            hasType = true;
                            break;

                        case NextValueType.AssetCategory:
                            if (AssetCategory.TryParse(content.ToString(), out EnumSpecTypeValue categoryEnum))
                            {
                                overrideAssetCategory = categoryEnum.Index;
                            }
                            else
                            {
                                log?.Invoke(fileName, "Can't parse \"AssetCategory\" tag.");
                            }

                            hasAssetCategory = true;
                            break;
                    }

                    nextValueType = NextValueType.None;
                    break;

                case DatTokenType.ListValue:
                    nextValueType = NextValueType.None;
                    break;

                case DatTokenType.DictionaryStart:
                    ++dictionaryDepth;
                    if (dictionaryDepth != 1 || listDepth != 0)
                    {
                        nextValueType = NextValueType.None;
                        break;
                    }

                    switch (nextValueType)
                    {
                        case NextValueType.Metadata when !hasMetadata:
                            isInMetadata = true;
                            hasMetadata = true;
                            hasType = false;
                            hasGuid = false;
                            break;

                        case NextValueType.Asset when !hasAssets:
                            isExplicitlyInAsset = true;
                            hasAssets = true;
                            hasId = false;
                            break;

                        case NextValueType.Guid:
                            hasGuid = true;
                            break;

                        case NextValueType.Id:
                            hasId = true;
                            break;

                        case NextValueType.Type:
                            hasType = true;
                            break;

                        case NextValueType.AssetCategory:
                            hasAssetCategory = true;
                            break;
                    }

                    nextValueType = NextValueType.None;
                    break;

                case DatTokenType.DictionaryEnd:
                    if (dictionaryDepth == 1)
                    {
                        isInMetadata = false;
                        isExplicitlyInAsset = false;
                    }

                    nextValueType = NextValueType.None;
                    --dictionaryDepth;
                    break;

                case DatTokenType.ListStart:
                    ++listDepth;

                    if (dictionaryDepth != 0 || listDepth != 1)
                    {
                        nextValueType = NextValueType.None;
                        break;
                    }

                    switch (nextValueType)
                    {
                        case NextValueType.Metadata when !hasMetadata:
                            hasMetadata = true;
                            ignoreMetadata = true;
                            break;

                        case NextValueType.Asset:
                            hasAssets = true;
                            ignoreExplicitAsset = true;
                            break;

                        case NextValueType.Guid:
                            hasGuid = true;
                            break;

                        case NextValueType.Id:
                            hasId = true;
                            break;

                        case NextValueType.Type:
                            hasType = true;
                            break;

                        case NextValueType.AssetCategory:
                            hasAssetCategory = true;
                            break;
                    }

                    nextValueType = NextValueType.None;
                    break;

                case DatTokenType.ListEnd:
                    --listDepth;
                    nextValueType = NextValueType.None;
                    break;

            }

            if (isInMetadata && dictionaryDepth <= 0)
            {
                isInMetadata = false;
            }
            if (isExplicitlyInAsset && dictionaryDepth <= 0)
            {
                isExplicitlyInAsset = false;
            }
        }

        if (isErrored)
        {
            throw new FormatException("Failed to parse file.");
        }

        if (!hasGuid)
        {
            throw new FormatException("Missing GUID.");
        }

        if (!hasType)
        {
            throw new FormatException("Missing Type.");
        }

        int c = AssetCategory.GetCategoryFromType(Type, database);
        if (c == -1 && hasAssetCategory)
        {
            if (overrideAssetCategory == -1)
            {
                throw new FormatException("Failed to parse file.");
            }

            Category = overrideAssetCategory;
        }
        else
        {
            Category = c;
        }

        if (Type.Equals("SDG.Unturned.ItemGunAsset, Assembly-CSharp"))
        {
            DatTokenizer t = original;
            ParseGunCalibers(ref t);
        }
        else if (database.Information.IsAssignableFrom(new QualifiedType("SDG.Unturned.ItemCaliberAsset, Assembly-CSharp", true), Type))
        {
            DatTokenizer t = original;
            ParseAttachmentCalibers(ref t);
        }
        else
        {
            Calibers = OneOrMore<ushort>.Null;
            MagazineCalibers = OneOrMore<ushort>.Null;
        }

        if (database.Information.IsAssignableFrom(new QualifiedType("SDG.Unturned.ItemWeaponAsset, Assembly-CSharp", true), Type))
        {
            ParseWeaponBladeIds(ref original);
        }
        else if (database.Information.IsAssignableFrom(new QualifiedType("SDG.Unturned.ObjectAsset, Assembly-CSharp", true), Type)
                 && !Type.Equals("SDG.Unturned.ObjectNPCAsset, Assembly-CSharp"))
        {
            ParseObjectBladeIds(ref original);
        }
        else if (database.Information.IsAssignableFrom(new QualifiedType("SDG.Unturned.ResourceAsset, Assembly-CSharp", true), Type))
        {
            ParseResourceBladeIds(ref original);
        }
        else
        {
            BladeIds = OneOrMore<byte>.Null;
        }

        // build fallback localization string.
        Span<char> fullPath = stackalloc char[lastIndex + 12];

        fileName.AsSpan(0, lastIndex + 1).CopyTo(fullPath);
        "English.dat".AsSpan().CopyTo(fullPath.Slice(lastIndex + 1));

        LocalizationFilePath = fullPath.ToString();

        UpdateLocalizationFile();
    }

    private void ParseGunCalibers(ref DatTokenizer tokenizer)
    {
        ushort[]? magCalibers = null;
        List<ushort>? magCalibersList = null;
        HashSet<int> magCaliberIndices = new HashSet<int>();

        ushort[]? attachmentCalibers = null;
        List<ushort>? attachmentCalibersList = null;
        HashSet<int> attCaliberIndices = new HashSet<int>();

        ReadOnlySpan<char> assetKey = "Asset".AsSpan();
        ReadOnlySpan<char> caliberKey = "Caliber".AsSpan();
        ReadOnlySpan<char> attachmentCalibersKey = "Attachment_Calibers".AsSpan();
        ReadOnlySpan<char> attachmentCaliberKey = "Attachment_Caliber_".AsSpan();
        ReadOnlySpan<char> magazineCalibersKey = "Magazine_Calibers".AsSpan();
        ReadOnlySpan<char> magazineCaliberKey = "Magazine_Caliber_".AsSpan();

        bool hasNewCalibers = false, hasAsset = false, hasAttachmentCalibers = false, hasLegacyCaliber = false, hasMagazineCalibers = false;

        bool isInAsset = false;

        ushort legacyCaliber = 0;

        NextCaliberType next = NextCaliberType.None;

        int caliberIndex = -1;

        int dictionaryDepth = 0, listDepth = 0;
        while (tokenizer.MoveNext())
        {
            switch (tokenizer.Token.Type)
            {
                case DatTokenType.Key:
                    if (dictionaryDepth == 0 && !hasAsset && tokenizer.Token.Content.Equals(assetKey, StringComparison.OrdinalIgnoreCase))
                    {
                        hasNewCalibers = false;
                        hasAsset = true;
                        hasAttachmentCalibers = false;
                        hasLegacyCaliber = false;
                        hasMagazineCalibers = false;
                        magCalibers = null;
                        magCalibersList = null;
                        attachmentCalibers = null;
                        attachmentCalibersList = null;
                        isInAsset = true;
                        magCaliberIndices.Clear();
                        attCaliberIndices.Clear();
                        next = NextCaliberType.None;
                        continue;
                    }

                    if (dictionaryDepth != (isInAsset ? 1 : 0) || listDepth != 0 || hasAsset && !isInAsset)
                    {
                        next = NextCaliberType.None;
                        break;
                    }

                    if (!hasMagazineCalibers && tokenizer.Token.Content.Equals(magazineCalibersKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextCaliberType.MagazineCalibers;
                    }
                    else if (tokenizer.Token.Content.StartsWith(magazineCaliberKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextCaliberType.MagazineCaliber;
                        if (!int.TryParse(tokenizer.Token.Content.Slice(magazineCaliberKey.Length).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out caliberIndex))
                            caliberIndex = -1;
                        if (caliberIndex < 0 || !magCaliberIndices.Add(caliberIndex))
                            next = NextCaliberType.None;
                    }
                    else if (!hasAttachmentCalibers && tokenizer.Token.Content.Equals(attachmentCalibersKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextCaliberType.AttachmentCalibers;
                    }
                    else if (tokenizer.Token.Content.StartsWith(attachmentCaliberKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextCaliberType.AttachmentCaliber;
                        if (!int.TryParse(tokenizer.Token.Content.Slice(attachmentCalibersKey.Length).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out caliberIndex))
                            caliberIndex = -1;
                        if (caliberIndex < 0 || !attCaliberIndices.Add(caliberIndex))
                            next = NextCaliberType.None;
                    }
                    else if (!hasNewCalibers && !hasLegacyCaliber && tokenizer.Token.Content.Equals(caliberKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextCaliberType.LegacyCaliber;
                    }
                    else
                    {
                        next = NextCaliberType.None;
                    }

                    break;

                case DatTokenType.Value:
                    if (dictionaryDepth == 0)
                    {
                        isInAsset = false;
                    }

                    switch (next)
                    {
                        case NextCaliberType.MagazineCalibers:
                            if (int.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int magCals)
                                && magCals > 0)
                            {
                                hasNewCalibers = true;
                                magCalibers = new ushort[magCals];
                                if (magCalibersList != null)
                                {
                                    magCalibersList.CopyTo(0, magCalibers, 0, Math.Min(magCalibersList.Count, magCals));
                                    magCalibersList = null;
                                }
                            }

                            hasMagazineCalibers = true;
                            break;

                        case NextCaliberType.AttachmentCalibers:
                            if (int.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int attCals)
                                && attCals > 0)
                            {
                                hasNewCalibers = true;
                                attachmentCalibers = new ushort[attCals];
                                if (attachmentCalibersList != null)
                                {
                                    attachmentCalibersList.CopyTo(0, attachmentCalibers, 0, Math.Min(attachmentCalibersList.Count, attCals));
                                    attachmentCalibersList = null;
                                }
                            }

                            hasAttachmentCalibers = true;
                            break;

                        case NextCaliberType.MagazineCaliber:
                        case NextCaliberType.AttachmentCaliber:
                            ushort[]? arr = next == NextCaliberType.MagazineCaliber ? magCalibers : attachmentCalibers;
                            scoped ref List<ushort>? list = ref magCalibersList;
                            if (next == NextCaliberType.AttachmentCaliber)
                                list = ref attachmentCalibersList;

                            if (ushort.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out ushort caliber))
                            {
                                if (arr == null)
                                {
                                    list ??= new List<ushort>(4);
                                    if (list.Capacity <= caliberIndex)
                                        list.Capacity = caliberIndex + 1;
                                    while (list.Count <= caliberIndex)
                                        list.Add(0);
                                    list[caliberIndex] = caliber;
                                }
                                else if (caliberIndex < arr.Length)
                                {
                                    arr[caliberIndex] = caliber;
                                }
                            }
                            break;

                        case NextCaliberType.LegacyCaliber:
                            hasLegacyCaliber = true;
                            ushort.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out legacyCaliber);
                            break;
                    }
                    break;

                case DatTokenType.DictionaryStart:
                    ++dictionaryDepth;
                    switch (next)
                    {
                        case NextCaliberType.MagazineCalibers:
                            hasMagazineCalibers = true;
                            break;

                        case NextCaliberType.AttachmentCalibers:
                            hasAttachmentCalibers = true;
                            break;

                        case NextCaliberType.LegacyCaliber:
                            hasLegacyCaliber = true;
                            break;
                    }
                    break;

                case DatTokenType.DictionaryEnd:
                    if (dictionaryDepth == 1)
                    {
                        isInAsset = false;
                    }
                    --dictionaryDepth;
                    break;

                case DatTokenType.ListStart:
                    if (dictionaryDepth == 0)
                    {
                        isInAsset = false;
                    }
                    switch (next)
                    {
                        case NextCaliberType.MagazineCalibers:
                            hasMagazineCalibers = true;
                            break;

                        case NextCaliberType.AttachmentCalibers:
                            hasAttachmentCalibers = true;
                            break;

                        case NextCaliberType.LegacyCaliber:
                            hasLegacyCaliber = true;
                            break;
                    }
                    ++listDepth;
                    break;

                case DatTokenType.ListEnd:
                    --listDepth;
                    break;
            }
        }

        if (magCalibers != null)
        {
            MagazineCalibers = new OneOrMore<ushort>(magCalibers);
        }
        else
        {
            MagazineCalibers = new OneOrMore<ushort>(legacyCaliber);
        }

        if (attachmentCalibers == null)
        {
            Calibers = MagazineCalibers;
        }
        else
        {
            Calibers = new OneOrMore<ushort>(attachmentCalibers);
        }
    }

    private void ParseAttachmentCalibers(ref DatTokenizer tokenizer)
    {
        ushort[]? calibers = null;
        List<ushort>? calibersList = null;
        HashSet<int> caliberIndices = new HashSet<int>();

        ReadOnlySpan<char> assetKey = "Asset".AsSpan();
        ReadOnlySpan<char> calibersKey = "Calibers".AsSpan();
        ReadOnlySpan<char> caliberKey = "Caliber_".AsSpan();

        bool hasCalibers = false, hasAsset = false;

        bool isInAsset = false;

        NextCaliberType next = NextCaliberType.None;

        int caliberIndex = -1;

        int dictionaryDepth = 0, listDepth = 0;
        while (tokenizer.MoveNext())
        {
            switch (tokenizer.Token.Type)
            {
                case DatTokenType.Key:
                    if (dictionaryDepth == 0 && !hasAsset && tokenizer.Token.Content.Equals(assetKey, StringComparison.OrdinalIgnoreCase))
                    {
                        hasCalibers = false;
                        hasAsset = true;
                        calibers = null;
                        calibersList = null;
                        isInAsset = true;
                        caliberIndices.Clear();
                        next = NextCaliberType.None;
                        continue;
                    }

                    if (dictionaryDepth != (isInAsset ? 1 : 0) || listDepth != 0 || hasAsset && !isInAsset)
                    {
                        next = NextCaliberType.None;
                        break;
                    }

                    if (!hasCalibers && tokenizer.Token.Content.Equals(calibersKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextCaliberType.AttachmentCalibers;
                    }
                    else if (tokenizer.Token.Content.StartsWith(caliberKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextCaliberType.AttachmentCaliber;
                        if (!int.TryParse(tokenizer.Token.Content.Slice(caliberKey.Length).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out caliberIndex))
                            caliberIndex = -1;
                        if (caliberIndex < 0 || !caliberIndices.Add(caliberIndex))
                            next = NextCaliberType.None;
                    }
                    else
                    {
                        next = NextCaliberType.None;
                    }

                    break;

                case DatTokenType.Value:
                    if (dictionaryDepth == 0)
                    {
                        isInAsset = false;
                    }

                    switch (next)
                    {
                        case NextCaliberType.AttachmentCalibers:
                            if (byte.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out byte attCals))
                            {
                                calibers = new ushort[attCals];
                                if (calibersList != null)
                                {
                                    calibersList.CopyTo(0, calibers, 0, Math.Min(calibersList.Count, attCals));
                                    calibersList = null;
                                }
                            }

                            hasCalibers = true;
                            break;

                        case NextCaliberType.AttachmentCaliber:
                            if (ushort.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out ushort caliber))
                            {
                                if (calibers == null)
                                {
                                    calibersList ??= new List<ushort>(4);
                                    if (calibersList.Capacity <= caliberIndex)
                                        calibersList.Capacity = caliberIndex + 1;
                                    while (calibersList.Count <= caliberIndex)
                                        calibersList.Add(0);
                                    calibersList[caliberIndex] = caliber;
                                }
                                else if (caliberIndex < calibers.Length)
                                {
                                    calibers[caliberIndex] = caliber;
                                }
                            }
                            break;
                    }
                    break;

                case DatTokenType.DictionaryStart:
                    ++dictionaryDepth;
                    switch (next)
                    {
                        case NextCaliberType.AttachmentCalibers:
                            hasCalibers = true;
                            break;
                    }
                    break;

                case DatTokenType.DictionaryEnd:
                    if (dictionaryDepth == 1)
                    {
                        isInAsset = false;
                    }
                    --dictionaryDepth;
                    break;

                case DatTokenType.ListStart:
                    if (dictionaryDepth == 0)
                    {
                        isInAsset = false;
                    }
                    switch (next)
                    {
                        case NextCaliberType.AttachmentCalibers:
                            hasCalibers = true;
                            break;
                    }
                    ++listDepth;
                    break;

                case DatTokenType.ListEnd:
                    --listDepth;
                    break;
            }
        }

        Calibers = calibers != null ? new OneOrMore<ushort>(calibers) : OneOrMore<ushort>.Null;
        MagazineCalibers = Calibers;
    }

    private void ParseObjectBladeIds(ref DatTokenizer tokenizer)
    {
        ReadOnlySpan<char> assetKey = "Asset".AsSpan();
        ReadOnlySpan<char> interactabilityKey = "Interactability".AsSpan();
        ReadOnlySpan<char> interactabilityBladeIdKey = "Interactability_Blade_ID".AsSpan();
        ReadOnlySpan<char> rubbleKey = "Rubble".AsSpan();
        ReadOnlySpan<char> rubbleBladeIdkey = "Rubble_Blade_ID".AsSpan();

        bool hasRubble = false, hasAsset = false, hasInteractability = false, hasInteractabilityBladeId = false, hasRubbleBladeId = false;

        bool isInAsset = false;

        NextBladeType next = NextBladeType.None;

        byte rubbleBladeId = 0, interactabilityBladeId = 0;

        bool isInteractabilityRubble = false;
        bool isBladeInteractable = false;

        int dictionaryDepth = 0, listDepth = 0;
        while (tokenizer.MoveNext())
        {
            switch (tokenizer.Token.Type)
            {
                case DatTokenType.Key:
                    if (dictionaryDepth == 0 && !hasAsset && tokenizer.Token.Content.Equals(assetKey, StringComparison.OrdinalIgnoreCase))
                    {
                        hasRubble = false;
                        hasAsset = true;
                        hasInteractability = false;
                        isInteractabilityRubble = false;
                        hasInteractabilityBladeId = false;
                        hasRubbleBladeId = false;
                        isBladeInteractable = false;
                        rubbleBladeId = 0;
                        interactabilityBladeId = 0;
                        isInAsset = true;
                        next = NextBladeType.None;
                        continue;
                    }

                    if (dictionaryDepth != (isInAsset ? 1 : 0) || listDepth != 0 || hasAsset && !isInAsset)
                    {
                        next = NextBladeType.None;
                        break;
                    }

                    if (!hasRubble && tokenizer.Token.Content.Equals(rubbleKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextBladeType.Rubble;
                    }
                    else if (!hasInteractability && tokenizer.Token.Content.Equals(interactabilityKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextBladeType.Interactability;
                    }
                    else if (!hasInteractabilityBladeId && tokenizer.Token.Content.Equals(interactabilityBladeIdKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextBladeType.BladeId;
                        isBladeInteractable = true;
                    }
                    else if (!hasRubbleBladeId && tokenizer.Token.Content.Equals(rubbleBladeIdkey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextBladeType.BladeId;
                        isBladeInteractable = false;
                    }
                    else
                    {
                        next = NextBladeType.None;
                    }

                    break;

                case DatTokenType.Value:
                    if (dictionaryDepth == 0)
                    {
                        isInAsset = false;
                    }

                    switch (next)
                    {
                        case NextBladeType.Interactability:
                            hasInteractability = true;
                            isInteractabilityRubble = tokenizer.Token.Content.Equals("RUBBLE", StringComparison.OrdinalIgnoreCase);
                            if (isInteractabilityRubble)
                            {
                                rubbleBladeId = 0;
                            }
                            break;

                        case NextBladeType.Rubble:
                            hasRubble = true;
                            break;

                        case NextBladeType.BladeId when isBladeInteractable:
                            if (byte.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out byte id))
                            {
                                interactabilityBladeId = id;
                            }
                            hasInteractabilityBladeId = true;
                            break;

                        case NextBladeType.BladeId:
                            if (byte.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out id))
                            {
                                rubbleBladeId = id;
                            }
                            hasRubbleBladeId = true;
                            break;
                    }
                    break;

                case DatTokenType.DictionaryStart:
                    ++dictionaryDepth;
                    switch (next)
                    {
                        case NextBladeType.Rubble:
                            hasRubble = true;
                            break;

                        case NextBladeType.Interactability:
                            hasInteractability = true;
                            isInteractabilityRubble = false;
                            break;

                        case NextBladeType.BladeId:
                            if (isBladeInteractable)
                                hasInteractabilityBladeId = true;
                            else
                                hasRubbleBladeId = true;
                            break;
                    }
                    break;

                case DatTokenType.DictionaryEnd:
                    if (dictionaryDepth == 1)
                    {
                        isInAsset = false;
                    }
                    --dictionaryDepth;
                    break;

                case DatTokenType.ListStart:
                    if (dictionaryDepth == 0)
                    {
                        isInAsset = false;
                    }
                    switch (next)
                    {
                        case NextBladeType.Rubble:
                            hasRubble = true;
                            break;

                        case NextBladeType.Interactability:
                            hasInteractability = true;
                            isInteractabilityRubble = false;
                            break;

                        case NextBladeType.BladeId:
                            if (isBladeInteractable)
                                hasInteractabilityBladeId = true;
                            else
                                hasRubbleBladeId = true;
                            break;
                    }
                    ++listDepth;
                    break;

                case DatTokenType.ListEnd:
                    --listDepth;
                    break;
            }
            if (isInteractabilityRubble && interactabilityBladeId != 0 && hasAsset)
                break;
        }

        if (isInteractabilityRubble)
        {
            BladeIds = new OneOrMore<byte>(interactabilityBladeId);
        }
        else if (hasRubble)
        {
            BladeIds = new OneOrMore<byte>(rubbleBladeId);
        }
        else
        {
            BladeIds = OneOrMore<byte>.Null;
        }
    }

    private void ParseResourceBladeIds(ref DatTokenizer tokenizer)
    {
        ReadOnlySpan<char> assetKey = "Asset".AsSpan();
        ReadOnlySpan<char> bladeIdKey = "BladeID".AsSpan();

        bool hasBladeId = false, hasAsset = false;

        bool isInAsset = false;

        NextBladeType next = NextBladeType.None;

        byte bladeId = 0;

        int dictionaryDepth = 0, listDepth = 0;
        while (tokenizer.MoveNext())
        {
            switch (tokenizer.Token.Type)
            {
                case DatTokenType.Key:
                    if (dictionaryDepth == 0 && !hasAsset && tokenizer.Token.Content.Equals(assetKey, StringComparison.OrdinalIgnoreCase))
                    {
                        hasAsset = true;
                        hasBladeId = false;
                        isInAsset = true;
                        bladeId = 0;
                        next = NextBladeType.None;
                        continue;
                    }

                    if (dictionaryDepth != (isInAsset ? 1 : 0) || listDepth != 0 || hasAsset && !isInAsset)
                    {
                        next = NextBladeType.None;
                        break;
                    }

                    if (!hasBladeId && tokenizer.Token.Content.Equals(bladeIdKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextBladeType.BladeId;
                    }
                    else
                    {
                        next = NextBladeType.None;
                    }

                    break;

                case DatTokenType.Value:
                    if (dictionaryDepth == 0)
                    {
                        isInAsset = false;
                    }

                    if (next == NextBladeType.BladeId)
                    {
                        byte.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out bladeId);
                        hasBladeId = true;
                    }
                    break;

                case DatTokenType.DictionaryStart:
                    ++dictionaryDepth;
                    if (next == NextBladeType.BladeId)
                    {
                        hasBladeId = true;
                    }
                    break;

                case DatTokenType.DictionaryEnd:
                    if (dictionaryDepth == 1)
                    {
                        isInAsset = false;
                    }
                    --dictionaryDepth;
                    break;

                case DatTokenType.ListStart:
                    if (dictionaryDepth == 0)
                    {
                        isInAsset = false;
                    }
                    if (next == NextBladeType.BladeId)
                    {
                        hasBladeId = true;
                    }
                    ++listDepth;
                    break;

                case DatTokenType.ListEnd:
                    --listDepth;
                    break;
            }

            if (hasBladeId && hasAsset)
            {
                break;
            }
        }

        BladeIds = new OneOrMore<byte>(bladeId);
    }

    private void ParseWeaponBladeIds(ref DatTokenizer tokenizer)
    {
        byte[]? bladeIds = null;
        List<byte>? bladeIdsList = null;
        HashSet<int> bladeIdsIndices = new HashSet<int>();

        ReadOnlySpan<char> assetKey = "Asset".AsSpan();
        ReadOnlySpan<char> bladeIdKey = "BladeID".AsSpan();
        ReadOnlySpan<char> bladeIdsKey = "BladeIDs".AsSpan();
        ReadOnlySpan<char> bladeIdPrefixKey = "BladeID_".AsSpan();

        bool hasBladeId = false, hasBladeIds = false, hasAsset = false;

        bool isInAsset = false;

        NextBladeType next = NextBladeType.None;

        byte bladeId = 0;

        int nextBladeIdIndex = -1;

        int bladeIdsLength = -1;

        int dictionaryDepth = 0, listDepth = 0;
        while (tokenizer.MoveNext())
        {
            switch (tokenizer.Token.Type)
            {
                case DatTokenType.Key:
                    if (dictionaryDepth == 0 && !hasAsset && tokenizer.Token.Content.Equals(assetKey, StringComparison.OrdinalIgnoreCase))
                    {
                        bladeIds = null;
                        bladeIdsList = null;
                        hasAsset = true;
                        hasBladeId = false;
                        hasBladeIds = false;
                        isInAsset = true;
                        bladeId = 0;
                        next = NextBladeType.None;
                        bladeIdsIndices.Clear();
                        continue;
                    }

                    if (dictionaryDepth != (isInAsset ? 1 : 0) || listDepth != 0 || hasAsset && !isInAsset)
                    {
                        next = NextBladeType.None;
                        break;
                    }

                    if (!hasBladeId && (!hasBladeIds || bladeIdsLength == 0) && tokenizer.Token.Content.Equals(bladeIdKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextBladeType.BladeId;
                        nextBladeIdIndex = -1;
                    }
                    else if (!hasBladeIds && tokenizer.Token.Content.Equals(bladeIdsKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextBladeType.BladeIds;
                    }
                    else if ((!hasBladeIds || bladeIdsLength > 0) && tokenizer.Token.Content.StartsWith(bladeIdPrefixKey, StringComparison.OrdinalIgnoreCase))
                    {
                        next = NextBladeType.BladeId;
                        if (int.TryParse(tokenizer.Token.Content.Slice(bladeIdPrefixKey.Length).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int index)
                            && index >= 0
                            && (!hasBladeIds || index < bladeIdsLength)
                            && bladeIdsIndices.Add(index))
                        {
                            nextBladeIdIndex = index;
                        }
                        else
                        {
                            next = NextBladeType.None;
                        }
                    }
                    else
                    {
                        next = NextBladeType.None;
                    }

                    break;

                case DatTokenType.Value:
                    if (dictionaryDepth == 0)
                    {
                        isInAsset = false;
                    }

                    switch (next)
                    {
                        case NextBladeType.BladeIds:
                            if (int.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int ct))
                            {
                                bladeIdsLength = ct < 0 ? 0 : ct;
                                if (bladeIdsLength > 0 && hasBladeId)
                                {
                                    bladeId = 0;
                                }
                                bladeIds = new byte[ct];
                                if (bladeIdsList != null)
                                {
                                    bladeIdsList.CopyTo(0, bladeIds, 0, Math.Min(bladeIdsList.Count, ct));
                                    bladeIdsList = null;
                                }
                            }

                            hasBladeIds = true;
                            break;

                        case NextBladeType.BladeId:
                            if (byte.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out byte id))
                            {
                                if (nextBladeIdIndex == -1)
                                {
                                    bladeId = id;
                                }
                                else if (bladeIds == null)
                                {
                                    bladeIdsList ??= new List<byte>(4);
                                    if (bladeIdsList.Capacity <= nextBladeIdIndex)
                                        bladeIdsList.Capacity = nextBladeIdIndex + 1;
                                    while (bladeIdsList.Count <= nextBladeIdIndex)
                                        bladeIdsList.Add(0);
                                    bladeIdsList[nextBladeIdIndex] = id;
                                }
                                else
                                {
                                    bladeIds[nextBladeIdIndex] = id;
                                }
                            }

                            if (nextBladeIdIndex == -1)
                                hasBladeId = true;
                            break;
                    }
                    break;

                case DatTokenType.DictionaryStart:
                    ++dictionaryDepth;
                    switch (next)
                    {
                        case NextBladeType.BladeIds:
                            hasBladeIds = true;
                            break;
                        case NextBladeType.BladeId when nextBladeIdIndex == -1:
                            hasBladeId = true;
                            break;
                    }
                    break;

                case DatTokenType.DictionaryEnd:
                    if (dictionaryDepth == 1)
                    {
                        isInAsset = false;
                    }
                    --dictionaryDepth;
                    break;

                case DatTokenType.ListStart:
                    if (dictionaryDepth == 0)
                    {
                        isInAsset = false;
                    }
                    switch (next)
                    {
                        case NextBladeType.BladeIds:
                            hasBladeIds = true;
                            break;
                        case NextBladeType.BladeId when nextBladeIdIndex == -1:
                            hasBladeId = true;
                            break;
                    }
                    ++listDepth;
                    break;

                case DatTokenType.ListEnd:
                    --listDepth;
                    break;
            }
        }

        if (!hasBladeIds || bladeIdsLength == 0)
        {
            BladeIds = new OneOrMore<byte>(bladeId);
        }
        else if (bladeIds != null)
        {
            BladeIds = new OneOrMore<byte>(bladeIds);
        }
        else
        {
            BladeIds = default; // 0, not null
        }
    }

    internal void UpdateLocalizationFile()
    {
        FriendlyName = null;
        if (!File.Exists(LocalizationFilePath))
            return;

        if (!AssetCategory.HasFriendlyName(Category))
            return;

        string localizationFileContents;
        using (FileStream fs = new FileStream(LocalizationFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 256, FileOptions.SequentialScan))
        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, true, 256, true))
        {
            localizationFileContents = sr.ReadToEnd();
        }

        if (string.IsNullOrEmpty(localizationFileContents))
        {
            LocalizationFilePath = null;
            return;
        }

        DatTokenizer localTokenizer = new DatTokenizer(localizationFileContents.AsSpan(), null);
        int dictionaryDepth = 0, listDepth = 0;
        ReadOnlySpan<char> nameKey = "Name".AsSpan();
        bool nextIsName = false;
        while (localTokenizer.MoveNext() && FriendlyName == null)
        {
            switch (localTokenizer.Token.Type)
            {
                case DatTokenType.Key:
                    nextIsName = dictionaryDepth == 0 && listDepth == 0 && localTokenizer.Token.Content.Equals(nameKey, StringComparison.Ordinal);
                    break;

                case DatTokenType.Value:
                    if (nextIsName)
                    {
                        FriendlyName = localTokenizer.Token.Content.ToString();
                    }

                    nextIsName = false;
                    break;

                case DatTokenType.DictionaryStart:
                    nextIsName = false;
                    ++dictionaryDepth;
                    break;
                case DatTokenType.DictionaryEnd:
                    --dictionaryDepth;
                    break;
                case DatTokenType.ListStart:
                    nextIsName = false;
                    ++listDepth;
                    break;
                case DatTokenType.ListEnd:
                    --listDepth;
                    break;
                case DatTokenType.ListValue:
                    nextIsName = false;
                    break;
            }
        }
    }

    /// <inheritdoc />
    public bool Equals(DiscoveredDatFile other) => other != null && string.Equals(other.FilePath, FilePath, StringComparison.Ordinal);


    private sealed class IdComparer : IComparer<DiscoveredDatFile>
    {
        public static readonly IdComparer Instance = new IdComparer();

        static IdComparer() { }
        private IdComparer() { }

        /// <inheritdoc />
        public int Compare(DiscoveredDatFile x, DiscoveredDatFile y)
        {
            if (x.Id == 0)
            {
                return y.Id == 0 ? 0 : -1;
            }

            if (y.Id == 0)
                return 1;

            return x.Id.CompareTo(y.Id);
        }
    }
}
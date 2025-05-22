using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;

public class DiscoveredDatFile : IEquatable<DiscoveredDatFile>
{
    private readonly int _nameIndexStart, _nameLength;
    private string? _name;

    internal DiscoveredDatFile? Next;
    internal DiscoveredDatFile? Prev;

    public bool IsRemoved { get; internal set; }

    public string FilePath { get; }
    public Guid Guid { get; }
    public ushort Id { get; }
    public int Category { get; }

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

        // build fallback localization string.
        Span<char> fullPath = stackalloc char[lastIndex + 12];

        fileName.AsSpan(0, lastIndex + 1).CopyTo(fullPath);
        "English.dat".AsSpan().CopyTo(fullPath.Slice(lastIndex + 1));

        LocalizationFilePath = fullPath.ToString();

        UpdateLocalizationFile();
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
}
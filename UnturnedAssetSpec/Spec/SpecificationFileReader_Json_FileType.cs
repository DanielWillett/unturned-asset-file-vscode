using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Tracing;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

partial class SpecificationFileReader
{
    private JsonElement _typeRoot;
    private int _currentTypeIndex;
    private List<DatFileType>? _currentFiles;

    public DatFileType? GetOrReadFileType(QualifiedType fileType)
    {
        if (!fileType.IsCaseInsensitive)
            fileType = fileType.CaseInsensitive;

        if (_database is { IsInitialized: true })
        {
            if (_database != null && _database.FileTypes.TryGetValue(fileType, out DatFileType? ft))
                return ft;
        }
        else
        {
            if (_fileTypeBuilder != null && _fileTypeBuilder.TryGetValue(fileType, out DatFileType? ft))
                return ft;

            if (_parsedFiles != null && _parsedFiles.Contains(fileType))
                return null;

            ft = _currentFiles?.Find(x => fileType.Equals(x.TypeName));
            if (ft != null)
                return ft;

            if (_assetFiles != null && _assetFiles.TryGetValue(fileType, out JsonDocument? jsonDoc))
            {
                _parsedFiles?.Add(fileType);
                if (_readInformation != null)
                {
                    // read parent types
                    InverseTypeHierarchy hierarchy = _readInformation.GetParentTypes(fileType);
                    for (int i = hierarchy.ParentTypes.Length - 1; i >= 0; --i)
                    {
                        QualifiedType parentType = hierarchy.ParentTypes[i];

                        if (_assetFiles == null || !_assetFiles.TryGetValue(fileType, out JsonDocument? parentJsonDoc))
                            continue;

                        if (_parsedFiles != null && !_parsedFiles.Add(parentType))
                            continue;

                        _ = ReadFileType(parentJsonDoc, parentType);
                    }
                }

                return ReadFileType(jsonDoc, fileType);
            }
        }

        return null;
    }

    private DatFileType ReadFileType(JsonDocument doc, QualifiedType fileType)
    {
        JsonElement root = doc.RootElement;

        // check that type is correct
        if (root.TryGetProperty("Type"u8, out JsonElement element))
        {
            AssertValueKind(in element, fileType, JsonValueKind.String);
            if (!element.ValueEquals(fileType.Type.AsSpan()))
                throw new JsonException(string.Format(Resources.JsonException_InvalidTypeInDatFileType, element.GetString(), fileType.GetFullTypeName()), "Type", null, null);
        }

        // get parent type from JSON
        DatFileType? parentType = null;
        if (root.TryGetProperty("Parent"u8, out element))
        {
            string? parentTypeStr = element.GetString();
            if (parentTypeStr != null)
            {
                QualifiedType qualifiedType = new QualifiedType(parentTypeStr, isCaseInsensitive: true);
                parentType = GetOrReadFileType(qualifiedType);
                if (parentType == null)
                {
                    throw new JsonException(
                        string.Format(
                            Resources.JsonException_ParentTypeNotFound,
                            parentType,
                            fileType.GetFullTypeName()
                        ), "Parent", null, null
                    );
                }
            }
        }

        bool isAsset = _readInformation != null && _readInformation.IsAssignableFrom(QualifiedType.AssetBaseType, fileType.Type);

        DatFileType type = DatType.CreateFileType(fileType, isAsset, root, parentType, this);
        if (_currentFiles == null)
            _currentFiles = new List<DatFileType>(2) { type };
        else
            _currentFiles.Add(type);
        try
        {
            if (_fileTypeBuilder != null)
            {
                if (_fileTypeBuilder.TryGetValue(fileType, out DatFileType existing))
                {
                    _logger.LogError(
                        "Duplicate file type name in specification: {0} and {1}.",
                        existing.Id,
                        type.Id
                    );
                }
                else
                {
                    _fileTypeBuilder.Add(fileType, type);
                }
            }

            if (_allTypeBuilder != null)
            {
                if (_allTypeBuilder.TryGetValue(fileType, out DatType existing))
                {
                    _logger.LogWarning(
                        "Duplicate type name in specification: ({0} from {1}) and ({2}). They're not conflicting since they're defined in different files but may cause issues.",
                        existing.Id,
                        existing.Owner.TypeName,
                        type.Id
                    );
                }
                else
                {
                    _allTypeBuilder.Add(fileType, type);
                }
            }

            if (type is DatAssetFileType assetType)
            {
                // Category
                if (root.TryGetProperty("Category"u8, out element) && element.ValueKind != JsonValueKind.Null)
                    assetType.Category = new AssetCategoryValue(element.GetString()!);
                else
                    assetType.Category = AssetCategoryValue.None;

                // VanillaIdLimit
                if (root.TryGetProperty("VanillaIdLimit"u8, out element) && element.ValueKind != JsonValueKind.Null)
                {
                    ushort id = element.GetUInt16();
                    assetType.VanillaIdLimit = id == 0 ? null : id;
                }
                else
                    assetType.VanillaIdLimit = null;

                // RequireId
                if (root.TryGetProperty("RequireId"u8, out element) && element.ValueKind != JsonValueKind.Null)
                    assetType.RequireId = element.GetBoolean();
            }

            // DisplayName
            if (root.TryGetProperty("DisplayName"u8, out element) && element.ValueKind != JsonValueKind.Null)
            {
                type.DisplayNameIntl = element.GetString()!;
                if (type.DisplayNameIntl.Length == 0)
                    type.DisplayNameIntl = type.TypeName.GetFullTypeName();
            }
            else
            {
                type.DisplayNameIntl = type.TypeName.GetFullTypeName();
            }

            // Docs
            if (root.TryGetProperty("Docs"u8, out element))
                type.Docs = element.GetString();

            // Version
            if (root.TryGetProperty("Version"u8, out element) && element.ValueKind != JsonValueKind.Null)
                type.Version = Version.Parse(element.GetString()!);

            // AutoGeneratedKeys
            if (root.TryGetProperty("AutoGeneratedKeys"u8, out element) && element.ValueKind != JsonValueKind.Null)
                type.AutoGeneratedKeys = element.GetBoolean();

            // OverridableProperties
            if (root.TryGetProperty("OverridableProperties"u8, out element) && element.ValueKind != JsonValueKind.Null)
                type.OverridableProperties = element.GetBoolean();

            if (root.TryGetProperty("Types"u8, out element) && element.ValueKind != JsonValueKind.Null)
            {
                _typeRoot = element;
                AssertValueKind(in element, fileType, JsonValueKind.Array);
                int typeCount = element.GetArrayLength();
                ImmutableDictionary<QualifiedType, DatType>.Builder typeDictionaryBuilder = ImmutableDictionary.CreateBuilder<QualifiedType, DatType>();
                type.TypesBuilder = typeDictionaryBuilder;

                for (int i = 0; i < typeCount; ++i)
                {
                    _currentTypeIndex = i;
                    ReadTypeFirstPass(in element, i, typeDictionaryBuilder, type);
                }

                type.Types = typeDictionaryBuilder.ToImmutable();
                type.TypesBuilder = null;
                _typeRoot = default;
            }

            if (root.TryGetProperty("Properties"u8, out element) && element.ValueKind != JsonValueKind.Null)
            {
                AssertValueKind(in element, fileType, JsonValueKind.Array);
                int propertyCount = element.GetArrayLength();

                ImmutableArray<DatProperty>.Builder propertyBuilder = ImmutableArray.CreateBuilder<DatProperty>(propertyCount);
                type.PropertiesBuilder = propertyBuilder;

                for (int i = 0; i < propertyCount; ++i)
                {
                    JsonElement prop = element[i];
                    ReadPropertyFirstPass(in prop, i, "Properties", t => t.Properties, propertyBuilder, SpecPropertyContext.Property, type);
                }

                type.Properties = propertyBuilder.ToImmutable();
                type.PropertiesBuilder = null;
            }

            if (type is DatAssetFileType assetFile && root.TryGetProperty("Localization"u8, out element) && element.ValueKind != JsonValueKind.Null)
            {
                AssertValueKind(in element, fileType, JsonValueKind.Array);
                int propertyCount = element.GetArrayLength();

                ImmutableArray<DatProperty>.Builder propertyBuilder = ImmutableArray.CreateBuilder<DatProperty>(propertyCount);
                assetFile.LocalizationPropertiesBuilder = propertyBuilder;

                for (int i = 0; i < propertyCount; ++i)
                {
                    JsonElement prop = element[i];
                    ReadPropertyFirstPass(in prop, i, "Localization", t => t.Properties, propertyBuilder, SpecPropertyContext.Localization, type);
                }

                assetFile.LocalizationProperties = propertyBuilder.ToImmutable();
                assetFile.LocalizationPropertiesBuilder = null;
            }
        }
        finally
        {
            _currentFiles.Remove(type);
        }

        return type;
    }

    private static void AssertValueKind(in JsonElement element, QualifiedType type, JsonValueKind kind)
    {
        if (element.ValueKind == kind)
            return;
        
        throw new JsonException(string.Format(Resources.JsonException_InvalidJsonToken, element.ValueKind, kind, type.GetFullTypeName()));
    }
}

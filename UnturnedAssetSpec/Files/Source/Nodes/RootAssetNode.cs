using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Immutable;
using System.IO;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal sealed class RootAssetNode : RootDictionaryNode, IAssetSourceFile
{
    public ImmutableArray<ILocalizationSourceFile> Localization { get; private set; }
    public Guid? Guid { get; private set; }
    public ushort? Id { get; private set; }
    public AssetCategoryValue Category { get; private set; }
    public QualifiedOrAliasedType AssetType { get; private set; }
    public string AssetName { get; private set; }
    public bool IsErrored { get; private set; }

    public new static RootAssetNode Create(IWorkspaceFile file, IAssetSpecDatabase database, int count, ISourceNode[] nodes, in AnySourceNodeProperties properties)
    {
        return new RootAssetNode(file, database, count, nodes, in properties);
    }

    /// <inheritdoc />
    private RootAssetNode(IWorkspaceFile file, IAssetSpecDatabase database, int count, ISourceNode[] nodes, in AnySourceNodeProperties properties)
        : base(file, database, count, nodes, in properties)
    {
        string fileName = file.File;

        LoadMetadata();

        string? dirName = null;
        if (fileName.EndsWith("Asset.dat", StringComparison.OrdinalIgnoreCase))
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            ReadOnlySpan<char> dirNameSpan = Path.GetDirectoryName(fileName.AsSpan());
            AssetName = Path.GetFileName(dirNameSpan).ToString();
#else
            dirName = Path.GetDirectoryName(fileName);
            AssetName = string.IsNullOrEmpty(dirName) ? string.Empty : Path.GetFileName(dirName);
#endif
        }
        else
        {
            AssetName = Path.GetFileNameWithoutExtension(fileName);
        }

        Localization = ImmutableArray<ILocalizationSourceFile>.Empty;
        
        if (Database.Types.TryGetValue(ActualType, out AssetSpecType specType) && specType.LocalizationProperties is not { Length: > 0 })
        {
            return;
        }

        dirName ??= Path.GetDirectoryName(fileName);
        if (string.IsNullOrEmpty(dirName))
            return;

        try
        {
            string[] files = Directory.GetFiles(dirName, "*.dat");
            ImmutableArray<ILocalizationSourceFile>.Builder builder = ImmutableArray.CreateBuilder<ILocalizationSourceFile>(files.Length);
            int englishIndex = -1;
            foreach (string localFile in files)
            {
                if (localFile.Equals(fileName, StringComparison.Ordinal))
                    continue;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                ReadOnlySpan<char> langName = Path.GetFileNameWithoutExtension(localFile.AsSpan());
                if (langName.IsWhiteSpace() || !char.IsUpper(langName[0]))
                    continue;
#else
                string langName = Path.GetFileNameWithoutExtension(localFile);
                if (string.IsNullOrWhiteSpace(langName) || !char.IsUpper(langName[0]))
                    continue;
#endif

                ReferencedWorkspaceFile workspaceFile = new ReferencedWorkspaceFile(localFile, database, this, static (file, state) =>
                {
                    string text;
                    try
                    {
                        text = System.IO.File.ReadAllText(file.File);
                    }
                    catch (SystemException)
                    {
                        return null!;
                    }

                    using SourceNodeTokenizer tokenizer = new SourceNodeTokenizer(
                        text,
                        SourceNodeTokenizerOptions.None
                    );
                    return tokenizer.ReadRootDictionary(SourceNodeTokenizer.RootInfo.Localization(file, file.Database, (IAssetSourceFile)state!));
                });

                if (workspaceFile.SourceFile is not ILocalizationSourceFile local)
                    continue;

                if (local.LanguageName.Equals("English", StringComparison.Ordinal))
                {
                    englishIndex = builder.Count;
                }

                builder.Add(local);
            }

            if (englishIndex > 0)
                (builder[0], builder[englishIndex]) = (builder[englishIndex], builder[0]);
            
            Localization = builder.MoveToImmutable();
        }
        catch (SystemException) { }
    }

    internal void LoadMetadata()
    {
        lock (TreeSync)
        {
            bool isErrored = false;
            Guid? guid = null;
            ushort? id = null;
            AssetCategoryValue? category = null;
            QualifiedOrAliasedType? type = null;
            QualifiedType? actualType = null;

            IValueSourceNode? guidProp, typeProp;
            Guid parsedGuid;
            IDictionarySourceNode? metadata = this.GetMetadataDictionary();
            if (metadata != null)
            {
                if (metadata.TryGetPropertyValue("GUID", out guidProp))
                {
                    if (!KnownTypeValueHelper.TryParseGuid(guidProp.Value, out parsedGuid) || parsedGuid == System.Guid.Empty)
                    {
                        isErrored = true;
                    }
                    else
                    {
                        guid = parsedGuid;
                    }
                }
                else
                {
                    isErrored = true;
                }

                if (metadata.TryGetPropertyValue("Type", out typeProp))
                {
                    if (!KnownTypeValueHelper.TryParseType(typeProp.Value, out QualifiedType parsedType))
                    {
                        isErrored = true;
                    }
                    else
                    {
                        type = new QualifiedOrAliasedType(parsedType);
                        actualType = parsedType;
                    }
                }
            }
            else if (!this.TryGetPropertyValue("GUID", out guidProp)
                     || !KnownTypeValueHelper.TryParseGuid(guidProp.Value, out parsedGuid)
                     || parsedGuid == System.Guid.Empty)
            {
                isErrored = true;
            }
            else
            {
                guid = parsedGuid;
            }

            IDictionarySourceNode assetData = this.AssetData;

            if (!type.HasValue)
            {
                if (!assetData.TryGetPropertyValue("Type", out typeProp) || string.IsNullOrWhiteSpace(typeProp.Value))
                {
                    isErrored = true;
                }
                else if (Database.Information.AssetAliases.TryGetValue(typeProp.Value, out QualifiedType parsedActualType))
                {
                    actualType = parsedActualType;
                    type = new QualifiedOrAliasedType(typeProp.Value);
                }
                else
                {
                    parsedActualType = new QualifiedType(typeProp.Value);
                    actualType = parsedActualType;
                    type = new QualifiedOrAliasedType(parsedActualType);
                }
            }

            if (assetData.TryGetPropertyValue("ID", out IValueSourceNode? idProp))
            {
                if (KnownTypeValueHelper.TryParseUInt16(idProp.Value, out ushort parsedId))
                {
                    id = parsedId;
                }
            }

            if (actualType.HasValue)
            {
                category = AssetCategoryValue.None;
                if (actualType.Equals("SDG.Unturned.RedirectorAsset, Assembly-CSharp"))
                {
                    if (assetData.TryGetPropertyValue("AssetCategory", out IValueSourceNode? assetCategoryProp)
                        && AssetCategory.TryParse(assetCategoryProp.Value, out int index))
                    {
                        category = new AssetCategoryValue(index);
                    }
                }
                else if (Database.Information.AssetCategories.TryGetValue(actualType.Value, out string? catStr))
                {
                    category = new AssetCategoryValue(catStr);
                }
                else
                {
                    InverseTypeHierarchy parentTypes = Database.Information.GetParentTypes(actualType.Value);
                    for (int i = parentTypes.ParentTypes.Length - 1; i >= 0; --i)
                    {
                        if (!Database.Information.AssetCategories.TryGetValue(parentTypes.ParentTypes[i], out catStr))
                            continue;

                        category = new AssetCategoryValue(catStr);
                        break;
                    }
                }
            }

            Guid = guid;
            Id = id;
            Category = category.GetValueOrDefault(AssetCategoryValue.None);
            AssetType = type.GetValueOrDefault();
            ActualType = actualType.GetValueOrDefault(QualifiedType.None);
            IsErrored = isErrored;
        }
    }
}
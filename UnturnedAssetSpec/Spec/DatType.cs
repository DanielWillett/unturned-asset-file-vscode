using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Types of data specifications
/// </summary>
public enum DatSpecificationType
{
    /// <summary>
    /// A non-asset type of .dat file specification. Represented by a <see cref="DatFileType"/> object.
    /// </summary>
    /// <remarks>Of type <see cref="DatFileType"/>.</remarks>
    File,

    /// <summary>
    /// An asset type of .dat file specification.
    /// </summary>
    /// <remarks>Of type <see cref="DatAssetFileType"/>.</remarks>
    AssetFile,

    /// <summary>
    /// A data structure which is defined in a specification and can be used within .dat files.
    /// </summary>
    /// <remarks>Of type <see cref="DatCustomType"/>.</remarks>
    Custom,

    /// <summary>
    /// A data structure which is defined in a specification and can be used within .dat files.
    /// </summary>
    /// <remarks>Of type <see cref="DatCustomAssetType"/>.</remarks>
    CustomAsset,

    /// <summary>
    /// A set of allowed values which are defined in a specification and can be used within .dat files.
    /// </summary>
    /// <remarks>Of type <see cref="DatEnumType"/>.</remarks>
    Enum,

    /// <summary>
    /// A set of allowed values which are defined in a specification and can be used within .dat files and combined using bitwise operators.
    /// <para>
    /// The C# definition should be annotated with the <see cref="FlagsAttribute"/>.
    /// </para>
    /// </summary>
    /// <remarks>Of type <see cref="DatFlagEnumType"/>.</remarks>
    FlagEnum
}

/// <summary>
/// Contains information about the properties and behavior available for a .dat file type or custom type.
/// </summary>
public abstract class DatType : BaseType<DatType>, IDatSpecificationObject
{
    internal string DisplayNameIntl;

    /// <summary>
    /// The root object of this type, unless it was created at runtime (ex. during a unit test).
    /// </summary>
    public JsonElement DataRoot { get; }

    /// <summary>
    /// The file in which this type is defined. For <see cref="DatFileType"/> objects, this property is equal to the defining object (this).
    /// </summary>
    public abstract DatFileType Owner { get; }

    /// <summary>
    /// The fully-qualified type of the object this type of file defines.
    /// </summary>
    public QualifiedType TypeName { get; }

    /// <inheritdoc />
    public override string Id => TypeName.Type;

    /// <summary>
    /// The type of specification this type defines.
    /// </summary>
    public abstract DatSpecificationType Type { get; }

    /// <summary>
    /// The base type of this type specification, or <see langword="null"/> if this type doesn't have a base type.
    /// </summary>
    public DatTypeWithProperties? BaseType { get; }


    /// <summary>
    /// The display name of the object this type of file defines.
    /// </summary>
    public override string DisplayName => DisplayNameIntl;

    /// <summary>
    /// URL to the SDG docs for this type of file.
    /// </summary>
    public string? Docs { get; internal set; }

    /// <summary>
    /// Whether or not this file type is a project file for this library as opposed to a file read by Unturned.
    /// </summary>
    public bool IsProjectFile { get; internal set; }

    /// <summary>
    /// The version of Unturned this type was added in.
    /// </summary>
    public Version? Version { get; internal set; }

    private protected abstract string FullName { get; }

    private protected DatType(QualifiedType type, DatTypeWithProperties? baseType, JsonElement element)
    {
        TypeName = type;
        DisplayNameIntl = type.Type;
        BaseType = baseType;
        DataRoot = element;
    }

    /// <summary>
    /// Creates a new enum type for the given <paramref name="typeName"/>.
    /// </summary>
    /// <param name="typeName">The fully-qualified type name of the type being created.</param>
    /// <param name="isFlagEnum">Whether or not the enum represents a bitwise flag enum.</param>
    /// <param name="element">The JSON element this type was read from.</param>
    /// <param name="file">The file this type is defined in.</param>
    /// <returns>The newly-created <see cref="DatEnumType"/> or <see cref="DatFlagEnumType"/> instance.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static DatEnumType CreateEnumType(QualifiedType typeName, bool isFlagEnum, JsonElement element, DatFileType file, IDatSpecificationReadContext context)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        if (string.IsNullOrEmpty(typeName.Type))
            throw new ArgumentNullException(nameof(typeName));

        return isFlagEnum
            ? new DatFlagEnumType(typeName, element, file, context)
            : new DatEnumType(typeName, element, file, context);
    }

    /// <summary>
    /// Creates a new custom type for the given <paramref name="typeName"/>.
    /// </summary>
    /// <param name="typeName">The fully-qualified type name of the type being created.</param>
    /// <param name="element">The JSON element this type was read from.</param>
    /// <param name="baseType">The base type of this custom type.</param>
    /// <param name="file">The file this type is defined in.</param>
    /// <returns>The newly-created <see cref="DatCustomType"/> or <see cref="DatCustomAssetType"/> instance.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static DatCustomType CreateCustomType(QualifiedType typeName, JsonElement element, DatTypeWithProperties? baseType, DatFileType file, IDatSpecificationReadContext context)
    {
        if (string.IsNullOrEmpty(typeName.Type))
            throw new ArgumentNullException(nameof(typeName));

        return file is DatAssetFileType assetFileType
            ? new DatCustomAssetType(typeName, baseType, element, assetFileType, context)
            : new DatCustomType(typeName, baseType, element, file, context);
    }

    /// <summary>
    /// Creates a new file type for the given <paramref name="typeName"/>.
    /// </summary>
    /// <param name="typeName">The fully-qualified type name of the type being created.</param>
    /// <param name="isAssetFileType">Whether or not this type is assignable to "SDG.Unturned.Asset".</param>
    /// <param name="element">The JSON element this type was read from.</param>
    /// <param name="baseType">The base type of this custom type.</param>
    /// <returns>The newly-created <see cref="DatFileType"/> or <see cref="DatAssetFileType"/> instance.</returns>
    /// <exception cref="ArgumentNullException"/>
    public static DatFileType CreateFileType(QualifiedType typeName, bool isAssetFileType, JsonElement element, DatFileType? baseType, IDatSpecificationReadContext context)
    {
        if (string.IsNullOrEmpty(typeName.Type))
            throw new ArgumentNullException(nameof(typeName));

        return isAssetFileType
            ? new DatAssetFileType(typeName, baseType, element)
            : new DatFileType(typeName, baseType, element);
    }

    /// <summary>
    /// Attempt to get a type by it's fully-qualified type name. Types in this type or any base types will be returned.
    /// </summary>
    public bool TryGetType(QualifiedType typeName, [NotNullWhen(true)] out DatType? type)
    {
        if (!typeName.IsCaseInsensitive)
        {
            typeName = typeName.CaseInsensitive;
        }

        for (DatType? baseType = this; baseType != null; baseType = baseType.BaseType)
        {
            if (typeName.Equals(baseType.TypeName))
            {
                type = baseType;
                return true;
            }

            if (baseType.TryGetTypeInFileIntl(typeName, out type))
            {
                return true;
            }
        }

        type = null;
        return false;
    }

    protected virtual bool TryGetTypeInFileIntl(QualifiedType typeName, [NotNullWhen(true)] out DatType? type)
    {
        type = null;
        return false;
    }

    string IDatSpecificationObject.FullName => FullName;

    public override void Visit<TVisitor>(ref TVisitor visitor) { }
    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStringValue(TypeName.Type);
    }

    /// <inheritdoc />
    protected override bool Equals(DatType other)
    {
        return (object)other == this;
    }

    /// <inheritdoc />
    public override int GetHashCode() => TypeName.GetHashCode();
}

/// <summary>
/// Base type for any <see cref="DatType"/> implementations that have properties.
/// </summary>
public abstract class DatTypeWithProperties : DatType
{
    internal ImmutableArray<DatProperty>.Builder? PropertiesBuilder;

    /// <summary>
    /// List of all available properties that can be stored in this type of file.
    /// </summary>
    public ImmutableArray<DatProperty> Properties { get; internal set; }

    /// <summary>
    /// If <see langword="true"/>, all property keys have the exact same name as their field/property name in code unless otherwise defined.
    /// </summary>
    public bool AutoGeneratedKeys { get; internal set; }

    /// <summary>
    /// If <see langword="true"/>, properties without values are parsed as their default value and don't raise an error.
    /// </summary>
    public bool OverridableProperties { get; internal set; }

    internal DatTypeWithProperties(QualifiedType type, DatTypeWithProperties? baseType, JsonElement element) : base(type, baseType, element)
    {
        Properties = ImmutableArray<DatProperty>.Empty;
    }
}

/// <summary>
/// Base interface for any <see cref="DatType"/> implementations that have localization properties.
/// </summary>
public interface IDatTypeWithLocalizationProperties
{
    internal ImmutableArray<DatProperty>.Builder? LocalizationPropertiesBuilder { get; }

    /// <summary>
    /// List of all available properties that can be stored in the localization file for this type of asset.
    /// </summary>
    ImmutableArray<DatProperty> LocalizationProperties { get; }
}

/// <summary>
/// Base interface for any <see cref="DatType"/> implementations that have bundle assets.
/// </summary>
public interface IDatTypeWithBundleAssets
{
    internal ImmutableArray<DatBundleAsset>.Builder? BundleAssetsBuilder { get; }

    /// <summary>
    /// List of all available Unity objects that can be in the bundle for this asset.
    /// </summary>
    ImmutableArray<DatBundleAsset> BundleAssets { get; }
}

/// <summary>
/// Base interface for any <see cref="DatType"/> that can use a string parser.
/// </summary>
public interface IDatTypeWithStringParseableType<T>
    where T : IEquatable<T>
{
    /// <summary>
    /// The type parser for this type.
    /// </summary>
    ITypeConverter<T>? StringParser { get; }

    /// <summary>
    /// Pre-defined fully-qualified C# type name used to parse values of this type.
    /// </summary>
    /// <remarks>This type should implement <see cref="ITypeConverter{T}"/> for the correct type.</remarks>
    QualifiedType StringParseableType { get; }
}

/// <summary>
/// A subclass of <see cref="DatType"/> which is created for non-asset types.
/// </summary>
public class DatFileType : DatTypeWithProperties
{
    // allows type properties to reference other types before they're finalized.
    internal ImmutableDictionary<QualifiedType, DatType>.Builder? TypesBuilder;

    /// <inheritdoc />
    public override DatSpecificationType Type => DatSpecificationType.File;

    /// <inheritdoc />
    public override DatFileType Owner => this;

    /// <summary>
    /// Whether or not this <see cref="DatFileType"/> was generated from the Localization .dat files.
    /// </summary>
    public bool IsLocalizationFile { get; internal set; }

    /// <summary>
    /// Whether or not this localization file should only allow keys (such as the <c>Curse_Words.txt</c> file).
    /// </summary>
    public bool IsKeyOnlyLocalizationFile { get; internal set; }

    private protected override string FullName => TypeName.GetFullTypeName();

    /// <inheritdoc cref="DatType.BaseType" />
    public DatFileType? Parent { get; }

    /// <summary>
    /// List of all available types that are defined within this file.
    /// </summary>
    public ImmutableDictionary<QualifiedType, DatType> Types { get; internal set; }

    internal DatFileType(QualifiedType type, DatFileType? baseType, JsonElement element) : base(type, baseType, element)
    {
        Types = ImmutableDictionary<QualifiedType, DatType>.Empty;
        Parent = baseType;
    }

    protected override bool TryGetTypeInFileIntl(QualifiedType typeName, [NotNullWhen(true)] out DatType? type)
    {
        if (TypesBuilder != null)
        {
            if (TypesBuilder.TryGetValue(typeName, out type))
            {
                return true;
            }
        }
        else if (Types.TryGetValue(typeName, out type))
        {
            return true;
        }

        type = null;
        return false;
    }
}


/// <summary>
/// A subclass of <see cref="DatType"/> which is only created for Asset types, including the base type, "SDG.Unturned.Asset".
/// </summary>
public class DatAssetFileType : DatFileType, IDatTypeWithLocalizationProperties, IDatTypeWithBundleAssets
{
    internal ImmutableArray<DatProperty>.Builder? LocalizationPropertiesBuilder { get; set; }

    internal ImmutableArray<DatBundleAsset>.Builder? BundleAssetsBuilder { get; set; }

    /// <inheritdoc />
    public override DatSpecificationType Type => DatSpecificationType.AssetFile;

    /// <summary>
    /// The category of the asset represented by this type.
    /// </summary>
    public AssetCategoryValue Category { get; internal set; }

    /// <summary>
    /// The first ID which isn't reserved by vanilla assets.
    /// </summary>
    public ushort? VanillaIdLimit { get; internal set; }

    /// <summary>
    /// Whether or not assets of this type must have a legacy ID to function correctly within the game.
    /// </summary>
    public bool RequireId { get; internal set; }

    /// <inheritdoc />
    public ImmutableArray<DatProperty> LocalizationProperties { get; internal set; }

    /// <inheritdoc />
    public ImmutableArray<DatBundleAsset> BundleAssets { get; internal set; }

    internal DatAssetFileType(QualifiedType type, DatFileType? baseType, JsonElement element) : base(type, baseType, element)
    {
        LocalizationProperties = ImmutableArray<DatProperty>.Empty;
        BundleAssets = ImmutableArray<DatBundleAsset>.Empty;
    }

    ImmutableArray<DatProperty>.Builder? IDatTypeWithLocalizationProperties.LocalizationPropertiesBuilder
        => LocalizationPropertiesBuilder;
    ImmutableArray<DatBundleAsset>.Builder? IDatTypeWithBundleAssets.BundleAssetsBuilder
        => BundleAssetsBuilder;
}
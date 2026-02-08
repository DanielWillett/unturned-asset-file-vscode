using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Used to allow lookup of other types while reading type info from JSON.
/// </summary>
public interface IDatSpecificationReadContext
{
    /// <summary>
    /// Pre-read asset information from the Assets.json file.
    /// </summary>
    AssetInformation Information { get; }

    /// <summary>
    /// The database being read into. Note that only some information will be available at this time.
    /// </summary>
    IAssetSpecDatabase Database { get; }

    /// <summary>
    /// Allows readers to create loggers to log non-critical errors.
    /// </summary>
    ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// Finds an already read file type or reads it from the current context.
    /// </summary>
    DatFileType? GetOrReadFileType(QualifiedType typeName);

    /// <summary>
    /// Finds an already read type or reads it from the <paramref name="owner"/>'s context.
    /// </summary>
    IType? GetOrReadType(IDatSpecificationObject owner, QualifiedType typeName);

    /// <summary>
    /// Reads a type from a JSON object or string.
    /// </summary>
    IType ReadType(in JsonElement root, DatProperty readObject, string context = "");

    /// <summary>
    /// Reads a property value from a JSON object or string.
    /// </summary>
    IValue ReadValue(in JsonElement root, IPropertyType valueType, IDatSpecificationObject readObject, string context = "", ValueReadOptions options = ValueReadOptions.Default);
}
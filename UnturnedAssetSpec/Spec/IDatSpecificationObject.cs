using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Any object defined in the file type specification.
/// </summary>
public interface IDatSpecificationObject
{
    /// <summary>
    /// The root object of this type, unless it was created at runtime (ex. during a unit test).
    /// </summary>
    JsonElement DataRoot { get; }

    /// <summary>
    /// The full name of this object.
    /// </summary>
    string FullName { get; }

    /// <summary>
    /// The file in which this object is defined.
    /// </summary>
    DatFileType Owner { get; }
}

using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// Shared interface between <see cref="PropertyOrderFile"/> and <see cref="ScaffoldedPropertyOrderFile"/>.
/// </summary>
public interface IPropertyOrderFile
{
    /// <summary>
    /// Gets the order-set for the given type.
    /// </summary>
    /// <returns>
    /// If the type is not defined in the orderfile, an empty array,
    /// otherwise, an array with property indicies referencing the type's property array.
    /// </returns>
    OrderedPropertyReference[] GetOrderForType(QualifiedType type, SpecPropertyContext context);

    /// <summary>
    /// Gets an array that contains values indexed by property index.
    /// For example, if the property at index <c>3</c> comes before the property at index <c>5</c>, <c>arr[3]</c> will be less than <c>arr[5]</c>.
    /// </summary>
    (int[] ReverseOrder, int AlternateOffset) GetRelativePositions(QualifiedType type, SpecPropertyContext context);
}
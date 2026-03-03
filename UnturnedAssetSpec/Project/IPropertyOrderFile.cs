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
    /// <param name="type"></param>
    /// <param name="context"></param>
    /// <returns>
    /// If the type is not defined in the orderfile, an empty array,
    /// otherwise, an array with property indicies referencing the type's property array.<br/>
    /// A value of <see cref="PropertyOrderFile.EmptyLine"/> indicates an empty line.<br/>
    /// A value of <see cref="PropertyOrderFile.SectionSeparator"/> indicates a new section.<br/>
    /// A value less than the property count of the type indicates a property positioning.<br/>
    /// A value greater than or equal to the property count of the type indicates a property reference to a base type.
    /// Each base type subtracts it's property count from the index.<br/>
    /// For example:
    /// <code>
    /// Type A is the base type of B (B extends A).
    /// A.Properties:
    ///    - PropA
    ///    - PropB
    /// B.Properties:
    ///    - PropC
    /// </code>
    /// An index of '2' would be equivalent to 'PropB'<br/>
    /// An index of '3' would be equivalent to '@PropC'.
    /// </returns>
    OrderedPropertyReference[] GetOrderForType(QualifiedType type, SpecPropertyContext context);
}
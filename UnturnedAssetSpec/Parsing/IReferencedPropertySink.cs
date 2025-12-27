using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// Allows parsers to indicate that they are dependant on another property.
/// </summary>
public interface IReferencedPropertySink
{
    /// <summary>
    /// Indicates that this property is being referenced by another explicitly defined property.
    /// </summary>
    /// <remarks>For example, this is used to parse legacy color components by referencing their <c>Property_R</c>, <c>Property_G</c>, <c>Property_B</c> components.</remarks>
    /// <param name="property">The source node of the property being referenced.</param>
    /// <exception cref="ArgumentNullException"/>
    void AcceptReferencedProperty(IPropertySourceNode property);

    /// <summary>
    /// Indicates that this property is not being referenced and can be removed.
    /// <para>
    /// For example, this may happen for 'Color' in the following case, if 'Color' was a legacy-only color32 property
    /// <code>
    /// Color #ffffff
    /// Color_R 255
    /// Color_G 255
    /// Color_B 255
    /// </code>
    /// </para>
    /// </summary>
    /// <param name="property">The source node of the property being dereferenced.</param>
    /// <exception cref="ArgumentNullException"/>
    void AcceptDereferencedProperty(IPropertySourceNode property);
}

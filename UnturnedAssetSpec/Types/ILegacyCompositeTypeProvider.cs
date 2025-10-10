using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A type which can be broken into multiple top-level properties.
/// </summary>
public interface ILegacyCompositeTypeProvider : ISpecPropertyType
{
    /// <summary>
    /// Whether or not the current type's settings allow it to be broken.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Visits all properties which are part of this property and type.
    /// </summary>
    /// <remarks>The implementer can assume that the property does not exist as a basic property if the parser has gotten to this method.</remarks>
    void VisitLinkedProperties<TVisitor>(
        in FileEvaluationContext context,
        SpecProperty property,
        IDictionarySourceNode propertyRoot,
        ref TVisitor propertyVisitor,
        OneOrMore<int> templateGroupIndices
    ) where TVisitor : ISourceNodePropertyVisitor;
}

using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// A model of a file storing all properties and directly relating properties.
/// </summary>
public interface IFileRelationalModel
{
    /// <summary>
    /// The file being modeled.
    /// </summary>
    ISourceFile SourceFile { get; }

    /// <summary>
    /// Whether or not to collect diagnostics on the next rebuild.
    /// </summary>
    bool CollectDiagnostics { get; set; }

    /// <summary>
    /// Diagnostics collected on the last rebuild if <see cref="CollectDiagnostics"/> is <see langword="true"/>, otherwise <see langword="default"/>.
    /// </summary>
    ImmutableArray<DatDiagnosticMessage> Diagnostics { get; }

    /// <summary>
    /// Rebuilds the model, collecting diagnostics if <see cref="CollectDiagnostics"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="force">If <see langword="false"/>, the model will only be rebuilt if the file has changed since it was last built.</param>
    void Rebuild(ISourceFile maybeNewFile, bool force);

    /// <summary>
    /// Rebuilds the model from a new file, collecting diagnostics if <see cref="CollectDiagnostics"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="newFile">A new file instance to replace the original one.</param>
    void Rebuild(ISourceFile newFile);

    /// <summary>
    /// Gets the property being referenced from a <see cref="IPropertySourceNode"/>.
    /// </summary>
    /// <param name="node">Any node that's part of the property on the same level as the main node of the property.</param>
    /// <param name="property">The found property.</param>
    /// <param name="valueOnly">If <see langword="true"/>, the property of a sub-level property (like the key of a dictionary) will not return the dictionary as a property, instead failing.</param>
    /// <returns>Whether or not the property was found.</returns>
    bool TryGetPropertyFromNode(
        IPropertySourceNode node,
        [NotNullWhen(true)] out DatProperty? property,
        bool valueOnly = false
    );

    /// <summary>
    /// Gets relational info about a property from a <see cref="IPropertySourceNode"/>.
    /// </summary>
    /// <param name="node">Any node that's part of the property on the same level as the main node of the property.</param>
    /// <param name="info">Various information about the property.</param>
    /// <returns>Whether or not the property was found.</returns>
    bool TryGetPropertyInfoFromNode(
        IPropertySourceNode node,
        [UnscopedRef] out PropertyNodeRelationalInfo info,
        bool includeValue = false
    );

    /// <summary>
    /// Gets the property being referenced from a <see cref="IPropertySourceNode"/> and visit it's value at the same time.
    /// </summary>
    /// <param name="node">Any node that's part of the property on the same level as the main node of the property.</param>
    /// <param name="property">The found property.</param>
    /// <param name="value">The value.</param>
    /// <param name="missingValueBehavior">What to do if the property isn't defined in the file.</param>
    /// <returns>Whether or not the property was found.</returns>
    bool TryVisitPropertyValueFromNode<TVisitor>(
        IPropertySourceNode node,
        ref TVisitor visitor,
        [NotNullWhen(true)] out DatProperty? property,
        TypeParserMissingValueBehavior missingValueBehavior = TypeParserMissingValueBehavior.ErrorIfValueOrPropertyNotProvided
    ) where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    ;
}

public struct PropertyNodeRelationalInfo
{
    public DatProperty Property { get; }
    public IPropertySourceNode? Node { get; }
    public IDictionarySourceNode Dictionary { get; }
    public ImmutableArray<IPropertySourceNode> RelatedProperties { get; }

    public IValue? Value { get; }
    public IType? ValueType { get; }
    public TypeParserResult ValueResult { get; }

    public PropertyNodeRelationalInfo(DatProperty property,
        IPropertySourceNode? node,
        IDictionarySourceNode dictionary,
        IValue? value,
        IType? valueType,
        ImmutableArray<IPropertySourceNode> relatedProperties,
        TypeParserResult result)
    {
        Property = property;
        Node = node;
        Dictionary = dictionary;
        Value = value;
        ValueType = valueType;
        RelatedProperties = relatedProperties;
        ValueResult = result;
    }
}
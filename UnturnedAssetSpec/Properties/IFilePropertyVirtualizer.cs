using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Responsible for identifying real and 'virtual' properties in files.
/// </summary>
public interface IFilePropertyVirtualizer
{
    IEnumerable<IFileProperty> EnumerateProperties(ISourceFile file);

    IFileProperty? FindProperty(ISourceFile file, SpecProperty property);
    IFileProperty? FindProperty(ISourceFile file, SpecProperty property, PropertyBreadcrumbs propertyBreadcrumbs);
}

/// <summary>
/// Represents one or more property nodes that link to a property.
/// </summary>
public interface IFileProperty
{
    SpecProperty Property { get; }

    ISpecType Owner { get; }

    bool TryGetValue(out ISpecDynamicValue? value);
}

public struct FilePropertyInstance
{
    public SpecProperty Property { get; }
    public IPropertySourceNode Node { get; }

    public FilePropertyInstance(SpecProperty property, IPropertySourceNode node)
    {
        Property = property;
        Node = node;
    }
}
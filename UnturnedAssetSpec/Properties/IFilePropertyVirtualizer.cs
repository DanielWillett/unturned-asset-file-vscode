using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Collections.Generic;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Responsible for identifying real and 'virtual' properties in files.
/// </summary>
public interface IFilePropertyVirtualizer
{
    IEnumerable<IFileProperty> EnumerateProperties(ISourceFile file);

    IFileProperty? FindProperty(ISourceFile file, DatProperty property);
    IFileProperty? FindProperty(ISourceFile file, DatProperty property, in PropertyBreadcrumbs propertyBreadcrumbs);

    DatProperty? GetProperty(IPropertySourceNode propertyNode, out PropertyResolutionContext context);
    DatProperty? GetProperty(IPropertySourceNode propertyNode, in AssetFileType fileType, in PropertyBreadcrumbs propertyBreadcrumbs, out PropertyResolutionContext context);
}

/// <summary>
/// Represents one or more property nodes that link to a property.
/// </summary>
public interface IFileProperty
{
    DatProperty Property { get; }

    DatType Owner { get; }

    bool TryGetValue(out IValue? value);
}

public struct FilePropertyInstance
{
    public DatProperty Property { get; }
    public IPropertySourceNode Node { get; }

    public FilePropertyInstance(DatProperty property, IPropertySourceNode node)
    {
        Property = property;
        Node = node;
    }
}
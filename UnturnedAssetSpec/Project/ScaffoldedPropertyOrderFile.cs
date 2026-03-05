using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// Multiple merged <see cref="IPropertyOrderFile"/>.
/// </summary>
public class ScaffoldedPropertyOrderFile : IPropertyOrderFile
{
    private readonly PropertyOrderFile[] _files;

    public ScaffoldedPropertyOrderFile(PropertyOrderFile[] files)
    {
        _files = files;
    }

    /// <summary>
    /// Rebuilds the order-set for an updated file and children of that file.
    /// </summary>
    public bool UpdateOrderFile(IAssetSpecDatabase database, ISourceFile file)
    {
        string fileName = file.WorkspaceFile.File;
        int index = -1;
        for (int i = 0; i < _files.Length; ++i)
        {
            if (!string.Equals(_files[i].FileName, fileName, OSPathHelper.PathComparison))
                continue;

            index = i;
        }

        if (index < 0)
            return false;

        bool s = true;
        for (int i = index; i >= 0; --i)
        {
            s &= _files[i].TryUpdateFromFile(database, file, i == _files.Length - 1 ? null : _files[i + 1]);
        }

        return s;
    }

    /// <inheritdoc />
    public OrderedPropertyReference[] GetOrderForType(QualifiedType type, SpecPropertyContext context)
    {
        for (int i = 0; i < _files.Length; ++i)
        {
            OrderedPropertyReference[] arr = _files[i].GetOrderForType(type, context);
            if (arr.Length == 0)
                continue;

            return arr;
        }

        return Array.Empty<OrderedPropertyReference>();
    }

    /// <inheritdoc />
    public (int[] ReverseOrder, int AlternateOffset) GetRelativePositions(QualifiedType type, SpecPropertyContext context)
    {
        for (int i = 0; i < _files.Length; ++i)
        {
            (int[], int) arr = _files[i].GetRelativePositions(type, context);
            if (arr.Item1.Length == 0)
                continue;

            return arr;
        }

        return (Array.Empty<int>(), 0);
    }
}
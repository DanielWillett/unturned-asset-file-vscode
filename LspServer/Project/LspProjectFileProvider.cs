using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Project;

internal class LspProjectFileProvider : IProjectFileProvider
{
    private readonly LspWorkspaceEnvironment _workspaceEnvironment;

    internal LspProjectFileProvider(LspWorkspaceEnvironment workspaceEnvironment)
    {
        _workspaceEnvironment = workspaceEnvironment;
    }

    /// <inheritdoc />
    public IEnumerable<ProjectFile> EnumerateProjectFiles(IWorkspaceFile? fileContext)
    {
        return Array.Empty<ProjectFile>();
    }

    /// <inheritdoc />
    public IPropertyOrderFile GetScaffoldedOrderfile(IWorkspaceFile? fileContext)
    {
        return new ScaffoldedPropertyOrderFile(Array.Empty<PropertyOrderFile>());
    }

    /// <inheritdoc />
    public T? AggregateProjectFiles<T>(IWorkspaceFile? fileContext, Func<ProjectFile, T> selector)
        where T : class
    {
        return null;
    }

    /// <inheritdoc />
    public T? AggregateProjectFiles<T>(IWorkspaceFile? fileContext, Func<ProjectFile, T?> selector)
        where T : struct
    {
        return null;
    }
}

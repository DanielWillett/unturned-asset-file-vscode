using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

internal class DiscoverAssetPropertiesHandler : IDiscoverAssetPropertiesHandler
{
    private static readonly Container<AssetProperty> Empty = new Container<AssetProperty>(Array.Empty<AssetProperty>());

    private readonly OpenedFileTracker _fileTracker;
    private readonly IAssetSpecDatabase _spec;
    private readonly IWorkspaceEnvironment _environment;
    private readonly InstallationEnvironment _installEnvironment;

    public DiscoverAssetPropertiesHandler(OpenedFileTracker fileTracker, IAssetSpecDatabase spec, IWorkspaceEnvironment environment, InstallationEnvironment installEnvironment)
    {
        _fileTracker = fileTracker;
        _spec = spec;
        _environment = environment;
        _installEnvironment = installEnvironment;
    }

    public Task<Container<AssetProperty>> Handle(DiscoverAssetPropertiesParams request, CancellationToken cancellationToken)
    {
        if (!_fileTracker.Files.TryGetValue(request.Document, out OpenedFile? file))
        {
            return Task.FromResult(Empty);
        }

        AssetFileType fileType = AssetFileType.FromFile(file.File, _spec);
        if (!fileType.IsValid)
        {
            return Task.FromResult(Empty);
        }

        FileEvaluationContext context = new FileEvaluationContext(null!, fileType.Information, file.File, _environment, _installEnvironment, _spec);

        List<SpecProperty> properties = fileType.Information.Properties
            .Where(x => !x.Deprecated)
            .OrderByDescending(x => x.Priority)
            .ThenBy(x => x.Key)
            .ToList();

        AssetProperty[] outputProperties = new AssetProperty[properties.Count];

        for (int i = 0; i < properties.Count; i++)
        {
            SpecProperty property = properties[i];
            FileEvaluationContext propContext = new FileEvaluationContext(in context, property);

            AssetProperty prop = new AssetProperty
            {
                Key = property.Key,
                Description = property.Description,
                Markdown = property.Markdown
            };

            outputProperties[i] = prop;

            if (!propContext.TryGetValue(out ISpecDynamicValue val, out AssetFileKeyValuePairNode? lineNode))
                continue;

            if (val != null && val.TryEvaluateValue(in propContext, out object? value))
            {
                prop.Value = value is QualifiedType { IsNormalized: false } qt ? qt.Normalized : value;
            }
            if (lineNode != null)
            {
                prop.Range = lineNode.Range.ToRange();
            }
        }

        return Task.FromResult(new Container<AssetProperty>(outputProperties));
    }
}
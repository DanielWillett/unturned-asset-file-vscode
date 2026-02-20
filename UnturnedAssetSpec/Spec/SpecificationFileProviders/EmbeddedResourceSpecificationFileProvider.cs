using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Pulls specification files from the assembly's embedded resources.
/// </summary>
public sealed class EmbeddedResourceSpecificationFileProvider : ISpecificationFileProvider
{
    private readonly ILogger<EmbeddedResourceSpecificationFileProvider> _logger;

    internal const string EmbeddedResourceLocation = "DanielWillett.UnturnedDataFileLspServer.Data..Asset_Spec.{0}.json";

    public int Priority => 0;

    public bool IsEnabled => true;

    public EmbeddedResourceSpecificationFileProvider(ILogger<EmbeddedResourceSpecificationFileProvider> logger)
    {
        _logger = logger;
    }

    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        string resourceLocation = string.Format(EmbeddedResourceLocation, type.Type.ToLowerInvariant());
        return ReadResourceAsync(resourceLocation, state, action, token);
    }

    public Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        if (file != KnownConfigurationFile.Assets)
            return Task.FromResult(false);

        string resourceLocation = string.Format(EmbeddedResourceLocation, "Assets");
        return ReadResourceAsync(resourceLocation, state, action, token);
    }

    private async Task<bool> ReadResourceAsync<TState>(string resourceLocation, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        Stream? stream;
        try
        {
            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation);
            if (stream == null)
            {
                _logger.LogWarning(Resources.Log_FailedToFindEmbeddedResource, resourceLocation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, Resources.Log_FailedToFindEmbeddedResource, resourceLocation);
            stream = null;
        }

        if (stream == null)
        {
            return false;
        }

        try
        {
            await action(stream, state, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Resources.Log_FailedToParseResource, resourceLocation);
            return false;
        }
        finally
        {
            stream.Dispose();
        }

        return true;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Concurrent;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public class FileRelationalCacheProvider : IFileRelationalModelProvider
{
    private readonly IParsingServices _parsingServices;

    private readonly ConcurrentDictionary<string, IFileRelationalModel>?[] _cachedModels;

    public FileRelationalCacheProvider(IParsingServices parsingServices)
    {
        _parsingServices = parsingServices;
        _cachedModels =
        [
            // Properties
            new ConcurrentDictionary<string, IFileRelationalModel>(OSPathHelper.PathComparer),
            
            // Localization
            new ConcurrentDictionary<string, IFileRelationalModel>(OSPathHelper.PathComparer),

            // cross-references
            null, null, null,
            
            // Bundle Assets
            new ConcurrentDictionary<string, IFileRelationalModel>(OSPathHelper.PathComparer)
        ];

#if NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        _valueFactory = ValueFactory;
#endif
    }

    public void RemoveModel(string filePath)
    {
        for (int i = 0; i < _cachedModels.Length; ++i)
        {
            ConcurrentDictionary<string, IFileRelationalModel>? d = _cachedModels[i];
            if (d != null && d.TryRemove(filePath, out IFileRelationalModel? model))
            {
                (model as IDisposable)?.Dispose();
            }
        }
    }

    /// <inheritdoc />
    public IFileRelationalModel GetProvider(ISourceFile file, SpecPropertyContext context = SpecPropertyContext.Property)
    {
        string fp = file.WorkspaceFile.File;
        int index = (int)context - 1;
        if (index < 0 || index >= _cachedModels.Length)
            throw new ArgumentOutOfRangeException(nameof(context));

        ConcurrentDictionary<string, IFileRelationalModel>? dict = _cachedModels[index];
        if (dict == null)
            throw new ArgumentOutOfRangeException(nameof(context));

#if NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        GetOrAddFileState state;
        state.File = file;
        state.Context = context;
        return dict.GetOrAdd(fp, _valueFactory, state);
#else
        return dict.GetOrAdd(fp, _ => new FileRelationalCache(file, false, _parsingServices, context));
#endif
    }

#if NET472_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    private readonly Func<string, GetOrAddFileState, IFileRelationalModel> _valueFactory;
    private IFileRelationalModel ValueFactory(string fp, GetOrAddFileState state)
    {
        return new FileRelationalCache(state.File, false, _parsingServices, state.Context);
    }

    private struct GetOrAddFileState
    {
        public ISourceFile File;
        public SpecPropertyContext Context;
    }
#endif
}
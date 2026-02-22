using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Protocol;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;

internal class UnturnedAssetFileSyncHandler : ITextDocumentSyncHandler
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly ILanguageServerFacade _langServer;
    private readonly ILogger<UnturnedAssetFileSyncHandler> _logger;
    private readonly FileRelationalCacheProvider _cacheProvider;

    private readonly TextDocumentChangeRegistrationOptions _changeRegistrationOptions;
    private readonly TextDocumentOpenRegistrationOptions _openRegistrationOptions;
    private readonly TextDocumentCloseRegistrationOptions _closeRegistrationOptions;
    private readonly TextDocumentSaveRegistrationOptions _saveRegistrationOptions;

    public event Action<OpenedFile>? ContentUpdated;
    public event Action<OpenedFile>? FileAdded;
    public event Action<OpenedFile>? FileRemoved;

    public UnturnedAssetFileSyncHandler(
        OpenedFileTracker fileTracker,
        ILanguageServerFacade langServer,
        ILogger<UnturnedAssetFileSyncHandler> logger,
        FileRelationalCacheProvider cacheProvider)
    {
        const TextDocumentSyncKind syncKind = TextDocumentSyncKind.Incremental;

        _fileTracker = fileTracker;
        _langServer = langServer;
        _logger = logger;
        _cacheProvider = cacheProvider;
        _changeRegistrationOptions = new TextDocumentChangeRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector,
            SyncKind = syncKind
        };
        _openRegistrationOptions = new TextDocumentOpenRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector
        };
        _closeRegistrationOptions = new TextDocumentCloseRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector
        };
        _saveRegistrationOptions = new TextDocumentSaveRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector,
            IncludeText = true // todo: do i need this?
        };
    }

    /// <inheritdoc />
    TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(
        TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return _changeRegistrationOptions;
    }

    /// <inheritdoc />
    TextDocumentOpenRegistrationOptions IRegistration<TextDocumentOpenRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(
        TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return _openRegistrationOptions;
    }

    /// <inheritdoc />
    TextDocumentCloseRegistrationOptions IRegistration<TextDocumentCloseRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(
        TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return _closeRegistrationOptions;
    }

    /// <inheritdoc />
    TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(
        TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
    {
        return _saveRegistrationOptions;
    }

    /// <inheritdoc />
    public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
    {
        return new TextDocumentAttributes(uri, UnturnedAssetFileLspServer.LanguageId);
    }

    /// <inheritdoc />
    public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Received didOpenTextDocument: {0}", request.TextDocument.Uri);
        OpenedFile file = _fileTracker.Files.AddOrUpdate(request.TextDocument.Uri,
            u =>
            {
                OpenedFile file = _fileTracker.CreateFile(u, request.TextDocument.Text);
                file.Version = request.TextDocument.Version;
                return file;
            },
            (_, v) =>
            {
                v.SetFullText(request.TextDocument.Text);
                v.Version = request.TextDocument.Version;
                return v;
            }
        );

        FileAdded?.Invoke(file);
        return Unit.Task;
    }

    /// <inheritdoc />
    public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Received didChangeTextDocument: {0}", request.TextDocument.Uri);

        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
            return Unit.Task;

        try
        {
            file.UpdateText(request, (file, request) =>
            {
                foreach (TextDocumentContentChangeEvent change in request.ContentChanges)
                {
                    if (change.Range == null)
                    {
                        file.SetFullText(change.Text);
                        continue;
                    }

                    if (string.IsNullOrEmpty(change.Text))
                    {
                        file.RemoveText(change.Range.ToFileRange());
                    }
                    else if (change.Range.IsEmpty())
                    {
                        file.InsertText(change.Range.Start.ToFilePosition(), change.Text);
                    }
                    else
                    {
                        file.ReplaceText(change.Range.ToFileRange(), change.Text);
                    }
                }
            });

            file.Version = request.TextDocument.Version;
        }
        catch
        {
            if (file.IsFaulted)
                RequestFileContent(file.Uri);
            throw;
        }

        ContentUpdated?.Invoke(file);
        return Unit.Task;
    }

    private void RequestFileContent(DocumentUri document)
    {
        Task.Run(async () =>
        {
            _logger.LogWarning("Had to request file content for {0}.", document.GetFileSystemPath());
            GetDocumentContentResponse response = await _langServer.General.SendRequest(new GetDocumentContentParams
            {
                Document = document
            }, CancellationToken.None);

            if (response.Text == null)
            {
                _logger.LogError("File not found or couldn't be read.");
            }

            if (_fileTracker.Files.TryGetValue(document, out OpenedFile? file))
            {
                file.SetFullText(response.Text);
                file.Version = response.Version;
                ContentUpdated?.Invoke(file);
                _logger.LogWarning("File text updated.");
            }
            else
            {
                _logger.LogError("File already closed.");
            }
        });
    }

    /// <inheritdoc />
    public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Received didCloseTextDocument: {0}", request.TextDocument.Uri);
        if (_fileTracker.Files.Remove(request.TextDocument.Uri, out OpenedFile? file))
        {
            file.Dispose();
            _cacheProvider.RemoveModel(file.File);
            FileRemoved?.Invoke(file);
        }
        return Unit.Task;
    }

    /// <inheritdoc />
    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        if (request.Text == null)
            return Unit.Task;

        bool added = false;
        //_logger.LogInformation("Received didSaveTextDocument: {0}", request.TextDocument.Uri);
        OpenedFile file = _fileTracker.Files.AddOrUpdate(request.TextDocument.Uri,
            u =>
            {
                OpenedFile v = _fileTracker.CreateFile(u, request.Text);
                if (request.TextDocument is OptionalVersionedTextDocumentIdentifier o)
                    v.Version = o.Version;
                else if (request.TextDocument is VersionedTextDocumentIdentifier t)
                    v.Version = t.Version;
                added = true;
                return v;
            },
            (_, v) =>
            {
                v.SetFullText(request.Text);
                if (request.TextDocument is OptionalVersionedTextDocumentIdentifier o)
                    v.Version = o.Version;
                else if (request.TextDocument is VersionedTextDocumentIdentifier t)
                    v.Version = t.Version;
                added = false;
                return v;
            }
        );
        
        if (added)
            FileAdded?.Invoke(file);
        else
            ContentUpdated?.Invoke(file);
        return Unit.Task;
    }
}

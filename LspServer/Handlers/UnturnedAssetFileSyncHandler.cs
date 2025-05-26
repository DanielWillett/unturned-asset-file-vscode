using DanielWillett.UnturnedDataFileLspServer.Files;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;
internal class UnturnedAssetFileSyncHandler : ITextDocumentSyncHandler
{
    private readonly OpenedFileTracker _fileTracker;

    private readonly TextDocumentChangeRegistrationOptions _changeRegistrationOptions;
    private readonly TextDocumentOpenRegistrationOptions _openRegistrationOptions;
    private readonly TextDocumentCloseRegistrationOptions _closeRegistrationOptions;
    private readonly TextDocumentSaveRegistrationOptions _saveRegistrationOptions;

    public event Action<OpenedFile>? ContentUpdated;

    public UnturnedAssetFileSyncHandler(OpenedFileTracker fileTracker)
    {
        const TextDocumentSyncKind syncKind = TextDocumentSyncKind.Incremental;

        _fileTracker = fileTracker;
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
            u => _fileTracker.CreateFile(u, request.TextDocument.Text),
            (_, v) =>
            {
                //lock (v.EditLock)
                    //v.Text = request.TextDocument.Text;
                return v;
            }
        );

        ContentUpdated?.Invoke(file);
        return Unit.Task;
    }

    /// <inheritdoc />
    public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Received didChangeTextDocument: {0}", request.TextDocument.Uri);

        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
            return Unit.Task;

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

        ContentUpdated?.Invoke(file);
        return Unit.Task;
    }

    /// <inheritdoc />
    public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        //_logger.LogInformation("Received didCloseTextDocument: {0}", request.TextDocument.Uri);
        _fileTracker.Files.Remove(request.TextDocument.Uri, out _);
        return Unit.Task;
    }

    /// <inheritdoc />
    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        if (request.Text == null)
            return Unit.Task;

        //_logger.LogInformation("Received didSaveTextDocument: {0}", request.TextDocument.Uri);
        OpenedFile file = _fileTracker.Files.AddOrUpdate(request.TextDocument.Uri,
            u => _fileTracker.CreateFile(u, request.Text),
            (_, v) =>
            {
                v.SetFullText(request.Text);
                return v;
            }
        );

        ContentUpdated?.Invoke(file);
        return Unit.Task;
    }
}

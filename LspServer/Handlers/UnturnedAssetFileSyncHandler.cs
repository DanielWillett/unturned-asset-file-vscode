using LspServer.Files;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace LspServer.Handlers;
internal class UnturnedAssetFileSyncHandler : ITextDocumentSyncHandler
{
    private readonly ILogger<UnturnedAssetFileSyncHandler> _logger;
    private readonly OpenedFileTracker _fileTracker;

    private readonly TextDocumentChangeRegistrationOptions _changeRegistrationOptions;
    private readonly TextDocumentOpenRegistrationOptions _openRegistrationOptions;
    private readonly TextDocumentCloseRegistrationOptions _closeRegistrationOptions;
    private readonly TextDocumentSaveRegistrationOptions _saveRegistrationOptions;

    public UnturnedAssetFileSyncHandler(ILogger<UnturnedAssetFileSyncHandler> logger, OpenedFileTracker fileTracker)
    {
        const TextDocumentSyncKind syncKind = TextDocumentSyncKind.Full; // todo: change this

        _logger = logger;
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
        _logger.LogInformation("Received didOpenTextDocument: {0}", request.TextDocument.Uri);
        _fileTracker.Files.AddOrUpdate(request.TextDocument.Uri,
            u => new OpenedFile(u, request.TextDocument.Text),
            (_, v) =>
            {
                v.Text = request.TextDocument.Text;
                return v;
            }
        );
        return Unit.Task;
    }

    /// <inheritdoc />
    public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received didChangeTextDocument: {0}", request.TextDocument.Uri);
        string? text = request.ContentChanges.FirstOrDefault(x => x.Range == null)?.Text;
        if (text == null)
            return Unit.Task;

        _fileTracker.Files.AddOrUpdate(request.TextDocument.Uri,
            u => new OpenedFile(u, text),
            (_, v) =>
            {
                v.Text = text;
                return v;
            }
        );
        return Unit.Task;
    }

    /// <inheritdoc />
    public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received didCloseTextDocument: {0}", request.TextDocument.Uri);
        _fileTracker.Files.Remove(request.TextDocument.Uri, out _);
        return Unit.Task;
    }

    /// <inheritdoc />
    public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received didSaveTextDocument: {0}", request.TextDocument.Uri);
        return Unit.Task;
    }
}

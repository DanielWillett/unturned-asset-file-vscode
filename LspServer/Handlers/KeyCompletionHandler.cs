using LspServer.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LspServer.Handlers;

internal class KeyCompletionHandler : ICompletionHandler
{
    private readonly ILogger<KeyCompletionHandler> _logger;
    private readonly CompletionRegistrationOptions _completionRegistrationOptions;
    private readonly OpenedFileTracker _fileTracker;

    private readonly AssetSpecDictionary _specDictionary;

    public KeyCompletionHandler(ILogger<KeyCompletionHandler> logger, OpenedFileTracker fileTracker, AssetSpecDictionary specDictionary)
    {
        _logger = logger;
        _fileTracker = fileTracker;
        _specDictionary = specDictionary;
        _completionRegistrationOptions = new CompletionRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector,
            CompletionItem = new CompletionRegistrationCompletionItemOptions
            {
                LabelDetailsSupport = false
            }
        };
    }

    private CompletionList AutocompleteKey(AssetSpec spec)
    {
        _logger.LogInformation("tkn");
        //ReadOnlySpan<char> textEntered = tokenContent.AsSpan(0, Math.Max(tokenContent.Length, tokenIndex));

        List<CompletionItem> completions = new List<CompletionItem>();
        FindSpecProperties(spec, completions);
        return completions;
    }

    private void FindSpecProperties(AssetSpec spec, List<CompletionItem> completions)
    {
        if (spec.ParentSpec != null)
        {
            FindSpecProperties(spec, completions);
        }

        if (spec.Properties == null)
            return;

        foreach (AssetSpecProperty property in spec.Properties)
        {
            if (property.Key == null)
                continue;

            completions.Add(CreateCompletionItemForKey(property, null));
        }

        foreach (AssetSpecProperty property in spec.Properties)
        {
            if (property.Aliases == null)
                continue;

            foreach (string a in property.Aliases)
            {
                completions.Add(CreateCompletionItemForKey(property, a));
            }
        }
    }

    private static CompletionItem CreateCompletionItemForKey(AssetSpecProperty property, string? alias)
    {
        CompletionItem item = new CompletionItem
        {
            Label = alias ?? property.Key,
            SortText = property.Key,
            InsertTextMode = InsertTextMode.AsIs,
            Kind = CompletionItemKind.Property,
            LabelDetails = new CompletionItemLabelDetails { Description = property.Description, Detail = property.Type },
            Deprecated = property.Deprecated,
            Tags = property.Deprecated ? new Container<CompletionItemTag>(CompletionItemTag.Deprecated) : null,
            Detail = " " + property.Type,
            Documentation =
                property.Markdown != null
                ? new StringOrMarkupContent(new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = property.Markdown
                })
                : property.Description != null ? new StringOrMarkupContent(property.Description) : null,
            InsertText = property.Key.Any(char.IsWhiteSpace) ? "\"" + property.Key + "\"" : property.Key,
            InsertTextFormat = InsertTextFormat.PlainText,
            CommitCharacters = new Container<string>(" ", "\t")
        };

        return item;
    }

    public async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        ++request.Position.Character;
        ++request.Position.Line;

        _logger.LogInformation("Received completion: {0} @ {1}.", request.TextDocument.Uri, request.Position);

        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        {
            _logger.LogInformation("File not found.");
            return new CompletionList();
        }

        AssetFileTree tree = file.Tree;
        _logger.LogInformation("Tree: {0}.", tree.Root);

        AssetFileNode? activeNode = tree.GetNode(request.Position);
        _logger.LogInformation("Active node: {0}.", activeNode);

        string? type = tree.GetType(out bool onlyClrType);
        _logger.LogInformation("Type: {0}.", type);
        AssetSpec? spec = (type == null ? null
                              : await _specDictionary.GetAssetSpecAsync(type, onlyClrType, cancellationToken).ConfigureAwait(false)) ??
                                await _specDictionary.GetAssetSpecAsync("SDG.Unturned.Asset, Assembly-CSharp", true, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Spec: {0}.", spec?.Type);
        if (spec == null)
        {
            return new CompletionList();
        }

        if (activeNode is AssetFileKeyNode || request.Position.Character <= 1)
        {
            return AutocompleteKey(spec);
        }

        return new CompletionList();
    }

    CompletionRegistrationOptions IRegistration<CompletionRegistrationOptions, CompletionCapability>.GetRegistrationOptions(
        CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        return _completionRegistrationOptions;
    }
}

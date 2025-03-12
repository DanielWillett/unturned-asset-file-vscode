using LspServer.Completions;
using LspServer.Files;
using LspServer.Types;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

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

    private void FindSpecProperties(ref KeyCompletionState state, List<CompletionItem> completions)
    {
        if (state.Spec.ParentSpec != null)
        {
            FindSpecProperties(ref state, completions);
        }

        if (state.Spec.Properties == null)
            return;

        foreach (AssetSpecProperty property in state.Spec.Properties)
        {
            if (property.Key == null)
                continue;

            state.Property = property;
            completions.Add(CreateCompletionItemForKey(in state));
        }

        foreach (AssetSpecProperty property in state.Spec.Properties)
        {
            if (property.Aliases == null)
                continue;

            foreach (string a in property.Aliases)
            {
                state.Property = property;
                state.Alias = a;
                completions.Add(CreateCompletionItemForKey(in state));
            }
        }
    }

    private static CompletionItem CreateCompletionItemForKey(in KeyCompletionState state)
    {
        TextEditOrInsertReplaceEdit? edit = null;

        AssetSpecProperty property = state.Property;
        bool needsQuotes = state.Node is AssetFileKeyNode { IsQuoted: true } || property.Key.Any(char.IsWhiteSpace);

        string keyText = needsQuotes ? $"\"{property.Key}\"" : property.Key;
        Position position = state.Position;
        if (state is { IsOnNewLine: true, Node: not AssetFileListValueNode })
        {
            edit = TextEditOrInsertReplaceEdit.From(new TextEdit
            {
                NewText = keyText,
                Range = new Range(position.Line - 1, position.Character - 1, position.Line - 1, position.Character + keyText.Length - 1)
            });
        }
        else
        {
            ReadOnlySpan<char> line = state.File.LineIndex.SliceLine(position.Line);
            int firstNonWhiteSpace = 0;
            for (int i = 0; i < line.Length; ++i)
            {
                if (char.IsWhiteSpace(line[i]))
                    continue;

                firstNonWhiteSpace = i;
                break;
            }

            edit = TextEditOrInsertReplaceEdit.From(new TextEdit
            {
                NewText = keyText + (KnownTypes.IsFlag(property.Type) ? string.Empty : " "),
                Range = new Range(position.Line - 1, firstNonWhiteSpace, position.Line - 1, line.Length)
            });
        }

        CompletionItem item = new CompletionItem
        {
            Label = state.Alias ?? property.Key,
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
            InsertTextFormat = InsertTextFormat.PlainText,
            CommitCharacters = new Container<string>(" ", "\t"),
            TextEdit = edit
        };

        return item;
    }

    public async Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        Position position = request.Position;

        ++position.Character;
        ++position.Line;

        _logger.LogInformation("Received completion: {0} @ {1}.", request.TextDocument.Uri, position);

        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        {
            _logger.LogInformation("File not found.");
            return new CompletionList();
        }

        bool isOnNewLine = file.LineIndex.SliceLine(position.Line, endColumn: position.Character - 1).IsWhiteSpace();

        AssetFileTree tree = file.Tree;
        _logger.LogInformation("Tree: {0}.", tree.Root);

        AssetFileNode? activeNode = tree.GetNode(position);
        _logger.LogInformation("Active node: {0}.", activeNode);

        string? type = tree.GetType(out bool onlyClrType);
        _logger.LogInformation("Type: {0}.", type);
        AssetSpec? spec = (type == null ? null
                              : await _specDictionary.GetAssetSpecAsync(type, onlyClrType, cancellationToken).ConfigureAwait(false)) ??
                                await _specDictionary.GetAssetSpecAsync("SDG.Unturned.Asset, Assembly-CSharp", true, cancellationToken).ConfigureAwait(false);

        AssetInformation information = await _specDictionary.GetAssetInformation(cancellationToken);

        _logger.LogInformation("Spec: {0}.", spec?.Type);
        if (spec == null)
        {
            return new CompletionList();
        }

        if (activeNode is AssetFileKeyNode || isOnNewLine && activeNode is not AssetFileListValueNode)
        {
            KeyCompletionState state = new KeyCompletionState(activeNode, position, isOnNewLine, spec, information, null, null!, file);

            List<CompletionItem> completions = new List<CompletionItem>();
            FindSpecProperties(ref state, completions);
            return completions;
        }

        return new CompletionList();
    }

    CompletionRegistrationOptions IRegistration<CompletionRegistrationOptions, CompletionCapability>.GetRegistrationOptions(
        CompletionCapability capability, ClientCapabilities clientCapabilities)
    {
        return _completionRegistrationOptions;
    }
}

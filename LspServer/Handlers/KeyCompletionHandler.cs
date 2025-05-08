using DanielWillett.UnturnedDataFileLspServer.Completions;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using DanielWillett.UnturnedDataFileLspServer.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;

internal class KeyCompletionHandler : ICompletionHandler
{
    private readonly ILogger<KeyCompletionHandler> _logger;
    private readonly CompletionRegistrationOptions _completionRegistrationOptions;
    private readonly OpenedFileTracker _fileTracker;

    private readonly AssetSpecDatabase _specDictionary;

    public KeyCompletionHandler(ILogger<KeyCompletionHandler> logger, OpenedFileTracker fileTracker, AssetSpecDatabase specDictionary)
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
        InverseTypeHierarchy hierarchy = state.TypeHierarchy;
        for (int i = -1; i < hierarchy.ParentTypes.Length; ++i)
        {
            QualifiedType type = i < 0 ? state.TypeHierarchy.Type : hierarchy.ParentTypes[i];
            if (!_specDictionary.Types.TryGetValue(type, out AssetTypeInformation? info))
                continue;

            state.Alias = null;
            foreach (SpecProperty property in info.Properties)
            {
                if (property.Key == null)
                    continue;

                state.Property = property;
                completions.Add(CreateCompletionItemForKey(in state));
            }

            foreach (SpecProperty property in info.Properties)
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
    }

    private static CompletionItem CreateCompletionItemForKey(in KeyCompletionState state)
    {
        TextEditOrInsertReplaceEdit? edit = null;

        SpecProperty property = state.Property;
        bool needsQuotes = state.Node is AssetFileKeyNode { IsQuoted: true } || property.Key.Any(char.IsWhiteSpace);

        string keyText = needsQuotes ? $"\"{property.Key}\"" : property.Key;
        FilePosition position = state.Position;
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
                NewText = keyText + (property.Type.Equals(KnownTypes.Flag) ? string.Empty : " "),
                Range = new Range(position.Line - 1, firstNonWhiteSpace, position.Line - 1, line.Length)
            });
        }

        CompletionItem item = new CompletionItem
        {
            Label = state.Alias ?? property.Key,
            SortText = property.Key,
            InsertTextMode = InsertTextMode.AsIs,
            Kind = CompletionItemKind.Property,
            LabelDetails = new CompletionItemLabelDetails { Description = property.Description, Detail = property.Type.DisplayName },
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
        _logger.LogInformation("Received completion: {0} @ {1}.", request.TextDocument.Uri, request.Position);

        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        {
            _logger.LogInformation("File not found.");
            return new CompletionList();
        }

        FilePosition position = request.Position.ToFilePosition();
        bool isOnNewLine = file.LineIndex.SliceLine(position.Line + 1, endColumn: position.Character - 1).IsWhiteSpace();

        AssetFileTree tree = file.Tree;

        AssetFileType fileType = AssetFileType.FromFile(file.Tree, _specDictionary);

        AssetFileNode? activeNode = tree.GetNode(position);

        InverseTypeHierarchy hierarchy = _specDictionary.Information.GetParentTypes(fileType.Type);

        AssetFileKeyNode? key = activeNode as AssetFileKeyNode;
        if (key == null && activeNode is AssetFileStringValueNode strValue)
        {
            key = (strValue.Parent as AssetFileKeyValuePairNode)?.Key;
        }

        if (key != null && _specDictionary.FindPropertyInfo(key.Value, fileType, SpecPropertyContext.Property) is { } property)
        {
            if (property.Type is not IAutoCompleteSpecPropertyType autoComplete)
                return new CompletionList();

            AutoCompleteParameters p = new AutoCompleteParameters(_specDictionary, tree, position, fileType, property);

            AutoCompleteResult[] results = await autoComplete.GetAutoCompleteResults(p);
            List<CompletionItem> completions = new List<CompletionItem>(results.Length);
            CompletionItemKind kind = property.Type.GetCompletionItemKind();
            for (int i = 0; i < results.Length; i++)
            {
                ref AutoCompleteResult result = ref results[i];

                CompletionItem item = new CompletionItem
                {
                    Label = result.Text,
                    InsertTextMode = InsertTextMode.AsIs,
                    InsertText = result.Text,
                    Detail = result.Description,
                    Kind = kind
                };
                completions.Add(item);
            }

            return completions;
        }
        
        if (activeNode is AssetFileKeyNode || isOnNewLine && activeNode is not AssetFileListValueNode)
        {
            KeyCompletionState state = new KeyCompletionState(activeNode, position, isOnNewLine, hierarchy, null, null!, file);

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

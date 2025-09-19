using DanielWillett.UnturnedDataFileLspServer.Completions;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
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
    private static readonly Container<string> CommitKeys = new Container<string>("\t");
    private static readonly Container<string> FlagCommitKeys = new Container<string>(" ", "\t");

    private readonly ILogger<KeyCompletionHandler> _logger;
    private readonly CompletionRegistrationOptions _completionRegistrationOptions;
    private readonly OpenedFileTracker _fileTracker;

    private readonly IAssetSpecDatabase _specDictionary;
    private readonly IWorkspaceEnvironment _workspace;
    private readonly InstallationEnvironment _installationEnvironment;

    public KeyCompletionHandler(
        ILogger<KeyCompletionHandler> logger,
        OpenedFileTracker fileTracker,
        IAssetSpecDatabase specDictionary,
        IWorkspaceEnvironment workspace,
        InstallationEnvironment installationEnvironment)
    {
        _logger = logger;
        _fileTracker = fileTracker;
        _specDictionary = specDictionary;
        _workspace = workspace;
        _installationEnvironment = installationEnvironment;
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
        if (!_specDictionary.Types.TryGetValue(state.TypeHierarchy.Type, out AssetSpecType? info))
            return;

        state.Alias = null;
        foreach (SpecProperty property in info.Properties)
        {
            if (property.Key == null || property.IsHidden)
                continue;

            state.Property = property;
            completions.Add(CreateCompletionItemForKey(in state));
        }

        foreach (SpecProperty property in info.Properties)
        {
            if (property.Aliases.IsNull || property.IsHidden)
                continue;

            foreach (string a in property.Aliases)
            {
                state.Property = property;
                state.Alias = a;
                completions.Add(CreateCompletionItemForKey(in state));
            }
        }
    }

    private CompletionItem CreateCompletionItemForKey(in KeyCompletionState state)
    {
        TextEditOrInsertReplaceEdit? edit;

        SpecProperty property = state.Property;
        bool needsQuotes = state.Node is IPropertySourceNode { KeyIsQuoted: true } || property.Key.Any(char.IsWhiteSpace);

        string keyText = needsQuotes ? $"\"{property.Key}\"" : property.Key;
        FilePosition position = state.Position;
        if (state is { IsOnNewLine: true, Node: not IListSourceNode })
        {
            edit = TextEditOrInsertReplaceEdit.From(new TextEdit
            {
                NewText = keyText,
                Range = new Range(position.Line - 1, position.Character - 1, position.Line - 1, position.Character + keyText.Length - 1)
            });
        }
        else
        {
            ReadOnlySpan<char> line = ReadOnlySpan<char>.Empty;// todo: state.File.LineIndex.SliceLine(position.Line);
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

        FileEvaluationContext ctx = new FileEvaluationContext(
            property,
            property.Owner,
            state.File.SourceFile,
            _workspace,
            _installationEnvironment,
            _specDictionary,
            state.File
        );

        ISpecPropertyType? type = property.Type.GetType(in ctx);

        string? description = null, markdown = null;
        property.Description?.TryEvaluateValue(in ctx, out description, out _);
        property.Markdown?.TryEvaluateValue(in ctx, out markdown, out _);

        property.Deprecated.TryEvaluateValue(in ctx, out bool deprecated, out _);

        CompletionItem item = new CompletionItem
        {
            Label = state.Alias ?? property.Key,
            SortText = property.Key,
            InsertTextMode = InsertTextMode.AsIs,
            Kind = CompletionItemKind.Property,
            LabelDetails = new CompletionItemLabelDetails
            {
                Description = description,
                Detail = type != null ? ": " + type.DisplayName : string.Empty
            },
            Deprecated = deprecated,
            Tags = deprecated ? new Container<CompletionItemTag>(CompletionItemTag.Deprecated) : null,
            Detail = type?.DisplayName,
            Documentation =
                !string.IsNullOrEmpty(markdown)
                ? new StringOrMarkupContent(new MarkupContent
                {
                    Kind = MarkupKind.Markdown,
                    Value = markdown
                })
                : !string.IsNullOrEmpty(description) ? new StringOrMarkupContent(description) : null,
            InsertTextFormat = InsertTextFormat.PlainText,
            CommitCharacters = property.Type.Equals(KnownTypes.Flag) ? FlagCommitKeys : CommitKeys,
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

        // todo
        //FileLineIndex lineIndex = default;// file.LineIndex;
        //while (position.Character > 1 && char.IsWhiteSpace(lineIndex.GetChar(position.Line, position.Character - 1)))
        //    --position.Character;

        ReadOnlySpan<char> line = ReadOnlySpan<char>.Empty;// lineIndex.SliceLine(position.Line, endColumn: position.Character - 1);
        bool isOnNewLine = line.IsWhiteSpace();
        
        ISourceFile sourceFile = file.SourceFile;

        AssetFileType fileType = AssetFileType.FromFile(sourceFile, _specDictionary);

        ISourceNode? activeNode = sourceFile.GetNodeFromPosition(position);

        InverseTypeHierarchy hierarchy = _specDictionary.Information.GetParentTypes(fileType.Type);

        IPropertySourceNode? propNode = activeNode as IPropertySourceNode;
        if (propNode == null && activeNode is IValueSourceNode strValue)
        {
            propNode = strValue.Parent as IPropertySourceNode;
        }

        if (propNode != null && _specDictionary.FindPropertyInfo(propNode.Key, fileType, SpecPropertyContext.Property) is { } property && position.Character >= propNode.Range.End.Character)
        {
            FileEvaluationContext ctx = new FileEvaluationContext(
                property,
                fileType.Information,
                sourceFile,
                _workspace,
                _installationEnvironment,
                _specDictionary,
                file
            );

            ISpecPropertyType? type = property.Type.GetType(in ctx);
            if (type is not IAutoCompleteSpecPropertyType autoComplete)
                return new CompletionList();

            AutoCompleteParameters p = new AutoCompleteParameters(
                _specDictionary,
                sourceFile,
                position,
                fileType,
                property,
                _workspace,
                _installationEnvironment,
                file
            );

            AutoCompleteResult[] results = await autoComplete.GetAutoCompleteResults(in p, in ctx);
            List<CompletionItem> completions = new List<CompletionItem>(results.Length);
            CompletionItemKind kind = type.GetCompletionItemKind();
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
        
        if (activeNode is IPropertySourceNode || isOnNewLine && activeNode is not IListSourceNode)
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

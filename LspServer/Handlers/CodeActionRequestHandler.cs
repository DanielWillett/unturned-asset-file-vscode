﻿using System.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Files;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;

internal class CodeActionRequestHandler : CodeActionHandlerBase
{
    private readonly GlobalCodeFixes _codeFixes;
    private readonly IFilePropertyVirtualizer _virtualizer;
    private readonly IAssetSpecDatabase _database;
    private readonly InstallationEnvironment _installEnv;
    private readonly IWorkspaceEnvironment _workspaceEnv;
    private readonly OpenedFileTracker _fileTracker;

    public CodeActionRequestHandler(
        GlobalCodeFixes codeFixes,
        IFilePropertyVirtualizer virtualizer,
        IAssetSpecDatabase database,
        InstallationEnvironment installEnv,
        IWorkspaceEnvironment workspaceEnv,
        OpenedFileTracker fileTracker)
    {
        _codeFixes = codeFixes;
        _virtualizer = virtualizer;
        _database = database;
        _installEnv = installEnv;
        _workspaceEnv = workspaceEnv;
        _fileTracker = fileTracker;
    }

    protected override CodeActionRegistrationOptions CreateRegistrationOptions(
        CodeActionCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new CodeActionRegistrationOptions
        {
            CodeActionKinds = new Container<CodeActionKind>(CodeActionKind.Refactor, CodeActionKind.QuickFix),
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector,
            ResolveProvider = true
        };
    }

    public override Task<CommandOrCodeActionContainer?> Handle(CodeActionParams request, CancellationToken cancellationToken)
    {
        return Task.FromResult<CommandOrCodeActionContainer?>(GetCodeActions(request, cancellationToken));
    }

    private CommandOrCodeActionContainer GetCodeActions(CodeActionParams request, CancellationToken token)
    {
        //Debugger.Launch();
        List<IPerPropertyCodeFix> perPropertyFixes = new List<IPerPropertyCodeFix>((int)(_codeFixes.All.Count * 0.75));
        List<ICodeFix> otherFixes = new List<ICodeFix>((int)(_codeFixes.All.Count * 0.75));

        List<CommandOrCodeAction> actions = new List<CommandOrCodeAction>(64);

        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        {
            return new CommandOrCodeActionContainer();
        }

        Options options = 0;
        CodeActionCapability? capabilities = ClientCapabilities.TextDocument?.CodeAction.Value;
        if (capabilities != null)
        {
            if (capabilities.HonorsChangeAnnotations)
                options |= Options.HonorsChangeAnnotations;
            if (capabilities.DataSupport)
                options |= Options.DataSupport;
            if (capabilities.DisabledSupport)
                options |= Options.DisabledSupport;
            if (capabilities.IsPreferredSupport)
                options |= Options.IsPreferredSupport;
        }

        WorkspaceEditCapability? wsCapabilities = ClientCapabilities.Workspace?.WorkspaceEdit.Value;
        if (wsCapabilities != null)
        {
            if (wsCapabilities.DocumentChanges)
                options |= Options.DocumentChanges;
            if (wsCapabilities.ResourceOperations is not null && wsCapabilities.ResourceOperations.Any())
                options |= Options.ResourceOperations;
        }

        PropertyInclusionFlags inclusionFlags = PropertyInclusionFlags.None;
        foreach (ICodeFix codeFix in _codeFixes.All)
        {
            if (codeFix is IPerPropertyCodeFix perProperty)
            {
                perPropertyFixes.Add(perProperty);
                inclusionFlags |= perProperty.InclusionFlags;
            }
            else
            {
                otherFixes.Add(codeFix);
            }
        }

        OptionalVersionedTextDocumentIdentifier identifier = new OptionalVersionedTextDocumentIdentifier
        {
            Uri = request.TextDocument.Uri,
            Version = (options & Options.DocumentChanges) != 0 ? file.Version : null
        };

        FileUpdateListener listener = new FileUpdateListener(options)
        {
            File = null!,
            Identifier = identifier
        };

        listener.File = new MutableVirtualFile(file, listener);
        try
        {
            if (perPropertyFixes.Count > 0)
            {
                InvokePerPropertyCodeFixesVisitor propertyVisitor = new InvokePerPropertyCodeFixesVisitor(
                    _virtualizer, _database, _installEnv, _workspaceEnv,
                    perPropertyFixes, actions, listener, options, listener.File, inclusionFlags
                )
                {
                    Token = token
                };

                file.SourceFile.Visit(ref propertyVisitor);
            }

            if (otherFixes.Count > 0)
            {
                List<CodeFixInstance> instances = new List<CodeFixInstance>();
                foreach (ICodeFix otherFix in otherFixes)
                {
                    otherFix.GetValidPositions(file.SourceFile, instances);

                    foreach (CodeFixInstance instance in instances)
                    {
                        GetCodeActionFromFix(instance, listener.File, options, listener, instance.Range);
                    }

                    instances.Clear();
                }
            }

            return new CommandOrCodeActionContainer(actions);
        }
        finally
        {
            listener.File.Dispose();
        }
    }

    [Flags]
    private enum Options
    {
        HonorsChangeAnnotations = 1 << 0,
        DataSupport = 1 << 1,
        DisabledSupport = 1 << 2,
        IsPreferredSupport = 1 << 3,
        DocumentChanges = 1 << 4,
        ResourceOperations = 1 << 5
    }

    public override Task<CodeAction> Handle(CodeAction request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    private static CodeAction GetCodeActionFromFix(
        CodeFixInstance instance,
        IMutableWorkspaceFile file,
        Options options,
        FileUpdateListener listener,
        FileRange range)
    {
        ICodeFix codeFix = instance.CodeFix;
        instance.ApplyCodeFix(file);
        return new CodeAction
        {
            Diagnostics = null/*new Container<Diagnostic>(new Diagnostic
            {
                Code = new DiagnosticCode(codeFix.Diagnostic.ErrorId),
                Severity = (DiagnosticSeverity)codeFix.Diagnostic.Severity,
                Range = range.ToRange(),
                Source = UnturnedAssetFileLspServer.DiagnosticSource
            })*/,
            Title = codeFix.GetLocalizedTitle(),
            Edit = listener.GetEditAndReset(),
            // todo
            Kind = CodeActionKind.Refactor
        };
    }

    private class InvokePerPropertyCodeFixesVisitor : ResolvedPropertyNodeVisitor
    {
        private readonly List<IPerPropertyCodeFix> _perPropertyFixes;
        private readonly List<CommandOrCodeAction> _actions;
        private readonly FileUpdateListener _listener;
        private readonly Options _options;
        private readonly IMutableWorkspaceFile _file;

        public InvokePerPropertyCodeFixesVisitor(
            IFilePropertyVirtualizer virtualizer,
            IAssetSpecDatabase database,
            InstallationEnvironment installEnv,
            IWorkspaceEnvironment workspaceEnv,
            List<IPerPropertyCodeFix> perPropertyFixes,
            List<CommandOrCodeAction> actions,
            FileUpdateListener listener,
            Options options,
            IMutableWorkspaceFile file,
            PropertyInclusionFlags flags)
            : base(virtualizer, database, installEnv, workspaceEnv, flags)
        {
            _perPropertyFixes = perPropertyFixes;
            _actions = actions;
            _listener = listener;
            _options = options;
            _file = file;
        }

        protected override void AcceptResolvedProperty(
            SpecProperty property,
            ISpecPropertyType propertyType,
            in SpecPropertyTypeParseContext parseCtx,
            IPropertySourceNode node,
            PropertyBreadcrumbs breadcrumbs)
        {
            foreach (IPerPropertyCodeFix codeFix in _perPropertyFixes)
            {
                if (!node.IsIncluded(codeFix.InclusionFlags))
                    continue;

                if (!codeFix.PassesTypeCheck(propertyType))
                    continue;

                CodeFixInstance? instance = codeFix.TryApplyToProperty(node, propertyType, property, in breadcrumbs, in parseCtx);
                if (instance == null)
                    continue;

                CodeAction? action = GetCodeActionFromFix(instance, _file, _options, _listener, node.GetValueRange());
                if (action != null)
                    _actions.Add(action);
            }
        }
    }

    private class FileUpdateListener : IFileUpdateListener
    {
        private readonly List<TextEdit> _changes;
        private readonly List<WorkspaceEditDocumentChange>? _changeSet;
        private Dictionary<ChangeAnnotationIdentifier, ChangeAnnotation>? _annotations;
        private readonly Dictionary<string, ChangeAnnotationIdentifier>? _identifiers;

        public required OptionalVersionedTextDocumentIdentifier Identifier;
        public required IMutableWorkspaceFile File;

        private readonly bool _legacyChangeSets;

        public FileUpdateListener(Options options)
        {
            _changes = new List<TextEdit>(16);

            _legacyChangeSets = (options & (Options.DocumentChanges | Options.ResourceOperations)) == 0;
            if (!_legacyChangeSets)
            {
                _changeSet = new List<WorkspaceEditDocumentChange>(1);
            }

            if (_legacyChangeSets || (options & Options.HonorsChangeAnnotations) == 0)
                return;

            _annotations = new Dictionary<ChangeAnnotationIdentifier, ChangeAnnotation>(4);
            _identifiers = new Dictionary<string, ChangeAnnotationIdentifier>(4, StringComparer.Ordinal);
        }

        public WorkspaceEdit GetEditAndReset()
        {
            if (!_legacyChangeSets)
            {
                _changeSet!.Add(new WorkspaceEditDocumentChange(new TextDocumentEdit
                {
                    Edits = TextEditContainer.From(_changes),
                    TextDocument = Identifier
                }));
                _changes.Clear();
            }

            WorkspaceEdit edit = new WorkspaceEdit
            {
                ChangeAnnotations = _annotations,
                DocumentChanges = _legacyChangeSets ? null : Container.From(_changeSet),
                Changes = !_legacyChangeSets ? null : new Dictionary<DocumentUri, IEnumerable<TextEdit>>
                {
                    { Identifier.Uri, _changes.ToArray() }
                }
            };

            if (!_legacyChangeSets)
            {
                _changeSet!.Clear();
            }
            else
            {
                _changes.Clear();
            }
            
            if (_annotations != null)
            {
                _annotations = new Dictionary<ChangeAnnotationIdentifier, ChangeAnnotation>();
                _identifiers?.Clear();
            }
            return edit;
        }

        public void RecordInsert(FilePosition position, ReadOnlySpan<char> text, string? annotationId)
        {
            Position p = position.ToPosition();
            SaveChange(new Range(p, p), text.ToString(), annotationId);
        }

        public void RecordRemove(FileRange range, string? annotationId)
        {
            SaveChange(range.ToRange(), string.Empty, annotationId);
        }

        public void RecordReplace(FileRange range, ReadOnlySpan<char> text, string? annotationId)
        {
            SaveChange(range.ToRange(), text.ToString(), annotationId);
        }

        public void RecordFullReplace(ReadOnlySpan<char> text, string? annotationId)
        {
            SaveChange(File.FullRange.ToRange(), text.ToString(), annotationId);
        }

        private void SaveChange(Range range, string text, string? annotationId)
        {
            if (_annotations == null)
                annotationId = null;

            ChangeAnnotationIdentifier? identifier = null;
            if (annotationId != null && !_identifiers!.TryGetValue(annotationId, out identifier))
            {
                _identifiers.Add(annotationId, identifier = annotationId);
            }

            _changes.Add(
                identifier is null
                    ? new TextEdit
                    {
                        NewText = text,
                        Range = range
                    }
                    : new AnnotatedTextEdit
                    {
                        NewText = text,
                        Range = range,
                        AnnotationId = identifier
                    }
            );
        }

        public void RecordNewAnnotation(string annotationId, string label, string? description, bool? needsConfirmation)
        {
            if (_annotations == null)
                return;

            if (!_identifiers!.TryGetValue(annotationId, out ChangeAnnotationIdentifier? identifier))
            {
                _identifiers.Add(annotationId, identifier = annotationId);
            }

            _annotations[identifier] = new ChangeAnnotation
            {
                Label = label,
                Description = description,
                NeedsConfirmation = needsConfirmation ?? false
            };
        }
    }
}

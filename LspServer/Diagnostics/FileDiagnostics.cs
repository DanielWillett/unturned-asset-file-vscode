using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Files;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Diagnostics;

internal class FileDiagnostics : IWorkspaceFile, IDiagnosticSink
{
    private readonly DiagnosticsManager _manager;
    private readonly IAssetSpecDatabase _database;

    public string FilePath { get; private set; }
    public DocumentUri Uri { get; private set; }

    public OpenedFile? OpenedFile { get => _openedFile; private set => _openedFile = value; }

    public int? LatestReclaculateVersion { get; internal set; }

    private List<Diagnostic> _diagnostics;
    private readonly Lock _diagnosticReadLock;
    private string? _localizationAssetFile;
    internal FileTypeInfo TypeInfo;
    private OpenedFile? _openedFile;
    private List<DatDiagnosticMessage> _diagnosticBuffer;

    public ISourceFile? SourceFile { get; set; }

    public FileDiagnostics(string filePath, DocumentUri uri, DiagnosticsManager manager, IAssetSpecDatabase database)
    {
        _manager = manager;
        _database = database;
        UpdateFileName(filePath, uri);
        _diagnostics = new List<Diagnostic>();
        _diagnosticReadLock = new Lock();
        SourceFile = null!;
        _diagnosticBuffer = new List<DatDiagnosticMessage>();
    }

    [MemberNotNull(nameof(FilePath), nameof(Uri))]
    internal void UpdateFileName(string newFileName, DocumentUri newUri)
    {
        FilePath = newFileName;
        TypeInfo = new FileTypeInfo(newFileName);
        Uri = newUri;
    }

    internal void SetOpenedFile(OpenedFile? file)
    {
        using Lock.Scope l = _diagnosticReadLock.EnterScope();

        _openedFile = file;
        SourceFile = file?.SourceFile;
    }
    internal OpenedFile? SetOpenedFile(OpenedFile? file, OpenedFile? comparand)
    {
        using Lock.Scope l = _diagnosticReadLock.EnterScope();

        OpenedFile? oldValue = Interlocked.CompareExchange(ref _openedFile, file, comparand);
        if (ReferenceEquals(oldValue, comparand))
        {
            SourceFile = file?.SourceFile;
        }

        return oldValue;
    }

    public void Clear()
    {
        using Lock.Scope l = _diagnosticReadLock.EnterScope();
        _diagnostics.Clear();
        LatestReclaculateVersion = OpenedFile?.Version;
        _manager.PushDiagnostics(this, new Container<Diagnostic>());
    }

    public void Recalculate()
    {
        using Lock.Scope l = _diagnosticReadLock.EnterScope();
        Lock? enteredLock = null;

        ISourceFile? sourceFile = null;
        bool hasLock = false;
        try
        {
            _diagnosticBuffer.Clear();
            if (OpenedFile != null)
            {
                lock (OpenedFile.EditLock)
                {
                    sourceFile = OpenedFile.SourceFile;
                    Monitor.Enter(sourceFile.TreeSync, ref hasLock);
                    if (OpenedFile.Version.HasValue && LatestReclaculateVersion.HasValue && LatestReclaculateVersion.Value == OpenedFile.Version.Value)
                    {
                        // up to date
                        return;
                    }

                    _diagnosticBuffer.AddRange(OpenedFile.ParseDiagnostics);
                }
            }
            else
            {
                using SourceNodeTokenizer tokenizer = new SourceNodeTokenizer(
                    File.ReadAllText(FilePath),
                    SourceNodeTokenizerOptions.Default | SourceNodeTokenizerOptions.SkipLocalizationInAssets,
                    this
                );

                SourceNodeTokenizer.RootInfo info;
                if (TypeInfo.IsAsset)
                {
                    // read asset file
                    info = SourceNodeTokenizer.RootInfo.Asset(this, _database);
                }
                else if (TypeInfo is { IsLocalization: true, AssetPath: not null })
                {
                    FileDiagnostics assetDiagnostics = _manager.GetOrAddFile(TypeInfo.AssetPath, uri: null);
                    enteredLock = assetDiagnostics._diagnosticReadLock;
                    enteredLock.Enter();

                    ISourceFile? srcFile = assetDiagnostics.SourceFile;
                    bool hasAssetLock = false;
                    try
                    {
                        if (srcFile is not IAssetSourceFile assetSrcFile)
                        {
                            using SourceNodeTokenizer assetTokenizer = new SourceNodeTokenizer(
                                File.ReadAllText(TypeInfo.AssetPath),
                                SourceNodeTokenizerOptions.Default | SourceNodeTokenizerOptions.SkipLocalizationInAssets
                            );
                            SourceNodeTokenizer.RootInfo assetRootInfo = SourceNodeTokenizer.RootInfo.Asset(assetDiagnostics, _database);
                            srcFile = assetTokenizer.ReadRootDictionary(assetRootInfo);
                            TryAddLocalizationFiles(ref srcFile, TypeInfo.AssetPath);
                            assetSrcFile = (IAssetSourceFile)srcFile;
                            assetDiagnostics.SourceFile = assetSrcFile;
                        }
                        else
                        {
                            Monitor.Enter(srcFile.TreeSync, ref hasAssetLock);
                        }

                        info = SourceNodeTokenizer.RootInfo.Localization(this, _database, assetSrcFile);
                    }
                    finally
                    {
                        if (hasAssetLock)
                            Monitor.Exit(srcFile!.TreeSync);
                    }
                }
                else
                {
                    info = SourceNodeTokenizer.RootInfo.Other(this, _database);
                }

                sourceFile = tokenizer.ReadRootDictionary(info);
                TryAddLocalizationFiles(ref sourceFile, FilePath);
                SourceFile = sourceFile;
            }

            DiagnosticsNodeVisitor visitor = new DiagnosticsNodeVisitor(
                _database,
                _manager.Virtualizer,
                _manager.Workspace,
                _manager.InstallEnvironment,
                _diagnosticBuffer
            );

            if (sourceFile is IAssetSourceFile asset)
            {
                asset.GetMetadataDictionary()?.Visit(ref visitor);
                asset.AssetData.Visit(ref visitor);
            }
            else
            {
                sourceFile.Visit(ref visitor);
            }

            _diagnostics.Clear();

            if (_diagnostics.Capacity < _diagnosticBuffer.Count)
                _diagnostics.Capacity = _diagnosticBuffer.Count;

            foreach (DatDiagnosticMessage msg in _diagnosticBuffer)
            {
                _diagnostics.Add(_manager.CreateDiagnostic(msg));
            }

            _diagnosticBuffer.Clear();
        }
        finally
        {
            if (hasLock)
                Monitor.Exit(sourceFile!.TreeSync);
            if (enteredLock != null) enteredLock.Exit();
        }

        LatestReclaculateVersion = OpenedFile?.Version;
        Container<Diagnostic> container = new Container<Diagnostic>(_diagnostics);
        _manager.PushDiagnostics(this, container);
    }

    private void TryAddLocalizationFiles(ref ISourceFile file, string fileName)
    {
        if (file is not IAssetSourceFile assetFile)
            return;

        string? dirName = Path.GetDirectoryName(fileName);
        if (string.IsNullOrEmpty(dirName))
            return;
        ImmutableArray<ILocalizationSourceFile> localizationFiles;
        try
        {
            string[] files = Directory.GetFiles(dirName, "*.dat");
            ImmutableArray<ILocalizationSourceFile>.Builder builder =
                ImmutableArray.CreateBuilder<ILocalizationSourceFile>(files.Length);
            int englishIndex = -1;
            foreach (string localFile in files)
            {
                if (localFile.Equals(fileName, OSPathHelper.PathComparison))
                    continue;

                ReadOnlySpan<char> langName = Path.GetFileNameWithoutExtension(localFile.AsSpan());
                if (langName.IsWhiteSpace() || !char.IsUpper(langName[0]))
                    continue;

                FileDiagnostics localFileDiags = _manager.GetOrAddFile(localFile, null);

                if (localFileDiags.SourceFile is not ILocalizationSourceFile local)
                    continue;

                if (local.LanguageName.Equals("English", StringComparison.Ordinal))
                {
                    englishIndex = builder.Count;
                }

                builder.Add(local);
            }

            if (englishIndex > 0)
                (builder[0], builder[englishIndex]) = (builder[englishIndex], builder[0]);

            localizationFiles = builder.MoveToImmutableOrCopy();
        }
        catch (SystemException)
        {
            localizationFiles = ImmutableArray<ILocalizationSourceFile>.Empty;
        }

        if (assetFile.TryAddLocalization(localizationFiles, out IAssetSourceFile? assetSourceFile))
        {
            file = assetSourceFile;
        }
    }

    void IDiagnosticSink.AcceptDiagnostic(DatDiagnosticMessage diagnostic)
    {

    }

    void IDisposable.Dispose() { }
    string IWorkspaceFile.File => FilePath;
    ISourceFile IWorkspaceFile.SourceFile => SourceFile ?? throw new InvalidOperationException();
    string IWorkspaceFile.GetFullText() => throw new NotSupportedException();


    private class DiagnosticsNodeVisitor : ResolvedPropertyNodeVisitor, IDiagnosticSink
    {
        /// <inheritdoc />
        protected override bool IgnoreMetadata => true;

        public readonly List<DatDiagnosticMessage> Diagnostics;

        public DiagnosticsNodeVisitor(IAssetSpecDatabase database,
            IFilePropertyVirtualizer virtualizer,
            IWorkspaceEnvironment workspace,
            InstallationEnvironment installEnvironment,
            List<DatDiagnosticMessage> diagnostics)
            : base(virtualizer, database, installEnvironment, workspace)
        {
            Diagnostics = diagnostics;
        }

        /// <inheritdoc />
        protected override void AcceptResolvedProperty(
            SpecProperty property,
            ISpecPropertyType propertyType,
            in SpecPropertyTypeParseContext parseCtx,
            IPropertySourceNode node,
            PropertyBreadcrumbs breadcrumbs)
        {
            SpecPropertyTypeParseContext ctx = parseCtx.WithDiagnostics(Diagnostics);
            propertyType.TryParseValue(in ctx, out _);
        }

        /// <inheritdoc />
        public void AcceptDiagnostic(DatDiagnosticMessage diagnostic)
        {
            Diagnostics.Add(diagnostic);
        }
    }
}

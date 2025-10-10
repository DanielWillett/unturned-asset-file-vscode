using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;

/// <summary>
/// Handles specifying the symbol tree for the client.
/// </summary>
internal class DocumentSymbolHandler : IDocumentSymbolHandler
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly ILogger<DocumentSymbolHandler> _logger;

    /// <inheritdoc />
    DocumentSymbolRegistrationOptions IRegistration<DocumentSymbolRegistrationOptions, DocumentSymbolCapability>.GetRegistrationOptions(
        DocumentSymbolCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DocumentSymbolRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector
        };
    }

    public DocumentSymbolHandler(OpenedFileTracker fileTracker, ILogger<DocumentSymbolHandler> logger)
    {
        _fileTracker = fileTracker;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<SymbolInformationOrDocumentSymbolContainer?> Handle(DocumentSymbolParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Document symbol received.");

        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        {
            return Task.FromResult<SymbolInformationOrDocumentSymbolContainer?>(new SymbolInformationOrDocumentSymbolContainer());
        }

        ISourceFile sourceFile = file.SourceFile;

        DocumentSymbolInformationVisitor visitor = new DocumentSymbolInformationVisitor(file);

        if (sourceFile is IAssetSourceFile asset)
        {
            IDictionarySourceNode? assetData = asset.GetAssetDataDictionary();
            IDictionarySourceNode? metadata = asset.GetMetadataDictionary();
            if (metadata != null)
            {
                DocumentSymbolInformationVisitor.SymbolBuilder builder = default;
                builder.Children = new PooledList<DocumentSymbol>();
                visitor.Levels.Push(builder);

                metadata.Visit(ref visitor);

                visitor.Levels.Pop();

                string? metaDetail;
                QualifiedType t = asset.ActualType;
                Guid guid = asset.Guid.GetValueOrDefault();
                if (t.IsNull && guid == Guid.Empty)
                {
                    metaDetail = null;
                }
                else if (!t.IsNull)
                {
                    metaDetail = guid == Guid.Empty ? t.GetTypeName() : $"{t.GetTypeName()} [{guid:N}]";
                }
                else
                {
                    metaDetail = guid.ToString("N");
                }

                Range r = metadata.Range.ToRange();
                DocumentSymbol symbol = new DocumentSymbol
                {
                    Range = r,
                    SelectionRange = new Range(r.Start, new Position(r.Start.Line, r.Start.Character + 1)),
                    Name = "Metadata",
                    Kind = SymbolKind.Class,
                    Detail = metaDetail,
                    Children = builder.GetContainer()
                };

                visitor.Symbols.Add(symbol);
            }

            if (assetData != null)
            {
                DocumentSymbolInformationVisitor.SymbolBuilder builder = default;
                builder.Children = new PooledList<DocumentSymbol>();
                visitor.Levels.Push(builder);

                assetData.Visit(ref visitor);

                visitor.Levels.Pop();

                Range r = assetData.Range.ToRange();
                DocumentSymbol symbol = new DocumentSymbol
                {
                    Range = r,
                    SelectionRange = new Range(r.Start, new Position(r.Start.Line, r.Start.Character + 1)),
                    Name = "Asset",
                    Kind = SymbolKind.Class,
                    Children = builder.GetContainer()
                };

                visitor.Symbols.Add(symbol);
            }
            else
            {
                visitor.Ignore = metadata?.Parent;
                sourceFile.Visit(ref visitor);
            }
        }
        else
        {
            sourceFile.Visit(ref visitor);
        }

        return Task.FromResult<SymbolInformationOrDocumentSymbolContainer?>(visitor.Symbols);
    }

    public class DocumentSymbolInformationVisitor(OpenedFile file) : TopLevelNodeVisitor
    {
        public ISourceNode? Ignore;
        public readonly List<SymbolInformationOrDocumentSymbol> Symbols = new List<SymbolInformationOrDocumentSymbol>(256);

        public readonly Stack<SymbolBuilder> Levels = new Stack<SymbolBuilder>();

        protected override bool IgnoreMetadata => true;

        /// <inheritdoc />
        protected override void AcceptAnyValue(IAnyValueSourceNode node)
        {
            if (Levels.Count == 0)
                return;

            string? valueString = (node as IValueSourceNode)?.Value;
            SymbolKind kind = node.Type switch
            {
                SourceNodeType.List or SourceNodeType.ListWithComment => SymbolKind.Array,
                SourceNodeType.Dictionary or SourceNodeType.DictionaryWithComment => SymbolKind.Object,
                _ => GetValueKind(valueString)
            };

            SymbolBuilder builder = default;

            bool hasChildren = node is IAnyChildrenSourceNode;
            if (hasChildren)
            {
                builder.Children = new PooledList<DocumentSymbol>();

                Levels.Push(builder);
                DocumentSymbolInformationVisitor t = this;
                node.Visit(ref t);
                Levels.Pop();
            }

            FileRange range = node.Range;
            range.End.Character = file.GetLineLength(range.End.Line);

            Range r = range.ToRange();
            Levels.Peek().Children.Add(new DocumentSymbol
            {
                Name = node.Index.ToString(),
                Detail = valueString,
                Kind = kind,
                Range = r,
                SelectionRange = hasChildren
                    ? new Range(r.Start, new Position(r.Start.Line, r.Start.Character + 1))
                    : r,
                Children = builder.GetContainer()
            });
        }

        /// <inheritdoc />
        protected override void AcceptProperty(IPropertySourceNode node)
        {
            if (ReferenceEquals(node, Ignore))
                return;

            SymbolBuilder builder = default;

            SymbolKind kind;
            string? valueString = null;

            FileRange range = node.Range;
            FileRange selectionRange = range;
            ValueTypeDataRefType valueKind = node.ValueKind;
            switch (valueKind)
            {
                case ValueTypeDataRefType.List:
                    kind = SymbolKind.Array;
                    IListSourceNode list = (IListSourceNode)node.Value!;
                    builder.Children = new PooledList<DocumentSymbol>();
                    Levels.Push(builder);
                    DocumentSymbolInformationVisitor t = this;
                    list.Visit(ref t);
                    range.Encapsulate(list.Range);
                    break;

                case ValueTypeDataRefType.Dictionary:
                    kind = SymbolKind.Object;
                    IDictionarySourceNode dict = (IDictionarySourceNode)node.Value!;
                    builder.Children = new PooledList<DocumentSymbol>();
                    Levels.Push(builder);
                    t = this;
                    dict.Visit(ref t);
                    range.Encapsulate(dict.Range);
                    break;

                default:
                    valueString = node.GetValueString(out _);
                    kind = GetValueKind(valueString!);
                    break;
            }

            range.End.Character = file.GetLineLength(range.End.Line);

            DocumentSymbol symbol = new DocumentSymbol
            {
                Name = string.IsNullOrEmpty(node.Key) ? "\"\"" : node.Key,
                Kind = kind,
                Detail = valueString,
                Children = builder.GetContainer(),
                Range = range.ToRange(),
                SelectionRange = selectionRange.ToRange()
            };

            builder.Children?.Dispose();

            if (valueKind is ValueTypeDataRefType.List or ValueTypeDataRefType.Dictionary)
            {
                Levels.Pop();
            }

            if (Levels.Count > 0)
            {
                Levels.Peek().Children.Add(symbol);
            }
            else
            {
                Symbols.Add(symbol);
            }
        }
         
        private static SymbolKind GetValueKind(string? valueString)
        {
            if (string.IsNullOrWhiteSpace(valueString))
                return SymbolKind.Property;
            
            if (valueString.Equals("true", StringComparison.OrdinalIgnoreCase)
                     || valueString.Equals("false", StringComparison.OrdinalIgnoreCase))
                return SymbolKind.Boolean;
            
            if (IsNumber(valueString))
                return SymbolKind.Number;
            
            if (Guid.TryParse(valueString, out _))
                return SymbolKind.Struct;
            
            return SymbolKind.String;
        }

        public struct SymbolBuilder
        {
            public PooledList<DocumentSymbol> Children;

            public readonly Container<DocumentSymbol>? GetContainer()
            {
                return Children == null ? null : new Container<DocumentSymbol>(Children);
            }
        }

        private static bool IsNumber(string str)
        {
            if (str.Length < 1)
                return false;

            int index;
            if (str[0] == '-')
            {
                index = 1;
                if (str.Length < 2)
                    return false;
            }
            else
            {
                index = 0;
            }

            for (; index < str.Length; ++index)
            {
                char c = str[index];
                if (c is ',' or '.' || char.IsDigit(c))
                    continue;

                return false;
            }

            return true;
        }
    }
}
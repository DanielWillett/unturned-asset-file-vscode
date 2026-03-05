using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging.Abstractions;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Handles keeping track of objects and property definitions.
/// </summary>
public class FileRelationalCache : IDiagnosticSink, IFileRelationalModel
{
    private readonly StringDictionary<Entry> _entries;
    private readonly IParsingServices _services;
    private readonly SpecPropertyContext _propertyTypes;

    private int _generation;
    private FileEvaluationContext _evalCtx;
    private List<DatDiagnosticMessage>? _diagnosticBuffer;
    private readonly Stack<ValueProcessor> _valueProcessorPool;

    public ISourceFile SourceFile { get; private set; }

    [MemberNotNullWhen(true, nameof(_diagnosticBuffer))]
    public bool CollectDiagnostics
    {
        get;
        set
        {
            if (field == value)
                return;

            if (value)
            {
                Diagnostics = ImmutableArray<DatDiagnosticMessage>.Empty;
                _diagnosticBuffer = new List<DatDiagnosticMessage>();
                field = value;
            }
            else
            {
                field = false;
                _diagnosticBuffer = null;
                Diagnostics = default;
            }
        }
    }

    public ImmutableArray<DatDiagnosticMessage> Diagnostics { get; private set; }

    public FileRelationalCache(ISourceFile file, bool collectDiagnostics, IParsingServices parsingServices, SpecPropertyContext propertyTypes = SpecPropertyContext.Property)
    {
        if (propertyTypes is not SpecPropertyContext.Property and not SpecPropertyContext.Localization)
            throw new ArgumentOutOfRangeException(nameof(propertyTypes));

        if (collectDiagnostics)
        {
            CollectDiagnostics = true;
        }

        _entries = new StringDictionary<Entry>(keyComparer: StringComparer.OrdinalIgnoreCase);
        _generation = 0;
        SourceFile = file;
        _services = parsingServices;
        _propertyTypes = propertyTypes;
        _valueProcessorPool = new Stack<ValueProcessor>();
        InitEvalCtx();
        Rebuild();
    }

    /// <inheritdoc />
    public void Rebuild(bool force = true)
    {
        ISourceFile file = SourceFile;
        lock (file.TreeSync)
        {
            if (!force && file.FileVersion == _generation)
            {
                return;
            }

            if (!_evalCtx.FileType.Type.Equals(file.ActualType))
            {
                InitEvalCtx();
            }

            RebuildIntl(_evalCtx.FileType.Information, _entries, file);
            _generation = file.FileVersion;
            if (!CollectDiagnostics)
                return;

            Diagnostics = _diagnosticBuffer!.ToImmutableArray();
            _diagnosticBuffer!.Clear();
        }
    }

    public void Rebuild(ISourceFile file)
    {
        lock (_entries)
        {
            SourceFile = file;
            InitEvalCtx();
            if (CollectDiagnostics && _diagnosticBuffer == null)
            {
                _diagnosticBuffer = new List<DatDiagnosticMessage>();
            }

            lock (file.File.TreeSync)
            {
                _generation = file.FileVersion;
                RebuildIntl(_evalCtx.FileType.Information, _entries, file);
            }

            if (_diagnosticBuffer != null)
            {
                Diagnostics = _diagnosticBuffer.ToImmutableArray();
                _diagnosticBuffer!.Clear();
            }
        }
    }

    private bool TryGetPropertyEntryIntl(
        IPropertySourceNode node,
        [NotNullWhen(true)] out Entry? entry
    )
    {
        // assume TreeSync locked
        return _entries.TryGetValue(node.Key, out entry);
    }

    /// <inheritdoc />
    public bool TryGetPropertyFromNode(
        IPropertySourceNode node,
        [NotNullWhen(true)] out DatProperty? property
    )
    {
        lock (SourceFile.TreeSync)
        {
            for (IParentSourceNode? parentNode = node; parentNode != null; parentNode = parentNode.Parent)
            {
                if (parentNode is not IPropertySourceNode propertyNode || !TryGetPropertyEntryIntl(propertyNode, out Entry? entry))
                    continue;

                property = entry.Property;
                if (parentNode == node)
                {
                    return true;
                }

                // todo
                return false;
            }

            property = null;
            return false;
        }
    }

    /// <inheritdoc />
    public bool TryGetPropertyInfoFromNode(
        IPropertySourceNode node,
        out PropertyNodeRelationalInfo info,
        bool includeValue = false
    )
    {
        lock (SourceFile.TreeSync)
        {
            if (!TryGetPropertyEntryIntl(node, out Entry? entry) || entry.ParentNode == null)
            {
                info = default;
                return false;
            }

            info = new PropertyNodeRelationalInfo(
                entry.Property, 
                entry.Node,
                entry.ParentNode,
                includeValue ? entry.CreateValue() : null,
                entry.Type,
                entry.RelatedProperties.UnsafeFreeze(),
                entry.Result
            );
            return true;
        }
    }

    /// <inheritdoc />
    public bool TryVisitPropertyValueFromNode<TVisitor>(
        IPropertySourceNode node,
        ref TVisitor visitor,
        [NotNullWhen(true)] out DatProperty? property,
        TypeParserMissingValueBehavior missingValueBehavior = TypeParserMissingValueBehavior.ErrorIfValueOrPropertyNotProvided
    ) where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        lock (SourceFile.TreeSync)
        {
            if (!TryGetPropertyEntryIntl(node, out Entry? entry))
            {
                property = null;
                return false;
            }

            property = entry.Property;
            entry.Visit(ref visitor);
            return true;
        }
    }

    private ValueProcessor PopValueProcessor(IDictionarySourceNode dictNode, DatTypeWithProperties type, StringDictionary<Entry> root, IPropertySourceNode? ignore = null)
    {
        if (_valueProcessorPool.Count == 0)
            return new ValueProcessor(this, dictNode, ignore, type, root);

        ValueProcessor p = _valueProcessorPool.Pop();
        p.Setup(dictNode, ignore, type, root);
        return p;
    }

    private void ReturnValueProcessor(ValueProcessor p)
    {
        p.Return();
        _valueProcessorPool.Push(p);
    }

    /// <inheritdoc />
    public void AcceptDiagnostic(DatDiagnosticMessage diagnostic)
    {
        _diagnosticBuffer?.Add(diagnostic);
    }

    private void InitEvalCtx()
    {
        _evalCtx = new FileEvaluationContext(_services, SourceFile);
    }

    private void RebuildIntl(DatTypeWithProperties type, StringDictionary<Entry> root, IDictionarySourceNode dictionary)
    {
        IPropertySourceNode? ignore = null;
        if (dictionary.IsRootNode && dictionary is IAssetSourceFile asset)
        {
            IDictionarySourceNode? metadata = asset.GetMetadataDictionary();
            if (metadata != null)
            {
                _evalCtx.RootPosition = AssetDatPropertyPosition.Metadata;
                RebuildIntl(type, root, metadata);
            }

            IDictionarySourceNode? assetData = asset.GetAssetDataDictionary();
            if (assetData != null)
            {
                _evalCtx.RootPosition = AssetDatPropertyPosition.Asset;
                RebuildIntl(type, root, assetData);

                // skip rest of file
                return;
            }

            _evalCtx.RootPosition = AssetDatPropertyPosition.Root;
            ignore = metadata?.Parent as IPropertySourceNode;
        }
        else
        {
            _evalCtx.RootPosition = AssetDatPropertyPosition.Root;
        }

        ValueProcessor p = PopValueProcessor(dictionary, type, root, ignore);
        try
        {
            switch (_propertyTypes)
            {
                case SpecPropertyContext.Localization:
                    if (type is IDatTypeWithLocalizationProperties localProps)
                    {
                        foreach (DatProperty property in localProps.LocalizationProperties)
                        {
                            p.EnqueuePropertyForProcessing(property);
                        }
                    }
                    break;

                case SpecPropertyContext.BundleAsset:
                    if (type is IDatTypeWithBundleAssets bundleAssets)
                    {
                        foreach (DatBundleAsset bundleAsset in bundleAssets.BundleAssets)
                        {
                            p.EnqueuePropertyForProcessing(bundleAsset);
                        }
                    }
                    break;

                default:
                    foreach (DatProperty property in type.Properties)
                    {
                        p.EnqueuePropertyForProcessing(property);
                    }
                    break;
            }

            while (p.TryDequeue(out DatProperty? property))
            {
                p.ProcessProperty(property);
            }

            if (_propertyTypes is SpecPropertyContext.Property or SpecPropertyContext.Localization)
            {
                ImmutableArray<ISourceNode> properties = dictionary.Children;
                foreach (ISourceNode child in properties)
                {
                    if (child is not IPropertySourceNode property || property == ignore)
                        continue;

                    p.CheckPropertyNodeExists(property);
                }
            }
        }
        finally
        {
            ReturnValueProcessor(p);
        }
    }


    internal class Entry
    {
#nullable disable
        public string Key;
        public DatProperty Property;
#nullable restore
        public IPropertySourceNode? Node;
        public IDictionarySourceNode? ParentNode;
        public int Generation;
        //public IValue? OtherValue;
        // todo: public StringDictionary<Entry>? LegacyObjectHead;
        // public ValueProcessor? Processor;
        public bool HasLiteralValue;
        public TypeParserResult Result;
        public IPropertySourceNode[]? RelatedProperties;

        public virtual IType? Type { get; }

        public virtual IValue? CreateValue()
        {
            return null;
        }

        public virtual bool Visit<TVisitor>(ref TVisitor visitor)
            where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
        {
            return false;
        }
    }

    internal class Entry<T> : Entry
        where T : IEquatable<T>
    {
        public Optional<T> Value;
        public IType<T> Type;

        /// <inheritdoc />
        public override IValue? CreateValue()
        {
            if (!HasLiteralValue)
            {
                return null;
            }

            return Type.CreateValue(Value);
        }

        public override bool Visit<TVisitor>(ref TVisitor visitor)
        {
            if (!HasLiteralValue)
            {
                return false;
            }

            visitor.Accept(Value);
            return true;
        }
    }

    internal class ValueProcessor : IReferencedPropertySink
    {
        private readonly FileRelationalCache _parent;
        private readonly Queue<DatProperty> _queue;
        private readonly HashSet<DatProperty> _processed;
        private readonly HashSet<IPropertySourceNode> _referencedProperties;
        private IPropertySourceNode? _currentProperty;
        private bool _currentPropertyWasDereferenced;
        private StringDictionary<Entry>? _entries;

        private IDictionarySourceNode _rootNode;
        private IPropertySourceNode? _ignore;
        private DatTypeWithProperties _type;
        private LegacyExpansionFilter _keyFilter;
        private List<IPropertySourceNode>? _referencedPropertyNodeBufferForThisProperty;

        public ValueProcessor(
            FileRelationalCache parent,
            IDictionarySourceNode dictionary,
            IPropertySourceNode? ignore,
            DatTypeWithProperties type,
            StringDictionary<Entry> root)
        {
            _parent = parent;
            _queue = new Queue<DatProperty>();
            _processed = new HashSet<DatProperty>();
            _referencedProperties = new HashSet<IPropertySourceNode>();
            _keyFilter = LegacyExpansionFilter.Either;
            Setup(dictionary, ignore, type, root);
        }

        [MemberNotNull(nameof(_rootNode))]
        [MemberNotNull(nameof(_type))]
        [MemberNotNull(nameof(_entries))]
        public void Setup(IDictionarySourceNode dictionary, IPropertySourceNode? ignore, DatTypeWithProperties type, StringDictionary<Entry> root)
        {
            _ignore = ignore;
            _rootNode = dictionary;
            _type = type;
            _entries = root;
        }

        public void Return()
        {
            _keyFilter = LegacyExpansionFilter.Either;
            _queue.Clear();
            _referencedProperties.Clear();
            _currentProperty = null;
            _currentPropertyWasDereferenced = false;
            _referencedPropertyNodeBufferForThisProperty?.Clear();
        }

        public bool TryDequeue([NotNullWhen(true)] out DatProperty? property)
        {
            while (true)
            {
                if (_queue.Count == 0)
                {
                    property = null;
                    return false;
                }

                property = _queue.Dequeue();
                if (!_processed.Add(property))
                    continue;

                return true;
            }
        }

        public void EnqueuePropertyForProcessing(DatProperty property)
        {
            _queue.Enqueue(property);
        }

        // invoked after ProcessProperty to identify any unknown properties
        public void CheckPropertyNodeExists(IPropertySourceNode property)
        {
            if (!_parent.CollectDiagnostics || _referencedProperties.Contains(property) || property == _ignore)
                return;

            _parent.AcceptDiagnostic(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1025,
                Message = string.Format(DiagnosticResources.UNT1025, property.Key),
                Range = property.GetFullRange()
            });
        }

        private IPropertySourceNode? FindDirectDescendantPropertyNode(DatProperty property)
        {
            _rootNode.TryResolveProperty(property, ref _parent._evalCtx, out IPropertySourceNode? propertyNode, _keyFilter);
            return propertyNode;
        }

        public void ProcessProperty(DatProperty property)
        {
            if (!property.Type.TryEvaluateType(out IType? propertyType, ref _parent._evalCtx))
            {
                if (_parent.CollectDiagnostics)
                {
                    _parent.AcceptDiagnostic(new DatDiagnosticMessage
                    {
                        Diagnostic = DatDiagnostics.UNT2005,
                        Message = string.Format(DiagnosticResources.UNT2005_Property, property.FullName),
                        Range = _parent.SourceFile.Range
                    });
                }

                return;
            }

            if (property is DatBundleAsset bundleAsset)
            {
                ProcessBundleAsset(bundleAsset, propertyType);
            }

            IPropertySourceNode? propertyNode = FindDirectDescendantPropertyNode(property);
            if (propertyNode == _ignore)
                return;
            
            ValueVisitor v;
            string key = propertyNode == null ? property.Key : propertyNode.Key;
            _entries!.TryGetValue(key, out v.Entry);
            v.NewEntry = null;
            v.Instance = this;
            v.Property = property;
            v.ParentNode = (IParentSourceNode?)propertyNode ?? _rootNode;
            v.ValueNode = propertyNode?.Value;
            v.ParseSuccess = false;

            try
            {
                propertyType.Visit(ref v);

                if (!v.ParseSuccess)
                {
                    return;
                }

                Entry createdEntry = v.NewEntry!;
                createdEntry.Generation = _rootNode.File.FileVersion;
                createdEntry.Key = key;
                createdEntry.Node = propertyNode;
                createdEntry.ParentNode = _rootNode;
                createdEntry.Property = property;

                if (_referencedPropertyNodeBufferForThisProperty is { Count: > 0 })
                {
                    createdEntry.RelatedProperties = _referencedPropertyNodeBufferForThisProperty.ToArray();
                }
                
                _entries[key] = createdEntry;

                if (!_currentPropertyWasDereferenced && propertyNode != null)
                {
                    _referencedProperties.Add(propertyNode);
                }
            }
            catch (Exception ex)
            {
                _parent._services.LoggerFactory
                    .CreateLogger<FileRelationalCache>()
                    .LogError(
                        ex,
                        "Error visiting property {0} in type {1} in file \"{2}\".",
                        property.FullName,
                        _type.TypeName.GetFullTypeName(),
                        _parent.SourceFile.WorkspaceFile.File
                    );
            }
            finally
            {
                _currentPropertyWasDereferenced = false;
                _referencedPropertyNodeBufferForThisProperty?.Clear();
            }
        }

        private void ProcessBundleAsset(DatBundleAsset bundleAsset, IType propertyType)
        {
            // todo: bundle assets
        }

        void IReferencedPropertySink.AcceptReferencedProperty(IPropertySourceNode property)
        {
            if (property.Parent != _rootNode)
                return;

            if (!_referencedProperties.Add(property))
                return;

            _referencedPropertyNodeBufferForThisProperty ??= new List<IPropertySourceNode>(4);
            _referencedPropertyNodeBufferForThisProperty.Add(property);
        }

        void IReferencedPropertySink.AcceptDereferencedProperty(IPropertySourceNode property)
        {
            if (property == _currentProperty)
            {
                _currentPropertyWasDereferenced = true;
            }
        }

        private struct ValueVisitor : ITypeVisitor
        {
            public ValueProcessor Instance;
            public Entry? Entry;
            public Entry? NewEntry;
            public DatProperty Property;
            public IParentSourceNode ParentNode;
            public IAnyValueSourceNode? ValueNode;
            public bool ParseSuccess;

            /// <inheritdoc />
            public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
            {
                FileRelationalCache cache = Instance._parent;
                TypeParserArgs<TValue> args = default;
                args.Type = type;
                args.DiagnosticSink = cache.CollectDiagnostics ? cache : null;
                args.ReferencedPropertySink = Instance;
                args.KeyFilter = Instance._keyFilter;
                args.ParentNode = ParentNode;
                args.ValueNode = ValueNode;
                args.Property = Property;
                args.MissingValueBehavior = TypeParserMissingValueBehavior.FallbackToDefaultValue;

                if (!type.Parser.TryParse(ref args, ref cache._evalCtx, out Optional<TValue> optionalValue))
                {
                    return;
                }

                if (Entry is not Entry<TValue> value)
                {
                    NewEntry = value = new Entry<TValue>();
                }
                else
                {
                    NewEntry = Entry;
                }

                value.Value = optionalValue;
                value.HasLiteralValue = true;
                ParseSuccess = true;
            }
        }
    }
}
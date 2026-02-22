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
using System.Threading;

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

    public bool CollectDiagnostics { get; set; }

    public ImmutableArray<DatDiagnosticMessage> Diagnostics { get; private set; }

    public FileRelationalCache(ISourceFile file, bool collectDiagnostics, IParsingServices parsingServices, SpecPropertyContext propertyTypes = SpecPropertyContext.Property)
    {
        if (propertyTypes is SpecPropertyContext.Property and not SpecPropertyContext.Localization)
            throw new ArgumentOutOfRangeException(nameof(propertyTypes));

        if (collectDiagnostics)
        {
            Diagnostics = ImmutableArray<DatDiagnosticMessage>.Empty;
            CollectDiagnostics = true;
            _diagnosticBuffer = new List<DatDiagnosticMessage>();
        }

        _entries = new StringDictionary<Entry>(keyComparer: StringComparer.OrdinalIgnoreCase);
        _generation = 0;
        SourceFile = file;
        _services = parsingServices;
        _propertyTypes = propertyTypes;
        _valueProcessorPool = new Stack<ValueProcessor>();
        InitEvalCtx();
        lock (file.TreeSync)
        {
            RebuildIntl(_evalCtx.FileType.Information, _entries, file);
            if (collectDiagnostics)
            {
                Diagnostics = _diagnosticBuffer!.ToImmutableArray();
                _diagnosticBuffer!.Clear();
            }
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
                Interlocked.Increment(ref _generation);
                RebuildIntl(_evalCtx.FileType.Information, _entries, file);
            }
            
            if (_diagnosticBuffer != null)
            {
                Diagnostics = _diagnosticBuffer.ToImmutableArray();
                _diagnosticBuffer!.Clear();
            }
        }
    }

    private void InitEvalCtx()
    {
        _evalCtx = new FileEvaluationContext(_services, SourceFile);
    }

    private void RebuildIntl(DatTypeWithProperties type, StringDictionary<Entry> root, IDictionarySourceNode dictionary, bool isMetadata = false)
    {
        IPropertySourceNode? ignore = null;
        if (dictionary.IsRootNode && dictionary is IAssetSourceFile asset)
        {
            IDictionarySourceNode? metadata = asset.GetMetadataDictionary();
            if (metadata != null)
            {
                RebuildIntl(type, root, metadata, isMetadata: true);
            }

            IDictionarySourceNode? assetData = asset.GetAssetDataDictionary();
            if (assetData != null)
            {
                RebuildIntl(type, root, assetData);

                // skip rest of file
                return;
            }

            ignore = metadata?.Parent as IPropertySourceNode;
        }

        ValueProcessor p = PopValueProcessor(dictionary, type, root, ignore);

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

        ReturnValueProcessor(p);
    }


    internal class Entry
    {
#nullable disable
        public string Key;
        public DatProperty Property;
#nullable restore
        public IPropertySourceNode? Node;
        public int Generation;
        public IValue? OtherValue;
        public StringDictionary<Entry>? LegacyObjectHead;
        public ValueProcessor? Processor;
        public bool HasLiteralValue;
        public IPropertySourceNode[]? RelatedProperties;

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
        private PropertyResolutionContext _keyFilter;
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
            _keyFilter = PropertyResolutionContext.Unknown;
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
            _keyFilter = PropertyResolutionContext.Unknown;
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
            _rootNode.TryResolveProperty(property, in _parent._evalCtx, out IPropertySourceNode? propertyNode, _keyFilter);
            return propertyNode;
        }

        public void ProcessProperty(DatProperty property)
        {
            if (!property.Type.TryEvaluateType(out IType? propertyType, in _parent._evalCtx))
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

                createdEntry.Key = key;
                createdEntry.Node = propertyNode;
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

                if (!type.Parser.TryParse(ref args, in cache._evalCtx, out Optional<TValue> optionalValue))
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

    }
}
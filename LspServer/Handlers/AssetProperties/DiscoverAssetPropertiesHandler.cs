using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Protocol;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

internal class DiscoverAssetPropertiesHandler : IDiscoverAssetPropertiesHandler
{
    private static readonly Container<AssetProperty> Empty = new Container<AssetProperty>(Array.Empty<AssetProperty>());

    private readonly OpenedFileTracker _fileTracker;
    private readonly IParsingServices _parsingServices;

    public DiscoverAssetPropertiesHandler(OpenedFileTracker fileTracker, IParsingServices parsingServices)
    {
        _fileTracker = fileTracker;
        _parsingServices = parsingServices;
    }

    public Task<Container<AssetProperty>> Handle(DiscoverAssetPropertiesParams request, CancellationToken cancellationToken)
    {
        if (!_fileTracker.Files.TryGetValue(request.Document, out OpenedFile? file))
        {
            return Task.FromResult(Empty);
        }

        ISourceFile sourceFile = file.SourceFile;

        List<AssetProperty> outputProperties = new List<AssetProperty>(64);

        if (sourceFile is IAssetSourceFile assetFile)
        {
            IDictionarySourceNode? asset = assetFile.GetAssetDataDictionary();
            IDictionarySourceNode? meta = assetFile.GetMetadataDictionary();
            if (meta != null)
                Execute(meta, AssetDatPropertyPosition.Metadata, outputProperties);

            if (asset != null)
                Execute(asset, AssetDatPropertyPosition.Asset, outputProperties);
            else
                Execute(sourceFile, AssetDatPropertyPosition.Root, outputProperties);
        }
        else
        {
            Execute(sourceFile, AssetDatPropertyPosition.Root, outputProperties);
        }

        QualifiedType actualType = sourceFile.ActualType;
        if (outputProperties.Count > 0 && !actualType.IsNull)
        {
            IPropertyOrderFile orderfile = _parsingServices.ProjectFileProvider.GetScaffoldedOrderfile(file);

            IComparer<AssetProperty> comparer = orderfile.CreateComparer<AssetProperty>(
                actualType,
                sourceFile is ILocalizationSourceFile
                    ? SpecPropertyContext.Localization
                    : SpecPropertyContext.Property,
                x => x.Property
            );
            outputProperties.Sort(comparer);
        }

        return Task.FromResult(new Container<AssetProperty>(outputProperties));
    }

    private void Execute(IDictionarySourceNode dictionary, AssetDatPropertyPosition position, List<AssetProperty> outputProperties)
    {
        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, dictionary.File, position);
        DatFileType type = ctx.FileType.Information;

        for (DatTypeWithProperties? childType = type; childType != null; childType = childType.BaseType)
        {
            ImmutableArray<DatProperty> properties = childType.GetPropertyArray(ctx.PropertyContext);
            if (properties.Length == 0)
                continue;

            outputProperties ??= new List<AssetProperty>();

            foreach (DatProperty property in properties)
            {
                if (property is { Type: NullType, AssetPosition: AssetDatPropertyPositionExpectation.Root })
                    continue;

                if (!property.AssetPosition.IsValidPosition(ctx.RootPosition, ctx.FileHasAssetDictionary, ctx.FileHasMetadataDictionary))
                {
                    continue;
                }

                property.Description.TryGetValueAs(ref ctx, out string? desc);
                property.MarkdownDescription.TryGetValueAs(ref ctx, out string? mdDesc);

                AssetProperty prop = new AssetProperty
                {
                    Key = property.Key,
                    Description = desc,
                    Markdown = mdDesc,
                    Property = property
                };

                scoped ValueVisitor v;
                v.Context = ref ctx;
                v.Property = prop;
                v.ParentProperty = property;
                v.Index = -1;

                if (!ctx.TryGetTargetPropertyNodeForProperty(property, out IPropertySourceNode? propertyNode))
                {
                    if (property.DefaultValue != null)
                    {
                        v.Node = null;
                        property.DefaultValue.VisitValue(ref v, ref ctx);
                    }
                }
                else
                {
                    prop.Range = propertyNode.Range.ToRange();
                    v.Node = propertyNode.Value;
                    property.VisitValue(ref v, ref ctx, missingValueBahvior: TypeParserMissingValueBehavior.FallbackToDefaultValue);
                }

                outputProperties.Add(prop);
            }
        }
    }

    private ref struct ValueVisitor : IValueVisitor, IEquatableArrayVisitor, IDictionaryPairVisitor
    {
        public AssetProperty Property;
        public DatProperty? ParentProperty;
        public int Index;
        public ISourceNode? Node;
        public ref FileEvaluationContext Context;
        public void Accept<TValue>(IType<TValue> type, Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            Accept(value);
        }

        private void Accept<TValue>(Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            if (!value.HasValue)
            {
                Property.Value = JValue.CreateNull();
                return;
            }

            QualifiedType qt;
            bool isAlias = false;
            if (typeof(TValue) == typeof(QualifiedType))
            {
                qt = Unsafe.As<Optional<TValue>, Optional<QualifiedType>>(ref value).Value;
            }
            else if (typeof(TValue) == typeof(QualifiedOrAliasedType))
            {
                ref Optional<QualifiedOrAliasedType> alias = ref Unsafe.As<Optional<TValue>, Optional<QualifiedOrAliasedType>>(ref value);
                qt = alias.Value.Type;
                isAlias = alias.Value.IsAlias;
            }
            else
            {
                qt = QualifiedType.None;
            }

            if (!qt.IsNull)
            {
                AssetInformation assetInfo = Context.Services.Database.Information;
                if (isAlias && assetInfo.AssetAliases.TryGetValue(qt.Type, out QualifiedType aliasedType))
                {
                    qt = aliasedType;
                }

                QualifiedType[]? parentTypes = assetInfo.GetParentTypes(qt).ParentTypes;
                if (parentTypes != null)
                {
                    AssetProperty.TypeHierarchyElement[] hierarchy
                        = new AssetProperty.TypeHierarchyElement[parentTypes.Length + 1];

                    hierarchy[0] = ElementFromType(qt);

                    for (int i = 0; i < parentTypes.Length; ++i)
                    {
                        hierarchy[i + 1] = ElementFromType(parentTypes[i]);
                    }

                    Property.TypeHierarchy = hierarchy;
                }
            }

            if (JsonHelper.TryCreateJValue(value.Value, out JValue? jvalue))
            {
                Property.Value = jvalue;
                return;
            }

            if (TypeConverters.TryGet<TValue>() is { } tc)
            {
                TypeConverterFormatArgs f = TypeConverterFormatArgs.Default;
                Property.Value = JValue.CreateString(tc.Format(value.Value!, ref f));
                return;
            }

            switch (value.Value)
            {
                case DatObjectValue obj:
                    VisitObject(obj);
                    break;
                
                case DatEnumValue enumValue:
                    Property.Value = JValue.CreateString(enumValue.Casing);
                    break;

                case IEquatableArray<TValue> arr:
                    arr.Visit(ref this);
                    break;

                case IDictionaryPair<TValue> pair:
                    pair.Visit(ref this);
                    break;

                default:
                    Property.Value = JValue.CreateString(value.Value!.ToString());
                    break;
            }
        }

        private AssetProperty.TypeHierarchyElement ElementFromType(QualifiedType type)
        {
            if (Context.Services.Database.AllTypes.TryGetValue(type, out DatType? dt))
            {
                return new AssetProperty.TypeHierarchyElement
                {
                    DisplayName = dt.DisplayName,
                    Type = dt.TypeName
                };
            }

            return new AssetProperty.TypeHierarchyElement { Type = type.Normalized, DisplayName = type.GetTypeName() };
        }

        private void VisitObject(DatObjectValue obj)
        {
            ImmutableArray<DatObjectPropertyValue> props = obj.Properties;
            DatObjectPropertyValue[] arr = props.UnsafeThaw();

            AssetProperty[] container = new AssetProperty[arr.Length];

            FileEvaluationContext ctx;
            if (Node is IPropertySourceNode || Node?.Parent is IListSourceNode)
            {
                Context.CreateSubContext(out ctx, Node);
            }
            else if (ParentProperty != null)
            {
                Context.CreateSubContext(out ctx, ParentProperty, Index);
            }
            else
            {
                ctx = Context;
            }

            scoped ValueVisitor v;
            v.Context = ref Context;
            v.Index = -1;

            for (int i = 0; i < arr.Length; ++i)
            {
                ref DatObjectPropertyValue pair = ref arr[i];

                pair.Property.Description.TryGetValueAs(ref ctx, out string? desc);
                pair.Property.MarkdownDescription.TryGetValueAs(ref ctx, out string? mdDesc);

                v.Node = pair.Node;
                v.Property = new AssetProperty
                {
                    Key = pair.Property.Key,
                    Description = desc,
                    Markdown = mdDesc,
                    Range = pair.Node?.Parent?.Range.ToRange() ?? Node?.Range.ToRange(),
                    Property = pair.Property
                };
                v.ParentProperty = pair.Property;

                pair.Value.VisitValue(ref v, ref ctx);
                container[i] = v.Property;
            }

            Property.Children = container;
        }

        public void Accept<T>(EquatableArray<T> array)
            where T : IEquatable<T>
        {
            T[] arr = array.Array;

            AssetProperty[] container = new AssetProperty[arr.Length];

            scoped ValueVisitor v;
            v.Context = ref Context;
            v.ParentProperty = ParentProperty;

            for (int i = 0; i < arr.Length; ++i)
            {
                if (Node is not IListSourceNode l || !l.TryGetElement(i, out IAnyValueSourceNode? valueNode))
                    valueNode = null;

                v.Node = valueNode;
                v.Property = new AssetProperty
                {
                    IndexPlusOne = i + 1,
                    Key = string.Empty,
                    Range = valueNode?.Range.ToRange(),
                    Property = ParentProperty!
                };
                v.Index = i;

                v.Accept(new Optional<T>(arr[i]));
                container[i] = v.Property;
            }

            Property.Children = container;
        }

        /// <inheritdoc />
        public void Accept<TElementType>(in DictionaryPair<TElementType> pair)
            where TElementType : IEquatable<TElementType>?
        {
#pragma warning disable CS8631 // Nullabiility mismatch, the Optional<> takes care of it

            Accept(new Optional<TElementType>(pair.Value));

#pragma warning restore CS8631

            if (Property.Value == null)
            {
                Property.Value = new JObject
                {
                    { "key", pair.Key }
                };
                return;
            }

            Property.Value = new JObject
            {
                { "key", pair.Key },
                { "value", Property.Value }
            };
        }
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// A type referenced by a file type.
/// </summary>
public class DatCustomType : DatTypeWithProperties, IType<DatObjectValue>, ITypeParser<DatObjectValue>, IDatTypeWithStringParseableType<DatObjectValue>, IDisposable
{
    internal readonly IDatSpecificationReadContext Context;
    private bool _hasStringParser;
    private bool _stringParserNoFallback;

    internal static readonly ThreadLocal<TypeParserArgs<DatObjectValue>> ValueParseInfo
        = new ThreadLocal<TypeParserArgs<DatObjectValue>>();

    /// <summary>
    /// The null value for this type of object.
    /// </summary>
    [field: MaybeNull]
    public IValue<DatObjectValue> Null => field ??= new NullValue<DatObjectValue>(this);
    
    /// <inheritdoc />
    public override DatSpecificationType Type => DatSpecificationType.Custom;

    /// <inheritdoc />
    private protected override string FullName => $"{Owner.TypeName.GetFullTypeName()}/{TypeName.GetFullTypeName()}";

    /// <inheritdoc />
    public ITypeParser<DatObjectValue> Parser => this;

    /// <inheritdoc />
    public override PropertySearchTrimmingBehavior TrimmingBehavior => PropertySearchTrimmingBehavior.CreatesOtherPropertiesInLinkedFiles;

    /// <inheritdoc />
    public override DatFileType Owner { get; }

    /// <summary>
    /// The default value when parsing a string, if any. 
    /// </summary>
    public IValue<DatObjectValue>? StringDefaultValue { get; internal set; }

    /// <inheritdoc />
    public QualifiedType StringParseableType { get; internal set; }

    internal DatCustomType(QualifiedType type, DatTypeWithProperties? baseType, JsonElement element, DatFileType file, IDatSpecificationReadContext context) : base(type, baseType, element)
    {
        Context = context;
        Owner = file;
    }

    /// <summary>
    /// The parser for this object type. Requires a constructor with the signature <c>ctor(DatCustomType)</c>.
    /// </summary>
    public ITypeConverter<DatObjectValue>? StringParser
    {
        get
        {
            if (_hasStringParser)
                return field;

            _stringParserNoFallback = false;
            QualifiedType stringParseableType = StringParseableType;
            if (stringParseableType.IsNull)
            {
                _hasStringParser = true;
                return null;
            }

            Type? clrType = System.Type.GetType(stringParseableType.Type, throwOnError: false, ignoreCase: true);
            if (clrType == null
                || clrType.GetCustomAttribute(typeof(StringParseableTypeAttribute), false) is not StringParseableTypeAttribute attr
                || !typeof(ITypeConverter<DatObjectValue>).IsAssignableFrom(clrType)
                || clrType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, [typeof(DatCustomType)], null) is not { } ctor)
            {
                _hasStringParser = true;
                Context.LoggerFactory
                    .CreateLogger(TypeName.GetFullTypeName())
                    .LogError(string.Format(Resources.Log_FailedToFindStringParseableType, stringParseableType.Type, TypeName.GetFullTypeName()));
                return null;
            }

            ITypeConverter<DatObjectValue> obj;
            try
            {
                obj = (ITypeConverter<DatObjectValue>)ctor.Invoke([this]);
            }
            catch (Exception ex)
            {
                Context.LoggerFactory
                    .CreateLogger(TypeName.GetFullTypeName())
                    .LogError(ex, string.Format(Resources.Log_FailedToFindStringParseableType, stringParseableType.Type, TypeName.GetFullTypeName()));
                _hasStringParser = true;
                return null;
            }

            ITypeConverter<DatObjectValue>? otherVal = Interlocked.CompareExchange(ref field, obj, null);
            if (otherVal != null && obj is IDisposable disp)
            {
                disp.Dispose();
            }

            _hasStringParser = true;
            _stringParserNoFallback = attr.PreventReadFallback;
            return field;
        }
    }

    /// <inheritdoc />
    public IValue<DatObjectValue> CreateValue(Optional<DatObjectValue> value)
    {
        return value.HasValue ? value.Value : Null;
    }

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<DatObjectValue> args, in FileEvaluationContext ctx, out Optional<DatObjectValue> value)
    {
        value = Optional<DatObjectValue>.Null;

        IDictionarySourceNode? legacyParentDictionary = (args.ParentNode as IPropertySourceNode)?.Parent as IDictionarySourceNode;

        bool maybeModern = args.KeyFilter != PropertyResolutionContext.Legacy;
        bool maybeLegacy = args.KeyFilter != PropertyResolutionContext.Modern;
        maybeLegacy &= legacyParentDictionary != null;
        switch (args.ValueNode)
        {
            default:
                if (maybeModern && !maybeLegacy)
                {
                    if (args.MissingValueBehavior != TypeParserMissingValueBehavior.FallbackToDefaultValue)
                    {
                        args.DiagnosticSink?.UNT2004_NoDictionary(ref args, args.ParentNode);
                    }
                    else
                    {
                        if (args.Property?.GetIncludedDefaultValue(args.ParentNode is IPropertySourceNode) is { } defValue)
                        {
                            return defValue.TryGetValueAs(in ctx, out value);
                        }

                        return false;
                    }
                }

                return maybeLegacy && TryParseLegacyObject(ref args, in ctx, out value, legacyParentDictionary!);

            case IValueSourceNode valueNode:
                if (StringParser is { } stringParser)
                {
                    args.CreateTypeConverterParseArgs(out TypeConverterParseArgs<DatObjectValue> parseArgs, valueNode.Value);
                    if (stringParser.TryParse(valueNode.Value, ref parseArgs, out DatObjectValue? stringParsedValue))
                    {
                        value = stringParsedValue;
                        return true;
                    }
                    if (_stringParserNoFallback)
                    {
                        return false;
                    }
                }

                if (StringDefaultValue != null)
                {
                    ValueParseInfo.Value = args;
                    try
                    {
                        if (StringDefaultValue.TryEvaluateValue(out value, in ctx))
                        {
                            return true;
                        }

                        args.DiagnosticSink?.UNT2004_Generic(ref args, valueNode.Value, Owner);
                        return false;

                    }
                    finally
                    {
                        ValueParseInfo.Value = default;
                    }
                }

                if (!maybeLegacy)
                {
                    if (maybeModern)
                        args.DiagnosticSink?.UNT2004_ValueInsteadOfDictionary(ref args, valueNode, this);
                    else
                        args.DiagnosticSink?.UNT2004_Generic(ref args, valueNode.Value, Owner);

                    return false;
                }

                return TryParseLegacyObject(ref args, in ctx, out value, legacyParentDictionary!);

            case IListSourceNode listNode:
                if (maybeModern)
                    args.DiagnosticSink?.UNT2004_ListInsteadOfDictionary(ref args, listNode, this);
                else
                    args.DiagnosticSink?.UNT2004_Generic(ref args, $"[ n = {listNode.Count} ]", Owner);
                return false;

            case IDictionarySourceNode dictNode:
                if (!maybeModern)
                {
                    args.DiagnosticSink?.UNT2004_LegacyFormatExpected(ref args, dictNode);
                    return false;
                }

                return TryParseModernObject(ref args, in ctx, out value, dictNode);

        }
    }

    private bool TryParseModernObject(ref TypeParserArgs<DatObjectValue> args, in FileEvaluationContext ctx, out Optional<DatObjectValue> value, IDictionarySourceNode dictionary)
    {
        // todo
        value = Optional<DatObjectValue>.Null;
        return false;
    }

    private bool TryParseLegacyObject(ref TypeParserArgs<DatObjectValue> args, in FileEvaluationContext ctx, out Optional<DatObjectValue> value, IDictionarySourceNode dictionary)
    {
        // todo
        value = Optional<DatObjectValue>.Null;
        return false;
    }

    public override void Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.Accept(this);
    }

    #region JSON

    /// <inheritdoc />
    public bool TryReadValueFromJson<TDataRefReadContext>(
        in JsonElement json,
        out Optional<DatObjectValue> value,
        IType<DatObjectValue> valueType,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        if (!StringParseableType.IsNull && StringParser is { } stringParser)
        {
            TypeConverterParseArgs<DatObjectValue> parseArgs = new TypeConverterParseArgs<DatObjectValue>(this);
            if (stringParser.TryReadJson(in json, out value, ref parseArgs))
                return true;
        }

        if (json.ValueKind == JsonValueKind.Null)
        {
            value = Optional<DatObjectValue>.Null;
            return true;
        }

        if (json.ValueKind != JsonValueKind.Object)
        {
            value = Optional<DatObjectValue>.Null;
            return false;
        }

        int capacity = Properties.Length;

        DatCustomAssetType? assetType = this as DatCustomAssetType;
        if (assetType != null
            && json.TryGetProperty("$udat-localization"u8, out JsonElement localizationProperties)
            && localizationProperties.ValueKind == JsonValueKind.Object)
        {
            capacity += assetType.LocalizationProperties.Length;
        }
        else
        {
            localizationProperties = default;
        }

        ImmutableArray<DatObjectPropertyValue>.Builder propertyArrayBuilder = ImmutableArray.CreateBuilder<DatObjectPropertyValue>(capacity);

        value = Optional<DatObjectValue>.Null;
        if (!TryReadAllProperties(Properties, in json, propertyArrayBuilder, ref dataRefContext))
        {
            return false;
        }

        if (localizationProperties.ValueKind == JsonValueKind.Object
            && !TryReadAllProperties(assetType!.LocalizationProperties, in localizationProperties, propertyArrayBuilder, ref dataRefContext))
        {
            return false;
        }

        return true;
    }

    private bool TryReadAllProperties<TDataRefReadContext>(
        ImmutableArray<DatProperty> properties,
        in JsonElement root,
        ImmutableArray<DatObjectPropertyValue>.Builder propertyArrayBuilder,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        foreach (DatProperty property in properties)
        {
            if (!root.TryGetProperty(property.Key, out JsonElement element))
                continue;

            IValue? value;
            if (!property.Type.TryGetConcreteType(out IType? type))
            {
                if (typeof(TDataRefReadContext) == typeof(DataRefs.NilDataRefContext)
                    || dataRefContext == null)
                {
                    value = new UnresolvedNonConcreteTypeObjectPropertyValue(element, property.Type, property);
                }
                else
                {
                    value = new UnresolvedNonConcreteTypeObjectPropertyValue<TDataRefReadContext>(element, property.Type, property, dataRefContext);
                }
            }
            else
            {
                value = Value.TryReadValueFromJson(in root, ValueReadOptions.Default, type, Context.Database, property, ref dataRefContext);
                if (value == null)
                    return false;
            }

            propertyArrayBuilder.Add(new DatObjectPropertyValue(value, property));
        }

        return true;
    }

    /// <inheritdoc />
    public void WriteValueToJson(Utf8JsonWriter writer, DatObjectValue value, IType<DatObjectValue> valueType, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("$udat-type"u8, TypeName.Type);

        bool mayHaveLocals = value.Type is DatCustomAssetType;

        ImmutableArray<DatObjectPropertyValue> properties = value.Properties;
        foreach (DatObjectPropertyValue val in properties)
        {
            if (mayHaveLocals && val.Property.Context != SpecPropertyContext.Property)
                continue;

            writer.WritePropertyName(val.Property.Key);
            val.Value.WriteToJson(writer, options);
        }

        if (mayHaveLocals)
        {
            bool any = false;
            foreach (DatObjectPropertyValue val in properties)
            {
                if (val.Property.Context == SpecPropertyContext.Localization)
                    continue;

                if (!any)
                {
                    any = true;
                    writer.WriteStartObject("$udat-localization");
                }
                writer.WritePropertyName(val.Property.Key);
                val.Value.WriteToJson(writer, options);
            }

            if (any)
                writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        if (_hasStringParser && StringParser is IDisposable disp)
            disp.Dispose();
    }

    private class UnresolvedNonConcreteTypeObjectPropertyValue : IValue
    {
        protected readonly JsonElement Element;
        protected readonly IPropertyType PropertyType;
        protected readonly DatProperty Owner;

        public UnresolvedNonConcreteTypeObjectPropertyValue(JsonElement element, IPropertyType propertyType, DatProperty owner)
        {
            Element = element;
            PropertyType = propertyType;
            Owner = owner;
        }

        public virtual bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
            where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
        {
            IValue? value = Value.TryReadValueFromJson(in Element, ValueReadOptions.Default, PropertyType, ctx.Services.Database, Owner);
            return value != null && value.VisitValue(ref visitor, in ctx);
        }

        bool IValue.IsNull => false;
        void IValue.WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options) => Element.WriteTo(writer);
        bool IEquatable<IValue?>.Equals(IValue? other) => (object)this == other;
        bool IValue.VisitConcreteValue<TVisitor>(ref TVisitor visitor) => false;
    }

    private class UnresolvedNonConcreteTypeObjectPropertyValue<TDataRefReadContext> : UnresolvedNonConcreteTypeObjectPropertyValue
        where TDataRefReadContext : IDataRefReadContext?
    {
        private readonly TDataRefReadContext _context;

        public UnresolvedNonConcreteTypeObjectPropertyValue(
            JsonElement element,
            IPropertyType propertyType,
            DatProperty owner,
            TDataRefReadContext context
        ) : base(element, propertyType, owner)
        {
            _context = context;
        }

        public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        {
            TDataRefReadContext context = _context;
            IValue? value = Value.TryReadValueFromJson(in Element, ValueReadOptions.Default, PropertyType, ctx.Services.Database, Owner, ref context);
            return value != null && value.VisitValue(ref visitor, in ctx);
        }
    }
}

/// <summary>
/// A type referenced by an asset file type.
/// </summary>
public class DatCustomAssetType : DatCustomType, IDatTypeWithLocalizationProperties, IDatTypeWithBundleAssets
{
    internal ImmutableArray<DatProperty>.Builder? LocalizationPropertiesBuilder { get; set; }
    
    internal ImmutableArray<DatBundleAsset>.Builder? BundleAssetsBuilder { get; set; }

    /// <inheritdoc />
    public override DatSpecificationType Type => DatSpecificationType.CustomAsset;

    /// <inheritdoc />
    public ImmutableArray<DatProperty> LocalizationProperties { get; internal set; }

    /// <inheritdoc />
    public ImmutableArray<DatBundleAsset> BundleAssets { get; internal set; }

    internal DatCustomAssetType(QualifiedType type, DatTypeWithProperties? baseType, JsonElement element, DatAssetFileType file, IDatSpecificationReadContext context)
        : base(type, baseType, element, file, context)
    {
        LocalizationProperties = ImmutableArray<DatProperty>.Empty;
        BundleAssets = ImmutableArray<DatBundleAsset>.Empty;
    }

    ImmutableArray<DatProperty>.Builder? IDatTypeWithLocalizationProperties.LocalizationPropertiesBuilder
        => LocalizationPropertiesBuilder;
    ImmutableArray<DatBundleAsset>.Builder? IDatTypeWithBundleAssets.BundleAssetsBuilder
        => BundleAssetsBuilder;
}
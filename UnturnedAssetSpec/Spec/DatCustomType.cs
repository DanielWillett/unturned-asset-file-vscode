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
public class DatCustomType : DatTypeWithProperties, IType<DatObjectValue>, ITypeParser<DatObjectValue>, IDisposable
{
    private readonly IDatSpecificationReadContext _context;
    private bool _hasStringParser;

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

    internal DatCustomType(QualifiedType type, DatTypeWithProperties? baseType, JsonElement element, DatFileType file, IDatSpecificationReadContext context) : base(type, baseType, element)
    {
        _context = context;
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

            QualifiedType stringParseableType = StringParseableType;
            if (stringParseableType.IsNull)
            {
                _hasStringParser = true;
                return null;
            }

            Type? clrType = System.Type.GetType(stringParseableType.Type, throwOnError: false, ignoreCase: true);
            if (clrType == null
                || !clrType.IsDefined(typeof(StringParseableTypeAttribute), false)
                || !typeof(ITypeConverter<DatObjectValue>).IsAssignableFrom(clrType)
                || clrType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, [typeof(DatCustomType)], null) is not { } ctor)
            {
                _hasStringParser = true;
                _context.LoggerFactory
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
                _context.LoggerFactory
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
            else
            {
                _hasStringParser = true;
            }

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

    #region JSON

    /// <inheritdoc />
    public bool TryReadValueFromJson(in JsonElement json, out Optional<DatObjectValue> value, IType<DatObjectValue> valueType)
    {
        if (!StringParseableType.IsNull && StringParser is { } stringParser)
        {
            TypeConverterParseArgs<DatObjectValue> parseArgs = new TypeConverterParseArgs<DatObjectValue>(this);
            if (stringParser.TryReadJson(in json, out value, ref parseArgs))
                return true;
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
        if (!TryReadAllProperties(Properties, in json, propertyArrayBuilder))
        {
            return false;
        }

        if (localizationProperties.ValueKind == JsonValueKind.Object
            && !TryReadAllProperties(assetType!.LocalizationProperties, in localizationProperties, propertyArrayBuilder))
        {
            return false;
        }

        return true;
    }

    private bool TryReadAllProperties(ImmutableArray<DatProperty> properties, in JsonElement root, ImmutableArray<DatObjectPropertyValue>.Builder propertyArrayBuilder)
    {
        foreach (DatProperty property in properties)
        {
            if (!root.TryGetProperty(property.Key, out JsonElement element))
                continue;

            IValue? value;
            if (!property.Type.TryGetConcreteType(out IType? type))
            {
                value = new UnresolvedNonConcreteTypeObjectPropertyValue(element, property.Type, property);
            }
            else
            {
                value = Value.TryReadValueFromJson(in root, ValueReadOptions.Default, type, _context.Database, property);
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
        private readonly JsonElement _element;
        private readonly IPropertyType _propertyType;
        private readonly DatProperty _owner;

        public UnresolvedNonConcreteTypeObjectPropertyValue(JsonElement element, IPropertyType propertyType, DatProperty owner)
        {
            _element = element;
            _propertyType = propertyType;
            _owner = owner;
        }

        public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx) where TVisitor : IValueVisitor
        {
            IValue? value = Value.TryReadValueFromJson(in _element, ValueReadOptions.Default, _propertyType, ctx.Services.Database, _owner);
            return value != null && value.VisitValue(ref visitor, in ctx);
        }

        bool IValue.IsNull => false;
        void IValue.WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options) => _element.WriteTo(writer);
        bool IEquatable<IValue?>.Equals(IValue? other) => (object)this == other;
        bool IValue.VisitConcreteValue<TVisitor>(ref TVisitor visitor) => false;
    }
}

/// <summary>
/// A type referenced by an asset file type.
/// </summary>
public class DatCustomAssetType : DatCustomType, IDatTypeWithLocalizationProperties, IDatTypeWithBundleAssets
{
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
}
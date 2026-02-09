using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An Orderfile defines the ideal order of properties in asset files.
/// </summary>
[SpecificationType(FactoryMethod = nameof(Create))]
#if NET5_0_OR_GREATER
[System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.NonPublicMethods)]
#endif
public sealed class Orderfile : DatFileType
{
    /// <summary>
    /// Link to the documentation for Orderfiles.
    /// </summary>
    internal const string? OrderfileDocs = null; // TODO

    internal Orderfile(IAssetSpecDatabase database)
        : base(new QualifiedType("DanielWillett.UnturnedDataFileLspServer.Data.Types.Orderfile, UnturnedAssetSpec", isCaseInsensitive: true), null, default)
    {
        DisplayNameIntl = Resources.Type_Name_Orderfile;
        Docs = OrderfileDocs;
        IsProjectFile = true;

        if (database.IsInitialized)
        {
            GenerateProperties(database, NullLoggerFactory.Instance);
        }
        else
        {
            database.OnInitialize(GenerateProperties);
        }
    }

    /// <summary>
    /// Factory method for the <see cref="Orderfile"/> type.
    /// </summary>
    private static Orderfile Create(in SpecificationTypeFactoryArgs args)
    {
        return FromDatabase(args.Context.Database);
    }

    /// <summary>
    /// Automatically generates an orderfile type for a given database.
    /// </summary>
    /// <remarks>If the database hasn't been initialized yet the properties of the returned object may not be filled out yet.</remarks>
    public static Orderfile FromDatabase(IAssetSpecDatabase database)
    {
        return new Orderfile(database);
    }

    // set up auto-generated auto-complete for the Orderfile file type
    private Task GenerateProperties(IAssetSpecDatabase database, ILoggerFactory loggerFactory)
    {
        ImmutableArray<DatProperty>.Builder properties = ImmutableArray.CreateBuilder<DatProperty>(database.FileTypes.Count);
        int firstCustomTypeIndex = 0;

        // skip this type if it's in there for some reason
        HashSet<QualifiedType> processed = [ TypeName ];

        foreach (DatFileType type in database.FileTypes.Values.OrderByDescending(x => x is not DatAssetFileType))
        {
            AddDatFileType(database, type, properties, processed, ref firstCustomTypeIndex);
        }

        Properties = properties.MoveToImmutableOrCopy();
        return Task.CompletedTask;
    }

    private void AddDatFileType(IAssetSpecDatabase database, DatType type, ImmutableArray<DatProperty>.Builder properties, HashSet<QualifiedType> processed, ref int firstCustomTypeIndex)
    {
        QualifiedType typeName = type.TypeName;

        if (processed.Contains(typeName))
            return;

        if (type.BaseType != null)
        {
            AddDatFileType(database, type.BaseType, properties, processed, ref firstCustomTypeIndex);
        }

        bool isFile = type is DatFileType;

        if (HasLocalizationProperties(type))
        {
            OrderfileListElementType elementType = new OrderfileListElementType(database, type, true);
            string key = PropertyReference.CreateContextSpecifier(SpecPropertyContext.Localization) + typeName.Type;
            DatProperty property = DatProperty.Create(key, ListType.Create(elementType), this, default, SpecPropertyContext.Property);
            if (isFile)
            {
                properties.Insert(firstCustomTypeIndex, property);
                ++firstCustomTypeIndex;
            }
            else
            {
                properties.Add(property);
            }

            if (type is IDatTypeWithLocalizationProperties lcl)
            {
                foreach (DatProperty existingProperty in lcl.LocalizationProperties)
                {
                    AddReferencedType(database, existingProperty.Type, properties, processed, ref firstCustomTypeIndex);
                }
            }
        }

        if (type is DatTypeWithProperties propertyType)
        {
            OrderfileListElementType elementType = new OrderfileListElementType(database, propertyType, false);
            string key = PropertyReference.CreateContextSpecifier(SpecPropertyContext.Localization) + typeName.Type;
            DatProperty property = DatProperty.Create(key, ListType.Create(elementType), this, default, SpecPropertyContext.Property);
            if (isFile)
            {
                properties.Insert(firstCustomTypeIndex, property);
                ++firstCustomTypeIndex;
            }
            else
            {
                properties.Add(property);
            }

            foreach (DatProperty existingProperty in propertyType.Properties)
            {
                AddReferencedType(database, existingProperty.Type, properties, processed, ref firstCustomTypeIndex);
            }
        }
    }

    private void AddReferencedType(IAssetSpecDatabase database, IPropertyType type, ImmutableArray<DatProperty>.Builder properties, HashSet<QualifiedType> processed, ref int firstCustomTypeIndex)
    {
        if (type is DatType dt)
        {
            AddDatFileType(database, dt, properties, processed, ref firstCustomTypeIndex);
        }
        else if (type is IReferencingType refType)
        {
            OneOrMore<IType> referencedTypes = refType.ReferencedTypes;
            foreach (IType referencedType in referencedTypes)
            {
                AddReferencedType(database, referencedType, properties, processed, ref firstCustomTypeIndex);
            }
        }
    }

    private static bool HasLocalizationProperties(DatType type)
    {
        if (type is IDatTypeWithLocalizationProperties { LocalizationProperties.IsDefaultOrEmpty: false })
            return true;

        if (type is not DatTypeWithProperties propertyType)
            return false;

        foreach (DatProperty property in propertyType.Properties)
        {
            if (property.Type is DatType dt && HasLocalizationProperties(dt))
            {
                return true;
            }

            if (property.Type is IReferencingType refType && refType.ReferencedTypes.Any(x => x is DatType dt && HasLocalizationProperties(dt)))
            {
                return true;
            }
        }

        return false;
    }
}

internal sealed class OrderfileListElementType : BaseType<string, OrderfileListElementType>, ITypeParser<string>
{
    private readonly IAssetSpecDatabase _database;
    private readonly DatType _type;
    private readonly bool _isLocalization;

    private ImmutableArray<DatProperty> RelevantProperties => _isLocalization
        ? _type is IDatTypeWithLocalizationProperties lcl ? lcl.LocalizationProperties : ImmutableArray<DatProperty>.Empty
        : _type is DatTypeWithProperties props ? props.Properties : ImmutableArray<DatProperty>.Empty;

    private ImmutableArray<DatProperty>.Builder? RelevantPropertiesFallback => _isLocalization
        ? _type is IDatTypeWithLocalizationProperties lcl ? lcl.LocalizationPropertiesBuilder : null
        : _type is DatTypeWithProperties props ? props.PropertiesBuilder : null;

    /// <inheritdoc />
    public override string Id => "DanielWillett.UnturnedDataFileLspServer.Data.Types.OrderfileListElementType, UnturnedAssetSpec";

    /// <inheritdoc />
    public override string DisplayName => Resources.Type_Name_OrderfilePropertyReference;

    /// <inheritdoc />
    public override ITypeParser<string> Parser => this;

    internal OrderfileListElementType(IAssetSpecDatabase database, DatType type, bool isLocalization)
    {
        _database = database;
        _type = type ?? throw new ArgumentNullException(nameof(type));
        _isLocalization = isLocalization;
    }

    /// <inheritdoc />
    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options) { WriteTypeName(writer); }

    /// <inheritdoc />
    protected override bool Equals(OrderfileListElementType other)
    {
        return other._type.Equals(_type) && other._isLocalization == _isLocalization;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(422463129, _type.TypeName, _isLocalization);
    }

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<string> args, in FileEvaluationContext ctx, out Optional<string> value)
    {
        if (TypeParsers.TryApplyMissingValueBehavior(ref args, in ctx, out value, out bool rtn))
        {
            return rtn;
        }

        if (!TypeParsers.String.TryParse(ref args, in ctx, out value))
        {
            return false;
        }

        if (args.DiagnosticSink == null)
        {
            return true;
        }

        IValueSourceNode valueNode = (IValueSourceNode)args.ValueNode!;
        string strValue = valueNode.Value;
        if (strValue is [ '_' ])
        {
            return true;
        }

        DatProperty? property = null;
        if (strValue.Length > 0 && strValue[0] == '@')
        {
            ReadOnlySpan<char> propRefIntl;
            if (strValue.Length > 1 && strValue[1] == '(' && strValue[^1] == ')')
                propRefIntl = strValue.AsSpan(2, strValue.Length - 3);
            else
                propRefIntl = strValue.AsSpan(1);

            strValue = StringHelper.Unescape(propRefIntl);
            PropertyReference pRef = PropertyReference.Parse(strValue);
            for (DatTypeWithProperties? baseType = _type.BaseType; baseType != null && property == null; baseType = baseType.BaseType)
            {
                property = FindProperty(
                    baseType,
                    _isLocalization
                        ? baseType is IDatTypeWithLocalizationProperties lcl ? lcl.LocalizationProperties : ImmutableArray<DatProperty>.Empty
                        : baseType.Properties,
                    _isLocalization
                        ? baseType is IDatTypeWithLocalizationProperties lcl2 ? lcl2.LocalizationPropertiesBuilder : null
                        : baseType.PropertiesBuilder,
                    in pRef
                );
            }
        }
        else if (strValue.Length > 0 && strValue[0] == '$')
        {
            ReadOnlySpan<char> typeNameIntl;
            if (strValue.Length > 1 && strValue[1] == '(' && strValue[^1] == ')')
                typeNameIntl = strValue.AsSpan(2, strValue.Length - 3);
            else
                typeNameIntl = strValue.AsSpan(1);

            strValue = StringHelper.Unescape(typeNameIntl);
            QualifiedType type = new QualifiedType(strValue, isCaseInsensitive: true);
            if (!_database.TryFindType(type, out DatType? dt, _type))
            {
                args.DiagnosticSink?.UPROJ2002(ref args, strValue);
            }
            else if (!type.IsNormalized || !string.Equals(type.Type, dt.TypeName.Type, StringComparison.Ordinal))
            {
                args.DiagnosticSink?.UPROJ1002(ref args, strValue, dt.TypeName.Type);
            }

            return true;
        }
        else
        {
            strValue = StringHelper.Unescape(strValue);
            PropertyReference propRef = new PropertyReference(SpecPropertyContext.Unspecified, null, strValue);
            property = FindProperty(_type, RelevantProperties, RelevantPropertiesFallback, in propRef);
        }

        if (property == null)
        {
            args.DiagnosticSink?.UPROJ2001(ref args, strValue);
        }
        else if (!string.Equals(strValue, property.Key, StringComparison.Ordinal))
        {
            args.DiagnosticSink?.UPROJ1001(ref args, strValue, property.Key);
        }

        return true;
    }

    private static DatProperty? FindProperty(DatType type, ImmutableArray<DatProperty> properties, ImmutableArray<DatProperty>.Builder? fallbackProperties, in PropertyReference propertyReference)
    {
        if (propertyReference.TypeName != null && !type.TypeName.Equals(propertyReference.TypeName))
        {
            return null;
        }

        string propName = propertyReference.PropertyName;
        foreach (DatProperty property in properties)
        {
            if (property.Key.Equals(propName, StringComparison.OrdinalIgnoreCase))
                return property;
        }

        if (fallbackProperties != null)
        {
            foreach (DatProperty property in fallbackProperties)
            {
                if (property.Key.Equals(propName, StringComparison.OrdinalIgnoreCase))
                    return property;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public bool TryReadValueFromJson(in JsonElement json, out Optional<string> value, IType<string> valueType)
    {
        return TypeParsers.String.TryReadValueFromJson(in json, out value, valueType);
    }

    /// <inheritdoc />
    public void WriteValueToJson(Utf8JsonWriter writer, string value, IType<string> valueType, JsonSerializerOptions options)
    {
        TypeParsers.String.WriteValueToJson(writer, value, valueType, options);
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public class PropertyRef : IEquatable<PropertyRef>, ISpecDynamicValue
{
    private readonly PropertyRefInfo _info;

    private SpecProperty? _property;
    private PropertyRef? _crossReferenceProperty;
    private bool _hasCache;

    public SpecPropertyContext Context => _info.Context;
    public string PropertyName => _info.PropertyName;
    public QualifiedType Type => _info.Type;

    public ISpecPropertyType? ValueType => _property?.Type.Type;

    public PropertyRef(PropertyRefInfo info)
    {
        _info = info;
    }

    public PropertyRef(ReadOnlySpan<char> propertyName, string? originalString)
    {
        _info = new PropertyRefInfo(propertyName, originalString);
    }

    public bool Equals(PropertyRef? other) => other != null && string.Equals(PropertyName, other.PropertyName, StringComparison.Ordinal) && Context == other.Context && Type.Equals(other.Type);

    public override bool Equals(object? obj) => obj is PropertyRef r && Equals(r);

    public override int GetHashCode() => PropertyName == null ? 0 : PropertyName.GetHashCode();

    public ISpecDynamicValue? GetValue(in FileEvaluationContext ctx)
    {
        SpecProperty? prop = ResolveProperty(in ctx);
        if (prop == null)
        {
            return null;
        }

        return _crossReferenceProperty != null
            ? _info.GetValue(in ctx, prop, _crossReferenceProperty._info, _crossReferenceProperty.ResolveProperty(in ctx))
            : _info.GetValue(in ctx, prop, default, null);
    }

    public bool GetIsIncluded(bool valueIncluded, in FileEvaluationContext ctx)
    {
        SpecProperty? prop = ResolveProperty(in ctx);
        return _crossReferenceProperty != null
            ? _info.GetIsIncluded(valueIncluded, in ctx, prop, _crossReferenceProperty._info, _crossReferenceProperty.ResolveProperty(in ctx))
            : _info.GetIsIncluded(valueIncluded, in ctx, prop, default, null);
    }

    public bool GetIsLegacy(in FileEvaluationContext ctx)
    {
        SpecProperty? prop = ResolveProperty(in ctx);
        if (_crossReferenceProperty != null)
            throw new InvalidOperationException("Cross-reference not supported.");

        return PropertyRefInfo.EvaluateIsLegacy(prop, in ctx);
    }

    public ValueTypeDataRefType GetValueType(in FileEvaluationContext ctx)
    {
        SpecProperty? prop = ResolveProperty(in ctx);
        if (_crossReferenceProperty != null)
            throw new InvalidOperationException("Cross-reference not supported.");

        return PropertyRefInfo.EvaluateValueType(prop, in ctx);
    }

    public SpecProperty? ResolveProperty(in FileEvaluationContext ctx)
    {
        if (_hasCache)
        {
            return _property;
        }

        _info.ResolveProperty(in ctx, out _property, out PropertyRefInfo crossRefInfo);
        if (crossRefInfo.PropertyName != null)
        {
            _crossReferenceProperty = new PropertyRef(_info);
        }

        _hasCache = true;
        return _property;
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Operation is ConditionOperation.Included or ConditionOperation.ValueIncluded)
        {
            return condition.Invert(GetIsIncluded(condition.Operation == ConditionOperation.Included, in ctx));
        }

        if (condition.Operation == ConditionOperation.Excluded)
        {
            return condition.Invert(!GetIsIncluded(false, in ctx));
        }

        ISpecDynamicValue? val = GetValue(in ctx);
        return val?.EvaluateCondition(in ctx, in condition) ?? condition.EvaluateNulls(true, condition.Comparand == null);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        ISpecDynamicValue? val = GetValue(in ctx);
        if (val != null)
        {
            return val.TryEvaluateValue(in ctx, out value, out isNull);
        }

        isNull = false;
        value = default;
        return false;
    }
    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        ISpecDynamicValue? val = GetValue(in ctx);
        if (val != null)
        {
            return val.TryEvaluateValue(in ctx, out value);
        }

        value = null;
        return false;
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        string str = _info.ToString();
        if (StringHelper.ContainsWhitespace(str))
        {
            str = "@(" + str + ")";
        }
        else
        {
            str = "@" + str;
        }

        writer.WriteStringValue(str);
    }

    public override string ToString() => _info.PropertyName;
}

public readonly struct PropertyRefInfo
{
    public SpecPropertyContext Context { get; }
    public string PropertyName { get; }
    public QualifiedType Type { get; }

    public PropertyRefInfo(SpecProperty property)
    {
        PropertyName = property.Key;
        Context = SpecPropertyContext.Unspecified;
        Type = property.Owner.Type;
    }

    public override string ToString()
    {
        return Context switch
        {
            SpecPropertyContext.Property => "$prop$::",
            SpecPropertyContext.Localization => "$local$::",
            SpecPropertyContext.CrossReferenceLocalization => "$cr.local$::",
            SpecPropertyContext.CrossReferenceProperty => "$cr.prop$::",
            SpecPropertyContext.CrossReferenceUnspecified => "$cr$::",
            SpecPropertyContext.BundleAsset => "$bndl$::",
            _ => string.Empty
        } + PropertyName;
    }

    public PropertyRefInfo(ReadOnlySpan<char> propertyName, string? originalString)
    {
        if (propertyName == null)
            throw new ArgumentNullException(nameof(propertyName));

        if (propertyName.StartsWith("$local$::".AsSpan(), StringComparison.Ordinal) && propertyName.Length > 9)
        {
            Context = SpecPropertyContext.Localization;
            propertyName = propertyName.Slice(9);
        }
        else if (propertyName.StartsWith("$prop$::".AsSpan(), StringComparison.Ordinal) && propertyName.Length > 8)
        {
            Context = SpecPropertyContext.Property;
            propertyName = propertyName.Slice(8);
        }
        else if (propertyName.StartsWith("$cr.local$::".AsSpan(), StringComparison.Ordinal) && propertyName.Length > 12)
        {
            Context = SpecPropertyContext.CrossReferenceLocalization;
            propertyName = propertyName.Slice(12);
        }
        else if (propertyName.StartsWith("$cr.prop$::".AsSpan(), StringComparison.Ordinal) && propertyName.Length > 11)
        {
            Context = SpecPropertyContext.CrossReferenceProperty;
            propertyName = propertyName.Slice(11);
        }
        else if (propertyName.StartsWith("$cr$::".AsSpan(), StringComparison.Ordinal) && propertyName.Length > 6)
        {
            Context = SpecPropertyContext.CrossReferenceUnspecified;
            propertyName = propertyName.Slice(6);
        }
        else if (propertyName.StartsWith("$bndl$::".AsSpan(), StringComparison.Ordinal) && propertyName.Length > 6)
        {
            Context = SpecPropertyContext.BundleAsset;
            propertyName = propertyName.Slice(8);
        }
        else
        {
            Context = SpecPropertyContext.Unspecified;
        }

        if (propertyName.Length >= 4)
        {
            int splitIndex = propertyName.IndexOf("::".AsSpan(), StringComparison.Ordinal);
            if (splitIndex > 0 && splitIndex < propertyName.Length - 2)
            {
                Type = new QualifiedType(propertyName.Slice(0, splitIndex).ToString());
                propertyName = propertyName.Slice(splitIndex + 2);
            }
        }

        PropertyName = originalString != null && propertyName.Length == originalString.Length ? originalString : propertyName.ToString();
    }

    public void ResolveProperty(in FileEvaluationContext ctx, out SpecProperty? property, out PropertyRefInfo crossRefProperty)
    {
        ISpecType? type = ctx.This;
        if (!Type.IsNull)
        {
            ISpecType? mappedType = ctx.Information.FindType(Type.Type, ctx.FileType);
            if (mappedType != null)
            {
                type = mappedType;
            }
        }

        if (type == null)
        {
            property = null;
            crossRefProperty = default;
            return;
        }

        SpecPropertyContext context = Context;
        bool crossReference = false;
        if (context == SpecPropertyContext.CrossReferenceUnspecified)
        {
            crossReference = true;
            context = SpecPropertyContext.Unspecified;
        }
        else if (context == SpecPropertyContext.CrossReferenceLocalization)
        {
            crossReference = true;
            context = SpecPropertyContext.Localization;
        }
        else if (context == SpecPropertyContext.CrossReferenceProperty)
        {
            crossReference = true;
            context = SpecPropertyContext.Property;
        }

        property = type.FindProperty(PropertyName, context);

        if (!crossReference || string.IsNullOrWhiteSpace(ctx.Self.FileCrossRef))
        {
            crossRefProperty = default;
            return;
        }

        crossRefProperty = new PropertyRefInfo(ctx.Self.FileCrossRef.AsSpan(), ctx.Self.FileCrossRef);
    }

    internal static bool EvaluateIsIncluded(SpecProperty? property, bool valueIncluded, in FileEvaluationContext context)
    {
        if (property != null && context.SourceFile.TryGetProperty(property, out IPropertySourceNode? prop2, context.PropertyContext))
        {
            if (!valueIncluded)
                return true;

            if (!prop2.HasValue)
                return false;

            if (prop2 is { ValueKind: ValueTypeDataRefType.Value, Value: IValueSourceNode strValNode })
            {
                if (string.IsNullOrWhiteSpace(strValNode.Value) && !strValNode.IsQuoted)
                    return false;
            }

            return true;
        }

        return false;
    }

    internal static bool EvaluateIsLegacy(SpecProperty? property, in FileEvaluationContext context)
    {
        if (property?.Owner is not CustomSpecType { IsLegacyExpandedType: true } t)
            return false;

        // todo
        return false;
    }

    internal static ValueTypeDataRefType EvaluateValueType(SpecProperty? property, in FileEvaluationContext context)
    {
        if (property != null && context.SourceFile.TryResolveProperty(property, out IPropertySourceNode? node, context.PropertyContext))
        {
            return node.ValueKind;
        }

        return ValueTypeDataRefType.Value;
    }

    public bool GetIsIncluded(bool valueIncluded, in FileEvaluationContext ctx, SpecProperty? prop, PropertyRefInfo crossReferencePropertyInfo, SpecProperty? crossReferenceProperty)
    {
        if (prop == null)
        {
            return false;
        }

        if (crossReferencePropertyInfo.PropertyName != null)
        {
            return CrossReferenceIsIncluded(valueIncluded, in ctx, crossReferencePropertyInfo, crossReferenceProperty);
        }

        return EvaluateIsIncluded(prop, valueIncluded, in ctx);
    }

    public ISpecDynamicValue? GetValue(in FileEvaluationContext ctx, SpecProperty? prop, PropertyRefInfo crossReferencePropertyInfo, SpecProperty? crossReferenceProperty)
    {
        if (prop == null)
        {
            return null;
        }

        if (crossReferencePropertyInfo.PropertyName != null)
        {
            return CrossReference(in ctx, crossReferencePropertyInfo, crossReferenceProperty);
        }

        if (!ctx.SourceFile.TryGetProperty(prop, out IPropertySourceNode? node, ctx.PropertyContext))
        {
            return prop.DefaultValue;
        }

        SpecPropertyTypeParseContext parse = SpecPropertyTypeParseContext.FromFileEvaluationContext(ctx, prop, node, node.Value);

        if (prop.Type.TryParseValue(in parse, out ISpecDynamicValue? value))
        {
            return value;
        }

        return prop.IncludedDefaultValue ?? prop.DefaultValue;
    }

    private ISpecDynamicValue? CrossReference(in FileEvaluationContext ctx, PropertyRefInfo crossReferencePropertyInfo, SpecProperty? crossReferenceProperty)
    {
        if (crossReferenceProperty == null)
        {
            crossReferencePropertyInfo.ResolveProperty(in ctx, out crossReferenceProperty, out _);
        }

        if (crossReferenceProperty == null)
        {
            return null;
        }

        FileEvaluationContext crossRefCtx = new FileEvaluationContext(in ctx, crossReferenceProperty, PropertyResolutionContext.Modern);

        ISpecDynamicValue? crossRef = crossReferencePropertyInfo.GetValue(in crossRefCtx, crossReferenceProperty, default, null);
        if (crossRef == null)
        {
            return null;
        }

        OneOrMore<DiscoveredDatFile> referenceFile;
        if (crossRef.TryEvaluateValue(in ctx, out Guid guid, out bool isNull) && !isNull && guid != Guid.Empty)
        {
            referenceFile = ctx.Environment.FindFile(guid);
        }
        else if (crossRef.TryEvaluateValue(in ctx, out ushort id, out isNull) && !isNull && id != 0
                 && crossRef is IElementTypeSpecPropertyType elementType
                 && AssetCategory.TryParse(elementType.ElementType, out EnumSpecTypeValue assetCategory))
        {
            referenceFile = ctx.Environment.FindFile(id, assetCategory);
        }
        else if (crossRef.ValueType is GuidOrIdSpecPropertyType &&
                 crossRef.TryEvaluateValue(in ctx, out GuidOrId guidOrId, out isNull) && !isNull &&
                 !guidOrId.IsNull)
        {
            if (guidOrId.Guid != Guid.Empty)
            {
                referenceFile = ctx.Environment.FindFile(guidOrId.Guid);
            }
            else if (crossRef is IElementTypeSpecPropertyType elementType2
                     && AssetCategory.TryParse(elementType2.ElementType, out assetCategory))
            {
                referenceFile = ctx.Environment.FindFile(guidOrId.Id, assetCategory);
            }
            else
            {
                referenceFile = default;
            }
        }
        else
        {
            referenceFile = default;
        }

        DiscoveredDatFile? file = referenceFile.FirstOrDefault();
        if (file == null)
        {
            return null;
        }

        using IWorkspaceFile? workspaceFile = ctx.Workspace.TemporarilyGetOrLoadFile(file);
        if (workspaceFile == null)
        {
            return null;
        }

        PropertyRefInfo crossValueInfo = new PropertyRefInfo(PropertyName.AsSpan(), PropertyName);

        FileEvaluationContext crossValueCtx = new FileEvaluationContext(in ctx, workspaceFile);

        crossValueInfo.ResolveProperty(in crossValueCtx, out SpecProperty? crossedProperty, out PropertyRefInfo crossCrossPropertyInfo);

        // can't nest cross references
        if (crossCrossPropertyInfo.PropertyName != null || crossedProperty == null)
        {
            return null;
        }

        crossValueCtx = new FileEvaluationContext(in crossValueCtx, crossedProperty, PropertyResolutionContext.Modern);

        return crossValueInfo.GetValue(in crossValueCtx, crossedProperty, default, null);
    }

    private bool CrossReferenceIsIncluded(bool valueIncluded, in FileEvaluationContext ctx, PropertyRefInfo crossReferencePropertyInfo, SpecProperty? crossReferenceProperty)
    {
        if (crossReferenceProperty == null)
        {
            crossReferencePropertyInfo.ResolveProperty(in ctx, out crossReferenceProperty, out _);
        }

        if (crossReferenceProperty == null)
        {
            return false;
        }

        FileEvaluationContext crossRefCtx = new FileEvaluationContext(in ctx, crossReferenceProperty, PropertyResolutionContext.Modern);

        ISpecDynamicValue? crossRef = crossReferencePropertyInfo.GetValue(in crossRefCtx, crossReferenceProperty, default, null);
        if (crossRef == null)
        {
            return false;
        }

        OneOrMore<DiscoveredDatFile> referenceFile;
        if (crossRef.TryEvaluateValue(in ctx, out Guid guid, out bool isNull) && !isNull && guid != Guid.Empty)
        {
            referenceFile = ctx.Environment.FindFile(guid);
        }
        else if (crossRef.TryEvaluateValue(in ctx, out ushort id, out isNull) && !isNull && id != 0
                 && crossRef is IElementTypeSpecPropertyType elementType
                 && AssetCategory.TryParse(elementType.ElementType, out EnumSpecTypeValue assetCategory))
        {
            referenceFile = ctx.Environment.FindFile(id, assetCategory);
        }
        else if (crossRef.ValueType is GuidOrIdSpecPropertyType &&
                 crossRef.TryEvaluateValue(in ctx, out GuidOrId guidOrId, out isNull) && !isNull &&
                 !guidOrId.IsNull)
        {
            if (guidOrId.Guid != Guid.Empty)
            {
                referenceFile = ctx.Environment.FindFile(guidOrId.Guid);
            }
            else if (crossRef is IElementTypeSpecPropertyType elementType2
                     && AssetCategory.TryParse(elementType2.ElementType, out assetCategory))
            {
                referenceFile = ctx.Environment.FindFile(guidOrId.Id, assetCategory);
            }
            else
            {
                referenceFile = default;
            }
        }
        else
        {
            referenceFile = default;
        }

        DiscoveredDatFile? file = referenceFile.FirstOrDefault();
        if (file == null)
        {
            return false;
        }

        using IWorkspaceFile? workspaceFile = ctx.Workspace.TemporarilyGetOrLoadFile(file);
        if (workspaceFile == null)
        {
            return false;
        }

        PropertyRefInfo crossValueInfo = new PropertyRefInfo(PropertyName.AsSpan(), PropertyName);

        FileEvaluationContext crossValueCtx = new FileEvaluationContext(in ctx, workspaceFile);

        crossValueInfo.ResolveProperty(in crossValueCtx, out SpecProperty? crossedProperty, out PropertyRefInfo crossCrossPropertyInfo);

        // can't nest cross references
        if (crossCrossPropertyInfo.PropertyName != null || crossedProperty == null)
        {
            return false;
        }

        crossValueCtx = new FileEvaluationContext(in crossValueCtx, crossedProperty, PropertyResolutionContext.Modern);

        return crossValueInfo.GetIsIncluded(valueIncluded, in crossValueCtx, crossedProperty, default, null);
    }
}
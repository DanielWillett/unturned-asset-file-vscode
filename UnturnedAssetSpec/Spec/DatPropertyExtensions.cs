using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

public static class DatPropertyExtensions
{
    /// <summary>
    /// Attempts to find a property by its key in <paramref name="type"/> or any of its child types.
    /// </summary>
    /// <param name="type">The type to search.</param>
    /// <param name="key">The exact key of the property. This method does not match aliases.</param>
    /// <param name="bundleAsset">The found bundle asset.</param>
    /// <returns>Whether or not the bundle asset was found.</returns>
    public static bool TryFindBundleAsset(this DatTypeWithProperties type, ReadOnlySpan<char> key, [NotNullWhen(true)] out DatBundleAsset? bundleAsset)
    {
        if (type.TryFindProperty(key, SpecPropertyContext.BundleAsset, out DatProperty? property))
        {
            bundleAsset = (DatBundleAsset)property;
            return true;
        }

        bundleAsset = null;
        return false;
    }

    /// <summary>
    /// Attempts to find a property or bundle asset by its key in <paramref name="type"/> or any of its parent types.
    /// </summary>
    /// <param name="type">The type to search.</param>
    /// <param name="key">The exact key of the property. This method does not match aliases.</param>
    /// <param name="context">The context to search for the property. Must be <see cref="SpecPropertyContext.Property"/>, <see cref="SpecPropertyContext.Localization"/>, or <see cref="SpecPropertyContext.BundleAsset"/>.</param>
    /// <param name="property">The found property or bundle asset.</param>
    /// <returns>Whether or not the property was found.</returns>
    public static bool TryFindProperty(this DatTypeWithProperties type, ReadOnlySpan<char> key, SpecPropertyContext context, [NotNullWhen(true)] out DatProperty? property)
    {
        int propListCode = context switch
        {
            SpecPropertyContext.Property => 0,
            SpecPropertyContext.Localization => 1,
            SpecPropertyContext.BundleAsset => 2,
            _ => throw new InvalidEnumArgumentException(nameof(context), (int)context, typeof(SpecPropertyContext))
        };

        for (DatTypeWithProperties? parent = type; parent != null; parent = parent.BaseType)
        {
            ImmutableArray<DatProperty> propertyList;
            switch (propListCode)
            {
                case 0:
                    propertyList = parent.Properties;
                    break;

                case 1:
                    if (parent is not IDatTypeWithLocalizationProperties locals)
                        continue;

                    propertyList = locals.LocalizationProperties;
                    break;

                default: // case 2:
                    if (parent is not IDatTypeWithBundleAssets bundles)
                        continue;

                    foreach (DatBundleAsset prop in bundles.BundleAssets)
                    {
                        if (!key.Equals(prop.Key, StringComparison.OrdinalIgnoreCase))
                            continue;

                        property = prop;
                        return true;
                    }
                    goto fallback;
            }

            foreach (DatProperty prop in propertyList)
            {
                if (!key.Equals(prop.Key, StringComparison.OrdinalIgnoreCase))
                    continue;

                property = prop;
                return true;
            }

            fallback:
            ImmutableArray<DatProperty>.Builder? propertyListFallback;
            switch (propListCode)
            {
                case 0:
                    propertyListFallback = parent.PropertiesBuilder;
                    break;

                case 1:
                    if (parent is not IDatTypeWithLocalizationProperties locals)
                        continue;

                    propertyListFallback = locals.LocalizationPropertiesBuilder;
                    break;

                default: // case 2:
                    if (parent is not IDatTypeWithBundleAssets { BundleAssetsBuilder: { } fallback })
                        continue;

                    foreach (DatBundleAsset prop in fallback)
                    {
                        if (!key.Equals(prop.Key, StringComparison.OrdinalIgnoreCase))
                            continue;

                        property = prop;
                        return true;
                    }
                    continue;
            }

            if (propertyListFallback != null)
            {
                foreach (DatProperty prop in propertyListFallback)
                {
                    if (!key.Equals(prop.Key, StringComparison.OrdinalIgnoreCase))
                        continue;

                    property = prop;
                    return true;
                }
            }
        }

        property = null;
        return false;
    }

    /// <summary>
    /// Attempts to find a property by its key or aliases in <paramref name="type"/> or any of its parent types.
    /// </summary>
    /// <param name="type">The type to search.</param>
    /// <param name="key">The exact key of the property. This method does not match aliases.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="property">The found property.</param>
    /// <param name="keyMatch">Information about which key was matched.</param>
    /// <param name="isCaseInsensitive">Whether or not matches should ignore case. Defaults to <see langword="true"/>.</param>
    /// <returns>Whether or not the property was found.</returns>
    public static bool TryFindPropertyByKey(
        this DatTypeWithProperties type,
        string key,
        ref FileEvaluationContext ctx,
        [NotNullWhen(true)] out DatProperty? property,
        out DatProperty.KeyMatch keyMatch,
        bool isCaseInsensitive = true)
    {
        int propListCode = ctx.PropertyContext switch
        {
            SpecPropertyContext.Localization or SpecPropertyContext.CrossReferenceLocalization => 1,
            _ => 0
        };

        for (DatTypeWithProperties? parent = type; parent != null; parent = parent.BaseType)
        {
            ImmutableArray<DatProperty> propertyList;
            switch (propListCode)
            {
                case 0:
                    propertyList = parent.Properties;
                    break;

                default: // case 1:
                    if (parent is not IDatTypeWithLocalizationProperties locals)
                        continue;

                    propertyList = locals.LocalizationProperties;
                    break;
            }

            foreach (DatProperty prop in propertyList)
            {
                if (!prop.MatchesKey(key, ref ctx, isCaseInsensitive, out keyMatch))
                    continue;

                property = prop;
                return true;
            }

            ImmutableArray<DatProperty>.Builder? propertyListFallback;
            switch (propListCode)
            {
                case 0:
                    propertyListFallback = parent.PropertiesBuilder;
                    break;

                default: // case 1:
                    if (parent is not IDatTypeWithLocalizationProperties locals)
                        continue;

                    propertyListFallback = locals.LocalizationPropertiesBuilder;
                    break;
            }

            if (propertyListFallback != null)
            {
                foreach (DatProperty prop in propertyListFallback)
                {
                    if (!prop.MatchesKey(key, ref ctx, isCaseInsensitive, out keyMatch))
                        continue;

                    property = prop;
                    return true;
                }
            }
        }

        property = null;
        keyMatch = default;
        return false;
    }

#pragma warning disable CS8500

    /// <summary>
    /// Checks whether or not a property is excluded.
    /// </summary>
    /// <param name="property">The property to evaluate.</param>
    /// <param name="ctx">Workspace context for the operation.</param>
    public static bool IsExcluded(this DatProperty property, ref FileEvaluationContext ctx)
    {
        if (property.Context == SpecPropertyContext.BundleAsset)
        {
            IBundleProxy bndl = ctx.File.WorkspaceFile.Bundle;
            if (property is DatBundleAsset asset)
                return bndl.GetCorrespondingAsset(asset, ref ctx) == null;

            return bndl.GetCorrespondingAsset(property.Key, property.Type, ref ctx) == null;
        }

        return !ctx.File.TryGetProperty(property, ref ctx, out _);
    }

    /// <summary>
    /// Gets the kind of value for a property, falling back to <see cref="SourceValueType.Value"/> if the property isn't included.
    /// </summary>
    /// <param name="property">The property to evaluate.</param>
    /// <param name="ctx">Workspace context for the operation.</param>
    public static SourceValueType GetValueType(this DatProperty property, ref FileEvaluationContext ctx)
    {
        if (property.Context == SpecPropertyContext.BundleAsset
         || !ctx.File.TryGetProperty(property, ref ctx, out IPropertySourceNode? propertyNode))
        {
            return SourceValueType.Value;
        }

        return propertyNode.ValueKind;
    }

    /// <summary>
    /// Checks whether or not a property is included, optionally with a valid value (<paramref name="requireValue"/>).
    /// </summary>
    /// <param name="property">The property to evaluate.</param>
    /// <param name="requireValue">Whether or not a valid value must also be present to be considered included.</param>
    /// <param name="ctx">Workspace context for the operation.</param>
    public static unsafe bool IsIncluded(this DatProperty property, bool requireValue, ref FileEvaluationContext ctx)
    {
        if (!ctx.File.TryGetProperty(property, ref ctx, out IPropertySourceNode? propertyNode))
        {
            return false;
        }

        if (!requireValue)
        {
            return true;
        }
        
        if (!property.Type.TryEvaluateType(out IType? propertyType, ref ctx))
        {
            return false;
        }

        SuccessVisitor visitor;
        visitor.Success = false;

        VisitValueTypeVisitor<SuccessVisitor> v;
        v.Property = property;
        v.ValueNode = propertyNode.Value;
        v.ParentNode = propertyNode;
        v.KeyFilter = ctx.GetKeyFilter();
        v.Visited = false;
        v.DiagnosticSink = null;
        v.ReferencedPropertySink = null;
        v.MissingValueBehavior = TypeParserMissingValueBehavior.ErrorIfValueOrPropertyNotProvided;
        v.GetValue = false;
        v.Value = null;
        fixed (FileEvaluationContext* evalCtxPtr = &ctx)
        {
            v.EvaluationContext = evalCtxPtr;
            v.Visitor = &visitor;
            propertyType.Visit(ref v);
        }

        return visitor.Success;
    }

    private struct SuccessVisitor : IValueVisitor
    {
        public bool Success;
        public void Accept<TValue>(IType<TValue> type, Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            Success = value.HasValue;
        }
    }

    /// <inheritdoc cref="VisitValue{TVisitor}(DatProperty,ref TVisitor,ref FileEvaluationContext,PropertyBreadcrumbs,IDiagnosticSink?,IReferencedPropertySink?,TypeParserMissingValueBehavior)"/>
    public static unsafe bool VisitValue<TVisitor>(
        this DatProperty property,
        ref TVisitor visitor,
        ref FileEvaluationContext ctx,
        IDiagnosticSink? diagnosticSink = null,
        IReferencedPropertySink? referencedPropertySink = null,
        TypeParserMissingValueBehavior missingValueBahvior = TypeParserMissingValueBehavior.ErrorIfValueOrPropertyNotProvided)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (!property.Type.TryEvaluateType(out IType? propertyType, ref ctx))
        {
            return false;
        }

        if (!ctx.TryGetTargetDictionary(out IDictionarySourceNode? targetDictionary, out _))
        {
            return false;
        }

        if (!targetDictionary.TryGetProperty(property, ref ctx, out IPropertySourceNode? propertyNode))
        {
            IValue? defaultValue = property.DefaultValue;
            return defaultValue != null && defaultValue.VisitValue(ref visitor, ref ctx);
        }

        VisitValueTypeVisitor<TVisitor> v;
        v.Property = property;
        v.ValueNode = propertyNode.Value;
        v.ParentNode = propertyNode;
        v.Visited = false;
        v.DiagnosticSink = diagnosticSink;
        v.ReferencedPropertySink = referencedPropertySink;
        v.MissingValueBehavior = missingValueBahvior;
        v.KeyFilter = ctx.GetKeyFilter();
        v.GetValue = false;
        v.Value = null;
        fixed (FileEvaluationContext* evalCtxPtr = &ctx)
        fixed (TVisitor* visitorPtr = &visitor)
        {
            v.EvaluationContext = evalCtxPtr;
            v.Visitor = visitorPtr;
            propertyType.Visit(ref v);
        }

        return v.Visited;
    }


    /// <summary>
    /// Invokes a visitor with the current value of the given property.
    /// If the property is not included the <see cref="DatProperty.DefaultValue"/> will be visited instead.
    /// If the property has no value the <see cref="DatProperty.IncludedDefaultValue"/> will be visited.
    /// </summary>
    /// <typeparam name="TVisitor">A visitor type to invoke <see cref="IValueVisitor.Accept{TValue}"/> on.</typeparam>
    /// <param name="property">The property to evaluate.</param>
    /// <param name="visitor">A visitor to invoke <see cref="IValueVisitor.Accept{TValue}"/> on.</param>
    /// <param name="ctx">Workspace context for the operation.</param>
    /// <param name="breadcrumbs">Breadcrumbs to the property within a file.</param>
    /// <param name="diagnosticSink">Object which will receive any parse diagnostics.</param>
    /// <param name="referencedPropertySink">Object which will receive any other referenced properties.</param>
    /// <returns>Whether or not the visitor was invoked.</returns>
    public static unsafe bool VisitValue<TVisitor>(
        this DatProperty property,
        ref TVisitor visitor,
        ref FileEvaluationContext ctx,
        PropertyBreadcrumbs breadcrumbs,
        IDiagnosticSink? diagnosticSink = null,
        IReferencedPropertySink? referencedPropertySink = null,
        TypeParserMissingValueBehavior missingValueBahvior = TypeParserMissingValueBehavior.ErrorIfValueOrPropertyNotProvided)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (!property.Type.TryEvaluateType(out IType? propertyType, ref ctx))
        {
            return false;
        }

        if (property is DatBundleAsset)
        {
            // todo: IBundleProxy bundleProxy = ctx.Services.Installation.GetBundleProxyForFile(ctx.File.WorkspaceFile);
            throw new NotImplementedException();
        }

        IDictionarySourceNode? startingDictionary;
        LegacyExpansionFilter filter;
        DatTypeWithProperties? type;
        if (breadcrumbs.Length > 0 && "~".Equals(breadcrumbs[0].Property))
        {
            if (!ctx.TryGetTargetRoot(out startingDictionary))
            {
                return false;
            }

            type = ctx.FileType.Information;
            filter = breadcrumbs[^1].Context.ToKeyFilter();
        }
        else if (!ctx.TryGetTargetDictionary(out startingDictionary, out type))
        {
            return false;
        }
        else
        {
            filter = ctx.GetKeyFilter();
        }

        if (!breadcrumbs.TryTraceRelativeTo(startingDictionary, type, out IAnyValueSourceNode? targetValue, out _, out _, ref ctx)
            || targetValue is not IDictionarySourceNode targetDictionary)
        {
            return false;
        }

        if (!targetDictionary.TryGetProperty(property, ref ctx, out IPropertySourceNode? propertyNode))
        {
            IValue? defaultValue = property.DefaultValue;
            return defaultValue != null && defaultValue.VisitValue(ref visitor, ref ctx);
        }

        VisitValueTypeVisitor<TVisitor> v;
        v.Property = property;
        v.ValueNode = propertyNode.Value;
        v.ParentNode = propertyNode;
        v.Visited = false;
        v.DiagnosticSink = diagnosticSink;
        v.ReferencedPropertySink = referencedPropertySink;
        v.MissingValueBehavior = missingValueBahvior;
        v.KeyFilter = filter;
        v.GetValue = false;
        v.Value = null;
        fixed (FileEvaluationContext* evalCtxPtr = &ctx)
        fixed (TVisitor* visitorPtr = &visitor)
        {
            v.EvaluationContext = evalCtxPtr;
            v.Visitor = visitorPtr;
            propertyType.Visit(ref v);
        }

        return v.Visited;
    }

    /// <inheritdoc cref="VisitValue(DatProperty,ref FileEvaluationContext,PropertyBreadcrumbs, out IValue?,out IPropertySourceNode?,IDiagnosticSink?,IReferencedPropertySink?,TypeParserMissingValueBehavior)"/>
    public static unsafe bool TryGetValue(
        this DatProperty property,
        ref FileEvaluationContext ctx,
        [NotNullWhen(true)] out IValue? value,
        out IPropertySourceNode? propertyNode,
        IDiagnosticSink? diagnosticSink = null,
        IReferencedPropertySink? referencedPropertySink = null,
        TypeParserMissingValueBehavior missingValueBahvior = TypeParserMissingValueBehavior.ErrorIfValueOrPropertyNotProvided)
    {
        value = null;
        propertyNode = null;

        if (!property.Type.TryEvaluateType(out IType? propertyType, ref ctx))
        {
            return false;
        }

        if (!ctx.TryGetTargetDictionary(out IDictionarySourceNode? targetDictionary, out _))
        {
            return false;
        }

        if (!targetDictionary.TryGetProperty(property, ref ctx, out propertyNode))
        {
            value = property.DefaultValue;
            propertyNode = null;
            return value != null;
        }

        VisitValueTypeVisitor<SuccessVisitor> v;
        v.Property = property;
        v.ValueNode = propertyNode.Value;
        v.ParentNode = propertyNode;
        v.Visited = false;
        v.DiagnosticSink = diagnosticSink;
        v.ReferencedPropertySink = referencedPropertySink;
        v.MissingValueBehavior = missingValueBahvior;
        v.KeyFilter = ctx.GetKeyFilter();
        v.GetValue = true;
        v.Value = null;
        v.Visitor = null;
        fixed (FileEvaluationContext* evalCtxPtr = &ctx)
        {
            v.EvaluationContext = evalCtxPtr;
            propertyType.Visit(ref v);
        }

        if (v.Value == null)
        {
            value = null;
            return false;
        }

        value = v.Value;
        return true;
    }


    /// <summary>
    /// Invokes a visitor with the current value of the given property.
    /// If the property is not included the <see cref="DatProperty.DefaultValue"/> will be visited instead.
    /// If the property has no value the <see cref="DatProperty.IncludedDefaultValue"/> will be visited.
    /// </summary>
    /// <typeparam name="TVisitor">A visitor type to invoke <see cref="IValueVisitor.Accept{TValue}"/> on.</typeparam>
    /// <param name="property">The property to evaluate.</param>
    /// <param name="visitor">A visitor to invoke <see cref="IValueVisitor.Accept{TValue}"/> on.</param>
    /// <param name="ctx">Workspace context for the operation.</param>
    /// <param name="breadcrumbs">Breadcrumbs to the property within a file.</param>
    /// <param name="diagnosticSink">Object which will receive any parse diagnostics.</param>
    /// <param name="referencedPropertySink">Object which will receive any other referenced properties.</param>
    /// <returns>Whether or not the visitor was invoked.</returns>
    public static unsafe bool VisitValue(
        this DatProperty property,
        ref FileEvaluationContext ctx,
        PropertyBreadcrumbs breadcrumbs,
        [NotNullWhen(true)] out IValue? value,
        out IPropertySourceNode? propertyNode,
        IDiagnosticSink? diagnosticSink = null,
        IReferencedPropertySink? referencedPropertySink = null,
        TypeParserMissingValueBehavior missingValueBahvior = TypeParserMissingValueBehavior.ErrorIfValueOrPropertyNotProvided)
    {
        value = null;
        propertyNode = null;

        if (!property.Type.TryEvaluateType(out IType? propertyType, ref ctx))
        {
            return false;
        }

        IDictionarySourceNode? startingDictionary;
        LegacyExpansionFilter filter;
        DatTypeWithProperties? type;
        if (breadcrumbs.Length > 0 && "~".Equals(breadcrumbs[0].Property))
        {
            if (!ctx.TryGetTargetRoot(out startingDictionary))
            {
                return false;
            }

            type = ctx.FileType.Information;
            filter = breadcrumbs[^1].Context.ToKeyFilter();
        }
        else if (!ctx.TryGetTargetDictionary(out startingDictionary, out type))
        {
            return false;
        }
        else
        {
            filter = ctx.GetKeyFilter();
        }

        if (!breadcrumbs.TryTraceRelativeTo(startingDictionary, type, out IAnyValueSourceNode? targetValue, out _, out _, ref ctx)
            || targetValue is not IDictionarySourceNode targetDictionary)
        {
            return false;
        }

        if (!targetDictionary.TryGetProperty(property, ref ctx, out propertyNode))
        {
            value = property.DefaultValue;
            propertyNode = null;
            return value != null;
        }

        VisitValueTypeVisitor<SuccessVisitor> v;
        v.Property = property;
        v.ValueNode = propertyNode.Value;
        v.ParentNode = propertyNode;
        v.Visited = false;
        v.DiagnosticSink = diagnosticSink;
        v.ReferencedPropertySink = referencedPropertySink;
        v.MissingValueBehavior = missingValueBahvior;
        v.KeyFilter = filter;
        v.GetValue = true;
        v.Value = null;
        v.Visitor = null;
        fixed (FileEvaluationContext* evalCtxPtr = &ctx)
        {
            v.EvaluationContext = evalCtxPtr;
            propertyType.Visit(ref v);
        }

        if (v.Value == null)
        {
            value = null;
            return false;
        }

        value = v.Value;
        return true;
    }

    private unsafe struct VisitValueTypeVisitor<TVisitor> : ITypeVisitor
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        public FileEvaluationContext* EvaluationContext;
        public TVisitor* Visitor;
        public DatProperty Property;
        public IAnyValueSourceNode? ValueNode;
        public IParentSourceNode ParentNode;
        public IDiagnosticSink? DiagnosticSink;
        public IReferencedPropertySink? ReferencedPropertySink;
        public TypeParserMissingValueBehavior MissingValueBehavior;
        public LegacyExpansionFilter KeyFilter;
        public bool Visited;
        public bool GetValue;
        public IValue? Value;

        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            TypeParserArgs<TValue> parseArgs = new TypeParserArgs<TValue>
            {
                DiagnosticSink = DiagnosticSink,
                ReferencedPropertySink = ReferencedPropertySink,
                KeyFilter = KeyFilter,
                Property = Property,
                ValueNode = ValueNode,
                ParentNode = ParentNode,
                Type = type,
                MissingValueBehavior = MissingValueBehavior
            };

            if (type.Parser.TryParse(ref parseArgs, ref Unsafe.AsRef<FileEvaluationContext>(EvaluationContext), out Optional<TValue> value))
            {
                if (GetValue)
                {
                    Value = type.CreateValue(value);
                }
                else
                {
                    Visitor->Accept(type, value);
                }
                Visited = true;
            }
            else
            {
                Visited = false;
            }
        }
    }

#pragma warning restore CS8500
}

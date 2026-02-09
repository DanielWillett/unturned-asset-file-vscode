using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
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
    /// Attempts to find a property or bundle asset by its key in <paramref name="type"/> or any of its child types.
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

#pragma warning disable CS8500

    /// <summary>
    /// Checks whether or not a property is excluded.
    /// </summary>
    /// <param name="property">The property to evaluate.</param>
    /// <param name="ctx">Workspace context for the operation.</param>
    /// <param name="keyFilter">The current object context (modern/legacy).</param>
    public static bool IsExcluded(this DatProperty property, in FileEvaluationContext ctx)
    {
        return !ctx.File.TryGetProperty(property, in ctx, out _);
    }

    /// <summary>
    /// Checks whether or not a property is included, optionally with a valid value (<paramref name="requireValue"/>).
    /// </summary>
    /// <param name="property">The property to evaluate.</param>
    /// <param name="requireValue">Whether or not a valid value must also be present to be considered included.</param>
    /// <param name="ctx">Workspace context for the operation.</param>
    /// <param name="keyFilter">The current object context (modern/legacy).</param>
    public static unsafe bool IsIncluded(this DatProperty property, bool requireValue, in FileEvaluationContext ctx)
    {
        if (!ctx.File.TryGetProperty(property, in ctx, out IPropertySourceNode? propertyNode))
        {
            return false;
        }

        if (!requireValue)
        {
            return true;
        }
        
        if (!property.Type.TryEvaluateType(out IType? propertyType, in ctx))
        {
            return false;
        }

        SuccessVisitor visitor;
        visitor.Success = false;

        VisitValueTypeVisitor<SuccessVisitor> v;
        v.Property = property;
        v.ValueNode = propertyNode.Value;
        v.ParentNode = propertyNode;
        v.Visited = false;
        v.DiagnosticSink = null;
        v.ReferencedPropertySink = null;
        v.MissingValueBehavior = TypeParserMissingValueBehavior.ErrorIfValueOrPropertyNotProvided;
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
        public void Accept<TValue>(Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            Success = value.HasValue;
        }
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
    /// <param name="diagnosticSink">Object which will receive any parse diagnostics.</param>
    /// <param name="referencedPropertySink">Object which will receive any other referenced properties.</param>
    /// <returns>Whether or not the visitor was invoked.</returns>
    public static unsafe bool VisitValue<TVisitor>(
        this DatProperty property,
        ref TVisitor visitor,
        in FileEvaluationContext ctx,
        IDiagnosticSink? diagnosticSink = null,
        IReferencedPropertySink? referencedPropertySink = null)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (!ctx.File.TryGetProperty(property, in ctx, out IPropertySourceNode? propertyNode))
        {
            IValue? defaultValue = property.DefaultValue;
            return defaultValue != null && defaultValue.VisitValue(ref visitor, in ctx);
        }

        if (!property.Type.TryEvaluateType(out IType? propertyType, in ctx))
        {
            return false;
        }

        VisitValueTypeVisitor<TVisitor> v;
        v.Property = property;
        v.ValueNode = propertyNode.Value;
        v.ParentNode = propertyNode;
        v.Visited = false;
        v.DiagnosticSink = diagnosticSink;
        v.ReferencedPropertySink = referencedPropertySink;
        v.MissingValueBehavior = TypeParserMissingValueBehavior.FallbackToDefaultValue;
        fixed (FileEvaluationContext* evalCtxPtr = &ctx)
        fixed (TVisitor* visitorPtr = &visitor)
        {
            v.EvaluationContext = evalCtxPtr;
            v.Visitor = visitorPtr;
            propertyType.Visit(ref v);
        }

        return v.Visited;
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
        public bool Visited;

        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            TypeParserArgs<TValue> parseArgs = new TypeParserArgs<TValue>
            {
                DiagnosticSink = DiagnosticSink,
                ReferencedPropertySink = ReferencedPropertySink,
                // todo: KeyFilter = EvaluationContext->PropertyContext.ToKeyFilter(),
                Property = Property,
                ValueNode = ValueNode,
                ParentNode = ParentNode,
                Type = type,
                MissingValueBehavior = MissingValueBehavior
            };

            if (type.Parser.TryParse(ref parseArgs, in Unsafe.AsRef<FileEvaluationContext>(EvaluationContext), out Optional<TValue> value))
            {
                Visitor->Accept(value);
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

using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;

/// <summary>
/// Provides an optimized API for checking for code fixes on each resolved property.
/// </summary>
public interface IPerPropertyCodeFix : ICodeFix
{
    /// <summary>
    /// Flags determining which properties should be tested.
    /// </summary>
    PropertyInclusionFlags InclusionFlags { get; }

    /// <summary>
    /// A set of types of properties this code fix is valid on, or <see langword="null"/> to analyze all types of properties.
    /// </summary>
    HashSet<IType>? ValidTypes { get; }

    /// <summary>
    /// Final check to see if this code fix should be added to the property.
    /// </summary>
    /// <remarks>No need to check type and inclusion flags if they're included.</remarks>
    CodeFixInstance? TryApplyToProperty(
        IPropertySourceNode propertyNode,
        IType propertyType,
        DatProperty property,
        in PropertyBreadcrumbs breadcrumbs,
        in FileEvaluationContext ctx
    );

    /// <summary>
    /// Final check to see if this code fix should be added to an unresolved property.
    /// </summary>
    /// <remarks>No need to check type and inclusion flags if they're included.</remarks>
    CodeFixInstance? TryApplyToUnknownProperty(
        IPropertySourceNode propertyNode,
        in PropertyBreadcrumbs breadcrumbs
    );

    /// <summary>
    /// Checks if a type passes the type check on <see cref="Type"/> and <see cref="Types"/>.
    /// </summary>
    bool PassesTypeCheck(IType propertyType);
}

[Flags]
public enum PropertyInclusionFlags
{
    None,

    /// <summary>
    /// The "Metadata" section, if it exists.
    /// </summary>
    Metadata = 1 << 0,

    /// <summary>
    /// The "Asset" section or root dictionary if the "Asset" section doesn't exist or the file isn't an asset file.
    /// </summary>
    AssetOrRoot = 1 << 1,

    /// <summary>
    /// Properties nested in objects or lists of objects.
    /// </summary>
    NonRootProperties = 1 << 2,

    /// <summary>
    /// Only include unresolved properties.
    /// </summary>
    UnresolvedOnly = 1 << 3,

    /// <summary>
    /// Only include resolved properties
    /// </summary>
    ResolvedOnly = 1 << 4,

    /// <summary>
    /// All resolved properties.
    /// </summary>
    All = Metadata | AssetOrRoot | NonRootProperties
}

public abstract class PerPropertyCodeFix<TState> : CodeFix<TState>, IPerPropertyCodeFix
{
    private readonly IFileRelationalModelProvider _modelProvider;
    private readonly IParsingServices _parsingServices;

    public HashSet<IType>? ValidTypes { get; protected set; }

    protected PerPropertyCodeFix(DatDiagnostic diagnostic,
        IFileRelationalModelProvider modelProvider,
        IParsingServices parsingServices) : base(diagnostic)
    {
        _modelProvider = modelProvider;
        _parsingServices = parsingServices;
    }

    public virtual PropertyInclusionFlags InclusionFlags => PropertyInclusionFlags.All | PropertyInclusionFlags.ResolvedOnly;


    public CodeFixInstance? TryApplyToProperty(
        IPropertySourceNode propertyNode,
        IType propertyType,
        DatProperty property,
        in PropertyBreadcrumbs breadcrumbs,
        in FileEvaluationContext ctx
    )
    {
        bool hasDiagnostic = false;
        if (!TryApplyToProperty(out TState? state, out FileRange range, ref hasDiagnostic, propertyNode, propertyType, property, in breadcrumbs, in ctx))
        {
            return null;
        }

        return new CodeFixInstance<TState>(new CodeFixParameters<TState>
        {
            State = state
        }, this, range, hasDiagnostic);
    }

    public CodeFixInstance? TryApplyToUnknownProperty(
        IPropertySourceNode propertyNode,
        in PropertyBreadcrumbs breadcrumbs
    )
    {
        bool hasDiagnostic = false;
        if (!TryApplyToUnknownProperty(out TState? state, out FileRange range, ref hasDiagnostic, propertyNode, in breadcrumbs))
        {
            return null;
        }

        return new CodeFixInstance<TState>(new CodeFixParameters<TState>
        {
            State = state
        }, this, range, hasDiagnostic);
    }

    public bool PassesTypeCheck(IType propertyType)
    {
        if (ValidTypes != null && !ValidTypes.Contains(propertyType))
            return false;

        return true;
    }

    public abstract bool TryApplyToProperty(
        [MaybeNullWhen(false)] out TState state,
        out FileRange range,
        ref bool hasDiagnostic,
        IPropertySourceNode propertyNode,
        IType propertyType,
        DatProperty property,
        in PropertyBreadcrumbs breadcrumbs,
        in FileEvaluationContext ctx
    );

    public virtual bool TryApplyToUnknownProperty(
        [MaybeNullWhen(false)] out TState state,
        out FileRange range,
        ref bool hasDiagnostic,
        IPropertySourceNode propertyNode,
        in PropertyBreadcrumbs breadcrumbs
    )
    {
        state = default;
        range = default;
        return false;
    }

    public override void GetValidPositions(ISourceNode root, FileRange? range, IList<CodeFixInstance> outputList)
    {
        PerPropertyVisitor visitor = new PerPropertyVisitor(_modelProvider, _parsingServices, outputList, this, range, InclusionFlags);

        root.Visit(ref visitor);
    }

    private sealed class PerPropertyVisitor : ResolvedPropertyNodeVisitor
    {
        private readonly IList<CodeFixInstance> _outputList;
        private readonly PerPropertyCodeFix<TState> _codeFix;

        public PerPropertyVisitor(IFileRelationalModelProvider modelProvider,
            IParsingServices parsingServices,
            IList<CodeFixInstance> outputList,
            PerPropertyCodeFix<TState> codeFix,
            FileRange? range,
            PropertyInclusionFlags flags = PropertyInclusionFlags.All | PropertyInclusionFlags.ResolvedOnly)
            : base(modelProvider, parsingServices, range, flags)
        {
            _outputList = outputList;
            _codeFix = codeFix;
        }

        protected override void AcceptResolvedProperty(
            DatProperty property,
            IType propertyType,
            in FileEvaluationContext ctx,
            IPropertySourceNode node,
            in PropertyBreadcrumbs breadcrumbs)
        {
            if (!_codeFix.PassesTypeCheck(propertyType))
            {
                return;
            }

            bool hasDiagnostic = false;
            if (_codeFix.TryApplyToProperty(out TState? state, out FileRange range, ref hasDiagnostic, node, propertyType, property, in breadcrumbs, in ctx))
            {
                _outputList.Add(new CodeFixInstance<TState>(new CodeFixParameters<TState>
                {
                    State = state
                }, _codeFix, range, hasDiagnostic));
            }
        }

        /// <inheritdoc />
        protected override void AcceptUnresolvedProperty(IPropertySourceNode node, in PropertyBreadcrumbs breadcrumbs)
        {
            bool hasDiagnostic = false;
            if (_codeFix.TryApplyToUnknownProperty(out TState? state, out FileRange range, ref hasDiagnostic, node, in breadcrumbs))
            {
                _outputList.Add(new CodeFixInstance<TState>(new CodeFixParameters<TState>
                {
                    State = state
                }, _codeFix, range, hasDiagnostic));
            }
        }
    }
}
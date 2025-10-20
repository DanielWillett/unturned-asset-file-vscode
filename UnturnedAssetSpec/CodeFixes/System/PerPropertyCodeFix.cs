using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
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
    HashSet<ISpecPropertyType>? ValidTypes { get; }

    /// <summary>
    /// Final check to see if this code fix should be added to the property.
    /// </summary>
    /// <remarks>No need to check type and inclusion flags if they're included.</remarks>
    CodeFixInstance? TryApplyToProperty(
        IPropertySourceNode propertyNode,
        ISpecPropertyType propertyType,
        SpecProperty property,
        in PropertyBreadcrumbs breadcrumbs,
        in SpecPropertyTypeParseContext parseContext
    );

    /// <summary>
    /// Checks if a type passes the type check on <see cref="Type"/> and <see cref="Types"/>.
    /// </summary>
    bool PassesTypeCheck(ISpecPropertyType propertyType);
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
    /// All properties.
    /// </summary>
    All = Metadata | AssetOrRoot | NonRootProperties
}

public abstract class PerPropertyCodeFix<TState> : CodeFix<TState>, IPerPropertyCodeFix
{
    private readonly IFilePropertyVirtualizer _virtualizer;
    private readonly IAssetSpecDatabase _database;
    private readonly InstallationEnvironment _installEnv;
    private readonly IWorkspaceEnvironment _workspaceEnv;

    public HashSet<ISpecPropertyType>? ValidTypes { get; protected set; }

    protected PerPropertyCodeFix(DatDiagnostic diagnostic,
        IFilePropertyVirtualizer virtualizer,
        IAssetSpecDatabase database,
        InstallationEnvironment installEnv,
        IWorkspaceEnvironment workspaceEnv) : base(diagnostic)
    {
        _virtualizer = virtualizer;
        _database = database;
        _installEnv = installEnv;
        _workspaceEnv = workspaceEnv;
    }

    public virtual PropertyInclusionFlags InclusionFlags => PropertyInclusionFlags.All;


    public CodeFixInstance? TryApplyToProperty(
        IPropertySourceNode propertyNode,
        ISpecPropertyType propertyType,
        SpecProperty property,
        in PropertyBreadcrumbs breadcrumbs,
        in SpecPropertyTypeParseContext parseContext
    )
    {
        if (!TryApplyToProperty(out TState? state, out FileRange range, propertyNode, propertyType, property, in breadcrumbs, in parseContext))
        {
            return null;
        }

        return new CodeFixInstance<TState>(new CodeFixParameters<TState>
        {
            State = state
        }, this, range);
    }

    public bool PassesTypeCheck(ISpecPropertyType propertyType)
    {
        if (ValidTypes != null && !ValidTypes.Contains(propertyType))
            return false;

        return true;
    }

    public abstract bool TryApplyToProperty(
        [MaybeNullWhen(false)] out TState state,
        out FileRange range,
        IPropertySourceNode propertyNode,
        ISpecPropertyType propertyType,
        SpecProperty property,
        in PropertyBreadcrumbs breadcrumbs,
        in SpecPropertyTypeParseContext parseContext
    );

    public override void GetValidPositions(ISourceNode root, IList<CodeFixInstance> outputList)
    {
        PerPropertyVisitor visitor = new PerPropertyVisitor(_virtualizer, _database, _installEnv, _workspaceEnv, outputList, this, InclusionFlags);

        root.Visit(ref visitor);
    }

    private sealed class PerPropertyVisitor : ResolvedPropertyNodeVisitor
    {
        private readonly IList<CodeFixInstance> _outputList;
        private readonly PerPropertyCodeFix<TState> _codeFix;

        public PerPropertyVisitor(
            IFilePropertyVirtualizer virtualizer,
            IAssetSpecDatabase database,
            InstallationEnvironment installEnv,
            IWorkspaceEnvironment workspaceEnv,
            IList<CodeFixInstance> outputList,
            PerPropertyCodeFix<TState> codeFix,
            PropertyInclusionFlags flags = PropertyInclusionFlags.All)
            : base(virtualizer, database, installEnv, workspaceEnv, flags)
        {
            _outputList = outputList;
            _codeFix = codeFix;
        }

        protected override void AcceptResolvedProperty(
            SpecProperty property,
            ISpecPropertyType propertyType,
            in SpecPropertyTypeParseContext parseCtx,
            IPropertySourceNode node,
            PropertyBreadcrumbs breadcrumbs)
        {
            if (!_codeFix.PassesTypeCheck(propertyType))
            {
                return;
            }
            if (_codeFix.TryApplyToProperty(out TState? state, out FileRange range, node, propertyType, property, in breadcrumbs, in parseCtx))
            {
                _outputList.Add(new CodeFixInstance<TState>(new CodeFixParameters<TState>
                {
                    State = state
                }, _codeFix, range));
            }
        }
    }
}
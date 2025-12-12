using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;

/// <summary>
/// A code fix expresses an action that can be performed ( ex. remove redundant parenthesis, etc ).
/// </summary>
/// <remarks>Actual implementations should use <see cref="ICodeFix{TState}"/>.</remarks>
public interface ICodeFix
{
    /// <summary>
    /// Index set by the code fix list.
    /// </summary>
    /// <remarks>Unique identifier for this code fix.</remarks>
    int Index { get; set; }

    /// <summary>
    /// The diagnostic this fix is associated with.
    /// </summary>
    DatDiagnostic Diagnostic { get; }

    /// <summary>
    /// If discovery needs to be ran (this code-fix won't be discovered by a parser).
    /// </summary>
    bool NeedsExplicitDiscover { get; }

    /// <summary>
    /// Gets the localized label of the fix in the IDE.
    /// </summary>
    string GetLocalizedTitle(CodeFixInstance instance);

    /// <summary>
    /// Adds every valid position under the given <paramref name="root"/> node for this code fix to a <paramref name="outputList"/>.
    /// </summary>
    void GetValidPositions(ISourceNode root, FileRange? range, IList<CodeFixInstance> outputList);
}

/// <summary>
/// A code fix expresses an action that can be performed ( ex. remove redundant parenthesis, etc ).
/// </summary>
/// <typeparam name="TState">A generic state passed from the discover method to the apply method.</typeparam>
public interface ICodeFix<TState> : ICodeFix
{
    /// <summary>
    /// Actually applies a code fix instance to a file.
    /// </summary>
    void ApplyCodeFix(in CodeFixParameters<TState> parameters, IMutableWorkspaceFile file);
}


/// <summary>
/// Base class for <see cref="ICodeFix{TState}"/>.
/// </summary>
public abstract class CodeFix<TState> : ICodeFix<TState>
{
    public int Index { get; set; }

    public DatDiagnostic Diagnostic { get; }
    public abstract bool NeedsExplicitDiscover { get; }

    protected CodeFix(DatDiagnostic diagnostic)
    {
        Diagnostic = diagnostic;
    }

    public string GetLocalizedTitle(CodeFixInstance instance) => GetLocalizedTitle((CodeFixInstance<TState>)instance);
    protected abstract string GetLocalizedTitle(CodeFixInstance<TState> instance);
    public abstract void GetValidPositions(ISourceNode root, FileRange? range, IList<CodeFixInstance> outputList);
    public abstract void ApplyCodeFix(in CodeFixParameters<TState> parameters, IMutableWorkspaceFile file);
}

public abstract class CodeFixInstance
{
    public abstract ICodeFix CodeFix { get; }
    public abstract FileRange Range { get; }
    public abstract object? State { get; }
    public bool HasDiagnostic { get; protected set; }
    public abstract void ApplyCodeFix(IMutableWorkspaceFile file);
}

public class CodeFixInstance<TState> : CodeFixInstance
{
    private readonly CodeFixParameters<TState> _parameters;
    private readonly ICodeFix<TState> _codeFix;

    public override ICodeFix CodeFix => _codeFix;

    public ref readonly CodeFixParameters<TState> Parameters => ref _parameters;

    public override FileRange Range { get; }

    public override object? State => _parameters.State;

    public CodeFixInstance(CodeFixParameters<TState> parameters, ICodeFix<TState> codeFix, FileRange range, bool hasDiagnostic)
    {
        _parameters = parameters;
        _codeFix = codeFix;
        Range = range;
        HasDiagnostic = hasDiagnostic;
    }

    public override void ApplyCodeFix(IMutableWorkspaceFile file)
    {
        _codeFix.ApplyCodeFix(in _parameters, file);
    }
}

public struct CodeFixParameters<TState>
{
    public TState State;
}
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Checks if the given property is not included at all.
/// <para>
/// Supported targets:
/// <list type="bullet">
///     <item><c>#Self</c></item>
///     <item>Any property reference.</item>
/// </list>
/// </para>
/// <para>
/// Syntax:<br/>
/// <c>#Target.Excluded</c><br/>
/// </para>
/// </summary>
public struct ExcludedProperty : IDataRefProperty, IEquatable<ExcludedProperty>
{
    /// <inheritdoc />
    public string PropertyName => "Excluded";

    /// <inheritdoc />
    public bool Equals(ExcludedProperty other) => true;
}
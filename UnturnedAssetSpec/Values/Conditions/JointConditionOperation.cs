namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// An operation of a <see cref="SpecDynamicSwitchCaseValue"/> condition list.
/// </summary>
public enum JointConditionOperation
{
    /// <summary>
    /// All conditions or cases must be <see langword="true"/>.
    /// </summary>
    And,

    /// <summary>
    /// At least one condition or case must be <see langword="true"/>.
    /// </summary>
    Or
}
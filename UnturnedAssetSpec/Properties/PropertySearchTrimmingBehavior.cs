namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Controls how this type may expand to other properties.
/// </summary>
public enum PropertySearchTrimmingBehavior
{
    /// <summary>
    /// This type behaves in the usual manner, meaning that it can only exist as it's own property name.
    /// <code>
    /// Prop 3
    /// </code>
    /// </summary>
    ExactPropertyOnly,

    /// <summary>
    /// This type may create other properties at the same level as this property, but only if the property exists.
    /// <code>
    /// Prop Test
    /// Prop_ID 3
    /// </code>
    /// </summary>
    CreatesSiblingPropertiesInSameFile,

    /// <summary>
    /// This type may create other properties at the same level as this property, but only if the property exists.
    /// <code>
    /// Prop_X 3
    /// Prop_Y 4
    /// Prop_Z 5
    /// </code>
    /// </summary>
    CreatesOtherPropertiesInSameFileAtSameLevel,

    /// <summary>
    /// This type may create other properties in this file (maybe not at the same level), but only if the property exists.
    /// <code>
    /// Prop_X 3
    /// Prop_Y 4
    /// Prop_Z 5
    /// SomeOther
    /// {
    ///     Prop_W 4
    /// }
    /// </code>
    /// </summary>
    CreatesOtherPropertiesInSameFile,

    /// <summary>
    /// This type may create other properties in either this file or other linked files.
    /// <code>
    /// // Asset.dat
    /// 
    ///     Prop_X 3
    ///     Prop_Y 4
    ///     Prop_Z 5
    ///     SomeOther
    ///     {
    ///         Prop_W 4
    ///     }
    ///
    /// 
    /// // English.dat
    /// 
    ///     Prop_Name This is crazy
    /// </code>
    /// </summary>
    CreatesOtherPropertiesInLinkedFiles
}

// ReSharper disable InconsistentNaming

using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;

/// <summary>
/// Common diagnostics.
/// </summary>
public static class DatDiagnostics
{
    /* External (not used by this library) */

    /// <summary>
    /// Emitted by the <c>Unturned.MSBuild</c> NuGet package when referencing Newtonsoft.Json using &lt;UnturnedReference&gt; instead of &lt;PackageReference&gt;.
    /// </summary>
    public static readonly DatDiagnostic UNT003 = new DatDiagnostic("UNT003", DatDiagnosticSeverity.Error);

    /* Project diagnostics */

    // Warnings

    /// <summary>
    /// Emitted when a project file references a property by an improper casing.
    /// </summary>
    public static readonly DatDiagnostic UPROJ1001 = new DatDiagnostic("UPROJ1001", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Emitted when a project file references a type by an improper casing.
    /// </summary>
    public static readonly DatDiagnostic UPROJ1002 = new DatDiagnostic("UPROJ1002", DatDiagnosticSeverity.Warning);

    // Errors

    /// <summary>
    /// Emitted when a project file references a property that doesn't exist.
    /// </summary>
    public static readonly DatDiagnostic UPROJ2001 = new DatDiagnostic("UPROJ2001", DatDiagnosticSeverity.Error);

    /// <summary>
    /// Emitted when a project file references a type that doesn't exist.
    /// </summary>
    public static readonly DatDiagnostic UPROJ2002 = new DatDiagnostic("UPROJ2002", DatDiagnosticSeverity.Error);

    /* Suggestions */

    /// <summary>
    /// Displayed when a blueprint ID could be replaced with 'this'.
    /// </summary>
    public static readonly DatDiagnostic UNT101 = new DatDiagnostic("UNT101", DatDiagnosticSeverity.Hint);

    /// <summary>
    /// Displayed when a format string doesn't fill all arguments.
    /// </summary>
    public static readonly DatDiagnostic UNT102 = new DatDiagnostic("UNT102", DatDiagnosticSeverity.Hint);

    /// <summary>
    /// Displayed when a TypeReference may not be assignable to its element type.
    /// </summary>
    public static readonly DatDiagnostic UNT103 = new DatDiagnostic("UNT103", DatDiagnosticSeverity.Hint);

    /// <summary>
    /// Displayed when a MasterBundleOrContentReference type uses the legacy ContentReference format.
    /// </summary>
    public static readonly DatDiagnostic UNT104 = new DatDiagnostic("UNT104", DatDiagnosticSeverity.Hint);

    /// <summary>
    /// Displayed when a quoted string has an unnecssary comma after it.
    /// </summary>
    public static readonly DatDiagnostic UNT105 = new DatDiagnostic("UNT105", DatDiagnosticSeverity.Hint);

    /// <summary>
    /// Use &lt;br&gt; instead of \n or \r\n.
    /// </summary>
    public static readonly DatDiagnostic UNT106 = new DatDiagnostic("UNT106", DatDiagnosticSeverity.Hint);

    /// <summary>
    /// Code action only - generate a new GUID.
    /// </summary>
    public static readonly DatDiagnostic UNT107 = new DatDiagnostic("UNT107", DatDiagnosticSeverity.Hint);

    /// <summary>
    /// Use string format instead of object format for a bundle references.
    /// </summary>
    public static readonly DatDiagnostic UNT108 = new DatDiagnostic("UNT108", DatDiagnosticSeverity.Hint);

    /// <summary>
    /// Relevant skill can not be determined for a SkillLevel property.
    /// </summary>
    public static readonly DatDiagnostic UNT109 = new DatDiagnostic("UNT109", DatDiagnosticSeverity.Warning);

    /* Warnings */

    /// <summary>
    /// Displayed when there is a value provided for a dictionary or list key.
    /// </summary>
    public static readonly DatDiagnostic UNT1001 = new DatDiagnostic("UNT1001", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when there is a quoted string without an ending quotation mark.
    /// </summary>
    public static readonly DatDiagnostic UNT1002 = new DatDiagnostic("UNT1002", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when there is a value provided for a flag property.
    /// </summary>
    public static readonly DatDiagnostic UNT1003 = new DatDiagnostic("UNT1003", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when there is an escape sequence (\n, etc) which isn't recognized by the game.
    /// </summary>
    public static readonly DatDiagnostic UNT1004 = new DatDiagnostic("UNT1004", DatDiagnosticSeverity.Warning);

    // todo 1005

    /// <summary>
    /// Displayed when rich text is used on a string property that doesn't usually support it.
    /// </summary>
    public static readonly DatDiagnostic UNT1006 = new DatDiagnostic("UNT1006", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when a joint property is missing one of its components (like a legacy Vector3), or a legacy list is missing an element.
    /// </summary>
    public static readonly DatDiagnostic UNT1007 = new DatDiagnostic("UNT1007", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when a cosmetic (face, beard, hair) index is out of range.
    /// </summary>
    public static readonly DatDiagnostic UNT1008 = new DatDiagnostic("UNT1008", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when an asset bundle version is invalid in an asset file.
    /// </summary>
    public static readonly DatDiagnostic UNT1009 = new DatDiagnostic("UNT1009", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when an a file path uses a backslash.
    /// </summary>
    public static readonly DatDiagnostic UNT1010 = new DatDiagnostic("UNT1010", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when an a file path doesn't match a glob pattern.
    /// </summary>
    public static readonly DatDiagnostic UNT1011 = new DatDiagnostic("UNT1011", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when a legacy color is using an out of range value (should be 0-1).
    /// </summary>
    public static readonly DatDiagnostic UNT1012 = new DatDiagnostic("UNT1012", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when a NPC achievement ID isn't recognized.
    /// </summary>
    public static readonly DatDiagnostic UNT1013 = new DatDiagnostic("UNT1013", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when a enum value isn't recognized.
    /// </summary>
    public static readonly DatDiagnostic UNT1014 = new DatDiagnostic("UNT1014", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when a type value isn't of the expected base type.
    /// </summary>
    public static readonly DatDiagnostic UNT1015 = new DatDiagnostic("UNT1015", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Skill level is not a valid level for a skill.
    /// </summary>
    public static readonly DatDiagnostic UNT1016 = new DatDiagnostic("UNT1016", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Ammount supplied for an item ID.
    /// </summary>
    public static readonly DatDiagnostic UNT1017 = new DatDiagnostic("UNT1017", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Deprecated property or usage.
    /// </summary>
    public static readonly DatDiagnostic UNT1018 = new DatDiagnostic("UNT1018", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Self reference when it shouldn't happen.
    /// </summary>
    public static readonly DatDiagnostic UNT1019 = new DatDiagnostic("UNT1019", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Maximum depth of parser exceeded.
    /// </summary>
    public static readonly DatDiagnostic UNT1020 = new DatDiagnostic("UNT1020", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// New-lines not supported for this string.
    /// </summary>
    public static readonly DatDiagnostic UNT1021 = new DatDiagnostic("UNT1021", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// An invalid new-line tag is used, such as &lt;br/&gt;.
    /// </summary>
    public static readonly DatDiagnostic UNT1022 = new DatDiagnostic("UNT1022", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// TypeReference missing assembly-qualified name.
    /// </summary>
    public static readonly DatDiagnostic UNT1023 = new DatDiagnostic("UNT1023", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// A list or dictionary has too many or too few elements. <see cref="SpecProperty.MinimumCount"/> and <see cref="SpecProperty.MaximumCount"/> trigger this warning.
    /// </summary>
    public static readonly DatDiagnostic UNT1024 = new DatDiagnostic("UNT1024", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// A property that was used isn't recognized by the game.
    /// </summary>
    public static readonly DatDiagnostic UNT1025 = new DatDiagnostic("UNT1025", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// A color supplied an alpha value when it doesn't support it.
    /// </summary>
    public static readonly DatDiagnostic UNT1026 = new DatDiagnostic("UNT1026", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// A list has a duplicate value where it shouldn't.
    /// </summary>
    public static readonly DatDiagnostic UNT1027 = new DatDiagnostic("UNT1027", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// A value is either under the declared minimum value or over the declared maximum value.
    /// </summary>
    public static readonly DatDiagnostic UNT1028 = new DatDiagnostic("UNT1028", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// A Steam64 ID is not using the correct account type.
    /// </summary>
    public static readonly DatDiagnostic UNT1029 = new DatDiagnostic("UNT1029", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// A localization file wasn't found when a property expected one.
    /// </summary>
    public static readonly DatDiagnostic UNT1030 = new DatDiagnostic("UNT1030", DatDiagnosticSeverity.Warning);

    /* Errors */

    /// <summary>
    /// Displayed when a closing bracket is missing on an dictionary.
    /// </summary>
    public static readonly DatDiagnostic UNT2001 = new DatDiagnostic("UNT2001", DatDiagnosticSeverity.Error);

    /// <summary>
    /// Displayed when a closing bracket is missing on an list.
    /// </summary>
    public static readonly DatDiagnostic UNT2002 = new DatDiagnostic("UNT2002", DatDiagnosticSeverity.Error);

    /// <summary>
    /// Displayed when there is a <see langword="false"/> value provided for a flag property.
    /// </summary>
    public static readonly DatDiagnostic UNT2003 = new DatDiagnostic("UNT2003", DatDiagnosticSeverity.Error);

    /// <summary>
    /// Displayed when a value can't be parsed to the correct type.
    /// </summary>
    public static readonly DatDiagnostic UNT2004 = new DatDiagnostic("UNT2004", DatDiagnosticSeverity.Error);

    /// <summary>
    /// Displayed when a configured type can't be found.
    /// </summary>
    public static readonly DatDiagnostic UNT2005 = new DatDiagnostic("UNT2005", DatDiagnosticSeverity.Error);

    /// <summary>
    /// Displayed when an asset bundle version is invalid in a master bundle file.
    /// </summary>
    public static readonly DatDiagnostic UNT2009 = new DatDiagnostic("UNT2009", DatDiagnosticSeverity.Error);

    /// <summary>
    /// Displayed when an asset is missing a GUID.
    /// </summary>
    public static readonly DatDiagnostic UNT2010 = new DatDiagnostic("UNT2010", DatDiagnosticSeverity.Error);

    /// <summary>
    /// Displayed when an asset is missing a legacy ID that is required.
    /// </summary>
    public static readonly DatDiagnostic UNT2011 = new DatDiagnostic("UNT2011", DatDiagnosticSeverity.Error);

    /// <summary>
    /// Malformed format string.
    /// </summary>
    public static readonly DatDiagnostic UNT2012 = new DatDiagnostic("UNT2012", DatDiagnosticSeverity.Error);

    /// <summary>
    /// Can't parse object in either modern or legacy format.
    /// </summary>
    public static readonly DatDiagnostic UNT2013 = new DatDiagnostic("UNT2013", DatDiagnosticSeverity.Error);
}
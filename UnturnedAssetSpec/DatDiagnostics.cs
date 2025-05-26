// ReSharper disable InconsistentNaming

using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data;

public static class DatDiagnostics
{
    /* Suggestions */

    /// <summary>
    /// Displayed when a blueprint ID could be replaced with 'this'.
    /// </summary>
    public static readonly DatDiagnostic UNT101 = new DatDiagnostic("UNT101", DatDiagnosticSeverity.Hint);

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

    /// <summary>
    /// Displayed when a property is missing a value that isn't a flag, list, or dictionary.
    /// </summary>
    public static readonly DatDiagnostic UNT1005 = new DatDiagnostic("UNT1005", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when rich text is used on a string property that doesn't usually support it.
    /// </summary>
    public static readonly DatDiagnostic UNT1006 = new DatDiagnostic("UNT1006", DatDiagnosticSeverity.Warning);

    /// <summary>
    /// Displayed when a joint property is missing one of its components (like a legacy Vector3).
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
    /// Displayed when there is a false value provided for a flag property.
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

}
// ReSharper disable InconsistentNaming

using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data;

internal static class DatDiagnostics
{
    /* Suggestions */

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
}
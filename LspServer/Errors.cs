namespace LspServer;
internal static class Errors
{
    /* Warnings */

    /// <summary>
    /// Displayed when there is a value provided for a dictionary or list key.
    /// </summary>
    public const string WarningExtraValue = "UNT1001";

    /// <summary>
    /// Displayed when there is a quoted string without an ending quotation mark.
    /// </summary>
    public const string WarningMissingEndQuote = "UNT1002";

    /// <summary>
    /// Displayed when there is an escape sequence (\n, etc) which isn't recognized by the game.
    /// </summary>
    public const string WarningUnrecognizedEscapeSequence = "UNT1003";

    /* Errors */

    /// <summary>
    /// Displayed when a closing bracket is missing on an dictionary.
    /// </summary>
    public const string ErrorMissingEndDictionary = "UNT2001";

    /// <summary>
    /// Displayed when a closing bracket is missing on an list.
    /// </summary>
    public const string ErrorMissingEndList = "UNT2002";
}

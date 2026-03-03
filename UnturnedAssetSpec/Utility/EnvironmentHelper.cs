using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class EnvironmentHelper
{
    public static bool ParseBooleanEnvironmentVariable(string var, bool defaultValue = false)
    {
        string? value = Environment.GetEnvironmentVariable(var);
        if (value == null)
        {
            return defaultValue;
        }

        if (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out int v))
        {
            return v != 0;
        }

        if (bool.TryParse(value, out bool b))
        {
            return b;
        }

        if (value.Equals("Y", StringComparison.OrdinalIgnoreCase) || value.Equals("Yes", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.Equals("N", StringComparison.OrdinalIgnoreCase) || value.Equals("No", StringComparison.OrdinalIgnoreCase))
            return false;

        return defaultValue;
    }
}

using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An included or excluded value. Just the presence of the property counts as a <see langword="true"/> value, even if the value is set to 'False'.
/// <para>Example: <c>ItemBarricadeAsset.Vulnerable</c></para>
/// </summary>
public sealed class FlagSpecPropertyType : BaseSpecPropertyType<FlagSpecPropertyType, bool>, IStringParseableSpecPropertyType
{
    public static readonly FlagSpecPropertyType Instance = new FlagSpecPropertyType();

    public override int GetHashCode() => 23;

    static FlagSpecPropertyType() { }
    private FlagSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Flag";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Boolean;

    /// <inheritdoc />
    public override string DisplayName => "Flag";

    protected override ISpecDynamicValue CreateValue(bool value) => SpecDynamicValue.Flag(value);

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (span.Equals("true".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
        {
            dynamicValue = SpecDynamicValue.Included;
            return true;
        }
        if (span.Equals("false".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
        {
            dynamicValue = SpecDynamicValue.Excluded;
            return true;
        }

        if (KnownTypeValueHelper.TryParseBoolean(stringValue ?? span.ToString(), out bool result))
        {
            dynamicValue = result ? SpecDynamicValue.Included : SpecDynamicValue.Excluded;
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<bool>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out bool value)
    {
        if (parse.Node != null && parse.HasDiagnostics)
        {
            FileRange range = parse.Node.Range;
            if (parse.Parent != null)
                range.Encapsulate(parse.Parent.Range);
            DatDiagnosticMessage diagnostic = new DatDiagnosticMessage
            {
                Range = range
            };

            if (parse.Node is IValueSourceNode stringValue
                && KnownTypeValueHelper.TryParseBoolean(stringValue.Value, out bool boolValue)
                && !boolValue)
            {
                diagnostic.Diagnostic = DatDiagnostics.UNT2003;
                diagnostic.Message = string.Format(DiagnosticResources.UNT2003, stringValue.Value);
            }
            else
            {
                diagnostic.Diagnostic = DatDiagnostics.UNT1003;
                diagnostic.Message = DiagnosticResources.UNT1003;
            }

            parse.Log(diagnostic);
        }

        value = parse.Node != null;
        return true;
    }
}

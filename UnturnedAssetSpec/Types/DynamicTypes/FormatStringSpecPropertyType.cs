using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class FormatStringSpecPropertyType : BasicSpecPropertyType<FormatStringSpecPropertyType, string>, IStringParseableSpecPropertyType
{
    public static readonly FormatStringSpecPropertyType OneNoRichText = new FormatStringSpecPropertyType(1, false);
    public static readonly FormatStringSpecPropertyType TwoNoRichText = new FormatStringSpecPropertyType(2, false);
    public static readonly FormatStringSpecPropertyType OneRichText = new FormatStringSpecPropertyType(1, true);
    public static readonly FormatStringSpecPropertyType TwoRichText = new FormatStringSpecPropertyType(2, true);

    private static readonly object Boxed999 = 999;

    private object[]? _formatArgs;

    /// <summary>
    /// Number of formatting arguments.
    /// </summary>
    public int ArgumentCount { get; }

    /// <summary>
    /// If rich text should be allowed.
    /// </summary>
    public bool AllowRichText { get; }

    protected override ISpecDynamicValue CreateValue(string? value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcrete<string>();
    }

    public FormatStringSpecPropertyType(int argCount, bool allowRichText)
    {
        if (argCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(argCount));

        ArgumentCount = argCount;
        AllowRichText = allowRichText;
    }

    /// <inheritdoc />
    public override string Type => "FormatString";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Format String";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode)
        {
            return FailedToParse(in parse, out value);
        }

        if (parse.HasDiagnostics)
        {
            if (!AllowRichText && KnownTypeValueHelper.ContainsRichText(strValNode.Value))
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = strValNode.Range,
                    Diagnostic = DatDiagnostics.UNT1006,
                    Message = DiagnosticResources.UNT1006
                });
            }
            
            if (_formatArgs == null)
            {
                _formatArgs = new object[ArgumentCount];
                for (int i = 0; i < _formatArgs.Length; ++i)
                    _formatArgs[i] = Boxed999;
            }

            bool malformed = false;
            try
            {
                _ = string.Format(strValNode.Value, _formatArgs);
            }
            catch (FormatException ex)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = strValNode.Range,
                    Diagnostic = DatDiagnostics.UNT2012,
                    Message = !string.IsNullOrEmpty(ex.Message)
                        ? string.Format(DiagnosticResources.UNT2012_WithMessage, ex.Message)
                        : DiagnosticResources.UNT2012
                });
                malformed = true;
            }

            if (!malformed)
            {
                for (uint i = 0; i < ArgumentCount; ++i)
                {
                    string iStr = i.ToString(CultureInfo.InvariantCulture);
                    if (strValNode.Value.Contains($"{{{iStr}"))
                        continue;

                    parse.Log(new DatDiagnosticMessage
                    {
                        Range = strValNode.Range,
                        Diagnostic = DatDiagnostics.UNT102,
                        Message = string.Format(DiagnosticResources.UNT102, iStr)
                    });
                }
            }
        }

        value = strValNode.Value;
        return true;
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        stringValue ??= span.ToString();
        dynamicValue = SpecDynamicValue.String(stringValue, this);
        return true;
    }
}
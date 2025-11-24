using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A string that doesn't support rich text tags.
/// <para>Example: <c>$local$::ItemAsset.Name</c></para>
/// <code>
/// Prop Plain Text
/// </code>
/// <para>
/// Supports the <c>SupportsNewLines</c> additional property which indicates whether or not &lt;br&gt; tags can be used.
/// </para>
/// <para>
/// Also supports the <c>MinimumCount</c> and <c>MaximumCount</c> properties for character count limits.
/// </para>
/// </summary>
public sealed class StringSpecPropertyType : BaseSpecPropertyType<StringSpecPropertyType, string>, IStringParseableSpecPropertyType
{
    public static readonly StringSpecPropertyType Instance = new StringSpecPropertyType();

    public override int GetHashCode() => 49;

    static StringSpecPropertyType() { }
    private StringSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "String";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Text";

    protected override ISpecDynamicValue CreateValue(string value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcrete<string>();
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode)
        {
            return FailedToParse(in parse, out value);
        }

        if (parse.HasDiagnostics)
        {
            KnownTypeValueHelper.TryGetMinimaxCountWarning(strValNode.Value.Length, in parse);
            KnownTypeValueHelper.CheckValidLineBreakOptions(strValNode, in parse);

            if (KnownTypeValueHelper.ContainsRichText(strValNode.Value))
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = strValNode.Range,
                    Diagnostic = DatDiagnostics.UNT1006,
                    Message = DiagnosticResources.UNT1006
                });
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
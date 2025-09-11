using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class RichTextStringSpecPropertyType : BasicSpecPropertyType<RichTextStringSpecPropertyType, string>, IStringParseableSpecPropertyType
{
    public static readonly RichTextStringSpecPropertyType Instance = new RichTextStringSpecPropertyType();

    static RichTextStringSpecPropertyType() { }
    private RichTextStringSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "RichTextString";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Rich Text";

    protected override ISpecDynamicValue CreateValue(string? value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

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

        if (parse.Node is not AssetFileStringValueNode strValNode)
        {
            return FailedToParse(in parse, out value);
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
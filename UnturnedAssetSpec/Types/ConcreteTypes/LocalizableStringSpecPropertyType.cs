using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class LocalizableStringSpecPropertyType : BasicSpecPropertyType<LocalizableStringSpecPropertyType, string>
{
    public static readonly LocalizableStringSpecPropertyType Instance = new LocalizableStringSpecPropertyType();

    static LocalizableStringSpecPropertyType() { }
    private LocalizableStringSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "LocalizableString";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Localizable Text";

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


        // todo: is in local ? unnecessary : suggestion(localize)
        value = strValNode.Value;
        return true;
    }
}
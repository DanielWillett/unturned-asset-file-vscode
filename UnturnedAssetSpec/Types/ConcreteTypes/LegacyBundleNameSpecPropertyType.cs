using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class LegacyBundleNameSpecPropertyType : BasicSpecPropertyType<LegacyBundleNameSpecPropertyType, string>
{
    public static readonly LegacyBundleNameSpecPropertyType Instance = new LegacyBundleNameSpecPropertyType();

    static LegacyBundleNameSpecPropertyType() { }
    private LegacyBundleNameSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "LegacyBundleName";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Legacy Bundle Name";

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

        // todo: file checking

        value = strValNode.Value;
        return true;
    }
}
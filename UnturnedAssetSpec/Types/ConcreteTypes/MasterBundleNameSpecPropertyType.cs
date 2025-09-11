using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class MasterBundleNameSpecPropertyType : BasicSpecPropertyType<MasterBundleNameSpecPropertyType, string>
{
    public static readonly MasterBundleNameSpecPropertyType Instance = new MasterBundleNameSpecPropertyType();

    static MasterBundleNameSpecPropertyType() { }
    private MasterBundleNameSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "MasterBundleName";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Masterbundle Name";

    protected override ISpecDynamicValue CreateValue(string? value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

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
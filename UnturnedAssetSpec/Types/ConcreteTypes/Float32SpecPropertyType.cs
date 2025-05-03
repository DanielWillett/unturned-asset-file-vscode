using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Float32SpecPropertyType : BasicSpecPropertyType<Float32SpecPropertyType, float>
{
    public static readonly Float32SpecPropertyType Instance = new Float32SpecPropertyType();

    static Float32SpecPropertyType() { }
    private Float32SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Float32";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Single-Precision Decimal";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out float value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseFloat(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
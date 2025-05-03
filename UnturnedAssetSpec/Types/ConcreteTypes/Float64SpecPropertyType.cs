using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Float64SpecPropertyType : BasicSpecPropertyType<Float64SpecPropertyType, double>
{
    public static readonly Float64SpecPropertyType Instance = new Float64SpecPropertyType();

    static Float64SpecPropertyType() { }
    private Float64SpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Float64";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Double-Precision Decimal";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out double value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseDouble(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}
namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class PositionSpecPropertyType : Vector3SpecPropertyType
{
    public static PositionSpecPropertyType Instance = new PositionSpecPropertyType();
    static PositionSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Position";

    /// <inheritdoc />
    public override string DisplayName => "Position (New)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;
}

public sealed class PositionOrLegacySpecPropertyType : Vector3SpecPropertyType
{
    public static PositionOrLegacySpecPropertyType Instance = new PositionOrLegacySpecPropertyType();
    static PositionOrLegacySpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "PositionOrLegacy";

    /// <inheritdoc />
    public override string DisplayName => "Position (New|Old)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;
}

public sealed class LegacyPositionSpecPropertyType : Vector3SpecPropertyType
{
    public static LegacyPositionSpecPropertyType Instance = new LegacyPositionSpecPropertyType();
    static LegacyPositionSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "LegacyPosition";

    /// <inheritdoc />
    public override string DisplayName => "Position (Old)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Legacy;
}
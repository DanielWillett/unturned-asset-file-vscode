namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class PositionSpecPropertyType : Vector3SpecPropertyType
{
    public static PositionSpecPropertyType Instance = new PositionSpecPropertyType();
    static PositionSpecPropertyType() { }

    public override int GetHashCode() => 40;

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

    public override int GetHashCode() => 41;

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

    public override int GetHashCode() => 42;

    /// <inheritdoc />
    public override string Type => "LegacyPosition";

    /// <inheritdoc />
    public override string DisplayName => "Position (Old)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Legacy;
}
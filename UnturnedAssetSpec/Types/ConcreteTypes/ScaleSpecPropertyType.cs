namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class ScaleSpecPropertyType : Vector3SpecPropertyType
{
    public static ScaleSpecPropertyType Instance = new ScaleSpecPropertyType();
    static ScaleSpecPropertyType() { }

    public override int GetHashCode() => 44;

    /// <inheritdoc />
    public override string Type => "Scale";

    /// <inheritdoc />
    public override string DisplayName => "Scale (New)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;
}

public sealed class ScaleOrLegacySpecPropertyType : Vector3SpecPropertyType
{
    public static ScaleOrLegacySpecPropertyType Instance = new ScaleOrLegacySpecPropertyType();
    static ScaleOrLegacySpecPropertyType() { }

    public override int GetHashCode() => 45;

    /// <inheritdoc />
    public override string Type => "ScaleOrLegacy";

    /// <inheritdoc />
    public override string DisplayName => "Scale (New|Old)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;
}

public sealed class LegacyScaleSpecPropertyType : Vector3SpecPropertyType
{
    public static LegacyScaleSpecPropertyType Instance = new LegacyScaleSpecPropertyType();
    static LegacyScaleSpecPropertyType() { }

    public override int GetHashCode() => 46;

    /// <inheritdoc />
    public override string Type => "LegacyScale";

    /// <inheritdoc />
    public override string DisplayName => "Scale (Old)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Legacy;
}
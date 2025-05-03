namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class ScaleSpecPropertyType : Vector3SpecPropertyType
{
    public static ScaleSpecPropertyType Instance = new ScaleSpecPropertyType();
    static ScaleSpecPropertyType() { }

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

    /// <inheritdoc />
    public override string Type => "LegacyScale";

    /// <inheritdoc />
    public override string DisplayName => "Scale (Old)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Legacy;
}
namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class EulerRotationSpecPropertyType : Vector3SpecPropertyType
{
    public static EulerRotationSpecPropertyType Instance = new EulerRotationSpecPropertyType();
    static EulerRotationSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "EulerRotation";

    /// <inheritdoc />
    public override string DisplayName => "Euler Rotation (New)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;
}

public sealed class EulerRotationOrLegacySpecPropertyType : Vector3SpecPropertyType
{
    public static EulerRotationOrLegacySpecPropertyType Instance = new EulerRotationOrLegacySpecPropertyType();
    static EulerRotationOrLegacySpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "EulerRotationOrLegacy";

    /// <inheritdoc />
    public override string DisplayName => "Euler Rotation (New|Old)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;
}

public sealed class LegacyEulerRotationSpecPropertyType : Vector3SpecPropertyType
{
    public static LegacyEulerRotationSpecPropertyType Instance = new LegacyEulerRotationSpecPropertyType();
    static LegacyEulerRotationSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "LegacyEulerRotation";

    /// <inheritdoc />
    public override string DisplayName => "Euler Rotation (Old)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Legacy;
}
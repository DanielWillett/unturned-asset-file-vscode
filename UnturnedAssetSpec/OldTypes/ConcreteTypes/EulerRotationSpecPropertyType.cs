namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A Euler XYZ rotation or rotation offset in degrees formatted either as an object or as a composite string.
/// <para>Component values range between 0 inclusively and 360 exclusively.</para>
/// <para>Example: <c>FoliageInfoAsset.Normal_Rotation_Offset</c></para>
/// <code>
/// Prop (85.5, 180.0, 270.0)
/// 
/// // or
/// 
/// Prop
/// {
///     X 85.5
///     Y 180.0
///     Z 270.0
/// }
/// </code>
/// </summary>
public sealed class EulerRotationSpecPropertyType : Vector3SpecPropertyType
{
    public static EulerRotationSpecPropertyType Instance = new EulerRotationSpecPropertyType();
    static EulerRotationSpecPropertyType() { }

    public override int GetHashCode() => 18;

    /// <inheritdoc />
    public override string Type => "EulerRotation";

    /// <inheritdoc />
    public override string DisplayName => "Euler Rotation";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;
}

/// <summary>
/// A Euler XYZ rotation or rotation offset in degrees formatted as either as an object, composite string, or legacy composite object.
/// <para>Component values range between 0 inclusively and 360 exclusively.</para>
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop (85.5, 180.0, 270.0)
/// 
/// // or
/// 
/// Prop
/// {
///     X 85.5
///     Y 180.0
///     Z 270.0
/// }
///
/// // or (deprecated)
/// 
/// Prop_X 85.5
/// Prop_Y 180.0
/// Prop_Z 270.0
/// </code>
/// </summary>
public sealed class EulerRotationOrLegacySpecPropertyType : Vector3SpecPropertyType
{
    public static EulerRotationOrLegacySpecPropertyType Instance = new EulerRotationOrLegacySpecPropertyType();
    static EulerRotationOrLegacySpecPropertyType() { }

    public override int GetHashCode() => 19;

    /// <inheritdoc />
    public override string Type => "EulerRotationOrLegacy";

    /// <inheritdoc />
    public override string DisplayName => "Euler Rotation (or Legacy)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;
}

/// <summary>
/// A Euler XYZ rotation or rotation offset in degrees formatted as a legacy composite object.
/// <para>Component values range between 0 inclusively and 360 exclusively.</para>
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop_X 85.5
/// Prop_Y 180.0
/// Prop_Z 270.0
/// </code>
/// </summary>
public sealed class LegacyEulerRotationSpecPropertyType : Vector3SpecPropertyType
{
    public static LegacyEulerRotationSpecPropertyType Instance = new LegacyEulerRotationSpecPropertyType();
    static LegacyEulerRotationSpecPropertyType() { }

    public override int GetHashCode() => 20;

    /// <inheritdoc />
    public override string Type => "LegacyEulerRotation";

    /// <inheritdoc />
    public override string DisplayName => "Euler Rotation (only Legacy)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Legacy;
}
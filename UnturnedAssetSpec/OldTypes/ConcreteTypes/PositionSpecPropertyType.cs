namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An XYZ position or offset in meters formatted either as an object or as a composite string.
/// <para>Example: <c>ItemCaliberAsset.AimAlignment_LocalOffset</c></para>
/// <code>
/// Prop (-10.0, 52.1, 0.0)
/// 
/// // or
/// 
/// Prop
/// {
///     X -10.0
///     Y 52.1
///     Z 0.0
/// }
/// </code>
/// </summary>
public sealed class PositionSpecPropertyType : Vector3SpecPropertyType
{
    public static PositionSpecPropertyType Instance = new PositionSpecPropertyType();
    static PositionSpecPropertyType() { }

    public override int GetHashCode() => 40;

    /// <inheritdoc />
    public override string Type => "Position";

    /// <inheritdoc />
    public override string DisplayName => "Position";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;
}

/// <summary>
/// An XYZ position or offset in meters formatted as either as an object, composite string, or legacy composite object.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop (-10.0, 52.1, 0.0)
/// 
/// // or
/// 
/// Prop
/// {
///     X -10.0
///     Y 52.1
///     Z 0.0
/// }
///
/// // or (deprecated)
/// 
/// Prop_X -10.0
/// Prop_Y 52.1
/// Prop_Z 0.0
/// </code>
/// </summary>
public sealed class PositionOrLegacySpecPropertyType : Vector3SpecPropertyType
{
    public static PositionOrLegacySpecPropertyType Instance = new PositionOrLegacySpecPropertyType();
    static PositionOrLegacySpecPropertyType() { }

    public override int GetHashCode() => 41;

    /// <inheritdoc />
    public override string Type => "PositionOrLegacy";

    /// <inheritdoc />
    public override string DisplayName => "Position (or Legacy)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;
}

/// <summary>
/// An XYZ position or offset in meters formatted as a legacy composite object.
/// <para>Example: <c>ObjectAsset.LOD_Center</c></para>
/// <code>
/// Prop_X -10.0
/// Prop_Y 52.1
/// Prop_Z 0.0
/// </code>
/// </summary>
public sealed class LegacyPositionSpecPropertyType : Vector3SpecPropertyType
{
    public static LegacyPositionSpecPropertyType Instance = new LegacyPositionSpecPropertyType();
    static LegacyPositionSpecPropertyType() { }

    public override int GetHashCode() => 42;

    /// <inheritdoc />
    public override string Type => "LegacyPosition";

    /// <inheritdoc />
    public override string DisplayName => "Position (only Legacy)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Legacy;
}
namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An XYZ scale multiplier formatted either as an object or as a composite string.
/// <para>Example: <c>FoliageInfoAsset.Min_Scale</c></para>
/// <code>
/// Prop (1.0, 0.5, 2.0)
/// 
/// // or
/// 
/// Prop
/// {
///     X 1.0
///     Y 0.5
///     Z 2.0
/// }
/// </code>
/// </summary>
public sealed class ScaleSpecPropertyType : Vector3SpecPropertyType
{
    public static ScaleSpecPropertyType Instance = new ScaleSpecPropertyType();
    static ScaleSpecPropertyType() { }

    public override int GetHashCode() => 44;

    /// <inheritdoc />
    public override string Type => "Scale";

    /// <inheritdoc />
    public override string DisplayName => "Scale";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;
}

/// <summary>
/// An XYZ scale multiplier formatted as either as an object, composite string, or legacy composite object.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop (1.0, 0.5, 2.0)
/// 
/// // or
/// 
/// Prop
/// {
///     X 1.0
///     Y 0.5
///     Z 2.0
/// }
///
/// // or (deprecated)
/// 
/// Prop_X 1.0
/// Prop_Y 0.5
/// Prop_Z 2.0
/// </code>
/// </summary>
public sealed class ScaleOrLegacySpecPropertyType : Vector3SpecPropertyType
{
    public static ScaleOrLegacySpecPropertyType Instance = new ScaleOrLegacySpecPropertyType();
    static ScaleOrLegacySpecPropertyType() { }

    public override int GetHashCode() => 45;

    /// <inheritdoc />
    public override string Type => "ScaleOrLegacy";

    /// <inheritdoc />
    public override string DisplayName => "Scale (or Legacy)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;
}

/// <summary>
/// An XYZ scale multiplier formatted as a legacy composite object.
/// <para>Example: <c>ObjectAsset.LOD_Size</c></para>
/// <code>
/// Prop_X 1.0
/// Prop_Y 0.5
/// Prop_Z 2.0
/// </code>
/// </summary>
public sealed class LegacyScaleSpecPropertyType : Vector3SpecPropertyType
{
    public static LegacyScaleSpecPropertyType Instance = new LegacyScaleSpecPropertyType();
    static LegacyScaleSpecPropertyType() { }

    public override int GetHashCode() => 46;

    /// <inheritdoc />
    public override string Type => "LegacyScale";

    /// <inheritdoc />
    public override string DisplayName => "Scale (only Legacy)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Legacy;
}
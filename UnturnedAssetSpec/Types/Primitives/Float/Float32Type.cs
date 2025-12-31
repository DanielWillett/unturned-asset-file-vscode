namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A 32-bit single-precision decimal value.
/// <para>Example: <c>EffectAsset.Lifetime</c></para>
/// <code>
/// Prop 123.456
/// </code>
/// </summary>
public sealed class Float32Type : PrimitiveType<float, Float32Type>
{
    public const string TypeId = "Float32";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Float32;

    public override int GetHashCode() => 2105995431;
}
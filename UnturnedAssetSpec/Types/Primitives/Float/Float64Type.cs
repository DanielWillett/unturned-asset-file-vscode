namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A 64-bit double-precision decimal value.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop 123.456
/// </code>
/// </summary>
public sealed class Float64Type : PrimitiveType<double, Float64Type>
{
    public const string TypeId = "Float64";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Float64;

    public override int GetHashCode() => 1668525674;
}
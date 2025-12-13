namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A 128-bit high precision decimal value.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop 123.456
/// </code>
/// </summary>
public sealed class Float128Type : PrimitiveType<decimal, Float128Type>
{
    public const string TypeId = "Float128";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Float128;

    public override int GetHashCode() => 1685130768;
}
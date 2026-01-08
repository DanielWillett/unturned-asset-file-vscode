namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// The assembly-qualified name of a CLR type.
/// <para>Example: <c>WeatherAssetBase.Component_Type</c></para>
/// <code>
/// Prop SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
/// </code>
/// </summary>
/// <remarks>To specify expected base types, use <see cref="TypeReferenceType"/> instead.</remarks>
public sealed class QualifiedTypeType : PrimitiveType<QualifiedType, QualifiedTypeType>
{
    public const string TypeId = "Type";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_QualifiedType;

    public override int GetHashCode() => 1183865098;
}
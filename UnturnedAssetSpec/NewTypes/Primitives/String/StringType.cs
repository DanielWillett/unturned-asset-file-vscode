namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A string that doesn't support rich text tags.
/// <para>Example: <c>$local$::ItemAsset.Name</c></para>
/// <code>
/// Prop Plain Text
/// </code>
/// <para>
/// Supports the <c>SupportsNewLines</c> additional property which indicates whether or not &lt;br&gt; tags can be used.
/// </para>
/// <para>
/// Also supports the <c>MinimumCount</c> and <c>MaximumCount</c> properties for character count limits.
/// </para>
/// </summary>
public sealed class StringType : PrimitiveType<string, StringType>
{
    // todo: make more advanced
    public const string TypeId = "String";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_String;

    public override int GetHashCode() => 42241426;
}
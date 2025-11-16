using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// If <see cref="IsTarget"/> is <see langword="true"/>, the value of this property is the key of a localization property.
/// Otherwise, this property is a string that can either appear in the dat file or localization file.
/// <para>Example: <c>ServerListCurationAsset.Name</c></para>
/// <code>
/// // -- IsTarget=True --
/// 
///    // Asset:
///    Prop Asset_Name
///    
///    // Local:
///    Asset_Name "Some Name"
/// 
/// // -- IsTarget=False --
/// 
///    // Asset:
///    Prop "Some Name"
///
///    // - or -
/// 
///    // Local:
///    Prop "Some Name"
/// </code>
/// <para>
/// Supports the <c>SupportsNewLines</c> additional property which indicates whether or not &lt;br&gt; tags can be used.
/// </para>
/// <para>
/// Supports the <c>LocalizationKeyOverride</c> additional property which
/// overrides the localization key when used on a non-target asset property.
/// </para>
/// </summary>
public sealed class LocalizableStringSpecPropertyType : BasicSpecPropertyType<LocalizableStringSpecPropertyType, string>
{
    /// <summary>
    /// A <see cref="LocalizableStringSpecPropertyType"/> where the property
    /// does not contain rich text and can be present in either the asset or localization file.
    /// </summary>
    public static readonly LocalizableStringSpecPropertyType Instance = new LocalizableStringSpecPropertyType(false, false);

    /// <summary>
    /// A <see cref="LocalizableStringSpecPropertyType"/> where the property
    /// does not contain rich text and the value of which is the key of a localization property.
    /// </summary>
    public static readonly LocalizableStringSpecPropertyType TargetInstance = new LocalizableStringSpecPropertyType(true, false);

    /// <summary>
    /// A <see cref="LocalizableStringSpecPropertyType"/> where the property
    /// does not contain rich text and can be present in either the asset or localization file.
    /// </summary>
    public static readonly LocalizableStringSpecPropertyType RichInstance = new LocalizableStringSpecPropertyType(false, true);

    /// <summary>
    /// A <see cref="LocalizableStringSpecPropertyType"/> where the property
    /// can contain rich text and the value of which is the key of a localization property.
    /// </summary>
    public static readonly LocalizableStringSpecPropertyType TargetRichInstance = new LocalizableStringSpecPropertyType(true, true);

    public override int GetHashCode() => _isTarget ? 34 : 35 ^ ((_isRich ? 1 : 0) * 397);

    private readonly bool _isTarget;
    private readonly bool _isRich;

    /// <summary>
    /// If the value of this property is the key of the expected localization property.
    /// </summary>
    public bool IsTarget => _isTarget;

    /// <summary>
    /// Whether or not the value of the localization property can support rich text in-game.
    /// </summary>
    public bool ValueSupportsRichText => _isRich;

    static LocalizableStringSpecPropertyType() { }
    private LocalizableStringSpecPropertyType(bool isTarget, bool isRich)
    {
        _isTarget = isTarget;
        _isRich = isRich;
    }

    /// <inheritdoc />
    public override string Type => _isTarget ? _isRich ? "LocalizableTargetRichString" : "LocalizableTargetString" : _isRich ? "LocalizableRichString" : "LocalizableString";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => _isTarget ? _isRich ? "Localization Key (Rich)" : "Localization Key" : _isRich ? "Localization Text (Rich)" : "Localization Text";

    protected override ISpecDynamicValue CreateValue(string? value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode)
        {
            return FailedToParse(in parse, out value);
        }

        if (!_isTarget)
        {
            string localKey = parse.EvaluationContext.Self.Key;
            if (parse.EvaluationContext.Self.TryGetAdditionalProperty("LocalizationKeyOverride", out string? keyOverride) && !string.IsNullOrEmpty(keyOverride))
            {
                localKey = keyOverride!;
            }
            // todo: support LocalizationKeyOverride
            // todo: is in local ? unnecessary : suggestion(localize)

            if (parse.HasDiagnostics)
            {
                KnownTypeValueHelper.CheckValidLineBreakOptions(strValNode, in parse);
            }
        }
        else
        {
            // todo: does exist?
        }

        value = strValNode.Value;
        return true;
    }
}
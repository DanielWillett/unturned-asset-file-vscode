using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class LocalizableStringSpecPropertyType : BasicSpecPropertyType<LocalizableStringSpecPropertyType, string>
{
    public static readonly LocalizableStringSpecPropertyType Instance = new LocalizableStringSpecPropertyType(false, false);
    public static readonly LocalizableStringSpecPropertyType TargetInstance = new LocalizableStringSpecPropertyType(true, false);
    public static readonly LocalizableStringSpecPropertyType RichInstance = new LocalizableStringSpecPropertyType(false, true);
    public static readonly LocalizableStringSpecPropertyType TargetRichInstance = new LocalizableStringSpecPropertyType(true, true);

    public override int GetHashCode() => _isTarget ? 34 : 35;

    private readonly bool _isTarget;
    private readonly bool _isRich;

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
        }
        else
        {
            // todo: does exist?
        }

        value = strValNode.Value;
        return true;
    }
}
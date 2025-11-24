using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System.Linq;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// References a valid action key for blueprint actions which are loaded from the <c>Localization\English\Player\PlayerDashboardInventory.dat</c> file.
/// <para>Example: <c>ItemAsset.Action.CommonTextId</c></para>
/// <code>
/// Prop Store
/// </code>
/// </summary>
/// <remarks>Any lines ending in <c>_Button</c> and <c>_Button_Tooltip</c> are valid.</remarks>
public sealed class ActionKeySpecPropertyType : BaseSpecPropertyType<ActionKeySpecPropertyType, string>
{
    public static readonly ActionKeySpecPropertyType Instance = new ActionKeySpecPropertyType();

    public override int GetHashCode() => 1;

    static ActionKeySpecPropertyType() { }
    private ActionKeySpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ActionKey";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Action Button Label";

    protected override ISpecDynamicValue CreateValue(string value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

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

        if (parse.Database.ValidActionButtons.Count > 0
            && parse.HasDiagnostics
            && !parse.Database.ValidActionButtons.Contains(strValNode.Value))
        {
            parse.Log(new DatDiagnosticMessage
            {
                Range = strValNode.Range,
                Diagnostic = DatDiagnostics.UNT1014,
                Message = string.Format(DiagnosticResources.UNT1014, strValNode.Value)
            });
        }

        value = strValNode.Value;
        return true;
    }
}
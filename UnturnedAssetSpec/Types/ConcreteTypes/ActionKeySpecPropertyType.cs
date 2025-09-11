using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System.Linq;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class ActionKeySpecPropertyType : BasicSpecPropertyType<ActionKeySpecPropertyType, string>
{
    public static readonly ActionKeySpecPropertyType Instance = new ActionKeySpecPropertyType();

    static ActionKeySpecPropertyType() { }
    private ActionKeySpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ActionKey";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Action Button Label";

    protected override ISpecDynamicValue CreateValue(string? value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode)
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
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using System;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class BooleanOrFlagSpecPropertyType : BasicSpecPropertyType<BooleanOrFlagSpecPropertyType, bool>, IStringParseableSpecPropertyType, IAutoCompleteSpecPropertyType
{
    public static readonly BooleanOrFlagSpecPropertyType Instance = new BooleanOrFlagSpecPropertyType();

    static BooleanOrFlagSpecPropertyType() { }
    private BooleanOrFlagSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "BooleanOrFlag";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Boolean;

    /// <inheritdoc />
    public override string DisplayName => "Boolean or Flag";

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (span.Equals("true".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
        {
            dynamicValue = SpecDynamicValue.True;
        }
        else if (span.Equals("false".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
        {
            dynamicValue = SpecDynamicValue.False;
        }
        else if (KnownTypeValueHelper.TryParseBoolean(stringValue ?? span.ToString(), out bool result))
        {
            dynamicValue = result ? SpecDynamicValue.True : SpecDynamicValue.False;
        }
        else
        {
            dynamicValue = SpecDynamicValue.Included;
        }

        return true;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out bool value)
    {
        if (parse.Node == null)
        {
            value = false;
            return true;
        }

        if (parse.Node is AssetFileStringValueNode stringValue
            && KnownTypeValueHelper.TryParseBoolean(stringValue.Value, out bool boolValue))
        {
            value = boolValue;
            return true;
        }

        if (parse.Node != null)
        {
            FailedToParse(in parse, out value, parse.Node);
        }

        value = true;
        return true;
    }

    private static readonly Task<AutoCompleteResult[]> BooleanResults = Task.FromResult(new AutoCompleteResult[]
    {
        new AutoCompleteResult("true"),
        new AutoCompleteResult("false")
    });

    public Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters parameters,
        in FileEvaluationContext context)
    {
        return BooleanResults;
    }
}

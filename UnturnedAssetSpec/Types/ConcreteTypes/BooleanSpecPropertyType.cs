using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using System;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class BooleanSpecPropertyType : BasicSpecPropertyType<BooleanSpecPropertyType, bool>, IStringParseableSpecPropertyType, IAutoCompleteSpecPropertyType
{
    public static readonly BooleanSpecPropertyType Instance = new BooleanSpecPropertyType();

    static BooleanSpecPropertyType() { }
    private BooleanSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Boolean";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Boolean;

    /// <inheritdoc />
    public override string DisplayName => "Boolean";

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (span.Equals("true".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
        {
            dynamicValue = SpecDynamicValue.True;
            return true;
        }
        if (span.Equals("false".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
        {
            dynamicValue = SpecDynamicValue.False;
            return true;
        }

        if (KnownTypeValueHelper.TryParseBoolean(stringValue ?? span.ToString(), out bool result))
        {
            dynamicValue = result ? SpecDynamicValue.True : SpecDynamicValue.False;
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out bool value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is AssetFileStringValueNode stringValue && KnownTypeValueHelper.TryParseBoolean(stringValue.Value, out bool boolValue))
        {
            value = boolValue;
        }
        else
        {
            return FailedToParse(in parse, out value, parse.Node);
        }

        return true;
    }

    private static readonly Task<AutoCompleteResult[]> BooleanResults = Task.FromResult(new AutoCompleteResult[]
    {
        new AutoCompleteResult("true"),
        new AutoCompleteResult("false")
    });

    public Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters context)
    {
        return BooleanResults;
    }
}

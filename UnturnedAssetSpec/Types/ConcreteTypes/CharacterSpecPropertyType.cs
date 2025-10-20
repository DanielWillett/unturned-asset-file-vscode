using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class CharacterSpecPropertyType : BasicSpecPropertyType<CharacterSpecPropertyType, char>, IStringParseableSpecPropertyType
{
    public static readonly CharacterSpecPropertyType Instance = new CharacterSpecPropertyType();

    public override int GetHashCode() => 8;

    static CharacterSpecPropertyType() { }
    private CharacterSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Character";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Character";

    protected override ISpecDynamicValue CreateValue(char value) => new SpecDynamicConcreteConvertibleValue<char>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        char? c = value.AsConcreteNullable<char>();
        return c.HasValue ? new string(c.Value, 1) : null;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out char value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseCharacter(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (span.Length == 1)
        {
            dynamicValue = new SpecDynamicConcreteConvertibleValue<char>(span[0], this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }
}
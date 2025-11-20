using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A path string such as a Unity asset path.
/// <para>Example: <c>ItemSightAsset.AimAlignment_Path</c></para>
/// <code>
/// Prop Model_0/Aim
/// </code>
/// <para>
/// Also supports the <c>MinimumCount</c> and <c>MaximumCount</c> properties for character count limits.
/// </para>
/// </summary>
public sealed class PathSpecPropertyType : BasicSpecPropertyType<PathSpecPropertyType, string>, IStringParseableSpecPropertyType
{
    public static readonly PathSpecPropertyType Instance = new PathSpecPropertyType();

    public override int GetHashCode() => 90;

    static PathSpecPropertyType() { }
    private PathSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Path";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Path";

    protected override ISpecDynamicValue CreateValue(string? value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcrete<string>();
    }

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

        string val = strValNode.Value;

        if (parse.HasDiagnostics)
        {
            KnownTypeValueHelper.TryGetMinimaxCountWarning(val.Length, in parse);
            KnownTypeValueHelper.TryGetBackslashWarning(val, in parse);
        }

        value = val;
        return true;
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        stringValue ??= span.ToString();
        dynamicValue = SpecDynamicValue.String(stringValue, this);
        return true;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A beard type number or index.
/// See <seealso href="https://github.com/DanielWillett/UnturnedUIAssets/tree/main/Customization/Beards"/> for a list of beard indices.
/// <para>Example: <c>ObjectNPCAsset.Beard</c></para>
/// <code>
/// Prop 15
/// </code>
/// </summary>
public sealed class BeardIndexSpecPropertyType : BaseSpecPropertyType<BeardIndexSpecPropertyType, byte>, IStringParseableSpecPropertyType
{
    public static readonly BeardIndexSpecPropertyType Instance = new BeardIndexSpecPropertyType();

    public override int GetHashCode() => 3;

    static BeardIndexSpecPropertyType() { }
    private BeardIndexSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "BeardIndex";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Beard (Index)";

    protected override ISpecDynamicValue CreateValue(byte value) => new SpecDynamicConcreteConvertibleValue<byte>(value, this);

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (byte.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte result))
        {
            dynamicValue = SpecDynamicValue.UInt8(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<byte>()?.ToString(CultureInfo.InvariantCulture);
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out byte value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseUInt8(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        if (value < parse.Database.Information.BeardCount)
            return true;

        if (!parse.HasDiagnostics)
            return false;

        parse.Log(new DatDiagnosticMessage
        {
            Range = parse.Node.Range,
            Diagnostic = DatDiagnostics.UNT1008,
            Message = string.Format(DiagnosticResources.UNT1008_Beard, (parse.Database.Information.BeardCount - 1).ToString(CultureInfo.CurrentUICulture))
        });

        return false;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class ColorRGBSpecPropertyType : ColorSpecPropertyType
{
    public static readonly ColorRGBSpecPropertyType Instance = new ColorRGBSpecPropertyType();

    static ColorRGBSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ColorRGB";

    /// <inheritdoc />
    public override string DisplayName => "Color (RBG; 0-1)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;

    /// <inheritdoc />
    private protected override bool HasAlpha => false;
}

public sealed class ColorRGBASpecPropertyType : ColorSpecPropertyType
{
    public static readonly ColorRGBASpecPropertyType Instance = new ColorRGBASpecPropertyType();

    static ColorRGBASpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ColorRGBA";

    /// <inheritdoc />
    public override string DisplayName => "Color (RBGA; 0-1)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;

    /// <inheritdoc />
    private protected override bool HasAlpha => true;
}

public sealed class ColorRGBLegacySpecPropertyType : ColorSpecPropertyType
{
    public static readonly ColorRGBLegacySpecPropertyType Instance = new ColorRGBLegacySpecPropertyType();

    static ColorRGBLegacySpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ColorRGBLegacy";

    /// <inheritdoc />
    public override string DisplayName => "Legacy Color (RBG; 0-1)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;

    /// <inheritdoc />
    private protected override bool HasAlpha => false;
}

public sealed class ColorRGBALegacySpecPropertyType : ColorSpecPropertyType
{
    public static readonly ColorRGBALegacySpecPropertyType Instance = new ColorRGBALegacySpecPropertyType();

    static ColorRGBALegacySpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ColorRGBALegacy";

    /// <inheritdoc />
    public override string DisplayName => "Legacy Color (RBGA; 0-1)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;

    /// <inheritdoc />
    private protected override bool HasAlpha => true;
}

public abstract class ColorSpecPropertyType : BasicSpecPropertyType<ColorSpecPropertyType, Color>, IStringParseableSpecPropertyType
{
    private protected abstract VectorTypeParseOptions Options { get; }
    private protected abstract bool HasAlpha { get; }

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (KnownTypeValueHelper.TryParseColorHex(span, out Color32 value, HasAlpha))
        {
            dynamicValue = new SpecDynamicConcreteValue<Color>(value, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out Color value)
    {
        VectorTypeParseOptions options = Options;
        if (parse.Node == null)
        {
            if ((options & VectorTypeParseOptions.Legacy) == 0)
            {
                MissingNode(in parse, out value);
                value = Color.Black;
                return false;
            }

            AssetFileDictionaryValueNode? dict;
            if (parse.Parent is AssetFileKeyValuePairNode { Parent: AssetFileDictionaryValueNode dict2 })
            {
                dict = dict2;
            }
            else if (parse.Parent is AssetFileDictionaryValueNode dict3)
            {
                dict = dict3;
            }
            else
            {
                MissingNode(in parse, out value);
                value = Color.Black;
                return false;
            }

            if (parse.BaseKey == null)
            {
                MissingNode(in parse, out value);
                value = Color.Black;
                return false;
            }

            string rKey = parse.BaseKey + "_R";
            string gKey = parse.BaseKey + "_G";
            string bKey = parse.BaseKey + "_B";

            return TryParseFromDictionary(in parse, dict, rKey, gKey, bKey, HasAlpha ? parse.BaseKey + "_A" : null, false, out value);
        }

        if ((options & VectorTypeParseOptions.Object) != 0 && parse.Node is AssetFileDictionaryValueNode v3Struct)
        {
            return TryParseFromDictionary(in parse, v3Struct, "R", "G", "B", HasAlpha ? "A" : null, true, out value);
        }

        if ((options & VectorTypeParseOptions.Composite) != 0 && parse.Node is AssetFileStringValueNode strValNode)
        {
            if (KnownTypeValueHelper.TryParseColorHex(strValNode.Value.AsSpan(), out Color32 c32, HasAlpha))
            {
                value = c32;
                return true;
            }

            value = Color.Black;
            return false;
        }

        FailedToParse(in parse, out value);
        value = Color.Black;
        return false;
    }

    protected bool TryParseFromDictionary(in SpecPropertyTypeParseContext parse, AssetFileDictionaryValueNode dict,
        string rKey, string gKey, string bKey, string? aKey,
        bool shouldParseAsColor32,
        out Color value)
    {
        bool rBad = false, gBad = false, bBad = false, aBad = false;

        string? rVal = null, gVal = null, bVal = null, aVal = null;

        if (!dict.TryGetValue(rKey, out AssetFileValueNode? rKeyObj)
            || rKeyObj is not AssetFileStringValueNode rString)
        {
            rBad = true;
        }
        else
        {
            rVal = rString.Value;
        }

        if (!dict.TryGetValue(gKey, out AssetFileValueNode? gKeyObj)
            || gKeyObj is not AssetFileStringValueNode gString)
        {
            gBad = true;
        }
        else
        {
            gVal = gString.Value;
        }

        if (!dict.TryGetValue(bKey, out AssetFileValueNode? bKeyObj)
            || bKeyObj is not AssetFileStringValueNode bString)
        {
            bBad = true;
        }
        else
        {
            bVal = bString.Value;
        }

        if (!dict.TryGetValue(aKey ?? string.Empty, out AssetFileValueNode? aKeyObj)
            || aKeyObj is not AssetFileStringValueNode aString)
        {
            aBad = aKey != null;
        }
        else
        {
            aVal = aString.Value;
        }

        if (parse.HasDiagnostics)
        {
            if (rBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, rKey),
                    Range = rKeyObj?.Range ?? gKeyObj?.Range ?? bKeyObj?.Range?? aKeyObj?.Range ?? dict.Range
                });
            }

            if (gBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, gKey),
                    Range = gKeyObj?.Range ?? rKeyObj?.Range ?? bKeyObj?.Range ?? aKeyObj?.Range ?? dict.Range
                });
            }

            if (bBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, bKey),
                    Range = bKeyObj?.Range ?? rKeyObj?.Range ?? gKeyObj?.Range ?? aKeyObj?.Range ?? dict.Range
                });
            }

            if (aBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, aKey),
                    Range = aKeyObj?.Range ?? rKeyObj?.Range ?? gKeyObj?.Range ?? bKeyObj?.Range ?? dict.Range
                });
            }
        }

        if (rBad | gBad | bBad | aBad)
        {
            value = Color.Black;
            return false;
        }

        bool good = true;
        float r, g, b, a = 1f;
        if (shouldParseAsColor32)
        {
            if (!KnownTypeValueHelper.TryParseUInt8(rVal!, out byte r8))
            {
                good &= FailedToParse(in parse, out value, rKeyObj);
                r = 0;
            }
            else
            {
                r = r8 / 255f;
            }
            if (!KnownTypeValueHelper.TryParseUInt8(gVal!, out byte g8))
            {
                good &= FailedToParse(in parse, out value, gKeyObj);
                g = 0;
            }
            else
            {
                g = g8 / 255f;
            }
            if (!KnownTypeValueHelper.TryParseUInt8(bVal!, out byte b8))
            {
                good &= FailedToParse(in parse, out value, bKeyObj);
                b = 0;
            }
            else
            {
                b = b8 / 255f;
            }
            if (aKey != null)
            {
                if (!KnownTypeValueHelper.TryParseUInt8(aVal!, out byte a8))
                {
                    good &= FailedToParse(in parse, out value, aKeyObj);
                }
                else
                {
                    a = a8 / 255f;
                }
            }
        }
        else
        {
            if (!KnownTypeValueHelper.TryParseFloat(rVal!, out r))
            {
                good &= FailedToParse(in parse, out value, rKeyObj);
            }
            if (!KnownTypeValueHelper.TryParseFloat(gVal!, out g))
            {
                good &= FailedToParse(in parse, out value, gKeyObj);
            }
            if (!KnownTypeValueHelper.TryParseFloat(bVal!, out b))
            {
                good &= FailedToParse(in parse, out value, bKeyObj);
            }
            if (aKey != null && !KnownTypeValueHelper.TryParseFloat(aVal!, out a))
            {
                good &= FailedToParse(in parse, out value, aKeyObj);
            }
        }

        if (!good)
        {
            value = Color.Black;
            return false;
        }

        if (parse.HasDiagnostics)
        {
            if (a is < 0 or > 1)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = aKeyObj!.Range,
                    Diagnostic = DatDiagnostics.UNT1012,
                    Message = DiagnosticResources.UNT1012
                });
            }

            if (r is < 0 or > 1)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = rKeyObj!.Range,
                    Diagnostic = DatDiagnostics.UNT1012,
                    Message = DiagnosticResources.UNT1012
                });
            }

            if (g is < 0 or > 1)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = gKeyObj!.Range,
                    Diagnostic = DatDiagnostics.UNT1012,
                    Message = DiagnosticResources.UNT1012
                });
            }

            if (b is < 0 or > 1)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = bKeyObj!.Range,
                    Diagnostic = DatDiagnostics.UNT1012,
                    Message = DiagnosticResources.UNT1012
                });
            }
        }

        value = new Color(a, r, g, b);
        return true;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class Color32RGBSpecPropertyType : Color32SpecPropertyType
{
    public static readonly Color32RGBSpecPropertyType Instance = new Color32RGBSpecPropertyType();

    static Color32RGBSpecPropertyType() { }
    
    /// <inheritdoc />
    public override string Type => "Color32RGB";

    /// <inheritdoc />
    public override string DisplayName => "Color (RBG; 0-255)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;

    /// <inheritdoc />
    private protected override bool HasAlpha => false;
}

public sealed class Color32RGBASpecPropertyType : Color32SpecPropertyType
{
    public static readonly Color32RGBASpecPropertyType Instance = new Color32RGBASpecPropertyType();

    static Color32RGBASpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Color32RGBA";

    /// <inheritdoc />
    public override string DisplayName => "Color (RBGA; 0-255)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object;

    /// <inheritdoc />
    private protected override bool HasAlpha => true;
}

public sealed class Color32RGBLegacySpecPropertyType : Color32SpecPropertyType
{
    public static readonly Color32RGBLegacySpecPropertyType Instance = new Color32RGBLegacySpecPropertyType();

    static Color32RGBLegacySpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Color32RGBLegacy";

    /// <inheritdoc />
    public override string DisplayName => "Legacy Color (RBG; 0-255)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;

    /// <inheritdoc />
    private protected override bool HasAlpha => false;
}

public sealed class Color32RGBALegacySpecPropertyType : Color32SpecPropertyType
{
    public static readonly Color32RGBALegacySpecPropertyType Instance = new Color32RGBALegacySpecPropertyType();

    static Color32RGBALegacySpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Color32RGBALegacy";

    /// <inheritdoc />
    public override string DisplayName => "Legacy Color (RBGA; 0-255)";

    /// <inheritdoc />
    private protected override VectorTypeParseOptions Options => VectorTypeParseOptions.Composite | VectorTypeParseOptions.Object | VectorTypeParseOptions.Legacy;

    /// <inheritdoc />
    private protected override bool HasAlpha => true;
}

public abstract class Color32SpecPropertyType : BaseColorSpecPropertyType<Color32SpecPropertyType, Color32>, IStringParseableSpecPropertyType
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
            dynamicValue = new SpecDynamicConcreteValue<Color32>(value, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out Color32 value)
    {
        VectorTypeParseOptions options = Options;
        if (parse.Node == null)
        {
            if ((options & VectorTypeParseOptions.Legacy) == 0)
            {
                MissingNode(in parse, out value);
                value = Color32.Black;
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
                value = Color32.Black;
                return false;
            }

            if (parse.BaseKey == null)
            {
                MissingNode(in parse, out value);
                value = Color32.Black;
                return false;
            }

            string rKey = parse.BaseKey + "_R";
            string gKey = parse.BaseKey + "_G";
            string bKey = parse.BaseKey + "_B";

            return TryParseFromDictionary(in parse, dict, rKey, gKey, bKey, HasAlpha ? parse.BaseKey + "_A" : null, out value);
        }

        if ((options & VectorTypeParseOptions.Object) != 0 && parse.Node is AssetFileDictionaryValueNode v3Struct)
        {
            return TryParseFromDictionary(in parse, v3Struct, "R", "G", "B", HasAlpha ? "A" : null, out value);
        }

        if ((options & VectorTypeParseOptions.Composite) != 0 && parse.Node is AssetFileStringValueNode strValNode)
        {
            return KnownTypeValueHelper.TryParseColorHex(strValNode.Value.AsSpan(), out value, HasAlpha);
        }

        FailedToParse(in parse, out value);
        value = Color32.Black;
        return false;
    }

    protected bool TryParseFromDictionary(in SpecPropertyTypeParseContext parse, AssetFileDictionaryValueNode dict, string rKey, string gKey, string bKey, string? aKey, out Color32 value)
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
            value = Color32.Black;
            return false;
        }

        bool good = true;
        if (!KnownTypeValueHelper.TryParseUInt8(rVal!, out byte r))
        {
            good &= FailedToParse(in parse, out value, rKeyObj);
        }
        if (!KnownTypeValueHelper.TryParseUInt8(gVal!, out byte g))
        {
            good &= FailedToParse(in parse, out value, gKeyObj);
        }
        if (!KnownTypeValueHelper.TryParseUInt8(bVal!, out byte b))
        {
            good &= FailedToParse(in parse, out value, bKeyObj);
        }

        byte a = 255;
        if (aKey != null && !KnownTypeValueHelper.TryParseUInt8(aVal!, out a))
        {
            good &= FailedToParse(in parse, out value, aKeyObj);
        }

        if (!good)
        {
            value = Color32.Black;
            return false;
        }

        value = new Color32(a, r, g, b);
        return true;
    }
}
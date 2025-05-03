using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System.Numerics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public abstract class Vector3SpecPropertyType : BasicSpecPropertyType<Vector3SpecPropertyType, Vector3>
{
    private protected abstract VectorTypeParseOptions Options { get; }

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out Vector3 value)
    {
        VectorTypeParseOptions options = Options;
        if (parse.Node == null)
        {
            if ((options & VectorTypeParseOptions.Legacy) == 0)
            {
                return MissingNode(in parse, out value);
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
                return MissingNode(in parse, out value);
            }

            if (parse.BaseKey == null)
            {
                return MissingNode(in parse, out value);
            }

            string xKey = parse.BaseKey + "_X";
            string yKey = parse.BaseKey + "_Y";
            string zKey = parse.BaseKey + "_Z";

            return TryParseFromDictionary(in parse, dict, xKey, yKey, zKey, out value);
        }

        if ((options & VectorTypeParseOptions.Object) != 0 && parse.Node is AssetFileDictionaryValueNode v3Struct)
        {
            return TryParseFromDictionary(in parse, v3Struct, "X", "Y", "Z", out value);
        }

        if ((options & VectorTypeParseOptions.Composite) != 0 && parse.Node is AssetFileStringValueNode strValNode)
        {
            return KnownTypeValueHelper.TryParseVector3Components(strValNode.Value, out value);
        }

        return FailedToParse(in parse, out value);
    }

    protected bool TryParseFromDictionary(in SpecPropertyTypeParseContext parse, AssetFileDictionaryValueNode dict, string xKey, string yKey, string zKey, out Vector3 value)
    {
        bool xBad = false, yBad = false, zBad = false;

        string? xVal = null, yVal = null, zVal = null;

        if (!dict.TryGetValue(xKey, out AssetFileValueNode? xKeyObj)
            || xKeyObj is not AssetFileStringValueNode xString)
        {
            xBad = true;
        }
        else
        {
            xVal = xString.Value;
        }

        if (!dict.TryGetValue(xKey, out AssetFileValueNode? yKeyObj)
            || yKeyObj is not AssetFileStringValueNode yString)
        {
            yBad = true;
        }
        else
        {
            yVal = yString.Value;
        }

        if (!dict.TryGetValue(xKey, out AssetFileValueNode? zKeyObj)
            || zKeyObj is not AssetFileStringValueNode zString)
        {
            zBad = true;
        }
        else
        {
            zVal = zString.Value;
        }

        if (parse.HasDiagnostics)
        {
            if (xBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, xKey),
                    Range = xKeyObj?.Range ?? yKeyObj?.Range ?? zKeyObj?.Range ?? dict.Range
                });
            }

            if (yBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, yKey),
                    Range = yKeyObj?.Range ?? zKeyObj?.Range ?? xKeyObj?.Range ?? dict.Range
                });
            }

            if (zBad)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1007,
                    Message = string.Format(DiagnosticResources.UNT1007, parse.BaseKey, zKey),
                    Range = zKeyObj?.Range ?? yKeyObj?.Range ?? xKeyObj?.Range ?? dict.Range
                });
            }
        }

        if (xBad | yBad | zBad)
        {
            value = default;
            return false;
        }

        if (!KnownTypeValueHelper.TryParseFloat(xVal!, out float x)
            || !KnownTypeValueHelper.TryParseFloat(yVal!, out float y)
            || !KnownTypeValueHelper.TryParseFloat(zVal!, out float z))
        {
            return FailedToParse(in parse, out value);
        }

        value = new Vector3(x, y, z);
        return true;
    }
}
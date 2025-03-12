using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LspServer.Types;
public static class KnownTypes
{
    public const string Flag = "Flag";
    public const string UInt8 = "UInt8";
    public const string UInt16 = "UInt16";
    public const string UInt32 = "UInt32";
    public const string UInt64 = "UInt64";
    public const string Int8 = "Int8";
    public const string Int16 = "Int16";
    public const string Int32 = "Int32";
    public const string Int64 = "Int64";
    public const string String = "String";
    public const string Float32 = "Float32";
    public const string Float64 = "Float64";
    public const string Float128 = "Float128";
    public const string Boolean = "Boolean";
    public const string Character = "Character";
    public const string Type = "Type";
    public const string Guid = "Guid";
    public const string GuidOrId = "GuidOrId";
    public const string DateTime = "DateTime";
    public const string DatList = "List";
    public const string DatDictionary = "Dictionary";
    public const string MasterBundleName = "MasterBundleName";
    public const string LegacyBundleName = "LegacyBundleName";
    public const string AssetBundleVersion = "AssetBundleVersion";

    public static SymbolKind GetSymbolKind(string type)
    {
        if (type == null)
        {
            return SymbolKind.String;
        }

        if (type.StartsWith("I", StringComparison.OrdinalIgnoreCase))
        {
            if (type.Equals(Int8, StringComparison.OrdinalIgnoreCase)
                || type.Equals(Int16, StringComparison.OrdinalIgnoreCase)
                || type.Equals(Int32, StringComparison.OrdinalIgnoreCase)
                || type.Equals(Int64, StringComparison.OrdinalIgnoreCase))
            {
                return SymbolKind.Number;
            }
        }

        if (type.StartsWith("U", StringComparison.OrdinalIgnoreCase))
        {
            if (type.Equals(UInt8, StringComparison.OrdinalIgnoreCase)
                || type.Equals(UInt16, StringComparison.OrdinalIgnoreCase)
                || type.Equals(UInt32, StringComparison.OrdinalIgnoreCase)
                || type.Equals(UInt64, StringComparison.OrdinalIgnoreCase))
            {
                return SymbolKind.Number;
            }
        }
        
        if (type.StartsWith("F", StringComparison.OrdinalIgnoreCase))
        {
            if (type.Equals(Float32, StringComparison.OrdinalIgnoreCase)
                || type.Equals(Float64, StringComparison.OrdinalIgnoreCase)
                || type.Equals(Float128, StringComparison.OrdinalIgnoreCase))
            {
                return SymbolKind.Number;
            }
        }
        
        if (type.StartsWith("G", StringComparison.OrdinalIgnoreCase))
        {
            if (type.Equals(Guid, StringComparison.OrdinalIgnoreCase)
                || type.Equals(GuidOrId, StringComparison.OrdinalIgnoreCase))
            {
                return SymbolKind.Number;
            }
        }

        if (type.Equals(Boolean, StringComparison.OrdinalIgnoreCase))
        {
            return SymbolKind.Boolean;
        }

        return SymbolKind.String;
    }

    public static bool IsFlag(string? type) => string.Equals(type, Flag, StringComparison.OrdinalIgnoreCase);
}

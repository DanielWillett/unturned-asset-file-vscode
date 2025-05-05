using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

public static class OmniSharpAssetSpecExtensionsExtensions
{
    public static FilePosition ToFilePosition(this Position position)
    {
        return new FilePosition(position.Line + 1, position.Character + 1);
    }

    public static Position ToPosition(this FilePosition position)
    {
        return new Position(position.Line - 1, position.Character - 1);
    }

    public static FileRange ToFileRange(this Range range)
    {
        return new FileRange(range.Start.Line + 1, range.Start.Character + 1, range.End.Line + 1, range.End.Character + 1);
    }

    public static Range ToRange(this FileRange range)
    {
        return new Range(range.Start.Line - 1, range.Start.Character - 1, range.End.Line - 1, range.End.Character - 1);
    }

    public static SymbolKind GetSymbolKind(this ISpecPropertyType type)
    {
        return type.Kind switch
        {
            SpecPropertyTypeKind.String => SymbolKind.String,
            SpecPropertyTypeKind.Number => SymbolKind.Number,
            SpecPropertyTypeKind.Boolean => SymbolKind.Boolean,
            SpecPropertyTypeKind.Struct => SymbolKind.Struct,
            SpecPropertyTypeKind.Class => SymbolKind.Class,
            SpecPropertyTypeKind.Enum => SymbolKind.EnumMember,
            _ => SymbolKind.String
        };
    }
}
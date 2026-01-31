using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
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
        return new Range(range.Start.Line - 1, range.Start.Character - 1, range.End.Line - 1, range.End.Character);
    }

    public static SymbolKind GetSymbolKind(this IType type)
    {
        SymbolKindVisitor v;
        v.Kind = SymbolKind.Class;
        type.Visit(ref v);
        return v.Kind;
    }

    private struct SymbolKindVisitor : ITypeVisitor
    {
        public SymbolKind Kind;
        public void Accept<TValue>(IType<TValue> type)
            where TValue : IEquatable<TValue>
        {
            if (typeof(TValue) == typeof(string))
            {
                Kind = SymbolKind.String;
            }
            else if (typeof(TValue).IsPrimitive)
            {
                if (typeof(TValue) == typeof(sbyte)
                    || typeof(TValue) == typeof(byte)
                    || typeof(TValue) == typeof(short)
                    || typeof(TValue) == typeof(ushort)
                    || typeof(TValue) == typeof(int)
                    || typeof(TValue) == typeof(uint)
                    || typeof(TValue) == typeof(long)
                    || typeof(TValue) == typeof(ulong)
                    || typeof(TValue) == typeof(float)
                    || typeof(TValue) == typeof(double)
                    || typeof(TValue) == typeof(nint)
                    || typeof(TValue) == typeof(nuint)
                    )
                {
                    Kind = SymbolKind.Number;
                }
                else if (typeof(TValue) == typeof(char))
                {
                    Kind = SymbolKind.String;
                }
                else if (typeof(TValue) == typeof(bool))
                {
                    Kind = SymbolKind.Boolean;
                }
                else
                {
                    Kind = SymbolKind.Struct;
                }
            }
            else if (typeof(TValue).IsValueType)
            {
                if (typeof(TValue) == typeof(decimal))
                {
                    Kind = SymbolKind.Number;
                }
                if (typeof(TValue) == typeof(QualifiedType) || typeof(TValue) == typeof(QualifiedOrAliasedType))
                {
                    Kind = SymbolKind.Class;
                }
                else if (typeof(TValue).IsGenericType && typeof(TValue).GetGenericTypeDefinition() == typeof(EquatableArray<>))
                {
                    Kind = SymbolKind.Array;
                }
                else
                {
                    Kind = SymbolKind.Struct;
                }
            }
            else if (typeof(DatEnumValue).IsAssignableFrom(typeof(TValue)))
            {
                Kind = SymbolKind.Enum;
            }
            else if (typeof(TValue) == typeof(Type))
            {
                Kind = SymbolKind.Class;
            }
            else
            {
                Kind = SymbolKind.Object;
            }
        }
    }
}
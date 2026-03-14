using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

// Allowing concrete parsing would give the impression that context doesn't matter.
// It does in fact matter, it's just being accessed through a static ThreadLocal<string>
// so it doesn't need a reference to the context.

/// <summary>
/// Data-ref referencing the current element in a dictionary's key in <see cref="DictionaryTypeArgs{TValueType}.DefaultValue"/>.
/// </summary>
public sealed class KeyDataRef<TKey> : RootDataRef<TKey, KeyDataRef<TKey>>
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc />
    public override IType<TKey> Type { get; }

    /// <inheritdoc />
    public override string PropertyName => "Key";

    protected override bool IsPropertyNameKeyword => true;

    public KeyDataRef(IType<TKey> type)
    {
        Type = type;
    }

    /// <inheritdoc />
    protected override bool Equals(KeyDataRef<TKey> other)
    {
        return Type.Equals(other.Type);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(127281853, Type);
    }

    /// <inheritdoc />
    public override bool TryEvaluateValue(
        ref FileEvaluationContext ctx,
        [NotNullWhen(true)] out IType<TKey>? type,
        out Optional<TKey> value)
    {
        string? key = DictionaryType.Key.Value;

        if (key != null)
        {
            if (Type is DatEnumType enumType)
            {
                if (enumType.TryParse(key, out DatEnumValue? enumValue))
                {
                    type = Type;
                    value = new Optional<TKey>((TKey)(object)enumValue);
                    return true;
                }
            }
            else if (TypeConverters.String.TryConvertTo(new Optional<string>(key), out value))
            {
                type = Type;
                return true;
            }
        }

        type = null;
        value = Optional<TKey>.Null;
        return false;
    }
}
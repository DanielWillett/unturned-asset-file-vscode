using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A face, hairstyle, or beard index.
/// See <seealso href="https://github.com/DanielWillett/UnturnedUIAssets/tree/main/Customization"/> for a list of indices.
/// <para>Example: <c>ObjectNPCAsset.Face</c></para>
/// <code>
/// Prop 31
/// </code>
/// </summary>
internal class CharacterCosmeticIndexType : BaseType<byte, CharacterCosmeticIndexType>, ITypeParser<byte>
{
    private static readonly string[] DisplayNames =
    [
        Resources.Type_Name_BeardIndex,
        Resources.Type_Name_FaceIndex,
        Resources.Type_Name_HairIndex
    ];

    /// <inheritdoc />
    public override string Id => TypeIds[(int)Kind];

    /// <inheritdoc />
    public override string DisplayName => DisplayNames[(int)Kind];

    /// <inheritdoc />
    public override ITypeParser<byte> Parser => this;

    /// <summary>
    /// Type IDs of this type indexed by <see cref="CharacterCosmeticKind"/>.
    /// </summary>
    public static readonly ImmutableArray<string> TypeIds = ImmutableArray.Create<string>
    (
        "BeardIndex",
        "FaceIndex",
        "HairIndex"
    );

    /// <summary>
    /// The type factory for the character cosmetic index type.
    /// </summary>
    public static ITypeFactory Factory { get; } = new TypeFactoryById(
        (TypeIds[(int)CharacterCosmeticKind.Beard], (ctx, _, _) => new CharacterCosmeticIndexType(CharacterCosmeticKind.Beard, ctx.Information)),
        (TypeIds[(int)CharacterCosmeticKind.Face],  (ctx, _, _) => new CharacterCosmeticIndexType(CharacterCosmeticKind.Face, ctx.Information)),
        (TypeIds[(int)CharacterCosmeticKind.Hair],  (ctx, _, _) => new CharacterCosmeticIndexType(CharacterCosmeticKind.Hair, ctx.Information))
    );

    /// <summary>
    /// The type of cosmetic to parse.
    /// </summary>
    public CharacterCosmeticKind Kind { get; }

    /// <summary>
    /// The maximum index allowed for this type of cosmetic.
    /// </summary>
    public int MaximumValue { get; }

    /// <summary>
    /// The template for links to this cosmetic type.
    /// </summary>
    public string? CosmeticTextureTemplate { get; }

    public CharacterCosmeticIndexType(CharacterCosmeticKind kind, AssetInformation assetInfo)
    {
        if (kind is < CharacterCosmeticKind.Beard or > CharacterCosmeticKind.Hair)
            throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(CharacterCosmeticKind));

        Kind = kind;
        (MaximumValue, CosmeticTextureTemplate) = kind switch
        {
            CharacterCosmeticKind.Beard => (assetInfo.BeardCount, assetInfo.BeardTextureTemplate),
            CharacterCosmeticKind.Face  => (assetInfo.FaceCount, assetInfo.FaceTextureTemplate),
            _                           => (assetInfo.HairCount, assetInfo.HairTextureTemplate)
        };
    }

    /// <inheritdoc />
    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Id);
    }

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<byte> args, in FileEvaluationContext ctx, out Optional<byte> value)
    {
        if (!TypeParsers.UInt8.TryParse(ref args, in ctx, out value))
        {
            return false;
        }

        if (MaximumValue > 0 && value.HasValue && value.Value > MaximumValue)
        {
            args.DiagnosticSink?.UNT1008(ref args, Kind, value.Value, MaximumValue);
        }

        return true;
    }

    /// <inheritdoc />
    public bool TryReadValueFromJson(in JsonElement json, out Optional<byte> value, IType<byte> valueType)
    {
        return TypeParsers.UInt8.TryReadValueFromJson(in json, out value, valueType);
    }

    /// <inheritdoc />
    public void WriteValueToJson(Utf8JsonWriter writer, byte value, IType<byte> valueType, JsonSerializerOptions options)
    {
        TypeParsers.UInt8.WriteValueToJson(writer, value, valueType, options);
    }

    /// <inheritdoc />
    protected override bool Equals(CharacterCosmeticIndexType other)
    {
        return other.Kind == Kind;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(572315408, Kind);
    }
}

/// <summary>
/// The kind of cosmetic to reference.
/// </summary>
public enum CharacterCosmeticKind
{
    /// <summary>
    /// A beard index.
    /// </summary>
    Beard,

    /// <summary>
    /// A face index.
    /// </summary>
    Face,

    /// <summary>
    /// A hairstyle index.
    /// </summary>
    Hair
}
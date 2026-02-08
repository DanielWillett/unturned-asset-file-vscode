using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

[StringParseableType]
internal class BlueprintItemTypeConverter : ITypeConverter<DatObjectValue>
{
    private readonly DatCustomType _type;
    private readonly DatProperty _idProperty;
    private readonly DatProperty _amtProperty;
    private readonly IType<GuidOrId> _idType;

    public IType<DatObjectValue> DefaultType => _type;

    public BlueprintItemTypeConverter(DatCustomType type)
    {
        _type = type;

        _idProperty = type.Properties.First(
            x => x.Variable.TryGetConcreteValueAs(out Optional<string> var) && string.Equals(var.Value, "ItemRef", StringComparison.OrdinalIgnoreCase)
        );

        _amtProperty = type.Properties.First(
            x => x.Variable.TryGetConcreteValueAs(out Optional<string> var) && string.Equals(var.Value, "amount", StringComparison.OrdinalIgnoreCase)
        );

        _idType = new BackwardsCompatibleAssetReferenceType(
            BackwardsCompatibleAssetReferenceKind.BcAssetReference,
            baseTypes: [ QualifiedType.ItemAssetType ],
            type.Context,
            supportsThis: true
        );
    }

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<DatObjectValue> args, [MaybeNullWhen(false)] out DatObjectValue parsedValue)
    {
        parsedValue = null;
        if (text.IsEmpty)
            return false;

        IAssetSourceFile? assetFile = args.ValueNode?.File switch
        {
            ILocalizationSourceFile lcl => lcl.Asset,
            IAssetSourceFile asset => asset,
            _ => null
        };

        GuidOrId assetContext = GuidOrId.Empty;
        if (assetFile != null)
        {
            Guid? guid = assetFile.Guid;
            assetContext = guid.HasValue ? new GuidOrId(guid.Value) : new GuidOrId(assetFile.Id.GetValueOrDefault());
        }

        if (!KnownTypeValueHelper.TryParseItemString(text, args.TextAsString, out GuidOrId assetRef, out int amount, assetContext))
        {
            return false;
        }

        ImmutableArray<DatObjectPropertyValue>.Builder bldr = ImmutableArray.CreateBuilder<DatObjectPropertyValue>(2);
        bldr.Add(new DatObjectPropertyValue(Value.Create(assetRef, _idType), _idProperty));
        bldr.Add(new DatObjectPropertyValue(Value.Create(amount, Int32Type.Instance), _amtProperty));
        parsedValue = new DatObjectValue(_type, bldr.MoveToImmutableOrCopy());
        return true;
    }

    private static GuidOrId TryGetGuidOrId(DatObjectValue value)
    {
        if (value.TryGetProperty("ID", out DatObjectPropertyValue idVal)
            && idVal.Value.TryGetConcreteValueAs(out Optional<GuidOrId> guidOrId))
        {
            return guidOrId.Value;
        }

        return GuidOrId.Empty;
    }

    public string Format(DatObjectValue value, ref TypeConverterFormatArgs args)
    {
        GuidOrId id = TryGetGuidOrId(value);

        int amount = 1;
        if (value.TryGetProperty("Amount", out DatObjectPropertyValue amtVal)
            && amtVal.Value.TryGetConcreteValueAs(out Optional<int> amt))
        {
            amount = amt.Value;
        }

        if (!id.IsNull)
        {
            if (amount > 1)
            {
                return id.IsId
                    ? $"{id.Id.ToString(CultureInfo.InvariantCulture)}x{amount}"
                    : $"{id.Guid.ToString("N", CultureInfo.InvariantCulture)}x{amount}";
            }

            return id.IsId ? id.Id.ToString(CultureInfo.InvariantCulture) : $"{id.Guid:N}x{amount}";
        }

        return "0";
    }

    public bool TryFormat(Span<char> output, DatObjectValue value, out int size, ref TypeConverterFormatArgs args)
    {
        string fmt = args.FormatCache ?? Format(value, ref args);
        size = fmt.Length;
        if (output.Length < fmt.Length)
        {
            args.FormatCache = fmt;
            return false;
        }

        fmt.AsSpan().CopyTo(output);
        return true;
    }

    public bool TryConvertTo<TTo>(Optional<DatObjectValue> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue || obj.Value == null)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        GuidOrId id = TryGetGuidOrId(obj.Value);
        return TypeConverters.GuidOrId.TryConvertTo(id, out result);
    }

    public void WriteJson(Utf8JsonWriter writer, DatObjectValue value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        ((ITypeParser<DatObjectValue>)_type).WriteValueToJson(writer, value, _type, options);
    }

    public bool TryReadJson(in JsonElement json, out Optional<DatObjectValue> value, ref TypeConverterParseArgs<DatObjectValue> args)
    {
        return ((ITypeParser<DatObjectValue>)_type).TryReadValueFromJson(in json, out value, _type);
    }
}

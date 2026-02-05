using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Base type for most vector/color types.
/// </summary>
/// <typeparam name="TVector">The vector type.</typeparam>
/// <typeparam name="TSelf">This type's concrete implementation.</typeparam>
public abstract class BaseVectorType<TVector, TSelf> :
    BaseType<TVector, TSelf>, ITypeParser<TVector>, ITypeFactory, ITypeConverter<TVector>
    where TVector : IEquatable<TVector>
    where TSelf : BaseVectorType<TVector, TSelf>
{
    public VectorTypeOptions Options { get; }

    public override ITypeParser<TVector> Parser => this;

    public override PropertySearchTrimmingBehavior TrimmingBehavior => (Options & VectorTypeOptions.Legacy) != 0
        ? PropertySearchTrimmingBehavior.CreatesOtherPropertiesInSameFileAtSameLevel
        : PropertySearchTrimmingBehavior.ExactPropertyOnly;

    protected BaseVectorType(VectorTypeOptions options)
    {
        Options = options;
    }

    protected abstract bool TryParseLegacy(ref TypeParserArgs<TVector> args, IDictionarySourceNode dictionary, string baseKey, out TVector value, out bool hadOneComp);

    protected abstract bool TryParseFromDictionary(ref TypeParserArgs<TVector> args, IDictionarySourceNode dict, out TVector value);

    protected abstract bool TryParseFromString(ReadOnlySpan<char> text, out TVector value, ISourceNode? property, ref TypeConverterParseArgs<TVector> args);

    protected virtual bool TryFormat(Span<char> output, TVector value, out int size, ref TypeConverterFormatArgs args)
    {
        string str = args.FormatCache ?? ToString(value);
        size = str.Length;
        if (str.AsSpan().TryCopyTo(output))
            return true;
        args.FormatCache = str;
        return false;
    }

    protected virtual string ToString(TVector vector)
    {
        return vector.ToString() ?? string.Empty;
    }

    protected abstract IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context = "");

    public bool TryParse(ref TypeParserArgs<TVector> args, in FileEvaluationContext ctx, out Optional<TVector> value)
    {
        value = Optional<TVector>.Null;
        TVector parsed;

        bool sendNullMsg = true;
        switch (args.ValueNode)
        {
            default:
                if ((Options & VectorTypeOptions.Legacy) != 0
                    // get parent dictionary
                    && (args.ParentNode as IDictionarySourceNode ?? (args.ParentNode as IPropertySourceNode)?.Parent as IDictionarySourceNode) is { } dictionary
                    && args.Property != null)
                {
                    if (TryParseLegacy(ref args, dictionary, args.Property.Key, out parsed, out bool hadOneComp))
                    {
                        if (args.ParentNode is IPropertySourceNode prop)
                            args.ReferencedPropertySink?.AcceptDereferencedProperty(prop);

                        value = parsed;
                        return true;
                    }

                    if (hadOneComp)
                    {
                        if (args.ParentNode is IPropertySourceNode prop)
                            args.ReferencedPropertySink?.AcceptDereferencedProperty(prop);
                        return false;
                    }
                }

                if (args.MissingValueBehavior != TypeParserMissingValueBehavior.FallbackToDefaultValue)
                {
                    if (sendNullMsg)
                    {
                        if ((Options & VectorTypeOptions.String) != 0)
                        {
                            args.DiagnosticSink?.UNT2004_NoValue(ref args, args.ParentNode);
                        }
                        else if ((Options & VectorTypeOptions.Object) != 0)
                        {
                            args.DiagnosticSink?.UNT2004_NoDictionary(ref args, args.ParentNode);
                        }
                        else
                        {
                            args.DiagnosticSink?.UNT2004_Generic(ref args, "<no value>", args.Type);
                        }
                    }
                }
                else
                {
                    if (args.Property?.GetIncludedDefaultValue(args.ParentNode is IPropertySourceNode) is { } defValue)
                    {
                        return defValue.TryGetValueAs(in ctx, out value);
                    }

                    return false;
                }

                break;

            case IListSourceNode l:
                if ((Options & VectorTypeOptions.String) != 0)
                {
                    args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, l, args.Type);
                }
                else if ((Options & VectorTypeOptions.Object) != 0)
                {
                    args.DiagnosticSink?.UNT2004_ListInsteadOfDictionary(ref args, l, args.Type);
                }
                else
                {
                    goto default;
                }
                break;

            case IValueSourceNode v:
                if ((Options & VectorTypeOptions.String) == 0)
                {
                    if ((Options & VectorTypeOptions.Object) != 0)
                    {
                        sendNullMsg = false;
                        args.DiagnosticSink?.UNT2004_ValueInsteadOfDictionary(ref args, v, args.Type);
                    }

                    if ((Options & VectorTypeOptions.Legacy) != 0)
                    {
                        goto default;
                    }

                    break;
                }

                args.CreateTypeConverterParseArgs(out TypeConverterParseArgs<TVector> parseArgs, v.Value);
                if (!TryParseFromString(v.Value, out parsed, args.ReferenceNode, ref parseArgs))
                {
                    sendNullMsg = false;
                    if (!args.ShouldIgnoreFailureDiagnostic)
                    {
                        args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
                    }
                    if ((Options & VectorTypeOptions.Legacy) != 0)
                        goto default;

                    return false;
                }

                value = parsed;
                return true;

            case IDictionarySourceNode d:
                if ((Options & VectorTypeOptions.Object) == 0)
                {
                    if ((Options & VectorTypeOptions.String) != 0)
                    {
                        sendNullMsg = false;
                        args.DiagnosticSink?.UNT2004_DictionaryInsteadOfValue(ref args, d, args.Type);
                    }

                    if ((Options & VectorTypeOptions.Legacy) != 0)
                    {
                        goto default;
                    }

                    break;
                }

                // check UnityDatColorEx.LegacyParseColor32RGB... if the dictionary is present it always returns true no matter what, so it wouldn't parse the colors here.
                TryParseFromDictionary(ref args, d, out parsed);
                value = parsed;
                return true;
        }

        return false;
    }

    public virtual bool TryReadValueFromJson(in JsonElement json, out Optional<TVector> value, IType<TVector> valueType)
    {
        if (json.ValueKind == JsonValueKind.Null)
        {
            value = Optional<TVector>.Null;
            return true;
        }

        if (json.ValueKind == JsonValueKind.Number
            && json.TryGetDouble(out double dbl)
            && VectorTypes.TryGetProvider<TVector>() is { } vectorProvider)
        {
            value = vectorProvider.Construct(dbl);
            return true;
        }

        TypeConverterParseArgs<TVector> args = default;
        args.Type = valueType;
        if (json.ValueKind != JsonValueKind.String || !TryParseFromString(json.GetString()!, out TVector v, null, ref args))
        {
            value = Optional<TVector>.Null;
            return false;
        }

        value = v;
        return true;
    }

    public virtual void WriteValueToJson(Utf8JsonWriter writer, TVector value, IType<TVector> valueType, JsonSerializerOptions options)
    {
        writer.WriteStringValue(ToString(value));
    }

    protected void WriteOptions(Utf8JsonWriter writer)
    {
        if (Options != VectorTypeOptions.Default)
        {
            writer.WriteString("Mode"u8, Options.ToString());
        }
    }

    protected static VectorTypeOptions ReadOptions(in JsonElement typeDefinition, DatProperty owner, string context)
    {
        if (typeDefinition.ValueKind != JsonValueKind.Object)
        {
            return VectorTypeOptions.Default;
        }

        VectorTypeOptions mode = VectorTypeOptions.Default;
        if (typeDefinition.TryGetProperty("Mode"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
        {
            if (!Enum.TryParse(element.GetString(), out mode))
            {
                throw new JsonException(
                    string.Format(
                        Resources.JsonException_FailedToParseEnum,
                        nameof(VectorTypeOptions),
                        element.GetString(),
                        context.Length != 0 ? $"{owner.FullName}.{context}.Mode" : $"{owner.FullName}.Mode"
                    )
                );
            }
        }

        return mode;
    }

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<TVector> args, [MaybeNullWhen(false)] out TVector parsedValue)
    {
        return TryParseFromString(text, out parsedValue, null, ref args);
    }

    string ITypeConverter<TVector>.Format(TVector value, ref TypeConverterFormatArgs args)
    {
        return ToString(value);
    }

    bool ITypeConverter<TVector>.TryFormat(Span<char> output, TVector value, out int size, ref TypeConverterFormatArgs args)
    {
        return TryFormat(output, value, out size, ref args);
    }


    public bool TryConvertTo<TTo>(Optional<TVector> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(TVector))
        {
            result = Unsafe.As<Optional<TVector>, Optional<TTo>>(ref obj);
            return true;
        }

        if (typeof(TTo) == typeof(string))
        {
            result = MathMatrix.As<string, TTo>(ToString(obj.Value));
            return true;
        }

        IVectorTypeProvider<TVector> provider = VectorTypes.GetProvider<TVector>();
        if (typeof(TTo) == typeof(double))
        {
            result = MathMatrix.As<double, TTo>(provider.GetComponent(obj.Value, 0));
            return true;
        }

        if (TypeConverters.IsNumericConvertible<TTo>())
        {
            return TypeConverters.Float64.TryConvertTo(provider.GetComponent(obj.Value, 0), out result);
        }

        if (VectorTypes.TryGetProvider<TTo>() is { } toProvider)
        {
            int newSize = toProvider.Size;
            Span<double> components = stackalloc double[newSize];
            provider.Deconstruct(obj.Value, components);
            result = toProvider.Construct(components);
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    IType<TVector> ITypeConverter<TVector>.DefaultType => this;

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        return CreateType(in typeDefinition, typeId, spec, owner, context);
    }

    void ITypeConverter<TVector>.WriteJson(Utf8JsonWriter writer, TVector value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        WriteValueToJson(writer, value, this, options);
    }

    bool ITypeConverter<TVector>.TryReadJson(in JsonElement json, out Optional<TVector> value, ref TypeConverterParseArgs<TVector> args)
    {
        return TryReadValueFromJson(in json, out value, this);
    }

    protected bool EqualsHelper(TSelf other)
    {
        return Options == other.Options;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(1016165920, (int)Options);
    }
}

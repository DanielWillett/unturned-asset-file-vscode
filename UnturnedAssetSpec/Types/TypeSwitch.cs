using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A special type of <see cref="SwitchValue"/> that represents a property type that changes depending on the context.
/// </summary>
/// <remarks>Implements <see cref="IPropertyType"/>.</remarks>
public class TypeSwitch : SwitchValue<IType>, IPropertyType
{
    /// <summary>
    /// The type used for <see cref="IType"/> values.
    /// </summary>
    public static ITypeFactory ValueTypeFactory => TypeOfType.Factory;

    /// <summary>
    /// The ID of the type used for <see cref="IType"/> values.
    /// </summary>
    public const string ValueTypeId = "DanielWillett.UnturnedDataFileLspServer.Data.Types.TypeOfType, UnturnedAssetSpec";

    public TypeSwitch(IType<IType> type, ImmutableArray<ISwitchCase<IType>> cases) : base(type, cases) { }

    /// <inheritdoc />
    public bool TryGetConcreteType([NotNullWhen(true)] out IType? type)
    {
        if (!TryGetConcreteValue(out Optional<IType> typeOptional) || !typeOptional.HasValue)
        {
            type = null;
            return false;
        }

        type = typeOptional.Value;
        return true;
    }

    /// <inheritdoc />
    public bool TryEvaluateType([NotNullWhen(true)] out IType? type, in FileEvaluationContext ctx)
    {
        if (!TryEvaluateValue(out Optional<IType> typeOptional, in ctx) || !typeOptional.HasValue)
        {
            type = null;
            return false;
        }

        type = typeOptional.Value;
        return true;
    }

    /// <inheritdoc />
    public bool Equals(IPropertyType? other)
    {
        return other is TypeSwitch sw && base.Equals(sw);
    }
}

/// <summary>
/// The type used for <see cref="IType"/> values.
/// </summary>
internal class TypeOfType : BaseType<IType, TypeOfType>, ITypeParser<IType>, ITypeFactory
{
    private readonly IDatSpecificationReadContext? _context;
    private readonly DatProperty? _owner;
    private readonly string? _contextString;

    internal static readonly TypeOfType Factory = new TypeOfType();
    static TypeOfType() { }

    public TypeOfType() { }
    public TypeOfType(IDatSpecificationReadContext context, DatProperty owner, string contextString)
    {
        _context = context;
        _owner = owner;
        _contextString = contextString;
    }

    public override string Id => TypeSwitch.ValueTypeId;
    public override string DisplayName => Resources.Type_Name_ValueType;
    public override ITypeParser<IType> Parser => this;
    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Id);
    }

    protected override bool Equals(TypeOfType other) => true;
    public override int GetHashCode() => 49349843;

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        return new TypeOfType(spec, owner, context);
    }

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<IType> args, in FileEvaluationContext ctx, out Optional<IType> value)
    {
        value = Optional<IType>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryReadValueFromJson(in JsonElement json, out Optional<IType> value, IType<IType> valueType)
    {
        if (_context == null || _owner == null)
        {
            value = Optional<IType>.Null;
            return false;
        }

        value = new Optional<IType>(_context.ReadType(in json, _owner, _contextString ?? string.Empty));
        return true;
    }

    /// <inheritdoc />
    public void WriteValueToJson(Utf8JsonWriter writer, IType value, IType<IType> valueType, JsonSerializerOptions options)
    {
        value.WriteToJson(writer, options);
    }
}
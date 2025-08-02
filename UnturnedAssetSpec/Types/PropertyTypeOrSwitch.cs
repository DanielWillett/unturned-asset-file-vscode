using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A spec property or switch case for the type.
/// </summary>
public readonly struct PropertyTypeOrSwitch : IEquatable<PropertyTypeOrSwitch>
{
    public bool IsSwitch
    {
        [MemberNotNullWhen(true, nameof(TypeSwitch))]
        [MemberNotNullWhen(false, nameof(Type))]
        get => TypeSwitch != null;
    }

    public ISpecPropertyType? Type { get; }
    public SpecDynamicSwitchValue? TypeSwitch { get; }

    public PropertyTypeOrSwitch(ISpecPropertyType type)
    {
        Type = type;
    }
    public PropertyTypeOrSwitch(SpecDynamicSwitchValue typeSwitch)
    {
        TypeSwitch = typeSwitch;
    }

    /// <inheritdoc />
    public bool Equals(PropertyTypeOrSwitch other)
    {
        bool isSwitch = IsSwitch;
        if (other.IsSwitch != isSwitch)
            return false;

        if (isSwitch)
        {
            return other.TypeSwitch!.Equals(TypeSwitch);
        }
        else
        {
            return other.Type != null && other.Type.Equals(Type);
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is PropertyTypeOrSwitch ts && Equals(ts);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return IsSwitch ? TypeSwitch.GetHashCode() : (Type == null ? 0 : Type.GetHashCode());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (IsSwitch)
        {
            return TypeSwitch.ToString();
        }
        if (Type != null)
        {
            return Type.ToString();
        }

        return string.Empty;
    }

    public bool TryParseValue(in SpecPropertyTypeParseContext context, [MaybeNullWhen(false)] out ISpecDynamicValue propertyValue)
    {
        ISpecPropertyType? type = GetType(in context.EvaluationContext);

        if (type != null)
            return type.TryParseValue(in context, out propertyValue) && propertyValue != null;

        propertyValue = null;
        return false;
    }

    public ISpecPropertyType? GetType(in FileEvaluationContext ctx)
    {
        if (Type != null)
            return Type;

        if (TypeSwitch == null)
            return null;

        if (!TypeSwitch.TryEvaluateValue(in ctx, out ISpecPropertyType? type, out bool isNull) || isNull)
            return null;

        return type;
    }
}
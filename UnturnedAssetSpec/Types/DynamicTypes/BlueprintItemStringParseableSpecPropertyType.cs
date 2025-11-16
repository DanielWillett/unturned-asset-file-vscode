using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Base class for string-parsable type implementations for <see cref="BlueprintSupplyStringParseableSpecPropertyType"/> and <see cref="BlueprintOutputStringParseableSpecPropertyType"/> string value parsers.
/// <para>
/// Parses a blueprint item expression from it's string value.
/// <para>Example: <c>ItemAsset.Blueprint.InputItems</c></para>
/// <code>
/// Prop fe71781c60314468b22c6b0642a51cd9
/// Prop 1374
/// Prop this
/// 
/// Prop fe71781c60314468b22c6b0642a51cd9 x 5
/// Prop 1374 x 5
/// Prop this x 5
/// </code>
/// </para>
/// </summary>
public abstract class BlueprintItemStringParseableSpecPropertyType :
    BaseSpecPropertyType<CustomSpecTypeInstance>,
    ISpecPropertyType<CustomSpecTypeInstance>
{
    private readonly IAssetSpecDatabase _database;
    private ISpecType? _cachedType;
    private BlueprintIdSpecPropertyType? _bpIdType;

    public abstract string BackingType { get; }

    public Type ValueType => typeof(CustomSpecTypeInstance);

    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    protected BlueprintItemStringParseableSpecPropertyType(IAssetSpecDatabase database)
    {
        _database = database.ResolveFacade();
    }

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (TryParseValue(in parse, out CustomSpecTypeInstance? instance))
        {
            value = (ISpecDynamicValue?)instance ?? SpecDynamicValue.Null;
            return true;
        }

        value = null!;
        return false;
    }

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out CustomSpecTypeInstance? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValue)
        {
            return FailedToParse(in parse, out value, parse.Node);
        }

        GuidOrId id = parse.GetThisId();

        if (!KnownTypeValueHelper.TryParseItemString(strValue.Value.AsSpan(), strValue.Value, out GuidOrId assetRef, out int amount, id))
        {
            return FailedToParse(in parse, out value, strValue);
        }

        ISpecType? type;

        if (_cachedType == null)
        {
            _cachedType = type = _database.FindType(BackingType, parse.FileType);
        }
        else
        {
            type = _cachedType;
        }

        if (type is not CustomSpecType backingType)
        {
            return MissingType(in parse, BackingType, out value);
        }

        List<CustomSpecTypeProperty> properties = new List<CustomSpecTypeProperty>(3);
        foreach (SpecProperty property in backingType.Properties)
        {
            ISpecDynamicValue? val;
            if (property.Key.Equals("Amount", StringComparison.Ordinal))
            {
                val = SpecDynamicValue.Int32(amount);
            }
            else if (property.Key.Equals("ID", StringComparison.Ordinal))
            {
                _bpIdType ??= new BlueprintIdSpecPropertyType(_database, true);
                val = new SpecDynamicConcreteValue<GuidOrId>(assetRef, _bpIdType);
            }
            else
            {
                val = null;
            }

            properties.Add(new CustomSpecTypeProperty(val, property, property.Key));
        }

        value = new CustomSpecTypeInstance(backingType, properties);
        return true;
    }
    
    public abstract bool Equals(ISpecPropertyType? other);
    public abstract bool Equals(ISpecPropertyType<CustomSpecTypeInstance>? other);
    public abstract bool Equals(BlueprintItemStringParseableSpecPropertyType? other);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}
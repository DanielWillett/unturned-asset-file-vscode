using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public abstract class BlueprintItemStringParseableSpecPropertyType :
    BaseSpecPropertyType<CustomSpecTypeInstance>,
    ISpecPropertyType<CustomSpecTypeInstance>
{
    private ISpecType? _cachedType;
    private IAssetSpecDatabase? _cachedDatabase;

    public abstract string BackingType { get; }

    public Type ValueType => typeof(CustomSpecTypeInstance);

    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

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

        if (parse.Node is not AssetFileStringValueNode strValue)
        {
            return FailedToParse(in parse, out value, parse.Node);
        }

        GuidOrId id = parse.GetThisId();

        if (!KnownTypeValueHelper.TryParseItemString(strValue.Value.AsSpan(), strValue.Value, out GuidOrId assetRef, out int amount, id))
        {
            return FailedToParse(in parse, out value, strValue);
        }

        ISpecType? type;

        lock (this)
        {
            if (_cachedDatabase == parse.Database)
            {
                type = _cachedType;
            }
            else
            {
                _cachedDatabase = parse.Database;
                _cachedType = type = parse.Database.FindType(BackingType, parse.FileType);
            }
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
                val = new SpecDynamicConcreteValue<GuidOrId>(assetRef, BlueprintIdSpecPropertyType.Instance);
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
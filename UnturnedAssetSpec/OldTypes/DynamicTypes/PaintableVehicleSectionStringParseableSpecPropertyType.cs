using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Parses PaintableVehicleSection objects in their string representation.
/// <para>Example: <c>VehicleAsset.ExplosionBurnMaterialSections</c></para>
/// <code>
/// Prop "Path/To/Model"
/// </code>
/// </summary>
public sealed class PaintableVehicleSectionStringParseableSpecPropertyType :
    BaseSpecPropertyType<PaintableVehicleSectionStringParseableSpecPropertyType, CustomSpecTypeInstance>,
    ISpecPropertyType<CustomSpecTypeInstance>,
    IEquatable<PaintableVehicleSectionStringParseableSpecPropertyType?>
{
    private ISpecType? _cachedType;
    private IAssetSpecDatabase? _cachedDatabase;

    private const string BackingType = "SDG.Unturned.PaintableVehicleSection, Assembly-CSharp";

    public override int GetHashCode()
    {
        return 88;
    }

    public override string Type => "DanielWillett.UnturnedDataFileLspServer.Data.Types.PaintableVehicleSectionStringParseableSpecPropertyType, UnturnedAssetSpec";

    public override string DisplayName => "Paintable Vehicle Section";

    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    protected override ISpecDynamicValue CreateValue(CustomSpecTypeInstance value) => (ISpecDynamicValue?)value ?? SpecDynamicValue.Null;

    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out CustomSpecTypeInstance? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode { Value.Length: > 0 } strValue)
        {
            return FailedToParse(in parse, out value, parse.Node);
        }

        string path = strValue.Value;

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
            if (property.Key.Equals("MaterialIndex", StringComparison.Ordinal))
            {
                val = SpecDynamicValue.Int32(-1);
            }
            else if (property.Key.Equals("AllMaterials", StringComparison.Ordinal))
            {
                val = SpecDynamicValue.True;
            }
            else if (property.Key.Equals("Path", StringComparison.Ordinal))
            {
                val = SpecDynamicValue.String(path);
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

    public bool Equals(PaintableVehicleSectionStringParseableSpecPropertyType? other) => other != null;
}
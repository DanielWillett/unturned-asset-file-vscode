# Dynamic Values
All property values are represented by the [ISpecDynamicValue](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.ISpecDynamicValue.html) interface.

This interface supports dynamic values such as [Switch Statements](./switch-statements.md), [Equations](./equations.md), [Data-Refs](./data-refs.md), [Property References](./property-refs.md), [Custom Types](./custom-types.md), and concrete values.

Certain properties may specify a default type, such as [Condition Variables](/api/DanielWillett.UnturnedDataFileLspServer.Data.Logic.SpecCondition.html#DanielWillett_UnturnedDataFileLspServer_Data_Logic_SpecCondition_Variable) which default to a property, but usually the default will be a concrete value.


## Concrete Values
Unless another default is specified, this is the default type of value to parse. Concrete values are just values such as `"#ffaa33"`, `13`, `"Some Name"`, `false`, etc.

Concrete values also includes <b>Enumeration</b> (enum) values, which are special text values which have a set number of options. An example of this would be the `Action` property for guns, which includes various values such as `Trigger`, `Rocket`, `Bolt`, `String`, etc. Read more [here](./enums.md).

Concrete values are generally represented by one of the following classes:
* [SpecDynamicConcreteValue&lt;T&gt;](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.SpecDynamicConcreteValue`1.html) - for other values
    * [SpecDynamicValue](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.SpecDynamicValue.html) contains static helper methods for creating common types.

* [SpecDynamicConcreteNullValue](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.SpecDynamicConcreteNullValue.html) - for null values

* [SpecDynamicConcreteEnumValue](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.SpecDynamicConcreteEnumValue.html) - for enums

When values aren't the default value, they can be prefixed with a `%` character to indicate a concrete value.

Example:
```json
{
    // can also use "%(10)"
    "Variable": "%10",
    "Operation": "lt",
    "Comparand": 8
}
```

Most values can be represented as a string, number, boolean, or null:

* Enums are case-sensitive (when read from the spec files) and are just represented using their `Value` string. They can not be parsed from their numeric value.
* Colors are parsed from a `#rrggbbaa` or `rrggbbaa` hex string format, with the alpha `aa` being optional.
* DateTimes are parsed in any invariant format in UTC time.
* Vectors are parsed with their components comma separated, optionally wrapped in parenthesis: `(0, 0, 0)` or `0, 0, 0`.
* AssetReferences and GUIDs can be parsed in any common GUID format.
* Types (such as the type present in the Metadata for the v2 format) are parsed as a Qualified CLR Type: `SDG.Unturned.ItemAsset, Assembly-CSharp`. Version info will be ignored if included.
* BcAssetReferences can be parsed as a GUID or ushort ID. If an ID is included and an element type isn't, the ID should be prefixed with a category: `Animal:4`.
* MasterBundleReferences and AudioReferences are parsed as `bundle:path`, such as `core.masterbundle:Sounds/Inventory/LightGrab.asset`.
* Skills and BlueprintSkills are just specified by their name: `CRAFT`, `VITALITY`.
* TypeOrEnum is parsed as only a Qualified CLR Type: `SDG.Unturned.UseableMelee, Assembly-CSharp` in the spec. It can also be `"null"` (note this is a string, not a JSON null).
* List types can be represented as an array of strings.

## Property References
Property references are used to fetch values from properties.

They can be prefixed with an `@` character to indicate a property reference.

```json
"Minimum": "@Ammo_Min"
```

See [Property References](./property-refs.md) for more information.

## Equations
Equations are used to calculate values based on other values.

They can be prefixed with an `=` character to indicate an equation.

See [Equations](./equations.md) for more information.

```json
// ATAND is an arctan function returning degrees
"DefaultValue": "=ATAND(@Spread_Hip)"
```

## Data-Refs
A 'Data-Ref' is used to represent special hard-coded properties that can be useful in certain situations.

They can be prefixed with an `#` character to indicate a Data-Ref.

See [Data-Refs](./data-refs.md) for more information.

```json
{
    "Variable": "#This.AssetName",
    "Operation": "contains",
    "Comparand": "Metal"
}
```

## Switch Statements
A switch statement can be used to specify different values depending on a set of conditions.

They only supported on some properties and are formatted as an array of switch cases.

See [Switch Statements](./switch-statements.md) for more information.

```json
"DefaultValue":
[
    {
        // avoid NaN from dividing by 0
        "And":
        [
            {
                "Variable": "Some_Property",
                "Operation": "neq",
                "Comparand": 0.0
            }
        ],
        "Value": "=DIV(1 @Some_Property)"
    },
    {
        "Value": 0.0
    }
]
```
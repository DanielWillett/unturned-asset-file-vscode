# Property References

Property references are used to reference the value from another property. Property references are prefixed with a `@` unless the place you're entering it defaults to a property reference, like [Condition Variables](/api/DanielWillett.UnturnedDataFileLspServer.Data.Logic.SpecCondition.html#DanielWillett_UnturnedDataFileLspServer_Data_Logic_SpecCondition_Variable).

Property references are represented by the [PropertyRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.PropertyRef.yml) class.

## Format

Fully qualified properties contain their category, type, name, and any breadcrumbs to the property.

`@([$xx$::][owning type::][breadcrumbs.]Property)`

Most property references will just look like this, however:

`@Property`

By default, the property will be searched for in the current type and every child type in the hierarchy. If the property is in another type or a parent type, it should be explicitly specified.

`@SDG.Unturned.ItemBarrelAsset, Assembly-CSharp::Silenced`

If a localization property is referring to a normal property, or some situation like that, a category tag will have to be put on the front.

`@$prop$::TextId` or `@$prop$::SDG.Unturned.ActionBlueprint, Assembly-CSharp::TextId`

Breadcrumbs can be supplied for advanced situations where a property needs to be accessed through an array. Breadcrumbs can access properties or indices. For example:

`@SDG.Unturned.BlueprintSupply, Assembly-CSharp::InputItems[0].ID`

> [!TIP]
> The property being referenced here is `ID`, so the type in front needs to point to the owner of `ID`, not `InputItems` (which would be `Blueprint` in that case).
> 
> This syntax is a little misleading in this case. but we're saying access `BlueprintSupply.ID` by navigating through `InputItems[0]`.

Other breadcrumb examples:

* `"Property[0][1]."` -> `Property[0]/[1]/` - Property is a list of lists, access the first element of property then the first element of that list.
* `"Dictionary.Property."` -> `Dictionary/Property/` - Dictionary is a dictionary/object, access Property in Dictionary
* `"List\[2][3].Property\.Name."` -> `List[2][3]/Property.Name/` - 'List[2]' is the key of a list, access element 3 in 'List[2]', then get the 'Property.Name' property from that dictionary.

| Category     | Description                                                                             |
| ------------ | --------------------------------------------------------------------------------------- |
| -            | Category auto-determined.                                                               |
| `$prop$`     | Normal property, found in the `Properties` list.                                        |
| `$local$`    | Localization property, found in the `Localization` list.                                |
| `$cr.prop$`  | Cross-referenced normal property, found in the `Properties` list of the target.         |
| `$cr.local$` | Cross-referenced localization property, found in the `Localization` list of the target. |
| `$cr$`       | Cross-referenced property, category auto-determined.                                    |
| `$bndl$`     | Unity asset property, found in the `BundleAssets` list.                                 |

### Cross-referenced properties
A cross-referenced property references a property in a different type on a different object. This object is determined by the value of the `FileCrossRef`, which is the name of a property. This property should be an AssetReference, LegacyAssetReference, BackwardsCompatibleAssetReference, or similar type which references another asset file.

For example:
```json
{
    "Key": "BarrelID",
    "Type": "Id",
    "ElementType": "SDG.Unturned.ItemBarrelAsset, Assembly-CSharp"
},
{
    "Key": "Is_Gun_Silenced",
    "Type": "Boolean",
    "FileCrossRef": "BarrelID",
    // DefaultValue = BarrelID != 0 && ((ItemBarrelAsset)Assets.find(EAssetType.ITEM, BarrelID)).isSilenced
    "DefaultValue":
    [
        {
            "And":
            [
                {
                    "Variable": "BarrelID",
                    "Operation": "neq",
                    "Comparand": 0
                }
            ],
            "Value": "@$cr$::SDG.Unturned.ItemBarrelAsset, Assembly-CSharp::Silenced"
        },
        {
            "Value": false
        }
    ]
}
```

There can only be one cross-reference target per property.
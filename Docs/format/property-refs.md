# Property References

Property references are used to reference the value from another property. Property references are prefixed with a `@` unless the place you're entering it defaults to a property reference, like [Condition Variables](/api/DanielWillett.UnturnedDataFileLspServer.Data.Logic.SpecCondition.html#DanielWillett_UnturnedDataFileLspServer_Data_Logic_SpecCondition_Variable).

Property references are represented by the [PropertyRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.PropertyRef.yml) class.

## Format

Fully qualified properties contain their category, type, and name.

`@([$xx$::][owning type::]Property)`

Most property references will just look like this, however:

`@Property`

By default, the property will be searched for in the current type and every child type in the hierarchy. If the property is in another type or a parent type, it should be explicitly specified.

`@SDG.Unturned.ItemBarrelAsset, Assembly-CSharp::Silenced`

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
A cross-referenced property references a property in a different type on a different object. This object is determined by the value of the `FileCrossRef`, which is the name of a property. This property should be a Guid, AssetReference, Id, GuidOrId, or similar type which references another asset file.

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
    // this.DefaultValue = BarrelID != 0 && ((ItemBarrelAsset)Assets.find(EAssetType.ITEM, BarrelID)).isSilenced
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
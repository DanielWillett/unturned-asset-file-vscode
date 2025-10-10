# Data-Refs

'Data-Refs' are a special type of property-ref that specifies a target and a property on that target. They are defined using the `#` character. Data-ref properties all inherit [DataRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.DataRef.yml).

Indexable Data-Ref properties implement [IIndexableDataRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.IIndexableDataRef.yml) and properties with settings implement [IPropertiesDataRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.IPropertiesDataRef.yml).

## Targets

All Data-Ref targets implement [IDataRefTarget](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.IDataRefTarget.yml).

| Target   | Description                                    |
| -------- | ---------------------------------------------- | 
| `This`   | The object containing the current property.    |
| `Self`   | The current property.                          |
| Property | A Property Reference without the `@` symbol.   |

If a property is named 'This' or 'Self', it should be entered as `\This` or `\Self`.

## Properties

All Data-Ref targets can contain the following properties:

| Property | Description | Targets |
| - | - | - |
| Excluded | Is the property excluded from the file? (opposite of Included) | any |
| Included | Is the property included in the file? | any |
| Key | The exact key given for this property. | any |
| Value | The value given for this property. | any |
| AssetName | The internal name of the asset. This is usually the file name. | `This` |
| KeyGroups | Array of key groups used for RegEx keys. | any |
| IsLegacy | Whether or not the currently parsing property is being parsed in the legacy format (ex. with blueprints, spawn tables, etc using the v1 format). | `This`, `@Property` |
| ValueType | Which type of value this property provides: 'Value', 'List', or 'Dictionary' | `This`, `@Property` |

Properties are currently hard-coded and can't be extended.

### Excluded
Returns a boolean indicating whether or not the target is not included in the file.

When used on `This` it targets the current property.

Represented by the class: [ExcludedDataRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.ExcludedDataRef.yml).

*No properties, not indexable*

### Included
Returns a boolean indicating whether or not the target is included in the file.

When used on `This` it targets the current property.

Represented by the class: [IncludedDataRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.IncludedDataRef.yml).

*No properties, not indexable*

### Key
Returns the exact key used to specify this property, including aliases, casing, etc. This doens't include quotes.

When used on `This` it targets the current property.

Represented by the class: [KeyDataRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.KeyDataRef.yml).

*No properties, not indexable*

### Value
Returns the value specified in this property. For flags, the value will be `true` when they are included and `false` when they are excluded.

When used on `This` it targets the current property.

Represented by the class: [ValueDataRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.ValueDataRef.yml).

*No properties, not indexable*

### AssetName
Returns the internal name of the currently opened asset (`Asset.name`), which is usually the file name without it's extension. Returns `null` if the current file isn't an asset file or hasn't been completed enough to be recognizable as one.

Not affected by the target, use `This` for consistancy.

Represented by the class: [AssetNameDataRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.AssetNameDataRef.yml).

*No properties, not indexable*

### Template Groups
Read more about key groups [here](./property-template-groups.md). Returns the value of the given template group for this object. Returns -1 if there are no template groups, otherwise returns which number the property is currently on.

An index can be included to reference a single template group instead of the entire array. Indices must be a non-negative integer. The first '#' is index 0, the second is index 1, etc.

The template group array contains all template group values in the current type hierarchy, from last to first, so if you have two nested keys such as in `Message_1_Page_2`, the array will be `[2, 1]`.

Represented by the class: [TemplateGroupsDataRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.TemplateGroupsDataRef.yml).

Example:
```properties
Calibers 3
Caliber_0 4
Caliber_1 8
Caliber_2 18346
# this isn't a real property
Caliber_2_Name Custom Bow
```
```json
{
    "Key": "Caliber_*",
    "Template": true,
    "Required": true,
    "TemplateGroups": [ "calibers" ]
},
{
    "Key": "Caliber_*_Name",
    "Required": true,
    "TemplateGroups": [ "calibers" ],
    // defaults to the ID of the caliber, would complete to @Caliber_2
    "DefaultValue": "@Caliber_#(Self.KeyGroups[0])"
}
```

Index can be used to reference a specific key group. Indexing is zero-based so 1 must be subtracted from the RegEx group number.

### IsLegacy
Equal to `true` if the target is in a type such as the `LegacyCompatibleList` and the legacy (v1) format is being used.

This property can not target cross-referenced properties or `#Self`.

Example:
```properties
GUID e2691008f3d746c9bde7bd28258f28d9
Type Spawn
ID 330

Tables 3
# IsLegacy = true for this property
Table_0_Asset_ID 1
Table_0_Weight 10
Table_1_Asset_ID 4
Table_1_Weight 10
Table_2_Asset_ID 6
Table_2_Weight 10

Tables
[
    {
        # IsLegacy = false for this property
        LegacyAssetId 1
        Weight 10
    }
    {
        LegacyAssetId 4
        Weight 10
    }
    {
        LegacyAssetId 6
        Weight 10
    }
]
```

### ValueType
Returns a string value indicating which type of value the property provides, which is one of the following: 'Value', 'List', or 'Dictionary'.

This property can not target cross-referenced properties or `#Self`.

By default 'Value' will be returned if the property isn't present or doesn't have any kind of value.

#### Settings
##### PreventSelfReference `bool`
When used with the `ValueRegexGroupReference` property, makes auto-complete not include `#Self.KeyGroups[0]` in the results for the target's key groups. `INPCCondition.UI_Requirements` is a good example of this.

```json
{
    "Key": "UI_Requirements",
    // 'This' is the current condition in this case
    "ValueRegexGroupReference": "#This.KeyGroups[0]{PreventSelfReference=true}",
}
```


## Format

`#Target.Property[index]{Setting=value}`

Do not include extra white space.

Indices and settings are optional and their brackets should be excluded if they're not being used. Indicies must always come before settings if both are included.

The index can be any integer, including negative numbers if supported by the property. 

The target can either be `This`, `Self`, or a property name. If a property name is 'This' or 'Self', the name can be escaped using a backslash.

Valid properties are hard-coded and listed above. More may be added in the future as needed.
# Bang-Refs

'Bang-Refs' are a special type of property-ref that specifies a target and a property on that target. They are defined using the `#` character. Bang-ref properties all inherit [BangRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.BangRef.yml).

Indexable Bang-Ref properties implement [IIndexableBangRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.IIndexableBangRef.yml) and properties with settings implement [IPropertiesBangRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.IPropertiesBangRef.yml).

## Targets

All Bang-Ref targets implement [IBangRefTarget](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.IBangRefTarget.yml).

| Target   | Description                                    |
| -------- | ---------------------------------------------- | 
| `This`   | The object containing the current property.    |
| `Self`   | The current property.                          |
| Property | A Property Reference without the `@` symbol.   |

If a property is named 'This' or 'Self', it should be entered as `\This` or `\Self`.

## Properties

All Bang-Ref targets can contain the following properties:

| Property | Description | Targets |
| - | - | - |
| Excluded | Is the property excluded from the file? (opposite of Included) | any |
| Included | Is the property included in the file? | any |
| Key | The exact key given for this property. | any |
| Value | The value given for this property. | any |
| AssetName | The internal name of the asset. This is usually the file name. | `This` |
| KeyGroups | Array of key groups used for RegEx keys. | any |

Properties are currently hard-coded and can't be extended.

### Excluded
Returns a boolean indicating whether or not the target is not included in the file.

When used on `This` it targets the current property.

Represented by the class: [ExcludedBangRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.ExcludedBangRef.yml).

*No properties, not indexable*

### Included
Returns a boolean indicating whether or not the target is included in the file.

When used on `This` it targets the current property.

Represented by the class: [IncludedBangRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.IncludedBangRef.yml).

*No properties, not indexable*

### Key
Returns the exact key used to specify this property, including aliases, casing, etc. This doens't include quotes.

When used on `This` it targets the current property.

Represented by the class: [KeyBangRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.KeyBangRef.yml).

*No properties, not indexable*

### Value
Returns the value specified in this property. For flags, the value will be `true` when they are included and `false` when they are excluded.

When used on `This` it targets the current property.

Represented by the class: [ValueBangRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.ValueBangRef.yml).

*No properties, not indexable*

### AssetName
Returns the internal name of the currently opened asset (`Asset.name`), which is usually the file name without it's extension. Returns `null` if the current file isn't an asset file or hasn't been completed enough to be recognizable as one.

Not affected by the target, use `This` for consistancy.

Represented by the class: [AssetNameBangRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.AssetNameBangRef.yml).

*No properties, not indexable*

### Key Groups
Read more about key groups [here](./property-key-groups.md). Returns the value of the given key group for this object. Returns -1 if there are no key groups, otherwise returns which number the property is currently on.

An index can be included to reference a single key-group instead of the entire array. Indices must be a non-negative integer. Indices are zero-based, where RegEx groups are one-based, so you need to subtract 1 from the RegEx group number.

The key group array contains all KeyGroup values in the current type hierarchy, from last to first, so if you have two nested keys such as in `Message_1_Page_2`, the array will be `[2, 1]`.

Represented by the class: [KeyGroupsBangRef](/api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.KeyGroupsBangRef.yml).

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
    "Key": "Caliber_(\\d+)",
    "KeyIsRegex": true,
    "Required": true,
    "KeyGroups":
    [
        {
            "Name": "calibers",
            "RegexGroup": 1
        }
    ]
},
{
    "Key": "Caliber_(\\d+)_Name",
    "KeyIsRegex": true,
    "KeyGroups":
    [
        {
            "Name": "calibers",
            "RegexGroup": 1
        }
    ],
    // defaults to the ID of the caliber, would complete to @Caliber_2
    "DefaultValue": "@Caliber_#(Self.KeyGroups[0])"
}
```

Index can be used to reference a specific key group. Indexing is zero-based so 1 must be subtracted from the RegEx group number.

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
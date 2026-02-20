# Data-Refs

'Data-Refs' are a special type of property-ref used to reference metadata about a target. They are defined using the `#` character.

Data-ref properties all implement [IDataRefProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.IDataRefProperty.yml). Note that data-ref targets are not the same thing as data-ref properties. Indexable Data-Ref properties implement [IIndexableDataRefProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.IIndexableDataRefProperty.yml) and properties with settings implement [IConfigurableDataRefProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.IConfigurableDataRefProperty.yml).

## Targets

All Data-Ref targets implement [IDataRefTarget](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.IDataRefTarget.yml).

| Target     | Description                                                |
| ---------- | ---------------------------------------------------------- | 
| `This`     | The object containing the current property.                |
| `Self`     | The current property.                                      |
| `Index`    | The current index when in `List` default value.            |
| `Key`      | The current key when in `Dictionary` default value.        |
| `Value`    | The string value when in a `StringDefaultValue` property.  |
| Property   | A Property Reference without the `@` symbol.               |

If a property is one of the reserved keywords, it should be escaped, such as `#\Value`.

## Properties

Data-Ref targets can use the following properties:

| Property | Description | Targets |
| - | - | - |
| Excluded | Is the property excluded from the file? (opposite of Included) | any |
| Included | Is the property included in the file? | any |
| Key | The exact key given for this property. | any |
| AssetName | The internal name of the asset. This is usually the file name. | `This` |
| Difficulty | The contextual difficulty of the current file. | `This` |
| Indices | Array of indices used to get the target's index within a list or it's key within a dictionary. | any |
| IsLegacy | Whether or not the currently parsing property is being parsed in the legacy format (ex. with blueprints, spawn tables, etc using the v1 format). | `Self`, `@Property` |
| ValueType | Which type of value this property provides: 'Value', 'List', or 'Dictionary' | `Self`, `@Property` |

Properties are currently hard-coded and can't be extended.

### Excluded
Returns a boolean indicating whether or not the target is not included in the file.

When used on `This` it targets the current property.

Represented by the class: [ExcludedProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.ExcludedProperty.yml).

*No properties, not indexable*

### Included
Returns a boolean indicating whether or not the target is included in the file.

When used on `This` it targets the current property.

Represented by the class: [IncludedProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.IncludedProperty.yml).

*No properties, not indexable*

### Key
Returns the exact key used to specify this property, including aliases, casing, etc. This doens't include quotes.

When used with `This` as a target, refers to the current object's key.

`#This.Key` can be used as a key to specify an empty key name, which is used for properties in legacy objects which are the same as the base property. For example, the localization property for dialogue responses use this because the key is just `Response_#`, as opposed to a property that may be `Response_#_Dialogue`.

Represented by the class: [KeyProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.KeyProperty.yml).

*No properties, not indexable*

### AssetName
Returns the internal name of the currently opened asset (`Asset.name`), which is usually the file name without it's extension. Returns `null` if the current file isn't an asset file or hasn't been completed enough to be recognizable as one.

Only valid with `This` as a target, which targets the current file.

Represented by the class: [AssetNameProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.AssetNameProperty.yml).

*No properties, not indexable*

### Difficulty
Returns the contextual difficulty of the file being edited. This is based on the `Mode Xxxx` command in the `Commands.dat` file relative to the active file and is primarily used by the server `ConfigData` file. If the file has a specific difficulty like `Config_EasyDifficulty.txt`, this will return `EASY` instead of whatever's specified in the `Commands.dat` file.

If the `difficulty` additional file property is provided, that value will be used instead.
```cs
// udat-difficulty: Easy

// > Unturned Server Configuration File
// > 
// ...
```

Note that the difficulty is usually cached and may not auto-update in some cases. Also note that if the server uses a custom localization mod which changes the display name of the gamemodes, the LSP will not be able to parse the mode correctly.

#### Possible return values
| Value    | Description            |
| -------- | ---------------------- |
| `EASY`   | The 'Easy' gamemode.   |
| `NORMAL` | The 'Normal' gamemode. |
| `HARD`   | The 'Hard' gamemode.   |

Not affected by the target, use `This` for consistancy.

Represented by the class: [DifficultyProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.DifficultyProperty.yml).

*No properties, not indexable*

### Indices
Returns the index of this object within it's parent lists and dictionaries.

An index can be included to reference a single index instead of the entire array. Indices must be a non-negative integer. The most-recent list is the first element (index 0), the second-most-recent is the second element, and so on. If the current object has no lists or dictionaries in it's hierarchy, or the index is out of range, an index access will return a null value.

Example:
```properties
# List.Type = List<Objects>
List
[
  {

  }
  {
    # Dictionary.Type = Dictionary<string, List<Object>>
    Dictionary
    {
      Key
      [
        {
          # Data-ref '#Property.Indices' returns '[ 0, "Key", 1 ]'
          # Data-ref '#Property.Indices[0]' returns '0'
          # Data-ref '#Property.Indices[1]' returns "Key"
          # Data-ref '#Property.Indices[2]' returns '1'
          Property 1
        }
      ]
    }
  }
]
```

Represented by the class: [IndicesProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.IndicesProperty.yml).

*No properties, indexable with one argument*

### IsLegacy
Equal to `true` if the target is in a type such as the `LegacyCompatibleList` and the legacy (v1) format is being used.

This property can not target cross-referenced properties or `#This`.

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

Represented by the class: [IsLegacyProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.IsLegacyProperty.yml).

*No properties, not indexable*

### ValueType
Returns a string value indicating which type of value the property provides, which is one of the following: 'Value', 'List', or 'Dictionary'.

This property can not target cross-referenced properties or `#This`.

By default 'Value' will be returned if the property isn't present or doesn't have any kind of value.

Represented by the class: [ValueTypeProperty](/api/DanielWillett.UnturnedDataFileLspServer.Data.Values.ValueTypeProperty.yml).

*No properties, not indexable*


## Format

`#Target.Property[index]{\"Setting\"=value}`

Do not include extra white space.

Indices and settings are optional and their brackets should be excluded if they're not being used. Indicies must always come before settings if both are included.

Data in the '{}' brackets is parsed as a JSON object.

The index can be any integer, including negative numbers if supported by the property. 

The target can either be `This`, `Self`, or a property name. If a property name is 'This' or 'Self', the name can be escaped using a backslash. In some contexts, `#Index`, `#Key`, and `#Value` can be used as roots.

Valid properties are hard-coded and listed above. More may be added in the future as needed.
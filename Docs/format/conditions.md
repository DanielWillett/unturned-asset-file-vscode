# Conditions
Conditions are used to toggle functionality based on a certain operation.

Conditions are represented by the [SpecCondition](../api/DanielWillett.UnturnedDataFileLspServer.Data.Logic.SpecCondition.yml) struct.

```json
{
    // 'Variable' is a Dynamic Value* which defaults to a Property Reference (@).
    "Variable": "Property",

    // The operation to check, see below for a list of allowed values.
    "Operation": "eq",
    
    // A value to compare 'Variable' to. Can be string, number, boolean, null, etc.
    // These are NOT dynamic values and can only be basic concrete values.
    // The comparand can also be an array of basic concrete values.
    // If you need to do a comparison between two dynamic values, use the '=CMP' or '=CMP_IC' expressions as the Variable.
    "Comparand": "value"
}
```
\* *See [Dynamic Values](./dynamic-values.html) for more information.*

They can be compared to the condition section of an if statement
```cs
if (this.Property == "value")
```

### Operations
| Value             | Operation                             | Comparand Type           |
| ----------------- | ------------------------------------- | ------------------------ |
| `lt`              | Less Than                             | Number                   |
| `gt`              | Greater Than                          | Number                   |
| `lte`             | Less Than or Equal To                 | Number                   |
| `gte`             | Greater Than or Equal To              | Number                   |
| `eq`              | Equal To                              | Any                      |
| `neq`             | Not Equal To                          | Any                      |
| `contains`        | String Contains                       | String                   |
| `starts-with`     | String Starts With                    | String                   |
| `ends-with`       | String Ends With                      | String                   |
| `matches`         | String Matches Regex                  | RegEx string             |
| `contains-i`      | String Contains (case-insensitive)    | String                   |
| `eq-i`            | Equals String (case-insensitive)      | String                   |
| `neq-i`           | Not Equals String (case-insensitive)  | String                   |
| `starts-with-i`   | String Starts With (case-insensitive) | String                   |
| `ends-with-i`     | String Ends With (case-insensitive)   | String                   |
| `assignable-to`   | Type Assignable To                    | Fully Qualified CLR Type |
| `assignable-from` | Type Assignable From                  | Fully Qualified CLR Type |
| `is-type`*        | Value is of Type                      | Fully Qualified CLR Type |

> [!TIP]
> <b>Fully Qualified CLR Type</b> refers to a type in the following format:
> `SDG.Unturned.ItemAsset, Assembly-CSharp`.

\*`is-type` is intended to be used on properties that reference another asset, such as GUID and ID properties. Example:
```json
// if 'ID' refers to a Gun item.
"Condition":
{
    "Variable": "ID",
    "Operation": "is-type",
    "Comparand": "SDG.Unturned.ItemGunAsset, Assembly-CSharp"
}
```

### JSON
Certain types of conditions can be encoded with a shorthand:

```json
"Condition":
{
    "Variable": "%true",
    "Operation": "eq",
    "Comparand": true
},

// can be written as
"Condition": true
```

```json
"Condition":
{
    "Variable": "%false",
    "Operation": "eq",
    "Comparand": true
},

// can be written as
"Condition": false
```

```json
"Condition":
{
    "Variable": "Property",
    "Operation": "eq",
    "Comparand": true
},

// can be written as
"Condition": "Property"
```
# Switch Statements
Switch Statements are used to choose a value based on [Conditions](./conditions.md).
They are similar to the switch expressions in C#.

[Examples](./switch-statements.md#examples) are below.

Switch Statements are represented by the [SpecDynamicSwitchValue](../api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.SpecDynamicSwitchValue.yml) dynamic value class.

## Format
Switches are formatted as an array of [Switch Cases](./switch-statements.md#case-format).

The are evaluated in order, and once a case is met no others will be evaluated.

The last case is the default case and shouldn't have conditions, just a value.
```json
"Required":
[
    {
        // view the full case format below
        "Or":
        [
            {
                "Variable": "Type",
                "Operation": "eq",
                "Comparand": "Shirt"
            },
            {
                "Variable": "Type",
                "Operation": "eq",
                "Comparand": "Pants"
            }
        ],
        "Value": false
    },
    {
        "Value": true
    }
]
```
This switch statement is the same as the following C# code:
```cs
switch (this.Type)
{
    case EAssetType.SHIRT:
    case EAssetType.PANTS:
        this.Required = false;
        break;

    default:
        this.Required = true;
        break;
}
```

# Case Format

Switch Cases are represented by the [SpecDynamicSwitchCaseValue](../api/DanielWillett.UnturnedDataFileLspServer.Data.Properties.SpecDynamicSwitchCaseValue.yml) class.

There are three valid formats for a switch case:

### Condition Case

A <b>Condition Case</b> takes an array of [Conditions](./conditions.md) or other [Switch Cases](./switch-statements.md#case-format) in either an `And` list or `Or` list. There can not be both an `And` and `Or` list.

Note that even with one condition it must be in a list.
```json
{
    // If all are true, the condition passes.
    "And":
    [
        // Condition or Case,
        // ...
    ],

    // xor

    // If at least one is true, the condition passes.
    "Or":
    [
        // Condition or Case,
        // ...
    ],


    // condition cases must also include a value, which can be any type
    "Value": "??"
}
```

### When Case

A <b>When Case</b> is used to add an extra <b>and</b> check to all child cases, which is a common pattern. <b>When Cases</b> do not need to specify a value as they themselves become a switch statement and their value will just be whatever `Cases` evaluates to.

If `Case` is used, it should be a <b>Default Case</b>.

```json
{
    // Initial condition that, if false, will instantly move to the next case.
    "When":
    {
        // Condition
    },



    // Cases to check if 'When' evaluates to true
    "Cases":
    [
        // Switch Statement

        // Case1,
        // Case2,
        // ...
    ],

    // or

    // Case for if 'When' evaluates to true
    "Case":
    {
        "Value": "whatever"
    }
}
```

### Default Case
A default case is always at the end of the list and should only specify a `Value`. This is the fallback value if no other case was met. 
```json
{
    "Value": "fallback value"
}
```

# Examples

The format for a switch statement can follow two templates.

### Condition Cases:
For most cases a <b>Condition</b> switch will be sufficient.

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
> [!NOTE]
> See [Equations](./equations.md) for more information on equations.

This translates to the following C# code:
```cs
float defaultValue;
if (Some_Property != 0)
{
    defaultValue = 1f / Some_Property;
}
else
{
    defaultValue = 20;
}
```

### With Cases:
In certain situations <b>With</b> switches end up being simpler.
```json
"DefaultValue":
[
    {
        "With":
        {
            "Variable": "Some_Property",
            "Operation": "lt",
            "Comparand": 8
        },
        "Cases":
        [
            {
                "And":
                [
                    {
                        "Variable": "Some_Property",
                        "Operation": "gt",
                        "Comparand": 4
                    }
                ],
                "Value": 15
            },
            {
                "Value": 10
            }
        ]
    },
    {
        "Value": 20
    }
]
```
This translates to the following C# code:
```cs
int defaultValue;
if (Some_Property < 8)
{
    switch (Some_Property)
    {
        case > 4:
            defaultValue = 15;
            break;

        default: // <= 4
            defaultValue = 10;
            break;
    }
}
else // >= 8
{
    defaultValue = 20;
}
```
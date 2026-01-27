# Unturned Data File LSP + VSCode Extension

This repository contains a work-in-progress Language Server for [Unturned Data Files](https://docs.smartlydressedgames.com/en/stable/assets/data-file-format.html).

When finished, it will support syntax highlighting, auto-complete, diagnostics, code actions, and more.

In the mean-time, a 'lite' version of the extension is available for download which only contains syntax highlighting from the [VSCode Marketplace](https://marketplace.visualstudio.com/items?itemName=DanielWillett.unturned-data-file-lite).
![Syntax Highlighting](https://raw.githubusercontent.com/DanielWillett/unturned-asset-file-vscode/refs/heads/master/Assets/Syntax%20Highlighting.png)

## Technology
* C#/.NET
* [OmniSharp Language Server](https://github.com/OmniSharp/csharp-language-server-protocol)
* TypeScript (vscode)
* [TextMate Grammars](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/unturned-dat/syntaxes/unturned-dat.tmLanguage.json)
* Complex type and value system using generics for performance.
* File parse trees.
* High-performance parallel file scanning.
* Dynamically resolved values
  * [Concrete Values](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Values/Concrete)
  * [Expressions](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Values/Expressions)
  * [Property References](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Values/PropertyRefs)
  * [Metadata References](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Values/DataRefs)
* The visitor pattern... a lot.

## Browse the Code

### VS Code Extensions
* [Full Version](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/unturned-dat)
* [Lite Version](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/unturned-dat-lite)

### [Asset Spec](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/Asset%20Spec)
Contains JSON files that are kept up to date so the extension doesn't have to be updated when new properties are added.

See [the docs](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/Docs/format) for how these files are formatted if you'd like to contribute (if I miss an update).

`asset-spec.g.bin` contains all the files combined together so only one web request has to be made to download them all.
See [GitHubSpecificationFileProvider](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Spec/ISpecificationFileProvider.cs#L141) for how to split the file up.

Basic format for a property includes a key, data type, default value, basic metadata, and information about it's relationship to other properties. To see a full list of properties use an IDE that supports JSON schemas (VS Code does) and read through the docs.
```json
{
    "Key": "Interactability_Drops",
    "Type":
    {
        "Type": "List",
        "CountType": "UInt8",
        "ElementType":
        {
            "Type": "LegacyAssetReference",
            "AssetType": "SDG.Unturned.ItemAsset, Assembly-CSharp"
        },
        "Mode": "LegacyList",
        "LegacyDefaultElementTypeValue": 0
    },
    "DefaultValue": [ ],
    "Description": "Options for item drops from this dropper object.",
    "Docs": "https://docs.smartlydressedgames.com/en/stable/assets/object-asset.html#interactables",
    "Variable": "interactabilityDrops",
    "InclusiveWith":
    [
        {
            "Key": "Interactability",
            "Value": "DROPPER"
        }
    ],
    "CountForTemplateGroup": "object-interactability-drops",
    "ExclusiveWith":
    [
        {
            "Key": "Type",
            "Value": "DECAL"
        },
        {
            "Key": "Type",
            "Value": "NPC"
        },
        {
            "Key": "Interactability_Reward_ID",
            "Condition":
            {
                "Variable": "Interactability",
                "Operation": "eq",
                "Comparand": "DROPPER"
            }
        }
    ]
}
```

Default value is usually just a value but can also be a dynamic value like an expression.
This example shows a default value of `Math.Ceiling((double)Splatters / Splatter) * Preload`.
```json
{
    "Key": "Splatter_Preload",
    "Type": "UInt8",
    "DefaultValue": "=MUL(=CEIL(=DIV(@Splatters @Splatter)) @Preload)",
    "IncludedDefaultValue": 0,
    "Description": "Number of splatter effects to cache and pool on startup. Note that @Preload has to be > 0 to preload splatters.",
    "Docs": "https://docs.smartlydressedgames.com/en/stable/assets/effect-asset.html#splatters",
    "Variable": "splatterPreload",
    "InclusiveWith":
    [
        {
            "Key": "Preload",
            "Condition":
            {
                "Variable": "Preload",
                "Operation": "neq",
                "Comparand": 0
            }
        }
    ]
}
```

### [MSBuild Tasks](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpecMSBuildTasks)
Automatically creates the `asset-spec.g.bin` file on build. It also compresses the files by removing indentation and white space. In the future a 'zip' compression method could be used but may slow down the build.

### [Types](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Types)
Contains implementations of [`IType<TValue>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Types/IType.cs) for every property type in the game.

Almost all types derive from [`BaseType<TValue>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Types/BaseType.cs) or [`PrimitiveType<TValue>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Types/Primitives/PrimitiveType.cs).

#### Type Factories
`ITypeFactory` is used to read types from their JSON object (using the `JsonDocument` STJ API). Some types are their own factory like [`AssetReferenceType`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Types/Objects/AssetReferenceType.cs), and some define separate factories like [`ListType<TCountType, TElementType>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Types/Containers/ListType.cs). Type factories are currently registered in [`CommonTypes`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Types/CommonTypes.cs).

#### [Type Converters](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Parsing/TypeConverters)
Type converters are responible for parsing values from strings or JSON values, and converting values to strings. They can optionally emit diagnostics but usually that's left up to the parser. (i.e. the boolean converter emits diagnositcs if a captial 'Y' is used, where Unturned only parses a lowercase 'y')

#### [Type Parsers](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Parsing/TypeParsers)
Type parsers are responible for parsing values from data files and emitting diagnostics. Most primitive types use the [`TypeConverterParser<T>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Parsing/TypeParsers/TypeConverterParser.cs) type, which parses only string values using a `ITypeConverter<TValue>`.

`IType<TValeus>` defines `ITypeParser<TValue> Parser { get; }` to allow types to share parser implementations, but a lot of complex types use `this` as a parser, such as [`AssetReferenceType`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Types/Objects/AssetReferenceType.cs).

#### Specially-defined types
* [`AssetCategory`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/AssetCategory.cs) - Defined in code as it's used a lot in various areas of the code-base.
* [`SpecificationTypeAttribute`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Types/SpecificationTypeAttribute.cs) - Marks a type as able to be instantiated from JSON given it's assembly-qualified name.
  * Allows special-handling of some values, especially used with the `StringParseableType` property (See [Built-in Types](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Types/BuiltinTypes)).

### [Values](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Types)
Contains implementations of [`IValue<TValue>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Values/IValue.cs) for different kinds of statically or dynamically resolved values.

This includes concrete values, expressions, property-refs, and data-refs (metadata references).

#### [Vector Providers](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Types/VectorProviders)
Some values are considered vectors, i.e. values with multiple numeric components. This includes `Color[32]` and the `Vector{1,2,3,4}` types.

`IVectorProvider`s allow for conversion to and from vectors and math operations between them.

### [MathMatrix](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/DanielWillett.MathMatrix)
This is definately one of the more rediculous parts of the codebase but it contains implementations for every combination of types for each math operation, accounting for overflows. It takes advantage of JIT branch cancelling to map generic methods to non-generic ones quickly.

The reasoning behind this is it would be very difficult to support different integer types without just doing all math with decimal values, which gets a bit difficult to keep the type correct, and has issues with division.

It has a circular dependency with `UnturnedAssetSpec` so it has to be built twice to get all the features.

### [Code Fixes](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/CodeFixes)
Code fixes and quick actions are defined in this folder. [`IPerPropertyCodeFix`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/CodeFixes/System/PerPropertyCodeFix.cs) is a kind of code fix that operates on each property, which provides an optimization where we only have to enumerate all properties once instead of for each code fix that needs it.

### [Diagnositcs](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Diagnostics/DatDiagnostics.cs)
Contains a definition for all diagnostics.

Localized resources for the diagnostics are defined in a [resx file](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Diagnostics/DiagnosticResources.resx).

To emit diagnostics, extension methods are usually created in [DiagnosticSinkExtensions](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Diagnostics/DiagnosticSinkExtensions.cs) to format the message consistantly and remove clutter from type parsers. The methods should stay in order by their number.

### Utilities
* [`OneOrMore<T>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Utility/OneOrMore.cs) - An effecient array implementation that doesn't create an array for just one element. Despite being named one-or-more it can contain zero elements.
  * It's important to remember that the `default` value of this struct is not an empty array, use `OneOrMore<T>.None` instead. A default value is an array with length 1 where the first element is the `default` value of that type.
* [`EquatableArray<T>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Utility/EquatableArray.cs) - An array wrapper implementing `IEquatable<T>`.
* [`InstallDirUtility`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Utility/InstallDirUtility.cs) - Used to locate the current user's Unturned installation path by looking in common areas, then falling back on a full search. On Windows, the installation directory can be found by looking at the Registry. 
* [`JsonHelper.TryGetGuid`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Utility/JsonHelper.cs#L363) - System.Text.Json will not parse GUIDs unless they use dashes, so this function has to be used to parse GUIDs from JSON.
* [`KnownTypeValueHelper`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Utility/KnownTypeValueHelper.cs) - Contains a bunch of parse methods that are semantically the exact same as how Unturned parses values.
* Visitor Utilities
  * [`ComparerVisitor<TValue>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Utility/ComparerVisitor.cs) - Compares the relationship between a value to some other generic value. Returns an integer determining whether a value is greather than, less than, or equal to another generic value.
  * [`ConvertVisitor<TResult>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Utility/ConvertVisitor.cs) - Converts a value to the given type.
  * [`EqualityVisitor<TValue>`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Utility/EqualityVisitor.cs) - Checks the equality of value to some other generic value.

### File parsing and in-memory representations
* [`DatTokenizer`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Files/DatTokenizer.cs) - Tokenizes a data file without creating nodes. Useful for low-allocation reading of a file to quickly read one property, ex. reading the GUID and ID from a file.
* [`SourceNodeTokenizer`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Files/Source/Parsing/SourceNodeTokenizer.cs) - Tokenizes a data file into a [`ISourceFile`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Files/Source/ISourceFile.cs). Can optionally read property values lazily and read metadata like comments and whitespace.
* [Source Nodes](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Files/Source/Nodes) - Used to construct a parse tree from a data file.
* [Node Visitors](https://github.com/DanielWillett/unturned-asset-file-vscode/tree/master/UnturnedAssetSpec/Files/Source/Visitors) - Various visitors used to enumerate all nodes in a file.
  * [`OrderedNodeVisitor`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Files/Source/Visitors/OrderedNodeVisitor.cs) can be extended to enumerate nodes in 'vertical' order.
  * [`ResolvedPropertyNodeVisitor`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Files/Source/Visitors/ResolvedPropertyNodeVisitor.cs) can be used to enumerate properties with their `DatProperty` information already fetched.
* [`OpenedFile`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/LspServer/Files/OpenedFile.cs) - Part of the Language Server, allows for incremental file changes to be tracked correctly. Implementation of `IWorkspaceFile` that represents an open file in the IDE.
* [`MutableVirtualFile`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Files/MutableVirtualFile.cs) - Tracks changes made via the `IMutableWorkspaceFile` interface. Used for diagnostics to display pending changes in the IDE.
* [`StaticSourceFile`](https://github.com/DanielWillett/unturned-asset-file-vscode/blob/master/UnturnedAssetSpec/Files/StaticSourceFile.cs) - Implementation of the `IWorkspaceFile` interface that reads a file from the disk.

## Contributing
### Asset Spec

If you'd like to contribute to the asset specification JSON data, your pull request should contain only the changes to the JSON files, do not bundle the changes, as I'll handle doing that after review.

Please read the docs before contributing as the schema is quite complex. It's important that every single property value is deterministic unless it's literally impossible (in which case the description should indicate this).

#### Formatting
* Opening brackets should be placed on new-lines when formatting your JSON data.
* Files should use spaces (4 per indention), not tabs.
* GUIDs should be in lowercase and use dashes, as it's more effecient when reading using `System.Text.Json`.
* Enum values have to match in case exactly.
* When adding new asset files, remove any fragment data from the link (ex. `.../data-file-format.html#doc-data-file-format` -> `.../data-file-format.html`).

### Code

If you'd like to contribute to the codebase, please read through the entire **Browse the Code** section.

#### Formatting and Style
Code style should follow the existing style in the repo.
* _camelCase for private fields (except `static readonly`, which are ProperCase)
* ProperCase for all classes, namespaces, constants, other members, enum fields, and nested functions.
* camelCase for local variables and `const` local variables.
* Prefix interfaces with 'I'.
* Don't prefix enums with 'E'.
* Brackets on new line.
* No implicit typing (var, new(), etc).
* Prefer collection initialization [ ], include spacing between brackets and content (`[ 1, 2, 3 ]`).
* Don't overcomment, write self-documenting code.
* All new public members should have XML documenation in `UnturnedAssetSpec`. Use `<see langword="xyz"/>` for keywords.
* Use `OneOrMore` or `System.Collections.Immutable` for public collections.
* Use visitors over reflection for invoking generic members from non-generic context. `IType`, `IValue` both have an Accept function that takes a visitor.
* Use value types for visitors.
* Keep performance in mind. The language server is scanning over thousands of files and needs to run fast and keep a low memory footprint.
* Include `unchecked ( <math> )` when math relies on overflow (such as a hash code calculation).
* Use `System.HashCode` over math.
* Singleton types/type converters/values should use a random int32 value as the hash code. I like to use the Visaul Studio's interactive C# window to get this (`Random.Shared.Next()` with no semicolon).
* Use spaces (4 per indentation), not tabs.

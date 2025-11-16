using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;

namespace UnturnedAssetSpecTests.Nodes;

[TestFixture]
public class PropertyBreadcrumbsTests
{
    private ILoggerFactory _loggerFactory;
    private IAssetSpecDatabase _database;

    [SetUp]
    public async Task SetUp()
    {
        _loggerFactory = LoggerFactory.Create(l => l.AddSimpleConsole());
        _database = AssetSpecDatabase.FromOffline();
        await _database.InitializeAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _loggerFactory.Dispose();
        if (_database is IDisposable d)
            d.Dispose();
    }

    [Test]
    public void RootNode([Values(true, false)] bool withPath, [Values(true, false)] bool withType)
    {
        const string fileContents = """
                                    GUID 7ae3737efae940999dc51919ce558ec4
                                    ID 2001
                                    Type Supply
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.Root;


        SpecProperty? testProperty = _database.FindPropertyInfo("ID", AssetFileType.AssetBaseType(_database));
        Assert.That(testProperty, Is.Not.Null);

        List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
        Assert.That(breadcrumbs.TryGetProperty(sourceFile, testProperty, out IPropertySourceNode? sourceNode, path));

        Assert.That(sourceNode, Is.Not.Null);
        Assert.That(sourceNode.Key, Is.EqualTo("ID"));
        Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("2001"));

        if (withType)
        {
            AssetFileType ft = AssetFileType.FromFile(sourceFile, _database);
            Assert.That(ft.Information, Is.Not.Null);

            path?.Clear();
            Assert.That(breadcrumbs.TryGetDictionaryAndType(sourceFile, in ft, _database, out _, out ISpecType? type, path));
            Assert.That(type, Is.SameAs(ft.Information));
        }

        if (withPath)
        {
            Assert.That(path, Is.Empty);
        }
    }

    [Test]
    public void RootNodeInAsset([Values(true, false)] bool withPath, [Values(true, false)] bool withType)
    {
        const string fileContents = """
                                    Metadata
                                    {
                                        GUID 7ae3737efae940999dc51919ce558ec4
                                        Type SDG.Unturned.ItemSupplyAsset, Assembly-CSharp
                                    }
                                    Asset
                                    {
                                        ID 2001
                                    }
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.Root;


        SpecProperty? testProperty = _database.FindPropertyInfo("ID", AssetFileType.AssetBaseType(_database));
        Assert.That(testProperty, Is.Not.Null);

        List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
        Assert.That(breadcrumbs.TryGetProperty(sourceFile, testProperty, out IPropertySourceNode? sourceNode, path));

        Assert.That(sourceNode, Is.Not.Null);
        Assert.That(sourceNode.Key, Is.EqualTo("ID"));
        Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("2001"));

        if (withType)
        {
            AssetFileType ft = AssetFileType.FromFile(sourceFile, _database);
            Assert.That(ft.Information, Is.Not.Null);

            path?.Clear();
            Assert.That(breadcrumbs.TryGetDictionaryAndType(sourceFile, in ft, _database, out _, out ISpecType? type, path));
            Assert.That(type, Is.SameAs(ft.Information));
        }

        if (withPath)
        {
            Assert.That(path, Is.Empty);
        }
    }

    [Test]
    public void RootNodeInMetadata([Values(true, false)] bool withPath, [Values(true, false)] bool withType)
    {
        const string fileContents = """
                                    Metadata
                                    {
                                        GUID 7ae3737efae940999dc51919ce558ec4
                                        Type SDG.Unturned.ItemSupplyAsset, Assembly-CSharp
                                    }
                                    Asset
                                    {
                                        ID 2001
                                    }
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.Root;


        SpecProperty? testProperty = _database.FindPropertyInfo("GUID", AssetFileType.AssetBaseType(_database));
        Assert.That(testProperty, Is.Not.Null);

        List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
        Assert.That(breadcrumbs.TryGetProperty(sourceFile, testProperty, out IPropertySourceNode? sourceNode, path));

        Assert.That(sourceNode, Is.Not.Null);
        Assert.That(sourceNode.Key, Is.EqualTo("GUID"));
        Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("7ae3737efae940999dc51919ce558ec4"));

        if (withType)
        {
            AssetFileType ft = AssetFileType.FromFile(sourceFile, _database);
            Assert.That(ft.Information, Is.Not.Null);

            path?.Clear();
            Assert.That(breadcrumbs.TryGetDictionaryAndType(sourceFile, in ft, _database, out _, out ISpecType? type, path));
            Assert.That(type, Is.SameAs(ft.Information));
        }

        if (withPath)
        {
            Assert.That(path, Is.Empty);
        }
    }

    private static CustomSpecType CreateCustomTestType(AssetSpecType assetType, SpecProperty nestedProperty)
    {
        CustomSpecType ct = new CustomSpecType
        {
            Parent = QualifiedType.None,
            Owner = assetType,
            Properties =
            [
                nestedProperty
            ],
            DisplayName = "Test Config Type",
            Type = assetType.Type.GetFullTypeName() + "+TestConfigType, Assembly-CSharp",
            Docs = null,
            AdditionalProperties = OneOrMore<KeyValuePair<string, object?>>.Null,
            LocalizationProperties = Array.Empty<SpecProperty>()
        };
        nestedProperty.Owner = ct;
        return ct;
    }

    [Test]
    public void SinglePropertyNode([Values(true, false)] bool withPath, [Values(true, false)] bool withType)
    {
        const string fileContents = """
                                    GUID 7ae3737efae940999dc51919ce558ec4
                                    ID 2001
                                    Type Supply
                                    
                                    Config
                                    {
                                        A 1
                                    }
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        AssetSpecType? supplyType = (AssetSpecType?)_database.FindType("SDG.Unturned.ItemSupplyAsset, Assembly-CSharp", default);
        Assert.That(supplyType, Is.Not.Null);

        SpecProperty nestedProperty = new SpecProperty
        {
            Key = "A",
            Type = new PropertyTypeOrSwitch(KnownTypes.Int32)
        };
        SpecProperty testProperty = new SpecProperty
        {
            Key = "Config",
            Type = new PropertyTypeOrSwitch(CreateCustomTestType(supplyType, nestedProperty)),
            Owner = supplyType
        };

        using IDisposable addProp = supplyType.AddRootPropertyForTest(SpecPropertyContext.Property, testProperty);

        // "/Config/"
        Assert.That(sourceFile.TryGetProperty(testProperty, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn1.Value!).TryGetProperty(nestedProperty, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, PropertyResolutionContext.Modern)
        );

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config/"));

            List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
            Assert.That(breadcrumb.TryGetProperty(sourceFile, nestedProperty, out IPropertySourceNode? sourceNode, path));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            if (withType)
            {
                AssetFileType ft = AssetFileType.FromFile(sourceFile, _database);
                Assert.That(ft.Information, Is.Not.Null);

                path?.Clear();
                Assert.That(breadcrumb.TryGetDictionaryAndType(sourceFile, in ft, _database, out _, out ISpecType? type, path));
                Assert.That(type, Is.SameAs(nestedProperty.Owner));
            }

            if (withPath)
            {
                Assert.That(path, Has.Count.EqualTo(1));
                Assert.That(path[0], Is.SameAs(sn1));
            }
        }
    }

    [Test]
    public void MultiplePropertyNodes([Values(true, false)] bool withPath, [Values(true, false)] bool withType)
    {
        const string fileContents = """
                                    GUID 7ae3737efae940999dc51919ce558ec4
                                    ID 2001
                                    Type Supply
                                    
                                    Config
                                    {
                                        NestedConfig
                                        {
                                            A 1
                                        }
                                    }
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        AssetSpecType? supplyType = (AssetSpecType?)_database.FindType("SDG.Unturned.ItemSupplyAsset, Assembly-CSharp", default);
        Assert.That(supplyType, Is.Not.Null);

        SpecProperty nestedProperty2 = new SpecProperty
        {
            Key = "A",
            Type = new PropertyTypeOrSwitch(KnownTypes.Int32)
        };
        SpecProperty nestedProperty1 = new SpecProperty
        {
            Key = "NestedConfig",
            Type = new PropertyTypeOrSwitch(CreateCustomTestType(supplyType, nestedProperty2))
        };
        SpecProperty testProperty = new SpecProperty
        {
            Key = "Config",
            Type = new PropertyTypeOrSwitch(CreateCustomTestType(supplyType, nestedProperty1)),
            Owner = supplyType
        };

        using IDisposable addProp = supplyType.AddRootPropertyForTest(SpecPropertyContext.Property, testProperty);

        // "/Config/NestedConfig/"
        Assert.That(sourceFile.TryGetProperty(testProperty, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn1.Value!).TryGetProperty(nestedProperty1, out IPropertySourceNode? sn2));
        Assert.That(sn2!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn2.Value!).TryGetProperty(nestedProperty2, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, PropertyResolutionContext.Modern),
            new PropertyBreadcrumbSection(nestedProperty1, PropertyResolutionContext.Modern)
        );

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config/NestedConfig/"));

            List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
            Assert.That(breadcrumb.TryGetProperty(sourceFile, nestedProperty2, out IPropertySourceNode? sourceNode, path));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            if (withType)
            {
                AssetFileType ft = AssetFileType.FromFile(sourceFile, _database);
                Assert.That(ft.Information, Is.Not.Null);

                path?.Clear();
                Assert.That(breadcrumb.TryGetDictionaryAndType(sourceFile, in ft, _database, out _, out ISpecType? type, path));
                Assert.That(type, Is.SameAs(nestedProperty2.Owner));
            }

            if (withPath)
            {
                Assert.That(path, Has.Count.EqualTo(2));
                Assert.That(path[0], Is.SameAs(sn1));
                Assert.That(path[1], Is.SameAs(sn2));
            }
        }
    }

    [Test]
    public void MultiplePropertyAndListNodes([Values(true, false)] bool withPath, [Values(true, false)] bool withType)
    {
        const string fileContents = """
                                    GUID 7ae3737efae940999dc51919ce558ec4
                                    ID 2001
                                    Type Supply
                                    
                                    Config
                                    {
                                        NestedConfig
                                        {
                                            List
                                            [
                                                {
                                                }
                                                {
                                                    // Dictionary<string,> property
                                                    Entry1
                                                    {
                                                        A 1
                                                    }
                                                    Entry2
                                                    {
                                                        A 1
                                                    }
                                                }
                                            ]
                                        }
                                    }
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        AssetSpecType? supplyType = (AssetSpecType?)_database.FindType("SDG.Unturned.ItemSupplyAsset, Assembly-CSharp", default);
        Assert.That(supplyType, Is.Not.Null);

        SpecProperty nestedProperty3 = new SpecProperty
        {
            Key = "A",
            Type = new PropertyTypeOrSwitch(KnownTypes.Int32)
        };
        SpecProperty nestedProperty2 = new SpecProperty
        {
            Key = "List",
            Type = new PropertyTypeOrSwitch(KnownTypes.List(KnownTypes.Dictionary(_database, CreateCustomTestType(supplyType, nestedProperty3)), false))
        };
        SpecProperty nestedProperty1 = new SpecProperty
        {
            Key = "NestedConfig",
            Type = new PropertyTypeOrSwitch(CreateCustomTestType(supplyType, nestedProperty2))
        };
        SpecProperty testProperty = new SpecProperty
        {
            Key = "Config",
            Type = new PropertyTypeOrSwitch(CreateCustomTestType(supplyType, nestedProperty1)),
            Owner = supplyType
        };

        using IDisposable addProp = supplyType.AddRootPropertyForTest(SpecPropertyContext.Property, testProperty);

        // "/Config/NestedConfig/List[1]/Entry2/"
        Assert.That(sourceFile.TryGetProperty(testProperty, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn1.Value!).TryGetProperty(nestedProperty1, out IPropertySourceNode? sn2));
        Assert.That(sn2!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn2.Value!).TryGetProperty(nestedProperty2, out IPropertySourceNode? sn3));
        Assert.That(sn3!.Value is IListSourceNode);
        Assert.That(((IListSourceNode)sn3.Value!).TryGetElement(1, out IAnyValueSourceNode? sn4));
        Assert.That(sn4 is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn4!).TryGetProperty("Entry2", out IPropertySourceNode? sn5));
        Assert.That(sn5!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn5.Value!).TryGetProperty(nestedProperty3, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, PropertyResolutionContext.Modern),
            new PropertyBreadcrumbSection(nestedProperty1, PropertyResolutionContext.Modern),
            new PropertyBreadcrumbSection(nestedProperty2, PropertyResolutionContext.Modern, 1),
            new PropertyBreadcrumbSection("Entry2", PropertyResolutionContext.Modern)
        );

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config/NestedConfig/List[1]/Entry2/"));

            List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
            Assert.That(breadcrumb.TryGetProperty(sourceFile, nestedProperty3, out IPropertySourceNode? sourceNode, path));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            if (withType)
            {
                AssetFileType ft = AssetFileType.FromFile(sourceFile, _database);
                Assert.That(ft.Information, Is.Not.Null);

                path?.Clear();
                Assert.That(breadcrumb.TryGetDictionaryAndType(sourceFile, in ft, _database, out _, out ISpecType? type, path));
                Assert.That(type, Is.SameAs(nestedProperty3.Owner));
            }

            if (withPath)
            {
                Assert.That(path, Has.Count.EqualTo(5));
                Assert.That(path[0], Is.SameAs(sn1));
                Assert.That(path[1], Is.SameAs(sn2));
                Assert.That(path[2], Is.SameAs(sn3));
                Assert.That(path[3], Is.SameAs(sn4));
                Assert.That(path[4], Is.SameAs(sn5));
            }
        }
    }

    [Test]
    public void NestedLists([Values(true, false)] bool withPath, [Values(true, false)] bool withType)
    {
        const string fileContents = """
                                    GUID 7ae3737efae940999dc51919ce558ec4
                                    ID 2001
                                    Type Supply
                                    
                                    Config
                                    [
                                        [
                                        ]
                                        [
                                            {
                                            }
                                            {
                                                A 1
                                            }
                                        ]
                                    ]
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        AssetSpecType? supplyType = (AssetSpecType?)_database.FindType("SDG.Unturned.ItemSupplyAsset, Assembly-CSharp", default);
        Assert.That(supplyType, Is.Not.Null);

        SpecProperty nestedProperty1 = new SpecProperty
        {
            Key = "A",
            Type = new PropertyTypeOrSwitch(KnownTypes.Int32)
        };
        SpecProperty testProperty = new SpecProperty
        {
            Key = "Config",
            Type = new PropertyTypeOrSwitch(KnownTypes.List(KnownTypes.List(CreateCustomTestType(supplyType, nestedProperty1), false), false)),
            Owner = supplyType
        };

        using IDisposable addProp = supplyType.AddRootPropertyForTest(SpecPropertyContext.Property, testProperty);

        // "/Config[1]/[1]/"
        Assert.That(sourceFile.TryGetProperty(testProperty, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IListSourceNode);
        Assert.That(((IListSourceNode)sn1.Value!).TryGetElement(1, out IAnyValueSourceNode? sn2));
        Assert.That(sn2 is IListSourceNode);
        Assert.That(((IListSourceNode)sn2!).TryGetElement(1, out IAnyValueSourceNode? sn3));
        Assert.That(sn3 is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn3!).TryGetProperty(nestedProperty1, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, PropertyResolutionContext.Modern, 1),
            new PropertyBreadcrumbSection(PropertyResolutionContext.Modern, 1)
        );

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config[1]/[1]/"));

            List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
            Assert.That(breadcrumb.TryGetProperty(sourceFile, nestedProperty1, out IPropertySourceNode? sourceNode, path));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            if (withType)
            {
                AssetFileType ft = AssetFileType.FromFile(sourceFile, _database);
                Assert.That(ft.Information, Is.Not.Null);

                path?.Clear();
                Assert.That(breadcrumb.TryGetDictionaryAndType(sourceFile, in ft, _database, out _, out ISpecType? type, path));
                Assert.That(type, Is.SameAs(nestedProperty1.Owner));
            }

            if (withPath)
            {
                Assert.That(path, Has.Count.EqualTo(3));
                Assert.That(path[0], Is.SameAs(sn1));
                Assert.That(path[1], Is.SameAs(sn2));
                Assert.That(path[2], Is.SameAs(sn3));
            }
        }
    }
    [Test]
    public void NestedLists3([Values(true, false)] bool withPath, [Values(true, false)] bool withType)
    {
        const string fileContents = """
                                    GUID 7ae3737efae940999dc51919ce558ec4
                                    ID 2001
                                    Type Supply
                                    
                                    Config
                                    [
                                        [
                                        ]
                                        [
                                            [
                                                {
                                                    A 1
                                                }
                                            ]
                                        ]
                                    ]
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        AssetSpecType? supplyType = (AssetSpecType?)_database.FindType("SDG.Unturned.ItemSupplyAsset, Assembly-CSharp", default);
        Assert.That(supplyType, Is.Not.Null);

        SpecProperty nestedProperty1 = new SpecProperty
        {
            Key = "A",
            Type = new PropertyTypeOrSwitch(KnownTypes.Int32)
        };
        SpecProperty testProperty = new SpecProperty
        {
            Key = "Config",
            Type = new PropertyTypeOrSwitch(KnownTypes.List(KnownTypes.List(CreateCustomTestType(supplyType, nestedProperty1), false), false)),
            Owner = supplyType
        };

        using IDisposable addProp = supplyType.AddRootPropertyForTest(SpecPropertyContext.Property, testProperty);

        // "/Config[1]/[0]/[0]/"
        Assert.That(sourceFile.TryGetProperty(testProperty, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IListSourceNode);
        Assert.That(((IListSourceNode)sn1.Value!).TryGetElement(1, out IAnyValueSourceNode? sn2));
        Assert.That(sn2 is IListSourceNode);
        Assert.That(((IListSourceNode)sn2!).TryGetElement(0, out IAnyValueSourceNode? sn3));
        Assert.That(sn3 is IListSourceNode);
        Assert.That(((IListSourceNode)sn3!).TryGetElement(0, out IAnyValueSourceNode? sn4));
        Assert.That(sn4 is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn4!).TryGetProperty(nestedProperty1, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, PropertyResolutionContext.Modern, 1),
            new PropertyBreadcrumbSection(PropertyResolutionContext.Modern, 0),
            new PropertyBreadcrumbSection(PropertyResolutionContext.Modern, 0)
        );

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config[1]/[0]/[0]/"));

            List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
            Assert.That(breadcrumb.TryGetProperty(sourceFile, nestedProperty1, out IPropertySourceNode? sourceNode, path));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            if (withType)
            {
                AssetFileType ft = AssetFileType.FromFile(sourceFile, _database);
                Assert.That(ft.Information, Is.Not.Null);

                path?.Clear();
                Assert.That(breadcrumb.TryGetDictionaryAndType(sourceFile, in ft, _database, out _, out ISpecType? type, path));
                Assert.That(type, Is.SameAs(nestedProperty1.Owner));
            }

            if (withPath)
            {
                Assert.That(path, Has.Count.EqualTo(4));
                Assert.That(path[0], Is.SameAs(sn1));
                Assert.That(path[1], Is.SameAs(sn2));
                Assert.That(path[2], Is.SameAs(sn3));
                Assert.That(path[3], Is.SameAs(sn4));
            }
        }
    }
}
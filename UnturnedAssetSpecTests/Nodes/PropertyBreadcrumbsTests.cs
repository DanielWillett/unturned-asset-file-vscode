#if NET
using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace UnturnedAssetSpecTests.Nodes;

[TestFixture]
public class PropertyBreadcrumbsTests
{
    private ILoggerFactory _loggerFactory;
    private IAssetSpecDatabase _database;
    private OpenedFileTracker _fileTracker;

    [SetUp]
    public async Task SetUp()
    {
        _loggerFactory = LoggerFactory.Create(l => l.AddSimpleConsole());
        _database = AssetSpecDatabase.FromOffline();
        await _database.InitializeAsync();
        _fileTracker = new OpenedFileTracker(_loggerFactory.CreateLogger<OpenedFileTracker>(), _database);
    }

    [TearDown]
    public void TearDown()
    {
        _loggerFactory.Dispose();
        if (_database is IDisposable d)
            d.Dispose();
        _fileTracker.Dispose();
    }

    [Test]
    public void RootNode([Values(true, false)] bool withPath)
    {
        const string fileContents = """
                                    GUID 7ae3737efae940999dc51919ce558ec4
                                    ID 2001
                                    Type Supply
                                    """;

        using OpenedFile openedFile = _fileTracker.CreateFile(DocumentUri.FromFileSystemPath("./test.dat"), fileContents);

        ISourceFile sourceFile = openedFile.SourceFile;

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.Root;


        SpecProperty? testProperty = _database.FindPropertyInfo("ID", AssetFileType.AssetBaseType(_database));
        Assert.That(testProperty, Is.Not.Null);

        List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
        Assert.That(breadcrumbs.TryGetProperty(sourceFile, testProperty, out IPropertySourceNode? sourceNode, path));

        Assert.That(sourceNode, Is.Not.Null);
        Assert.That(sourceNode.Key, Is.EqualTo("ID"));
        Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("2001"));

        if (withPath)
        {
            Assert.That(path, Is.Empty);
        }
    }

    [Test]
    public void RootNodeInAsset([Values(true, false)] bool withPath)
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

        using OpenedFile openedFile = _fileTracker.CreateFile(DocumentUri.FromFileSystemPath("./test.dat"), fileContents);

        ISourceFile sourceFile = openedFile.SourceFile;

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.Root;


        SpecProperty? testProperty = _database.FindPropertyInfo("ID", AssetFileType.AssetBaseType(_database));
        Assert.That(testProperty, Is.Not.Null);

        List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
        Assert.That(breadcrumbs.TryGetProperty(sourceFile, testProperty, out IPropertySourceNode? sourceNode, path));

        Assert.That(sourceNode, Is.Not.Null);
        Assert.That(sourceNode.Key, Is.EqualTo("ID"));
        Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("2001"));

        if (withPath)
        {
            Assert.That(path, Is.Empty);
        }
    }

    [Test]
    public void RootNodeInMetadata([Values(true, false)] bool withPath)
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

        using OpenedFile openedFile = _fileTracker.CreateFile(DocumentUri.FromFileSystemPath("./test.dat"), fileContents);

        ISourceFile sourceFile = openedFile.SourceFile;

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.Root;


        SpecProperty? testProperty = _database.FindPropertyInfo("GUID", AssetFileType.AssetBaseType(_database));
        Assert.That(testProperty, Is.Not.Null);

        List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
        Assert.That(breadcrumbs.TryGetProperty(sourceFile, testProperty, out IPropertySourceNode? sourceNode, path));

        Assert.That(sourceNode, Is.Not.Null);
        Assert.That(sourceNode.Key, Is.EqualTo("GUID"));
        Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("7ae3737efae940999dc51919ce558ec4"));

        if (withPath)
        {
            Assert.That(path, Is.Empty);
        }
    }

    private static CustomSpecType CreateCustomTestType(AssetSpecType assetType, SpecProperty nestedProperty)
    {
        return new CustomSpecType
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
    }

    [Test]
    public void SinglePropertyNode([Values(true, false)] bool withPath)
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

        using OpenedFile openedFile = _fileTracker.CreateFile(DocumentUri.FromFileSystemPath("./test.dat"), fileContents);

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

        // "/Config/"
        Assert.That(sourceFile.TryGetProperty(testProperty, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn1.Value!).TryGetProperty(nestedProperty, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(new PropertyBreadcrumbSection(testProperty, null, PropertyResolutionContext.Modern));

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config/"));

            List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
            Assert.That(breadcrumb.TryGetProperty(sourceFile, nestedProperty, out IPropertySourceNode? sourceNode, path));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            if (withPath)
            {
                Assert.That(path, Has.Count.EqualTo(1));
                Assert.That(path[0], Is.SameAs(sn1));
            }
        }
    }

    [Test]
    public void MultiplePropertyNodes([Values(true, false)] bool withPath)
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

        using OpenedFile openedFile = _fileTracker.CreateFile(DocumentUri.FromFileSystemPath("./test.dat"), fileContents);

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

        // "/Config/NestedConfig/"
        Assert.That(sourceFile.TryGetProperty(testProperty, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn1.Value!).TryGetProperty(nestedProperty1, out IPropertySourceNode? sn2));
        Assert.That(sn2!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn2.Value!).TryGetProperty(nestedProperty2, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, null, PropertyResolutionContext.Modern),
            new PropertyBreadcrumbSection(nestedProperty1, null, PropertyResolutionContext.Modern)
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

            if (withPath)
            {
                Assert.That(path, Has.Count.EqualTo(2));
                Assert.That(path[0], Is.SameAs(sn1));
                Assert.That(path[1], Is.SameAs(sn2));
            }
        }
    }

    [Test]
    public void MultiplePropertyAndListNodes([Values(true, false)] bool withPath)
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
                                                    A 1
                                                }
                                            ]
                                        }
                                    }
                                    """;

        using OpenedFile openedFile = _fileTracker.CreateFile(DocumentUri.FromFileSystemPath("./test.dat"), fileContents);

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
            Type = new PropertyTypeOrSwitch(KnownTypes.List(KnownTypes.Dictionary(CreateCustomTestType(supplyType, nestedProperty3)), false))
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

        // "/Config/NestedConfig/List[1]/"
        Assert.That(sourceFile.TryGetProperty(testProperty, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn1.Value!).TryGetProperty(nestedProperty1, out IPropertySourceNode? sn2));
        Assert.That(sn2!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn2.Value!).TryGetProperty(nestedProperty2, out IPropertySourceNode? sn3));
        Assert.That(sn3!.Value is IListSourceNode);
        Assert.That(((IListSourceNode)sn3.Value!).TryGetElement(1, out IAnyValueSourceNode? sn4));
        Assert.That(sn4 is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn4!).TryGetProperty(nestedProperty3, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, null, PropertyResolutionContext.Modern),
            new PropertyBreadcrumbSection(nestedProperty1, null, PropertyResolutionContext.Modern),
            new PropertyBreadcrumbSection(nestedProperty2, null, PropertyResolutionContext.Modern, 1)
        );

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config/NestedConfig/List[1]/"));

            List<ISourceNode>? path = withPath ? new List<ISourceNode>() : null;
            Assert.That(breadcrumb.TryGetProperty(sourceFile, nestedProperty3, out IPropertySourceNode? sourceNode, path));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

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

    [Test]
    public void NestedLists([Values(true, false)] bool withPath)
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

        using OpenedFile openedFile = _fileTracker.CreateFile(DocumentUri.FromFileSystemPath("./test.dat"), fileContents);

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
            new PropertyBreadcrumbSection(testProperty, null, PropertyResolutionContext.Modern, 1),
            new PropertyBreadcrumbSection(null, null, PropertyResolutionContext.Modern, 1)
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
    public void NestedLists3([Values(true, false)] bool withPath)
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

        using OpenedFile openedFile = _fileTracker.CreateFile(DocumentUri.FromFileSystemPath("./test.dat"), fileContents);

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
            new PropertyBreadcrumbSection(testProperty, null, PropertyResolutionContext.Modern, 1),
            new PropertyBreadcrumbSection(null, null, PropertyResolutionContext.Modern, 0),
            new PropertyBreadcrumbSection(null, null, PropertyResolutionContext.Modern, 0)
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
#endif
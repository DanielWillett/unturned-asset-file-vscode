using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;

namespace UnturnedAssetSpecTests.Nodes;

[TestFixture]
public class ValueNodeDescriptorTests
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
    public void RootNode([Values(true, false)] bool useProperty)
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

        Assert.That(sourceFile.TryGetProperty("Type", out IPropertySourceNode? property));
        Assert.That(property!.Key, Is.EqualTo("Type"));
        Assert.That(property.Value, Is.AssignableTo<IValueSourceNode>());
        Assert.That(((IValueSourceNode)property.Value).Value, Is.EqualTo("Supply"));

        ValueNodeDescriptor descriptor = ValueNodeDescriptor.FromNode(useProperty ? property : property.Value);

        Assert.That(descriptor.HasValue);
        Assert.That(descriptor.IsListElement, Is.False);
        Assert.That(descriptor.Value, Is.SameAs(property.Value));
        Assert.That(descriptor.Property, Is.SameAs(property));
        Assert.That(descriptor.ListDepth, Is.Zero);
        Assert.That(descriptor.ListProperty, Is.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(1));

        Assert.That(descriptor.ToString(), Is.EqualTo("Type = Supply"));
    }

    [Test]
    public void RootNodeNoValue()
    {
        const string fileContents = """
                                    GUID 7ae3737efae940999dc51919ce558ec4
                                    ID 2001
                                    Type
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        Assert.That(sourceFile.TryGetProperty("Type", out IPropertySourceNode? property));
        Assert.That(property!.Key, Is.EqualTo("Type"));
        Assert.That(property.Value, Is.Null);

        ValueNodeDescriptor descriptor = ValueNodeDescriptor.FromNode(property);

        Assert.That(descriptor.HasValue, Is.False);
        Assert.That(descriptor.IsListElement, Is.False);
        Assert.That(descriptor.Value, Is.Null);
        Assert.That(descriptor.Property, Is.SameAs(property));
        Assert.That(descriptor.ListDepth, Is.Zero);
        Assert.That(descriptor.ListProperty, Is.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(1));

        Assert.That(descriptor.ToString(), Is.EqualTo("Type"));
    }

    [Test]
    public void RootNodeInAsset([Values(true, false)] bool useProperty)
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

        IAssetSourceFile sourceFile = (IAssetSourceFile)openedFile.SourceFile;

        Assert.That(sourceFile.AssetData.TryGetProperty("ID", out IPropertySourceNode? property));
        Assert.That(property!.Key, Is.EqualTo("ID"));
        Assert.That(property.Value, Is.AssignableTo<IValueSourceNode>());
        Assert.That(((IValueSourceNode)property.Value).Value, Is.EqualTo("2001"));

        ValueNodeDescriptor descriptor = ValueNodeDescriptor.FromNode(useProperty ? property : property.Value);

        Assert.That(descriptor.HasValue);
        Assert.That(descriptor.IsListElement, Is.False);
        Assert.That(descriptor.Value, Is.SameAs(property.Value));
        Assert.That(descriptor.Property, Is.SameAs(property));
        Assert.That(descriptor.ListDepth, Is.Zero);
        Assert.That(descriptor.ListProperty, Is.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(1));

        Assert.That(descriptor.ToString(), Is.EqualTo("ID = 2001"));
    }

    [Test]
    public void RootNodeInMetadata([Values(true, false)] bool useProperty)
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

        IAssetSourceFile sourceFile = (IAssetSourceFile)openedFile.SourceFile;

        IDictionarySourceNode? metadata = sourceFile.GetMetadataDictionary();
        Assert.That(metadata, Is.Not.Null);
        Assert.That(metadata.TryGetProperty("GUID", out IPropertySourceNode? property));
        Assert.That(property!.Key, Is.EqualTo("GUID"));
        Assert.That(property.Value, Is.AssignableTo<IValueSourceNode>());
        Assert.That(((IValueSourceNode)property.Value).Value, Is.EqualTo("7ae3737efae940999dc51919ce558ec4"));

        ValueNodeDescriptor descriptor = ValueNodeDescriptor.FromNode(useProperty ? property : property.Value);

        Assert.That(descriptor.HasValue);
        Assert.That(descriptor.IsListElement, Is.False);
        Assert.That(descriptor.Value, Is.SameAs(property.Value));
        Assert.That(descriptor.Property, Is.SameAs(property));
        Assert.That(descriptor.ListDepth, Is.Zero);
        Assert.That(descriptor.ListProperty, Is.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(1));

        Assert.That(descriptor.ToString(), Is.EqualTo("GUID = 7ae3737efae940999dc51919ce558ec4"));
    }

    [Test]
    public void SinglePropertyNode([Values(true, false)] bool useProperty)
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

        IAssetSourceFile sourceFile = (IAssetSourceFile)openedFile.SourceFile;

        Assert.That(sourceFile.TryGetProperty("Config", out IPropertySourceNode? property));
        Assert.That(property!.Key, Is.EqualTo("Config"));
        Assert.That(property.Value, Is.AssignableTo<IDictionarySourceNode>());

        Assert.That(((IDictionarySourceNode)property.Value).TryGetProperty("A", out property));
        Assert.That(property!.Key, Is.EqualTo("A"));
        Assert.That(property.Value, Is.AssignableTo<IValueSourceNode>());
        Assert.That(((IValueSourceNode)property.Value).Value, Is.EqualTo("1"));

        ValueNodeDescriptor descriptor = ValueNodeDescriptor.FromNode(useProperty ? property : property.Value);

        Assert.That(descriptor.HasValue);
        Assert.That(descriptor.IsListElement, Is.False);
        Assert.That(descriptor.Value, Is.SameAs(property.Value));
        Assert.That(descriptor.Property, Is.SameAs(property));
        Assert.That(descriptor.ListDepth, Is.Zero);
        Assert.That(descriptor.ListProperty, Is.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(1));

        Assert.That(descriptor.ToString(), Is.EqualTo("A = 1"));
    }

    [Test]
    public void MultiplePropertyNodes([Values(true, false)] bool useProperty)
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

        IAssetSourceFile sourceFile = (IAssetSourceFile)openedFile.SourceFile;

        Assert.That(sourceFile.TryGetProperty("Config", out IPropertySourceNode? property));
        Assert.That(property!.Key, Is.EqualTo("Config"));
        Assert.That(property.Value, Is.AssignableTo<IDictionarySourceNode>());

        Assert.That(((IDictionarySourceNode)property.Value).TryGetProperty("NestedConfig", out property));
        Assert.That(property!.Key, Is.EqualTo("NestedConfig"));
        Assert.That(property.Value, Is.AssignableTo<IDictionarySourceNode>());

        Assert.That(((IDictionarySourceNode)property.Value).TryGetProperty("A", out property));
        Assert.That(property!.Key, Is.EqualTo("A"));
        Assert.That(property.Value, Is.AssignableTo<IValueSourceNode>());
        Assert.That(((IValueSourceNode)property.Value).Value, Is.EqualTo("1"));

        ValueNodeDescriptor descriptor = ValueNodeDescriptor.FromNode(useProperty ? property : property.Value);

        Assert.That(descriptor.HasValue);
        Assert.That(descriptor.IsListElement, Is.False);
        Assert.That(descriptor.Value, Is.SameAs(property.Value));
        Assert.That(descriptor.Property, Is.SameAs(property));
        Assert.That(descriptor.ListDepth, Is.Zero);
        Assert.That(descriptor.ListProperty, Is.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(1));

        Assert.That(descriptor.ToString(), Is.EqualTo("A = 1"));
    }

    [Test]
    public void MultiplePropertyAndListNodes([Values(true, false)] bool useProperty)
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

        IAssetSourceFile sourceFile = (IAssetSourceFile)openedFile.SourceFile;

        Assert.That(sourceFile.TryGetProperty("Config", out IPropertySourceNode? property));
        Assert.That(property!.Key, Is.EqualTo("Config"));
        Assert.That(property.Value, Is.AssignableTo<IDictionarySourceNode>());

        Assert.That(((IDictionarySourceNode)property.Value).TryGetProperty("NestedConfig", out property));
        Assert.That(property!.Key, Is.EqualTo("NestedConfig"));
        Assert.That(property.Value, Is.AssignableTo<IDictionarySourceNode>());

        Assert.That(((IDictionarySourceNode)property.Value).TryGetProperty("List", out property));
        Assert.That(property!.Key, Is.EqualTo("List"));
        Assert.That(property.Value, Is.AssignableTo<IListSourceNode>());

        Assert.That(((IListSourceNode)property.Value).Children, Has.Length.EqualTo(2));
        Assert.That(((IDictionarySourceNode)((IListSourceNode)property.Value).Children[1]).TryGetProperty("Entry1", out property));
        Assert.That(property!.Key, Is.EqualTo("Entry1"));
        Assert.That(property.Value, Is.AssignableTo<IDictionarySourceNode>());

        Assert.That(((IDictionarySourceNode)property.Value).TryGetProperty("A", out property));
        Assert.That(property!.Key, Is.EqualTo("A"));
        Assert.That(property.Value, Is.AssignableTo<IValueSourceNode>());
        Assert.That(((IValueSourceNode)property.Value).Value, Is.EqualTo("1"));

        ValueNodeDescriptor descriptor = ValueNodeDescriptor.FromNode(useProperty ? property : property.Value);

        Assert.That(descriptor.HasValue);
        Assert.That(descriptor.IsListElement, Is.False);
        Assert.That(descriptor.Value, Is.SameAs(property.Value));
        Assert.That(descriptor.Property, Is.SameAs(property));
        Assert.That(descriptor.ListDepth, Is.Zero);
        Assert.That(descriptor.ListProperty, Is.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(1));

        Assert.That(descriptor.ToString(), Is.EqualTo("A = 1"));
    }

    [Test]
    public void ValueInList()
    {
        const string fileContents = """
                                    GUID 7ae3737efae940999dc51919ce558ec4
                                    ID 2001
                                    Type Supply
                                    
                                    Config
                                    [
                                        A
                                        B
                                        C
                                    ]
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        IAssetSourceFile sourceFile = (IAssetSourceFile)openedFile.SourceFile;

        Assert.That(sourceFile.TryGetProperty("Config", out IPropertySourceNode? property));
        Assert.That(property!.Key, Is.EqualTo("Config"));
        Assert.That(property.Value, Is.AssignableTo<IListSourceNode>());

        Assert.That(((IListSourceNode)property.Value).Children, Has.Length.EqualTo(3));
        Assert.That(((IListSourceNode)property.Value).Children[1], Is.AssignableTo<IValueSourceNode>());

        IValueSourceNode value = (IValueSourceNode)((IListSourceNode)property.Value).Children[1];
        Assert.That(value.Value, Is.EqualTo("B"));

        ValueNodeDescriptor descriptor = ValueNodeDescriptor.FromNode(value);

        Assert.That(descriptor.HasValue);
        Assert.That(descriptor.IsListElement, Is.True);
        Assert.That(descriptor.Value, Is.SameAs(value));
        Assert.That(descriptor.Property, Is.Null);
        Assert.That(descriptor.ListDepth, Is.EqualTo(1));
        Assert.That(descriptor.ListProperty, Is.SameAs(property));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(-1));
        Assert.That(descriptor.GetListIndexByDepth(0), Is.EqualTo(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(1));

        Assert.That(descriptor.ToString(), Is.EqualTo("Config[1] = B"));
    }

    [Test]
    public void NestedLists()
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
                                            A
                                            B
                                            C
                                        ]
                                    ]
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _database,
            SourceNodeTokenizerOptions.None
        );

        IAssetSourceFile sourceFile = (IAssetSourceFile)openedFile.SourceFile;

        Assert.That(sourceFile.TryGetProperty("Config", out IPropertySourceNode? property));
        Assert.That(property!.Key, Is.EqualTo("Config"));
        Assert.That(property.Value, Is.AssignableTo<IListSourceNode>());

        Assert.That(((IListSourceNode)property.Value).Children, Has.Length.EqualTo(2));
        Assert.That(((IListSourceNode)property.Value).Children[1], Is.AssignableTo<IListSourceNode>());

        Assert.That(((IListSourceNode)((IListSourceNode)property.Value).Children[1]).Children, Has.Length.EqualTo(3));
        Assert.That(((IListSourceNode)((IListSourceNode)property.Value).Children[1]).Children[2], Is.AssignableTo<IValueSourceNode>());

        IValueSourceNode value = (IValueSourceNode)((IListSourceNode)((IListSourceNode)property.Value).Children[1]).Children[2];
        Assert.That(value.Value, Is.EqualTo("C"));

        ValueNodeDescriptor descriptor = ValueNodeDescriptor.FromNode(value);

        Assert.That(descriptor.HasValue);
        Assert.That(descriptor.IsListElement, Is.True);
        Assert.That(descriptor.Value, Is.SameAs(value));
        Assert.That(descriptor.Property, Is.Null);
        Assert.That(descriptor.ListDepth, Is.EqualTo(2));
        Assert.That(descriptor.ListProperty, Is.SameAs(property));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(-1));
        Assert.That(descriptor.GetListIndexByDepth(0), Is.EqualTo(1));
        Assert.That(descriptor.GetListIndexByDepth(1), Is.EqualTo(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => descriptor.GetListIndexByDepth(2));

        Assert.That(descriptor.ToString(), Is.EqualTo("Config[1][2] = C"));
    }
}
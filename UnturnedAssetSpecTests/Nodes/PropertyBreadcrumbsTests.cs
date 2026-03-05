using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;

namespace UnturnedAssetSpecTests.Nodes;

[TestFixture]
public class PropertyBreadcrumbsTests
{
    private IParsingServices _parsingServices;

    [SetUp]
    public async Task SetUp()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(l => l.AddSimpleConsole());
        IAssetSpecDatabase database = AssetSpecDatabase.FromOffline(loggerFactory: loggerFactory);

        _parsingServices = new ParsingServiceProvider(
            database,
            loggerFactory,
            new StaticSourceFileWorkspaceEnvironment(false, database),
            database.UnturnedInstallDirectory,
            new InstallationEnvironment(database),
            new NilProjectFileProvider(database)
        );

        await database.InitializeAsync();
    }

    [TearDown]
    public void TearDown()
    {
        (_parsingServices as IDisposable)?.Dispose();
    }

    [Test]
    public void RootNode()
    {
        const string fileContents = """
                                    GUID 7ae3737efae940999dc51919ce558ec4
                                    ID 2001
                                    Type Supply
                                    """;

        using StaticSourceFile openedFile = StaticSourceFile.FromAssetFile(
            "./test.dat",
            fileContents.AsMemory(),
            _parsingServices.Database,
            SourceNodeTokenizerOptions.None
        );

        IAssetSourceFile sourceFile = (IAssetSourceFile)openedFile.SourceFile;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, sourceFile);

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.Root;
        DatFileType? assetType = AssetFileType.AssetBaseType(_parsingServices.Database).Information;
        Assert.That(assetType, Is.Not.Null);

        Assert.That(assetType.TryFindProperty("ID", SpecPropertyContext.Property, out DatProperty? testProperty));
        Assert.That(testProperty, Is.Not.Null);

        ctx.RootBreadcrumbs = breadcrumbs;
        Assert.That(ctx.TryGetTargetPropertyNodeForProperty(testProperty, out IPropertySourceNode? sourceNode));

        Assert.That(sourceNode, Is.Not.Null);
        Assert.That(sourceNode.Key, Is.EqualTo("ID"));
        Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("2001"));

        Assert.That(ctx.TryGetTargetRoot(out IDictionarySourceNode? targetRoot));
        Assert.That(breadcrumbs.TryTraceRelativeTo(targetRoot!, assetType, out IAnyValueSourceNode? dictionary, out IType? valueType, ref ctx));
        Assert.That(dictionary, Is.SameAs(sourceFile));
        Assert.That(valueType, Is.SameAs(assetType));

        PropertyBreadcrumbs bc = PropertyBreadcrumbs.FromPropertyRef("ID", out string propertyName);
        Assert.That(bc.IsRoot);
        Assert.That(propertyName, Is.EqualTo("ID"));

        bc.ResolveFromPropertyRef(assetType, _parsingServices.Database);
        Assert.That(bc.IsRoot);
    }

    [Test]
    public void RootNodeInAsset()
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
            _parsingServices.Database,
            SourceNodeTokenizerOptions.None
        );

        IAssetSourceFile sourceFile = (IAssetSourceFile)openedFile.SourceFile;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, sourceFile, AssetDatPropertyPosition.Asset);

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.Root;
        DatFileType? assetType = AssetFileType.AssetBaseType(_parsingServices.Database).Information;
        Assert.That(assetType, Is.Not.Null);

        Assert.That(assetType.TryFindProperty("ID", SpecPropertyContext.Property, out DatProperty? testProperty));
        Assert.That(testProperty, Is.Not.Null);

        ctx.RootBreadcrumbs = breadcrumbs;
        Assert.That(ctx.TryGetTargetPropertyNodeForProperty(testProperty, out IPropertySourceNode? sourceNode));

        Assert.That(sourceNode, Is.Not.Null);
        Assert.That(sourceNode.Key, Is.EqualTo("ID"));
        Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("2001"));
        
        Assert.That(ctx.TryGetTargetRoot(out IDictionarySourceNode? targetRoot));
        Assert.That(breadcrumbs.TryTraceRelativeTo(targetRoot!, assetType, out IAnyValueSourceNode? dictionary, out IType? valueType, ref ctx));
        Assert.That(dictionary, Is.SameAs(sourceFile.GetAssetDataDictionary()));
        Assert.That(valueType, Is.SameAs(assetType));

        PropertyBreadcrumbs bc = PropertyBreadcrumbs.FromPropertyRef("ID", out string propertyName);
        Assert.That(bc.IsRoot);
        Assert.That(propertyName, Is.EqualTo("ID"));

        bc.ResolveFromPropertyRef(assetType, _parsingServices.Database);
        Assert.That(bc.IsRoot);
    }

    [Test]
    public void RootNodeInMetadata()
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
            _parsingServices.Database,
            SourceNodeTokenizerOptions.None
        );

        IAssetSourceFile sourceFile = (IAssetSourceFile)openedFile.SourceFile;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, sourceFile, AssetDatPropertyPosition.Metadata);

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.Root;
        DatFileType? assetType = AssetFileType.AssetBaseType(_parsingServices.Database).Information;
        Assert.That(assetType, Is.Not.Null);

        Assert.That(assetType.TryFindProperty("GUID", SpecPropertyContext.Property, out DatProperty? testProperty));
        Assert.That(testProperty, Is.Not.Null);

        ctx.RootBreadcrumbs = breadcrumbs;
        Assert.That(ctx.TryGetTargetPropertyNodeForProperty(testProperty, out IPropertySourceNode? sourceNode));

        Assert.That(sourceNode, Is.Not.Null);
        Assert.That(sourceNode.Key, Is.EqualTo("GUID"));
        Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("7ae3737efae940999dc51919ce558ec4"));

        Assert.That(ctx.TryGetTargetRoot(out IDictionarySourceNode? targetRoot));
        Assert.That(breadcrumbs.TryTraceRelativeTo(targetRoot!, assetType, out IAnyValueSourceNode? dictionary, out IType? valueType, ref ctx));
        Assert.That(dictionary, Is.SameAs(sourceFile.GetMetadataDictionary()));
        Assert.That(valueType, Is.SameAs(assetType));

        PropertyBreadcrumbs bc = PropertyBreadcrumbs.FromPropertyRef("GUID", out string propertyName);
        Assert.That(bc.IsRoot);
        Assert.That(propertyName, Is.EqualTo("GUID"));

        bc.ResolveFromPropertyRef(assetType, _parsingServices.Database);
        Assert.That(bc.IsRoot);
    }

    private DatCustomAssetType CreateCustomTestType(DatAssetFileType assetType, Func<DatCustomType, DatProperty> nestedProperty)
    {
        DatCustomType ct = DatCustomType.CreateCustomType(
            assetType.TypeName.GetFullTypeName() + "+TestConfigType, Assembly-CSharp",
            default,
            null,
            assetType,
            _parsingServices.Database.ReadContext
        );

        ct.DisplayNameIntl = "Test Config Type";
        ct.Properties = ImmutableArray.Create(nestedProperty(ct));
        return (DatCustomAssetType)ct;
    }

    [Test]
    public void SinglePropertyNode()
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
            _parsingServices.Database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, sourceFile, AssetDatPropertyPosition.Root);

        DatAssetFileType? supplyType = (DatAssetFileType)AssetFileType.FromType("SDG.Unturned.ItemSupplyAsset, Assembly-CSharp", _parsingServices.Database).Information;
        Assert.That(supplyType, Is.Not.Null);


        DatProperty testProperty = DatProperty.Create("Config", supplyType, default, SpecPropertyContext.Property);

        DatProperty? nestedProperty = null;
        testProperty.Type = CreateCustomTestType(supplyType, ct =>
        {
            nestedProperty = DatProperty.Create("A", ct, default, SpecPropertyContext.Property);
            nestedProperty.Type = Int32Type.Instance;
            return nestedProperty;
        });

        Assert.That(nestedProperty, Is.Not.Null);

        using IDisposable addProp = AddRootPropertyForTest(supplyType, SpecPropertyContext.Property, testProperty);

        // "/Config/"
        Assert.That(sourceFile.TryGetProperty(testProperty, ref ctx, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IDictionarySourceNode);
        IDictionarySourceNode expectedDictionary = (IDictionarySourceNode)sn1.Value!;
        Assert.That(expectedDictionary.TryGetProperty(nestedProperty, ref ctx, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, PropertyResolutionContext.Modern)
        );

        PropertyBreadcrumbs parsedBreadcrumbs = PropertyBreadcrumbs.FromPropertyRef("Config.A", out string propertyName);

        Assert.That(parsedBreadcrumbs.Length, Is.EqualTo(1));
        Assert.That(propertyName, Is.EqualTo("A"));
        parsedBreadcrumbs.ResolveFromPropertyRef(supplyType, _parsingServices.Database);

        PropertyBreadcrumbs combinedBreadcrumbsString = PropertyBreadcrumbs.Root.Combine("Config");
        PropertyBreadcrumbs combinedBreadcrumbsNode = PropertyBreadcrumbs.Root.Combine(sn1);

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs, parsedBreadcrumbs, combinedBreadcrumbsString, combinedBreadcrumbsNode ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config/"));

            ctx.RootBreadcrumbs = breadcrumb;
            Assert.That(ctx.TryGetTargetPropertyNodeForProperty(nestedProperty, out IPropertySourceNode? sourceNode));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            Assert.That(ctx.TryGetTargetRoot(out IDictionarySourceNode? targetRoot));
            Assert.That(breadcrumb.TryTraceRelativeTo(targetRoot!, supplyType, out IAnyValueSourceNode? dictionary, out IType? valueType, ref ctx));
            Assert.That(dictionary, Is.SameAs(expectedDictionary));
            Assert.That(valueType, Is.SameAs(nestedProperty.Owner));
        }
    }

    [Test]
    public void MultiplePropertyNodes()
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
            _parsingServices.Database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, sourceFile, AssetDatPropertyPosition.Root);

        DatAssetFileType? supplyType = (DatAssetFileType)AssetFileType.FromType("SDG.Unturned.ItemSupplyAsset, Assembly-CSharp", _parsingServices.Database).Information;
        Assert.That(supplyType, Is.Not.Null);

        DatProperty? nestedProperty1 = null, nestedProperty2 = null;
        DatProperty testProperty = DatProperty.Create("Config", supplyType, default, SpecPropertyContext.Property);

        testProperty.Type = CreateCustomTestType(supplyType, ct =>
        {
            nestedProperty1 = DatProperty.Create("NestedConfig", ct, default, SpecPropertyContext.Property);
            nestedProperty1.Type = CreateCustomTestType(supplyType, ct =>
            {
                nestedProperty2 = DatProperty.Create("A", ct, default, SpecPropertyContext.Property);
                nestedProperty2.Type = Int32Type.Instance;
                return nestedProperty2;
            });

            return nestedProperty1;
        });

        Assert.That(nestedProperty1, Is.Not.Null);
        Assert.That(nestedProperty2, Is.Not.Null);

        using IDisposable addProp = AddRootPropertyForTest(supplyType, SpecPropertyContext.Property, testProperty);

        // "/Config/NestedConfig/"
        Assert.That(sourceFile.TryGetProperty(testProperty, ref ctx, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn1.Value!).TryGetProperty(nestedProperty1, ref ctx, out IPropertySourceNode? sn2));
        Assert.That(sn2!.Value is IDictionarySourceNode);
        IDictionarySourceNode expectedDictionary = ((IDictionarySourceNode)sn2.Value!);
        Assert.That(expectedDictionary.TryGetProperty(nestedProperty2, ref ctx, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, PropertyResolutionContext.Modern),
            new PropertyBreadcrumbSection(nestedProperty1, PropertyResolutionContext.Modern)
        );

        PropertyBreadcrumbs parsedBreadcrumbs = PropertyBreadcrumbs.FromPropertyRef("Config.NestedConfig.A", out string propertyName);
        Assert.That(parsedBreadcrumbs.Length, Is.EqualTo(2));
        Assert.That(propertyName, Is.EqualTo("A"));
        parsedBreadcrumbs.ResolveFromPropertyRef(supplyType, _parsingServices.Database);

        PropertyBreadcrumbs combinedBreadcrumbsString = PropertyBreadcrumbs.Root.Combine("Config").Combine("NestedConfig");
        PropertyBreadcrumbs combinedBreadcrumbsNode = PropertyBreadcrumbs.Root.Combine(sn1).Combine(sn2);

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs, parsedBreadcrumbs, combinedBreadcrumbsString, combinedBreadcrumbsNode ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config/NestedConfig/"));

            ctx.RootBreadcrumbs = breadcrumb;
            Assert.That(ctx.TryGetTargetPropertyNodeForProperty(nestedProperty2, out IPropertySourceNode? sourceNode));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            Assert.That(ctx.TryGetTargetRoot(out IDictionarySourceNode? targetRoot));
            Assert.That(breadcrumb.TryTraceRelativeTo(targetRoot!, supplyType, out IAnyValueSourceNode? dictionary, out IType? valueType, ref ctx));
            Assert.That(dictionary, Is.SameAs(expectedDictionary));
            Assert.That(valueType, Is.SameAs(nestedProperty2.Owner));
        }
    }

    [Test]
    public void MultiplePropertyAndListNodes()
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
            _parsingServices.Database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, sourceFile, AssetDatPropertyPosition.Root);

        DatAssetFileType? supplyType = (DatAssetFileType)AssetFileType.FromType("SDG.Unturned.ItemSupplyAsset, Assembly-CSharp", _parsingServices.Database).Information;
        Assert.That(supplyType, Is.Not.Null);

        DatProperty? nestedProperty1 = null, nestedProperty2 = null, nestedProperty3 = null;
        DatProperty testProperty = DatProperty.Create("Config", supplyType, default, SpecPropertyContext.Property);

        testProperty.Type = CreateCustomTestType(supplyType, ct =>
        {
            nestedProperty1 = DatProperty.Create("NestedConfig", ct, default, SpecPropertyContext.Property);
            nestedProperty1.Type = CreateCustomTestType(supplyType, ct =>
            {
                nestedProperty2 = DatProperty.Create("List", ct, default, SpecPropertyContext.Property);
                nestedProperty2.Type = ListType.Create(
                    DictionaryType.Create(
                        new DictionaryTypeArgs<DatObjectValue>(),
                        CreateCustomTestType(supplyType, ct =>
                        {
                            nestedProperty3 = DatProperty.Create("A", ct, default, SpecPropertyContext.Property);
                            nestedProperty3.Type = Int32Type.Instance;
                            return nestedProperty3;
                        })
                    )
                );

                return nestedProperty2;
            });

            return nestedProperty1;
        });

        Assert.That(nestedProperty1, Is.Not.Null);
        Assert.That(nestedProperty2, Is.Not.Null);
        Assert.That(nestedProperty3, Is.Not.Null);

        using IDisposable addProp = AddRootPropertyForTest(supplyType, SpecPropertyContext.Property, testProperty);

        // "/Config/NestedConfig/List[1]/Entry2/"
        Assert.That(sourceFile.TryGetProperty(testProperty, ref ctx, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn1.Value!).TryGetProperty(nestedProperty1, ref ctx, out IPropertySourceNode? sn2));
        Assert.That(sn2!.Value is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn2.Value!).TryGetProperty(nestedProperty2, ref ctx, out IPropertySourceNode? sn3));
        Assert.That(sn3!.Value is IListSourceNode);
        Assert.That(((IListSourceNode)sn3.Value!).TryGetElement(1, out IAnyValueSourceNode? sn4));
        Assert.That(sn4 is IDictionarySourceNode);
        Assert.That(((IDictionarySourceNode)sn4!).TryGetProperty("Entry2", out IPropertySourceNode? sn5));
        Assert.That(sn5!.Value is IDictionarySourceNode);
        IDictionarySourceNode expectedDictionary = (IDictionarySourceNode)sn5.Value!;
        Assert.That(expectedDictionary.TryGetProperty(nestedProperty3, ref ctx, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, PropertyResolutionContext.Modern),
            new PropertyBreadcrumbSection(nestedProperty1, PropertyResolutionContext.Modern),
            new PropertyBreadcrumbSection(nestedProperty2, PropertyResolutionContext.Modern, 1),
            new PropertyBreadcrumbSection("Entry2", PropertyResolutionContext.Modern)
        );

        PropertyBreadcrumbs parsedBreadcrumbs = PropertyBreadcrumbs.FromPropertyRef("Config.NestedConfig.List[1].Entry2.A", out string propertyName);
        Assert.That(parsedBreadcrumbs.Length, Is.EqualTo(4));
        Assert.That(propertyName, Is.EqualTo("A"));
        parsedBreadcrumbs.ResolveFromPropertyRef(supplyType, _parsingServices.Database);

        PropertyBreadcrumbs combinedBreadcrumbsString = PropertyBreadcrumbs.Root.Combine("Config").Combine("NestedConfig").Combine("List", index: 1).Combine("Entry2");
        PropertyBreadcrumbs combinedBreadcrumbsNode = PropertyBreadcrumbs.Root.Combine(sn1).Combine(sn2).Combine(sn4).Combine(sn5);

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs, parsedBreadcrumbs, combinedBreadcrumbsString, combinedBreadcrumbsNode ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config/NestedConfig/List[1]/Entry2/"));

            ctx.RootBreadcrumbs = breadcrumb;
            Assert.That(ctx.TryGetTargetPropertyNodeForProperty(nestedProperty3, out IPropertySourceNode? sourceNode));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            Assert.That(ctx.TryGetTargetRoot(out IDictionarySourceNode? targetRoot));
            Assert.That(breadcrumb.TryTraceRelativeTo(targetRoot!, supplyType, out IAnyValueSourceNode? dictionary, out IType? valueType, ref ctx));
            Assert.That(dictionary, Is.SameAs(expectedDictionary));
            Assert.That(valueType, Is.SameAs(nestedProperty3.Owner));
        }
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
            _parsingServices.Database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, sourceFile, AssetDatPropertyPosition.Root);

        DatAssetFileType? supplyType = (DatAssetFileType)AssetFileType.FromType("SDG.Unturned.ItemSupplyAsset, Assembly-CSharp", _parsingServices.Database).Information;
        Assert.That(supplyType, Is.Not.Null);

        DatProperty testProperty = DatProperty.Create("Config", supplyType, default, SpecPropertyContext.Property);

        DatProperty? nestedProperty = null;
        testProperty.Type = ListType.Create(
            ListType.Create(
                CreateCustomTestType(supplyType, ct =>
                {
                    nestedProperty = DatProperty.Create("A", ct, default, SpecPropertyContext.Property);
                    nestedProperty.Type = Int32Type.Instance;
                    return nestedProperty;
                })
            )
        );

        Assert.That(nestedProperty, Is.Not.Null);

        using IDisposable addProp = AddRootPropertyForTest(supplyType, SpecPropertyContext.Property, testProperty);

        // "/Config[1]/[1]/"
        Assert.That(sourceFile.TryGetProperty(testProperty, ref ctx, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IListSourceNode);
        Assert.That(((IListSourceNode)sn1.Value!).TryGetElement(1, out IAnyValueSourceNode? sn2));
        Assert.That(sn2 is IListSourceNode);
        Assert.That(((IListSourceNode)sn2!).TryGetElement(1, out IAnyValueSourceNode? sn3));
        Assert.That(sn3 is IDictionarySourceNode);
        IDictionarySourceNode expectedDictionary = (IDictionarySourceNode)sn3!;
        Assert.That(expectedDictionary.TryGetProperty(nestedProperty, ref ctx, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, PropertyResolutionContext.Modern, 1),
            new PropertyBreadcrumbSection(PropertyResolutionContext.Modern, 1)
        );
        
        PropertyBreadcrumbs parsedBreadcrumbs = PropertyBreadcrumbs.FromPropertyRef("Config[1][1].A", out string propertyName);
        Assert.That(parsedBreadcrumbs.Length, Is.EqualTo(2));
        Assert.That(propertyName, Is.EqualTo("A"));
        parsedBreadcrumbs.ResolveFromPropertyRef(supplyType, _parsingServices.Database);

        PropertyBreadcrumbs combinedBreadcrumbsString = PropertyBreadcrumbs.Root.Combine("Config", index: 1).Combine(index: 1);
        PropertyBreadcrumbs combinedBreadcrumbsNode = PropertyBreadcrumbs.Root.Combine(sn1).Combine(sn2).Combine(sn3);

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs, parsedBreadcrumbs, combinedBreadcrumbsString, combinedBreadcrumbsNode ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config[1]/[1]/"));

            ctx.RootBreadcrumbs = breadcrumb;
            Assert.That(ctx.TryGetTargetPropertyNodeForProperty(nestedProperty, out IPropertySourceNode? sourceNode));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            Assert.That(ctx.TryGetTargetRoot(out IDictionarySourceNode? targetRoot));
            Assert.That(breadcrumb.TryTraceRelativeTo(targetRoot!, supplyType, out IAnyValueSourceNode? dictionary, out IType? valueType, ref ctx));
            Assert.That(dictionary, Is.SameAs(expectedDictionary));
            Assert.That(valueType, Is.SameAs(nestedProperty.Owner));
            
        }
    }

    [Test]
    public void NestedLists3()
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
            _parsingServices.Database,
            SourceNodeTokenizerOptions.None
        );

        ISourceFile sourceFile = openedFile.SourceFile;

        FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices, sourceFile, AssetDatPropertyPosition.Root);

        DatAssetFileType? supplyType = (DatAssetFileType)AssetFileType.FromType("SDG.Unturned.ItemSupplyAsset, Assembly-CSharp", _parsingServices.Database).Information;
        Assert.That(supplyType, Is.Not.Null);

        DatProperty testProperty = DatProperty.Create("Config", supplyType, default, SpecPropertyContext.Property);

        DatProperty? nestedProperty = null;
        testProperty.Type = ListType.Create(
            ListType.Create(
                ListType.Create(
                    CreateCustomTestType(supplyType, ct =>
                    {
                        nestedProperty = DatProperty.Create("A", ct, default, SpecPropertyContext.Property);
                        nestedProperty.Type = Int32Type.Instance;
                        return nestedProperty;
                    })
                )
            )
        );

        Assert.That(nestedProperty, Is.Not.Null);

        using IDisposable addProp = AddRootPropertyForTest(supplyType, SpecPropertyContext.Property, testProperty);

        // "/Config[1]/[0]/[0]/"
        Assert.That(sourceFile.TryGetProperty(testProperty, ref ctx, out IPropertySourceNode? sn1));
        Assert.That(sn1!.Value is IListSourceNode);
        Assert.That(((IListSourceNode)sn1.Value!).TryGetElement(1, out IAnyValueSourceNode? sn2));
        Assert.That(sn2 is IListSourceNode);
        Assert.That(((IListSourceNode)sn2!).TryGetElement(0, out IAnyValueSourceNode? sn3));
        Assert.That(sn3 is IListSourceNode);
        Assert.That(((IListSourceNode)sn3!).TryGetElement(0, out IAnyValueSourceNode? sn4));
        Assert.That(sn4 is IDictionarySourceNode);
        IDictionarySourceNode expectedDictionary = (IDictionarySourceNode)sn4!;
        Assert.That(expectedDictionary.TryGetProperty(nestedProperty, ref ctx, out IPropertySourceNode? prop));

        PropertyBreadcrumbs autoBreadcrumbs = PropertyBreadcrumbs.FromNode(prop!);

        PropertyBreadcrumbs manualBreadcrumbs = new PropertyBreadcrumbs(
            new PropertyBreadcrumbSection(testProperty, PropertyResolutionContext.Modern, 1),
            new PropertyBreadcrumbSection(PropertyResolutionContext.Modern, 0),
            new PropertyBreadcrumbSection(PropertyResolutionContext.Modern, 0)
        );
        
        PropertyBreadcrumbs parsedBreadcrumbs = PropertyBreadcrumbs.FromPropertyRef("Config[1][0][0].A", out string propertyName);
        Assert.That(parsedBreadcrumbs.Length, Is.EqualTo(3));
        Assert.That(propertyName, Is.EqualTo("A"));
        parsedBreadcrumbs.ResolveFromPropertyRef(supplyType, _parsingServices.Database);

        PropertyBreadcrumbs combinedBreadcrumbsString = PropertyBreadcrumbs.Root.Combine("Config", index: 1).Combine(index: 0).Combine(index: 0);
        PropertyBreadcrumbs combinedBreadcrumbsNode = PropertyBreadcrumbs.Root.Combine(sn1).Combine(sn2).Combine(sn3).Combine(sn4!);

        PropertyBreadcrumbs[] breadcrumbs = [ autoBreadcrumbs, manualBreadcrumbs, parsedBreadcrumbs, combinedBreadcrumbsString, combinedBreadcrumbsNode ];

        foreach (PropertyBreadcrumbs breadcrumb in breadcrumbs)
        {
            Assert.That(breadcrumb.ToString(), Is.EqualTo("/Config[1]/[0]/[0]/"));

            ctx.RootBreadcrumbs = breadcrumb;
            Assert.That(ctx.TryGetTargetPropertyNodeForProperty(nestedProperty, out IPropertySourceNode? sourceNode));

            Assert.That(sourceNode, Is.Not.Null);
            Assert.That(sourceNode.Key, Is.EqualTo("A"));
            Assert.That((sourceNode.Value as IValueSourceNode)?.Value, Is.EqualTo("1"));

            Assert.That(ctx.TryGetTargetRoot(out IDictionarySourceNode? targetRoot));
            Assert.That(breadcrumb.TryTraceRelativeTo(targetRoot!, supplyType, out IAnyValueSourceNode? dictionary, out IType? valueType, ref ctx));
            Assert.That(dictionary, Is.SameAs(expectedDictionary));
            Assert.That(valueType, Is.SameAs(nestedProperty.Owner));
        }
    }

    [Test]
    [TestCase("", "", "")]
    [TestCase("Property", "", "Property")]
    [TestCase("Dictionary.Property", "Dictionary/", "Property")]
    [TestCase("List[2].Property", "List[2]/", "Property")]
    [TestCase("List[2].Dictionary.Property", "List[2]/Dictionary/", "Property")]
    [TestCase("List[2][3].Property", "List[2]/[3]/", "Property")]
    [TestCase(@"List\[2][3].Property\.Name", "List[2][3]/", "Property.Name")]
    [TestCase(@"Property\.Name", "", "Property.Name")]
    public void FromPropertyRef(string propertyRef, string expectedBreadcrumbs, string expectedProperty)
    {
        PropertyBreadcrumbs crumbs = PropertyBreadcrumbs.FromPropertyRef(propertyRef, out string propertyName);
        
        Assert.That(crumbs.ToString(rootSlash: false), Is.EqualTo(expectedBreadcrumbs));
        Assert.That(propertyName, Is.EqualTo(expectedProperty));
    }

    private static IDisposable AddRootPropertyForTest(DatTypeWithProperties type, SpecPropertyContext context, DatProperty testProperty)
    {
        ImmutableArray<DatProperty> oldArray = type.GetPropertyArray(context);
        ImmutableArray<DatProperty>.Builder bldr = ImmutableArray.CreateBuilder<DatProperty>(oldArray.Length + 1);
        bldr.AddRange(oldArray);
        bldr.Add(testProperty);
        switch (context)
        {
            case SpecPropertyContext.Property:
            case SpecPropertyContext.CrossReferenceProperty:
                type.Properties = bldr.ToImmutable();
                break;
            case SpecPropertyContext.Localization:
            case SpecPropertyContext.CrossReferenceLocalization:
                switch (type)
                {
                    case DatAssetFileType af:
                        af.LocalizationProperties = bldr.ToImmutable();
                        break;
                    case DatCustomAssetType at:
                        at.LocalizationProperties = bldr.ToImmutable();
                        break;
                }
                break;
        }

        return new UndoAddProperty(oldArray, type, context);
    }


    private class UndoAddProperty : IDisposable
    {
        private readonly ImmutableArray<DatProperty> _oldArray;
        private readonly DatTypeWithProperties _type;
        private readonly SpecPropertyContext _context;

        public UndoAddProperty(ImmutableArray<DatProperty> oldArray, DatTypeWithProperties type, SpecPropertyContext context)
        {
            _oldArray = oldArray;
            _type = type;
            _context = context;
        }

        public void Dispose()
        {
            switch (_context)
            {
                case SpecPropertyContext.Property:
                case SpecPropertyContext.CrossReferenceProperty:
                    _type.Properties = _oldArray;
                    break;
                case SpecPropertyContext.Localization:
                case SpecPropertyContext.CrossReferenceLocalization:
                    switch (_type)
                    {
                        case DatAssetFileType af:
                            af.LocalizationProperties = _oldArray;
                            break;
                        case DatCustomAssetType at:
                            at.LocalizationProperties = _oldArray;
                            break;
                    }
                    break;
            }
        }
    }
}
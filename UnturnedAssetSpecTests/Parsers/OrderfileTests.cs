using System;
using System.Collections.Generic;
using System.Text;
using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using Microsoft.Extensions.Logging;

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
internal class OrderfileTests
{
    private ParsingServiceProvider _parsingServices;

    [SetUp]
    public async Task SetUp()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(l => l.AddConsole());

        AssetSpecDatabase database = AssetSpecDatabase.FromOffline(loggerFactory: loggerFactory);

        database.ReadOrderfile = false;

        InstallationEnvironment env = new InstallationEnvironment(database);

        _parsingServices = new ParsingServiceProvider(
            database,
            loggerFactory,
            new StaticSourceFileWorkspaceEnvironment(useCache: true, database, installationEnvironment: env),
            database.UnturnedInstallDirectory,
            env,
            new NilProjectFileProvider(database)
        );

        await database.InitializeAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _parsingServices.Dispose();
    }

    [Test]
    public void TestBasicOrderfileDefinition()
    {
        QualifiedType type = new QualifiedType("SDG.Unturned.MasterBundleConfig, Assembly-CSharp", true);

        DatTypeWithProperties typeDef = _parsingServices.Database.FileTypes[type];

        const string fileContents =
            """
            "SDG.Unturned.MasterBundleConfig, Assembly-CSharp"
            [
                Asset_Bundle_Name
                Asset_Prefix 
                Master_Bundle_Version
            ]
            """;

        using StaticSourceFile sf = StaticSourceFile.FromOtherFile(
            SpecificationFileReader.GlobalOrderfileName,
            fileContents,
            _parsingServices.Database,
            SourceNodeTokenizerOptions.Metadata
        );

        Assert.That(PropertyOrderFile.TryReadFromFile(
            _parsingServices.Database,
            sf.SourceFile,
            out PropertyOrderFile? orderfile
        ));

        OrderedPropertyReference[]? prefs = orderfile?.GetOrderForType(type, SpecPropertyContext.Property);

        Assert.That(prefs, Is.Not.Null);
        Assert.That(prefs, Is.Not.Empty);

        Assert.That(prefs.Select(x => x.GetString(typeDef, false)), Is.EquivalentTo(
        [
            "Asset_Bundle_Name",
            "Asset_Prefix",
            "Master_Bundle_Version"
        ]));
    }

    [Test]
    public void TestDerivedOrderfileDefinitionAfter()
    {
        QualifiedType type = new QualifiedType("SDG.Unturned.ItemAsset, Assembly-CSharp", true);

        DatTypeWithProperties typeDef = _parsingServices.Database.FileTypes[type];

        const string fileContents =
            """
            "SDG.Unturned.Asset, Assembly-CSharp"
            [
                GUID
                Type
                ID
            ]
            "SDG.Unturned.ItemAsset, Assembly-CSharp"
            [
                @Type
                Rarity
                Useable
            ]
            """;

        using StaticSourceFile sf = StaticSourceFile.FromOtherFile(
            SpecificationFileReader.GlobalOrderfileName,
            fileContents,
            _parsingServices.Database,
            SourceNodeTokenizerOptions.Metadata
        );

        Assert.That(PropertyOrderFile.TryReadFromFile(
            _parsingServices.Database,
            sf.SourceFile,
            out PropertyOrderFile? orderfile
        ));

        OrderedPropertyReference[]? prefs = orderfile?.GetOrderForType(type, SpecPropertyContext.Property);

        Assert.That(prefs, Is.Not.Null);
        Assert.That(prefs, Is.Not.Empty);

        Assert.That(prefs.Select(x => x.GetString(typeDef, false)), Is.EquivalentTo(
        [
            "@GUID",
            "@Type",
            "Rarity",
            "Useable",
            "@ID"
        ]));
    }

    [Test]
    public void TestDerivedOrderfileDefinitionBefore()
    {
        QualifiedType type = new QualifiedType("SDG.Unturned.ItemAsset, Assembly-CSharp", true);

        DatTypeWithProperties typeDef = _parsingServices.Database.FileTypes[type];

        const string fileContents =
            """
            "SDG.Unturned.Asset, Assembly-CSharp"
            [
                GUID
                Type
                ID
                Exclude_From_Master_Bundle
            ]
            "SDG.Unturned.ItemAsset, Assembly-CSharp"
            [
                Rarity
                Useable
                @ID
                
                @Exclude_From_Master_Bundle
                Instantiated_Item_Name_Override
            ]
            """;

        using StaticSourceFile sf = StaticSourceFile.FromOtherFile(
            SpecificationFileReader.GlobalOrderfileName,
            fileContents,
            _parsingServices.Database,
            SourceNodeTokenizerOptions.Metadata
        );

        Assert.That(PropertyOrderFile.TryReadFromFile(
            _parsingServices.Database,
            sf.SourceFile,
            out PropertyOrderFile? orderfile
        ));

        OrderedPropertyReference[]? prefs = orderfile?.GetOrderForType(type, SpecPropertyContext.Property);

        Assert.That(prefs, Is.Not.Null);
        Assert.That(prefs, Is.Not.Empty);

        Assert.That(prefs.Select(x => x.GetString(typeDef, false)), Is.EquivalentTo(
        [
            "@GUID",
            "@Type",
            "Rarity",
            "Useable",
            "@ID",
            "@Exclude_From_Master_Bundle",
            "Instantiated_Item_Name_Override"
        ]));
    }

    [Test]
    public void TestDerivedOrderfileDefinitionMiddle()
    {
        QualifiedType type = new QualifiedType("SDG.Unturned.ItemAsset, Assembly-CSharp", true);

        DatTypeWithProperties typeDef = _parsingServices.Database.FileTypes[type];

        const string fileContents =
            """
            "SDG.Unturned.Asset, Assembly-CSharp"
            [
                GUID
                Type
                ID
                Exclude_From_Master_Bundle
            ]
            "SDG.Unturned.ItemAsset, Assembly-CSharp"
            [
                Rarity
                @ID
                Useable
            ]
            """;

        using StaticSourceFile sf = StaticSourceFile.FromOtherFile(
            SpecificationFileReader.GlobalOrderfileName,
            fileContents,
            _parsingServices.Database,
            SourceNodeTokenizerOptions.Metadata
        );

        Assert.That(PropertyOrderFile.TryReadFromFile(
            _parsingServices.Database,
            sf.SourceFile,
            out PropertyOrderFile? orderfile
        ));

        OrderedPropertyReference[]? prefs = orderfile?.GetOrderForType(type, SpecPropertyContext.Property);

        Assert.That(prefs, Is.Not.Null);
        Assert.That(prefs, Is.Not.Empty);

        Assert.That(prefs.Select(x => x.GetString(typeDef, false)), Is.EquivalentTo(
        [
            "@GUID",
            "@Type",
            "Rarity",
            "@ID",
            "Useable",
            "@Exclude_From_Master_Bundle"
        ]));
    }
}

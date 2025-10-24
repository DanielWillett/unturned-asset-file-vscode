#if NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Files;
using Microsoft.Extensions.Logging.Abstractions;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace UnturnedAssetSpecTests.FileProperties;

[TestFixture]
public class PropertyVirtualizerSandbox
{
    private AssetSpecDatabase _database;
    private OpenedFileTracker _fileTracker;
    private LspWorkspaceEnvironment _env;
    private InstallationEnvironment _install;
    private SourceFilePropertyVirtualizer _virtualizer;
    [SetUp]
    public async Task Setup()
    {
        _database = AssetSpecDatabase.FromOffline();
        await _database.InitializeAsync();

        _fileTracker = new OpenedFileTracker(
            NullLogger<OpenedFileTracker>.Instance,
            _database
        );

        _env = new LspWorkspaceEnvironment(
            _fileTracker,
            NullLogger<LspWorkspaceEnvironment>.Instance,
            _database,
            null,
            null
        );

        _install = new InstallationEnvironment(_database);

        _virtualizer = new SourceFilePropertyVirtualizer(_database, _env, _install);
    }

    [TearDown]
    public void TearDown()
    {
        _fileTracker.Dispose();
        _install.Dispose();
        _database.Dispose();
        _env.Dispose();
    }

    [Test]
    public async Task BasicProperty()
    {
        string file = Path.GetFullPath("../../../Assets/TestGun.dat");

        OpenedFile openedFile = _fileTracker.CreateFile(DocumentUri.File(file), File.ReadAllText(file));

        ISourceFile sourceFile = openedFile.SourceFile;
        QualifiedType actualType = sourceFile.ActualType;
        
        Assert.That(actualType, Is.EqualTo(new QualifiedType("SDG.Unturned.ItemGunAsset, Assembly-CSharp", false)));

        AssetFileType fileType = AssetFileType.FromFile(sourceFile, _database);
        
        Assert.That(fileType.Type, Is.EqualTo(actualType));
        AssetSpecType info = fileType.Information;

        SpecProperty? slotProperty = info.FindProperty("Slot", SpecPropertyContext.Property);
        Assert.That(slotProperty, Is.Not.Null);

        IFileProperty? definedProperty = _virtualizer.FindProperty(sourceFile, slotProperty);

        Assert.That(definedProperty, Is.Not.Null);
        Assert.That(definedProperty.TryGetValue(out ISpecDynamicValue? v), Is.True);
        Assert.That(v, Is.Not.Null);
        if (v.ValueType is IStringParseableSpecPropertyType str)
        {
            Assert.That(str.ToString(v), Is.EqualTo("PRIMARY"));
        }
    }

    [Test]
    public async Task LegacyProperty()
    {
        string file = Path.GetFullPath("../../../Assets/TestDialogue.dat");

        OpenedFile openedFile = _fileTracker.CreateFile(DocumentUri.File(file), File.ReadAllText(file));

        ISourceFile sourceFile = openedFile.SourceFile;
        QualifiedType actualType = sourceFile.ActualType;
        
        Assert.That(actualType, Is.EqualTo(new QualifiedType("SDG.Unturned.DialogueAsset, Assembly-CSharp", false)));

        AssetFileType fileType = AssetFileType.FromFile(sourceFile, _database);
        
        Assert.That(fileType.Type, Is.EqualTo(actualType));
        AssetSpecType info = fileType.Information;

        SpecProperty? messageProperties = info.FindProperty("Message_#", SpecPropertyContext.Property);
        Assert.That(messageProperties, Is.Not.Null);

        // IFileProperty? definedProperty = _virtualizer.FindProperty(
        //     sourceFile,
        //     messageProperties,
        //     new PropertyBreadcrumbs(new PropertyBreadcrumbSection(messageProperties, PropertyResolutionContext.Legacy, 1))
        // );
        // 
        // Assert.That(definedProperty, Is.Not.Null);
        // Assert.That(definedProperty.TryGetValue(out ISpecDynamicValue? v), Is.True);
        // Assert.That(v, Is.Not.Null);
        // if (v.ValueType is IStringParseableSpecPropertyType str)
        // {
        //     Assert.That(str.ToString(v), Is.EqualTo("PRIMARY"));
        // }
    }
}
#endif
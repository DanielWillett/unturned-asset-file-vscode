using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using Microsoft.Extensions.Logging;

namespace UnturnedAssetSpecTests.Bundles;

[TestFixture]
//[Ignore("Slow")]
public class UnityObjectTests
{
    private IParsingServices _parsingServices;

    [SetUp]
    public async Task SetUp()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(l => l.AddSimpleConsole());

        IAssetSpecDatabase database = AssetSpecDatabase.FromOffline(
            useInstallDir: true,
            loggerFactory: loggerFactory,
            cache: new TestCache()
        );

        _parsingServices = new ParsingServiceProvider(
            database,
            loggerFactory,
            new StaticSourceFileWorkspaceEnvironment(false, new Lazy<IParsingServices>(() => _parsingServices)),
            database.UnturnedInstallDirectory,
            new InstallationEnvironment(database, loggerFactory),
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
    public void TestReadUnityObjectFromBundle()
    {
        string directory = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "../../../Assets"));

        _parsingServices.Installation.AddSearchableDirectory(directory);
        _parsingServices.Installation.Discover();

        DatBundleAsset bundleAsset = DatBundleAsset.Create(
            "TPV_Char_Server",
            UnityObjectAssetType.Create("UnityEngine.GameObject, UnityEngine.CoreModule"),
            new DatFileType(QualifiedType.AssetBaseType, null, default),
            default
        );

        string testFile = Path.Combine(directory, "Resources", "Asset.dat");
        DiscoveredDatFile file = _parsingServices.Installation.FindFile(new Guid("260d3c7718664fd09a600e18ca3a4255")).First();

        IBundleProxy bundleProxy = file.GetBundleProxy(_parsingServices);
        Assert.That(bundleProxy, Is.Not.Null);
        Assert.That(bundleProxy, Is.Not.InstanceOf<NullBundleProxy>());

        using StaticSourceFile srcFile = StaticSourceFile.FromAssetFile(testFile, _parsingServices.Database, bundle: bundleProxy);

        FileEvaluationContext context = new FileEvaluationContext(_parsingServices, srcFile.SourceFile);

        UnityObject? unityObject = bundleProxy.GetCorrespondingAsset(bundleAsset, ref context);

        Assert.That(unityObject, Is.Not.Null);

        UnityTransform? transform = unityObject.Transform;

        Assert.That(transform, Is.Not.Null);

        foreach (UnityTransform child in transform)
        {
            Console.WriteLine(child);
        }
    }

    [Test]
    public void TestReadUnityObjectFromCoreBundle()
    {
        if (!_parsingServices.GameDirectory.TryGetInstallDirectory(out GameInstallDir gameDir))
        {
            Assert.Inconclusive();
        }

        _parsingServices.Installation.AddSearchableDirectory(gameDir.GetFile("Bundles"));
        _parsingServices.Installation.Discover();

        DiscoveredDatFile file = _parsingServices.Installation.FindFile(
            new Guid("011d1369cd56497488827b44509b0b4b")
        ).First();

        IBundleProxy bundleProxy = file.GetBundleProxy(_parsingServices);
        Assert.That(bundleProxy, Is.Not.Null);
        Assert.That(bundleProxy, Is.Not.InstanceOf<NullBundleProxy>());

        using StaticSourceFile srcFile = StaticSourceFile.FromAssetFile(file.FilePath, _parsingServices.Database, bundle: bundleProxy);

        FileEvaluationContext context = new FileEvaluationContext(_parsingServices, srcFile.SourceFile);

        IBundleAssetType gameObjectType = UnityObjectAssetType.Create("UnityEngine.GameObject, UnityEngine.CoreModule");

        UnityObject? unityObject = bundleProxy.GetCorrespondingAsset("Resource", gameObjectType, ref context);
        Assert.That(unityObject, Is.Not.Null);

        UnityTransform? transform = unityObject.Transform;
        Assert.That(transform, Is.Not.Null);

        UnityTransform? model0 = transform.Find("Model_0");
        Assert.That(model0, Is.Not.Null);

        UnityTransform? foliage0 = transform.Find("Model_0/Foliage_0");
        Assert.That(foliage0, Is.Not.Null);
    }

    [Test]
    //[Ignore("Requires files on tester PC.")]
    public void TestComponentTest()
    {
        if (!_parsingServices.GameDirectory.TryGetInstallDirectory(out GameInstallDir gameDir))
        {
            Assert.Inconclusive();
        }

        _parsingServices.Installation.AddSearchableDirectory(
            @"C:\Program Files (x86)\Steam\steamapps\workshop\content\304930\2839462324\UncreatedUI"
        );
        _parsingServices.Installation.Discover();

        DiscoveredDatFile file = _parsingServices.Installation.FindFile(
            new Guid("f298af0b4d34405b98a539b8d2ff0505")
        ).First();

        IBundleProxy bundleProxy = file.GetBundleProxy(_parsingServices);
        Assert.That(bundleProxy, Is.Not.Null);
        Assert.That(bundleProxy, Is.Not.InstanceOf<NullBundleProxy>());

        using StaticSourceFile srcFile = StaticSourceFile.FromAssetFile(file.FilePath, _parsingServices.Database, bundle: bundleProxy);

        FileEvaluationContext context = new FileEvaluationContext(_parsingServices, srcFile.SourceFile);

        IBundleAssetType gameObjectType = UnityObjectAssetType.Create("UnityEngine.GameObject, UnityEngine.CoreModule");

        UnityObject? unityObject = bundleProxy.GetCorrespondingAsset("Effect", gameObjectType, ref context);
        Assert.That(unityObject, Is.Not.Null);

        UnityTransform? transform = unityObject.Transform;
        Assert.That(transform, Is.Not.Null);

        UnityTransform canvas = transform.First();
        Assert.That(canvas.TryGetComponent(AssetClassID.Canvas, out UnityComponent? comp), Is.True);

        Assert.That(comp!.TryReadProperty("m_SortingOrder", out Optional<short> value1), Is.True);
        Assert.That(value1.HasValue, Is.True);
        Assert.That(value1.Value, Is.EqualTo((short)0));

        Assert.That(comp.TryReadProperty("m_GameObject.m_Name", out Optional<string> value2), Is.True);
        Assert.That(value2.HasValue, Is.True);
        Assert.That(value2.Value, Is.EqualTo("Canvas"));
    }
}

internal class TestCache : ISpecDatabaseCache
{
    public string RootDirectory => Path.Combine(Environment.CurrentDirectory, "Cache");

    public Task CacheNewFilesAsync(IAssetSpecDatabase database, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }
}
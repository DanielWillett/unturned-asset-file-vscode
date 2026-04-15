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
            new UnityObjectAssetType(new QualifiedType("UnityEngine.GameObject, UnityEngine.CoreModule")),
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

        UnityObject? unityObject = bundleProxy.GetCorrespondingAsset(bundleAsset.Key, bundleAsset.Type, ref context);

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

        IBundleAssetType gameObjectType = new UnityObjectAssetType(
            new QualifiedType("UnityEngine.GameObject, UnityEngine.CoreModule", true)
        );

        UnityObject? unityObject = bundleProxy.GetCorrespondingAsset("Resource", gameObjectType, ref context);
        Assert.That(unityObject, Is.Not.Null);

        UnityTransform? transform = unityObject.Transform;
        Assert.That(transform, Is.Not.Null);

        UnityTransform? model0 = transform.Find("Model_0");
        Assert.That(model0, Is.Not.Null);

        UnityTransform? foliage0 = transform.Find("Model_0/Foliage_0");
        Assert.That(foliage0, Is.Not.Null);
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
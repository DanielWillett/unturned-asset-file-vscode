using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Diagnostics;

namespace UnturnedAssetSpecTests;

#pragma warning disable VSTHRD103
#pragma warning disable VSTHRD200

[NonParallelizable]
public class InstallationEnvironmentTest
{
#nullable disable
    private AssetSpecDatabase _database;
    private InstallationEnvironment _runner;
#nullable restore

    [Test]
    public async Task TryReadFromGame()
    {
        if (!Directory.Exists(InstallDirTests.ExpectedInstallDir))
        {
            Assert.Inconclusive("Game not installed where it's expected.");
        }

        _database = new AssetSpecDatabase { UseInternet = false };

        await _database.InitializeAsync();

        _runner = new InstallationEnvironment(_database);
        _runner.AddUnturnedSearchableDirectories(_database.UnturnedInstallDirectory.InstallDirectory);

        Stopwatch sw = Stopwatch.StartNew();
        _runner.Discover();
        sw.Stop();

        Console.WriteLine($"Discovered {_runner.FileCount} in {sw.Elapsed:g}.");

        _runner.ForEachFile(file => Console.WriteLine($"File: {file.GetDisplayName(),-40} @ {file.FilePath}."));

        Assert.That(_runner.FileCount > 0);

        OneOrMore<DiscoveredDatFile> aceAsset = _runner.FindFile(new Guid("92b49222958d4c6fbeca1bd00987b0fd"));
        Assert.That(aceAsset.Length, Is.EqualTo(1));
        Assert.That(aceAsset.Value.Type, Is.EqualTo(new QualifiedType("SDG.Unturned.ItemGunAsset, Assembly-CSharp")));
        Assert.That(aceAsset.Value.Id, Is.EqualTo(107));

        OneOrMore<DiscoveredDatFile> aceAssetById = _runner.FindFile(107, AssetCategory.Item);
        Assert.That(aceAssetById, Is.EqualTo(aceAsset));
    }

    [Test]
    public async Task CheckVariousFileDiscoveryScenarios()
    {
        DirectoryInfo homeDir = Directory.CreateDirectory("Environment");

        if (homeDir.Exists)
            homeDir.Delete(true);

        homeDir.Create();
        DirectoryInfo testAsset1 = homeDir.CreateSubdirectory("TestAsset1");
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "TestAsset1.asset")).CreateText())
        {
            writer.WriteLine("ID 1");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID 334dcdd23701482f99cb2ea87e52e826");
        }
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "English.dat")).CreateText())
        {
            writer.WriteLine("Name Test Asset 1");
        }
        DirectoryInfo testAsset2 = homeDir.CreateSubdirectory("TestAsset2");
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset2.FullName, "Asset.dat")).CreateText())
        {
            writer.WriteLine("ID 2");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID c4279d6b6a77415da88d89fff6a2fdad");
        }
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset2.FullName, "English.dat")).CreateText())
        {
            writer.WriteLine("Name Test Asset 2");
        }
        using (StreamWriter writer = new FileInfo(Path.Combine(homeDir.FullName, "TestAsset3.asset")).CreateText())
        {
            writer.WriteLine("Type SDG.Unturned.OutfitAsset, Assembly-CSharp");
            writer.WriteLine("GUID bfa96b32227e4484940e343376f0bcfb");
        }
        DirectoryInfo testAsset4 = homeDir.CreateSubdirectory("TestAsset4");
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset4.FullName, "TestAsset4.dat")).CreateText())
        {
            writer.WriteLine("ID 4");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID 7119d16d648e4ebc9d14e4a1f2eb3429");
        }
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset4.FullName, "English.dat")).CreateText())
        {
            writer.WriteLine("Name Test Asset 4");
        }

        _database = new AssetSpecDatabase { UseInternet = false };

        await _database.InitializeAsync();

        _runner = new InstallationEnvironment(_database, homeDir.FullName);
        _runner.Discover();

        Assert.That(_runner.FileCount, Is.EqualTo(4));

        DiscoveredDatFile testAsset1File = _runner.FindFile(new Guid("334dcdd23701482f99cb2ea87e52e826")).Single();
        DiscoveredDatFile testAsset2File = _runner.FindFile(new Guid("c4279d6b6a77415da88d89fff6a2fdad")).Single();
        DiscoveredDatFile testAsset3File = _runner.FindFile(new Guid("bfa96b32227e4484940e343376f0bcfb")).Single();
        DiscoveredDatFile testAsset4File = _runner.FindFile(new Guid("7119d16d648e4ebc9d14e4a1f2eb3429")).Single();

        Assert.That(testAsset1File.FriendlyName, Is.EqualTo("Test Asset 1"));
        Assert.That(testAsset2File.FriendlyName, Is.EqualTo("Test Asset 2"));
        Assert.That(testAsset3File.FriendlyName, Is.Null);
        Assert.That(testAsset4File.FriendlyName, Is.EqualTo("Test Asset 4"));

        Assert.That(testAsset1File.Id, Is.EqualTo(1));
        Assert.That(testAsset2File.Id, Is.EqualTo(2));
        Assert.That(testAsset3File.Id, Is.EqualTo(0));
        Assert.That(testAsset4File.Id, Is.EqualTo(4));
    }

    private TaskCompletionSource<DiscoveredDatFile>? _waitForUpdate;

    private void RunnerOnOnFileUpdated(DiscoveredDatFile oldfile, DiscoveredDatFile newfile)
    {
        _waitForUpdate?.SetResult(newfile);
    }

    private void RunnerOnOnFileAddedOrRemoved(DiscoveredDatFile file)
    {
        _waitForUpdate?.SetResult(file);
    }


    [Test]
    public async Task TestFileWatcherFileUpdates()
    {
        DirectoryInfo homeDir = Directory.CreateDirectory("Environment");

        if (homeDir.Exists)
            homeDir.Delete(true);

        homeDir.Create();
        DirectoryInfo testAsset1 = homeDir.CreateSubdirectory("TestAsset1");
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "TestAsset1.asset")).CreateText())
        {
            writer.WriteLine("ID 1");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID 334dcdd23701482f99cb2ea87e52e826");
            writer.WriteLine("Size_X 2");
            writer.WriteLine("Size_Y 2");
        }
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "English.dat")).CreateText())
        {
            writer.WriteLine("Name Test Asset 1");
        }

        _database = new AssetSpecDatabase { UseInternet = false };
        await _database.InitializeAsync();

        _runner = new InstallationEnvironment(_database, homeDir.FullName);
        _runner.Discover();

        _runner.OnFileUpdated += RunnerOnOnFileUpdated;

        // -----

        _waitForUpdate = new TaskCompletionSource<DiscoveredDatFile>();

        // random change
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "TestAsset1.asset")).CreateText())
        {
            writer.WriteLine("ID 1");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID 334dcdd23701482f99cb2ea87e52e826");
            writer.WriteLine("Size_X 3");
            writer.WriteLine("Size_Y 3");
        }
        
        await Task.WhenAny(_waitForUpdate.Task, Task.Delay(10000));

        Assert.That(_waitForUpdate.Task.IsCompleted);

        // file still exists
        Assert.That(_runner.FindFile(new Guid("334dcdd23701482f99cb2ea87e52e826")).Single(), Is.EqualTo(_waitForUpdate.Task.Result));

        // -----

        _waitForUpdate = new TaskCompletionSource<DiscoveredDatFile>();

        // GUID change
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "TestAsset1.asset")).CreateText())
        {
            writer.WriteLine("ID 1");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID 86510e2d8de7482ea1a9f42a64064865");
            writer.WriteLine("Size_X 2");
            writer.WriteLine("Size_Y 2");
        }
        
        await Task.WhenAny(_waitForUpdate.Task, Task.Delay(10000));

        Assert.That(_waitForUpdate.Task.IsCompleted);

        // file GUID was changed
        Assert.That(_runner.FindFile(new Guid("86510e2d8de7482ea1a9f42a64064865")).Single(), Is.EqualTo(_waitForUpdate.Task.Result));
        Assert.That(_runner.FindFile(new Guid("334dcdd23701482f99cb2ea87e52e826")), Is.EqualTo(OneOrMore<DiscoveredDatFile>.Null));

        // -----

        _waitForUpdate = new TaskCompletionSource<DiscoveredDatFile>();

        // ID change
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "TestAsset1.asset")).CreateText())
        {
            writer.WriteLine("ID 2");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID 86510e2d8de7482ea1a9f42a64064865");
            writer.WriteLine("Size_X 2");
            writer.WriteLine("Size_Y 2");
        }
        
        await Task.WhenAny(_waitForUpdate.Task, Task.Delay(10000));

        Assert.That(_waitForUpdate.Task.IsCompleted);

        // file ID was changed
        Assert.That(_runner.FindFile(2, AssetCategory.Item).Single(), Is.EqualTo(_waitForUpdate.Task.Result));
        Assert.That(_runner.FindFile(1, AssetCategory.Item), Is.EqualTo(OneOrMore<DiscoveredDatFile>.Null));

        // -----

        _waitForUpdate = new TaskCompletionSource<DiscoveredDatFile>();

        // English.dat change
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "English.dat")).CreateText())
        {
            writer.WriteLine("Name Test Asset 1 (Changed)");
        }

        await Task.WhenAny(_waitForUpdate.Task, Task.Delay(10000));

        Assert.That(_waitForUpdate.Task.IsCompleted);

        // file ID was changed
        Assert.That(_waitForUpdate.Task.Result.FriendlyName, Is.EqualTo("Test Asset 1 (Changed)"));
        Assert.That(_runner.FindFile(new Guid("86510e2d8de7482ea1a9f42a64064865")).Single(), Is.EqualTo(_waitForUpdate.Task.Result));
        Assert.That(_runner.FindFile(2, AssetCategory.Item).Single(), Is.EqualTo(_waitForUpdate.Task.Result));

        _runner.OnFileUpdated -= RunnerOnOnFileUpdated;
    }

    [Test]
    public async Task TestFileWatcherRename()
    {
        DirectoryInfo homeDir = Directory.CreateDirectory("Environment");

        if (homeDir.Exists)
            homeDir.Delete(true);

        homeDir.Create();
        DirectoryInfo testAsset1 = homeDir.CreateSubdirectory("TestAsset1");
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "TestAsset1.asset")).CreateText())
        {
            writer.WriteLine("ID 1");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID 334dcdd23701482f99cb2ea87e52e826");
        }
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "English.dat")).CreateText())
        {
            writer.WriteLine("Name Test Asset 1");
        }

        _database = new AssetSpecDatabase { UseInternet = false };
        await _database.InitializeAsync();

        _runner = new InstallationEnvironment(_database, homeDir.FullName);
        _runner.Discover();

        Assert.That(_runner.FileCount, Is.EqualTo(1));
        _runner.OnFileUpdated += RunnerOnOnFileUpdated;

        // -----

        _waitForUpdate = new TaskCompletionSource<DiscoveredDatFile>();

        DiscoveredDatFile file = _runner.FindFile(new Guid("334dcdd23701482f99cb2ea87e52e826")).Single();

        // rename file
        File.Move(file.FilePath, Path.Combine(testAsset1.FullName, "Asset.dat"));

        await Task.WhenAny(_waitForUpdate.Task, Task.Delay(10000));

        Assert.That(_waitForUpdate.Task.IsCompleted);

        Assert.That(file.IsRemoved);

        // file still exists
        Assert.That(_runner.FindFile(new Guid("334dcdd23701482f99cb2ea87e52e826")).Single(), Is.EqualTo(_waitForUpdate.Task.Result));
        Assert.That(_waitForUpdate.Task.Result.FilePath, Is.EqualTo(Path.Combine(testAsset1.FullName, "Asset.dat")));
        Assert.That(_runner.FileCount, Is.EqualTo(1));


        // -----

        _waitForUpdate = new TaskCompletionSource<DiscoveredDatFile>();

        file = _runner.FindFile(new Guid("334dcdd23701482f99cb2ea87e52e826")).Single();

        // rename directory
        string newPath = Path.Combine(testAsset1.Parent!.FullName, "NewFolderName");
        testAsset1.MoveTo(newPath);

        await Task.WhenAny(_waitForUpdate.Task, Task.Delay(10000));

        Assert.That(_waitForUpdate.Task.IsCompleted);

        Assert.That(file.IsRemoved);

        // file still exists
        Assert.That(_runner.FindFile(new Guid("334dcdd23701482f99cb2ea87e52e826")).Single(), Is.EqualTo(_waitForUpdate.Task.Result));
        Assert.That(_waitForUpdate.Task.Result.FilePath, Is.EqualTo(Path.Combine(newPath, "Asset.dat")));
        Assert.That(_waitForUpdate.Task.Result.Name.ToString(), Is.EqualTo("NewFolderName"));
        Assert.That(_runner.FileCount, Is.EqualTo(1));

        _runner.OnFileUpdated -= RunnerOnOnFileUpdated;
    }

    [Test]
    public async Task TestFileWatcherRemove()
    {
        DirectoryInfo homeDir = Directory.CreateDirectory("Environment");

        if (homeDir.Exists)
            homeDir.Delete(true);

        homeDir.Create();
        DirectoryInfo testAsset1 = homeDir.CreateSubdirectory("TestAsset1");
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "TestAsset1.asset")).CreateText())
        {
            writer.WriteLine("ID 1");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID 334dcdd23701482f99cb2ea87e52e826");
        }
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "English.dat")).CreateText())
        {
            writer.WriteLine("Name Test Asset 1");
        }

        _database = new AssetSpecDatabase { UseInternet = false };
        await _database.InitializeAsync();

        _runner = new InstallationEnvironment(_database, homeDir.FullName);
        _runner.Discover();

        Assert.That(_runner.FileCount, Is.EqualTo(1));
        _runner.OnFileRemoved += RunnerOnOnFileAddedOrRemoved;

        // -----

        _waitForUpdate = new TaskCompletionSource<DiscoveredDatFile>();

        DiscoveredDatFile file = _runner.FindFile(new Guid("334dcdd23701482f99cb2ea87e52e826")).Single();

        // rename file
        File.Delete(file.FilePath);

        await Task.WhenAny(_waitForUpdate.Task, Task.Delay(10000));

        Assert.That(_waitForUpdate.Task.IsCompleted);

        Assert.That(_runner.FileCount, Is.EqualTo(0));
        Assert.That(file.IsRemoved);

        // file still exists
        Assert.That(_runner.FindFile(new Guid("334dcdd23701482f99cb2ea87e52e826")), Is.EqualTo(OneOrMore<DiscoveredDatFile>.Null));

        _runner.OnFileRemoved -= RunnerOnOnFileAddedOrRemoved;
    }

    [Test]
    public async Task TestFileWatcherCreate()
    {
        DirectoryInfo homeDir = Directory.CreateDirectory("Environment");

        if (homeDir.Exists)
            homeDir.Delete(true);

        homeDir.Create();
        DirectoryInfo testAsset1 = homeDir.CreateSubdirectory("TestAsset1");
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "TestAsset1.asset")).CreateText())
        {
            writer.WriteLine("ID 1");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID 334dcdd23701482f99cb2ea87e52e826");
        }
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset1.FullName, "English.dat")).CreateText())
        {
            writer.WriteLine("Name Test Asset 1");
        }

        _database = new AssetSpecDatabase { UseInternet = false };
        await _database.InitializeAsync();

        _runner = new InstallationEnvironment(_database, homeDir.FullName);
        _runner.Discover();

        Assert.That(_runner.FileCount, Is.EqualTo(1));
        _runner.OnFileAdded += RunnerOnOnFileAddedOrRemoved;

        // -----

        _waitForUpdate = new TaskCompletionSource<DiscoveredDatFile>();

        // create file
        DirectoryInfo testAsset2 = homeDir.CreateSubdirectory("TestAsset2");
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset2.FullName, "TestAsset2.asset")).CreateText())
        {
            writer.WriteLine("ID 2");
            writer.WriteLine("Type Supply");
            writer.WriteLine("GUID 5fce3a69054b4001bb4d46bb19d239d8");
        }
        using (StreamWriter writer = new FileInfo(Path.Combine(testAsset2.FullName, "English.dat")).CreateText())
        {
            writer.WriteLine("Name Test Asset 2");
        }

        await Task.WhenAny(_waitForUpdate.Task, Task.Delay(10000));

        Assert.That(_runner.FileCount, Is.EqualTo(2));
        Assert.That(_waitForUpdate.Task.IsCompleted);

        // file still exists
        Assert.That(_runner.FindFile(new Guid("5fce3a69054b4001bb4d46bb19d239d8")), Is.EqualTo(_waitForUpdate.Task.Result));
        Assert.That(_waitForUpdate.Task.Result.AssetName, Is.EqualTo("TestAsset2"));

        _runner.OnFileAdded -= RunnerOnOnFileAddedOrRemoved;
    }

    [TearDown]
    public void TearDown()
    {
        _database?.Dispose();
        _runner?.Dispose();
    }
}
#pragma warning restore VSTHRD200
#pragma warning restore VSTHRD103
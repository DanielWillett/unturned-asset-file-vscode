using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace UnturnedAssetSpecTests;

#pragma warning disable VSTHRD200

public class DiscoveredDatFileParseCaliberTests
{
#nullable disable
    private AssetSpecDatabase _db;
#nullable restore

    [OneTimeSetUp]
    public async Task Setup()
    {
        _db = AssetSpecDatabase.FromOffline();
        await _db.InitializeAsync();
    }

    [Test]
    public void ReadAttachmentCalibersCountBeforeTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Magazine

            Calibers 4
            Caliber_0 1
            Caliber_1 2
            Caliber_2 3
            Caliber_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadAttachmentCaliberCountBeforeTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Magazine

            Calibers 1
            Caliber_0 1
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadAttachmentCalibersCountMiddleTopLevel1()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Magazine

            Caliber_0 1
            Calibers 4
            Caliber_1 2
            Caliber_2 3
            Caliber_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadAttachmentCalibersCountMiddleTopLevel2()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Magazine

            Caliber_0 1
            Caliber_1 2
            Calibers 4
            Caliber_2 3
            Caliber_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadAttachmentCalibersCountMiddleTopLevel3()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Magazine

            Caliber_0 1
            Caliber_1 2
            Caliber_2 3
            Calibers 4
            Caliber_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadAttachmentCalibersCountAfterTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Magazine

            Caliber_0 1
            Caliber_1 2
            Caliber_2 3
            Caliber_3 4
            Calibers 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadAttachmentCaliberCountAfterTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Magazine

            Caliber_0 1
            Calibers 1
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadAttachmentNoCalibersZeroTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Magazine

            Calibers 0
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(0));
    }

    [Test]
    public void ReadAttachmentNoCalibersTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Magazine
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(0));
    }

    [Test]
    public void ReadAttachmentNoCalibersExtraTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Calibers 0
                Caliber_0 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(0));
    }

    [Test]
    public void ReadAttachmentCalibersCountBeforeAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Calibers 4
                Caliber_0 1
                Caliber_1 2
                Caliber_2 3
                Caliber_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadAttachmentCaliberCountBeforeAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Calibers 1
                Caliber_0 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadAttachmentCalibersCountMiddleAssetLevel1()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Caliber_0 1
                Calibers 4
                Caliber_1 2
                Caliber_2 3
                Caliber_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadAttachmentCalibersCountMiddleAssetLevel2()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Caliber_0 1
                Caliber_1 2
                Calibers 4
                Caliber_2 3
                Caliber_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadAttachmentCalibersCountMiddleAssetLevel3()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Caliber_0 1
                Caliber_1 2
                Caliber_2 3
                Calibers 4
                Caliber_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadAttachmentCalibersCountAfterAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Caliber_0 1
                Caliber_1 2
                Caliber_2 3
                Caliber_3 4
                Calibers 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadAttachmentCaliberCountAfterAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Caliber_0 1
                Calibers 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadAttachmentNoCalibersZeroAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Calibers 0
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(0));
    }

    [Test]
    public void ReadAttachmentNoCalibersAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(0));
    }

    [Test]
    public void ReadAttachmentNoCalibersExtraAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemCaliberAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
            
                Calibers 0
                Caliber_0 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(0));
    }



    [Test]
    public void ReadGunCalibersCountBeforeTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun

            Magazine_Calibers 4
            Magazine_Caliber_0 1
            Magazine_Caliber_1 2
            Magazine_Caliber_2 3
            Magazine_Caliber_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.MagazineCalibers, Has.Length.EqualTo(4));
        Assert.That(dat.MagazineCalibers[0], Is.EqualTo(1));
        Assert.That(dat.MagazineCalibers[1], Is.EqualTo(2));
        Assert.That(dat.MagazineCalibers[2], Is.EqualTo(3));
        Assert.That(dat.MagazineCalibers[3], Is.EqualTo(4));
        Assert.That(dat.Calibers, Is.EquivalentTo(dat.MagazineCalibers));
    }

    [Test]
    public void ReadGunCalibersCountBeforeTopLevelWithAttachmentCalibers()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun

            Magazine_Calibers 4
            Magazine_Caliber_0 1
            Magazine_Caliber_1 2
            Magazine_Caliber_2 3
            Magazine_Caliber_3 4
            
            Attachment_Calibers 3
            Attachment_Caliber_0 5
            Attachment_Caliber_1 6
            Attachment_Caliber_2 7
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.MagazineCalibers, Has.Length.EqualTo(4));
        Assert.That(dat.MagazineCalibers[0], Is.EqualTo(1));
        Assert.That(dat.MagazineCalibers[1], Is.EqualTo(2));
        Assert.That(dat.MagazineCalibers[2], Is.EqualTo(3));
        Assert.That(dat.MagazineCalibers[3], Is.EqualTo(4));

        Assert.That(dat.Calibers, Has.Length.EqualTo(3));
        Assert.That(dat.Calibers[0], Is.EqualTo(5));
        Assert.That(dat.Calibers[1], Is.EqualTo(6));
        Assert.That(dat.Calibers[2], Is.EqualTo(7));
    }

    [Test]
    public void ReadGunCaliberCountBeforeTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun

            Magazine_Calibers 1
            Magazine_Caliber_0 1
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadGunCalibersCountMiddleTopLevel1()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun

            Magazine_Caliber_0 1
            Magazine_Calibers 4
            Magazine_Caliber_1 2
            Magazine_Caliber_2 3
            Magazine_Caliber_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadGunCalibersCountMiddleTopLevel2()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun

            Magazine_Caliber_0 1
            Magazine_Caliber_1 2
            Magazine_Calibers 4
            Magazine_Caliber_2 3
            Magazine_Caliber_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadGunCalibersCountMiddleTopLevel3()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun

            Magazine_Caliber_0 1
            Magazine_Caliber_1 2
            Magazine_Caliber_2 3
            Magazine_Calibers 4
            Magazine_Caliber_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadGunCalibersCountAfterTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun

            Magazine_Caliber_0 1
            Magazine_Caliber_1 2
            Magazine_Caliber_2 3
            Magazine_Caliber_3 4
            Magazine_Calibers 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadGunCaliberCountAfterTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun

            Magazine_Caliber_0 1
            Magazine_Calibers 1
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadGunNoCalibersZeroTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun

            Magazine_Calibers 0
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.Zero);
    }

    [Test]
    public void ReadGunNoCalibersTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.Zero);
    }

    [Test]
    public void ReadGunNoCalibersExtraTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Magazine_Calibers 0
                Magazine_Caliber_0 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.Zero);
    }

    [Test]
    public void ReadGunCalibersCountBeforeAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Magazine_Calibers 4
                Magazine_Caliber_0 1
                Magazine_Caliber_1 2
                Magazine_Caliber_2 3
                Magazine_Caliber_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadGunCaliberCountBeforeAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Magazine_Calibers 1
                Magazine_Caliber_0 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadGunCalibersCountMiddleAssetLevel1()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Magazine_Caliber_0 1
                Magazine_Calibers 4
                Magazine_Caliber_1 2
                Magazine_Caliber_2 3
                Magazine_Caliber_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadGunCalibersCountMiddleAssetLevel2()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Magazine_Caliber_0 1
                Magazine_Caliber_1 2
                Magazine_Calibers 4
                Magazine_Caliber_2 3
                Magazine_Caliber_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadGunCalibersCountMiddleAssetLevel3()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Magazine_Caliber_0 1
                Magazine_Caliber_1 2
                Magazine_Caliber_2 3
                Magazine_Calibers 4
                Magazine_Caliber_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadGunCalibersCountAfterAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Magazine_Caliber_0 1
                Magazine_Caliber_1 2
                Magazine_Caliber_2 3
                Magazine_Caliber_3 4
                Magazine_Calibers 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(4));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
        Assert.That(dat.Calibers[2], Is.EqualTo(3));
        Assert.That(dat.Calibers[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadGunCaliberCountAfterAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Magazine_Caliber_0 1
                Magazine_Calibers 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadGunNoCalibersZeroAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                
                Magazine_Calibers 0
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.Zero);
    }

    [Test]
    public void ReadGunNoCalibersAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.Zero);
    }

    [Test]
    public void ReadGunNoCalibersExtraAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemGunAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
            
                Magazine_Calibers 0
                Magazine_Caliber_0 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.Zero);
    }

    [Test]
    public void ReadGunLegacyCaliber()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun
            
            Caliber 8
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.EqualTo(8));
    }

    [Test]
    public void ReadGunLegacyCaliberAndNew()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun
            
            Caliber 8
            Magazine_Calibers 2
            Magazine_Caliber_0 1
            Magazine_Caliber_1 2
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(2));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
    }

    [Test]
    public void ReadGunLegacyCaliberAndNewSwap()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun
            
            Magazine_Calibers 2
            Magazine_Caliber_0 1
            Magazine_Caliber_1 2
            Caliber 8
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(2));
        Assert.That(dat.Calibers[0], Is.EqualTo(1));
        Assert.That(dat.Calibers[1], Is.EqualTo(2));
    }

    [Test]
    public void ReadGunLegacyCaliberAndNewSwapZero()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Gun
            
            Magazine_Calibers 0
            Caliber 8
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.Calibers, Has.Length.EqualTo(1));
        Assert.That(dat.Calibers[0], Is.EqualTo(8));
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _db?.Dispose();
    }
}

#pragma warning restore VSTHRD200
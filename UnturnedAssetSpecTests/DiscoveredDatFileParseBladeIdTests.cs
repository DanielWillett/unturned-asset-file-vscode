using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace UnturnedAssetSpecTests;

#pragma warning disable VSTHRD200

public class DiscoveredDatFileParseBladeIdTests
{
#nullable disable
    private AssetSpecDatabase _db;
#nullable restore

    [OneTimeSetUp]
    public async Task Setup()
    {
        _db = new AssetSpecDatabase { UseInternet = false };
        await _db.InitializeAsync();
    }

    [Test]
    public void ReadWeaponBladeIdsAboveTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee

            BladeIDs 4
            BladeID_0 1
            BladeID_1 2
            BladeID_2 3
            BladeID_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(4));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
        Assert.That(dat.BladeIds[2], Is.EqualTo(3));
        Assert.That(dat.BladeIds[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadWeaponBladeIdsMiddle1TopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee

            BladeID_0 1
            BladeIDs 4
            BladeID_1 2
            BladeID_2 3
            BladeID_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(4));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
        Assert.That(dat.BladeIds[2], Is.EqualTo(3));
        Assert.That(dat.BladeIds[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadWeaponBladeIdsMiddle2TopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee

            BladeID_0 1
            BladeID_1 2
            BladeIDs 4
            BladeID_2 3
            BladeID_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(4));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
        Assert.That(dat.BladeIds[2], Is.EqualTo(3));
        Assert.That(dat.BladeIds[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadWeaponBladeIdsMiddle3TopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee

            BladeID_0 1
            BladeID_1 2
            BladeID_2 3
            BladeIDs 4
            BladeID_3 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(4));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
        Assert.That(dat.BladeIds[2], Is.EqualTo(3));
        Assert.That(dat.BladeIds[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadWeaponBladeIdsBelowTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee

            BladeID_0 1
            BladeID_1 2
            BladeID_2 3
            BladeID_3 4
            BladeIDs 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(4));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
        Assert.That(dat.BladeIds[2], Is.EqualTo(3));
        Assert.That(dat.BladeIds[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadWeaponSingleBladeIdTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee

            BladeID 1
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadWeaponNoBladeIdTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(0));
    }

    [Test]
    public void ReadWeaponBladeIdsTopLevelWithSingleAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee

            BladeID 8
            BladeIDs 2
            BladeID_0 1
            BladeID_1 2
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(2));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
    }

    [Test]
    public void ReadWeaponBladeIdsTopLevelWithSingleBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee

            BladeIDs 2
            BladeID_0 1
            BladeID_1 2
            BladeID 8
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(2));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
    }

    [Test]
    public void ReadWeaponBladeIdsAboveTopLevelWithSingle()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee

            BladeIDs 2
            BladeID 8
            BladeID_0 1
            BladeID_1 2
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(2));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
    }

    [Test]
    public void ReadWeaponBladeIdsBelowTopLevelWithSingle()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee
            
            BladeID 8
            BladeID_0 1
            BladeID_1 2
            BladeIDs 2
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(2));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
    }

    [Test]
    public void ReadWeaponZeroBladeIdsTopLevelWithLegacyAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee
            
            BladeIDs 0
            BladeID 8
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(8));
    }

    [Test]
    public void ReadWeaponZeroBladeIdsTopLevelWithLegacyBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Melee
            
            BladeID 8
            BladeIDs 0
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(8));
    }

    [Test]
    public void ReadWeaponBladeIdsAboveAssetLevell()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeIDs 4
                BladeID_0 1
                BladeID_1 2
                BladeID_2 3
                BladeID_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(4));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
        Assert.That(dat.BladeIds[2], Is.EqualTo(3));
        Assert.That(dat.BladeIds[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadWeaponBladeIdsMiddle1AssetLevell()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeID_0 1
                BladeIDs 4
                BladeID_1 2
                BladeID_2 3
                BladeID_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(4));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
        Assert.That(dat.BladeIds[2], Is.EqualTo(3));
        Assert.That(dat.BladeIds[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadWeaponBladeIdsMiddle2AssetLevell()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeID_0 1
                BladeID_1 2
                BladeIDs 4
                BladeID_2 3
                BladeID_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(4));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
        Assert.That(dat.BladeIds[2], Is.EqualTo(3));
        Assert.That(dat.BladeIds[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadWeaponBladeIdsMiddle3AssetLevell()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeID_0 1
                BladeID_1 2
                BladeID_2 3
                BladeIDs 4
                BladeID_3 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(4));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
        Assert.That(dat.BladeIds[2], Is.EqualTo(3));
        Assert.That(dat.BladeIds[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadWeaponBladeIdsBelowAssetLevell()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                    
                BladeID_0 1
                BladeID_1 2
                BladeID_2 3
                BladeID_3 4
                BladeIDs 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(4));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
        Assert.That(dat.BladeIds[2], Is.EqualTo(3));
        Assert.That(dat.BladeIds[3], Is.EqualTo(4));
    }

    [Test]
    public void ReadWeaponSingleBladeIdAssetLevell()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeID 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadWeaponNoBladeIdAssetLevell()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(0));
    }

    [Test]
    public void ReadWeaponBladeIdsAssetLevellWithSingleAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeID 8
                BladeIDs 2
                BladeID_0 1
                BladeID_1 2
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(2));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
    }

    [Test]
    public void ReadWeaponBladeIdsAssetLevellWithSingleBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeIDs 2
                BladeID_0 1
                BladeID_1 2
                BladeID 8
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(2));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
    }

    [Test]
    public void ReadWeaponBladeIdsAboveAssetLevellWithSingle()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeIDs 2
                BladeID 8
                BladeID_0 1
                BladeID_1 2
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(2));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
    }

    [Test]
    public void ReadWeaponBladeIdsBelowAssetLevellWithSingle()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeID 8
                BladeID_0 1
                BladeID_1 2
                BladeIDs 2
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(2));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
        Assert.That(dat.BladeIds[1], Is.EqualTo(2));
    }

    [Test]
    public void ReadWeaponZeroBladeIdsAssetLevellWithLegacyAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeIDs 0
                BladeID 8
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(8));
    }

    [Test]
    public void ReadWeaponZeroBladeIdsAssetLevellWithLegacyBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeID 8
                BladeIDs 0
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(8));
    }

    [Test]
    public void ReadWeaponNoBladeIdsAssetLevelLegacyOutsideAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            BladeID 8
            Asset
            {
                Type Melee
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(0));
    }

    [Test]
    public void ReadWeaponNoBladeIdsAssetLevelLegacyOutsideBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
            }
            BladeID 8
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(0));
    }

    [Test]
    public void ReadWeaponBladeIdsAssetLevelLegacyOutsideAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            BladeID 8
            Asset
            {
                Type Melee
                
                BladeIDs 1
                BladeID_0 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadWeaponBladeIdsAssetLevelLegacyOutsideBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeIDs 1
                BladeID_0 1
            }
            BladeID 8
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadWeaponBladeIdsAssetLevelNewOutsideAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            BladeIDs 1
            BladeID_0 8
            Asset
            {
                Type Melee
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(0));
    }

    [Test]
    public void ReadWeaponBladeIdsAssetLevelNewOutsideBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
            }
            BladeIDs 1
            BladeID_0 8
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(0));
    }

    [Test]
    public void ReadWeaponBladeIdsLegacyAssetLevelNewOutsideAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            BladeIDs 1
            BladeID_0 8
            Asset
            {
                Type Melee
                
                BladeID 1
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadWeaponBladeIdsLegacyAssetLevelNewOutsideBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ItemWeaponAsset, Assembly-CSharp
            }
            Asset
            {
                Type Melee
                
                BladeID 1
            }
            BladeIDs 1
            BladeID_0 8
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(1));
    }

    [Test]
    public void ReadResourceBladeIdTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Resource

            BladeID 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(4));
    }

    [Test]
    public void ReadResourceBladeIdTopLevelExcluded()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Resource
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(0));
    }

    [Test]
    public void ReadResourceBladeIdAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ResourceAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
                
                BladeID 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(4));
    }

    [Test]
    public void ReadResourceBladeIdAssetLevelExcluded()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ResourceAsset, Assembly-CSharp
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

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(0));
    }

    [Test]
    public void ReadResourceBladeIdAssetLevelExcludedButInOuterScopeBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ResourceAsset, Assembly-CSharp
            }
            Asset
            {
                ID 9999
            }
            BladeID 8
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(0));
    }

    [Test]
    public void ReadResourceBladeIdAssetLevelExcludedButInOuterScopeAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ResourceAsset, Assembly-CSharp
            }
            BladeID 8
            Asset
            {
                ID 9999
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(0));
    }

    [Test]
    public void ReadObjectInteractabilityBladeIdTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Medium

            Interactability Rubble
            Interactability_Blade_ID 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(4));
    }

    [Test]
    public void ReadObjectInteractabilityBladeIdWithRubbleTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Medium

            Interactability Rubble
            Interactability_Blade_ID 4
            
            Rubble Destroy
            Rubble_Blade_ID 6
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(4));
    }

    [Test]
    public void ReadObjectInteractabilityBladeIdWithRubbleTopLevelBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Medium

            Rubble Destroy
            Rubble_Blade_ID 6
            
            Interactability Rubble
            Interactability_Blade_ID 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(4));
    }

    [Test]
    public void ReadObjectRubbleBladeIdTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Medium

            Rubble Destroy
            Rubble_Blade_ID 6
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(6));
    }

    [Test]
    public void ReadObjectRubbleBladeIdWithOtherInteractableTopLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Medium
            
            Interactability Dropper
            Interactability_Reward_ID 2

            Rubble Destroy
            Rubble_Blade_ID 6
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(6));
    }

    [Test]
    public void ReadObjectRubbleBladeIdWithOtherInteractableTopLevelBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Medium
            
            Rubble Destroy
            Rubble_Blade_ID 6
            
            Interactability Dropper
            Interactability_Reward_ID 2
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(6));
    }

    [Test]
    public void ReadObjectRubbleBladeIdOrderSwapped()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Medium
            
            Rubble_Blade_ID 6
            Rubble Destroy
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(6));
    }

    [Test]
    public void ReadObjectInteractableBladeIdOrderSwapped()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            GUID 00000000000000000000000000000001
            ID 9999
            Type Medium
            
            Interactability_Blade_ID 6
            Interactability Rubble
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(6));
    }

    [Test]
    public void ReadObjectInteractabilityBladeIdAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999

                Interactability Rubble
                Interactability_Blade_ID 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(4));
    }

    [Test]
    public void ReadObjectInteractabilityBladeIdWithRubbleAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999
            
                Interactability Rubble
                Interactability_Blade_ID 4
                
                Rubble Destroy
                Rubble_Blade_ID 6
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(4));
    }

    [Test]
    public void ReadObjectInteractabilityBladeIdWithRubbleAssetLevelBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999
            
                Rubble Destroy
                Rubble_Blade_ID 6
                
                Interactability Rubble
                Interactability_Blade_ID 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(4));
    }

    [Test]
    public void ReadObjectRubbleBladeIdAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999
            
                Rubble Destroy
                Rubble_Blade_ID 6
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(6));
    }

    [Test]
    public void ReadObjectRubbleBladeIdWithOtherInteractableAssetLevel()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999
            
                Interactability Dropper
                Interactability_Reward_ID 2

                Rubble Destroy
                Rubble_Blade_ID 6
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(6));
    }

    [Test]
    public void ReadObjectRubbleBladeIdWithOtherInteractableAssetLevelBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999

                Rubble Destroy
                Rubble_Blade_ID 6
                
                Interactability Dropper
                Interactability_Reward_ID 2
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(6));
    }

    [Test]
    public void ReadObjectRubbleBladeIdWithOtherInteractableAssetLevelOutsideAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Rubble Destroy
            Rubble_Blade_ID 6
            Asset
            {
                Type Medium
                ID 9999
            
                Interactability Dropper
                Interactability_Reward_ID 2
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.Zero);
    }

    [Test]
    public void ReadObjectRubbleBladeIdWithOtherInteractableAssetLevelOutsideBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999
            
                Interactability Dropper
                Interactability_Reward_ID 2
            }
            Rubble Destroy
            Rubble_Blade_ID 6
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.Zero);
    }

    [Test]
    public void ReadObjectInteractableBladeIdAssetLevelRubbleOutsideAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Rubble Destroy
            Rubble_Blade_ID 6
            Asset
            {
                Type Medium
                ID 9999
            
                Interactability Rubble
                Interactability_Blade_ID 4
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(4));
    }

    [Test]
    public void ReadObjectInteractableBladeIdAssetLevelRubbleOutsideBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999
            
                Interactability Rubble
                Interactability_Blade_ID 4
            }
            Rubble Destroy
            Rubble_Blade_ID 6
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(4));
    }

    [Test]
    public void ReadObjectRubbleBladeIdAssetLevelInteractabilityOutsideAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Interactability Rubble
            Interactability_Blade_ID 4
            Asset
            {
                Type Medium
                ID 9999
            
                Rubble Destroy
                Rubble_Blade_ID 6
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(6));
    }

    [Test]
    public void ReadObjectRubbleBladeIdAssetLevelInteractabilityOutsideBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999
            
                Rubble Destroy
                Rubble_Blade_ID 6
            }
            Interactability Rubble
            Interactability_Blade_ID 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.EqualTo(1));
        Assert.That(dat.BladeIds[0], Is.EqualTo(6));
    }

    [Test]
    public void ReadObjectNoBladeIdAssetLevelInteractabilityOutsideAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Interactability Rubble
            Interactability_Blade_ID 4
            Asset
            {
                Type Medium
                ID 9999
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.Zero);
    }

    [Test]
    public void ReadObjectNoBladeIdAssetLevelInteractabilityOutsideBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999
            }
            Interactability Rubble
            Interactability_Blade_ID 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.Zero);
    }

    [Test]
    public void ReadObjectNoBladeIdAssetLevelRubbleOutsideAbove()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Rubble Destroy
            Rubble_Blade_ID 4
            Asset
            {
                Type Medium
                ID 9999
            }
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.Zero);
    }

    [Test]
    public void ReadObjectNoBladeIdAssetLevelRubbleOutsideBelow()
    {
        DiscoveredDatFile dat = new DiscoveredDatFile(
            "t.dat",
            """
            Metadata
            {
                GUID 00000000000000000000000000000001
                Type SDG.Unturned.ObjectAsset, Assembly-CSharp
            }
            Asset
            {
                Type Medium
                ID 9999
            }
            Rubble Destroy
            Rubble_Blade_ID 4
            """.AsSpan(),
            _db,
            null,
            static (_, str) => Console.WriteLine(str)
        );

        Assert.That(dat.BladeIds, Has.Length.Zero);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _db?.Dispose();
    }
}

#pragma warning restore VSTHRD200
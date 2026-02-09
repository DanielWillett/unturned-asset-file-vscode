using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests.Nodes;

public class SourceNodeTokenizerFullFileTests
{
    private IAssetSpecDatabase _database;

    [SetUp]
    public async Task Setup()
    {
        _database = AssetSpecDatabase.FromOffline();
        await _database.InitializeAsync();
    }

    [TearDown]
    public void Teardown()
    {
        if (_database is IDisposable d)
            d.Dispose();
    }

    [Test]
    public void File2FailingNegativeRange([Values(SourceNodeTokenizerOptions.Lazy, SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.Lazy | SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.None)] SourceNodeTokenizerOptions options, [Values(true, false)] bool unix)
    {
        string file =
"""
GUID 98ea676858b54de68706a7552c9bc1a6
Type Backpack

Pro

""";

        FixLineEnds(unix, ref file, out int endlLen);

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(file, options);

        ISourceFile sourceFile = tok.ReadRootDictionary(SourceNodeTokenizer.RootInfo.Asset(new TestWorkspaceFile(), _database));

        StringWriter sw = new StringWriter();
        NodeWriteToTextWriterVisitor visitor = new NodeWriteToTextWriterVisitor(sw);

        sourceFile.Visit(ref visitor);

        Console.WriteLine(sw.ToString());

        bool metadata = (options & SourceNodeTokenizerOptions.Metadata) != 0;
        int index = 0;
        int charIndex = 0;

        // GUID 98ea676858b54de68706a7552c9bc1a6
        AssertNode<IValueSourceNode>(
            AssertNode<IPropertySourceNode>(
                sourceFile,
                ref index,
                metadata,
                new FileRange(1, 1, 1, 4),
                4,
                0,
                ref charIndex,
                node =>
                {
                    Assert.That(node.Key, Is.EqualTo("GUID"));
                    Assert.That(node.KeyIsQuoted, Is.False);
                }
            ).Value,
            metadata,
            new FileRange(1, 6, 1, 37),
            32,
            1,
            ref charIndex,
            node =>
            {
                Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                Assert.That(node.Value, Is.EqualTo("98ea676858b54de68706a7552c9bc1a6"));
                Assert.That(node.IsQuoted, Is.False);
            }
        );

        // Type Backpack
        AssertNode<IValueSourceNode>(
            AssertNode<IPropertySourceNode>(
                sourceFile,
                ref index,
                metadata,
                new FileRange(2, 1, 2, 4),
                4,
                endlLen,
                ref charIndex,
                node =>
                {
                    Assert.That(node.Key, Is.EqualTo("Type"));
                    Assert.That(node.KeyIsQuoted, Is.False);
                }
            ).Value,
            metadata,
            new FileRange(2, 6, 2, 13),
            8,
            1,
            ref charIndex,
            node =>
            {
                Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                Assert.That(node.Value, Is.EqualTo("Backpack"));
                Assert.That(node.IsQuoted, Is.False);
            }
        );

        AssertNode<IWhiteSpaceSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(3, 1, 3, 1),
            endlLen,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node.Lines, Is.EqualTo(1));
            }
        );

        // Pro
        AssertNode<IPropertySourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(4, 1, 4, 3),
            3,
            0,
            ref charIndex,
            node =>
            {
                Assert.That(node.Key, Is.EqualTo("Pro"));
                Assert.That(node.KeyIsQuoted, Is.False);
                Assert.That(node.HasValue, Is.False);
            }
        );
    }


    [Test]
    public void File1([Values(SourceNodeTokenizerOptions.Lazy, SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.Lazy | SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.None)] SourceNodeTokenizerOptions options, [Values(true, false)] bool unix)
    {
        string file =
"""


// Comment


Key
Key Value // not inline comment
"Key"

// comment
Key "Value" // inline comment
"Key"
{
    
    "Key" "Value"
    "Key"
    [
        {
        
        }
        "Value"
        [
            // value comment
            // value comment 2
            Value
        ]
        [
        
        ]
        [
        ]
        {
            K V
            Key ""
            Key
        }
        ""
    ]
    "Key" "Vlu"
}
"List"
[

]

"SpacedList"

[
    Value
]

"DictWithComments"
{ / comment 1
    // comment in dict
    Key Value
    // comment 2 in dict
} /comment 2

""";

        FixLineEnds(unix, ref file, out int endlLen);

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(file, options);

        ISourceFile sourceFile = tok.ReadRootDictionary(SourceNodeTokenizer.RootInfo.Asset(new TestWorkspaceFile(), _database));

        StringWriter sw = new StringWriter();
        NodeWriteToTextWriterVisitor visitor = new NodeWriteToTextWriterVisitor(sw);

        sourceFile.Visit(ref visitor);

        Console.WriteLine(sw.ToString());

        bool metadata = (options & SourceNodeTokenizerOptions.Metadata) != 0;
        int index = 0;
        int charIndex = 0;

        AssertNode<IWhiteSpaceSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(1, 1, 2, 1),
            endlLen * 2,
            0,
            ref charIndex,
            node =>
            {
                Assert.That(node.Lines, Is.EqualTo(2));
            }
        );

        // // Comment
        AssertNode<ICommentSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(3, 1, 3, 10),
            10,
            0,
            ref charIndex,
            node =>
            {
                Assert.That(node.Comments, Is.EquivalentTo([ new Comment(CommentPrefix.Default, "Comment", CommentPosition.NewLine) ]));
            }
        );

        AssertNode<IWhiteSpaceSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(4, 1, 5, 1),
            endlLen * 2,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node.Lines, Is.EqualTo(2));
            }
        );

        // Key
        AssertNode<IPropertySourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(6, 1, 6, 3),
            3,
            0,
            ref charIndex,
            node =>
            {
                Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                Assert.That(node.Key, Is.EqualTo("Key"));
                Assert.That(node.KeyIsQuoted, Is.False);
            }
        );

        // Key Value // not inline comment
        AssertNode<IValueSourceNode>(
            AssertNode<IPropertySourceNode>(
                sourceFile,
                ref index,
                metadata,
                new FileRange(7, 1, 7, 3),
                3,
                endlLen,
                ref charIndex,
                node =>
                {
                    Assert.That(node.Key, Is.EqualTo("Key"));
                    Assert.That(node.KeyIsQuoted, Is.False);
                }
            ).Value,
            metadata,
            new FileRange(7, 5, 7, 31),
            27,
            1,
            ref charIndex,
            node =>
            {
                Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                Assert.That(node.Value, Is.EqualTo("Value // not inline comment"));
                Assert.That(node.IsQuoted, Is.False);
            }
        );

        // "Key"
        AssertNode<IPropertySourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(8, 1, 8, 5),
            5,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                Assert.That(node.Key, Is.EqualTo("Key"));
                Assert.That(node.KeyIsQuoted, Is.True);
            }
        );

        AssertNode<IWhiteSpaceSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(9, 1, 9, 1),
            endlLen,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node.Lines, Is.EqualTo(1));
            }
        );

        // // comment
        AssertNode<ICommentSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(10, 1, 10, 10),
            10,
            0,
            ref charIndex,
            node =>
            {
                Assert.That(node.Comments, Is.EquivalentTo([ new Comment(CommentPrefix.Default, "comment", CommentPosition.NewLine) ]));
            }
        );

        // Key "Value" // inline comment
        AssertNode<IValueSourceNode>(
            AssertNode<IPropertySourceNode>(
                sourceFile,
                ref index,
                metadata,
                new FileRange(11, 1, 11, 3),
                3,
                endlLen,
                ref charIndex,
                node =>
                {
                    Assert.That(node.Key, Is.EqualTo("Key"));
                    Assert.That(node.KeyIsQuoted, Is.False);
                    if (metadata)
                    {
                        Assert.That(node, Is.AssignableTo<ICommentSourceNode>());
                        Assert.That(((ICommentSourceNode)node).Comments, Is.EquivalentTo([ new Comment(CommentPrefix.Default, "inline comment", CommentPosition.EndOfLine) ]));
                    }
                    else
                    {
                        Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                    }
                }
            ).Value,
            metadata,
            new FileRange(11, 5, 11, 11),
            7,
            1,
            ref charIndex,
            node =>
            {
                Assert.That(node.Value, Is.EqualTo("Value"));
                Assert.That(node.IsQuoted, Is.True);
                Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
            }
        );

        charIndex += " // inline comment".Length;

        // "Key" {

        IDictionarySourceNode? mainDict = (IDictionarySourceNode?)AssertNode<IPropertySourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(12, 1, 12, 5),
            5,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node.Key, Is.EqualTo("Key"));
                Assert.That(node.KeyIsQuoted, Is.True);
                Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
            }
        ).Value;

        Assert.That(mainDict, Is.Not.Null);

        {
            int subIndex = 0;

            AssertNode<IWhiteSpaceSourceNode>(
                mainDict,
                ref subIndex,
                metadata,
                new FileRange(14, 5, 14, 5),
                endlLen + 4,
                endlLen * 2 + 5,
                ref charIndex,
                node =>
                {
                    Assert.That(node.Lines, Is.EqualTo(1));
                }
            );


            // "Key" "Value"
            AssertNode<IValueSourceNode>(
                AssertNode<IPropertySourceNode>(
                    mainDict,
                    ref subIndex,
                    metadata,
                    new FileRange(15, 5, 15, 9),
                    5,
                    0,
                    ref charIndex,
                    node =>
                    {
                        Assert.That(node.Key, Is.EqualTo("Key"));
                        Assert.That(node.KeyIsQuoted, Is.True);
                        Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                    }
                ).Value,
                metadata,
                new FileRange(15, 11, 15, 17),
                7,
                1,
                ref charIndex,
                node =>
                {
                    Assert.That(node.Value, Is.EqualTo("Value"));
                    Assert.That(node.IsQuoted, Is.True);
                    Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                }
            );
            // "Key" [

            IListSourceNode? l1 = (IListSourceNode?)AssertNode<IPropertySourceNode>(
                mainDict,
                ref subIndex,
                metadata,
                new FileRange(16, 5, 16, 9),
                5,
                endlLen + 4,
                ref charIndex,
                node =>
                {
                    Assert.That(node.Key, Is.EqualTo("Key"));
                    Assert.That(node.KeyIsQuoted, Is.True);
                    Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                }
            ).Value;
        }
    }


    [Test]
    public void FileWithProperties([Values(SourceNodeTokenizerOptions.Lazy, SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.Lazy | SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.None)] SourceNodeTokenizerOptions options, [Values(true, false)] bool unix)
    {
        string file =
"""
// udat-type: SDG.Unturned.ConfigData, Assembly-CSharp
// udat-prop:
//  udat-prop2  :  value  
// not-udat-prop: test
// Comment


Key
Key Value // not inline comment
"Key"
""";

        FixLineEnds(unix, ref file, out int endlLen);

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(file, options);

        ISourceFile sourceFile = tok.ReadRootDictionary(SourceNodeTokenizer.RootInfo.Asset(new TestWorkspaceFile(), _database));

        StringWriter sw = new StringWriter();
        NodeWriteToTextWriterVisitor visitor = new NodeWriteToTextWriterVisitor(sw);

        sourceFile.Visit(ref visitor);

        Console.WriteLine(sw.ToString());

        bool metadata = (options & SourceNodeTokenizerOptions.Metadata) != 0;
        int index = 0;
        int charIndex = 0;

        Assert.That(sourceFile.TryGetAdditionalProperty("type", out string? propVal));
        Assert.That(propVal, Is.EqualTo("SDG.Unturned.ConfigData, Assembly-CSharp"));

        Assert.That(sourceFile.TryGetAdditionalProperty("prop", out propVal));
        Assert.That(propVal, Is.Null);

        Assert.That(sourceFile.TryGetAdditionalProperty("prop2", out propVal));
        Assert.That(propVal, Is.EqualTo("value"));

        Assert.That(sourceFile.TryGetAdditionalProperty("not-udat-prop", out propVal), Is.False);
        Assert.That(sourceFile.TryGetAdditionalProperty("udat-prop", out propVal), Is.False);
        Assert.That(sourceFile.TryGetAdditionalProperty("Comment", out propVal), Is.False);
        Assert.That(sourceFile.TryGetAdditionalProperty(string.Empty, out propVal), Is.False);

        // // udat-type: SDG.Unturned.ConfigData, Assembly-CSharp
        AssertNode<ICommentSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(1, 1, 1, 54),
            54,
            0,
            ref charIndex,
            node =>
            {
                Assert.That(node.Comments, Is.EquivalentTo([ Comment.AdditionalProperty("type", "SDG.Unturned.ConfigData, Assembly-CSharp") ]));
            }
        );

        // // udat-prop:
        AssertNode<ICommentSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(2, 1, 2, 13),
            13,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node.Comments, Is.EquivalentTo([ Comment.AdditionalProperty("prop", null) ]));
            }
        );

        // //  udat-prop2  :  value  
        AssertNode<ICommentSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(3, 1, 3, 26),
            26,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node.Comments, Is.EquivalentTo([ new Comment(new CommentPrefix(2, 2), "udat-prop2  :  value  ", CommentPosition.NewLine) ]));
            }
        );

        // // not-udat-prop: test
        AssertNode<ICommentSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(4, 1, 4, 22),
            22,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node.Comments, Is.EquivalentTo([ new Comment(CommentPrefix.Default, "not-udat-prop: test", CommentPosition.NewLine) ]));
            }
        );

        // // Comment
        AssertNode<ICommentSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(5, 1, 5, 10),
            10,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node.Comments, Is.EquivalentTo([ new Comment(CommentPrefix.Default, "Comment", CommentPosition.NewLine) ]));
            }
        );

        AssertNode<IWhiteSpaceSourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(6, 1, 7, 1),
            endlLen * 2,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node.Lines, Is.EqualTo(2));
            }
        );

        // Key
        AssertNode<IPropertySourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(8, 1, 8, 3),
            3,
            0,
            ref charIndex,
            node =>
            {
                Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                Assert.That(node.Key, Is.EqualTo("Key"));
                Assert.That(node.KeyIsQuoted, Is.False);
            }
        );

        // Key Value // not inline comment
        AssertNode<IValueSourceNode>(
            AssertNode<IPropertySourceNode>(
                sourceFile,
                ref index,
                metadata,
                new FileRange(9, 1, 9, 3),
                3,
                endlLen,
                ref charIndex,
                node =>
                {
                    Assert.That(node.Key, Is.EqualTo("Key"));
                    Assert.That(node.KeyIsQuoted, Is.False);
                }
            ).Value,
            metadata,
            new FileRange(9, 5, 9, 31),
            27,
            1,
            ref charIndex,
            node =>
            {
                Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                Assert.That(node.Value, Is.EqualTo("Value // not inline comment"));
                Assert.That(node.IsQuoted, Is.False);
            }
        );

        // "Key"
        AssertNode<IPropertySourceNode>(
            sourceFile,
            ref index,
            metadata,
            new FileRange(10, 1, 10, 5),
            5,
            endlLen,
            ref charIndex,
            node =>
            {
                Assert.That(node, Is.Not.AssignableTo<ICommentSourceNode>());
                Assert.That(node.Key, Is.EqualTo("Key"));
                Assert.That(node.KeyIsQuoted, Is.True);
            }
        );
    }

    [Test]
    public void LocalizationFileTest([Values(SourceNodeTokenizerOptions.Lazy, SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.Lazy | SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.None)] SourceNodeTokenizerOptions options, [Values(true, false)] bool unix)
    {
        string file = 
"""
Rarity_0 Common
Rarity_1 Uncommon
Rarity_2 Rare
Rarity_3 Epic
Rarity_4 Legendary
Rarity_5 Mythical

Type_0 Hat
Type_1 Pants
Type_2 Shirt
Type_3 Mask
Type_4 Backpack
Type_5 Vest
Type_6 Glasses
Type_7 Ranged Weapon
Type_8 Sight Attachment
Type_9 Tactical Attachment
Type_10 Grip Attachment
Type_11 Barrel Attachment
Type_12 Magazine Attachment
Type_13 Food
Type_14 Drink
Type_15 Medicine
Type_16 Melee Weapon
Type_17 Fuel Canister
Type_18 Tool
Type_19 Barricade
Type_20 Item Storage
Type_21 Beacon
Type_22 Plant
Type_23 Trap
Type_24 Structure
Type_25 Crafting Supply
Type_26 Projectile
Type_27 Growth Supplement
Type_28 Optic
Type_29 Water Canister
Type_30 Fishing Pole
Type_31 Parachute
Type_32 Map
Type_33 Key
Type_34 Box
Type_35 Catcher
Type_36 Releaser
Type_37 Liquid Storage
Type_38 Generator
Type_39 Remote Trigger
Type_40 Remote Explosive
Type_41 Experience Storage
Type_42 Radiation Filter
Type_43 Robotic Turret
Type_44 Tool
Type_45 Tool
Type_46 Compass
Type_47 Oil Pump
Type_48 Paint
Type_49 Vehicle Lockpick

Hotkey_Set Hotkey: {0}
Hotkey_Unset Hotkey: [3-9]

Rarity_Type_Label {0} {1}
Quality Quality: {0}%
Amount Amount: {0}

Refill Water: {0}
Empty Empty
Full Full
Clean Clean
Salty Salty
Dirty Dirty

Ammo Ammo: {0} {1}/{2}
None Empty

ItemDescription_AmountWithCapacity Amount: {0}/{1}
ItemDescription_SightAttachment Sight: {0}
ItemDescription_TacticalAttachment Tactical: {0}
ItemDescription_GripAttachment Grip: {0}
ItemDescription_BarrelAttachment Barrel: {0}
ItemDescription_Clothing_Armor Armor: {0}
ItemDescription_Clothing_Earpiece Listens to radio chatter.
ItemDescription_Clothing_ExplosionArmor Explosion Armor: {0}
ItemDescription_Clothing_FireProof Protects against fire.
ItemDescription_Clothing_RadiationProof Protects against radiation.
ItemDescription_Clothing_FallingBoneBreakingProof Protects against breaking bones from long falls.
ItemDescription_ClothingMovementSpeedModifier Wearing Movement Speed: {0}
ItemDescription_FallingDamageModifier Falling Damage: {0}
ItemDescription_FilterDegradationRateMultiplier Filter Degradation: {0}
ItemDescription_EquipableMovementSpeedModifier Equipped Movement Speed: {0}
ItemDescription_PelletCount Pellets: {0}
ItemDescription_BulletDamageModifier Bullet Damage: {0}
ItemDescription_RecoilModifier_X Horizontal Recoil Control: {0}
ItemDescription_RecoilModifier_Y Vertical Recoil Control: {0}
ItemDescription_RecoilModifier_Aiming Aiming Recoil Control: {0}
ItemDescription_AimDurationModifier Aiming Speed: {0}
ItemDescription_AimingMovementSpeedModifier Aiming Movement Speed: {0}
ItemDescription_SpreadModifier Accuracy: {0}
ItemDescription_SwayModifier Scope Sway: {0}
ItemDescription_AimingSpreadModifier Aiming Accuracy: {0}
ItemDescription_ZoomFactor Zoom: {0}x
ItemDescription_ThirdPersonZoomFactor Zoom (3rd-Person): {0}x
ItemDescription_BulletGravityModifier Bullet Gravity: {0}
ItemDescription_InvulnerableModifier Upgrades gun to Heavy Weapon: Can damage Tough entities.
ItemDescription_Consumeable_Explosive Explodes when consumed.
ItemDescription_Consumeable_HealthPositive Restores Health: {0}
ItemDescription_Consumeable_HealthNegative Damages Health: {0}
ItemDescription_Consumeable_FoodPositive Restores Food: {0}
ItemDescription_Consumeable_FoodNegative Depletes Food: {0}
ItemDescription_Consumeable_WaterPositive Restores Water: {0}
ItemDescription_Consumeable_WaterNegative Depletes Water: {0}
ItemDescription_Consumeable_VirusPositive Restores Immunity: {0}
ItemDescription_Consumeable_VirusNegative Infection Damage: {0}
ItemDescription_Consumeable_StaminaPositive Restores Stamina: {0}
ItemDescription_Consumeable_StaminaNegative Depletes Stamina: {0}
ItemDescription_Consumeable_OxygenPositive Restores Oxygen: {0}
ItemDescription_Consumeable_OxygenNegative Depletes Oxygen: {0}
ItemDescription_Consumeable_WarmthPositive Provides Warmth: {0}
ItemDescription_ConsumeableBleeding_Heal Stops bleeding.
ItemDescription_ConsumeableBleeding_Cut Causes bleeding.
ItemDescription_ConsumeableBones_Heal Fixes bones.
ItemDescription_ConsumeableBones_Break Breaks bones.
ItemDescription_ConsumeableMoldy Low quality indicates spoiling/mold and will hurt immunity.
ItemDescription_Firerate Rate of Fire: {0} rpm
ItemDescription_Spread Spread (Hipfire): {0} °
ItemDescription_Spread_Aim Spread (Aiming): {0} °
ItemDescription_DamageFalloff Damage falloff begins at {0} down to {2} at {1}.
ItemDescription_WeaponRange Range: {0}
ItemDescription_WeaponDamage_Player_Head Player Headshot Damage: {0}
ItemDescription_WeaponDamage_Player_Body Player Body Damage: {0}
ItemDescription_WeaponDamage_Player_Arm Player Arm Damage: {0}
ItemDescription_WeaponDamage_Player_Leg Player Leg Damage: {0}
ItemDescription_WeaponDamage_Player_FoodPositive Impact Restores Food: {0}
ItemDescription_WeaponDamage_Player_FoodNegative Hunger Damage: {0}
ItemDescription_WeaponDamage_Player_WaterPositive Impact Restores Water: {0}
ItemDescription_WeaponDamage_Player_WaterNegative Thirst Damage: {0}
ItemDescription_WeaponDamage_Player_VirusPositive Impact Restores Immunity: {0}
ItemDescription_WeaponDamage_Player_VirusNegative Infection Damage: {0}
ItemDescription_WeaponDamage_Player_HallucinationPositive Damage Causes Hallucinations: {0}
ItemDescription_WeaponDamage_Player_HallucinationNegative Impact Heals Hallucinations: {0}
ItemDescription_WeaponBleeding_Always Damage always causes bleeding.
ItemDescription_WeaponBleeding_Heal Impact stops bleeding.
ItemDescription_WeaponBones_Always Damage breaks bones.
ItemDescription_WeaponBones_Heal Impact fixes bones.
ItemDescription_WeaponDamage_Zombie_Head Zombie Headshot Damage: {0}
ItemDescription_WeaponDamage_Zombie_Body Zombie Body Damage: {0}
ItemDescription_WeaponDamage_Zombie_Arm Zombie Arm Damage: {0}
ItemDescription_WeaponDamage_Zombie_Leg Zombie Leg Damage: {0}
ItemDescription_WeaponDamage_Animal_Head Animal Headshot Damage: {0}
ItemDescription_WeaponDamage_Animal_Body Animal Body Damage: {0}
ItemDescription_WeaponDamage_Animal_Limb Animal Limb Damage: {0}
ItemDescription_WeaponDamage_Barricade Barricade Damage: {0}
ItemDescription_WeaponDamage_Structure Structure Damage: {0}
ItemDescription_WeaponDamage_Vehicle Vehicle Damage: {0}
ItemDescription_WeaponDamage_Resource Resource Damage: {0}
ItemDescription_WeaponDamage_Object Object Damage: {0}
ItemDescription_WeaponDamage_Invulnerable Heavy Weapon: Can damage Tough entities.
ItemDescription_ExplosiveBullet Explodes on impact.
ItemDescription_ExplosionBlastRadius Explosion Radius: {0}
ItemDescription_ExplosionPlayerDamage Player Damage (Explosion): {0}
ItemDescription_ExplosionZombieDamage Zombie Damage (Explosion): {0}
ItemDescription_ExplosionAnimalDamage Animal Damage (Explosion): {0}
ItemDescription_ExplosionBarricadeDamage Barricade Damage (Explosion): {0}
ItemDescription_ExplosionStructureDamage Structure Damage (Explosion): {0}
ItemDescription_ExplosionVehicleDamage Vehicle Damage (Explosion): {0}
ItemDescription_ExplosionResourceDamage Resource Damage (Explosion): {0}
ItemDescription_ExplosionObjectDamage Object Damage (Explosion): {0}
ItemDescription_Buildable_ArmorTier_Low Armor Tier: Low
ItemDescription_Buildable_ArmorTier_High Armor Tier: High
ItemDescription_Buildable_CannotPickup Cannot be picked up after placement.
ItemDescription_Buildable_CannotSalvage Cannot be salvaged when damaged.
ItemDescription_Buildable_CannotRepair Cannot be repaired when damaged.
ItemDescription_Buildable_Health Health: {0}
ItemDescription_Buildable_ExplosionProof Cannot be damaged by explosions.
ItemDescription_Buildable_Lockable Locked for players outside your group.
ItemDescription_Buildable_Invulnerable Tough: Can only be damaged by Heavy Weapons.
ItemDescription_Farmable_GrowSpecificItem Produces: {0}
ItemDescription_Farmable_RequiresSoil Must be planted in arable terrain.
ItemDescription_Farmable_CanFertilize Growth can be expedited with fertilizer.
ItemDescription_Farmable_AffectedByAgricultureSkill Yield is affected by your Agriculture skill level.
ItemDescription_Farmable_AffectedByRain Growth can be expedited by rainy weather.
ItemDescription_Trap_BreaksBones Trap breaks bones.
ItemDescription_Trap_DamagesTires Trap pierces tires.
ItemDescription_Trap_RequiresPower Trap requires electricity.
ItemDescription_Trap_PlayerDamage Player Damage: {0}
ItemDescription_Trap_ZombieDamage Zombie Damage: {0}
ItemDescription_Trap_AnimalDamage Animal Damage: {0}
ItemDescription_StorageDimensions Storage: {0} x {1}
ItemDescription_Throwable_Flash Blinds nearby players looking into the explosion.
ItemDescription_Throwable_Sticky Sticks to surface on impact.
ItemDescription_Throwable_ExplodeOnImpact Explodes on impact.
ItemDescription_Throwable_FuseLength Fuse Length: {0}
ItemDescription_Melee_StrongAttackModifier Strong Attack Damage: {0}
ItemDescription_Melee_StrongAttackStamina Strong Attack Stamina Cost: {0}
ItemDescription_ArrestEnd_UnlocksItem Unlocks: {0}
ItemDescription_FuelAmountWithCapacity Fuel: {0}/{1} ({2})
ItemDescription_FuelCapacity Fuel Capacity: {0}
ItemDescription_WaterCapacity Water Capacity: {0}
ItemDescription_FuelBurnRate Fuel Burned: {0}/hour
ItemDescription_FuelMaxRuntime Max Runtime: {0} hour(s)
ItemDescription_AmmoConsumptionProbability Ammo Decrease Chance: {0}
ItemDescription_QualityConsumptionProbability Quality Decrease Chance: {0}
ItemDescription_ProvidesCraftingTags Crafting Capabilities:
ItemDescription_ListItem • {0}
ItemDescription_LockpickFailureProbability Failure Chance: {0}

Equip_Button Equip
Equip_Button_Tooltip Equip in your hands.
Dequip_Button Dequip
Dequip_Button_Tooltip Dequip from your hands.
Drop_Button Drop
Drop_Button_Tooltip Drop on the ground.
Pickup_Button Pickup
Pickup_Button_Tooltip Pickup from the ground.

Take_Button Take
Take_Button_Tooltip Take from storage.
Store_Button Store
Store_Button_Tooltip Store in storage.

Repair_Button Repair
Repair_Button_Tooltip Repair to 100% quality.
Salvage_Button Salvage
Salvage_Button_Tooltip Salvage into component materials.
Refill_Button Refill
Refill_Button_Tooltip Refill with bullets.
Stack_Button Stack
Stack_Button_Tooltip Consolidate items into a bundle.
Unstack_Button Unstack
Unstack_Button_Tooltip Separate bundle into individual items.

ActionBlueprint_SkipCraftingTooltip Skip Crafting Menu [{0}]
ActionBlueprint_CraftAllTooltip Craft All [{0}]

Attachments_Button Strip
Attachments_Button_Tooltip Dismantle attachments.

Craft_Rag_Button Craft Rag
Craft_Rag_Button_Tooltip Upgrade to rags.
Craft_Bandage_Button Craft Bandage
Craft_Bandage_Button_Tooltip Upgrade to bandage.
Craft_Dressing_Button Craft Dressing
Craft_Dressing_Button_Tooltip Upgrade to dressing.
Craft_Seed_Button Craft Seed
Craft_Seed_Button_Tooltip Salvage seed.

Swap_Cosmetics_Tooltip Toggle Cosmetics
Swap_Skins_Tooltip Toggle Skins
Swap_Mythics_Tooltip Toggle Mythics

Vehicle_Lock_On Lock [{0}]
Vehicle_Lock_Off Unlock [{0}]
Vehicle_Lock_Tooltip Keep out vehicle thieves.
Vehicle_Horn Horn [{0}]
Vehicle_Horn_Tooltip Warning audio.
Vehicle_Headlights_On Headlights On [{0}]
Vehicle_Headlights_Off Headlights Off [{0}]
Vehicle_Headlights_Tooltip Bright for nighttime travel.
Vehicle_Sirens_On Sirens On [{0}]
Vehicle_Sirens_Off Sirens Off [{0}]
Vehicle_Sirens_Tooltip Emergency audio.
Vehicle_Blimp_On Ascend [{0}]
Vehicle_Blimp_Off Descend [{0}]
Vehicle_Blimp_Tooltip Adjust baloon ballast.
Vehicle_Hook Towing Cable [{0}]
Vehicle_Hook_Tooltip Carry nearby vehicles.
Vehicle_Seat_Empty Empty
Vehicle_Seat_Slot {0} [{1}]
Vehicle_Steal_Battery Steal Battery
Vehicle_Steal_Battery_Tooltip Remove battery from engine.
Vehicle_Skin_On Apply Paintjob
Vehicle_Skin_Off Clear Paintjob
Vehicle_Skin_Tooltip Use your active vehicle skin.

Hands Hands
Storage Storage
Storage_Trunk {0} Trunk Storage
Area Nearby

Rot_X X++
Rot_Y Y++
Rot_Z Z++

""";

        FixLineEnds(unix, ref file, out int endlLen);

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(file, options);

        ISourceFile sourceFile = tok.ReadRootDictionary(SourceNodeTokenizer.RootInfo.Asset(new TestWorkspaceFile(), _database));

        Assert.That(sourceFile.Properties, Has.Length.EqualTo(282 - 16));
    }

    private static TNode AssertNode<TNode>(IAnyChildrenSourceNode parent, ref int childIndex, bool metadata, FileRange range, int length, int prevOffset, ref int charIndex, Action<TNode> other) where TNode : class, ISourceNode
    {
        if (!metadata && (typeof(TNode) == typeof(ICommentSourceNode) || typeof(TNode) == typeof(IWhiteSpaceSourceNode)))
        {
            charIndex += prevOffset + length;

            return null!;
        }

        TNode node = (TNode)parent.Children[childIndex++];
        AssertNode(node, metadata, range, length, prevOffset, ref charIndex, other);
        return node;
    }

    private static void AssertNode<TNode>(ISourceNode? node, bool metadata, FileRange range, int length, int prevOffset, ref int charIndex, Action<TNode> other) where TNode : class, ISourceNode
    {
        Assert.That(node, Is.Not.Null);

        TNode n = (TNode)node;

        if (!metadata && node.Type is SourceNodeType.Comment or SourceNodeType.Whitespace)
        {
            charIndex += prevOffset + length;

            return;
        }

        Assert.That(n.Range, Is.EqualTo(range));
        
        Assert.That(n.LastCharacterIndex - n.FirstCharacterIndex + 1, Is.EqualTo(length));

        Assert.That(n.FirstCharacterIndex - charIndex, Is.EqualTo(prevOffset));
        
        charIndex += prevOffset + length;

        Assert.That(n.LastCharacterIndex, Is.EqualTo(charIndex - 1));

        other(n);
    }

    private static void FixLineEnds(bool unix, ref string text, out int endlLen)
    {
        endlLen = 1 + (!unix ? 1 : 0);
        bool textIsUnix = !text.Contains("\r\n");
        if (unix)
        {
            if (!textIsUnix)
                text = text.Replace("\r\n", "\n");
        }
        else if (textIsUnix)
        {
            text = text.Replace("\n", "\r\n");
        }
    }

    private class TestWorkspaceFile : IWorkspaceFile
    {
        /// <inheritdoc />
        public void Dispose()
        {

        }

        /// <inheritdoc />
        public string File => "./test.asset";

        /// <inheritdoc />
        public ISourceFile SourceFile => null!;

        /// <inheritdoc />
        public string GetFullText() => null!;

        /// <inheritdoc />
        public event Action<IWorkspaceFile, FileRange>? OnUpdated
        {
            add { }
            remove { }
        }
    }
}

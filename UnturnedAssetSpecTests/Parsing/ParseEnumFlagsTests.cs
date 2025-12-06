using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests.Parsing;

public class ParseEnumFlagsTests
{
    private EnumSpecType _enumType;
    [SetUp]
    public void SetUp()
    {
        _enumType = new EnumSpecType
        {
            Values = new EnumSpecTypeValue[4],
            Type = "SDG.Unturned.ERayMask, Assembly-CSharp",
            DisplayName = "Ray Mask",
            IsFlags = true,
            AdditionalProperties = OneOrMore<KeyValuePair<string, object?>>.Null,
            Docs = null
        };

        _enumType.Values[0] = new EnumSpecTypeValue
        {
            Value = "DEFAULT",
            AdditionalProperties = OneOrMore<KeyValuePair<string, object?>>.Null,
            Index = 0,
            Casing = "Default",
            Type = _enumType,
            NumericValue = 1
        };

        _enumType.Values[1] = new EnumSpecTypeValue
        {
            Value = "TRANSPARENT_FX",
            AdditionalProperties = OneOrMore<KeyValuePair<string, object?>>.Null,
            Index = 1,
            Casing = "Transparent_FX",
            Type = _enumType,
            NumericValue = 2
        };

        _enumType.Values[2] = new EnumSpecTypeValue
        {
            Value = "IGNORE_RAYCAST",
            AdditionalProperties = OneOrMore<KeyValuePair<string, object?>>.Null,
            Index = 2,
            Casing = "Ignore_Raycast",
            Type = _enumType,
            NumericValue = 4
        };

        _enumType.Values[3] = new EnumSpecTypeValue
        {
            Value = "BUILTIN_3",
            AdditionalProperties = OneOrMore<KeyValuePair<string, object?>>.Null,
            Index = 3,
            Casing = "Builtin_3",
            Type = _enumType,
            NumericValue = 8
        };
    }

    [Test]
    public void CheckParseSingle()
    {
        Assert.That(SpecDynamicConcreteFlagsEnumValue.TryParseFlags(_enumType, "Ignore_Raycast", out OneOrMore<int> flags, ignoreCase: true));
        Assert.That(flags, Is.EquivalentTo([ 2 ]));
    }

    [Test]
    public void CheckParseTwo()
    {
        Assert.That(SpecDynamicConcreteFlagsEnumValue.TryParseFlags(_enumType, "Ignore_Raycast, Transparent_FX", out OneOrMore<int> flags, ignoreCase: true));
        Assert.That(flags, Is.EquivalentTo([ 2, 1 ]));
    }

    [Test]
    public void CheckParseMore()
    {
        Assert.That(SpecDynamicConcreteFlagsEnumValue.TryParseFlags(_enumType, "Ignore_Raycast, Transparent_FX, Builtin_3", out OneOrMore<int> flags, ignoreCase: true));
        Assert.That(flags, Is.EquivalentTo([ 2, 1, 3 ]));
    }

    [Test]
    public void CheckParseDifferentSpacing()
    {
        Assert.That(SpecDynamicConcreteFlagsEnumValue.TryParseFlags(_enumType, "Ignore_Raycast,Transparent_FX, Builtin_3", out OneOrMore<int> flags, ignoreCase: true));
        Assert.That(flags, Is.EquivalentTo([ 2, 1, 3 ]));
        Assert.That(SpecDynamicConcreteFlagsEnumValue.TryParseFlags(_enumType, "Ignore_Raycast , Transparent_FX,Builtin_3 ", out flags, ignoreCase: true));
        Assert.That(flags, Is.EquivalentTo([ 2, 1, 3 ]));
        Assert.That(SpecDynamicConcreteFlagsEnumValue.TryParseFlags(_enumType, " Ignore_Raycast , Transparent_FX, Builtin_3  ", out flags, ignoreCase: true));
        Assert.That(flags, Is.EquivalentTo([ 2, 1, 3 ]));
        Assert.That(SpecDynamicConcreteFlagsEnumValue.TryParseFlags(_enumType, "Ignore_Raycast  ,  Transparent_FX , Builtin_3 ", out flags, ignoreCase: true));
        Assert.That(flags, Is.EquivalentTo([ 2, 1, 3 ]));
        Assert.That(SpecDynamicConcreteFlagsEnumValue.TryParseFlags(_enumType, "Ignore_Raycast  ,Transparent_FX ,Builtin_3", out flags, ignoreCase: true));
        Assert.That(flags, Is.EquivalentTo([ 2, 1, 3 ]));
    }

    [Test]
    public void CheckDeconstructZero()
    {
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 0, true), Is.Empty);
    }

    [Test]
    public void CheckDeconstructOne()
    {
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 1, true), Is.EquivalentTo([ 0 ]));
        
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 2, true), Is.EquivalentTo([ 1 ]));
        
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 4, true), Is.EquivalentTo([ 2 ]));
        
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 8, true), Is.EquivalentTo([ 3 ]));
    }

    [Test]
    public void CheckDeconstructTwo()
    {
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 1 | 2, true), Is.EquivalentTo([ 0, 1 ]));
        
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 2 | 4, true), Is.EquivalentTo([ 1, 2 ]));
        
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 2 | 8, true), Is.EquivalentTo([ 1, 3 ]));
        
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 1 | 8, true), Is.EquivalentTo([ 0, 3 ]));
    }

    [Test]
    public void CheckDeconstructMore()
    {
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 1 | 2 | 4, true), Is.EquivalentTo([ 0, 1, 2 ]));
        
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 1 | 2 | 8, true), Is.EquivalentTo([ 0, 1, 3 ]));
        
        Assert.That(SpecDynamicConcreteFlagsEnumValue.Deconstruct(_enumType, 2 | 4 | 8, true), Is.EquivalentTo([ 1, 2, 3 ]));
    }
}

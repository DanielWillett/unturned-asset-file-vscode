using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Collections.Immutable;

namespace UnturnedAssetSpecTests.Parsing;

public class ParseEnumFlagsTests
{
    private DatFlagEnumType _enumType;
    [SetUp]
    public void SetUp()
    {
        _enumType = (DatFlagEnumType)DatType.CreateEnumType(new QualifiedType("SDG.Unturned.ERayMask, Assembly-CSharp", true), true, default, null);
        _enumType.DisplayName = "Ray Mask";

        ImmutableArray<DatEnumValue>.Builder builder = ImmutableArray.CreateBuilder<DatEnumValue>(4);

        DatFlagEnumValue valueTemp = DatFlagEnumValue.Create("DEFAULT", 0, _enumType, 1, default);
        valueTemp.Casing = "Default";
        builder.Add(valueTemp);

        valueTemp = DatFlagEnumValue.Create("TRANSPARENT_FX", 1, _enumType, 2, default);
        valueTemp.Casing = "Transparent_FX";
        builder.Add(valueTemp);

        valueTemp = DatFlagEnumValue.Create("IGNORE_RAYCAST", 2, _enumType, 4, default);
        valueTemp.Casing = "Ignore_Raycast";
        builder.Add(valueTemp);

        valueTemp = DatFlagEnumValue.Create("BUILTIN_3", 3, _enumType, 8, default);
        valueTemp.Casing = "Builtin_3";
        builder.Add(valueTemp);

        _enumType.Values = builder.MoveToImmutableOrCopy();
    }

    [Test]
    public void CheckParseSingle()
    {
        Assert.That(_enumType.TryParse("Ignore_Raycast", out DatFlagEnumValue? value));
        Assert.That(value, Is.Not.Null);
        Assert.That(value, Is.SameAs(_enumType.Values[2]));
    }

    [Test]
    public void CheckParseTwo()
    {
        Assert.That(_enumType.TryParse("Ignore_Raycast, Transparent_FX", out DatFlagEnumValue? value));
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Values, Is.EquivalentTo([ _enumType.Values[2], _enumType.Values[1] ]));
        Assert.That(value.Value, Is.EqualTo("IGNORE_RAYCAST, TRANSPARENT_FX"));
        Assert.That(value.Casing, Is.EqualTo("Ignore_Raycast, Transparent_FX"));
    }

    [Test]
    public void CheckParseMore()
    {
        Assert.That(_enumType.TryParse("Ignore_Raycast, Transparent_FX, Builtin_3", out DatFlagEnumValue? value));
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Values, Is.EquivalentTo([ _enumType.Values[2], _enumType.Values[1], _enumType.Values[3] ]));
        Assert.That(value.Value, Is.EqualTo("IGNORE_RAYCAST, TRANSPARENT_FX, BUILTIN_3"));
        Assert.That(value.Casing, Is.EqualTo("Ignore_Raycast, Transparent_FX, Builtin_3"));
    }

    [Test]
    public void CheckParseDifferentSpacing()
    {
        Assert.That(_enumType.TryParse("Ignore_Raycast,Transparent_FX, Builtin_3", out DatFlagEnumValue? value));
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Values, Is.EquivalentTo([ _enumType.Values[2], _enumType.Values[1], _enumType.Values[3] ]));
        Assert.That(value.Value, Is.EqualTo("IGNORE_RAYCAST, TRANSPARENT_FX, BUILTIN_3"));
        Assert.That(value.Casing, Is.EqualTo("Ignore_Raycast, Transparent_FX, Builtin_3"));
        
        Assert.That(_enumType.TryParse("Ignore_Raycast , Transparent_FX,Builtin_3 ", out value));
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Values, Is.EquivalentTo([ _enumType.Values[2], _enumType.Values[1], _enumType.Values[3] ]));
        Assert.That(value.Value, Is.EqualTo("IGNORE_RAYCAST, TRANSPARENT_FX, BUILTIN_3"));
        Assert.That(value.Casing, Is.EqualTo("Ignore_Raycast, Transparent_FX, Builtin_3"));

        Assert.That(_enumType.TryParse(" Ignore_Raycast , Transparent_FX, Builtin_3  ", out value));
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Values, Is.EquivalentTo([ _enumType.Values[2], _enumType.Values[1], _enumType.Values[3] ]));
        Assert.That(value.Value, Is.EqualTo("IGNORE_RAYCAST, TRANSPARENT_FX, BUILTIN_3"));
        Assert.That(value.Casing, Is.EqualTo("Ignore_Raycast, Transparent_FX, Builtin_3"));

        Assert.That(_enumType.TryParse("Ignore_Raycast  ,  Transparent_FX , Builtin_3 ", out value));
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Values, Is.EquivalentTo([ _enumType.Values[2], _enumType.Values[1], _enumType.Values[3] ]));
        Assert.That(value.Value, Is.EqualTo("IGNORE_RAYCAST, TRANSPARENT_FX, BUILTIN_3"));
        Assert.That(value.Casing, Is.EqualTo("Ignore_Raycast, Transparent_FX, Builtin_3"));

        Assert.That(_enumType.TryParse("Ignore_Raycast  ,Transparent_FX ,Builtin_3", out value));
        Assert.That(value, Is.Not.Null);
        Assert.That(value.Values, Is.EquivalentTo([ _enumType.Values[2], _enumType.Values[1], _enumType.Values[3] ]));
        Assert.That(value.Value, Is.EqualTo("IGNORE_RAYCAST, TRANSPARENT_FX, BUILTIN_3"));
        Assert.That(value.Casing, Is.EqualTo("Ignore_Raycast, Transparent_FX, Builtin_3"));
    }

    [Test]
    public void CheckDeconstructZero()
    {
        Assert.That(_enumType.Deconstruct(0, true), Is.Empty);
    }

    [Test]
    public void CheckDeconstructOne()
    {
        Assert.That(_enumType.Deconstruct(1, true), Is.EquivalentTo([ _enumType.Values[0] ]));
        
        Assert.That(_enumType.Deconstruct(2, true), Is.EquivalentTo([ _enumType.Values[1] ]));
        
        Assert.That(_enumType.Deconstruct(4, true), Is.EquivalentTo([ _enumType.Values[2] ]));
        
        Assert.That(_enumType.Deconstruct(8, true), Is.EquivalentTo([ _enumType.Values[3] ]));
    }

    [Test]
    public void CheckDeconstructTwo()
    {
        Assert.That(_enumType.Deconstruct(1 | 2, true), Is.EquivalentTo([ _enumType.Values[0], _enumType.Values[1] ]));
        
        Assert.That(_enumType.Deconstruct(2 | 4, true), Is.EquivalentTo([ _enumType.Values[1], _enumType.Values[2] ]));
        
        Assert.That(_enumType.Deconstruct(2 | 8, true), Is.EquivalentTo([ _enumType.Values[1], _enumType.Values[3] ]));
        
        Assert.That(_enumType.Deconstruct(1 | 8, true), Is.EquivalentTo([ _enumType.Values[0], _enumType.Values[3] ]));
    }

    [Test]
    public void CheckDeconstructMore()
    {
        Assert.That(_enumType.Deconstruct(1 | 2 | 4, true), Is.EquivalentTo([ _enumType.Values[0], _enumType.Values[1], _enumType.Values[2] ]));
        
        Assert.That(_enumType.Deconstruct(1 | 2 | 8, true), Is.EquivalentTo([ _enumType.Values[0], _enumType.Values[1], _enumType.Values[3] ]));
        
        Assert.That(_enumType.Deconstruct(2 | 4 | 8, true), Is.EquivalentTo([ _enumType.Values[1], _enumType.Values[2], _enumType.Values[3] ]));
    }
}

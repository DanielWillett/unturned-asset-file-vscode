using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace UnturnedAssetSpecTests.Expressions;

[TestFixture]
public class DataRefParseTests
{
    [Test]
    public void TestBasicRoots()
    {
        Assert.That(DataRefs.TryReadDataRef("#This", null, null!, out IDataRef? thisDataRef));

        Assert.That(thisDataRef, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(thisDataRef);

        Assert.That(DataRefs.TryReadDataRef("#Self", null, null!, out IDataRef? selfDataRef));

        Assert.That(selfDataRef, Is.InstanceOf<SelfDataRef>());
        Console.WriteLine(selfDataRef);



        Assert.That(DataRefs.TryReadDataRef("#(This)", null, null!, out thisDataRef));

        Assert.That(thisDataRef, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(thisDataRef);

        Assert.That(DataRefs.TryReadDataRef("#(Self)", null, null!, out selfDataRef));

        Assert.That(selfDataRef, Is.InstanceOf<SelfDataRef>());
        Console.WriteLine(selfDataRef);
    }
    
    [Test]
    public void TestContextualRoots()
    {
        TestDataRefContext ctx = new TestDataRefContext();

        Assert.That(DataRefs.TryReadDataRef("#Value", null, null!, out IDataRef? valueDataRef, ctx));

        Assert.That(valueDataRef, Is.InstanceOf<ValueDataRef>());
        Console.WriteLine(valueDataRef);

        Assert.That(DataRefs.TryReadDataRef("#Index", null, null!, out IDataRef? indexDataRef, ctx));

        Assert.That(indexDataRef, Is.InstanceOf<IndexDataRef<int>>());
        Console.WriteLine(indexDataRef);

        Assert.That(DataRefs.TryReadDataRef("#Key", null, null!, out IDataRef? keyDataRef, ctx));

        Assert.That(keyDataRef, Is.InstanceOf<KeyDataRef<string>>());
        Console.WriteLine(keyDataRef);

        Assert.That(DataRefs.TryReadDataRef("#(Value)", null, null!, out valueDataRef, ctx));

        Assert.That(valueDataRef, Is.InstanceOf<ValueDataRef>());
        Console.WriteLine(valueDataRef);

        Assert.That(DataRefs.TryReadDataRef("#(Index)", null, null!, out indexDataRef, ctx));

        Assert.That(indexDataRef, Is.InstanceOf<IndexDataRef<int>>());
        Console.WriteLine(indexDataRef);

        Assert.That(DataRefs.TryReadDataRef("#(Key)", null, null!, out keyDataRef, ctx));

        Assert.That(keyDataRef, Is.InstanceOf<KeyDataRef<string>>());
        Console.WriteLine(keyDataRef);
    }

    [Test]
    public void FallbackPropertyRoot()
    {
        DatProperty property = new DatProperty("Property", null!, default, SpecPropertyContext.Property) { Type = StringType.Instance };
        

        Assert.That(DataRefs.TryReadDataRef("#Property", null, property, out IDataRef? dataRef));

        Assert.That(dataRef, Is.InstanceOf<PropertyDataRef>());
        Assert.That(((PropertyDataRef)dataRef).PropertyName, Is.EqualTo("Property"));
        Console.WriteLine(dataRef);
        

        Assert.That(DataRefs.TryReadDataRef("#(Property)", null, property, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<PropertyDataRef>());
        Assert.That(((PropertyDataRef)dataRef).PropertyName, Is.EqualTo("Property"));
        Console.WriteLine(dataRef);
    }

    [Test]
    public void EscapedPropertyRoot()
    {
        DatProperty property = new DatProperty("This", null!, default, SpecPropertyContext.Property) { Type = StringType.Instance };
        

        Assert.That(DataRefs.TryReadDataRef("#\\This", null, property, out IDataRef? dataRef));

        Assert.That(dataRef, Is.InstanceOf<PropertyDataRef>());
        Assert.That(((PropertyDataRef)dataRef).PropertyName, Is.EqualTo("This"));
        Console.WriteLine(dataRef);
        

        Assert.That(DataRefs.TryReadDataRef("#(\\This)", null, property, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<PropertyDataRef>());
        Assert.That(((PropertyDataRef)dataRef).PropertyName, Is.EqualTo("This"));
        Console.WriteLine(dataRef);
    }

    [Test]
    public void TestBasicProperties()
    {
        Assert.That(DataRefs.TryReadDataRef("#This.Included", null, null!, out IDataRef? dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#This.(Included)", null, null!, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#(This).(Included)", null, null!, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#(This).Included", null, null!, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);
    }

    [Test]
    public void TestBasicPropertiesWithProperty()
    {
        Assert.That(DataRefs.TryReadDataRef("#This.Included{\"RequireValue\":true}", null, null!, out IDataRef? dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(((DataRefProperty<IncludedProperty>)dataRef).Property.RequireValue, Is.True);
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#This.(Included){\"RequireValue\":false}", null, null!, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(((DataRefProperty<IncludedProperty>)dataRef).Property.RequireValue, Is.False);
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);
    }

    [Test]
    public void TestBasicPropertiesWithIndex()
    {
        Assert.That(DataRefs.TryReadDataRef("#This.Indices[-1]", null, null!, out IDataRef? dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IndicesProperty>>());
        Assert.That(((DataRefProperty<IndicesProperty>)dataRef).Property.Index, Is.EqualTo(-1));
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#This.(Indices)[1]", null, null!, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IndicesProperty>>());
        Assert.That(((DataRefProperty<IndicesProperty>)dataRef).Property.Index, Is.EqualTo(1));
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#This.Indices", null, null!, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IndicesProperty>>());
        Assert.That(((DataRefProperty<IndicesProperty>)dataRef).Property.Index, Is.Null);
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);
    }


    public class TestDataRefContext : IDataRefReadContext
    {
        /// <inheritdoc />
        public bool TryReadTarget(ReadOnlySpan<char> root, IType? type, [NotNullWhen(true)] out IDataRefTarget? target)
        {
            switch (root.ToString())
            {
                case "Value":
                    target = ValueDataRef.Instance;
                    return true;

                case "Index":
                    target = IndexDataRef<int>.Instance;
                    return true;

                case "Key":
                    target = new KeyDataRef<string>("test", StringType.Instance);
                    return true;

                default:
                    Assert.Fail("Unknown data-ref type (shouldn't be reached).");
                    target = null;
                    return false;
            }
        }
    }
}

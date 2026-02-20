using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System.Diagnostics.CodeAnalysis;

namespace UnturnedAssetSpecTests.Expressions;

[TestFixture]
public class DataRefParseTests
{
    [Test]
    public void TestBasicRoots()
    {
        DatProperty property = new DatProperty("Property", null!, default, SpecPropertyContext.Property) { Type = StringType.Instance };


        Assert.That(DataRefs.TryReadDataRef("#This", null, property, out IDataRef? thisDataRef));

        Assert.That(thisDataRef, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(thisDataRef);

        Assert.That(DataRefs.TryReadDataRef("#Self", null, property, out IDataRef? selfDataRef));

        Assert.That(selfDataRef, Is.InstanceOf<SelfDataRef>());
        Console.WriteLine(selfDataRef);



        Assert.That(DataRefs.TryReadDataRef("#(This)", null, property, out thisDataRef));

        Assert.That(thisDataRef, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(thisDataRef);

        Assert.That(DataRefs.TryReadDataRef("#(Self)", null, property, out selfDataRef));

        Assert.That(selfDataRef, Is.InstanceOf<SelfDataRef>());
        Console.WriteLine(selfDataRef);
    }
    
    [Test]
    public void TestContextualRoots()
    {
        DatProperty property = new DatProperty("Property", null!, default, SpecPropertyContext.Property) { Type = StringType.Instance };


        TestDataRefContext ctx = new TestDataRefContext();

        Assert.That(DataRefs.TryReadDataRef("#Value", null, property, out IDataRef? valueDataRef, ref ctx));

        Assert.That(valueDataRef, Is.InstanceOf<ValueDataRef<string>>());
        Console.WriteLine(valueDataRef);

        Assert.That(DataRefs.TryReadDataRef("#Index", null, property, out IDataRef? indexDataRef, ref ctx));

        Assert.That(indexDataRef, Is.InstanceOf<IndexDataRef<int>>());
        Console.WriteLine(indexDataRef);

        Assert.That(DataRefs.TryReadDataRef("#Key", null, property, out IDataRef? keyDataRef, ref ctx));

        Assert.That(keyDataRef, Is.InstanceOf<KeyDataRef<string>>());
        Console.WriteLine(keyDataRef);

        Assert.That(DataRefs.TryReadDataRef("#(Value)", null, property, out valueDataRef, ref ctx));

        Assert.That(valueDataRef, Is.InstanceOf<ValueDataRef<string>>());
        Console.WriteLine(valueDataRef);

        Assert.That(DataRefs.TryReadDataRef("#(Index)", null, property, out indexDataRef, ref ctx));

        Assert.That(indexDataRef, Is.InstanceOf<IndexDataRef<int>>());
        Console.WriteLine(indexDataRef);

        Assert.That(DataRefs.TryReadDataRef("#(Key)", null, property, out keyDataRef, ref ctx));

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
        DatProperty property = new DatProperty("Property", null!, default, SpecPropertyContext.Property) { Type = StringType.Instance };


        Assert.That(DataRefs.TryReadDataRef("#This.Included", null, property, out IDataRef? dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#This.(Included)", null, property, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#(This).(Included)", null, property, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#(This).Included", null, property, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);
    }

    [Test]
    public void TestBasicPropertiesWithProperty()
    {
        DatProperty property = new DatProperty("Property", null!, default, SpecPropertyContext.Property) { Type = StringType.Instance };


        Assert.That(DataRefs.TryReadDataRef("#This.Included{\"RequireValue\":true}", null, property, out IDataRef? dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(((DataRefProperty<IncludedProperty>)dataRef).Property.RequireValue, Is.True);
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#This.(Included){\"RequireValue\":false}", null, property, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IncludedProperty>>());
        Assert.That(((DataRefProperty<IncludedProperty>)dataRef).Property.RequireValue, Is.False);
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);
    }

    [Test]
    public void TestBasicPropertiesWithBoth()
    {
        DatProperty property = new DatProperty("Property", null!, default, SpecPropertyContext.Property) { Type = StringType.Instance };


        Assert.That(DataRefs.TryReadDataRef("#This.Indices[0]{\"PreventSelfReference\":true}", null, property, out IDataRef? dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IndicesProperty>>());
        Assert.That(((DataRefProperty<IndicesProperty>)dataRef).Property.PreventSelfReference, Is.True);
        Assert.That(((DataRefProperty<IndicesProperty>)dataRef).Property.Index, Is.EqualTo(0));
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#This.(Indices)[-1]{\"PreventSelfReference\":false}", null, property, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IndicesProperty>>());
        Assert.That(((DataRefProperty<IndicesProperty>)dataRef).Property.PreventSelfReference, Is.False);
        Assert.That(((DataRefProperty<IndicesProperty>)dataRef).Property.Index, Is.EqualTo(-1));
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);
    }

    [Test]
    public void TestBasicPropertiesWithIndex()
    {
        DatProperty property = new DatProperty("Property", null!, default, SpecPropertyContext.Property) { Type = StringType.Instance };


        Assert.That(DataRefs.TryReadDataRef("#This.Indices[-1]", null, property, out IDataRef? dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IndicesProperty>>());
        Assert.That(((DataRefProperty<IndicesProperty>)dataRef).Property.Index, Is.EqualTo(-1));
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#This.(Indices)[1]", null, property, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IndicesProperty>>());
        Assert.That(((DataRefProperty<IndicesProperty>)dataRef).Property.Index, Is.EqualTo(1));
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);

        Assert.That(DataRefs.TryReadDataRef("#This.Indices", null, property, out dataRef));

        Assert.That(dataRef, Is.InstanceOf<DataRefProperty<IndicesProperty>>());
        Assert.That(((DataRefProperty<IndicesProperty>)dataRef).Property.Index, Is.Null);
        Assert.That(dataRef.Target, Is.InstanceOf<ThisDataRef>());
        Console.WriteLine(dataRef);
    }


    public struct TestDataRefContext : IDataRefReadContext
    {
        /// <inheritdoc />
        public bool TryReadTarget(ReadOnlySpan<char> root, IType? type, DatProperty owner, [NotNullWhen(true)] out IDataRefTarget? target)
        {
            switch (root.ToString())
            {
                case "Value":
                    target = new ValueDataRef<string>(StringType.Instance);
                    return true;

                case "Index":
                    target = new IndexDataRef<int>(Int32Type.Instance);
                    return true;

                case "Key":
                    target = new KeyDataRef<string>(StringType.Instance);
                    return true;

                default:
                    Assert.Fail("Unknown data-ref type (shouldn't be reached).");
                    target = null;
                    return false;
            }
        }
    }
}

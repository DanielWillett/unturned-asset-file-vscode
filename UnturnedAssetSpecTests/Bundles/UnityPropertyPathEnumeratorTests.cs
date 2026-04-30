using DanielWillett.UnturnedDataFileLspServer.Data.Values;

namespace UnturnedAssetSpecTests.Bundles;

[TestFixture]
public class UnityPropertyPathEnumeratorTests
{
    [Test]
    [TestCase("m_Components")]
    [TestCase("m_Components.")]
    public void TestBasic(string text)
    {
        UnityPropertyPathEnumerator enumerator = new UnityPropertyPathEnumerator(text);
        
        Assert.That(enumerator.MoveNext());

        Assert.That(enumerator.Index, Is.Null);
        Assert.That(enumerator.Property, Is.EqualTo("m_Components"));

        Assert.That(enumerator.MoveNext(), Is.False);
    }

    [Test]
    [TestCase("m_Components[0]")]
    [TestCase("m_Components[ 0]")]
    [TestCase("m_Components[0 ]")]
    [TestCase("m_Components[ 0 ]")]
    [TestCase("m_Components[ 0 ].")]
    public void TestBasicWithIndexFromStart(string text)
    {
        UnityPropertyPathEnumerator enumerator = new UnityPropertyPathEnumerator(text);
        
        Assert.That(enumerator.MoveNext());

        Assert.That(enumerator.Index, Is.Not.Null);
        Assert.That(enumerator.Index.Value.IsFromEnd, Is.False);
        Assert.That(enumerator.Index.Value.Value, Is.Zero);
        Assert.That(enumerator.Property, Is.EqualTo("m_Components"));

        Assert.That(enumerator.MoveNext(), Is.False);
    }

    [Test]
    [TestCase("m_Components[^1]")]
    [TestCase("m_Components[^ 1]")]
    [TestCase("m_Components[^1 ]")]
    [TestCase("m_Components[^ 1 ]")]
    [TestCase("m_Components[ ^1]")]
    [TestCase("m_Components[ ^ 1]")]
    [TestCase("m_Components[ ^1 ]")]
    [TestCase("m_Components[ ^ 1 ]")]
    [TestCase("m_Components[^ 1]")]
    [TestCase("m_Components[^  1]")]
    [TestCase("m_Components[^ 1 ]")]
    [TestCase("m_Components[^  1 ]")]
    [TestCase("m_Components[ ^ 1]")]
    [TestCase("m_Components[ ^  1]")]
    [TestCase("m_Components[ ^ 1 ]")]
    [TestCase("m_Components[ ^  1 ]")]
    [TestCase("m_Components[^1].")]
    public void TestBasicWithIndexFromEnd(string text)
    {
        UnityPropertyPathEnumerator enumerator = new UnityPropertyPathEnumerator(text);
        
        Assert.That(enumerator.MoveNext());

        Assert.That(enumerator.Index, Is.Not.Null);
        Assert.That(enumerator.Index.Value.IsFromEnd, Is.True);
        Assert.That(enumerator.Index.Value.Value, Is.EqualTo(1));
        Assert.That(enumerator.Property, Is.EqualTo("m_Components"));

        Assert.That(enumerator.MoveNext(), Is.False);
    }

    private const int NoIndex = int.MinValue;

    [Test]
    [TestCase("m_Components[0].m_Enabled",              new string[] { "m_Components", "m_Enabled" },               new int[] { 0, NoIndex })]
    [TestCase("m_Components[^1].m_Enabled",             new string[] { "m_Components", "m_Enabled" },               new int[] { -1, NoIndex })]
    [TestCase("m_Components.m_Enabled",                 new string[] { "m_Components", "m_Enabled" },               new int[] { NoIndex, NoIndex })]
    [TestCase("m_Components[0].m_GameObject.m_Test",    new string[] { "m_Components", "m_GameObject", "m_Test" },  new int[] { 0, NoIndex, NoIndex })]
    // error cases
    [TestCase("m_Components[0]..m_Enabled",             new string[] { "m_Components", "m_Enabled" },               new int[] { 0, NoIndex })]
    [TestCase("m_Components[0]...m_Enabled",            new string[] { "m_Components", "m_Enabled" },               new int[] { 0, NoIndex })]
    [TestCase("m_Components[^1]..m_Enabled",            new string[] { "m_Components", "m_Enabled" },               new int[] { -1, NoIndex })]
    [TestCase("m_Components[^1]...m_Enabled",           new string[] { "m_Components", "m_Enabled" },               new int[] { -1, NoIndex })]
    [TestCase("m_Components..m_Enabled",                new string[] { "m_Components", "m_Enabled" },               new int[] { NoIndex, NoIndex })]
    [TestCase("m_Components...m_Enabled",               new string[] { "m_Components", "m_Enabled" },               new int[] { NoIndex, NoIndex })]
    [TestCase("m_Components[.m_GameObject.m_Test",      new string[] { "m_Components[", "m_GameObject", "m_Test" }, new int[] { NoIndex, NoIndex, NoIndex })]
    [TestCase("m_Components[^.m_GameObject",            new string[] { "m_Components[^", "m_GameObject" },          new int[] { NoIndex, NoIndex })]
    [TestCase("m_Components[0.m_GameObject",            new string[] { "m_Components[0", "m_GameObject" },          new int[] { NoIndex, NoIndex })]
    [TestCase("m_Components[^1.m_GameObject",           new string[] { "m_Components[^1", "m_GameObject" },         new int[] { NoIndex, NoIndex })]
    [TestCase("m_Components[",                          new string[] { "m_Components[" },                           new int[] { NoIndex })]
    [TestCase("m_Components[^",                         new string[] { "m_Components[^" },                          new int[] { NoIndex })]
    [TestCase("m_Components[0",                         new string[] { "m_Components[0" },                          new int[] { NoIndex })]
    [TestCase("m_Components[^1",                        new string[] { "m_Components[^1" },                         new int[] { NoIndex })]
    public void TestMultiple(string text, string[] properties, int[] indices)
    {
        UnityPropertyPathEnumerator enumerator = new UnityPropertyPathEnumerator(text);
        
        for (int i = 0; i < properties.Length; ++i)
        {
            Assert.That(enumerator.MoveNext());

            int expectedIndex = indices[i];
            if (expectedIndex == NoIndex)
            {
                Assert.That(enumerator.Index, Is.Null);
            }
            else if (expectedIndex < 0)
            {
                expectedIndex = -expectedIndex;
                Assert.That(enumerator.Index, Is.Not.Null);
                Assert.That(enumerator.Index.Value.IsFromEnd, Is.True);
                Assert.That(enumerator.Index.Value.Value, Is.EqualTo(expectedIndex));
            }
            else
            {
                Assert.That(enumerator.Index, Is.Not.Null);
                Assert.That(enumerator.Index.Value.IsFromEnd, Is.False);
                Assert.That(enumerator.Index.Value.Value, Is.EqualTo(expectedIndex));
            }

            string expectedProperty = properties[i];
            Assert.That(enumerator.Property, Is.EqualTo(expectedProperty));
        }

        Assert.That(enumerator.MoveNext(), Is.False);
    }
}
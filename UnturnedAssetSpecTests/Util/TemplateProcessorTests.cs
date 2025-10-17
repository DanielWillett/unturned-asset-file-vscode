using System.Text.RegularExpressions;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests.Util;

[TestFixture]
public class TemplateProcessorTests
{
    [Test]
    [TestCase("Basic")]
    [TestCase("")]
    [TestCase("M")]
    [TestCase("My_Key")]
    public void BasicKey(string key)
    {
        string k2 = key;
        TemplateProcessor tp = TemplateProcessor.CreateForKey(ref k2);

        Assert.That(tp, Is.SameAs(TemplateProcessor.None));

        Assert.That(TemplateProcessor.EscapeKey(k2, tp), Is.EqualTo(key));

        Assert.That(tp.CreateKey(k2, OneOrMore<int>.Null), Is.EqualTo(key));
    }

    [Test]
    [TestCase("Separated_*_Key")]
    [TestCase("Separated_*")]
    [TestCase("*_Separated")]
    [TestCase("*")]
    [TestCase("Separated_*_Key_*_With_Multiple_*")]
    public void SeparatedKey(string key)
    {
        string k2 = key;
        TemplateProcessor tp = TemplateProcessor.CreateForKey(ref k2);

        Assert.That(tp, Is.Not.SameAs(TemplateProcessor.None));

        Assert.That(k2, Is.EqualTo(key.Replace('*', '#')));
        Assert.That(TemplateProcessor.EscapeKey(k2, tp), Is.EqualTo(key));

        OneOrMore<int> ind = Enumerable.Range(0, key.Count(x => x == '*')).ToArray();

        string createdKey = tp.CreateKey(k2, ind);
        Assert.That(Regex.IsMatch(createdKey, key.Replace("*", "(\\d+)")));
        Assert.That(Regex.IsMatch(tp.CreateKey(k2, ind.ToArray().AsSpan()), key.Replace("*", "(\\d+)")));

        Span<int> sp = stackalloc int[tp.TemplateCount];
        Assert.That(tp.TryParseKeyValues(createdKey, sp), Is.True);

        Assert.That(ind, Is.EqualTo(OneOrMoreExtensions.Create(sp)));
    }

    [Test]
    public void EscapedKey()
    {
        const string key = @"Test\With_*_\*_\\*_Others";
        const string keyConverted = @"Test\With_#_*_\#_Others";

        string k2 = key;
        TemplateProcessor tp = TemplateProcessor.CreateForKey(ref k2);

        Assert.That(tp, Is.Not.SameAs(TemplateProcessor.None));

        Assert.That(k2, Is.EqualTo(keyConverted));
        Assert.That(TemplateProcessor.EscapeKey(k2, tp), Is.EqualTo(key));

        OneOrMore<int> ind = [ 1, 2 ];

        string createdKey = tp.CreateKey(k2, ind);
        Assert.That(createdKey, Is.EqualTo(@"Test\With_1_*_\2_Others"));
        Assert.That(tp.CreateKey(k2, ind.ToArray().AsSpan()), Is.EqualTo(@"Test\With_1_*_\2_Others"));

        Span<int> sp = stackalloc int[tp.TemplateCount];
        Assert.That(tp.TryParseKeyValues(createdKey, sp), Is.True);

        Assert.That(ind, Is.EqualTo(OneOrMoreExtensions.Create(sp)));
    }

    [Test]
    public void EscapedKeyOneCharacter()
    {
        const string key = @"\*";
        const string keyConverted = @"*";

        string k2 = key;
        TemplateProcessor tp = TemplateProcessor.CreateForKey(ref k2);

        Assert.That(tp, Is.Not.SameAs(TemplateProcessor.None));

        Assert.That(k2, Is.EqualTo(keyConverted));
        Assert.That(TemplateProcessor.EscapeKey(k2, tp), Is.EqualTo(key));

        Assert.That(tp.CreateKey(k2, OneOrMore<int>.Null), Is.EqualTo(@"*"));
        Assert.That(tp.CreateKey(k2, ReadOnlySpan<int>.Empty), Is.EqualTo(@"*"));

        Span<int> sp = stackalloc int[tp.TemplateCount];
        Assert.That(tp.TryParseKeyValues(keyConverted, sp), Is.True);
        Assert.That(sp.Length, Is.EqualTo(0));
    }

    [Test]
    public void EscapedKeyTrailingSlash()
    {
        const string key = @"Test_*_Name\";
        const string keyConverted = @"Test_#_Name\";

        string k2 = key;
        TemplateProcessor tp = TemplateProcessor.CreateForKey(ref k2);

        Assert.That(tp, Is.Not.SameAs(TemplateProcessor.None));

        Assert.That(k2, Is.EqualTo(keyConverted));
        Assert.That(TemplateProcessor.EscapeKey(k2, tp), Is.EqualTo(key));

        OneOrMore<int> indices = [ 1 ];

        string createdKey = tp.CreateKey(k2, indices);
        Assert.That(createdKey, Is.EqualTo(@"Test_1_Name\"));
        Assert.That(tp.CreateKey(k2, indices.ToArray().AsSpan()), Is.EqualTo(@"Test_1_Name\"));

        Span<int> sp = stackalloc int[tp.TemplateCount];
        Assert.That(tp.TryParseKeyValues(createdKey, sp), Is.True);

        Assert.That(indices, Is.EqualTo(OneOrMoreExtensions.Create(sp)));
    }
}

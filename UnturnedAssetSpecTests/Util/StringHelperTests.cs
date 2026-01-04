using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests.Util;

[TestFixture]
public class StringHelperTests
{
    [Test]
    [TestCase("Unable to initialize Steam network authentication.", -1)]
    [TestCase("String {{n}}.", -1)]
    [TestCase("String {{0}}.", -1)]
    [TestCase("String {{{{n}}}}.", -1)]
    [TestCase("String {{{{0}}}}.", -1)]
    [TestCase("String {{{0}}}.", 0)]
    [TestCase("String {{{n}}}.", -1)]
    [TestCase("String {0}.", 0)]
    [TestCase("String {n}.", -1)]
    [TestCase("String {0}", 0)]
    [TestCase("Snap [{0}] Move [{1}] Build [{2}]", 2)]
    [TestCase("Snap [{0}] Move [{2}] Build [{1}]", 2)]
    [TestCase("Snap [{2}] Move [{1}] Build [{0}]", 2)]
    [TestCase("{0}", 0)]
    [TestCase("{0}{1}", 1)]
    [TestCase("{1}{0}", 1)]
    [TestCase("String {0:N2}", 0)]
    [TestCase("{0:N2} String", 0)]
    [TestCase("{0:N2}", 0)]
    [TestCase("{0:N2}{1:P}", 1)]
    [TestCase("{1:N2}{0:P}", 1)]
    public void TestGetMaxFormatArg(string str, int expected)
    {
        int max = StringHelper.GetHighestFormattingArgument(str);

        Assert.That(max, Is.EqualTo(expected));
    }
}

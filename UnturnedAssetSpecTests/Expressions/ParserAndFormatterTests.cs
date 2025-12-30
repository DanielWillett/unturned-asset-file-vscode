using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System.Text;

namespace UnturnedAssetSpecTests.Expressions;

[TestFixture]
public class ParserAndFormatterTests
{
    [Test]
    [TestCase(@"CAT(())")]
    [TestCase(@"ABS(1)")]
    [TestCase(@"ABS(-1)")]
    [TestCase(@"ABS(@Property)")]
    [TestCase(@"ABS(@SDG.Unturned.ItemAsset::Property)")]
    [TestCase(@"ABS(@$prop$::SDG.Unturned.ItemAsset::Property)")]
    [TestCase(@"ABS(@(Property))")]
    [TestCase(@"ABS(@(SDG.Unturned.ItemAsset::Property))")]
    [TestCase(@"ABS(@($prop$::SDG.Unturned.ItemAsset::Property))")]
    [TestCase(@"CAT(str (ing with space))")]
    [TestCase(@"CAT((str) (ing with space))")]
    [TestCase(@"REP((base stri\\g) \\ n)")]
    [TestCase(@"REP((base stri\\g) (\\) (n))")]
    [TestCase(@"REP(=CAT(=CAT(a b c) b) \\ n)")]
    [TestCase(@"REP(=CAT(=CAT((a) (b) (c)) (b)) (\\) (n))")]
    [TestCase(@"ABS(3f)")]
    [TestCase(@"ABS(3d)")]
    [TestCase(@"ABS(3ul)")]
    [TestCase(@"ABS(3u)")]
    [TestCase(@"ABS(3l)")]
    [TestCase(@"ABS(3)")]
    [TestCase(@"ABS((3f))")]
    [TestCase(@"ABS((3d))")]
    [TestCase(@"ABS((3ul))")]
    [TestCase(@"ABS((3u))")]
    [TestCase(@"ABS((3l))")]
    [TestCase(@"ABS((3))")]
    public void ConsistancyTest(string input)
    {
        using ExpressionNodeParser parser = new ExpressionNodeParser(input, false);

        IExpressionNode node = parser.Parse<double>();
        Assert.That(node, Is.AssignableTo<IFunctionExpressionNode>());

        StringBuilder sb = new StringBuilder();
        ExpressionNodeFormatter formatter = new ExpressionNodeFormatter(sb);

        formatter.WriteExpression((IFunctionExpressionNode)node);

        string str = sb.ToString();
        Console.WriteLine(str);

        using ExpressionNodeParser parser2 = new ExpressionNodeParser(input, false);

        IExpressionNode node2 = parser2.Parse<double>();

        Assert.That(node2, Is.EqualTo(node));
    }
}

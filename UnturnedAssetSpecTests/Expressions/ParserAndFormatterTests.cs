using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System.Text;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;

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
    public unsafe void ConsistancyTest(string input)
    {
        DataRefs.NilDataRefContext c;
#if NET7_0_OR_GREATER
        using ExpressionNodeParser<DataRefs.NilDataRefContext> parser = new ExpressionNodeParser<DataRefs.NilDataRefContext>(input, null!, ref c, false);
#else
        using ExpressionNodeParser<DataRefs.NilDataRefContext> parser = new ExpressionNodeParser<DataRefs.NilDataRefContext>(input, null!, &c, false);
#endif

        IExpressionNode node = parser.Parse<double>();
        Assert.That(node, Is.AssignableTo<IFunctionExpressionNode>());

        StringBuilder sb = new StringBuilder();
        ExpressionNodeFormatter formatter = new ExpressionNodeFormatter(sb);

        formatter.WriteExpression((IFunctionExpressionNode)node);

        string str = sb.ToString();
        Console.WriteLine(str);

#if NET7_0_OR_GREATER
        using ExpressionNodeParser<DataRefs.NilDataRefContext> parser2 = new ExpressionNodeParser<DataRefs.NilDataRefContext>(input, null!, ref c, false);
#else
        using ExpressionNodeParser<DataRefs.NilDataRefContext> parser2 = new ExpressionNodeParser<DataRefs.NilDataRefContext>(input, null!, &c, false);
#endif

        IExpressionNode node2 = parser2.Parse<double>();

        Assert.That(node2, Is.EqualTo(node));
    }
}

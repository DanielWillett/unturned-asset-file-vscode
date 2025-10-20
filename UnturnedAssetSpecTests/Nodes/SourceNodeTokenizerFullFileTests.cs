using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

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
        public ISourceFile SourceFile => null;

        /// <inheritdoc />
        public string GetFullText() => null;
    }
}

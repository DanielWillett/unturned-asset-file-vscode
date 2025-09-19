using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace UnturnedAssetSpecTests.Nodes;

public class SourceNodeTokenizerFullFileTests
{
    private IAssetSpecDatabase _database;

    [SetUp]
    public void Setup()
    {
        _database = AssetSpecDatabase.FromOffline();
    }

    [TearDown]
    public void Teardown()
    {
        if (_database is IDisposable d)
            d.Dispose();
    }

    [Test]
    public void File1([Values(SourceNodeTokenizerOptions.Lazy, SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.Lazy | SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.None)] SourceNodeTokenizerOptions options)
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
    "Key
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
""";

        SourceNodeTokenizer tok = new SourceNodeTokenizer(file, options);

        ISourceFile sourceFile = tok.ReadRootDictionary(SourceNodeTokenizer.RootInfo.Asset(new TestWorkspaceFile(), _database));

        StringWriter sw = new StringWriter();
        NodeWriteToTextWriterVisitor visitor = new NodeWriteToTextWriterVisitor(sw);

        sourceFile.Visit(ref visitor);

        Console.WriteLine(sw.ToString());
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
    }
}

using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

public class OpenedFile : IWorkspaceFile
{
    private string _text;
    private AssetFileTree? _tree;
    private FileLineIndex _index;

    public AssetFileTree File
    {
        get
        {
            if (_tree != null)
                return _tree;

            DatTokenizer tokenizer = new DatTokenizer(_text);
            return _tree = AssetFileTree.Create(ref tokenizer);
        }
    }

    public FileLineIndex LineIndex
    {
        get
        {
            if (_index.IsValid)
                return _index;

            _index = new FileLineIndex(_text);
            return _index;
        }
    }

    public DocumentUri Uri { get; }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            _tree = null;
            _index = default;
        }
    }

    public OpenedFile(DocumentUri uri, string text)
    {
        Uri = uri;
        _text = text;
    }

    void IDisposable.Dispose() { }
}
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LspServer.Files;

internal class OpenedFile
{
    private string _text;
    public AssetFileTree? _tree;

    public AssetFileTree Tree
    {
        get
        {
            if (_tree != null)
                return _tree;

            DatTokenizer tokenizer = new DatTokenizer(_text);
            return _tree = AssetFileTree.Create(ref tokenizer);
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
        }
    }

    public OpenedFile(DocumentUri uri, string text)
    {
        Uri = uri;
        _text = text;
    }

    public int GetIndex(Position position)
    {
        int index = 0;
        for (int i = 0; i < position.Line; i++)
        {
            index = _text.IndexOf('\n', index) + 1;
        }

        return index + position.Character;
    }
}
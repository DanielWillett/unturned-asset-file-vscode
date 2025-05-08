using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;

public readonly struct AutoCompleteParameters
{
    public AssetSpecDatabase Database { get; }
    public AssetFileTree File { get; }
    public FilePosition Position { get; }
    public AssetFileType FileType { get; }
    public SpecProperty Property { get; }

    public AutoCompleteParameters(AssetSpecDatabase database, AssetFileTree file, FilePosition position, AssetFileType fileType, SpecProperty property)
    {
        Database = database;
        File = file;
        Position = position;
        FileType = fileType;
        Property = property;
    }
}

using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public struct FileEvaluationContext
{
    internal static readonly FileEvaluationContext None = default;

    public readonly IParsingServices Services;
    public readonly ISourceFile File;
    public readonly AssetFileType FileType;

    public FileEvaluationContext(IParsingServices services, ISourceFile sourceFile)
    {
        Services = services;
        File = sourceFile;
        FileType = AssetFileType.FromFile(sourceFile, services.Database);
    }

    public readonly IFileRelationalModel GetRelationalModel(SpecPropertyContext context = SpecPropertyContext.Property)
    {
        return Services.RelationalModelProvider.GetProvider(File, context);
    }

    public readonly bool TryGetRelevantMap([NotNullWhen(true)] out RelevantMapInfo? mapInfo)
    {
        // todo
        mapInfo = null;
        return false;
    }
}

public class RelevantMapInfo;
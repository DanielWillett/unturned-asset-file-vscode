using DanielWillett.UnturnedDataFileLspServer.Data.Project;

namespace DanielWillett.UnturnedDataFileLspServer.Project;

internal class LspProjectFile(string filePath, string folderPath) : ProjectFile(filePath)
{
    public string FolderPath { get; internal set; } = folderPath;

    internal ScaffoldedPropertyOrderFile? EffectiveOrderFile;
}
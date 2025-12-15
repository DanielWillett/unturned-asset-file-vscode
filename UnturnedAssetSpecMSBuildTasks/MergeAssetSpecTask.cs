using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace UnturnedAssetSpecMSBuildTasks;

/// <summary>
/// Merges all the asset spec JSON files into one file so it's quicker to download.
/// </summary>
public class MergeAssetSpecTask : Task
{
    [Required]
    public string Folder { get; set; }
    
    [Required]
    public string OutputFile { get; set; }
    
    [Required]
    public string[] Blacklist { get; set; }
    
    [Output]
    public string MergedFile { get; set; }

    private struct AssetFileInfo
    {
        public string FullPath;
        public string FileName;
        public int DataPos;
        public int FileNameSize;
        public long Length;
        public long Offset;
    }

    public override unsafe bool Execute()
    {
        // files to not include in the merged file.
        string folder = Path.GetFullPath(Folder);

        List<string> allFiles = Directory.EnumerateFiles(folder, "*.json", SearchOption.TopDirectoryOnly).Where(x => Array.FindIndex(Blacklist, v => Path.GetFileName(x) == v) < 0).ToList();

        if (allFiles.Count == 0)
        {
            Log.LogError("No files found in asset spec folder \"{0}\".", folder);
            return false;
        }

        AssetFileInfo[] fileInfo = new AssetFileInfo[allFiles.Count];
        int totalFileNameSize = 0, maxFileNameSize = 0;
        for (int i = 0; i < fileInfo.Length; ++i)
        {
            ref AssetFileInfo info = ref fileInfo[i];
            info.FileName = Path.GetFileName(allFiles[i]);
            info.FullPath = allFiles[i];
            info.FileNameSize = Encoding.UTF8.GetByteCount(info.FileName);
            maxFileNameSize = Math.Max(maxFileNameSize, info.FileName.Length);
            totalFileNameSize += info.FileNameSize;
        }

        Log.LogMessage(MessageImportance.Low, "Found {0} file(s) in asset spec folder \"{1}\".", allFiles.Count, Folder);

        string outputFilePath = Path.GetFullPath(OutputFile);

        maxFileNameSize -= folder.Length + 1;

        int headerSize = 17 + 18 * allFiles.Count + totalFileNameSize;

        // hdr:        [version:4B][maxFileNameSize:4B][fileCt:4B][hdrSize:4B][\n]
        // hdr x file: [fn size:1B][file name:UTF-8][offset:8B][size:8B][\n]

        byte[] buffer = new byte[Math.Max(2048, headerSize)];
        int headerIndex = 17;
        long totalFileSize;
        fixed (byte* ptr = buffer)
        {
            using FileStream fs = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.Read, 8192, FileOptions.RandomAccess);

            // version
            // buffer[0] = 0;
            // buffer[1] = 0;
            // buffer[2] = 0;
            // buffer[3] = 0;

            *(int*)(ptr + 4) = maxFileNameSize;
            *(int*)(ptr + 8) = allFiles.Count;
            *(ptr + 16) = (byte)'\n';

            for (int i = 0; i < fileInfo.Length; i++)
            {
                ref AssetFileInfo info = ref fileInfo[i];
                int lPos = headerIndex;
                ++headerIndex;
                fixed (char* fnPtr = info.FileName)
                {
                    int sz = Encoding.UTF8.GetBytes(fnPtr, info.FileName.Length, ptr + headerIndex, headerSize - headerIndex);
                    if (sz > byte.MaxValue)
                    {
                        Log.LogError("File name \"{0}\" is longer than 255 bytes.", info.FileName);
                        return false;
                    }

                    headerIndex += sz;
                    *(ptr + lPos) = (byte)sz;
                }

                info.DataPos = headerIndex;
                headerIndex += 16;
                *(ptr + headerIndex) = (byte)'\n';
                ++headerIndex;
            }

            *(int*)(ptr + 12) = headerIndex;

            fs.Write(buffer, 0, headerIndex);

            long offset = headerIndex;
            for (int i = 0; i < fileInfo.Length; i++)
            {
                ref AssetFileInfo info = ref fileInfo[i];

                using FileStream mergedFileStream = new FileStream(info.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 2048, FileOptions.SequentialScan);

                using (JsonDocument document = JsonDocument.Parse(mergedFileStream, new JsonDocumentOptions
                {
                    MaxDepth = 16,
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Skip
                }))
                using (Utf8JsonWriter writer = new Utf8JsonWriter(fs, new JsonWriterOptions
                {
                    Indented = false,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    SkipValidation = true
                }))
                {
                    document.WriteTo(writer);
                    writer.Flush();

                    info.Length = writer.BytesCommitted;
                }

                info.Offset = offset;
                offset += info.Length;
            }

            totalFileSize = offset;

            for (int i = 0; i < fileInfo.Length; i++)
            {
                ref AssetFileInfo info = ref fileInfo[i];
                fs.Seek(info.DataPos, SeekOrigin.Begin);
                *(long*)ptr = info.Offset;
                *(long*)(ptr + 8) = info.Length;
                fs.Write(buffer, 0, 16);
            }

            fs.Seek(0, SeekOrigin.End);
            fs.Dispose();
        }

        // GitHub doesn't let you download files > 1MB from the API
        if (totalFileSize > 1_000_000)
        {
            Log.LogError("Combined file size ({0} B) is too large. Whatever will we do.", totalFileSize.ToString("N"));
            return false;
        }

        OutputFile = outputFilePath;
        return true;
    }
}
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.FileSystemGlobbing.Internal.Patterns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class FilePathStringSpecPropertyType :
    BaseSpecPropertyType<string>,
    ISpecPropertyType<string>,
    IEquatable<FilePathStringSpecPropertyType?>,
    IElementTypeSpecPropertyType
{
    public static readonly FilePathStringSpecPropertyType Instance = new FilePathStringSpecPropertyType();

    private bool _hasPatternContext;
    private IPatternContext? _patternContext;
    private string? _patternFailMessage;

    public string? GlobPattern { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName => "File Path";

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "FilePathString";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public Type ValueType => typeof(string);

    string? IElementTypeSpecPropertyType.ElementType => GlobPattern;

    public FilePathStringSpecPropertyType(string? globPattern = null)
    {
        GlobPattern = globPattern;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out string? val))
        {
            value = null!;
            return false;
        }

        value = val == null ? SpecDynamicValue.Null : new SpecDynamicConcreteConvertibleValue<string>(val, this);
        return true;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
            return MissingNode(in parse, out value);

        if (parse.Node is not AssetFileStringValueNode stringNode)
            return FailedToParse(in parse, out value);

        string val = stringNode.Value;
        if (parse.HasDiagnostics && val.IndexOf('\\') >= 0)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Range = stringNode.Range,
                Diagnostic = DatDiagnostics.UNT1010,
                Message = DiagnosticResources.UNT1010
            });
        }

        if (parse.HasDiagnostics && GlobPattern != null)
        {
            if (!_hasPatternContext)
                BuildPatternContext(in parse);
            else if (_patternFailMessage != null)
            {
                parse.Log(new DatDiagnosticMessage { Range = parse.Node!.Range, Diagnostic = DatDiagnostics.UNT2005, Message = _patternFailMessage });
            }
            else if (_patternContext != null)
            {
                PatternTestResult result = _patternContext.Test(new VirutalFileInfo(val));
                if (!result.IsSuccessful)
                {
                    parse.Log(new DatDiagnosticMessage
                    {
                        Range = stringNode.Range,
                        Diagnostic = DatDiagnostics.UNT1011,
                        Message = string.Format(DiagnosticResources.UNT1011, GlobPattern)
                    });
                }
            }
        }

        value = val;
        return true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void BuildPatternContext(in SpecPropertyTypeParseContext parse)
    {
        PatternBuilder patternBuilder = new PatternBuilder(StringComparison.Ordinal);
        try
        {
            IPattern pattern = patternBuilder.Build(GlobPattern);
            _patternContext = pattern.CreatePatternContextForInclude();
        }
        catch (ArgumentException ex)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Range = parse.Node!.Range,
                Diagnostic = DatDiagnostics.UNT2005,
                Message = _patternFailMessage ??= string.Format(DiagnosticResources.UNT2005, "FilePathString: \"" + GlobPattern + "\"") + ex.Message
            });
            _patternContext = null;
        }

        _hasPatternContext = true;
    }

    /// <inheritdoc />
    public bool Equals(FilePathStringSpecPropertyType? other) => other != null && string.Equals(GlobPattern, other.GlobPattern, StringComparison.Ordinal);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is FilePathStringSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<string>? other) => other is FilePathStringSpecPropertyType t && Equals(t);

#nullable disable
    private class VirutalFileInfo : FileInfoBase
    {
        /// <inheritdoc />
        public override string Name { get; }

        /// <inheritdoc />
        public override string FullName { get; }

        /// <inheritdoc />
        public override DirectoryInfoBase ParentDirectory => field ??= new VirutalDirectoryInfo(GetDirectoryName(FullName));

        public VirutalFileInfo(string path)
        {
            FullName = path;
            Name = path.Length == 0 ? string.Empty : GetFileName(path);
        }
    }
    private class VirutalDirectoryInfo : DirectoryInfoBase
    {
        /// <inheritdoc />
        public override string Name { get; }

        /// <inheritdoc />
        public override string FullName { get; }

        /// <inheritdoc />
        public override DirectoryInfoBase ParentDirectory => field ??= new VirutalDirectoryInfo(GetDirectoryName(FullName));

        /// <inheritdoc />
        public override IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos()
        {
            return Enumerable.Empty<FileSystemInfoBase>();
        }

        /// <inheritdoc />
        public override DirectoryInfoBase GetDirectory(string path)
        {
            return path.Length == 0 ? this : new VirutalDirectoryInfo(path[0] == '/' ? FullName + path : FullName + "/" + path);
        }

        /// <inheritdoc />
        public override FileInfoBase GetFile(string path)
        {
            return path.Length == 0 ? new VirutalFileInfo(FullName + "/") : new VirutalFileInfo(path[0] == '/' ? FullName + path : FullName + "/" + path);
        }

        public VirutalDirectoryInfo(string path)
        {
            FullName = path;
            if (path.Length == 0)
                Name = string.Empty;
        }
    }
#nullable restore
    private static string GetDirectoryName(string fullName)
    {
        int slashIndex = fullName.LastIndexOf('/');
        if (slashIndex == fullName.Length - 1)
            return fullName.Substring(fullName.Length - 1);

        return slashIndex != -1 ? fullName.Substring(0, slashIndex) : fullName;
    }
    private static string GetFileName(string fullName)
    {
        int slashIndex = fullName.LastIndexOf('/');
        if (slashIndex == fullName.Length - 1)
            return string.Empty;

        return slashIndex != -1 ? fullName.Substring(slashIndex + 1) : fullName;
    }

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}
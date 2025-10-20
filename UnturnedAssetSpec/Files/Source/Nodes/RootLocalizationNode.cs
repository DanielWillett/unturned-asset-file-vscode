using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.IO;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal class RootLocalizationNode : RootDictionaryNode, ILocalizationSourceFile
{
    /// <inheritdoc />
    public string LanguageName { get; }

    /// <inheritdoc />
    public IAssetSourceFile Asset { get; }

    /// <inheritdoc />
    public override QualifiedType ActualType
    {
        get => Asset.ActualType;
        protected set => throw new NotSupportedException();
    }

    public static RootLocalizationNode Create(IWorkspaceFile file, IAssetSourceFile asset, IAssetSpecDatabase database, int count, ISourceNode[] nodes, in AnySourceNodeProperties properties)
    {
        return new RootLocalizationNode(file, asset, database, count, nodes, in properties);
    }

    /// <inheritdoc />
    private protected RootLocalizationNode(IWorkspaceFile file, IAssetSourceFile asset, IAssetSpecDatabase database, int count, ISourceNode[] nodes, in AnySourceNodeProperties properties)
        : base(file, database, count, nodes, in properties)
    {
        Asset = asset;
        LanguageName = GetInternedLanguageName(file.File);
    }

    private static string GetInternedLanguageName(string fullName)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        ReadOnlySpan<char> fn = Path.GetFileNameWithoutExtension(fullName.AsSpan());
#else
        int extIndex = fullName.LastIndexOf('.');
        ReadOnlySpan<char> ext = extIndex < 0 ? ReadOnlySpan<char>.Empty : fullName.AsSpan(extIndex + 1);
        int lastSlash = fullName.AsSpan().LastIndexOfAny([ Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar ]);
        if (lastSlash < -1)
            lastSlash = -1;
        ReadOnlySpan<char> fn = fullName.AsSpan(lastSlash + 1, fullName.Length - ext.Length - (lastSlash + 1));
#endif

        if (fn.IsEmpty)
            return string.Empty;

        switch (fn[0])
        {
            case 'A':
                if (fn.Equals("Arabic", StringComparison.Ordinal))
                    return "Arabic";
                break;

            case 'B':
                if (fn.Equals("Bulgarian", StringComparison.Ordinal))
                    return "Bulgarian";
                if (fn.Equals("Brazilian", StringComparison.Ordinal))
                    return "Brazilian";
                break;

            case 'C':
                if (fn.Equals("Czech", StringComparison.Ordinal))
                    return "Czech";
                break;

            case 'D':
                if (fn.Equals("Danish", StringComparison.Ordinal))
                    return "Danish";
                if (fn.Equals("Dutch", StringComparison.Ordinal))
                    return "Dutch";
                break;

            case 'E':
                if (fn.Equals("English", StringComparison.Ordinal))
                    return "English";
                break;

            case 'F':
                if (fn.Equals("Finnish", StringComparison.Ordinal))
                    return "Finnish";
                if (fn.Equals("French", StringComparison.Ordinal))
                    return "French";
                break;

            case 'G':
                if (fn.Equals("German", StringComparison.Ordinal))
                    return "German";
                if (fn.Equals("Greek", StringComparison.Ordinal))
                    return "Greek";
                break;

            case 'H':
                if (fn.Equals("Hungarian", StringComparison.Ordinal))
                    return "Hungarian";
                break;

            case 'I':
                if (fn.Equals("Indonesian", StringComparison.Ordinal))
                    return "Indonesian";
                if (fn.Equals("Italian", StringComparison.Ordinal))
                    return "Italian";
                break;

            case 'J':
                if (fn.Equals("Japanese", StringComparison.Ordinal))
                    return "Japanese";
                break;

            case 'K':
                if (fn.Equals("Koreana", StringComparison.Ordinal))
                    return "Koreana";
                break;

            case 'N':
                if (fn.Equals("Norwegian", StringComparison.Ordinal))
                    return "Norwegian";
                break;

            case 'P':
                if (fn.Equals("Polish", StringComparison.Ordinal))
                    return "Polish";
                if (fn.Equals("Portuguese", StringComparison.Ordinal))
                    return "Portuguese";
                break;

            case 'R':
                if (fn.Equals("Romanian", StringComparison.Ordinal))
                    return "Romanian";
                if (fn.Equals("Russian", StringComparison.Ordinal))
                    return "Russian";
                break;

            case 'L':
                if (fn.Equals("Latam", StringComparison.Ordinal))
                    return "Latam";
                break;

            case 'S':
                if (fn.Equals("Schinese", StringComparison.Ordinal))
                    return "Schinese";
                if (fn.Equals("Spanish", StringComparison.Ordinal))
                    return "Spanish";
                if (fn.Equals("Swedish", StringComparison.Ordinal))
                    return "Swedish";
                break;

            case 'T':
                if (fn.Equals("Tchinese", StringComparison.Ordinal))
                    return "Tchinese";
                if (fn.Equals("Thai", StringComparison.Ordinal))
                    return "Thai";
                if (fn.Equals("Turkish", StringComparison.Ordinal))
                    return "Turkish";
                break;

            case 'U':
                if (fn.Equals("Ukrainian", StringComparison.Ordinal))
                    return "Ukrainian";
                break;

            case 'V':
                if (fn.Equals("Vietnamese", StringComparison.Ordinal))
                    return "Vietnamese";
                break;
        }

        return fn.ToString();
    }
}
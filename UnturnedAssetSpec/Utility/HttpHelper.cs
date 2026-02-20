using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class HttpHelper
{
#if NET6_0_OR_GREATER
    public static Version LatestVersion => HttpVersion.Version30;
#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
    public static Version LatestVersion => HttpVersion.Version20;
#else
    public static Version LatestVersion => HttpVersion.Version11;
#endif

    private static string? _userAgent;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void AddUserAgentHeader(HttpRequestMessage msg)
    {
        _userAgent ??= $"unturned-asset-file-vscode/{Assembly.GetExecutingAssembly().GetName().Version!.ToString(3)}";

        msg.Headers.Add("User-Agent", _userAgent);
    }
}

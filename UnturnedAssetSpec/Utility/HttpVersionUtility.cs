using System;
using System.Net;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class HttpVersionUtility
{
#if NET6_0_OR_GREATER
    public static Version LatestVersion => HttpVersion.Version30;
#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
    public static Version LatestVersion => HttpVersion.Version20;
#else
    public static Version LatestVersion => HttpVersion.Version11;
#endif
}

using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class PlatformLockHelper
{
#if NET9_0_OR_GREATER
    internal static void EnterLock(Lock @lock, ref bool hasLock)
    {
        try
        {
            @lock.Enter();
        }
        catch
        {
            hasLock = false;
            throw;
        }
        finally
        {
            hasLock = true;
        }
    }

    internal static void ExitLock(Lock @lock)
    {
        @lock.Exit();
    }
#else
    internal static void EnterLock(object @lock, ref bool hasLock)
    {
        Monitor.Enter(@lock, ref hasLock);
    }

    internal static void ExitLock(object @lock)
    {
        Monitor.Exit(@lock);
    }
#endif
}

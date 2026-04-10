namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class PlatformLockHelper
{
#if NET9_0_OR_GREATER
    internal static void EnterLock(TfmLock @lock, ref bool hasLock)
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

    internal static void ExitLock(TfmLock @lock)
    {
        @lock.Exit();
    }
#else
    internal static void EnterLock(TfmLock @lock, ref bool hasLock)
    {
        System.Threading.Monitor.Enter(@lock, ref hasLock);
    }

    internal static void ExitLock(TfmLock @lock)
    {
        System.Threading.Monitor.Exit(@lock);
    }
#endif
}

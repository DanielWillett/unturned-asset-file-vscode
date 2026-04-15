using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// <see cref="IBundleCache"/> implementation using instance tracking to unload <see cref="DiscoveredBundle"/> instances as they stop being used.
/// </summary>
public class MasterBundleCache : IBundleCache, IDisposable
{
    private readonly TfmLock _lock = new TfmLock();
    private readonly Dictionary<string, BundleRecord> _cachedBundles;
    private readonly Action<string, string> _logAction;
    private bool _disposed;

    public MasterBundleCache(ILogger<MasterBundleCache> logger)
    {
        _cachedBundles = new Dictionary<string, BundleRecord>(OSPathHelper.PathComparer);
        _logAction = (file, msg) =>
        {
            logger.LogInformation($"{{0}} - {msg}", file);
        };
    }

    /// <inheritdoc />
    public bool TryGetCachedMasterBundle(string configPath, [NotNullWhen(true)] out DiscoveredBundleReference? bundle)
    {
        lock (_lock)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MasterBundleCache));

            if (!_cachedBundles.TryGetValue(configPath, out BundleRecord? record))
            {
                bundle = null;
                return false;
            }

            Interlocked.Increment(ref record.Users);
            bundle = new DiscoveredBundleReference(record.Bundle, this);
            return true;
        }
    }

    /// <inheritdoc />
    public DiscoveredBundleReference? GetOrAddMasterBundle(string configPath)
    {
        try
        {
            lock (_lock)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(MasterBundleCache));

                if (_cachedBundles.TryGetValue(configPath, out BundleRecord? record))
                {
                    Interlocked.Increment(ref record.Users);
                    try
                    {
                        return new DiscoveredBundleReference(record.Bundle, this);
                    }
                    catch
                    {
                        if (Interlocked.Decrement(ref record.Users) <= 0)
                        {
                            record.Bundle.Dispose();
                            _cachedBundles.Remove(configPath);
                        }
                        throw;
                    }
                }

                DiscoveredBundle? bundle = DiscoveredBundle.FromMasterBundleConfig(configPath, log: _logAction);
                if (bundle == null)
                {
                    return null;
                }

                try
                {
                    record = new BundleRecord(bundle);
                }
                catch
                {
                    bundle.Dispose();
                    throw;
                }

                try
                {
                    _cachedBundles.Add(bundle.ConfigurationFile, record);
                    return new DiscoveredBundleReference(bundle, this);
                }
                catch
                {
                    if (Interlocked.Decrement(ref record.Users) <= 0)
                    {
                        bundle.Dispose();
                        _cachedBundles.Remove(configPath);
                    }
                    throw;
                }
            }
        }
        catch (SecurityException ex)
        {
            throw new IOException("Failed to read file due to a permission issue.", ex);
        }
    }

    /// <inheritdoc />
    public void ReturnMasterBundle(DiscoveredBundle bundle)
    {
        if (_disposed)
            return;

        string path = bundle.ConfigurationFile;
        lock (_lock)
        {
            if (!_cachedBundles.TryGetValue(path, out BundleRecord? b))
                return;

            int newVal = Interlocked.Decrement(ref b.Users);
            if (newVal > 0)
                return;

            _cachedBundles.Remove(path);
            b.Bundle.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (BundleRecord record in _cachedBundles.Values)
            {
                record.Bundle?.Dispose();
            }

            _cachedBundles.Clear();
        }
    }

    private sealed class BundleRecord
    {
        public readonly DiscoveredBundle Bundle;
        public int Users;

        public BundleRecord(DiscoveredBundle bundle, int users = 1)
        {
            Bundle = bundle;
            Users = users;
        }

        public override string? ToString() => Bundle.ToString();
    }
}
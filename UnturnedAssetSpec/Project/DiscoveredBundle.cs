using System;
using System.Diagnostics;
using System.Threading;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// A Bundle or MasterBundle file.
/// </summary>
public sealed class DiscoveredBundle : IDisposable
{
    private BundleFileInstance? _openedfile;
    private AssetsManager? _assetsManager;
    private InstallationEnvironment? _installationEnvironment;

    /// <summary>
    /// Whether or not the bundle is a legacy (unity3d) bundle.
    /// </summary>
    public bool IsLegacyBundle { get; }

    /// <summary>
    /// The directory this bundle applies to. This is the folder the bundle is in.
    /// </summary>
    public string Directory { get; }
    
    /// <summary>
    /// The file this bundle was discovered from.
    /// </summary>
    /// <remarks>
    /// For master bundles this is the <c>Masterbundle.dat</c> file.<br/>
    /// For legacy bundles this is the corresponding asset.
    /// </remarks>
    public string ConfigurationFile { get; }

    /// <summary>
    /// The file that contains the actual bundle contents.
    /// </summary>
    /// <remarks>For master bundles, this is the Windows masterbundle file.</remarks>
    public string BundleFile { get; }

    /// <summary>
    /// The version of this bundle.
    /// </summary>
    public int Version { get; }

    public DiscoveredBundle(bool isLegacyBundle, string directory, string configurationFile, string bundleFile, int version)
    {
        IsLegacyBundle = isLegacyBundle;
        Directory = directory;
        ConfigurationFile = configurationFile;
        BundleFile = bundleFile;
        Version = version;
    }

    public bool IsReferencingFile(string file)
    {
        if (BundleFile.Equals(file, OSPathHelper.PathComparison) || ConfigurationFile.Equals(file, OSPathHelper.PathComparison))
            return true;

        if (IsLegacyBundle)
            return false;

        ReadOnlySpan<char> directory = OSPathHelper.GetDirectoryName(file);
        if (!directory.Equals(Directory, OSPathHelper.PathComparison))
            return false;

        ReadOnlySpan<char> fileName = OSPathHelper.GetFileName(file);
        if (OSPathHelper.IsExtension(fileName, ".hash"))
        {
            fileName = fileName[..^5];
        }
        else if (OSPathHelper.IsExtension(fileName, ".manifest"))
        {
            fileName = fileName[..^9];
        }

        ReadOnlySpan<char> expectedFileName = OSPathHelper.GetFileName(BundleFile);
        return CompareFileNameWithPrefix(fileName, "_linux", expectedFileName) || CompareFileNameWithPrefix(fileName, "_mac", expectedFileName);
    }

    private static bool CompareFileNameWithPrefix(ReadOnlySpan<char> fileName, string prefix, ReadOnlySpan<char> expected)
    {
        if (fileName.Length - prefix.Length != expected.Length)
        {
            return false;
        }

        int startIndex = expected.IndexOf('.');
        if (!expected.Slice(0, startIndex).Equals(fileName.Slice(0, startIndex), OSPathHelper.PathComparison)
            || !expected.Slice(startIndex + 1).Equals(fileName.Slice(startIndex + prefix.Length + 1), OSPathHelper.PathComparison))
        {
            return false;
        }

        return fileName.Slice(startIndex, prefix.Length).Equals(prefix, OSPathHelper.PathComparison);
    }

    /// <summary>
    /// Opens a reader for the bundle file.
    /// </summary>
    /// <param name="parsingServices"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"><paramref name="parsingServices"/> has changed since the last time this was called.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="parsingServices"/>'s <see cref="InstallationEnvironment"/> has been disposed.</exception>
    public BundleFileInstance GetOrOpen(IParsingServices parsingServices)
    {
        InstallationEnvironment env = parsingServices.Installation;
        InstallationEnvironment? originalValue = Interlocked.CompareExchange(ref _installationEnvironment, env, null);

        if (originalValue != null && !ReferenceEquals(originalValue, env))
        {
            throw new InvalidOperationException("This file does not belong to the provided InstallationEnvironment.");
        }

        BundleFileInstance? file = _openedfile;
        if (file != null)
        {
            return file;
        }

        lock (env.AssetBundleLock)
        {
            if (_openedfile != null)
            {
                return _openedfile;
            }

            AssetsManager? assetsManager = env.AssetBundleManager;

            if (assetsManager == null)
            {
                assetsManager = _assetsManager ?? throw new ObjectDisposedException(nameof(InstallationEnvironment));
            }
            else
            {
                _assetsManager = assetsManager;
            }

            file = assetsManager.LoadBundleFile(BundleFile, false);
            _openedfile = file;
        }

        return file;
    }


    public void Dispose()
    {
        BundleFileInstance? file = Interlocked.Exchange(ref _openedfile, null);
        if (file == null)
            return;

        InstallationEnvironment? env = _installationEnvironment;
        if (env == null)
            return;

        lock (env.AssetBundleLock)
        {
            AssetsManager? manager = _assetsManager;
            try
            {
                if (manager == null)
                    file.file.Close();
                else
                    manager.UnloadBundleFile(file);
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            _assetsManager = null;
        }
    }
}

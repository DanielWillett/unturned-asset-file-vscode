using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

partial class OpenedFile
{
    /*
     * TODO: this doesn't work for files outside the folders loaded by InstallationEnvironment
     */

    private IBundleProxy? _intlBundleProxy;
    private bool _ownsBundleProxy;
    private int _lastBundleUpdateVersion;

    private bool _isListeningForFileRemoved;

    [MemberNotNull(nameof(_intlBundleProxy))]
    private void UpdateBundle(bool force = false)
    {
        // assume locked on EditLock
        if (_lastBundleUpdateVersion == ChangeVersion && !force && _intlBundleProxy != null)
        {
            return;
        }

        if (_ownsBundleProxy)
        {
            _ownsBundleProxy = false;
            (Interlocked.Exchange(ref _intlBundleProxy, null) as IDisposable)?.Dispose();
        }

        DiscoveredDatFile? datFile = _services.Installation.FindFile(File, matchLocalizationAlso: true);
        if (datFile != null)
        {
            if (!_isListeningForFileRemoved)
            {
                _isListeningForFileRemoved = true;
                _services.Installation.OnFileRemoved += OnFileRemoved;
            }

            _ = datFile.GetBundleProxy(_services);
            _intlBundleProxy = datFile;
            _ownsBundleProxy = false;
        }
        else
        {
            if (_isListeningForFileRemoved)
            {
                _services.Installation.OnFileRemoved -= OnFileRemoved;
                _isListeningForFileRemoved = false;
            }

            ISourceFile src = SourceFile;
            if (!_services.Database.FileTypes.TryGetValue(src.ActualType, out DatFileType? fileType)
                || fileType is not DatAssetFileType { HasBundleAssets: true })
            {
                _intlBundleProxy = IBundleProxy.Null;
                _ownsBundleProxy = false;
            }
            else
            {
                // todo: use IWorkspaceEnvironment.LoadBundleForAsset instead
                datFile = new DiscoveredDatFile(File, _contentSegment.AsSpan(), _services.Database, null, null);
                _ = datFile.GetBundleProxy(_services);
                _intlBundleProxy = datFile;
                _ownsBundleProxy = true;
            }
        }

        _lastBundleUpdateVersion = ChangeVersion;
    }

    private void OnFileRemoved(DiscoveredDatFile file)
    {
        if ((object)file != _intlBundleProxy || _ownsBundleProxy)
        {
            return;
        }

        lock (EditLock)
        {
            if ((object)file != _intlBundleProxy || _ownsBundleProxy)
            {
                return;
            }

            UpdateBundle(true);
        }
    }

    /// <summary>
    /// Bundle loaded for this asset file.
    /// </summary>
    public IBundleProxy Bundle => this;

    bool IBundleProxy.Exists
    {
        get
        {
            if (_intlBundleProxy == null || _lastBundleUpdateVersion != ChangeVersion)
            {
                lock (EditLock)
                    UpdateBundle();
            }
            return _intlBundleProxy.Exists;
        }
    }

    DiscoveredBundle? IBundleProxy.Bundle
    {
        get
        {
            if (_intlBundleProxy == null || _lastBundleUpdateVersion != ChangeVersion)
            {
                lock (EditLock)
                    UpdateBundle();
            }

            return _intlBundleProxy.Bundle;
        }
    }

    string? IBundleProxy.Path
    {
        get
        {
            if (_intlBundleProxy == null || _lastBundleUpdateVersion != ChangeVersion)
            {
                lock (EditLock)
                    UpdateBundle();
            }

            return _intlBundleProxy.Path;
        }
    }

    bool IBundleProxy.ConvertShadersToStandard
    {
        get
        {
            if (_intlBundleProxy == null || _lastBundleUpdateVersion != ChangeVersion)
            {
                lock (EditLock)
                    UpdateBundle();
            }

            return _intlBundleProxy.ConvertShadersToStandard;
        }
    }

    bool IBundleProxy.ConsolidateShaders
    {
        get
        {
            if (_intlBundleProxy == null || _lastBundleUpdateVersion != ChangeVersion)
            {
                lock (EditLock)
                    UpdateBundle();
            }

            return _intlBundleProxy.ConsolidateShaders;
        }
    }
}

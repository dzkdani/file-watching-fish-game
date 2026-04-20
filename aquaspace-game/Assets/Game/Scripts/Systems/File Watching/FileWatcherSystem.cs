using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

/// <summary>
/// Orchestrates file watching system by coordinating multiple responsibilities:
/// - Folder initialization
/// - Config system integration
/// - File scanning and state tracking
/// - Spawning trigger
/// - Config file watching
/// </summary>
public class FileWatcherSystem : MonoBehaviour
{
    [SerializeField] public string path = "";
    [SerializeField] private bool scanRecursively = true;
    [SerializeField] private float configDebounceSeconds = 0.25f;

    private FolderSetup _folderSetup;
    private ConfigIntegration _configIntegration;
    private FileScanner _fileScanner;
    private FileStateTracker _fileStateTracker;
    private SpawnHandler _spawningTrigger;
    private ConfigFileWatcher _configFileWatcher;

    private float _scanInterval = 1f;
    private int _configReloadPending;
    private long _lastConfigSignalTicks;

    private void Start()
    {
        InitializeSystems();
        StartCoroutine(ScanRoutine());
    }

    private void OnDestroy()
    {
        CleanupSystems();
    }

    private void Update()
    {
        HandleConfigReloadDebouncing();
    }

    private void InitializeSystems()
    {
        // Setup folder structure
        _folderSetup = new FolderSetup(path);
        if (!_folderSetup.IsValid())
        {
            Debug.LogError("[FileWatcherSystem] Failed to initialize folder structure");
            return;
        }

        // Initialize config system
        _configIntegration = new ConfigIntegration();
        _configIntegration.ConfigChanged += OnConfigChanged;
        _scanInterval = _configIntegration.ScanInterval;

        // Initialize file scanner
        _fileScanner = new FileScanner(_folderSetup.FishFolderPath, _folderSetup.TrashFolderPath, scanRecursively);

        // Initialize file state tracking
        _fileStateTracker = new FileStateTracker();
        _fileStateTracker.FileChanged += OnFileChanged;
        _fileStateTracker.FileRemoved += OnFileRemoved;

        // Initialize spawning trigger
        _spawningTrigger = new SpawnHandler();

        // Initialize config file watcher
        _configFileWatcher = new ConfigFileWatcher(configDebounceSeconds);
        _configFileWatcher.ConfigChangeDetected += OnConfigFileChangeDetected;
        _configFileWatcher.StartWatching();

        Debug.Log("[FileWatcherSystem] All systems initialized successfully");
    }

    private void CleanupSystems()
    {
        if (_configIntegration != null) 
        {
            _configIntegration.ConfigChanged -= OnConfigChanged;
            _configIntegration.Cleanup();
        }

        if (_fileStateTracker != null)
        {
            _fileStateTracker.FileChanged -= OnFileChanged;
            _fileStateTracker.FileRemoved -= OnFileRemoved;
        }

        if (_configFileWatcher != null) _configFileWatcher.ConfigChangeDetected -= OnConfigFileChangeDetected;
        _configFileWatcher?.Dispose();

        Debug.Log("[FileWatcherSystem] All systems cleaned up");
    }

    private IEnumerator ScanRoutine()
    {
        while (true)
        {
            ScanAndUpdateFiles();
            yield return new WaitForSeconds(_scanInterval);
        }
    }

    private void ScanAndUpdateFiles()
    {
        if (!_folderSetup.IsValid() || _fileScanner == null || _fileStateTracker == null)
        {
            return;
        }

        var files = _fileScanner.GetCategoryFiles();
        _fileScanner.ShuffleFiles(files);
        HashSet<string> currentFiles = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);

        // Process existing files
        foreach (var file in files)
        {
            if (_fileScanner.TryGetFileModificationTime(file, out DateTime lastWrite))
            {
                _fileStateTracker.UpdateFileState(file, lastWrite);
            }
        }

        // Cleanup removed files
        if (_fileStateTracker.HasTrackedFiles())
        {
            _fileStateTracker.CleanupRemovedFiles(currentFiles);
        }
    }

    private void OnFileChanged(string filePath)
    {
        _ = _spawningTrigger.ProcessFile(filePath);
    }

    private void OnFileRemoved(string filePath)
    {
        _spawningTrigger.RemoveSpawned(filePath);
    }

    private void OnConfigChanged(ConfigData config)
    {
        if (config != null && config.scanInterval > 0f)
        {
            _scanInterval = config.scanInterval;
        }
    }

    private void OnConfigFileChangeDetected()
    {
        QueueConfigReload();
    }

    private void QueueConfigReload()
    {
        Interlocked.Exchange(ref _lastConfigSignalTicks, DateTime.UtcNow.Ticks);
        Interlocked.Exchange(ref _configReloadPending, 1);
    }

    private void HandleConfigReloadDebouncing()
    {
        if (Volatile.Read(ref _configReloadPending) == 0)
        {
            return;
        }

        long nowTicks = DateTime.UtcNow.Ticks;
        long pendingSinceTicks = Interlocked.Read(ref _lastConfigSignalTicks);
        double elapsedSeconds = (nowTicks - pendingSinceTicks) / (double)TimeSpan.TicksPerSecond;

        if (elapsedSeconds < configDebounceSeconds)
        {
            return;
        }

        Interlocked.Exchange(ref _configReloadPending, 0);
        _configIntegration?.Reload();
    }
}

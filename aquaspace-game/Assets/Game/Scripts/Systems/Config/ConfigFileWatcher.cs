using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Monitors the config file for changes and triggers reload operations with debouncing.
/// </summary>
public class ConfigFileWatcher : IDisposable
{
    public event Action ConfigChangeDetected;

    private FileSystemWatcher _configWatcher;
    private readonly string _configPath;
    private readonly float _debounceSeconds;

    public ConfigFileWatcher(float debounceSeconds = 0.25f)
    {
        _configPath = Path.Combine(Application.dataPath, "config.json");
        _debounceSeconds = debounceSeconds;
    }

    public void StartWatching()
    {
        string configDirectory = Path.GetDirectoryName(_configPath);
        string configFileName = Path.GetFileName(_configPath);

        if (string.IsNullOrWhiteSpace(configDirectory) || !Directory.Exists(configDirectory))
        {
            Debug.LogWarning($"[ConfigFileWatcher] Directory not found: {configDirectory}");
            return;
        }

        _configWatcher = new FileSystemWatcher(configDirectory, configFileName);
        _configWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.FileName;
        _configWatcher.Changed += OnConfigFileChanged;
        _configWatcher.Created += OnConfigFileChanged;
        _configWatcher.Renamed += OnConfigFileRenamed;
        _configWatcher.EnableRaisingEvents = true;

        Debug.Log($"[ConfigFileWatcher] Watching config file: {_configPath}");
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs args)
    {
        ConfigChangeDetected?.Invoke();
    }

    private void OnConfigFileRenamed(object sender, RenamedEventArgs args)
    {
        ConfigChangeDetected?.Invoke();
    }

    public void Dispose()
    {
        if (_configWatcher == null)
        {
            return;
        }

        _configWatcher.EnableRaisingEvents = false;
        _configWatcher.Changed -= OnConfigFileChanged;
        _configWatcher.Created -= OnConfigFileChanged;
        _configWatcher.Renamed -= OnConfigFileRenamed;
        _configWatcher.Dispose();
        _configWatcher = null;
    }
}

using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using System.Threading;

public class FileWatcherSystem : MonoBehaviour
{
    public float scanInterval = 1f;
    [Tooltip("Relative path under persistentDataPath. Leave empty to scan persistentDataPath root.")]
    public string path = "";
    [SerializeField] private bool scanRecursively = true;
    [SerializeField] private float configDebounceSeconds = 0.25f;

    private string folderPath;
    private string fishFolderPath;
    private string trashFolderPath;
    private readonly Dictionary<string, DateTime> trackedFileWriteTimes = new(StringComparer.OrdinalIgnoreCase);
    private FileSystemWatcher configWatcher;
    private int configReloadPending;
    private long lastConfigSignalTicks;

    private void Start()
    {
        ConfigSystem.Initialize();
        ConfigSystem.ConfigChanged += OnConfigChanged;
        if (ConfigSystem.Data != null && ConfigSystem.Data.scanInterval > 0f)
        {
            scanInterval = ConfigSystem.Data.scanInterval;
        }

        folderPath = ResolveModFolderPath();
        Directory.CreateDirectory(folderPath);
        fishFolderPath = Path.Combine(folderPath, "fish");
        trashFolderPath = Path.Combine(folderPath, "trash");
        Directory.CreateDirectory(fishFolderPath);
        Directory.CreateDirectory(trashFolderPath);
        Debug.Log($"fish folder: {fishFolderPath}");
        Debug.Log($"trash folder: {trashFolderPath}");
        SetupConfigWatcher();
        StartCoroutine(ScanRoutine());
    }

    private void OnDestroy()
    {
        ConfigSystem.ConfigChanged -= OnConfigChanged;
        DisposeConfigWatcher();
    }

    private void Update()
    {
        if (Volatile.Read(ref configReloadPending) == 0)
        {
            return;
        }

        long nowTicks = DateTime.UtcNow.Ticks;
        long pendingSinceTicks = Interlocked.Read(ref lastConfigSignalTicks);
        double elapsedSeconds = (nowTicks - pendingSinceTicks) / (double)TimeSpan.TicksPerSecond;
        if (elapsedSeconds < configDebounceSeconds)
        {
            return;
        }

        Interlocked.Exchange(ref configReloadPending, 0);
        ConfigSystem.Reload();
    }

    private IEnumerator ScanRoutine()
    {
        while (true)
        {
            ScanFolder();
            yield return new WaitForSeconds(scanInterval);
        }
    }

    private void ScanFolder()
    {
        if (!Directory.Exists(folderPath))
        {
            return;
        }

        var files = GetCategoryFiles();
        ShuffleFiles(files);
        HashSet<string> currentFiles = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            DateTime lastWrite;
            try
            {
                lastWrite = File.GetLastWriteTimeUtc(file);
            }
            catch (IOException)
            {
                continue;
            }

            if (trackedFileWriteTimes.TryGetValue(file, out DateTime knownWriteTime) && knownWriteTime == lastWrite)
            {
                continue;
            }

            trackedFileWriteTimes[file] = lastWrite;

            if (SpawnSystem.Instance != null)
            {
                SpawnSystem.Instance.ProcessFile(file);
            }
        }

        if (trackedFileWriteTimes.Count == 0)
        {
            return;
        }

        var removedFiles = new List<string>();
        foreach (var trackedFile in trackedFileWriteTimes.Keys)
        {
            if (!currentFiles.Contains(trackedFile))
            {
                removedFiles.Add(trackedFile);
            }
        }

        for (int i = 0; i < removedFiles.Count; i++)
        {
            string removedFile = removedFiles[i];
            trackedFileWriteTimes.Remove(removedFile);
            if (SpawnSystem.Instance != null)
            {
                SpawnSystem.Instance.RemoveSpawnedFromSource(removedFile);
            }
        }
    }

    private void OnConfigChanged(ConfigData config)
    {
        if (config == null || config.scanInterval <= 0f)
        {
            return;
        }

        scanInterval = config.scanInterval;
    }

    private void SetupConfigWatcher()
    {
        string configPath = Path.Combine(Application.dataPath, "config.json");
        string configDirectory = Path.GetDirectoryName(configPath);
        string configFileName = Path.GetFileName(configPath);

        if (string.IsNullOrWhiteSpace(configDirectory) || !Directory.Exists(configDirectory))
        {
            Debug.LogWarning($"Config watcher directory not found: {configDirectory}");
            return;
        }

        configWatcher = new FileSystemWatcher(configDirectory, configFileName);
        configWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime | NotifyFilters.FileName;
        configWatcher.Changed += OnConfigFileChanged;
        configWatcher.Created += OnConfigFileChanged;
        configWatcher.Renamed += OnConfigFileRenamed;
        configWatcher.EnableRaisingEvents = true;
    }

    private void DisposeConfigWatcher()
    {
        if (configWatcher == null)
        {
            return;
        }

        configWatcher.EnableRaisingEvents = false;
        configWatcher.Changed -= OnConfigFileChanged;
        configWatcher.Created -= OnConfigFileChanged;
        configWatcher.Renamed -= OnConfigFileRenamed;
        configWatcher.Dispose();
        configWatcher = null;
    }

    private void OnConfigFileChanged(object sender, FileSystemEventArgs args)
    {
        QueueConfigReload();
    }

    private void OnConfigFileRenamed(object sender, RenamedEventArgs args)
    {
        QueueConfigReload();
    }

    private void QueueConfigReload()
    {
        Interlocked.Exchange(ref lastConfigSignalTicks, DateTime.UtcNow.Ticks);
        Interlocked.Exchange(ref configReloadPending, 1);
    }

    private string ResolveModFolderPath()
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Application.persistentDataPath;
        }

        return Path.Combine(Application.persistentDataPath, path);
    }

    private List<string> GetCategoryFiles()
    {
        SearchOption searchOption = scanRecursively ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        List<string> files = new List<string>();

        if (Directory.Exists(fishFolderPath))
        {
            files.AddRange(Directory.GetFiles(fishFolderPath, "*.png", searchOption));
        }

        if (Directory.Exists(trashFolderPath))
        {
            files.AddRange(Directory.GetFiles(trashFolderPath, "*.png", searchOption));
        }

        return files;
    }

    private void ShuffleFiles(List<string> files)
    {
        for (int i = files.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            if (randomIndex == i)
            {
                continue;
            }

            string temp = files[i];
            files[i] = files[randomIndex];
            files[randomIndex] = temp;
        }
    }
}

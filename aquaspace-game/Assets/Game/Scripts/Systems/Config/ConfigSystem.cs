using UnityEngine;
using System;
using System.IO;

public static class ConfigSystem
{
    public static ConfigData Data { get; private set; }
    public static event Action<ConfigData> ConfigChanged;

    private static readonly object SyncRoot = new object();
    private static bool isInitialized;


    private static string _configPath;
    private static string ConfigPath
    {
        get
        {
            if (_configPath == null)
            {
                string root = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                _configPath = Path.Combine(root, "config.json");
            }
            return _configPath;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        Initialize();
    }

    public static void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
        Reload(false);
    }

    public static void Load()
    {
        Reload();
    }

    public static bool Reload(bool invokeEvent = true)
    {
        if (!isInitialized)
        {
            isInitialized = true;
        }

        ConfigData parsedConfig;

        lock (SyncRoot)
        {
            if (!File.Exists(ConfigPath))
            {
                Debug.LogError($"Config file not found at: {ConfigPath}");
                CreateDefaultConfig();
                return false;
            }
        }
        try
        {
            string json = File.ReadAllText(ConfigPath);
            parsedConfig = JsonUtility.FromJson<ConfigData>(json);
            if (!Validate(parsedConfig))
            {
                Debug.LogError("Config validation failed. Keeping previous config.");
                CreateDefaultConfig();
                return false;
            }
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to read config: {exception.Message}");
            return false;
        }
        
        TryReadFile(ConfigPath, out string content);
        if (string.IsNullOrEmpty(content))
        {
            Debug.LogError("Config file is empty or unreadable.");
            return false;
        }     

        if (parsedConfig == null)
        {
            Debug.LogError("Failed to parse config.json. Keeping previous config.");
            return false;
        }

        Data = parsedConfig;

        if (invokeEvent)
        {
            ConfigChanged?.Invoke(Data);
        }

        return true;
    }

    private static void CreateDefaultConfig()
    {
        ConfigData defaultConfig = new ConfigData
        {
            fishMinSpd = 1f,
            fishMaxSpd = 5f,
            fishMaxHungerMeter = 100f,
            fishHungerCooldown = 5f,
            foodDetectionRadius = 10f,
            trashMinSpd = 1f,
            trashMaxSpd = 3f,
            maxFish = 10,
            maxTrash = 5,
            scanInterval = 1f
        };

        Data = defaultConfig;
        try
        {
            string json = JsonUtility.ToJson(defaultConfig, true);
            File.WriteAllText(ConfigPath, json);
            Debug.Log($"Default config created at: {ConfigPath}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to create default config: {exception.Message}");
        }
    }


    private static bool TryReadFile(string path, out string content)
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                content = File.ReadAllText(path);
                return true;
            }
            catch
            {
                System.Threading.Thread.Sleep(50);
            }
        }

        content = null;
        return false;
    }

    private static bool Validate(ConfigData data)
    {
        if (data == null) return false;

        if (data.fishMinSpd < 0) return false;
        if (data.fishMaxSpd < data.fishMinSpd) return false;
        if (data.fishMaxHungerMeter < 0) return false;
        if (data.fishHungerCooldown < 0) return false;
        if (data.foodDetectionRadius < 0) return false;
        if (data.maxFish < 0) return false;
        if (data.trashMinSpd < 0) return false;
        if (data.trashMaxSpd < data.trashMinSpd) return false;
        if (data.maxTrash < 0) return false;
        if (data.scanInterval <= 0f) return false;

        return true;
    }
}

[Serializable]
public class ConfigData
{
    public float fishMinSpd;
    public float fishMaxSpd;
    public float fishMaxHungerMeter;
    public float fishHungerCooldown;
    public float foodDetectionRadius;
    public float trashMinSpd;
    public float trashMaxSpd;
    public int maxFish;
    public int maxTrash;
    public float scanInterval;
}

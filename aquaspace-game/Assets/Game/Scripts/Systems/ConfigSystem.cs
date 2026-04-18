using UnityEngine;
using System;
using System.IO;

public static class ConfigSystem
{
    public static ConfigData Data { get; private set; }
    public static event Action<ConfigData> ConfigChanged;

    private static readonly object SyncRoot = new object();
    private static bool isInitialized;

    private static string ConfigPath => Path.Combine(Application.dataPath, "config.json");

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
                return false;
            }

            try
            {
                string json = File.ReadAllText(ConfigPath);
                parsedConfig = JsonUtility.FromJson<ConfigData>(json);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to read config: {exception.Message}");
                return false;
            }
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
}

[System.Serializable]
public class ConfigData
{
    public float fishMinSpd;
    public float fishMaxSpd;
    public float fishMaxHungerMeter;
    public float fishHungerDecay;
    public float fishHungerCooldown;
    public float foodDetectionRadius;
    public float trashMinSpd;
    public float trashMaxSpd;
    public int maxFish;
    public int maxTrash;
    public float scanInterval;
}

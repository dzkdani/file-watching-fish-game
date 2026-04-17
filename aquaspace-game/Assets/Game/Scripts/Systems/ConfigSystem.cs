using UnityEngine;
using System.IO;

public class ConfigSystem
{
    public static ConfigData Data;

    public static void Load()
    {
        string path = Path.Combine(Application.dataPath, "../config.json");
        string json = File.ReadAllText(path);
        Data = JsonUtility.FromJson<ConfigData>(json);
    }
}

[System.Serializable]
public class ConfigData
{
    public float spawnRadius;
    public float maxFish;
    public float scanInterval;
}
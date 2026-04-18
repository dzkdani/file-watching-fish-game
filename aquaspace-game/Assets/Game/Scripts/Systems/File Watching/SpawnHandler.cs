using UnityEngine;

/// <summary>
/// Triggers spawning through the SpawnSystem for detected file changes.
/// </summary>
public class SpawnHandler
{
    public void ProcessFile(string filePath)
    {
        if (SpawnSystem.Instance != null)
        {
            SpawnSystem.Instance.ProcessFile(filePath);
        }
    }

    public void RemoveSpawned(string sourceFile)
    {
        if (SpawnSystem.Instance != null)
        {
            SpawnSystem.Instance.RemoveSpawnedFromSource(sourceFile);
        }
    }
}

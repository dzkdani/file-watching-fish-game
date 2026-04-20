using UnityEngine;
using System;
using System.Threading.Tasks;

public class SpawnHandler
{
    public async Task ProcessFile(string path)
    {
        ParsedFileData parsed;
        try
        {
            parsed = FileParser.Parse(path);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Parse failed: {e.Message}");
            return;
        }

        Texture2D tex;
        try
        {
            byte[] data = await System.IO.File.ReadAllBytesAsync(path);
            tex = new Texture2D(2, 2);
            tex.LoadImage(data);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Texture load failed: {e.Message}");
            return;
        }

        SpawnSystem.Instance.Spawn(path, parsed, tex);
    }

    public void RemoveSpawned(string path)
    {
        SpawnSystem.Instance.RemoveBySource(path);
    }
}

using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SpawnSystem : MonoBehaviour
{
    public static SpawnSystem Instance;

    public Transform spawnParent;
    public Vector2 minSpawn;
    public Vector2 maxSpawn;

    public List<FishData> fishDatabase;
    public List<TrashData> trashDatabase;

    void Awake() => Instance = this;

    public async void ProcessFile(string path)
    {
        var parsed = FileParser.Parse(path);
        Texture2D tex = await LoadTexture(path);

        Spawn(parsed, tex);
    }

    private async Task<Texture2D> LoadTexture(string path)
    {
        byte[] data = await File.ReadAllBytesAsync(path);
        Texture2D tex = new Texture2D(2,2);
        tex.LoadImage(data);
        return tex;
    }

    private void Spawn(ParsedFileData parsed, Texture2D tex)
    {
        
    }
}
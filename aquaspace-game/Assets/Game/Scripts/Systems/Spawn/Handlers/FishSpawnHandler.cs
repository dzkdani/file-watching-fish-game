using UnityEngine;

public class FishSpawnHandler : ISpawnHandler
{
    private readonly ISpawnFactory factory;
    private readonly SpawnTracker tracker;
    private readonly int maxFish;

    public FishSpawnHandler(ISpawnFactory factory, SpawnTracker tracker, int maxFish)
    {
        this.factory = factory;
        this.tracker = tracker;
        this.maxFish = maxFish;
    }

    public bool CanHandle(string category) => category == "fish";

    public GameObject Spawn(ParsedFileData data, Texture2D tex)
    {
        if (!tracker.CanSpawn("Fish", maxFish))
            return null;

        var obj = factory.Create($"Fish_{data.type}", "Fish", Vector3.zero, tex);

        obj.AddComponent<FishController>();
        tracker.Register("Fish");

        return obj;
    }

    public void Update(GameObject obj, ParsedFileData data, Texture2D tex)
    {
        factory.Update(obj, $"Fish_{data.type}", "Fish", tex);
    }
}
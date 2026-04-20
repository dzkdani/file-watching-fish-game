using System;
using UnityEngine;

public class TrashSpawnHandler : ISpawnHandler
{
    private readonly ISpawnFactory factory;
    private readonly SpawnTracker tracker;
    private readonly Func<int> maxTrash;

    public TrashSpawnHandler(ISpawnFactory factory, SpawnTracker tracker, Func<int>  maxTrash)
    {
        this.factory = factory;
        this.tracker = tracker;
        this.maxTrash = maxTrash;
    }

    public bool CanHandle(string category) => category == "trash";

    public GameObject Spawn(ParsedFileData data, Texture2D tex)
    {
        if (!tracker.CanSpawn("Trash", maxTrash()))
            return null;

        string poolKey = $"trash::Trash";

        var obj = factory.Create(
            $"Trash_{data.type}",
            "Trash",
            Vector3.zero,
            tex,
            poolKey
        );

        if (obj.GetComponent<TrashController>() == null)
            obj.AddComponent<TrashController>();

        tracker.Register("Trash");

        return obj;
}

    public void Update(GameObject obj, ParsedFileData data, Texture2D tex)
    {
        factory.Update(obj, $"Trash_{data.type}", "Trash", tex);
    }
}
using UnityEngine;
using System.Collections.Generic;


public class SourceMapper
{
    private readonly Dictionary<string, GameObject> pathToObject = new();
    private readonly Dictionary<GameObject, string> objectToPath = new();

    public bool TryGet(string path, out GameObject obj)
        => pathToObject.TryGetValue(path, out obj);

    public void Register(string path, GameObject obj)
    {
        pathToObject[path] = obj;
        objectToPath[obj] = path;
    }

    public void Remove(GameObject obj)
    {
        if (!objectToPath.TryGetValue(obj, out var path)) return;

        objectToPath.Remove(obj);
        pathToObject.Remove(path);
    }
}
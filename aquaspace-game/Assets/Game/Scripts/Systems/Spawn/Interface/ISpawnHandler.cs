using UnityEngine;

public interface ISpawnHandler
{
    bool CanHandle(string category);
    GameObject Spawn(ParsedFileData data, Texture2D texture);
    void Update(GameObject obj, ParsedFileData data, Texture2D texture);
}
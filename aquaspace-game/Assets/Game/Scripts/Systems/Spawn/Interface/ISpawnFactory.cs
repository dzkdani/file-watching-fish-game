using UnityEngine;

public interface ISpawnFactory
{
    GameObject Create(string name, string tag, Vector3 position, Texture2D texture, string poolKey = "");
    void Update(GameObject obj, string name, string tag, Texture2D texture);
}
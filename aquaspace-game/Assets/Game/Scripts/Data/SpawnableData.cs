using UnityEngine;

[CreateAssetMenu(fileName = "SpawnableData", menuName = "ScriptableObjects/SpawnableData", order = 1)]
public class SpawnableData : ScriptableObject
{
    public ScriptableObject spawnableScriptableObject;
    public string spawnableName;
    public GameObject prefab;

    public string GetSpawnableType()
    {
        if (spawnableScriptableObject == null)
        {
            Debug.LogError("Spawnable ScriptableObject is not assigned.");
            return string.Empty;
        }
        switch (spawnableScriptableObject)
        {
            case FishData fishData:
                spawnableName = fishData.type;
                break;
            case FoodData foodData:
                spawnableName = foodData.type;
                break;
            case TrashData trashData:
                spawnableName = trashData.type;
                break;
            default:
                Debug.LogError("Unsupported spawnable type.");
                return string.Empty;
        }
        return spawnableName;
    }
}


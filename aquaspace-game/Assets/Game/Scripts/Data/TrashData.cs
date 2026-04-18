using UnityEngine;

[CreateAssetMenu(fileName = "TrashData", menuName = "ScriptableObjects/TrashData", order = 1)]
public class TrashData : ScriptableObject, ISpawnable
{
    public string type;
    public string trashType;
    public float minSpeed;
    public float maxSpeed;
}
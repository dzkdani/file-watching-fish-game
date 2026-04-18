using UnityEngine;

[CreateAssetMenu(fileName = "FishData", menuName = "ScriptableObjects/FishData", order = 1)]
public class FishData : ScriptableObject, ISpawnable
{
    public string type;
    public string fishType;
    public float minSpeed;
    public float maxSpeed;
    public float detectionRadius;
    public float hungerCooldown;
}
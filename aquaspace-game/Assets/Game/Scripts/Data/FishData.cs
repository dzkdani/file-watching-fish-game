using UnityEngine;

[CreateAssetMenu(fileName = "FishData", menuName = "ScriptableObjects/FishData", order = 1)]
public class FishData : ScriptableObject
{
    public string type;
    public float minSpeed;
    public float maxSpeed;
    public float detectionRadius;
    public float hungerCooldown;
    public Sprite fishSprite;
}
using UnityEngine;

[CreateAssetMenu(menuName = "Aquascape/Fish Config")]
public class FishConfig : ScriptableObject
{
    [Header("Movement")]
    public float minSpeed = 1f;
    public float maxSpeed = 2f;

    [Header("Wander")]
    public float wanderInterval = 1.5f;

    [Header("Food")]
    public float detectionRadius = 5f;
    public float arriveDistance = 0.15f;

    [Header("Fear")]
    public float fearDuration = 1f;
    public float fearSpeedMultiplier = 1.5f;

    [Header("Hunger")]
    public float maxHunger = 100f;
    public float hungerDecay = 10f;
    public float hungerCooldown = 3f;
}
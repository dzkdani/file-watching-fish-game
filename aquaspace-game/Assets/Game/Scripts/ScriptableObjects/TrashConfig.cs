using UnityEngine;

[CreateAssetMenu(menuName = "Aquascape/Trash Config")]
public class TrashConfig : ScriptableObject
{
    [Header("Movement")]
    public float minSpeed = 0.5f;
    public float maxSpeed = 1.5f;

    [Header("Direction")]
    public float directionChangeInterval = 2f;

    [Header("Separation")]
    public float separationStrength = 0.95f;
}
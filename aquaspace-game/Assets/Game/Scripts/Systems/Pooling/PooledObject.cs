using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [SerializeField] private string poolKey;

    public string PoolKey
    {
        get => poolKey;
        set => poolKey = value;
    }
}

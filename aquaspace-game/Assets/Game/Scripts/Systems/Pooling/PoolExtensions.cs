using UnityEngine;

public static class PoolExtensions
{
    public static void AssignPoolKey(this GameObject instance, string poolKey)
    {
        if (instance == null)
        {
            return;
        }

        PooledObject pooledObject = instance.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            pooledObject = instance.AddComponent<PooledObject>();
        }

        pooledObject.PoolKey = poolKey;
    }

    public static bool TryAcquireFromPool(this string poolKey, out GameObject instance)
    {
        return ObjectPoolRegistry.TryAcquire(poolKey, out instance);
    }

    public static bool ReturnToPool(this GameObject instance, Transform poolRoot = null)
    {
        if (instance == null)
        {
            return false;
        }


        PooledObject pooledObject = instance.GetComponent<PooledObject>();
        if (pooledObject == null || string.IsNullOrWhiteSpace(pooledObject.PoolKey))
        {
            if (Application.isPlaying)
            {
                Object.Destroy(instance);
            }
            else
            {
                Object.DestroyImmediate(instance);
            }
            return false;
        }

        ObjectPoolRegistry.Release(pooledObject.PoolKey, instance, poolRoot);
        return true;
    }
}

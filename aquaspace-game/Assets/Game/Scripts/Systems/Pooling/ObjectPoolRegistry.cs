using System;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectPoolRegistry
{
    private static readonly Dictionary<string, Stack<GameObject>> Pools = new(StringComparer.OrdinalIgnoreCase);

    public static bool TryAcquire(string poolKey, out GameObject instance)
    {
        instance = null;
        if (string.IsNullOrWhiteSpace(poolKey))
        {
            return false;
        }

        if (!Pools.TryGetValue(poolKey, out Stack<GameObject> pool))
        {
            return false;
        }

        while (pool.Count > 0)
        {
            instance = pool.Pop();
            if (instance == null)
            {
                continue;
            }

            instance.SetActive(true);
            return true;
        }

        return false;
    }

    public static void Release(string poolKey, GameObject instance, Transform poolRoot = null)
    {
        if (string.IsNullOrWhiteSpace(poolKey) || instance == null)
        {
            return;
        }

        if (!Pools.TryGetValue(poolKey, out Stack<GameObject> pool))
        {
            pool = new Stack<GameObject>();
            Pools[poolKey] = pool;
        }

        if (poolRoot != null)
        {
            instance.transform.SetParent(poolRoot);
        }

        instance.SetActive(false);
        pool.Push(instance);
    }
}

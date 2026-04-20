using UnityEngine;

public class ColliderFactory
{
    public void Apply(GameObject obj, Sprite sprite)
    {
        var collider = obj.GetComponent<PolygonCollider2D>();
        if (collider == null)
            collider = obj.AddComponent<PolygonCollider2D>();

        Bounds bounds = collider.bounds;
    }
}
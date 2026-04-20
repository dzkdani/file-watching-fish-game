using UnityEngine;

public class ColliderFactory
{
    public void Apply(GameObject obj, Sprite sprite)
    {
        var collider = obj.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = obj.AddComponent<BoxCollider2D>();

        collider.size = sprite.bounds.size;
    }
}
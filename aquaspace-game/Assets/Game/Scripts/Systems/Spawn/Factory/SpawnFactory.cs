using UnityEngine;


public class SpawnFactory : ISpawnFactory
{
    private readonly ColliderFactory colliderFactory;

    public SpawnFactory(ColliderFactory colliderFactory)
    {
        this.colliderFactory = colliderFactory;
    }

    public GameObject Create(string name, string tag, Vector3 pos, Texture2D tex)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = pos;

        var renderer = obj.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f);

        colliderFactory.Apply(obj, renderer.sprite);

        obj.tag = tag;
        obj.layer = LayerMask.NameToLayer(tag);

        return obj;
    }

    public void Update(GameObject obj, string name, string tag, Texture2D tex)
    {
        obj.name = name;

        var renderer = obj.GetComponent<SpriteRenderer>();
        renderer.sprite = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f);

        colliderFactory.Apply(obj, renderer.sprite);
    }
}
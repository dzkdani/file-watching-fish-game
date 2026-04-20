using UnityEngine;


public class SpawnFactory : ISpawnFactory
{
    private readonly ColliderFactory colliderFactory;
    private readonly Transform parent;
    private readonly Transform poolRoot;


    public SpawnFactory(ColliderFactory colliderFactory, Transform parent, Transform poolRoot)
    {
        this.colliderFactory = colliderFactory;
        this.parent = parent;
        this.poolRoot = poolRoot;
    }

    public GameObject Create(string name, string tag, Vector3 pos, Texture2D tex, string poolKey = "")
    {
        GameObject obj;

        if (!string.IsNullOrEmpty(poolKey) && poolKey.TryAcquireFromPool(out obj))
        {
            Prepare(obj, name, tag, pos, tex);
            obj.AssignPoolKey(poolKey);
            return obj;
        }
        else
        {
            obj = new GameObject(name);
        }

        if (parent != null)
            obj.transform.SetParent(parent);
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

    private void Prepare(GameObject obj, string name, string tag, Vector3 pos, Texture2D tex)
    {
        obj.SetActive(true);
        obj.name = name;
        obj.transform.SetParent(parent);
        obj.transform.position = pos;

        var renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = obj.AddComponent<SpriteRenderer>();

        renderer.sprite = Sprite.Create(tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f);

        colliderFactory.Apply(obj, renderer.sprite);

        obj.tag = tag;
        obj.layer = LayerMask.NameToLayer(tag);
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
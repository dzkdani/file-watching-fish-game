using UnityEngine;
using System.Collections.Generic;


public class SpawnSystem : MonoBehaviour
{
    public static SpawnSystem Instance;
    [SerializeField] private Transform parent;
    [SerializeField] private Transform poolRoot;
    private int maxFish;
    private int maxTrash;
    private List<ISpawnHandler> handlers;
    private SourceMapper mapper;
    private SpatialGrid grid;
    private SpawnTracker tracker;
    private CameraBoundsUtility bounds;

    private void Awake()
    {
        ConfigSystem.Initialize();
        ConfigSystem.ConfigChanged += ApplyConfig;

        if (ConfigSystem.Data != null)
            ApplyConfig(ConfigSystem.Data);

        tracker = new SpawnTracker();
        mapper = new SourceMapper();
        grid = new SpatialGrid(1.5f);

        var colliderFactory = new ColliderFactory();
        var factory = new SpawnFactory(colliderFactory, parent, poolRoot);

        handlers = new List<ISpawnHandler>
        {
            new FishSpawnHandler(factory, tracker, () => maxFish),
            new TrashSpawnHandler(factory, tracker, () => maxTrash)
        };

        bounds = new CameraBoundsUtility(Camera.main, 2f);
        BoundsUtility.Initialize(bounds);

        Instance = this;
    }

    public void Spawn(string path, ParsedFileData data, Texture2D tex)
    {
        if (mapper.TryGet(path, out var existing))
        {
            foreach (var h in handlers)
            {
                if (h.CanHandle(data.category))
                {
                    h.Update(existing, data, tex);
                    return;
                }
            }
        }

        foreach (var h in handlers)
        {
            if (!h.CanHandle(data.category)) continue;

            var obj = h.Spawn(data, tex);
            if (obj == null) return;

            obj.transform.position = GetSpawnPosition();
            grid.Register(obj);
            mapper.Register(path, obj);
            return;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Rect boundsRect = bounds.GetBounds(0);

        for (int i = 0; i < 10; i++)
        {
            Vector2 pos = new Vector2(
                Random.Range(boundsRect.xMin, boundsRect.xMax),
                Random.Range(boundsRect.yMin, boundsRect.yMax));

            if (!grid.IsOccupied(pos, 0.5f))
                return pos;
        }

        return boundsRect.center;
    }

    public bool Despawn(GameObject obj)
    {
        if (obj == null) return false;

        string tag = obj.tag;

        grid.Unregister(obj);
        mapper.Remove(obj);
        tracker.Unregister(tag);

        if (!obj.ReturnToPool(poolRoot))
        {
            Destroy(obj);
        }

        return true;
    }

    public void RemoveBySource(string path)
    {
        if (!mapper.TryGet(path, out var obj))
            return;

        Despawn(obj);
    }

    private void ApplyConfig(ConfigData config)
    {
        maxFish = Mathf.Max(0, config.maxFish);
        maxTrash = Mathf.Max(0, config.maxTrash);
    }

}

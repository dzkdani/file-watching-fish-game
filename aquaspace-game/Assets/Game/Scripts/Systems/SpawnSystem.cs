using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

public class SpawnSystem : MonoBehaviour
{
    private enum ColliderBuildMode
    {
        PolygonPreferred,
        Box,
        Circle
    }

    public static SpawnSystem Instance;

    [Header("Spawn Root")]
    public Transform spawnParent;
    public float collisionCheckRadius = 0.25f;
    public int maxSpawnPositionAttempts = 10; 
    [SerializeField] private float spawnIntervalSeconds = 0.2f; 

    [Header("Bounds")]
    [SerializeField] private Camera boundsCamera;
    [SerializeField] private float spawnAreaPadding = 2.0f; 
    [SerializeField] private float movementBoundsPadding = 2.5f;

    [Header("Collider")]
    [SerializeField] private ColliderBuildMode colliderBuildMode = ColliderBuildMode.PolygonPreferred;
    [SerializeField] private float colliderScale = 1f;

    private float fallbackSpawnRadius = 500f;
    private int maxFish;
    private int maxTrashSpawn;

    private readonly Dictionary<string, GameObject> spawnedBySourcePath = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<GameObject, string> sourcePathBySpawned = new();
    private readonly Queue<string> pendingFiles = new();
    private readonly HashSet<string> pendingFileSet = new(StringComparer.OrdinalIgnoreCase);
    private bool queueRunning;
    private Transform poolRoot;

    private void Awake()
    {
        Instance = this;
        EnsurePoolRoot();

        ConfigSystem.Initialize();
        ConfigSystem.ConfigChanged += ApplyConfig;
        if (ConfigSystem.Data != null)
        {
            ApplyConfig(ConfigSystem.Data);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        ConfigSystem.ConfigChanged -= ApplyConfig;
    }

    private void ApplyConfig(ConfigData config)
    {
        if (config == null)
        {
            Debug.LogWarning("Config data is null.");
            return;
        }
        maxFish = Mathf.Max(0, config.maxFish);
        maxTrashSpawn = Mathf.Max(0, config.maxTrash);
    }

    public void ProcessFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!pendingFileSet.Add(path))
        {
            return;
        }

        pendingFiles.Enqueue(path);

        if (!queueRunning)
        {
            StartCoroutine(ProcessQueueRoutine());
        }
    }

    private IEnumerator ProcessQueueRoutine()
    {
        queueRunning = true;

        while (pendingFiles.Count > 0)
        {
            string path = pendingFiles.Dequeue();
            pendingFileSet.Remove(path);

            Task task = ProcessSingleFileAsync(path);
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                Debug.LogWarning($"Failed to process mod file '{path}': {task.Exception?.GetBaseException().Message}");
            }

            if (spawnIntervalSeconds > 0f)
            {
                yield return new WaitForSeconds(spawnIntervalSeconds);
            }
            else
            {
                yield return null;
            }
        }

        queueRunning = false;
    }

    private async Task ProcessSingleFileAsync(string path)
    {
        ParsedFileData parsed;
        try
        {
            parsed = FileParser.Parse(path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to parse file name: {path}. {exception.Message}");
            return;
        }

        Texture2D tex;
        try
        {
            tex = await LoadTexture(path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Failed to load texture file: {path}. {exception.Message}");
            return;
        }

        SpawnOrUpdate(path, parsed, tex);
    }

    private async Task<Texture2D> LoadTexture(string path)
    {
        byte[] data = await File.ReadAllBytesAsync(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.LoadImage(data);
        return tex;
    }

    private void SpawnOrUpdate(string sourcePath, ParsedFileData parsed, Texture2D tex)
    {
        if (spawnedBySourcePath.TryGetValue(sourcePath, out GameObject existingObject) && existingObject != null)
        {
            UpdateExistingSpawn(sourcePath, existingObject, parsed, tex);
            return;
        }

        SpawnNew(sourcePath, parsed, tex);
    }

    private void SpawnNew(string sourcePath, ParsedFileData parsed, Texture2D tex)
    {
        string category = parsed.category.ToLowerInvariant();
        if (category == "fish")
        {
            SpawnFish(sourcePath, parsed, tex);
            return;
        }

        if (category == "trash")
        {
            SpawnTrash(sourcePath, parsed, tex);
            return;
        }

        Debug.LogWarning($"Unsupported spawn category: {parsed.category}");
    }

    private void SpawnFish(string sourcePath, ParsedFileData parsed, Texture2D tex)
    {
        if (CountByTag("Fish") >= maxFish)
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        GameObject fishObject = BuildSpriteObject($"Fish_{parsed.type}_{parsed.timestamp}", "Fish", spawnPosition, tex);
        if (fishObject.GetComponent<FishController>() == null) fishObject.AddComponent<FishController>();
        RegisterSourceMapping(sourcePath, fishObject);
    }

    private void SpawnTrash(string sourcePath, ParsedFileData parsed, Texture2D tex)
    {
        if (CountByTag("Trash") >= maxTrashSpawn)
        {
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        string objectName = $"Trash_{parsed.type}_{parsed.timestamp}";
        string poolKey = $"trash::{parsed.type}";
        GameObject trashObject;

        if (poolKey.TryAcquireFromPool(out trashObject))
        {
            PrepareSpawnedObject(trashObject, objectName, "Trash", spawnPosition, tex);
            trashObject.AssignPoolKey(poolKey);
        }
        else
        {
            trashObject = BuildSpriteObject(objectName, "Trash", spawnPosition, tex);
            trashObject.AssignPoolKey(poolKey);
        }

        if (trashObject.GetComponent<TrashController>() == null) trashObject.AddComponent<TrashController>();
        RegisterSourceMapping(sourcePath, trashObject);
    }

    private void UpdateExistingSpawn(string sourcePath, GameObject target, ParsedFileData parsed, Texture2D tex)
    {
        string objectName = $"{parsed.category}_{parsed.type}_{parsed.timestamp}";
        string tagName = parsed.category.Equals("trash", StringComparison.OrdinalIgnoreCase) ? "Trash" : "Fish";
        PrepareSpawnedObject(target, objectName, tagName, target.transform.position, tex);
        RegisterSourceMapping(sourcePath, target);
    }

    private GameObject BuildSpriteObject(string objectName, string tagName, Vector3 position, Texture2D texture)
    {
        GameObject spawnedObject = new GameObject(objectName);
        PrepareSpawnedObject(spawnedObject, objectName, tagName, position, texture);
        return spawnedObject;
    }

    private void PrepareSpawnedObject(GameObject spawnedObject, string objectName, string tagName, Vector3 position, Texture2D texture)
    {
        if (spawnedObject == null)
        {
            return;
        }

        spawnedObject.SetActive(true);
        spawnedObject.name = objectName;
        Transform parent = spawnParent != null ? spawnParent : transform;
        spawnedObject.transform.SetParent(parent);
        spawnedObject.transform.position = position;

        SpriteRenderer renderer = spawnedObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = spawnedObject.AddComponent<SpriteRenderer>();
        }

        Sprite sprite = CreateSprite(texture);
        renderer.sprite = sprite;
        ConfigureCollider(spawnedObject, sprite);

        TrySetTag(spawnedObject, tagName);
    }

    private Sprite CreateSprite(Texture2D texture)
    {
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            Vector4.zero,
            true);
    }

    private void ConfigureCollider(GameObject target, Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        switch (colliderBuildMode)
        {
            case ColliderBuildMode.Box:
                EnsureBoxCollider(target, sprite);
                break;
            case ColliderBuildMode.Circle:
                EnsureCircleCollider(target, sprite);
                break;
            case ColliderBuildMode.PolygonPreferred:
            default:
                if (!EnsurePolygonCollider(target, sprite))
                {
                    EnsureBoxCollider(target, sprite);
                }
                break;
        }
    }

    private bool EnsurePolygonCollider(GameObject target, Sprite sprite)
    {
        int shapeCount = sprite.GetPhysicsShapeCount();
        if (shapeCount <= 0)
        {
            return false;
        }

        RemoveColliderIfExists<BoxCollider2D>(target);
        RemoveColliderIfExists<CircleCollider2D>(target);

        PolygonCollider2D polygonCollider = target.GetComponent<PolygonCollider2D>();
        if (polygonCollider == null)
        {
            polygonCollider = target.AddComponent<PolygonCollider2D>();
        }

        polygonCollider.pathCount = shapeCount;
        List<Vector2> path = new List<Vector2>();
        for (int i = 0; i < shapeCount; i++)
        {
            path.Clear();
            sprite.GetPhysicsShape(i, path);
            if (!Mathf.Approximately(colliderScale, 1f))
            {
                for (int j = 0; j < path.Count; j++)
                {
                    path[j] *= colliderScale;
                }
            }

            polygonCollider.SetPath(i, path);
        }

        return true;
    }

    private void EnsureBoxCollider(GameObject target, Sprite sprite)
    {
        RemoveColliderIfExists<PolygonCollider2D>(target);
        RemoveColliderIfExists<CircleCollider2D>(target);

        BoxCollider2D boxCollider = target.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = target.AddComponent<BoxCollider2D>();
        }

        boxCollider.offset = sprite.bounds.center;
        boxCollider.size = sprite.bounds.size * colliderScale;
    }

    private void EnsureCircleCollider(GameObject target, Sprite sprite)
    {
        RemoveColliderIfExists<PolygonCollider2D>(target);
        RemoveColliderIfExists<BoxCollider2D>(target);

        CircleCollider2D circleCollider = target.GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            circleCollider = target.AddComponent<CircleCollider2D>();
        }

        circleCollider.offset = sprite.bounds.center;
        circleCollider.radius = Mathf.Max(sprite.bounds.extents.x, sprite.bounds.extents.y) * colliderScale;
    }

    private void RemoveColliderIfExists<T>(GameObject target) where T : Collider2D
    {
        T collider = target.GetComponent<T>();
        if (collider == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(collider);
        }
        else
        {
            DestroyImmediate(collider);
        }
    }

    private void TrySetTag(GameObject target, string tagName)
    {
        try
        {
            target.tag = tagName;
            target.layer = LayerMask.NameToLayer(tagName);
        }
        catch (UnityException)
        {
            Debug.LogWarning($"Tag '{tagName}' is not defined. Add it in Project Settings > Tags and Layers.");
        }
    }

    private void EnsurePoolRoot()
    {
        GameObject poolRootObject = new GameObject("PoolingRoot");
        poolRootObject.transform.SetParent(transform);
        poolRootObject.transform.localPosition = Vector3.zero;
        poolRoot = poolRootObject.transform;
    }

    private void RegisterSourceMapping(string sourcePath, GameObject spawnedObject)
    {
        if (string.IsNullOrWhiteSpace(sourcePath) || spawnedObject == null)
        {
            return;
        }

        if (spawnedBySourcePath.TryGetValue(sourcePath, out GameObject previousObject) &&
            previousObject != null &&
            previousObject != spawnedObject)
        {
            sourcePathBySpawned.Remove(previousObject);
        }

        spawnedBySourcePath[sourcePath] = spawnedObject;
        sourcePathBySpawned[spawnedObject] = sourcePath;
    }

    private void RemoveSourceMappingForObject(GameObject spawnedObject)
    {
        if (spawnedObject == null)
        {
            return;
        }

        if (!sourcePathBySpawned.TryGetValue(spawnedObject, out string sourcePath))
        {
            return;
        }

        sourcePathBySpawned.Remove(spawnedObject);
        spawnedBySourcePath.Remove(sourcePath);
    }

    private int CountByTag(string tagName)
    {
        try
        {
            return GameObject.FindGameObjectsWithTag(tagName).Length;
        }
        catch (UnityException)
        {
            return 0;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        if (TryGetSpawnBounds(out Rect spawnBounds))
        {
            for (int i = 0; i < maxSpawnPositionAttempts; i++)
            {
                Vector3 candidate = new Vector3(
                    UnityEngine.Random.Range(spawnBounds.xMin, spawnBounds.xMax),
                    UnityEngine.Random.Range(spawnBounds.yMin, spawnBounds.yMax),
                    0f);

                if (!Physics2D.OverlapCircle(candidate, collisionCheckRadius))
                {
                    return candidate;
                }
            }

            return new Vector3(
                UnityEngine.Random.Range(spawnBounds.xMin, spawnBounds.xMax),
                UnityEngine.Random.Range(spawnBounds.yMin, spawnBounds.yMax),
                0f);
        }

        Vector2 center = GetFallbackCenter();
        for (int i = 0; i < maxSpawnPositionAttempts; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * fallbackSpawnRadius;
            Vector3 candidate = new Vector3(center.x + offset.x, center.y + offset.y, 0f);
            if (!Physics2D.OverlapCircle(candidate, collisionCheckRadius))
            {
                return candidate;
            }
        }

        return (Vector3)center;
    }

    public bool TryGetMovementBounds(out Rect bounds)
    {
        return TryGetCameraWorldRect(movementBoundsPadding, out bounds);
    }

    private bool TryGetSpawnBounds(out Rect bounds)
    {
        return TryGetCameraWorldRect(spawnAreaPadding, out bounds);
    }

    private bool TryGetCameraWorldRect(float padding, out Rect bounds)
    {
        Camera cameraToUse = ResolveBoundsCamera();
        if (cameraToUse == null)
        {
            bounds = default;
            return false;
        }

        float depth = Mathf.Abs(cameraToUse.transform.position.z - GetTargetPlaneZ());
        Vector3 bottomLeft = cameraToUse.ScreenToWorldPoint(new Vector3(0f, 0f, depth));
        Vector3 topRight = cameraToUse.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, depth));

        float minX = Mathf.Min(bottomLeft.x, topRight.x) + padding;
        float maxX = Mathf.Max(bottomLeft.x, topRight.x) - padding;
        float minY = Mathf.Min(bottomLeft.y, topRight.y) + padding;
        float maxY = Mathf.Max(bottomLeft.y, topRight.y) - padding;

        if (minX >= maxX || minY >= maxY)
        {
            bounds = default;
            return false;
        }

        bounds = Rect.MinMaxRect(minX, minY, maxX, maxY);
        return true;
    }

    private Camera ResolveBoundsCamera()
    {
        if (boundsCamera != null)
        {
            return boundsCamera;
        }

        return Camera.main;
    }

    private float GetTargetPlaneZ()
    {
        if (spawnParent != null)
        {
            return spawnParent.position.z;
        }

        return transform.position.z;
    }

    private Vector2 GetFallbackCenter()
    {
        if (spawnParent != null)
        {
            return spawnParent.position;
        }

        return transform.position;
    }

    public void RemoveSpawnedFromSource(string sourcePath)
    {
        if (!spawnedBySourcePath.TryGetValue(sourcePath, out GameObject existingObject))
        {
            return;
        }

        spawnedBySourcePath.Remove(sourcePath);
        sourcePathBySpawned.Remove(existingObject);
        pendingFileSet.Remove(sourcePath);
        if (existingObject != null)
        {
            if (existingObject.CompareTag("Trash"))
            {
                existingObject.ReturnToPool(poolRoot);
            }
            else
            {
                Destroy(existingObject);
            }
        }
    }

    public bool Despawn(GameObject obj, string tag)
    {
        if (obj == null || !obj.CompareTag(tag))
        {
            return false;
        }

        RemoveSourceMappingForObject(obj);
        obj.ReturnToPool(poolRoot);
        return true;
    }
}

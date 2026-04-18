using UnityEngine;

public class InputSystem : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Camera inputCamera;

    [Header("Food Spawn")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Transform foodParent;
    [SerializeField] private Sprite fallbackFoodSprite;
    [SerializeField] private float defaultFoodPixelsPerUnit = 64f;
    [SerializeField] private float defaultFoodColliderRadius = 0.12f;
    [SerializeField] private float defaultFoodSinkSpeed = 0.5f;

    private Sprite generatedFallbackSprite;

    private void Update()
    {
        CheckInput();
    }

    private void CheckInput()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        Camera cameraToUse = ResolveInputCamera();
        if (cameraToUse == null)
        {
            return;
        }

        Vector3 worldPos = cameraToUse.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        HandleClick(worldPos);
    }

    private void HandleClick(Vector2 pos)
    {
        Collider2D hit = Physics2D.OverlapPoint(pos);
        if (hit == null)
        {
            SpawnFood(pos);
            return;
        }

        if (hit.CompareTag("Trash"))
        {
            if (SpawnSystem.Instance == null || !SpawnSystem.Instance.DespawnTrash(hit.gameObject))
            {
                Destroy(hit.gameObject);
            }
            return;
        }

        if (hit.CompareTag("Fish") && hit.TryGetComponent(out FishController fishController))
        {
            fishController.TriggerFear();
        }
    }

    private void SpawnFood(Vector2 pos)
    {
        if (foodPrefab != null)
        {
            GameObject spawned = Instantiate(foodPrefab, pos, Quaternion.identity, ResolveFoodParent());
            SetupFoodRuntimeComponents(spawned);
            return;
        }

        GameObject food = new GameObject($"Food_{Time.frameCount}");
        food.transform.SetParent(ResolveFoodParent());
        food.transform.position = pos;

        SpriteRenderer renderer = food.AddComponent<SpriteRenderer>();
        renderer.sprite = ResolveFoodSprite();

        CircleCollider2D collider = food.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = defaultFoodColliderRadius;

        Rigidbody2D body = food.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Kinematic;

        FoodController foodController = food.AddComponent<FoodController>();
        foodController.Initialize(defaultFoodSinkSpeed);

        TrySetTag(food, "Food");
    }

    private void SetupFoodRuntimeComponents(GameObject spawned)
    {
        if (spawned == null)
        {
            return;
        }

        CircleCollider2D collider = spawned.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = spawned.AddComponent<CircleCollider2D>();
        }

        collider.isTrigger = true;
        if (collider.radius <= 0f)
        {
            collider.radius = defaultFoodColliderRadius;
        }

        Rigidbody2D body = spawned.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            body = spawned.AddComponent<Rigidbody2D>();
        }

        body.gravityScale = 0.75f;
        body.bodyType = RigidbodyType2D.Kinematic;

        FoodController foodController = spawned.GetComponent<FoodController>();
        if (foodController == null)
        {
            foodController = spawned.AddComponent<FoodController>();
        }

        foodController.Initialize(defaultFoodSinkSpeed);
        TrySetTag(spawned, "Food");
    }

    private Transform ResolveFoodParent()
    {
        if (foodParent != null)
        {
            return foodParent;
        }

        if (SpawnSystem.Instance != null && SpawnSystem.Instance.spawnParent != null)
        {
            return SpawnSystem.Instance.spawnParent;
        }

        return null;
    }

    private Camera ResolveInputCamera()
    {
        if (inputCamera != null)
        {
            return inputCamera;
        }

        return Camera.main;
    }

    private Sprite ResolveFoodSprite()
    {
        if (fallbackFoodSprite != null)
        {
            return fallbackFoodSprite;
        }

        if (generatedFallbackSprite != null)
        {
            return generatedFallbackSprite;
        }

        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color color = new Color(0.98f, 0.85f, 0.35f, 1f);
        Color[] pixels = new Color[1024];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        generatedFallbackSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            defaultFoodPixelsPerUnit);

        return generatedFallbackSprite;
    }

    private void TrySetTag(GameObject target, string tagName)
    {
        try
        {
            target.tag = tagName;
        }
        catch (UnityException)
        {
            Debug.LogWarning($"Tag '{tagName}' is not defined. Add it in Project Settings > Tags and Layers.");
        }
    }
}

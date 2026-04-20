using UnityEngine;

public class InputSystem : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Camera inputCamera;

    [Header("Food Spawn")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Transform foodParent;
    [SerializeField] private float defaultFoodColliderRadius = 0.12f;
    [SerializeField] private float defaultFoodSinkSpeed = 0.5f;


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
            if (SpawnSystem.Instance == null || !SpawnSystem.Instance.Despawn(hit.gameObject, hit.tag))
            {
                Debug.Log($"Trash{hit.gameObject.name} clicked but failed to despawn from pool");
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
            string objectName = $"Food";
            string poolKey = $"food::{objectName}";
            GameObject foodObject;


            if (poolKey.TryAcquireFromPool(out foodObject))
            {
                foodObject.transform.SetParent(ResolveFoodParent());
                foodObject.transform.position = pos;
                SetupFoodRuntimeComponents(foodObject);
                return;
            }
            else
            {
                foodObject = Instantiate(foodPrefab, pos, Quaternion.identity, ResolveFoodParent());
                foodObject.AssignPoolKey(poolKey);
                foodObject.transform.SetParent(ResolveFoodParent());
                foodObject.transform.position = pos;
                SetupFoodRuntimeComponents(foodObject);
                return;
            }
        }
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

        // if (SpawnSystem.Instance != null && SpawnSystem.Instance.spawnParent != null)
        // {
        //     return SpawnSystem.Instance.spawnParent;
        // }

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
}

using UnityEngine;

public class InputSystem : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Camera inputCamera;

    [Header("Refs")]
    [SerializeField] private FoodSpawner foodSpawner;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Camera cam = ResolveCamera();
        if (cam == null) return;

        Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;

        HandleClick(world);
    }

    private void HandleClick(Vector2 worldPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit == null)
        {
            foodSpawner.Spawn(worldPos);
            return;
        }

        if (hit.CompareTag("Trash"))
        {
            SpawnSystem.Instance.Despawn(hit.gameObject);
            return;
        }

        if (hit.TryGetComponent(out FishController fish))
        {
            fish.TriggerFear();
        }
    }

    private Camera ResolveCamera()
    {
        return inputCamera != null ? inputCamera : Camera.main;
    }
}
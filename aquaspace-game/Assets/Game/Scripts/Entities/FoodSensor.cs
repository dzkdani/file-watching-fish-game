using UnityEngine;

public class FoodSensor : MonoBehaviour
{
    [SerializeField] private float detectionRadius;
    [SerializeField] private LayerMask foodMask;    

    private readonly Collider2D[] buffer = new Collider2D[32];
    private ContactFilter2D filter;

    private void Awake()
    {
        foodMask = LayerMask.GetMask("Food");
        filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(foodMask);
    }

    public bool TryGetNearest(Vector2 position, out Collider2D nearest)
    {
        nearest = null;
        float best = float.MaxValue;
        foodMask = LayerMask.GetMask("Food");
        filter.SetLayerMask(foodMask);


        int count = Physics2D.OverlapCircle(position, detectionRadius, filter, buffer);

        for (int i = 0; i < count; i++)
        {
            var col = buffer[i];
            if (col == null) continue;

            float dist = ((Vector2)col.transform.position - position).sqrMagnitude;
            if (dist < best)
            {
                best = dist;
                nearest = col;
            }
        }

        return nearest != null;
    }

    public void SetRadius(float radius)
    {
        detectionRadius = radius;
    }
}
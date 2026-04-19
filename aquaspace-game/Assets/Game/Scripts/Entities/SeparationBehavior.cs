using UnityEngine;

public class SeparationBehavior : MonoBehaviour
{
    [SerializeField] private float radius = 2f;
    [SerializeField] private float strength = 1f;
    [SerializeField] private LayerMask layerMask;

    private readonly Collider2D[] buffer = new Collider2D[64];
    private ContactFilter2D filter;
    private float sqrRadius;

    private void Awake()
    {
        filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(LayerMask.GetMask("Fish", "Trash"));
        // filter.SetLayerMask(layerMask);
        sqrRadius = radius * radius;
    }

    public void SetLayerMask(LayerMask newMask)
    {
        filter.SetLayerMask(newMask);
    }

    public Vector2 Calculate(Vector2 position, Transform self)
    {
        int count = Physics2D.OverlapCircle(position, radius, filter, buffer);

        Vector2 result = Vector2.zero;

        for (int i = 0; i < count; i++)
        {
            var col = buffer[i];
            if (col == null || col.gameObject == self) continue;

            Vector2 away = position - (Vector2)col.transform.position;
            float sqrDist = away.sqrMagnitude;

            if (sqrDist <= 0.000001f) continue;

            float weight = 1f - Mathf.Clamp01(sqrDist / sqrRadius);
            result += away.normalized * weight;
        }

        if (count > 0)
            result /= count;

        return result * strength;
    }
}
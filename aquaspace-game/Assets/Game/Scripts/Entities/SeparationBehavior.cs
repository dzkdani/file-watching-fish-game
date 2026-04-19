using UnityEngine;

public static class SeparationBehavior
{
    private static readonly Collider2D[] separationBuffer = new Collider2D[32];
    private static readonly ContactFilter2D overlapFilter = new ContactFilter2D { useTriggers = true };

    private static Vector2 ComputeSeparation(Transform transform, float separationRadius, string[] targetTag)
    {
        Vector2 result = Vector2.zero;
        int count = Physics2D.OverlapCircle(transform.position, separationRadius, overlapFilter, separationBuffer);

        for (int i = 0; i < count; i++)
        {
            Collider2D collider = separationBuffer[i];
            if (collider == null || collider.gameObject == transform.gameObject)
            {
                continue;
            }

            bool isTargetTag = false;
            foreach (string tag in targetTag)
            {
                if (collider.CompareTag(tag))
                {
                    isTargetTag = true;
                    break;
                }
            }

            if (!isTargetTag)
            {
                continue;
            }

            Vector2 away = (Vector2)transform.position - (Vector2)collider.transform.position;
            float distance = away.magnitude;
            if (distance <= 0.001f)
            {
                continue;
            }

            float weight = 1f - Mathf.Clamp01(distance / separationRadius);
            result += (away / distance) * weight;
        }

        return result;
    }
}
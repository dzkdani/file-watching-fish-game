
using UnityEngine;
public static class BoundsUtility
{
    public static bool TryGetBounds(out Rect bounds)
    {
        if (SpawnSystem.Instance != null &&
            SpawnSystem.Instance.TryGetMovementBounds(out bounds))
        {
            return true;
        }

        bounds = Rect.MinMaxRect(-5, -5, 5, 5);
        return true;
    }

    public static Vector3 Clamp(Vector3 pos)
    {
        if (!TryGetBounds(out var b)) return pos;

        return new Vector3(
            Mathf.Clamp(pos.x, b.xMin, b.xMax),
            Mathf.Clamp(pos.y, b.yMin, b.yMax),
            pos.z
        );
    }
}
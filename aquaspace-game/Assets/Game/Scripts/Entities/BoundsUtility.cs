using UnityEngine;

public static class BoundsUtility
{
    private static CameraBoundsUtility _service;

    public static void Initialize(CameraBoundsUtility service)
    {
        _service = service;
    }

    public static bool TryGetBounds(out Rect bounds)
    {
        if (_service != null)
        {
            bounds = _service.GetBounds(0f);
            return true;
        }

        bounds = default;
        return false;
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
using UnityEngine;

public static class BoundsBehavior
{
    [Header("Bounds")]
    private static Camera boundsCamera;
    private static readonly float spawnAreaPadding = 2.0f;
    private static readonly float movementBoundsPadding = 2.5f;

    public static bool TryGetMovementBounds(this Camera cameraToUse, out Rect bounds)
    {
        return TryGetCameraWorldRect(cameraToUse, movementBoundsPadding, out bounds);
    }

    public static bool TryGetSpawnBounds(this Camera cameraToUse, out Rect bounds)
    {
        return TryGetCameraWorldRect(cameraToUse, spawnAreaPadding, out bounds);
    }

    private static bool TryGetCameraWorldRect(Camera cameraToUse, float padding, out Rect bounds)
    {
        // Camera cameraToUse = ResolveBoundsCamera();
        if (cameraToUse == null)
        {
            bounds = default;
            return false;
        }

        Vector3 bottomLeft = cameraToUse.ScreenToWorldPoint(Vector3.zero);
        Vector3 topRight = cameraToUse.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f));

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
}
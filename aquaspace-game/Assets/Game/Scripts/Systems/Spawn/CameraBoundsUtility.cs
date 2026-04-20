using UnityEngine;

public class CameraBoundsUtility
{
    private Camera cam;
    private float padding;

    public CameraBoundsUtility(Camera cam, float padding)
    {
        this.cam = cam;
        this.padding = padding;
    }

    public Rect GetBounds(float z)
    {
        float depth = Mathf.Abs(cam.transform.position.z - z);

        Vector3 bl = cam.ScreenToWorldPoint(new Vector3(0, 0, depth));
        Vector3 tr = cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, depth));

        return Rect.MinMaxRect(
            bl.x + padding,
            bl.y + padding,
            tr.x - padding,
            tr.y - padding);
    }
}
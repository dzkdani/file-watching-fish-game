using UnityEngine;

public class MovementController : MonoBehaviour
{
    private Vector2 velocity;

    public void Move(Vector2 direction, float speed, float deltaTime)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon || speed <= 0f)
            return;

        velocity = direction * speed;
        Vector3 next = transform.position + (Vector3)(velocity * deltaTime);
        transform.position = BoundsUtility.Clamp(next);
    }
}
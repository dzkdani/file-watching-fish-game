using UnityEngine;

public class TrashController : MonoBehaviour
{
    private Vector3 direction;
    private float speed;

    void Float()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}
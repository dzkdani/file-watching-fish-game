using UnityEngine;

public class FoodController : MonoBehaviour
{
    [SerializeField] private float sinkSpeed = 0.5f;

    public void Initialize(float newSinkSpeed)
    {
        if (newSinkSpeed > 0f)
        {
            sinkSpeed = newSinkSpeed;
        }
    }

    private void Update()
    {
        transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
    }
}

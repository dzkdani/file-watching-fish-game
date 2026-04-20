using UnityEngine;

public class FoodController : MonoBehaviour, IPoolable
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
        Sink();
        DespawnFood();
    }

    private void Sink()
    {
        transform.position += Vector3.down * sinkSpeed * Time.deltaTime;
    }

    private void DespawnFood()
    {
        if (transform.position.y < -10f)
        {
            ReturnToPool();
        }
    }

    public void ReturnToPool()
    {
        gameObject.ReturnToPool();
    }
}

using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Food")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private Transform foodParent;

    private const string POOL_KEY = "food";

    public void Spawn(Vector2 position)
    {
        if (foodPrefab == null) return;

        GameObject food;

        if (POOL_KEY.TryAcquireFromPool(out food))
        {
            Setup(food, position);
        }
        else
        {
            food = Instantiate(foodPrefab, position, Quaternion.identity, foodParent);
            food.AssignPoolKey(POOL_KEY);
            Setup(food, position);
        }
    }

    private void Setup(GameObject food, Vector2 position)
    {
        food.transform.SetParent(foodParent);
        food.transform.position = position;

        food.SetActive(true);
    }
}
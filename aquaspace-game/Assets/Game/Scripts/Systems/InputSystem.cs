using UnityEngine;

public class InputSystem : MonoBehaviour
{
    void Update()
    {
        
    }

    void CheckInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            HandleClick(worldPos);

        }
    }

    void HandleClick(Vector2 pos)
    {
        Collider2D hit = Physics2D.OverlapPoint(pos);

        if(hit == null)
        {
            SpawnFood(pos);
        }
        else if(hit.CompareTag("Trash"))
        {
            Destroy(hit.gameObject);
        }
        else if(hit.CompareTag("Fish"))
        {
            hit.GetComponent<FishController>().TriggerFear();
        }
    }

    void SpawnFood(Vector2 pos)
    {
        // Instantiate food prefab at pos
    }
}
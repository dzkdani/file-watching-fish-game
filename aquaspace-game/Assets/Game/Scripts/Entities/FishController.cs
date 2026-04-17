using UnityEngine;
using System.Linq;
using System.Collections;

public class FishController : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float minSpeed;
    private float maxSpeed;
    private float detectionRadius;
    private float hunger;
    private Collider2D[] foods;

    void Update()
    {
        Move();
        hunger -= Time.deltaTime;
        if (hunger <= 0)
        {
            SeekFood();
        }
        else
        {
            Move();
        }
    }

    void Move()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void SeekFood()
    {
        var nearest = foods
            .Where(f => f.CompareTag("Food"))
            .OrderBy(f => Vector2.Distance(transform.position, f.transform.position))
            .FirstOrDefault();
        
        direction = (nearest.transform.position - transform.position).normalized;
    }

    public void TriggerFear()
    {
        StartCoroutine(FearRoutine());
    }

    IEnumerator FearRoutine()
    {
        speed *= 2f;
        yield return new WaitForSeconds(2f);
        speed /= 2f;
    }


}
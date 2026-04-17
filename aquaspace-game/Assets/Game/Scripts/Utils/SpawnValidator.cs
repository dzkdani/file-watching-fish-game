using UnityEngine;

public class SpawnValidator 
{
    Vector3 GetValidPosition(Vector2 minSpawn, Vector2 maxSpawn)
    {
        for(int i=0;i<10;i++)
        {
            Vector3 pos = new Vector3(
                UnityEngine.Random.Range(minSpawn.x, maxSpawn.x),
                UnityEngine.Random.Range(minSpawn.y, maxSpawn.y),
                0);

            if(IsValid(pos)) return pos;
        }

        return Vector3.zero;
    }

    bool IsValid(Vector3 pos)
    {
        return Physics2D.OverlapCircle(pos, 0.5f) == null;
    }
}


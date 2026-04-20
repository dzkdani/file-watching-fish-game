using UnityEngine;
using System.Collections.Generic;


public class SpatialGrid
{
    private readonly Dictionary<Vector2Int, List<GameObject>> grid = new();
    private float cellSize;

    public SpatialGrid(float cellSize)
    {
        this.cellSize = cellSize;
    }

    private Vector2Int GetCell(Vector2 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / cellSize),
            Mathf.FloorToInt(pos.y / cellSize));
    }

    public void Register(GameObject obj)
    {
        var cell = GetCell(obj.transform.position);
        if (!grid.TryGetValue(cell, out var list))
        {
            list = new List<GameObject>();
            grid[cell] = list;
        }
        list.Add(obj);
    }

    public void Unregister(GameObject obj)
    {
        var cell = GetCell(obj.transform.position);
        if (grid.TryGetValue(cell, out var list))
        {
            list.Remove(obj);
        }
    }

    public bool IsOccupied(Vector2 position, float radius)
    {
        var cell = GetCell(position);

        if (!grid.TryGetValue(cell, out var list))
            return false;

        foreach (var obj in list)
        {
            if (Vector2.Distance(obj.transform.position, position) < radius)
                return true;
        }

        return false;
    }
}
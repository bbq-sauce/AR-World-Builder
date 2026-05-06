using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages an invisible world-space grid.
/// Each cell is cellSize x cellSize units (metres in AR space).
/// Objects reserve grid cells based on their footprint size.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    [Tooltip("Size of one grid cell in world units (metres).")]
    public float cellSize = 0.1f; // 10 cm per cell

    // Key = grid coordinate, Value = ID of object occupying it
    private Dictionary<Vector2Int, string> occupiedCells = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // -------------------------------------------------------
    // Convert a world position to the nearest grid coordinate
    // -------------------------------------------------------
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / cellSize);
        int z = Mathf.RoundToInt(worldPos.z / cellSize);
        return new Vector2Int(x, z);
    }

    // Snap a world position to the nearest grid centre
    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        Vector2Int cell = WorldToGrid(worldPos);
        return new Vector3(cell.x * cellSize, worldPos.y, cell.y * cellSize);
    }

    // -------------------------------------------------------
    // Compute all cells an object would occupy given its
    // world-space footprint (width & depth in metres).
    // -------------------------------------------------------
    public List<Vector2Int> GetRequiredCells(Vector3 worldPos, Vector2 footprintMetres)
    {
        var cells = new List<Vector2Int>();

        int halfW = Mathf.CeilToInt((footprintMetres.x / cellSize) / 2f);
        int halfD = Mathf.CeilToInt((footprintMetres.y / cellSize) / 2f);
        Vector2Int origin = WorldToGrid(worldPos);

        for (int x = -halfW; x <= halfW; x++)
            for (int z = -halfD; z <= halfD; z++)
                cells.Add(new Vector2Int(origin.x + x, origin.y + z));

        return cells;
    }

    // -------------------------------------------------------
    // Check whether a placement is free
    // -------------------------------------------------------
    public bool CanPlace(Vector3 worldPos, Vector2 footprintMetres)
    {
        foreach (var cell in GetRequiredCells(worldPos, footprintMetres))
            if (occupiedCells.ContainsKey(cell)) return false;
        return true;
    }

    // -------------------------------------------------------
    // Reserve cells for a placed object
    // -------------------------------------------------------
    public void OccupyCells(string objectId, Vector3 worldPos, Vector2 footprintMetres)
    {
        foreach (var cell in GetRequiredCells(worldPos, footprintMetres))
            occupiedCells[cell] = objectId;
    }

    // -------------------------------------------------------
    // Free cells when an object is removed
    // -------------------------------------------------------
    public void FreeCells(string objectId)
    {
        var toRemove = new List<Vector2Int>();
        foreach (var kvp in occupiedCells)
            if (kvp.Value == objectId) toRemove.Add(kvp.Key);
        foreach (var cell in toRemove)
            occupiedCells.Remove(cell);
    }

    // -------------------------------------------------------
    // Optional: debug draw grid in Scene view
    // -------------------------------------------------------
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        foreach (var kvp in occupiedCells)
        {
            Vector3 centre = new Vector3(kvp.Key.x * cellSize, 0f, kvp.Key.y * cellSize);
            Gizmos.DrawCube(centre, new Vector3(cellSize * 0.95f, 0.005f, cellSize * 0.95f));
        }
    }
#endif
}

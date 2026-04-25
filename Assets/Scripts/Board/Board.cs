using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Dimensions")]
    public int width = 8;
    public int height = 16;

    // The grid: null = empty, otherwise holds a reference to the object in that cell
    private Transform[,] grid;

    void Awake()
    {
        grid = new Transform[width, height];
    }

    // Convert a world position to grid coordinates
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x - transform.position.x),
            Mathf.RoundToInt(worldPos.y - transform.position.y)
        );
    }

    // Check if a grid position is within bounds and unoccupied
    public bool IsValidPosition(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= width) return false;
        if (cell.y < 0 || cell.y >= height) return false;
        return grid[cell.x, cell.y] == null;
    }

    // Lock a transform into the grid
    public void PlaceInGrid(Transform piece, Vector2Int cell)
    {
        grid[cell.x, cell.y] = piece;
    }

    // Remove from grid (after a match)
    public void ClearCell(Vector2Int cell)
    {
        grid[cell.x, cell.y] = null;
    }

    public Transform GetCell(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            return null;
        return grid[cell.x, cell.y];
    }
}
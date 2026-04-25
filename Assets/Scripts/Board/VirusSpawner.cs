using UnityEngine;
using System.Collections.Generic;

public class VirusSpawner : MonoBehaviour
{
    private Board board;

    [Header("Level Settings")]
    public int level = 1; // 1-20 like Dr Mario

    // Dr Mario virus count formula: 4 + (level * 4), capped at 84
    public int VirusCount => Mathf.Min(4 + (level * 4), 84);

    // How high viruses can spawn — increases with level
    // Level 1: up to row 9, Level 20: up to row 15
    public int MaxVirusRow => Mathf.Min(9 + (level - 1), visibleHeightLimit);
    private int visibleHeightLimit;

    void Awake()
    {
        board = GetComponent<Board>();
    }

    public void SpawnViruses()
    {
        visibleHeightLimit = board.visibleHeight - 2; // keep top rows clear for capsule entry
        
        List<Vector2Int> candidates = GetCandidateCells();
        Shuffle(candidates);

        int spawned = 0;
        int attempts = 0;

        foreach (Vector2Int cell in candidates)
        {
            if (spawned >= VirusCount) break;

            Cell.CellColor color = PickColorAvoidingMatches(cell);
            SpawnVirus(cell, color);
            spawned++;
        }

        Debug.Log($"Spawned {spawned} viruses for level {level}");
    }

    List<Vector2Int> GetCandidateCells()
    {
        List<Vector2Int> cells = new List<Vector2Int>();

        for (int x = 0; x < board.width; x++)
        {
            for (int y = 1; y <= MaxVirusRow; y++) // start at row 1, leave bottom row clear
            {
                cells.Add(new Vector2Int(x, y));
            }
        }

        return cells;
    }

    // Pick a color that won't immediately create a match of 3 or more
    Cell.CellColor PickColorAvoidingMatches(Vector2Int cell)
    {
        List<Cell.CellColor> colors = new List<Cell.CellColor>
        {
            Cell.CellColor.Red,
            Cell.CellColor.Yellow,
            Cell.CellColor.Blue
        };

        Shuffle(colors);

        foreach (Cell.CellColor color in colors)
        {
            if (!WouldCreateRun(cell, color, 3))
                return color;
        }

        return colors[0]; // fallback
    }

    // Check if placing this color would create a run of `length` or more
    bool WouldCreateRun(Vector2Int cell, Cell.CellColor color, int length)
    {
        return CountRun(cell, Vector2Int.right, color) + 
               CountRun(cell, Vector2Int.left, color) + 1 >= length
            || CountRun(cell, Vector2Int.up, color) + 
               CountRun(cell, Vector2Int.down, color) + 1 >= length;
    }

    int CountRun(Vector2Int start, Vector2Int direction, Cell.CellColor color)
    {
        int count = 0;
        Vector2Int current = start + direction;

        while (true)
        {
            Transform t = board.GetCell(current);
            if (t == null) break;

            Cell cell = t.GetComponent<Cell>();
            if (cell == null || cell.cellColor != color) break;

            count++;
            current += direction;
        }

        return count;
    }

    void SpawnVirus(Vector2Int cell, Cell.CellColor color)
    {
        Vector3 worldPos = board.GridToWorld(cell);
        GameObject obj = Instantiate(board.cellPrefab, worldPos, Quaternion.identity);
        obj.transform.SetParent(board.transform);

        Cell cellComponent = obj.GetComponent<Cell>();
        cellComponent.Init(Cell.CellType.Virus, color);
        board.PlaceInGrid(obj.transform, cell);
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
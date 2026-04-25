using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchDetector : MonoBehaviour
{
    private Board board;

    void Awake()
    {
        board = GetComponent<Board>();
    }

    public void RunMatchDetection()
    {
        HashSet<Vector2Int> toDestroy = new HashSet<Vector2Int>();

        // Scan every cell for horizontal and vertical matches
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (board.GetCell(cell) == null) continue;

                Cell.CellColor color = GetColor(cell);

                // Check horizontal run starting at this cell
                List<Vector2Int> hRun = GetRun(cell, Vector2Int.right, color);
                if (hRun.Count >= 4) toDestroy.UnionWith(hRun);

                // Check vertical run starting at this cell
                List<Vector2Int> vRun = GetRun(cell, Vector2Int.up, color);
                if (vRun.Count >= 4) toDestroy.UnionWith(vRun);
            }
        }

        if (toDestroy.Count > 0)
            StartCoroutine(ClearAndSettle(toDestroy));
    }

    // Follow a direction and collect consecutive cells of the same color
    List<Vector2Int> GetRun(Vector2Int start, Vector2Int direction, Cell.CellColor color)
    {
        List<Vector2Int> run = new List<Vector2Int>();
        Vector2Int current = start;

        while (true)
        {
            Transform t = board.GetCell(current);
            if (t == null) break;

            Cell cell = t.GetComponent<Cell>();
            if (cell == null || cell.cellColor != color) break;

            run.Add(current);
            current += direction;
        }

        return run;
    }

    Cell.CellColor GetColor(Vector2Int cell)
    {
        return board.GetCell(cell).GetComponent<Cell>().cellColor;
    }

    IEnumerator ClearAndSettle(HashSet<Vector2Int> toDestroy)
    {
        // Brief pause so the player can see what's being cleared
        yield return new WaitForSeconds(0.2f);

        // Destroy matched cells
        foreach (Vector2Int cell in toDestroy)
        {
            Transform t = board.GetCell(cell);
            if (t != null)
            {
                board.ClearCell(cell);
                Destroy(t.gameObject);
            }
        }

        // Wait for gravity to settle
        yield return new WaitForSeconds(0.1f);
        ApplyGravity();

        // Check for chain reactions
        yield return new WaitForSeconds(0.3f);
        RunMatchDetection();
    }

    void ApplyGravity()
    {
        // Work bottom-up so falling cells don't overwrite each other
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 1; y < board.height; y++)
            {
                if (board.GetCell(new Vector2Int(x, y)) == null) continue;

                // Find how far this cell can fall
                int dropTo = y;
                while (dropTo - 1 >= 0 && board.GetCell(new Vector2Int(x, dropTo - 1)) == null)
                    dropTo--;

                if (dropTo == y) continue; // didn't move

                // Move in grid
                Vector2Int from = new Vector2Int(x, y);
                Vector2Int to = new Vector2Int(x, dropTo);
                Transform t = board.GetCell(from);
                board.ClearCell(from);
                board.PlaceInGrid(t, to);

                // Move visually
                t.position = board.GridToWorld(to);
            }
        }
    }
}
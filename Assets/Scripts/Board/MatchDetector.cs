using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchDetector : MonoBehaviour
{
    private Board board;
    private GameLoop gameLoop;

    void Awake()
    {
        board = GetComponent<Board>();
        gameLoop = GetComponent<GameLoop>();
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
        else if (!board.HasViruses())
            gameLoop.OnWin();
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
        // pause before clear
        yield return new WaitForSeconds(.5f);

        // Sever partner links and clear
        foreach (Vector2Int cell in toDestroy)
        {
            Transform t = board.GetCell(cell);
            if (t != null)
            {
                Cell cellComponent = t.GetComponent<Cell>();

                // Tell the partner it's now independent
                if (cellComponent.partner != null)
                {
                    cellComponent.partner.partner = null;
                    cellComponent.partner = null;
                }

                board.ClearCell(cell);
                Destroy(t.gameObject);
            }
        }

        // pause before applying gravity
        yield return new WaitForSeconds(0.1f);
        ApplyGravity();

        // pause before running new match detection
        yield return new WaitForSeconds(0.5f);
        RunMatchDetection();
    }

    void ApplyGravity()
    {
        // Work bottom to top
        for (int y = 1; y < board.height; y++)
        {
            for (int x = 0; x < board.width; x++)
            {
                Vector2Int cellPos = new Vector2Int(x, y);
                Transform t = board.GetCell(cellPos);
                if (t == null) continue;

                Cell cell = t.GetComponent<Cell>();
                if (cell == null || cell.cellType == Cell.CellType.Virus) continue;

                bool hasSupport = cellPos.y > 0 && 
                                board.GetCell(new Vector2Int(x, cellPos.y - 1)) != null;

                if (hasSupport) continue;

                // If this cell has a living partner, only fall if partner 
                // also has no support
                if (cell.partner != null)
                {
                    Vector2Int partnerPos = board.WorldToGrid(cell.partner.transform.position);
                    Vector2Int belowPartner = new Vector2Int(partnerPos.x, partnerPos.y - 1);
                    // Exclude the current cell itself — for vertical capsules, the cell
                    // directly below the partner is this cell, which is not external support.
                    bool partnerHasSupport = partnerPos.y > 0 &&
                                            belowPartner != cellPos &&
                                            board.GetCell(belowPartner) != null;
                    if (partnerHasSupport) continue; // partner is supported, neither falls
                }

                // Fall
                int dropTo = cellPos.y;
                while (dropTo - 1 >= 0 && 
                    board.GetCell(new Vector2Int(x, dropTo - 1)) == null)
                    dropTo--;

                if (dropTo == cellPos.y) continue;

                Vector2Int to = new Vector2Int(x, dropTo);
                board.ClearCell(cellPos);
                board.PlaceInGrid(t, to);
                StartCoroutine(AnimateFall(t, board.GridToWorld(to)));
            }
        }
    }

    IEnumerator AnimateFall(Transform piece, Vector3 destination)
    {
        float speed = 8f; // units per second — tweak to taste
        float distance = Vector3.Distance(piece.position, destination);
        float duration = distance / speed; // further = longer

        float elapsed = 0f;
        Vector3 start = piece.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            piece.position = Vector3.Lerp(start, destination, t);
            yield return null;
        }

        piece.position = destination;
    }
}
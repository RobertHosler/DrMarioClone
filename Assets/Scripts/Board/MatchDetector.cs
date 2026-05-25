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

    public void RunMatchDetection(bool isCascade = false)
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
            StartCoroutine(ClearAndSettle(toDestroy, isCascade));
        else if (!board.HasViruses())
            gameLoop.OnWin();
        else
            board.OnCascadeComplete();
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

    IEnumerator ClearAndSettle(HashSet<Vector2Int> toDestroy, bool isCascade)
    {
        // pause before clear
        yield return new WaitForSeconds(.5f);

        if (isCascade)
            gameLoop.AddScore(1000);

        int virusesCleared = 0;
        int capsulesCleared = 0;
        foreach (Vector2Int cell in toDestroy)
        {
            Transform t = board.GetCell(cell);
            if (t == null) continue;
            if (t.GetComponent<Cell>().cellType == Cell.CellType.Virus)
                virusesCleared++;
            else
                capsulesCleared++;
        }
        gameLoop.AddScore(virusesCleared * 1000 + capsulesCleared * 100);

        // Sever partner links and clear
        foreach (Vector2Int cell in toDestroy)
        {
            Transform t = board.GetCell(cell);
            if (t != null)
            {
                Cell cellComponent = t.GetComponent<Cell>();

                if (cellComponent.partner != null)
                {
                    cellComponent.partner.isSplit = true;
                    cellComponent.partner.ApplyVisuals();
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
        RunMatchDetection(isCascade: true);
    }

    void ApplyGravity()
    {
        for (int y = 1; y < board.height; y++)
        {
            for (int x = 0; x < board.width; x++)
            {
                Vector2Int cellPos = new Vector2Int(x, y);
                Transform t = board.GetCell(cellPos);
                if (t == null) continue;

                Cell cell = t.GetComponent<Cell>();
                if (cell == null || cell.cellType == Cell.CellType.Virus) continue;

                if (cell.partner != null)
                {
                    // Intact pair — move both halves by the same distance
                    Vector2Int partnerPos = cell.partner.gridPosition;
                    bool isVertical = partnerPos.x == cellPos.x;

                    int drop;
                    if (isVertical)
                    {
                        // Bottom-to-top scan means cellPos is always the lower half
                        drop = GetDropDistance(cellPos.x, cellPos.y);
                    }
                    else
                    {
                        drop = Mathf.Min(
                            GetDropDistance(cellPos.x, cellPos.y),
                            GetDropDistance(partnerPos.x, partnerPos.y)
                        );
                    }

                    if (drop == 0) continue;

                    MoveCell(t, cellPos, drop);
                    MoveCell(cell.partner.transform, partnerPos, drop);
                }
                else
                {
                    // Solo (split or never had a partner) — fall independently
                    int drop = GetDropDistance(cellPos.x, cellPos.y);
                    if (drop == 0) continue;
                    MoveCell(t, cellPos, drop);
                }
            }
        }
    }

    int GetDropDistance(int x, int y)
    {
        int drop = 0;
        while (y - drop - 1 >= 0 && board.GetCell(new Vector2Int(x, y - drop - 1)) == null)
            drop++;
        return drop;
    }

    void MoveCell(Transform t, Vector2Int from, int drop)
    {
        Vector2Int to = new Vector2Int(from.x, from.y - drop);
        board.ClearCell(from);
        board.PlaceInGrid(t, to);
        StartCoroutine(AnimateFall(t, board.GridToWorld(to)));
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
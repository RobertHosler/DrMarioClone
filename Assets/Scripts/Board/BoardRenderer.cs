using UnityEngine;

[RequireComponent(typeof(Board))]
public class BoardRenderer : MonoBehaviour
{
    [Header("Visuals")]
    public Color gridColor = new Color(1f, 1f, 1f, 0.15f);  // faint white
    public Color borderColor = new Color(1f, 1f, 1f, 0.8f); // bright white

    private Board board;

    void Awake()
    {
        board = GetComponent<Board>();
    }

    // OnDrawGizmos runs in the editor even while not playing — great for layout
    void OnDrawGizmos()
    {
        if (board == null) board = GetComponent<Board>();
        if (board == null) return;

        DrawGrid();
        DrawBorder();
    }

    void DrawGrid()
    {
        Gizmos.color = gridColor;

        // Vertical lines
        for (int x = 0; x <= board.width; x++)
        {
            Vector3 start = transform.position + new Vector3(x, 0, 0);
            Vector3 end   = transform.position + new Vector3(x, board.visibleHeight, 0);
            Gizmos.DrawLine(start, end);
        }

        // Horizontal lines
        for (int y = 0; y <= board.visibleHeight; y++)
        {
            Vector3 start = transform.position + new Vector3(0, y, 0);
            Vector3 end   = transform.position + new Vector3(board.width, y, 0);
            Gizmos.DrawLine(start, end);
        }
    }

    void DrawBorder()
    {
        Gizmos.color = borderColor;

        Vector3 origin = transform.position;
        Vector3 topLeft     = origin + new Vector3(0, board.visibleHeight, 0);
        Vector3 topRight    = origin + new Vector3(board.width, board.visibleHeight, 0);
        Vector3 bottomRight = origin + new Vector3(board.width, 0, 0);

        Gizmos.DrawLine(origin, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, origin);
    }
}
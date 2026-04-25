using UnityEngine;

public class Capsule : MonoBehaviour
{
    [Header("References")]
    public Cell cellA;  // left / bottom cell
    public Cell cellB;  // right / top cell
    public Board board;

    [Header("Timing")]
    public float fallInterval = 0.8f;  // seconds between each downward step
    public float softDropInterval = 0.05f; // how fast it falls while down is held

    // 0=horizontal(A left, B right), 1=vertical(A bottom, B top)
    // 2=horizontal(A right, B left), 3=vertical(A top, B bottom)
    private int rotation = 0;
    private float fallTimer = 0f;
    private float softDropTimer = 0f;
    private bool locked = false;

    void Update()
    {
        if (locked) return;

        HandleInput();

        fallTimer += Time.deltaTime;
        if (fallTimer >= fallInterval)
        {
            fallTimer = 0f;
            TryFall();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))  TryMove(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryMove(Vector2Int.right);
        if (Input.GetKeyDown(KeyCode.UpArrow))    TryRotate();
        
        // Soft drop — held down key
        if (Input.GetKey(KeyCode.DownArrow))
        {
            softDropTimer += Time.deltaTime;
            if (softDropTimer >= softDropInterval)
            {
                softDropTimer = 0f;
                TryFall();
                fallTimer = 0f; // reset normal fall so it doesn't double-drop
            }
        }
        else
        {
            softDropTimer = 0f; // reset when key released
        }

    }

    void TryMove(Vector2Int direction)
    {
        Vector3 move = new Vector3(direction.x, direction.y, 0);
        transform.position += move;

        if (!IsValidPosition())
            transform.position -= move; // undo if invalid
    }

    void TryFall()
    {
        transform.position += Vector3.down;

        if (!IsValidPosition())
        {
            transform.position -= Vector3.down; // undo
            LockCapsule();
        }
    }

    void TryRotate()
    {
        int previousRotation = rotation;
        rotation = (rotation + 1) % 4;
        ApplyRotation();

        if (!IsValidPosition())
        {
            // Try wall kick — nudge left or right to make room
            transform.position += Vector3.right;
            if (!IsValidPosition())
            {
                transform.position -= Vector3.right;
                transform.position += Vector3.left;
                if (!IsValidPosition())
                {
                    transform.position -= Vector3.left;
                    // Still invalid — undo rotation
                    rotation = previousRotation;
                    ApplyRotation();
                }
            }
        }
    }

    void ApplyRotation()
    {
        // All positions relative to cellA (the anchor)
        switch (rotation)
        {
            case 0: // horizontal: A left, B right
                cellA.transform.localPosition = Vector3.zero;
                cellB.transform.localPosition = Vector3.right;
                break;
            case 1: // vertical: A bottom, B top
                cellA.transform.localPosition = Vector3.zero;
                cellB.transform.localPosition = Vector3.up;
                break;
            case 2: // horizontal flipped: A right, B left
                cellA.transform.localPosition = Vector3.right;
                cellB.transform.localPosition = Vector3.zero;
                break;
            case 3: // vertical flipped: A top, B bottom
                cellA.transform.localPosition = Vector3.up;
                cellB.transform.localPosition = Vector3.zero;
                break;
        }
    }

    bool IsValidPosition()
    {
        foreach (Transform cell in new[] { cellA.transform, cellB.transform })
        {
            Vector2Int gridPos = board.WorldToGrid(cell.position);
            if (!board.IsValidPosition(gridPos)) return false;
        }
        return true;
    }

    void LockCapsule()
    {
        locked = true;

        // Link the two halves as partners
        cellA.partner = cellB;
        cellB.partner = cellA;

        foreach (Cell cell in new[] { cellA, cellB })
        {
            Vector2Int gridPos = board.WorldToGrid(cell.transform.position);
            cell.transform.SetParent(board.transform);
            board.PlaceInGrid(cell.transform, gridPos);
        }

        board.OnCapsuleLocked();
        Destroy(gameObject);
    }
}
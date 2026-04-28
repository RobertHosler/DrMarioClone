using UnityEngine;
using UnityEngine.InputSystem;

public class Capsule : MonoBehaviour
{
    [Header("References")]
    public Cell cellA;  // left / bottom cell
    public Cell cellB;  // right / top cell
    public Board board;

    [Header("Timing")]
    public float fallInterval = 0.8f;
    public float softDropInterval = 0.05f;
    public float dasDelay = 0.2f;
    public float dasInterval = 0.08f;

    private InputAction moveLeftAction;
    private InputAction moveRightAction;
    private InputAction softDropAction;
    private InputAction rotateAction;
    private InputAction rotateReverseAction;

    private float leftTimer = 0f;
    private float rightTimer = 0f;

    // 0=horizontal(A left, B right), 1=vertical(A bottom, B top)
    // 2=horizontal(A right, B left), 3=vertical(A top, B bottom)
    private int rotation = 0;
    private float fallTimer = 0f;
    private float softDropTimer = 0f;
    private bool locked = false;

    public void SetActions(InputAction left, InputAction right, InputAction down, InputAction rotate, InputAction rotateReverse)
    {
        moveLeftAction      = left;
        moveRightAction     = right;
        softDropAction      = down;
        rotateAction        = rotate;
        rotateReverseAction = rotateReverse;
    }

    void Update()
    {
        if (locked || moveLeftAction == null || Time.timeScale == 0f) return;

        HandleInput();

        fallTimer += Time.deltaTime;
        if (fallTimer >= fallInterval)
        {
            fallTimer = 0f;
            TryFall();
        }
    }

    public void UpdateCellEnds()
    {
        switch (rotation)
        {
            case 0:
                cellA.SetCapsuleEnd(Cell.CapsuleEnd.Left);
                cellB.SetCapsuleEnd(Cell.CapsuleEnd.Right);
                break;
            case 1:
                cellA.SetCapsuleEnd(Cell.CapsuleEnd.Bottom);
                cellB.SetCapsuleEnd(Cell.CapsuleEnd.Top);
                break;
            case 2:
                cellA.SetCapsuleEnd(Cell.CapsuleEnd.Right);
                cellB.SetCapsuleEnd(Cell.CapsuleEnd.Left);
                break;
            case 3:
                cellA.SetCapsuleEnd(Cell.CapsuleEnd.Top);
                cellB.SetCapsuleEnd(Cell.CapsuleEnd.Bottom);
                break;
        }
    }

    void HandleInput()
    {
        bool leftHeld  = moveLeftAction.IsPressed();
        bool rightHeld = moveRightAction.IsPressed();

        if (leftHeld && rightHeld)
        {
            leftTimer = 0f;
            rightTimer = 0f;
        }
        else
        {
            if (moveLeftAction.WasPressedThisFrame())
            {
                TryMove(Vector2Int.left);
                leftTimer = -dasDelay;
            }
            else if (leftHeld)
            {
                leftTimer += Time.deltaTime;
                if (leftTimer >= dasInterval) { leftTimer = 0f; TryMove(Vector2Int.left); }
            }
            else leftTimer = 0f;

            if (moveRightAction.WasPressedThisFrame())
            {
                TryMove(Vector2Int.right);
                rightTimer = -dasDelay;
            }
            else if (rightHeld)
            {
                rightTimer += Time.deltaTime;
                if (rightTimer >= dasInterval) { rightTimer = 0f; TryMove(Vector2Int.right); }
            }
            else rightTimer = 0f;
        }

        if (rotateAction.WasPressedThisFrame())        TryRotate(1);
        if (rotateReverseAction.WasPressedThisFrame()) TryRotate(-1);

        if (softDropAction.IsPressed())
        {
            softDropTimer += Time.deltaTime;
            if (softDropTimer >= softDropInterval)
            {
                softDropTimer = 0f;
                TryFall();
                fallTimer = 0f;
            }
        }
        else softDropTimer = 0f;
    }

    void TryMove(Vector2Int direction)
    {
        Vector3 move = new Vector3(direction.x, direction.y, 0);
        transform.position += move;

        if (!IsValidPosition())
            transform.position -= move;
    }

    void TryFall()
    {
        transform.position += Vector3.down;

        if (!IsValidPosition())
        {
            transform.position -= Vector3.down;
            LockCapsule();
        }
    }

    void TryRotate(int direction)
    {
        int previousRotation = rotation;
        rotation = (rotation + direction + 4) % 4;
        ApplyRotation();

        if (!IsValidPosition())
        {
            transform.position += Vector3.right;
            if (!IsValidPosition())
            {
                transform.position -= Vector3.right;
                transform.position += Vector3.left;
                if (!IsValidPosition())
                {
                    transform.position -= Vector3.left;
                    rotation = previousRotation;
                    ApplyRotation();
                }
            }
        }
    }

    void ApplyRotation()
    {
        switch (rotation)
        {
            case 0:
                cellA.transform.localPosition = Vector3.zero;
                cellB.transform.localPosition = Vector3.right;
                break;
            case 1:
                cellA.transform.localPosition = Vector3.zero;
                cellB.transform.localPosition = Vector3.up;
                break;
            case 2:
                cellA.transform.localPosition = Vector3.right;
                cellB.transform.localPosition = Vector3.zero;
                break;
            case 3:
                cellA.transform.localPosition = Vector3.up;
                cellB.transform.localPosition = Vector3.zero;
                break;
        }
        UpdateCellEnds();
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

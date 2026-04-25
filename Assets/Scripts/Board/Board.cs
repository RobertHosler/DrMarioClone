using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Dimensions")]
    public int width = 8;
    public int height = 16;

    [Header("Prefabs")]
    public GameObject cellPrefab;
    public GameObject capsulePrefab;
    
    [Header("Next Capsule Preview")]
    public Transform previewAnchor;  // empty GameObject positioned to the right of the board

    private Cell.CellColor nextColorA;
    private Cell.CellColor nextColorB;
    private Cell previewCellA;
    private Cell previewCellB;
    private bool previewInitialized = false;


    // The grid: null = empty, otherwise holds a reference to the object in that cell
    private Transform[,] grid;

    void Awake()
    {
        grid = new Transform[width, height];
    }

    void Start()
    {
        // Generate first "next" colors
        nextColorA = RandomColor();
        nextColorB = RandomColor();
        SpawnPreview();
        SpawnCapsule();
    }

    public void SpawnCapsule()
    {
        // Spawn at top center of the board
        Vector2Int spawnCell = new Vector2Int(width / 2 - 1, height - 1);
        Vector3 spawnPos = GridToWorld(spawnCell);
        GameObject obj = Instantiate(capsulePrefab, spawnPos, Quaternion.identity);

        Capsule capsule = obj.GetComponent<Capsule>();
        capsule.board = this;

        // Use the previewed colors
        capsule.cellA.Init(Cell.CellType.CapsuleHalf, nextColorA);
        capsule.cellB.Init(Cell.CellType.CapsuleHalf, nextColorB);

        // Generate the next preview
        nextColorA = RandomColor();
        nextColorB = RandomColor();
        UpdatePreview();
    }

    void SpawnPreview()
    {
        if (previewAnchor == null) return;

        GameObject objA = Instantiate(cellPrefab, previewAnchor.position, Quaternion.identity);
        GameObject objB = Instantiate(cellPrefab, previewAnchor.position + Vector3.right, Quaternion.identity);

        previewCellA = objA.GetComponent<Cell>();
        previewCellB = objB.GetComponent<Cell>();

        previewCellA.Init(Cell.CellType.CapsuleHalf, nextColorA);
        previewCellB.Init(Cell.CellType.CapsuleHalf, nextColorB);
    }

    void UpdatePreview()
    {
        if (previewCellA == null || previewCellB == null) return;
        previewCellA.Init(Cell.CellType.CapsuleHalf, nextColorA);
        previewCellB.Init(Cell.CellType.CapsuleHalf, nextColorB);
    }

    public void OnCapsuleLocked()
    {
        // We'll add match detection here next
        // For now just spawn the next capsule
        SpawnCapsule();
    }

    Cell.CellColor RandomColor()
    {
        int r = Random.Range(0, 3);
        return (Cell.CellColor)r;
    }

    // Convert a world position to grid coordinates
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x - transform.position.x - 0.5f),
            Mathf.RoundToInt(worldPos.y - transform.position.y - 0.5f)
        );
    }

    // Convert grid coordinates to a world position
    public Vector3 GridToWorld(Vector2Int cell)
    {
        return transform.position + new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0);
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
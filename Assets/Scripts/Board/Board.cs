using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour
{
    [Header("Board Dimensions")]
    public int width = 8;
    public int visibleHeight = 16;  // what the player sees
    public int bufferRows = 2;      // hidden rows above
    public int height => visibleHeight + bufferRows;  // total grid height

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

    public bool isGameOver = false;

    public Capsule activeCapsule;

    private MatchDetector matchDetector;
    private VirusSpawner virusSpawner;
    private GameLoop gameLoop;

    // The grid: null = empty, otherwise holds a reference to the object in that cell
    private Transform[,] grid;

    void Awake()
    {
        grid = new Transform[width, height];
        matchDetector = GetComponent<MatchDetector>();
        virusSpawner = GetComponent<VirusSpawner>();
        gameLoop = GetComponent<GameLoop>();
    }

    void Start()
    {
        virusSpawner.SpawnViruses();
        nextColorA = RandomColor();
        nextColorB = RandomColor();
        SpawnPreview();
        SpawnCapsule();
    }

    public void SpawnCapsule()
    {
        Vector2Int spawnCell = new Vector2Int(width / 2 - 1, visibleHeight);

        // Lose if either spawn cell is occupied
        if (!IsValidPosition(spawnCell) || !IsValidPosition(new Vector2Int(spawnCell.x + 1, spawnCell.y)))
        {
            gameLoop.OnLose();
            return;
        }

        Vector3 spawnPos = GridToWorld(spawnCell);
        GameObject obj = Instantiate(capsulePrefab, spawnPos, Quaternion.identity);

        Capsule capsule = obj.GetComponent<Capsule>();
        capsule.board = this;
        capsule.fallInterval = Mathf.Lerp(0.8f, 0.15f, virusSpawner.level / 20f);
        activeCapsule = capsule;

        // Use the previewed colors
        capsule.cellA.Init(Cell.CellType.CapsuleHalf, nextColorA);
        capsule.cellB.Init(Cell.CellType.CapsuleHalf, nextColorB);
        capsule.UpdateCellEnds();

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
        previewCellA.SetCapsuleEnd(Cell.CapsuleEnd.Left);
        previewCellB.Init(Cell.CellType.CapsuleHalf, nextColorB);
        previewCellB.SetCapsuleEnd(Cell.CapsuleEnd.Right);
    }

    void UpdatePreview()
    {
        if (previewCellA == null || previewCellB == null) return;
        previewCellA.Init(Cell.CellType.CapsuleHalf, nextColorA);
        previewCellA.SetCapsuleEnd(Cell.CapsuleEnd.Left);
        previewCellB.Init(Cell.CellType.CapsuleHalf, nextColorB);
        previewCellB.SetCapsuleEnd(Cell.CapsuleEnd.Right);
    }

    public void OnCapsuleLocked()
    {
        activeCapsule = null;
        matchDetector.RunMatchDetection();
        // Small delay before spawning next capsule so clears play out first
        StartCoroutine(SpawnAfterDelay(0.8f));
    }

    IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isGameOver)
            SpawnCapsule();
    }

    public bool HasViruses()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < visibleHeight; y++)
            {
                Transform t = grid[x, y];
                if (t != null && t.GetComponent<Cell>().cellType == Cell.CellType.Virus)
                    return true;
            }
        return false;
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
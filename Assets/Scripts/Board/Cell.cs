using UnityEngine;

public class Cell : MonoBehaviour
{
    public enum CellType { Virus, CapsuleHalf }
    public enum CellColor { Red, Yellow, Blue }

    public CellType cellType;
    public CellColor cellColor;

    public Cell partner; // the other half of this capsule, null if virus or partner destroyed

    private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite virusSprite;
    [SerializeField] private Sprite capsuleSprite;
    [SerializeField] private GameObject virusAnimatorObject; // only active for viruses

    // Match Dr Mario's classic palette
    private static readonly Color RedColor    = new Color(0.95f, 0.2f,  0.2f);
    private static readonly Color YellowColor = new Color(0.95f, 0.85f, 0.1f);
    private static readonly Color BlueColor   = new Color(0.2f,  0.5f,  0.95f);

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(CellType type, CellColor color)
    {
        cellType = type;
        cellColor = color;
        ApplyColor();
    }

    void ApplyColor()
    {
        if (cellType == CellType.Virus)
        {
            // Parent shows colored circle
            spriteRenderer.sprite = virusSprite; // circle sprite
            spriteRenderer.color = cellColor switch
            {
                CellColor.Red    => RedColor,
                CellColor.Yellow => YellowColor,
                CellColor.Blue   => BlueColor,
                _                => Color.white
            };

            if (virusAnimatorObject != null)
                virusAnimatorObject.SetActive(true); // show animated face
        }
        else
        {
            // Capsule half — just color the square, no animation
            spriteRenderer.sprite = capsuleSprite;
            spriteRenderer.color = cellColor switch
            {
                CellColor.Red    => RedColor,
                CellColor.Yellow => YellowColor,
                CellColor.Blue   => BlueColor,
                _                => Color.white
            };

            if (virusAnimatorObject != null)
                virusAnimatorObject.SetActive(false); // hide animated face
        }
    }
}
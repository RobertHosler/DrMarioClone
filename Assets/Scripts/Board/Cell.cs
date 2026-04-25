using UnityEngine;

public class Cell : MonoBehaviour
{
    public enum CellType { Virus, CapsuleHalf }
    public enum CellColor { Red, Yellow, Blue }
    public enum CapsuleEnd { Left, Right, Top, Bottom }

    public CellType cellType;
    public CellColor cellColor;
    public CapsuleEnd capsuleEnd;

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
        ApplyVisuals();
    }

    public void SetCapsuleEnd(CapsuleEnd end)
    {
        capsuleEnd = end;
        ApplyVisuals();
    }

    void ApplyVisuals()
    {
        spriteRenderer.color = cellColor switch

        {
            CellColor.Red    => RedColor,
            CellColor.Yellow => YellowColor,
            CellColor.Blue   => BlueColor,
            _                => Color.white
        };
        if (cellType == CellType.Virus)
        {
            // Parent shows colored circle
            spriteRenderer.sprite = virusSprite; // circle sprite

            if (virusAnimatorObject != null)
                virusAnimatorObject.SetActive(true); // show animated face
        }
        else
        {
            // Capsule half — just color the square, no animation
            spriteRenderer.sprite = capsuleSprite;

            // Rotate sprite to correct orientation
            // Base sprite is flat on right, rounded on left (0 degrees)
            float angle = capsuleEnd switch
            {
                CapsuleEnd.Left   => 90f,
                CapsuleEnd.Right  => 270f,
                CapsuleEnd.Top    => 0f,
                CapsuleEnd.Bottom => 180f,
                _                 => 90f
            };

            spriteRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, angle);

            if (virusAnimatorObject != null)
                virusAnimatorObject.SetActive(false); // hide animated face
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoop : MonoBehaviour
{
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;

    private Board board;

    void Awake()
    {
        board = GetComponent<Board>();
        if (winPanel != null)  winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
    }

    public void OnWin()
    {
        board.isGameOver = true;

        if (board.activeCapsule != null)
            Destroy(board.activeCapsule.gameObject);

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    public void OnLose()
    {
        board.isGameOver = true;

        if (losePanel != null)
            losePanel.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        VirusSpawner.pendingLevel = Mathf.Min(board.GetComponent<VirusSpawner>().level + 1, 20);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

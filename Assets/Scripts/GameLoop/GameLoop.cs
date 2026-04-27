using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameLoop : MonoBehaviour
{
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] TMP_Text scoreText;

    public int score = 0;

    private Board board;

    void Awake()
    {
        board = GetComponent<Board>();
        if (winPanel != null)  winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        UpdateScoreText();
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = $"Score\n{score}";
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

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameLoop : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject winPanel;
    [SerializeField] GameObject losePanel;
    [SerializeField] GameObject pausePanel;

    [Header("UI")]
    [SerializeField] TMP_Text scoreText;

    [Header("Scenes")]
    [SerializeField] string mainMenuSceneName = "MainMenu";

    public int score = 0;

    private Board board;
    private bool isPaused = false;

    void Awake()
    {
        board = GetComponent<Board>();
        if (winPanel != null)   winPanel.SetActive(false);
        if (losePanel != null)  losePanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        Time.timeScale = 1f;
        UpdateScoreText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !board.isGameOver)
            TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    public void Resume()
    {
        if (isPaused) TogglePause();
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        VirusSpawner.pendingLevel = Mathf.Min(board.GetComponent<VirusSpawner>().level + 1, 20);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

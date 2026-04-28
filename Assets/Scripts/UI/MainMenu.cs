using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] GameObject titlePanel;
    [SerializeField] GameObject levelSelectPanel;

    [Header("Level Select")]
    [SerializeField] TMP_Text levelText;
    [SerializeField] int minLevel = 0;
    [SerializeField] int maxLevel = 20;

    [Header("Scene")]
    [SerializeField] string gameSceneName = "GameScene";

    private int selectedLevel = 5;

    void Start()
    {
        titlePanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        UpdateLevelText();
    }

    // Wired to the Play button on the title panel
    public void OnPlayPressed()
    {
        titlePanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    // Wired to the < button
    public void DecrementLevel()
    {
        selectedLevel = Mathf.Max(minLevel, selectedLevel - 1);
        UpdateLevelText();
    }

    // Wired to the > button
    public void IncrementLevel()
    {
        selectedLevel = Mathf.Min(maxLevel, selectedLevel + 1);
        UpdateLevelText();
    }

    // Wired to the Start button
    public void StartGame()
    {
        VirusSpawner.pendingLevel = selectedLevel;
        SceneManager.LoadScene(gameSceneName);
    }

    void UpdateLevelText()
    {
        if (levelText != null)
            levelText.text = $"Level {selectedLevel}";
    }
}

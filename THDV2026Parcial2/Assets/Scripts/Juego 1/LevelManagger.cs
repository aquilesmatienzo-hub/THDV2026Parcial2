
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Vidas")]
    [SerializeField] private Image[] hearts;

    [Header("Pantallas")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;

    private int currentLives;
    private bool levelFinished;

    private void Awake()
    {
        Instance = this;
        currentLives = hearts.Length;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateHearts();
    }

    public bool LoseLife()
    {
        if (levelFinished)
            return false;

        currentLives--;
        UpdateHearts();

        if (currentLives <= 0)
        {
            GameOver();
            return false;
        }

        return true;
    }

    private void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] != null)
                hearts[i].enabled = i < currentLives;
        }
    }

    public void WinLevel()
    {
        if (levelFinished)
            return;

        levelFinished = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    private void GameOver()
    {
        levelFinished = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
    [Header("--- Paneles ---")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("--- Botones de Reinicio ---")]
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private Button gameOverRestartButton;

    void Awake()
    {
        // Aseguramos que arranquen ocultos
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Asignamos la función de reinicio a ambos botones
        if (victoryRestartButton != null)
        {
            victoryRestartButton.onClick.RemoveAllListeners();
            victoryRestartButton.onClick.AddListener(RestartGame);
        }

        if (gameOverRestartButton != null)
        {
            gameOverRestartButton.onClick.RemoveAllListeners();
            gameOverRestartButton.onClick.AddListener(RestartGame);
        }
    }

    void OnEnable()
    {
        GameRoundManager.OnGameWon += ShowVictoryScreen;
        PlayerController.OnPlayerDied += ShowGameOverScreen;
    }

    void OnDisable()
    {
        GameRoundManager.OnGameWon -= ShowVictoryScreen;
        PlayerController.OnPlayerDied -= ShowGameOverScreen;
    }

    private void ShowVictoryScreen()
    {
        Time.timeScale = 0f;
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    private void ShowGameOverScreen()
    {
        Time.timeScale = 0f;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        // Restablece el tiempo antes de recargar para que la nueva partida no arranque pausada
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
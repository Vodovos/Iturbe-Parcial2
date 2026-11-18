using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Stats del Jugador")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Control de Enemigos")]
    public int enemiesDefeatedCount = 0;
    public int activeEnemyID = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentHP = maxHP;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterEnemyDefeated()
    {
        enemiesDefeatedCount++;
        if (enemiesDefeatedCount >= 2)
        {
            // Mostrar Widget de Victoria
            UIManager.Instance.ShowVictoryScreen();
        }
    }

    public void TriggerGameOver()
    {
        // Mostrar Widget de Derrota
        UIManager.Instance.ShowDefeatScreen();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
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
            DontDestroyOnLoad(gameObject); // Opcional si usas una sola escena, asegúrate de que no duplique instancias
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
            // Pausar el juego o detener corrutinas si es necesario y mostrar victoria
            UIManager.Instance.ShowVictoryScreen();
        }
    }

    public void TriggerGameOver()
    {
        UIManager.Instance.ShowDefeatScreen();
    }

    public void RestartGame()
    {
        // Restablecer valores estáticos o de instancia si es necesario
        Time.timeScale = 1f; // Despausar el juego por si acaso
        currentHP = maxHP;
        enemiesDefeatedCount = 0;

        // Recargar la escena actual de forma limpia
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
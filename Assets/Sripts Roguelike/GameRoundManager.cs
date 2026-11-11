using System;
using UnityEngine;

public class GameRoundManager : MonoBehaviour
{
    [Header("--- Duración de Rondas (Escalada Suave) ---")]
    [Tooltip("10 rondas: arranca en 15s, 30s y escala hasta 60s")]
    [SerializeField]
    private float[] roundDurations = new float[10]
    {
        15f, 30f, 35f, 40f, 45f,
        50f, 55f, 60f, 60f, 60f
    };

    [Header("--- Referencias del Juego ---")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerController player;
    [SerializeField] private UpgradeSelectionUI upgradeUI;

    [Header("--- Mejoras (ScriptableObjects) ---")]
    [SerializeField] private UpgradeData speedUpgrade;
    [SerializeField] private UpgradeData damageUpgrade;
    [SerializeField] private UpgradeData healthUpgrade;

    public int CurrentRoundIndex { get; private set; } = 0;
    public float TimeRemaining { get; private set; }
    public bool IsRoundActive { get; private set; } = false;

    // Eventos para UI externa (reloj, indicadores de ronda, etc.)
    public static event Action<int, float> OnRoundStarted; // (rondaActual, duracionTotal)
    public static event Action OnGameWon;

    void Start()
    {
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        if (upgradeUI == null)
            upgradeUI = FindFirstObjectByType<UpgradeSelectionUI>();

        StartRound(0);
    }

    void Update()
    {
        if (!IsRoundActive) return;

        TimeRemaining -= Time.deltaTime;

        if (TimeRemaining <= 0f)
        {
            EndCurrentRound();
        }
    }

    private void StartRound(int roundIndex)
    {
        CurrentRoundIndex = roundIndex;
        TimeRemaining = roundDurations[CurrentRoundIndex];
        IsRoundActive = true;

        // Aseguramos que el tiempo corra
        Time.timeScale = 1f;

        // Activamos el spawn de enemigos
        if (enemySpawner != null)
        {
            enemySpawner.StartSpawning();
        }

        OnRoundStarted?.Invoke(CurrentRoundIndex + 1, roundDurations[CurrentRoundIndex]);
        Debug.Log($"--- Comienza Ronda {CurrentRoundIndex + 1} ({roundDurations[CurrentRoundIndex]}s) ---");
    }

    private void EndCurrentRound()
    {
        IsRoundActive = false;
        TimeRemaining = 0f;

        // 1. Detener spawner
        if (enemySpawner != null)
        {
            enemySpawner.StopSpawning();
        }

        // 2. Barrer todos los enemigos presentes en la arena
        ClearAllEnemies();

        // 3. Chequear si completó las 10 rondas
        if (CurrentRoundIndex >= roundDurations.Length - 1)
        {
            Debug.Log("¡Victoria! Completaste las 10 rondas.");
            OnGameWon?.Invoke();
            return;
        }

        // 4. Pausar el juego y abrir el panel de UI con las mejoras
        Time.timeScale = 0f;

        if (upgradeUI != null)
        {
            upgradeUI.Open(speedUpgrade, damageUpgrade, healthUpgrade);
        }
    }

    private void ClearAllEnemies()
    {
        SimpleEnemyController[] enemies = FindObjectsByType<SimpleEnemyController>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i].gameObject);
        }
    }

    // Método que ejecuta la tarjeta seleccionada desde la UI
    public void ApplySelectedUpgrade(UpgradeData chosenUpgrade)
    {
        if (chosenUpgrade != null && player != null)
        {
            chosenUpgrade.ApplyUpgrade(player);
        }

        // 5. Iniciar la siguiente ronda con el tiempo que le corresponde
        StartRound(CurrentRoundIndex + 1);
    }
}
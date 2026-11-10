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

    [Header("--- Mejoras (ScriptableObjects) ---")]
    [SerializeField] private UpgradeData speedUpgrade;
    [SerializeField] private UpgradeData damageUpgrade;
    [SerializeField] private UpgradeData healthUpgrade;

    public int CurrentRoundIndex { get; private set; } = 0;
    public float TimeRemaining { get; private set; }
    public bool IsRoundActive { get; private set; } = false;

    private bool showUpgradeSelection = false;

    // Eventos para conectar a la UI cuando la diseñes
    public static event Action<int, float> OnRoundStarted; // (rondaActual, duracionTotal)
    public static event Action OnGameWon;

    void Start()
    {
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

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
        showUpgradeSelection = false;

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

        // 4. Pausar el juego y abrir la selección de mejoras
        Time.timeScale = 0f;
        showUpgradeSelection = true;
    }

    private void ClearAllEnemies()
    {
        SimpleEnemyController[] enemies = FindObjectsByType<SimpleEnemyController>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i].gameObject);
        }
    }

    // Método que llamarán los botones de mejora
    public void ApplySelectedUpgrade(UpgradeData chosenUpgrade)
    {
        if (chosenUpgrade != null && player != null)
        {
            chosenUpgrade.ApplyUpgrade(player);
        }

        showUpgradeSelection = false;

        // 5. Iniciar la siguiente ronda con reloj nuevo
        StartRound(CurrentRoundIndex + 1);
    }

    // -------------------------------------------------------------
    // INTERFAZ TEMPORAL (Funcional hasta que diseñes el Widget)
    // -------------------------------------------------------------
    void OnGUI()
    {
        // HUD básico de ronda y tiempo arriba al centro
        GUIStyle hudStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            alignment = TextAnchor.UpperCenter,
            fontStyle = FontStyle.Bold
        };

        if (IsRoundActive)
        {
            string hudText = $"Ronda: {CurrentRoundIndex + 1} / {roundDurations.Length} | Tiempo: {Mathf.CeilToInt(TimeRemaining)}s";
            GUI.Label(new Rect(Screen.width / 2f - 200, 20, 400, 40), hudText, hudStyle);
        }

        // Ventana modal temporal de selección de mejoras
        if (showUpgradeSelection)
        {
            float boxWidth = 320;
            float boxHeight = 260;
            float startX = (Screen.width - boxWidth) / 2f;
            float startY = (Screen.height - boxHeight) / 2f;

            GUI.Box(new Rect(startX, startY, boxWidth, boxHeight), $"¡Ronda {CurrentRoundIndex + 1} Superada!\nElegí una Mejora (+5%)");

            if (GUI.Button(new Rect(startX + 30, startY + 60, 260, 45), "+5% Velocidad de Movimiento"))
            {
                ApplySelectedUpgrade(speedUpgrade);
            }

            if (GUI.Button(new Rect(startX + 30, startY + 120, 260, 45), "+5% Daño de Disparo"))
            {
                ApplySelectedUpgrade(damageUpgrade);
            }

            if (GUI.Button(new Rect(startX + 30, startY + 180, 260, 45), "+5% Vida Máxima"))
            {
                ApplySelectedUpgrade(healthUpgrade);
            }
        }
    }
}
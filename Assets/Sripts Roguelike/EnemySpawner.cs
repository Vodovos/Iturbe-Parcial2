using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("--- Prefabs y Referencias ---")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform playerTransform;

    [Header("--- Tiempos de Spawn Base (Ronda 1) ---")]
    [Tooltip("Intervalo inicial en segundos en la Ronda 1")]
    [SerializeField] private float baseSpawnInterval = 1.5f;

    [Tooltip("Cantidad máxima base de enemigos vivos en la Ronda 1")]
    [SerializeField] private int baseMaxEnemiesAlive = 20;

    [Header("--- Escalada por Oleada (Balanceables) ---")]
    [Tooltip("Segundos que se restan al intervalo por cada ronda avanzada")]
    [SerializeField] private float intervalReductionPerRound = 0.12f;

    [Tooltip("Tope mínimo del intervalo de spawn (ej: 0.25s = máx 4 enemigos/seg)")]
    [SerializeField] private float minSpawnInterval = 0.25f;

    [Tooltip("Enemigos extra permitidos en pantalla por cada ronda")]
    [SerializeField] private int extraMaxEnemiesPerRound = 5;

    [Header("--- Área de Spawn (Límites de la Arena) ---")]
    [SerializeField] private float minDistanceToPlayer = 6f;
    [SerializeField] private float arenaExtentX = 20f;
    [SerializeField] private float arenaExtentZ = 20f;
    [SerializeField] private float spawnHeightY = 0.5f;

    private float currentSpawnInterval;
    private int currentMaxEnemies;
    private float nextSpawnTime;
    private bool isSpawningActive = false;

    void Awake()
    {
        currentSpawnInterval = baseSpawnInterval;
        currentMaxEnemies = baseMaxEnemiesAlive;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (!isSpawningActive || enemyPrefab == null) return;

        if (playerTransform == null)
        {
            isSpawningActive = false;
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            TrySpawnEnemy();
            nextSpawnTime = Time.time + currentSpawnInterval;
        }
    }

    // Recibe el índice de ronda (0 = Ronda 1, 9 = Ronda 10) desde GameRoundManager
    public void SetRoundDifficulty(int roundIndex)
    {
        // Reduce el intervalo de spawn sin bajar del mínimo seguro
        float calculatedInterval = baseSpawnInterval - (roundIndex * intervalReductionPerRound);
        currentSpawnInterval = Mathf.Max(minSpawnInterval, calculatedInterval);

        // Aumenta el límite de enemigos simultáneos en la arena
        currentMaxEnemies = baseMaxEnemiesAlive + (roundIndex * extraMaxEnemiesPerRound);

        Debug.Log($"[Spawner] Ronda {roundIndex + 1} -> Intervalo: {currentSpawnInterval:F2}s | Máx Enemigos: {currentMaxEnemies}");
    }

    private void TrySpawnEnemy()
    {
        int currentEnemies = FindObjectsByType<SimpleEnemyController>(FindObjectsSortMode.None).Length;
        if (currentEnemies >= currentMaxEnemies) return;

        Vector3 spawnPos = GetRandomSpawnPosition();
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 candidatePos = Vector3.zero;
        int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(-arenaExtentX, arenaExtentX);
            float randomZ = Random.Range(-arenaExtentZ, arenaExtentZ);
            candidatePos = new Vector3(randomX, spawnHeightY, randomZ);

            if (playerTransform != null)
            {
                float distToPlayer = Vector3.Distance(candidatePos, playerTransform.position);
                if (distToPlayer >= minDistanceToPlayer)
                {
                    return candidatePos;
                }
            }
            else
            {
                return candidatePos;
            }
        }

        return candidatePos;
    }

    public void StartSpawning()
    {
        isSpawningActive = true;
        // Resetea el timer para que el primer enemigo aparezca casi al instante al iniciar la ronda
        nextSpawnTime = Time.time + 0.5f;
    }

    public void StopSpawning() => isSpawningActive = false;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(0f, spawnHeightY, 0f), new Vector3(arenaExtentX * 2, 0.2f, arenaExtentZ * 2));
    }
}
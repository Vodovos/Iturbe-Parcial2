using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("--- Prefabs y Referencias ---")]
    [Tooltip("Prefab del enemigo con el script SimpleEnemyController")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Transform del Player para chequear distancias mínimas")]
    [SerializeField] private Transform playerTransform;

    [Header("--- Tiempos de Spawn (Balanceables) ---")]
    [Tooltip("Intervalo en segundos entre cada aparición")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Tooltip("Cantidad máxima de enemigos vivos al mismo tiempo")]
    [SerializeField] private int maxEnemiesAlive = 30;

    [Header("--- Área de Spawn (Límites de la Arena) ---")]
    [Tooltip("Radio o distancia mínima respecto al player para que no nazcan encima")]
    [SerializeField] private float minDistanceToPlayer = 6f;

    [Tooltip("Límite horizontal (eje X) del mapa")]
    [SerializeField] private float arenaExtentX = 20f;

    [Tooltip("Límite vertical/profundidad (eje Z) del mapa")]
    [SerializeField] private float arenaExtentZ = 20f;

    [Tooltip("Altura en Y donde se instancian los enemigos")]
    [SerializeField] private float spawnHeightY = 0.5f;

    private float nextSpawnTime;
    private bool isSpawningActive = true;

    void Awake()
    {
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

        // Si el jugador murió o fue destruido, detenemos el spawn
        if (playerTransform == null)
        {
            isSpawningActive = false;
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            TrySpawnEnemy();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    private void TrySpawnEnemy()
    {
        // Limitar enemigos en pantalla para evitar saturación
        int currentEnemies = FindObjectsByType<SimpleEnemyController>(FindObjectsSortMode.None).Length;
        if (currentEnemies >= maxEnemiesAlive) return;

        // Intentar encontrar una posición válida aleatoria lejos del jugador
        Vector3 spawnPos = GetRandomSpawnPosition();
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 candidatePos = Vector3.zero;
        int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            // Coordenada aleatoria dentro de los límites de la arena
            float randomX = Random.Range(-arenaExtentX, arenaExtentX);
            float randomZ = Random.Range(-arenaExtentZ, arenaExtentZ);
            candidatePos = new Vector3(randomX, spawnHeightY, randomZ);

            // Verificamos que esté fuera del radio seguro del jugador
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

    // Métodos para pausar/reanudar el spawn entre rondas o menús
    public void StartSpawning() => isSpawningActive = true;
    public void StopSpawning() => isSpawningActive = false;

    // Visualización del área de spawn en la vista de Escena
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(0f, spawnHeightY, 0f), new Vector3(arenaExtentX * 2, 0.2f, arenaExtentZ * 2));
    }
}